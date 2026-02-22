using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text;
using Azure;
using Azure.AI.DocumentIntelligence;
using Azure.AI.Translation.Document;
using Azure.Identity;
using OpenAI.Chat;

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
    private static readonly string AzureOpenAIDeploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "intent-extraction-model";

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
            // Validate required Azure configuration keys
            var missing = new List<string>();
            if (string.IsNullOrWhiteSpace(DocumentIntelligenceEndpoint)) missing.Add(nameof(DocumentIntelligenceEndpoint));
            if (string.IsNullOrWhiteSpace(DocumentIntelligenceKey)) missing.Add("AZURE_DOCUMENT_INTELLIGENCE_KEY");
            if (string.IsNullOrWhiteSpace(TranslatorEndpoint)) missing.Add(nameof(TranslatorEndpoint));
            if (string.IsNullOrWhiteSpace(TranslatorKey)) missing.Add("AZURE_TRANSLATOR_KEY");
            if (string.IsNullOrWhiteSpace(AzureOpenAIEndpoint)) missing.Add(nameof(AzureOpenAIEndpoint));
            if (string.IsNullOrWhiteSpace(AzureOpenAIKey)) missing.Add("AZURE_OPENAI_KEY");

            if (missing.Count > 0)
            {
                _logger.LogError("Missing configuration keys for ScanDocumentFunction: {Missing}", string.Join(',', missing));
                var bad = req.CreateResponse(System.Net.HttpStatusCode.InternalServerError);
                await bad.WriteAsJsonAsync(new { error = "Missing configuration keys", missing });
                return bad;
            }
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

            // If OCR returned no text, return a sensible empty result instead of
            // continuing to translation/OpenAI which would either return nothing
            // or error. This keeps the API predictable for images without text.
            if (string.IsNullOrWhiteSpace(ocrText))
            {
                _logger.LogInformation("OCR returned no text; returning empty analysis result.");
                var emptyResult = new DocumentAnalysisResultDto(
                    "unknown",
                    string.Empty,
                    false,
                    null);

                var okResp = req.CreateResponse(System.Net.HttpStatusCode.OK);
                await okResp.WriteAsJsonAsync(emptyResult);
                return okResp;
            }

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
                content);

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
            var bodyJson = JsonSerializer.Serialize(requestBody);

            // Try multiple translator endpoint patterns and header styles.
            var baseEndpoint = TranslatorEndpoint?.TrimEnd('/') ?? "";
            var candidates = new List<(string url, string headerName, string headerRegion)>
            {
                // Classic Translator resource pattern (cognitiveservices) - uses Ocp-Apim-Subscription-Key
                ($"{baseEndpoint}/translate?api-version=3.0&from={sourceLanguage}&to=en", "Ocp-Apim-Subscription-Key", "germanywestcentral"),
                // AI Services / Foundry pattern for Translator (uses api-key header)
                ($"{baseEndpoint}/translator/text/v3.0/translate?api-version=3.0&from={sourceLanguage}&to=en", "api-key", null),
                // Fallback: try translate path on services.ai style endpoint
                ($"{baseEndpoint}/translate?api-version=3.0&from={sourceLanguage}&to=en", "api-key", null)
            };

            foreach (var (url, headerName, headerRegion) in candidates)
            {
                try
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, url);
                    request.Content = new StringContent(bodyJson, System.Text.Encoding.UTF8, "application/json");

                    if (string.Equals(headerName, "api-key", StringComparison.OrdinalIgnoreCase))
                    {
                        request.Headers.Remove("api-key");
                        request.Headers.Add("api-key", TranslatorKey);
                    }
                    else
                    {
                        request.Headers.Remove("Ocp-Apim-Subscription-Key");
                        request.Headers.Add("Ocp-Apim-Subscription-Key", TranslatorKey);
                    }

                    if (!string.IsNullOrEmpty(headerRegion))
                    {
                        request.Headers.Remove("Ocp-Apim-Subscription-Region");
                        request.Headers.Add("Ocp-Apim-Subscription-Region", headerRegion);
                    }

                    _logger.LogInformation("Trying Translator URL {Url} with header {Header}", url, headerName);
                    var response = await _httpClient.SendAsync(request);
                    var resultJson = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("Translator candidate {Url} returned {Status}: {Body}", url, response.StatusCode, resultJson);
                        continue;
                    }

                    using var doc = JsonDocument.Parse(resultJson);
                    // Classic translate response is an array with translations
                    if (doc.RootElement.ValueKind == JsonValueKind.Array && doc.RootElement.GetArrayLength() > 0)
                    {
                        var translatedText = doc.RootElement[0].GetProperty("translations")[0].GetProperty("text").GetString();
                        return translatedText ?? text;
                    }

                    // If the shape is different, attempt to extract a text field gracefully
                    if (doc.RootElement.TryGetProperty("translations", out var trans) && trans.GetArrayLength() > 0)
                    {
                        var t = trans[0].GetProperty("text").GetString();
                        return t ?? text;
                    }

                    // If nothing matched, return the raw body as fallback
                    return text;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Translator candidate {url} failed: {ex.Message}");
                    // try next candidate
                }
            }

            _logger.LogWarning("All translator endpoint candidates failed; returning original text");
            return text;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Translation API error: {ex.Message}");
            return text;
        }
    }

    private async Task<DocumentAnalysisResultDto> ExtractStructuredDataAsync(string englishText, string originalText, string detectedLanguage)
    {
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

                try
        {
            // Create Azure OpenAI client using the OpenAI package
            var credential = new System.ClientModel.ApiKeyCredential(AzureOpenAIKey);
            var options = new OpenAI.OpenAIClientOptions
            {
                Endpoint = new Uri(AzureOpenAIEndpoint)
            };
            
            _logger.LogInformation("Using OpenAI endpoint {Endpoint} and deployment {Deployment}", AzureOpenAIEndpoint, AzureOpenAIDeploymentName);

            var client = new ChatClient(
                model: AzureOpenAIDeploymentName,
                credential,
                options);

                        var messages = new List<ChatMessage>
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(userMessage)
            };

            var completionOptions = new ChatCompletionOptions
            {
                Temperature = 0.3f,
            };

            var completion = await client.CompleteChatAsync(messages, completionOptions);

            var responseText = completion.Value.Content[0].Text;
            _logger.LogInformation($"OpenAI response: {responseText}");

            var result = ParseOpenAIResponse(responseText, detectedLanguage);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"OpenAI error: {ex.Message}");

            // Fallback: call the Azure OpenAI REST deployments/chat/completions endpoint
            try
            {
                _logger.LogInformation("Attempting REST fallback to OpenAI deployments endpoint");

                var restUrl = $"{AzureOpenAIEndpoint.TrimEnd('/')}/openai/deployments/{AzureOpenAIDeploymentName}/chat/completions?api-version=2023-06-01-preview";

                var restBody = new
                {
                    messages = new[] {
                        new { role = "system", content = systemPrompt },
                        new { role = "user", content = userMessage }
                    },
                    temperature = 0.3f
                };

                var request = new HttpRequestMessage(HttpMethod.Post, restUrl);
                request.Headers.Add("api-key", AzureOpenAIKey);
                request.Content = new StringContent(JsonSerializer.Serialize(restBody), Encoding.UTF8, "application/json");

                var restResp = await _httpClient.SendAsync(request);
                var restText = await restResp.Content.ReadAsStringAsync();

                if (restResp.IsSuccessStatusCode)
                {
                    // Extract assistant content robustly from returned JSON
                    string assistantText = "";
                    using (var doc = JsonDocument.Parse(restText))
                    {
                        if (doc.RootElement.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                        {
                            var first = choices[0];
                            if (first.TryGetProperty("message", out var message) && message.TryGetProperty("content", out var contentProp))
                            {
                                assistantText = contentProp.GetString() ?? "";
                            }
                            else if (first.TryGetProperty("content", out var contentProp2))
                            {
                                assistantText = contentProp2.GetString() ?? "";
                            }
                        }
                    }

                    _logger.LogInformation($"OpenAI REST fallback response: {assistantText}");
                    var parsed = ParseOpenAIResponse(assistantText, detectedLanguage);
                    return parsed;
                }
                else
                {
                    _logger.LogError($"OpenAI REST fallback returned {restResp.StatusCode}: {restText}");
                }
            }
            catch (Exception rex)
            {
                _logger.LogError($"OpenAI REST fallback error: {rex.Message}");
            }

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
                // Map assistant-provided categories to application enum values
                category = MapToAllowedCategory(category);

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

    private static string MapToAllowedCategory(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return "Personal";

        var s = raw.Trim().ToLowerInvariant();

        return s switch
        {
            // Assistant categories -> frontend display categories
            "government" => "Government",
            "kids school" => "Kids School",
            "kids_school" => "Kids School",
            "kids-school" => "Kids School",
            "school" => "Kids School",
            "health" => "Health",
            "personal" => "Personal",
            "car" => "Car",
            "finance" => "Finance",
            "financial" => "Finance",
            "tax" => "Finance",
            _ =>
                // Try simple contains checks for robustness
                s.Contains("school") ? "Kids School" :
                s.Contains("health") ? "Health" :
                s.Contains("tax") || s.Contains("finance") ? "Finance" :
                s.Contains("personal") ? "Personal" :
                "Other"
        };
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
