using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Azure;
using Azure.AI.DocumentIntelligence;
using Azure.AI.Translation.Document;
using Azure.Identity;
using Azure.AI.OpenAI;

namespace Backend.Function;

public class ScanDocumentFunction
{
    private readonly ILogger<ScanDocumentFunction> _logger;
    private readonly HttpClient _httpClient;

    // Azure Configuration - Load from environment variables
    private static readonly string DocumentIntelligenceEndpoint = Environment.GetEnvironmentVariable("AZURE_DOCUMENT_INTELLIGENCE_ENDPOINT") ?? "";
    private static readonly string DocumentIntelligenceKey = Environment.GetEnvironmentVariable("AZURE_DOCUMENT_INTELLIGENCE_KEY") ?? "";
    
    private static readonly string TranslatorEndpoint = Environment.GetEnvironmentVariable("AZURE_TRANSLATOR_ENDPOINT") ?? "";
    private static readonly string TranslatorKey = Environment.GetEnvironmentVariable("AZURE_TRANSLATOR_KEY") ?? "";
    
    private static readonly string AzureOpenAIEndpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? "";
    private static readonly string AzureOpenAIKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY") ?? "";
    private const string AzureOpenAIDeploymentName = "intent-extraction-model";

    public ScanDocumentFunction(ILogger<ScanDocumentFunction> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    [Function("ScanDocument")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "scan-document")] HttpRequestData req)
    {
        try
        {
            _logger.LogInformation("Starting document scan analysis");

            // Read image from request
            var contentType = req.Headers.TryGetValues("Content-Type", out var ctValues)
                ? ctValues.FirstOrDefault()
                : null;

            byte[] imageData;
            using (var ms = new MemoryStream())
            {
                await req.Body.CopyToAsync(ms);
                imageData = ms.ToArray();
            }

            if (imageData.Length == 0)
            {
                _logger.LogWarning("No image data provided");
                var response = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await response.WriteAsJsonAsync(new { error = "No image data provided" });
                return response;
            }

            _logger.LogInformation($"Image received: {imageData.Length} bytes");

            // 1. OCR using Azure Document Intelligence
            var ocrText = await PerformOCRAsync(imageData);
            _logger.LogInformation($"OCR completed, text length: {ocrText.Length}");

            // 2. Detect language and translate if needed
            var (detectedLanguage, englishText) = await DetectLanguageAndTranslateAsync(ocrText);
            _logger.LogInformation($"Language: {detectedLanguage}, translated text length: {englishText.Length}");

            // 3. Extract structured data using Azure OpenAI
            var result = await ExtractStructuredDataAsync(englishText, ocrText, detectedLanguage);
            _logger.LogInformation($"Analysis complete: ContainsAppointment={result.ContainsAppointment}");

            var httpResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await httpResponse.WriteAsJsonAsync(result);
            return httpResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in ScanDocumentFunction: {ex.Message}\n{ex.StackTrace}");
            var response = req.CreateResponse(System.Net.HttpStatusCode.InternalServerError);
            await response.WriteAsJsonAsync(new { error = ex.Message });
            return response;
        }
    }

    private async Task<string> PerformOCRAsync(byte[] imageData)
    {
        try
        {
            var client = new DocumentIntelligenceClient(
                new Uri(DocumentIntelligenceEndpoint),
                new AzureKeyCredential(DocumentIntelligenceKey));

            var content = BinaryData.FromBytes(imageData);
            var operation = await client.AnalyzeDocumentAsync(
                WaitUntil.Completed,
                "prebuilt-read",
                analyzeDocumentContent: content);

            var result = operation.Value;
            var text = string.Join("\n", result.Pages.SelectMany(p => p.Lines.Select(l => l.Content)));
            return text;
        }
        catch (Exception ex)
        {
            _logger.LogError($"OCR error: {ex.Message}");
            throw;
        }
    }

    private async Task<(string, string)> DetectLanguageAndTranslateAsync(string text)
    {
        try
        {
            // For now, we'll detect language through translation service
            // Full language detection would require additional API call
            var detectedLanguage = "en"; // Default
            var translatedText = text;

            // If needed, translate to English
            if (text.Length > 0)
            {
                // Simple heuristic: try translation API for detection
                // In production, use Azure Language Detection API
                detectedLanguage = DetectLanguageSimple(text);
                
                if (detectedLanguage != "en")
                {
                    translatedText = await TranslateToEnglishAsync(text, detectedLanguage);
                }
            }

            return (detectedLanguage, translatedText);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Translation error: {ex.Message}");
            return ("unknown", text);
        }
    }

    private string DetectLanguageSimple(string text)
    {
        // Simple language detection based on common German words
        var germanWords = new[] { "der", "die", "das", "und", "ein", "eine", "ist", "nicht", "dass", "mit" };
        var words = text.ToLower().Split(new[] { ' ', '\n', '\t', ',', '.' }, StringSplitOptions.RemoveEmptyEntries);
        var germanCount = words.Count(w => germanWords.Contains(w));
        
        return germanCount > words.Length * 0.1 ? "de" : "en";
    }

    private async Task<string> TranslateToEnglishAsync(string text, string sourceLanguage)
    {
        try
        {
            var requestBody = new object[] { new { Text = text } };
            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                System.Text.Encoding.UTF8,
                "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, 
                $"{TranslatorEndpoint}translate?api-version=3.0&from={sourceLanguage}&to=en");
            request.Headers.Add("Ocp-Apim-Subscription-Key", TranslatorKey);
            request.Headers.Add("Ocp-Apim-Subscription-Region", "germanywestcentral");
            request.Content = content;

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Translation API returned {response.StatusCode}");
                return text;
            }

            var resultJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(resultJson);
            var translatedText = doc.RootElement[0]
                .GetProperty("translations")[0]
                .GetProperty("text")
                .GetString();

            return translatedText ?? text;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Translation API error: {ex.Message}");
            return text;
        }
    }

    private async Task<DocumentAnalysisResultDto> ExtractStructuredDataAsync(string englishText, string originalText, string detectedLanguage)
    {
        try
        {
            var client = new AzureOpenAIClient(
                new Uri(AzureOpenAIEndpoint),
                new AzureKeyCredential(AzureOpenAIKey));

            var systemPrompt = @"You are an AI assistant for a Reminder application designed for German expats.

Your responsibilities:
1. Summarize the document clearly and concisely in English.
2. Determine whether the document contains an appointment, deadline, or required action.
3. Extract structured appointment information if present.
4. Categorize the appointment into one of: Government, Kids School, Personal, Health, Car, Finance

IMPORTANT RULES:
- Do NOT hallucinate missing data.
- If a date, time, or location is not clearly stated, return null.
- Return valid JSON in this format:
{
  ""summary"": ""Brief summary of document"",
  ""contains_appointment"": true/false,
  ""appointment"": {
    ""title"": ""Appointment title"",
    ""date"": ""2026-02-19"",
    ""time"": ""14:30"" or null,
    ""location"": ""Location"" or null,
    ""category"": ""Government|Kids School|Personal|Health|Car|Finance""
  } or null
}";

            var userMessage = $"Analyze this document:\n\n{englishText}";

            var chatCompletionOptions = new Azure.AI.OpenAI.ChatCompletionOptions
            {
                Temperature = 0.3f,
                MaxTokens = 500,
            };

            chatCompletionOptions.Messages.Add(new Azure.AI.OpenAI.ChatCompletionMessage(
                Azure.AI.OpenAI.ChatRole.System,
                systemPrompt));

            chatCompletionOptions.Messages.Add(new Azure.AI.OpenAI.ChatCompletionMessage(
                Azure.AI.OpenAI.ChatRole.User,
                userMessage));

            var completion = await client.GetChatCompletionsAsync(
                AzureOpenAIDeploymentName,
                chatCompletionOptions);

            var responseText = completion.Value.Choices[0].Message.Content;
            _logger.LogInformation($"OpenAI response: {responseText}");

            // Parse JSON response
            var result = ParseOpenAIResponse(responseText, detectedLanguage);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"OpenAI error: {ex.Message}");
            return new DocumentAnalysisResultDto(
                detectedLanguage,
                "Unable to analyze document",
                false,
                null);
        }
    }

    private DocumentAnalysisResultDto ParseOpenAIResponse(string responseText, string detectedLanguage)
    {
        try
        {
            // Extract JSON from response (may contain markdown formatting)
            var jsonStart = responseText.IndexOf('{');
            var jsonEnd = responseText.LastIndexOf('}');
            
            if (jsonStart < 0 || jsonEnd < 0)
            {
                _logger.LogWarning("No JSON found in OpenAI response");
                return new DocumentAnalysisResultDto(detectedLanguage, responseText, false, null);
            }

            var jsonStr = responseText.Substring(jsonStart, jsonEnd - jsonStart + 1);
            using var doc = JsonDocument.Parse(jsonStr);
            var root = doc.RootElement;

            var summary = root.TryGetProperty("summary", out var prop) ? prop.GetString() ?? "" : "";
            var containsAppointment = root.TryGetProperty("contains_appointment", out var prop2) && prop2.GetBoolean();

            AppointmentExtractionDto? appointment = null;

            if (containsAppointment && root.TryGetProperty("appointment", out var appointmentProp) && appointmentProp.ValueKind == JsonValueKind.Object)
            {
                var apt = appointmentProp;
                var title = apt.GetProperty("title").GetString() ?? "";
                var dateStr = apt.GetProperty("date").GetString();
                var timeStr = apt.TryGetProperty("time", out var timeProp) && timeProp.ValueKind == JsonValueKind.String ? timeProp.GetString() : null;
                var location = apt.TryGetProperty("location", out var locProp) && locProp.ValueKind == JsonValueKind.String ? locProp.GetString() : null;
                var category = apt.GetProperty("category").GetString() ?? "Personal";

                if (DateOnly.TryParse(dateStr, out var date))
                {
                    TimeOnly? time = null;
                    if (!string.IsNullOrEmpty(timeStr) && TimeOnly.TryParse(timeStr, out var parsedTime))
                    {
                        time = parsedTime;
                    }

                    appointment = new AppointmentExtractionDto(title, date, time, location, category);
                }
            }

            return new DocumentAnalysisResultDto(detectedLanguage, summary, containsAppointment, appointment);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error parsing OpenAI response: {ex.Message}");
            return new DocumentAnalysisResultDto(detectedLanguage, "Error processing response", false, null);
        }
    }
}

public record DocumentAnalysisResultDto(
    string DetectedLanguage,
    string Summary,
    bool ContainsAppointment,
    AppointmentExtractionDto? Appointment
);

public record AppointmentExtractionDto(
    string Title,
    DateOnly Date,
    TimeOnly? Time,
    string? Location,
    string Category
);
