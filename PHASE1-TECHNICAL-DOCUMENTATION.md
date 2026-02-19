# ReminderSaaS Phase 1 - Technical Documentation

## Executive Summary
Phase 1 implements a complete schedule management system with full CRUD operations across a .NET MAUI mobile frontend and Azure Functions backend, with SQL Server persistence.

---

## 1. Architecture Overview

### Technology Stack
- **Frontend**: .NET MAUI (net10.0-android)
- **Backend**: Azure Functions (isolated worker model)
- **Database**: SQL Server 2022
- **Pattern**: Clean Architecture with MVVM
- **Communication**: RESTful HTTP APIs with JSON

### Project Structure
```
/src
├── ReminderSaaS.Maui/          # MAUI Frontend (Android)
│   ├── Views/Schedules/        # UI Pages
│   ├── ViewModels/Schedules/   # MVVM Logic
│   ├── Services/               # API Clients
│   ├── Models/                 # UI Models
│   └── Converters/             # Value Converters
├── Backend.Function/            # Azure Functions
│   ├── CreateSchedule.cs
│   ├── GetSchedulesByMonth.cs
│   ├── GetScheduleById.cs
│   ├── UpdateSchedule.cs
│   └── DeleteSchedule.cs
├── Domain/                      # Entity Models
│   └── Entities/
│       ├── Schedule.cs
│       └── Reminder.cs
├── Application/                 # Business Logic Layer
│   ├── Services/
│   │   ├── ScheduleService.cs
│   │   └── ReminderService.cs
│   └── Interfaces/
├── Infrastructure/              # Data Access Layer
│   └── Persistence/
└── Shared.Contracts/           # Shared DTOs
    └── Schedules/
```

---

## 2. Database Schema

### Schedule Entity
```sql
CREATE TABLE [dbo].[Schedules] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [Title] NVARCHAR(255) NOT NULL,
    [Description] NVARCHAR(1000),
    [Date] DATE NOT NULL,
    [Time] TIME,
    [Location] NVARCHAR(255),
    [Category] NVARCHAR(50) NOT NULL DEFAULT 'Other',
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 DEFAULT GETUTCDATE()
)
```

### Categories Supported
- Health
- Tax
- School
- Personal
- Other (default if not specified)

### Key Constraints
- `Title`: Mandatory, max 255 characters
- `Date`: Mandatory, format YYYY-MM-DD
- `Category`: Mandatory, defaults to "Other" if not provided
- `Time`: Optional (nullable)
- `Location`: Optional (nullable)
- `Description`: Optional (nullable)

---

## 3. API Endpoints

### Base URL
```
http://localhost:7071
```

### Endpoints

#### 1. Create Schedule
**POST** `/api/schedules`

Request Body:
```json
{
  "title": "Doctor Appointment",
  "date": "2026-02-25",
  "time": "14:30:00",
  "category": "Health",
  "description": "Annual checkup",
  "location": "Medical Center"
}
```

Response (201 Created):
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000"
}
```

#### 2. Get Schedules by Month
**GET** `/api/schedules?year=2026&month=2`

Response (200 OK):
```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "title": "Doctor Appointment",
    "date": "2026-02-25",
    "time": "14:30:00",
    "category": "Health",
    "description": "Annual checkup",
    "location": "Medical Center"
  }
]
```

#### 3. Get Schedule by ID
**GET** `/api/schedules/{id}`

Response (200 OK):
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "title": "Doctor Appointment",
  "date": "2026-02-25",
  "time": "14:30:00",
  "category": "Health",
  "description": "Annual checkup",
  "location": "Medical Center"
}
```

Response (404 Not Found):
```json
null
```

#### 4. Update Schedule
**PUT** `/api/schedules/{id}`

