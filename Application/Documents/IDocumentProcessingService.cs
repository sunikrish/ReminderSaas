using ReminderSaaS.Shared.Contracts.Documents;

namespace ReminderSaaS.Application.Documents;

public interface IDocumentProcessingService
{
    Task<DocumentAnalysisResultDto> AnalyzeDocumentAsync(Stream imageStream);
}
