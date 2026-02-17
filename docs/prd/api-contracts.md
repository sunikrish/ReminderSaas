API Contracts
Create Schedule
POST /api/schedules
{
 title,
 description,
 date,
 time,
 location,
 category,
 checklistItems
}
Get Month Schedules
GET /api/schedules?year=2026&month=3
Generate Monthly Summary
POST /api/ai/monthly-summary
Input:
List<Schedule>
Output:
{
 summaryText,
 categoryBreakdown,
 urgentItems
}
Analyze Document
POST /api/ai/analyze-document
Input:
OCR text
Output:
Intent DTO
Process Voice Command
POST /api/ai/voice-command