Request Body:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "title": "Doctor Appointment - Updated",
  "date": "2026-02-26",
  "time": "15:00:00",
  "category": "Health",
  "description": "Annual checkup - rescheduled",
  "location": "Medical Center B"
}
```

Response (204 No Content)

#### 5. Delete Schedule
**DELETE** `/api/schedules/{id}`

Response (204 No Content)

---

## 4. Frontend Architecture

### Navigation Structure
```
Shell Navigation Routes:
├── ///calendar           # CalendarPage - Main calendar view
├── ///scheduleform       # ScheduleFormPage - Create/Edit schedule
│   └── ?id={scheduleId}  # Edit mode with schedule ID
├── ///scheduledetail     # ScheduleDetailPage - View schedule details
│   └── ?scheduleid={id}  # Schedule to display
└── ///reminders         # Reminders (placeholder for Phase 2)
```

### MVVM Components

#### CalendarViewModel
**Key Properties:**
- `CalendarWeeks`: ObservableCollection of weeks with day cells
- `SelectedDate`: Currently selected date
- `CurrentMonth/CurrentYear`: Active calendar month/year
- `IsLoading`: Loading indicator state

**Key Methods:**
- `InitializeCommand`: Load calendar for current month
- `PreviousMonthCommand`: Navigate to previous month
- `NextMonthCommand`: Navigate to next month
- `SelectDateCommand`: Handle date cell taps
- `LoadSchedulesForMonthAsync()`: Fetch schedules from API

#### ScheduleFormViewModel
**Key Properties:**
- `Title`: Schedule title (required)
- `SelectedDate`: Schedule date (required)
- `SelectedCategory`: Category selection (required, defaults to "Other")
- `SelectedTime`: Optional time
- `Location`: Optional location
- `Description`: Optional description

**Key Methods:**
- `InitializeForCreate()`: Prepare form for new schedule
- `InitializeForEditAsync(id)`: Load existing schedule for editing
- `SaveAsync()`: Validate and submit schedule
- `CancelAsync()`: Discard changes and navigate back

**Validation Rules:**
- Title: Required, non-empty
- Date: Required, cannot be null or default
- Category: Required, auto-selects "Other" if empty

#### ScheduleDetailViewModel
**Key Properties:**
- `Schedule`: Currently displayed schedule DTO
- `IsLoading`: Loading indicator state
- `ErrorMessage`: Error display

**Key Methods:**
- `LoadScheduleAsync(id)`: Fetch schedule details
- `EditScheduleAsync()`: Navigate to edit form
- `DeleteScheduleAsync()`: Delete with confirmation

### UI Components

#### CalendarPage
- Month navigation (previous/next buttons)
- 7-column calendar grid (Sun-Sat)
- Day cells with:
  - Date number (bold, 16px)
  - Green dot indicators showing schedule count
  - Consistent height (70px minimum)
  - Border colors: Red (today), Green (has events), Gray (default)

**Calculation:**
- Generates weeks array for full month
- Highlights current date in red
- Shows green dots for dates with schedules
- 1 dot = 1 schedule, 2 dots = 2 schedules, 3+ dots = "●●●"

#### ScheduleFormPage
- Mandatory fields marked with red asterisk (*):
  - Title *
  - Date *
  - Category *
- Optional fields:
  - Time (TimePicker)
  - Location
  - Description (Editor, 100px height)
- Save/Cancel buttons
- Error message display area
- Loading indicator during submission

#### ScheduleDetailPage
- Display-only fields:
  - Title (bold)
  - Category (badge with color)
  - Date
  - Time
  - Location
  - Description
- Action buttons:
  - Edit (navigates to form with pre-populated data)
  - Delete (with confirmation dialog)

### Value Converters
- `DateOnlyConverter`: Converts DateOnly ↔ DateTime for DatePicker binding
- `StringNullOrEmptyBoolConverter`: Empty string → false for visibility
- `CountToDotIndicatorConverter`: int → dot string (●, ●●, ●●●)

---

## 5. Data Models

### ScheduleDto (Shared.Contracts)
```csharp
public record ScheduleDto(
    Guid Id,
    string Title,
    DateOnly Date,
    string Category,
    string? Description = null,
    TimeOnly? Time = null,
    string? Location = null
);
```

### CreateScheduleDto
```csharp
public record CreateScheduleDto(
    string Title,
    DateOnly Date,
    string Category,
    string? Description = null,
    TimeOnly? Time = null,
    string? Location = null
);
```

### UpdateScheduleDto
```csharp
public record UpdateScheduleDto(
    Guid Id,
    string Title,
    DateOnly Date,
    string Category,
    string? Description = null,
    TimeOnly? Time = null,
    string? Location = null
);
```

---

## 6. Service Layer

### IScheduleApiClient (Frontend)
```csharp
public interface IScheduleApiClient
{
    Task<List<ScheduleDto>> GetSchedulesByMonthAsync(int year, int month);
    Task<ScheduleDto?> GetScheduleByIdAsync(Guid id);
    Task<Guid> CreateScheduleAsync(CreateScheduleDto dto);
    Task UpdateScheduleAsync(UpdateScheduleDto dto);
    Task DeleteScheduleAsync(Guid id);
}
```

**Implementation:** `ScheduleApiClient.cs`
- Base URL: `http://localhost:7071` (Debug) / configured endpoint (Release)
- Timeout: 30 seconds
- JSON serialization with custom options (PropertyNameCaseInsensitive: true)

