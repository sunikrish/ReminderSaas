AI Agents Design
1. MonthlySummaryAgent
Input:
List<Schedule>
Output:
SummaryText
CategoryBreakdown
UrgentItems
Suggestions
Prompt rule:
Never include personal data.
Only summarize structured fields.
2. DocumentIntentAgent
Input:
Raw OCR text
Steps:
Detect language
Translate to English
Extract:
IsAppointment (bool)
Title
Date
Time
Location
Category
RequiredPreparation
Output:
Structured DTO only.
Never store raw OCR text.
3. VoiceCommandAgent
Input:
Speech text
Output:
Command DTO:
{
 action: Add | Modify | Delete,
 date,
 title,
 category,
 checklistItems
}
