# GitHub Copilot Combined Prompt

## Context
We are building a .NET MAUI + Azure Functions SaaS app.  
Backend uses Clean Architecture: Domain, Application, Infrastructure, Functions.  
Database is Azure SQL / EF Core.  

## References
- /docs/CameraReminderCopilot.md (feature requirement: camera → document scan → reminder)  
- /docs/coding-standards.md (naming, MVVM, async/await, Clean Architecture, error handling)  
- /docs/architecture.md (solution structure, DI, services, EF Core, Function triggers)

## Task for Copilot
- Generate a **MAUI page** or **ViewModel** that implements the feature in CameraReminderCopilot.md  
- Follow **coding standards** from coding-standards.md  
- Follow **project architecture** from architecture.md  
- Use **async/await** and DI  
- Keep domain logic in Application/Infrastructure  
- Use Azure Functions endpoint `CreateReminder` for saving reminders  
- Ask user if any Azure resource (Form Recognizer, OpenAI endpoint) is missing  

## Output
- MAUI XAML page and code-behind / ViewModel  
- Services for AI call and HTTP API call  
- Command binding for camera button  
- Confirmation dialog with pre-filled fields  
- Proper namespaces, folder structure, PascalCase/camelCase rules
