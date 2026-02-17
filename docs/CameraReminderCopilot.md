# GitHub Copilot: Generate Reminder Scanner Feature

## Context

- Project is a .NET MAUI cross-platform app
- Backend is Azure Functions with endpoint `CreateReminder` that accepts `CreateReminderRequest` (Title, Description, ReminderDate)
- EF Core is set up for persistence
- Application uses Clean Architecture (UI → ViewModel → Application → Infrastructure → Domain)
- Goal: Scan a document with the camera, extract reminder info, confirm with user, and save reminder

## Requirements for Copilot

1. Add a **camera button** at the **bottom of the main Reminder screen**
2. When user clicks:
    - Open **device camera** using MAUI MediaPicker or Community Toolkit
    - Allow user to take a picture and optionally retake
3. Send captured image to **Azure Document Intelligence / Form Recognizer**
    - If service key or endpoint missing, prompt user to provide
    - Receive extracted text
4. Parse extracted text to identify reminder fields:
    - Title
    - Description
    - ReminderDate
    - Use LLM summarization if needed
5. Show **confirmation dialog** to user:
    - Pre-filled fields
    - Buttons: Confirm / Edit / Cancel
6. If user confirms:
    - Call **Backend Function `CreateReminder`**
    - Pass fields via `CreateReminderRequest`
    - Show success notification
7. Handle errors:
    - Camera permission denied → request
    - Network error → retry option
    - AI parsing failure → ask user to edit manually
8. Use **MVVM pattern**:
    - Bind camera button command to ViewModel
    - All API calls in injected services
9. Keep UI clean and async/await for all network calls
10. Use **ObservableCollection** for reminders list

## Additional Guidance

- Copilot should generate:
    - XAML button and layout
    - ViewModel camera command
    - Camera capture code
    - HttpClient call to Azure Document Intelligence
    - Parsing logic for Title/Description/Date
    - Confirmation dialog with editable fields
    - HttpClient call to `CreateReminder` endpoint
- Copilot should **ask the user** if any Azure resources (endpoint/key) are missing
- Keep domain logic in Application/Infrastructure; UI only calls services