### IScheduleService (Backend)
```csharp
public interface IScheduleService
{
    Task<List<Schedule>> GetSchedulesByMonthAsync(int year, int month);
    Task<Schedule?> GetByIdAsync(Guid id);
    Task<Guid> CreateAsync(CreateScheduleDto dto);
    Task UpdateAsync(UpdateScheduleDto dto);
    Task DeleteAsync(Guid id);
}
```

---

## 7. User Flows

### Create Schedule Flow
```
CalendarPage (tap date)
  → ScheduleFormPage (InitializeForCreate)
    → Fill Title, Date (pre-selected), Category (defaults to "Other")
    → Fill optional fields
    → Save
      → Validation (Title, Date, Category required)
      → API: POST /api/schedules
      → Success: "Schedule created successfully!" alert
      → Navigate back to ///calendar
      → Calendar refreshes with new dot indicator
```

### Edit Schedule Flow
```
CalendarPage (tap date with schedule)
  → Select schedule from list
  → ScheduleDetailPage (LoadScheduleAsync)
  → Edit button
  → ScheduleFormPage (InitializeForEditAsync with ID)
    → Form pre-populated with existing data
    → Modify fields
    → Save
      → Validation
      → API: PUT /api/schedules/{id}
      → Success: "Schedule updated successfully!" alert
      → Navigate back to ///calendar
      → Calendar refreshes
```

### Delete Schedule Flow
```
ScheduleDetailPage (Delete button)
  → Confirmation dialog: "Are you sure you want to delete '{Title}'?"
  → User confirms
    → API: DELETE /api/schedules/{id}
    → Success: "Schedule deleted successfully!" alert
    → Navigate back to ///calendar
    → Calendar refreshes
```

### View Schedules by Date
```
CalendarPage loads
  → OnAppearing: Call LoadSchedulesForMonthAsync
  → API: GET /api/schedules?year=2026&month=2
  → ViewModel processes response:
    - Groups schedules by date
    - Updates day cell colors (green if has schedules)
    - Calculates dot indicators
  → CalendarPage renders with:
    - Red highlighted dates (today)
    - Green highlighted dates (with schedules)
    - Dot count matching schedule count
```

---

## 8. Configuration & Settings

### Backend (local.settings.json)
```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "DefaultEndpointsProtocol=https;AccountName=...",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "ConnectionStrings:DefaultConnection": "Server=localhost;Database=ReminderSaaS_Dev;..."
  }
}
```

### Frontend (MauiProgram.cs)
```csharp
services.AddScoped<IScheduleApiClient>(sp =>
    new ScheduleApiClient("http://localhost:7071"));
services.AddScoped<CalendarViewModel>();
services.AddScoped<ScheduleFormViewModel>();
services.AddScoped<ScheduleDetailViewModel>();
```

---

## 9. Error Handling

### Validation Errors
- **Title empty**: "Title is required"
- **Date null**: "Schedule date is required"
- **Category empty**: "Category is required"

### API Errors
- **Network error**: Display generic "Failed to save schedule" with exception message
- **Timeout (30s)**: Same generic error message
- **404 Not Found**: "Schedule not found" on detail page
- **500 Server Error**: Display server error message from response

### User Feedback
- Loading indicator during async operations
- Success alerts for create/update/delete operations
- Error alerts with specific messages
- Red error text displayed above buttons

---

## 10. Performance Optimizations Implemented

### Timeline
1. ✅ Removed verbose Debug.WriteLine logs (100+ per operation)
2. ✅ Eliminated per-day cell logging in calendar rendering
3. ✅ Removed schedule detail inspection logs
4. ✅ Cleaned up API response logging

### Current Optimizations
- Calendar refresh only on page appear (not every property change)
- Efficient dot indicator calculation (LINQ operations)
- Minimal UI rebuilds (MVVM binding optimization)
- Async/await for non-blocking operations

