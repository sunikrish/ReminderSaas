1. Project Structure
Backend / Domain / Application / Infrastructure / MAUI UI
Domain: Pure entities, no dependencies.
Application: Business logic and services, uses DTOs from Shared.Contracts.
Infrastructure: Database, APIs, Azure services.
Backend.Functions: Azure Function triggers, DI, minimal logic.
MAUI App: UI + ViewModels only; calls services via HTTP.
Always follow dependency inversion: UI → Application → Domain → Infrastructure.
Never reference Infrastructure or Domain directly from MAUI.

2. Naming Conventions
Classes: PascalCase (ReminderService, CreateReminderRequest)
Methods: PascalCase (CreateAsync, MarkAsCompleted)
Interfaces: Prefix with I (IReminderService, IReminderRepository)
Private fields: _camelCase (_repository, _context)
Local variables: camelCase
XAML elements: PascalCase (CameraButton, ReminderListView)

3. Files & Folder Structure
One class per file
Folder names = namespace names
MAUI XAML + XAML.cs in same folder, named the same as page (MainPage.xaml / MainPage.xaml.cs)
Keep services, models, helpers in separate subfolders
4. Clean Architecture Guidelines
UI layer: No direct database calls, only calls Application services or API clients
Application layer: Implements business logic, orchestrates domain and infrastructure
Domain layer: Entities and domain logic only, no EF Core or Http references
Infrastructure layer: EF Core DbContext, repository implementation, external service clients

5. Asynchronous Programming
Always use async/await for I/O operations (HTTP calls, DB calls, file I/O)
Return Task<T> or Task
Never use .Result or .Wait() in async methods

6. Dependency Injection
All services must be registered in Program.cs or MAUI MauiProgram.cs
Constructor injection only, no service locators
Example:
builder.Services.AddScoped<IReminderService, ReminderService>();
builder.Services.AddScoped<IReminderRepository, ReminderRepository>();

7. Error Handling
Use try/catch in boundary layers (UI, Function triggers, API clients)
Return meaningful messages to the user
Log exceptions via ILogger<T>

8. API / HTTP Clients
Use typed HttpClient with DI
Base URL configurable via appsettings or local.settings.json
Always use JSON serialization with System.Text.Json
Include retry for transient errors

9. MAUI UI Guidelines
Use MVVM pattern
Bind buttons to Commands in ViewModel
Use ObservableCollection for lists
Do not put business logic in XAML.cs
Use async commands for network calls

10. Azure Functions Guidelines
Keep functions minimal: only HTTP trigger, validation, DI call
Do not put business logic in function files
Name functions clearly: CreateReminder, GetReminders
Use isolated worker model for .NET 8+

11. Copilot Instructions
Always generate code following this document
Ask for missing Azure resources (keys, endpoints) before generating code
Use Clean Architecture and MVVM
Prefer async/await, strong typing, and error handling
Keep all domains, DTOs, and services in proper folders and namespaces
Maintain consistent naming and PascalCase / camelCase rules