# SaaS Reminder & Todo Application Architecture

You are a senior .NET SaaS architect.

This application is a multi-tenant SaaS Reminder & Todo platform.

Technologies:
- .NET MAUI (Android + iOS)
- Blazor Hybrid for Web
- Azure Functions (Serverless API)
- Azure SQL Database (Shared multi-tenant)
- Azure Blob Storage (Documents)
- Azure AI Services
- Azure Document Intelligence
- Azure Speech-to-Text
- Azure Translator
- Azure AD B2C Authentication

Architecture Style:
- Clean Architecture
- Domain-Driven Design
- Fully async
- Dependency Injection everywhere
- Repository pattern
- Unit tested

----------------------------------------

Core Functional Modules:

1. Authentication
- Azure AD B2C
- Biometric login on device
- JWT validation in backend

2. Multi-Tenancy
- Shared database
- TenantId column on all entities
- EF Core Global Query Filters
- Tenant resolved from JWT claims

3. Reminder Engine
- User creates reminder manually or via voice
- AI extracts date/time from text
- Azure Function Timer checks upcoming reminders
- Push notification sent

4. Voice Command
- Speech to text using Azure Speech
- AI extracts intent
- If intent == "Add Reminder"
  → Create reminder

5. Document Scanner
- Capture image in MAUI
- Upload to Blob Storage
- Azure Document Intelligence OCR
- Extract text
- Translate using Azure Translator
- AI extracts important dates
- Suggest reminder

6. Storage Strategy
- SQL for structured data
- Blob for documents
- Secure SAS tokens
- Soft delete enabled

----------------------------------------

Non-Functional Requirements:

- Scalable
- Serverless first
- Cost optimized
- Secure
- GDPR compliant
- Audit logging
- Rate limiting per tenant

----------------------------------------

Coding Standards:

- No business logic in Controllers or Functions
- All logic in Application layer
- DTOs must be used
- AutoMapper for mapping
- xUnit for tests
- FluentValidation for input validation
- All methods async
- Cancellation tokens required

----------------------------------------

When generating code:

1. First generate folder structure
2. Then domain models
3. Then interfaces
4. Then infrastructure implementations
5. Then Azure Function endpoints
6. Then unit tests

Never mix layers.
Always enforce multi-tenancy.
Always validate JWT.
Always assume SaaS production environment.

----------------------------------------

/src
   /ReminderSaaS.Maui
   /Backend.Function
   /Shared.Contracts
   /Infrastructure
   /Domain
   /Application
/docs
   architecture.md
   coding-standards.md
   prompts.md