### Remaining Debug Logs
- Error-only logs for troubleshooting (kept intentionally)
- Stack traces for critical failures

---

## 11. Known Limitations & Future Enhancements

### Current Limitations
1. **Single user**: No authentication/authorization yet
2. **No reminders**: Reminder entities defined but not implemented
3. **No time zone support**: Uses system time only
4. **No recurring schedules**: One-time events only
5. **No attachments**: Text-only schedule details
6. **No search/filter**: Only monthly calendar view
7. **iOS/macOS**: Skipped due to code signing (Android only for Phase 1)

### Phase 2 Candidates
- User authentication and multi-user support
- Reminder notifications (local and push)
- Recurring schedule patterns
- Search and filtering capabilities
- Schedule categories with color customization
- Attachments/media support
- Export/sharing functionality
- Analytics and statistics
- iOS and macOS support

---

## 12. Testing Summary

### Tested Scenarios
✅ Create schedule with all fields
✅ Create schedule with only mandatory fields
✅ Edit existing schedule
✅ Delete schedule with confirmation
✅ View calendar with multiple schedules on same date
✅ Navigate between months
✅ Validation error messages
✅ Success alert messages
✅ Navigate back from form/detail pages
✅ Handle missing schedule (404 response)

### Test Environment
- Backend: localhost:7071
- Database: SQL Server 2022 (Docker)
- Frontend: Android Emulator (API level 34)
- Network: Direct HTTP (no SSL for dev)

---

## 13. Deployment Considerations

### Backend Deployment
- Target: Azure Functions
- Runtime: .NET 10.0 Isolated Worker
- Connection String: Requires Azure SQL Database or on-premises SQL Server
- Authentication: None yet (add in Phase 2)

### Frontend Deployment
- Target: Google Play Store (Android)
- Build: Release configuration with proper signing
- API Endpoint: Configure via settings for production URL
- Secrets: API keys and connection strings in secure configuration

### Database Migrations
- Current: Manual schema creation
- Future: Consider EF Core migrations for automated deployments

---

## 14. File Locations Reference

### Frontend Files
- Views: `/ReminderSaaS.Maui/Views/Schedules/`
- ViewModels: `/ReminderSaaS.Maui/ViewModels/Schedules/`
- Services: `/ReminderSaaS.Maui/Services/`
- Converters: `/ReminderSaaS.Maui/Converters/`

### Backend Files
- Functions: `/Backend.Function/`
- Domain: `/Domain/Entities/`
- Application: `/Application/Services/`
- Contracts: `/Shared.Contracts/Schedules/`

---

## 15. Quick Start Commands

### Build All
```bash
dotnet build -c Debug
```

### Build Android Only
```bash
dotnet build ReminderSaaS.Maui/ReminderSaaS.Maui.csproj -f net10.0-android -c Debug
```

### Run Backend
```bash
cd Backend.Function && func start
```

### Run Frontend
```bash
dotnet run -f net10.0-android --project ReminderSaaS.Maui/ReminderSaaS.Maui.csproj
```

### Test API Endpoints
```bash
# Get schedules for February 2026
curl -s "http://localhost:7071/api/schedules?year=2026&month=2" | jq .

# Get specific schedule
curl -s "http://localhost:7071/api/schedules/{id}" | jq .

# Create schedule
curl -X POST "http://localhost:7071/api/schedules" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Test Event",
    "date": "2026-02-20",
    "category": "Personal"
  }' | jq .
```

---

## 16. Git Configuration

### .gitignore Additions (Phase 1)
- `*.dll` - Compiled assemblies
- `*.pdb` - Debug symbols
- `obj/` - Build output
- `bin/` - Build artifacts
- `.vs/` - Visual Studio cache
- `.vscode/` - VS Code settings

---

## Summary

Phase 1 delivers a fully functional schedule management mobile application with:
- ✅ Complete CRUD operations via REST API
- ✅ Clean architecture with separation of concerns
- ✅ MVVM pattern for testability and maintainability
- ✅ Professional calendar UI with visual indicators
- ✅ Form validation with user-friendly errors
- ✅ Performance optimization with minimal logging
- ✅ Secure data persistence in SQL Server

**Ready for Phase 2 enhancements**: Authentication, notifications, recurring schedules, and multi-user support.
