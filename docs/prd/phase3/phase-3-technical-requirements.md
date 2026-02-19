🏗 TECHNICAL ARCHITECTURE
Backend Flow
ScanDocumentFunction (Azure Function)

    ↓
Azure Document Intelligence (OCR)

    ↓
Language Detection + Translation (Azure AI Translator)

    ↓
AI Intent Extraction (Azure OpenAI)

    ↓
Return structured response
New Azure Function
POST /api/documents/analyze
Accepts:
Multipart image
OR
Base64 image
New Application Layer
Application/Interfaces/IDocumentProcessingService.cs
Application/Services/DocumentProcessingService.cs
Responsibilities:
Call OCR
Detect language
Translate
Build AI prompt
Parse JSON response
Return structured DTO
New DTO
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
🔐 PRIVACY ARCHITECTURE
Critical Design Decisions:
No Blob Storage used
No image saved
No OCR text saved
All processing in-memory
Logs redact content
AI prompt excludes personal data where possible
Use HTTPS only
Use Azure region in EU (Germany West Central recommended)

---------------------------------------------------------
📱 MAUI FRONTEND FLOW
UI Flow
Tap Camera icon
Capture image
Show loading spinner
Show summary card
If appointment detected:
Show appointment preview card
Show confirm button
If confirmed:
Call existing Schedule API
📂 Folder Structure Additions
Backend:
Backend.Function/
   AnalyzeDocumentFunction.cs

Application/
   Interfaces/IDocumentProcessingService.cs
   Services/DocumentProcessingService.cs

Infrastructure/
   AI/
      DocumentIntelligenceClient.cs
      TranslatorClient.cs
      AzureOpenAIClient.cs
Frontend:
Views/Documents/
ViewModels/Documents/
Services/DocumentApiClient.cs
🔒 GDPR & Privacy Compliance Considerations
Since targeting German expats:
Use EU region
Do not persist raw document
Add privacy disclaimer
Add toggle for user consent
Document retention policy = none
🚀 DEVELOPMENT STRATEGY
Implement in 3 Steps:
Phase 3A
Camera capture
OCR only
Display raw text
Phase 3B
Translation
AI summary
Structured output
Phase 3C
Confirmation flow
Add to schedule
UI polish
---------------------------------------------------
Azure Document Intelligence
Endpoint: https://saas-ai-di.cognitiveservices.azure.com/
key: [REDACTED - Use AZURE_DOCUMENT_INTELLIGENCE_KEY environment variable]
Location: germanywestcentral

--------------------------------------------------
Azure AI translator
Tex translation endpoint : https://api.cognitive.microsofttranslator.com/
document translation endpoint : https://saas-ai-translator.cognitiveservices.azure.com/
key : [REDACTED - Use AZURE_TRANSLATOR_KEY environment variable]
location: germanywestcentral

--------------------
Azure Open AI

var endpoint = new Uri("https://sunik-mltjy8d8-eastus2.cognitiveservices.azure.com/");
var model = "gpt-4o-mini";
var deploymentName = "intent-extraction-model";
var apiKey = "[REDACTED - Use AZURE_OPENAI_KEY environment variable]";

system prompt and context :
You are an AI assistant for a Reminder application designed for German expats.

Your responsibilities:

1. Detect the language of the document.
2. Translate the content into English if it is not already English.
3. Summarize the document clearly and concisely in English.
4. Determine whether the document contains an appointment, deadline, or required action.
5. Extract structured appointment information if present.
6. Categorize the appointment into one of the following categories:
   - Government
   - Kids School
   - Personal
   - Health
   - Car
   - Finance

IMPORTANT RULES:
- Do NOT hallucinate missing data.
- If a date, time, or location is not clearly stated, return null.
- Do NOT guess.
- Only extract what is explicitly written.
- If no appointment exists, clearly state that.
- Never include personal data beyond what is required for appointment scheduling.
- Do not store or retain document information.
- Output MUST be valid JSON only.
- Do NOT include explanations outside JSON.
- All output must be in English.

JSON OUTPUT FORMAT:

{
  "detectedLanguage": "string",
  "translatedSummary": "string",
  "containsAppointment": true/false,
  "category": "Government | Kids School | Personal | Health | Car | Finance | null",
  "appointment": {
    "title": "string | null",
    "description": "string | null",
    "date": "YYYY-MM-DD | null",
    "time": "HH:mm | null",
    "location": "string | null",
    "preparationChecklist": ["string"] | []
  },
  "confidenceScore": 0.0 to 1.0
}


