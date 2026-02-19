🎯 Epic: Monthly AI Summary Agent
👤 US-1: View Monthly AI Summary
As a user,
I want to see an AI-generated summary of my monthly schedules
So that I understand my workload and priorities at a glance.
Acceptance Criteria:
When I open the app, the Monthly AI Summary is displayed.
Summary includes:
Total schedules
Category distribution
Critical schedules
Suggested actions
Loading indicator shown while generating.
If AI fails, fallback deterministic summary is shown.
👤 US-2: Identify Critical Schedules
As a user,
I want critical or important schedules highlighted
So that I do not miss important deadlines.
Acceptance Criteria:
Tax and Health automatically considered critical.
Days with >3 schedules flagged.
AI may flag additional risks.
Critical schedules visually highlighted in red badge.
👤 US-3: Category Breakdown
As a user,
I want to see how my schedules are distributed across categories
So that I understand balance (Tax, Health, School, Personal).
Acceptance Criteria:
Show count per category.
Show percentage per category.
Identify dominant category.
👤 US-4: Receive AI Suggestions
As a user,
I want intelligent suggestions
So that I can better manage my month.
Acceptance Criteria:
AI suggests preparation steps.
AI detects overload or clustering.
AI identifies missing areas (e.g., no health events).
Suggestions displayed as bullet list.
👤 US-5: Refresh Monthly Summary
As a user,
I want to refresh the summary
So that I see updated analysis after adding schedules.
Acceptance Criteria:
Refresh button triggers new AI analysis.
Cached version replaced.

--------------------------------------------------
📌 FUNCTIONAL REQUIREMENTS
FR-1: Monthly Summary Endpoint
System shall provide:
GET /api/schedules/monthly-summary?year=YYYY&month=MM
Returns:
{
  "naturalLanguageSummary": "...",
  "criticalSchedules": [],
  "categoryCounts": {},
  "totalSchedules": 12,
  "suggestedActions": []
}
FR-2: Deterministic Analytics Layer
Before AI call, system must compute:
Total schedules
Category distribution
Overloaded days (>3)
Tax schedules
Health schedules
End-of-month clustering
Schedules without time
FR-3: AI Enhancement Layer
AI must:
Summarize month naturally
Identify risks
Suggest actions
Provide concise structured JSON
FR-4: UI Integration
Frontend must:
Display summary card
Display critical schedule list
Display category breakdown
Display suggestions
Provide refresh capability
FR-5: Security
AI API key must not be exposed to frontend.
Keys stored in Azure Key Vault.
Backend reads from configuration.