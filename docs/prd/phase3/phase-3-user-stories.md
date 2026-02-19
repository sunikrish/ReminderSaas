🧾 USER STORIES
👤 US-1: Scan Government Letter
As a German expat,
I want to scan a letter from a government office
So that I understand what it says in English.
Acceptance Criteria:
User taps camera icon
Camera opens
Image captured
OCR extracts text
Language auto-detected
Translated to English
Summary displayed in readable format
👤 US-2: Detect Appointment Intent
As a user,
I want the app to detect if the letter contains an appointment
So that I don’t miss official meetings.
Acceptance Criteria:
AI identifies date/time
AI identifies location
AI identifies category:
Government
Kids School
Personal
Health
Car
Finance
Structured output returned
👤 US-3: Confirm Before Adding
As a user,
I want to confirm before the appointment is added
So that incorrect events are not added automatically.
Acceptance Criteria:
Show extracted details:
Title
Date
Time
Location
Category
Buttons:
✅ Add to Calendar
❌ Dismiss
Only add if confirmed
👤 US-4: Privacy Protection
As a user,
I want my scanned letters NOT stored
So that my personal data remains private.
Acceptance Criteria:
Image not stored in DB
Text not stored in DB
Only structured schedule saved (if confirmed)
No logs contain letter text
AI processing is stateless
👤 US-5: Multilingual Support
As a user,
I want letters in German (and other languages future) translated
So that I understand them clearly.
Acceptance Criteria:
Auto language detection
German → English translation
Summary in English
📌 FUNCTIONAL REQUIREMENTS
FR-1: Camera Integration (MAUI)
Use MAUI MediaPicker or camera library
Capture image
Convert to stream
Send to backend API
FR-2: OCR Extraction
System must:
Extract printed German text from image
Support PDF (future)
Support handwritten text (best effort)
FR-3: Language Detection
System must:
Detect source language
Translate to English
FR-4: AI Processing
AI must return structured JSON:
{
  "detectedLanguage": "German",
  "summary": "...",
  "containsAppointment": true,
  "appointment": {
    "title": "...",
    "date": "2026-03-15",
    "time": "10:30",
    "location": "Berlin Bürgeramt",
    "category": "Government"
  }
}
FR-5: Confirmation Flow
Frontend must:
Show summary
Show appointment card if detected
Ask for confirmation
Call existing POST /api/schedules
FR-6: Privacy
No document storage
No blob storage for letters
No DB persistence
In-memory processing only
Logs must exclude raw content