Phase 1 – Technical Requirements (Copilot Prompt)
This document defines implementation constraints for Phase 1.
Copilot must strictly follow Clean Architecture.
1️⃣ Domain Layer
Entity: Schedule
Schedule
---------
Id (Guid)
Title (string, required)
Description (string?)
Date (DateOnly)
Time (TimeOnly?)
Location (string?)
Category (enum)
CreatedAt (DateTime)
UpdatedAt (DateTime?)
Enum: ScheduleCategory
Health
Tax
School
Personal
Other
No EF references inside Domain.
2️⃣ Application Layer
Interfaces
Create:
IScheduleService
IScheduleRepository
IScheduleService
Methods:
Task<List<ScheduleDto>> GetSchedulesByMonthAsync(int year, int month);
Task<ScheduleDto> GetByIdAsync(Guid id);
Task<Guid> CreateAsync(CreateScheduleDto dto);
Task UpdateAsync(UpdateScheduleDto dto);
Task DeleteAsync(Guid id);
DTOs (in Shared.Contracts)
Create:
ScheduleDto
CreateScheduleDto
UpdateScheduleDto
No domain entities returned to UI.
3️⃣ Infrastructure Layer
AppDbContext
DbSet:
DbSet<Schedule> Schedules
Configure:
Required fields
Index on Date
Enum stored as string
ScheduleRepository
Implements:
IScheduleRepository
Use EF Core async methods only.
4️⃣ Azure Functions Layer
Create HTTP-triggered functions:
GetSchedulesByMonth
Route:
GET /api/schedules?year=2026&month=3
GetScheduleById
GET /api/schedules/{id}
CreateSchedule
POST /api/schedules
UpdateSchedule
PUT /api/schedules/{id}
DeleteSchedule
DELETE /api/schedules/{id}
All functions:
Validate input
Call IScheduleService
Return proper HTTP status codes
Must not contain business logic
5️⃣ MAUI UI Requirements
Structure
/Views
   CalendarPage.xaml
   ScheduleFormPage.xaml
   ScheduleDetailPage.xaml

/ViewModels
   CalendarViewModel
   ScheduleFormViewModel
   ScheduleDetailViewModel
Use MVVM.
6️⃣ Calendar UI Requirements
Use:
CollectionView OR third-party MAUI calendar
Month navigation
Date selection
Indicator dots per schedule
When date selected:
Load schedules for that date
Show list below calendar
7️⃣ ViewModel Rules
All data loading must:
Call backend API
Use HttpClient
Be async
Use ObservableCollection
No direct DB calls from MAUI.
8️⃣ Validation Rules
Title required.
Date required.
Use FluentValidation OR manual validation.
9️⃣ Dependency Injection
Register:
IScheduleRepository
IScheduleService
DbContext
In Program.cs (Backend)
Register HttpClient in MAUI.
🔟 Performance Rules
No synchronous DB calls
Use AsNoTracking for read operations
Paginate if month contains many records