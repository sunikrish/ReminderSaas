Project: Reminder SaaS — Document Scanner to Reminder Flow
Objective
Enable the user to scan documents with the mobile camera, extract intent, identify reminder details, ask for confirmation, and set reminders automatically via the backend.
Features
1. UI — Camera Button
Add a camera icon button at the bottom of the main Reminder screen.
The button should be always visible on Android and iOS.
On click, open the device camera in a modal or full screen.
2. Camera Capture
Capture a photo of a document.
Optionally allow retake if user is not satisfied.
Save the photo temporarily in memory or local cache.
3. Send Image to Azure AI for Analysis
Azure Resources Required:
Azure Form Recognizer / Document Intelligence (or Azure Cognitive Services OCR)
Azure AI OpenAI (optional) for summarizing intent
Process:
Convert image to byte array or stream.
Send image to Azure Document Intelligence for text extraction.
Receive the extracted text.
4. Extract Reminder Details
From the extracted text, identify:
Title / Subject
Description / Details
Date and Time for the reminder
Use Azure AI / LLM to summarize intent and identify potential reminder fields.
5. Ask User Confirmation
After processing, display a confirmation dialog with:
Title: <extracted title>
Description: <extracted description>
Reminder Date: <extracted date/time>
Buttons: Confirm / Edit / Cancel
If user selects Edit, allow manual editing of Title/Description/Date.
6. Save Reminder via Backend
On confirmation, call Azure Function API CreateReminder.
Pass extracted or edited fields as CreateReminderRequest.
Show a success toast / snackbar after saving.
7. Handle Errors
Network error → show retry option
AI parsing failure → show “Could not parse reminder, try again”
Camera permission denied → request permission


