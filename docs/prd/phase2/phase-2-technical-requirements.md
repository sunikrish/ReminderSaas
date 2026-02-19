🏗 TECHNICAL REQUIREMENTS
1️⃣ Backend Changes
1.1 New Azure Function
File:
Backend.Function/MonthlySummaryFunction.cs
Trigger:
[Function("GetMonthlySummary")]
public async Task<HttpResponseData> Run(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "schedules/monthly-summary")] HttpRequestData req)
1.2 New Application Layer Interface
Application/Interfaces/IMonthlySummaryService.cs
Task<MonthlySummaryDto> GenerateMonthlySummaryAsync(int year, int month);
1.3 New Service
Application/Services/MonthlySummaryService.cs
Responsibilities:
Get schedules
Perform deterministic analysis
Construct AI prompt
Call AI provider
Parse response
Return DTO
1.4 AI Abstraction Layer
Application/Interfaces/IAIClient.cs
Infrastructure/AI/AzureOpenAIClient.cs
This ensures provider can be swapped.
1.5 New DTO
In Shared.Contracts/MonthlySummary/:
public record MonthlySummaryDto(
    string NaturalLanguageSummary,
    List<CriticalScheduleDto> CriticalSchedules,
    Dictionary<string,int> CategoryCounts,
    int TotalSchedules,
    List<string> SuggestedActions
);
2️⃣ AI Architecture
AI Call Flow
Azure Function
  → MonthlySummaryService
      → DeterministicAnalyzer
      → AI Prompt Builder
      → AzureOpenAIClient
      → JSON Parsing
Prompt Template
You are a productivity AI assistant.

Analyze the following schedules for February 2026:

[Structured schedule list]

Tasks:
1. Provide short monthly summary.
2. Identify critical schedules and explain why.
3. Suggest improvements.
4. Detect overload or risk patterns.

Return JSON only.
3️⃣ Frontend Changes (MAUI)
3.1 New Page
Views/Summary/MonthlySummaryPage.xaml
Sections:
📝 Summary Card
⚠ Critical Schedules
📊 Category Breakdown
💡 Suggestions
🔄 Refresh Button
3.2 ViewModel
MonthlySummaryViewModel.cs
Properties:
SummaryText
CriticalSchedules
CategoryCounts
SuggestedActions
IsLoading