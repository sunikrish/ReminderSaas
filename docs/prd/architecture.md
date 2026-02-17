Technical Architecture – Reminder SaaS
1. Architecture Style
Clean Architecture + Vertical Slice
Layers:
Presentation (MAUI)
Application
Domain
Infrastructure
Backend.Functions
Shared.Contracts
2. Technology Stack
Frontend:
.NET MAUI
MVVM
CarouselView navigation
Backend:
Azure Functions (Isolated .NET 8+)
Azure SQL
Azure Blob (temporary document processing only)
Azure AI Services
Azure Document Intelligence
3. Navigation Model
HomePage contains CarouselView:
MonthlySummaryPage (default)
CalendarPage
CategorizedPage
Swipe horizontally between pages.
Bottom fixed bar:
Camera icon
Speaker icon
4. AI Agent Architecture
We implement AI as Application-level agents.
Interfaces:
IMonthlySummaryAgent
IDocumentIntentAgent
IVoiceCommandAgent
Infrastructure implements them using Azure AI.
5. Data Flow
Camera Flow:
MAUI → Azure Function → Document Intelligence
→ Azure OpenAI → Intent Extracted → Suggest Schedule → Confirm → Save to SQL
Voice Flow:
MAUI → Speech to Text → Azure Function → Intent Agent
→ Structured Command → Apply to DB