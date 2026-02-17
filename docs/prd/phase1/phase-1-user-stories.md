Phase 1 – Core Schedule & Calendar
🎯 Goal
Implement:
Calendar View
Create Schedule
Edit Schedule
Delete Schedule
SQL persistence
No AI yet.
🧑‍💼 User Stories
🗓️ US-1: View Monthly Calendar
As a user,
I want to see a monthly calendar when I open the app
So that I can view all my scheduled events for that month.
Acceptance Criteria
App opens to Calendar View (Phase 1 default)
Current month is shown
Each date cell:
Displays indicator if schedule exists
Uses category color coding
User can navigate:
Previous month
Next month
Selecting a date shows list of schedules below the calendar
Calendar must be responsive (Android, iOS, Web)

➕ US-2: Create Schedule
As a user,
I want to create a new schedule
So that I can remember important events.
Acceptance Criteria
User taps a date
User taps “Add Schedule”
Form opens with fields:
Title (required)
Description (optional)
Date (pre-filled)
Time (optional)
Location (optional)
Category (Health, Tax, School, Personal)
Checklist items (optional)
On save:
Data persists to SQL
Calendar refreshes
New event indicator appears
Validation:
Title required
Date required

✏️ US-3: Edit Schedule
As a user,
I want to modify an existing schedule
So that I can keep my information up to date.
Acceptance Criteria
User taps a schedule
Edit button available
Form pre-filled
Changes saved to SQL
Calendar updates immediately

🗑️ US-4: Delete Schedule
As a user,
I want to delete a schedule
So that I can remove cancelled events.
Acceptance Criteria
Delete option available in schedule detail
Confirmation dialog required
On confirm:
Record deleted from SQL
Calendar refreshes