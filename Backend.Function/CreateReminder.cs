using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using ReminderSaaS.Shared.Contracts;
using ReminderSaaS.Application.Reminders;
using System.Net;

namespace Backend.Function;

public class CreateReminder
{
    private readonly ILogger<CreateReminder> _logger;
    private readonly IReminderService _reminderService;

    public CreateReminder(ILogger<CreateReminder> logger, IReminderService reminderService)
    {
        _logger = logger;
        _reminderService = reminderService;
    }

    [Function("CreateReminder")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
{
    var request = await req.ReadFromJsonAsync<CreateReminderRequest>();

    var result = await _reminderService.CreateAsync(request!);

    var response = req.CreateResponse(HttpStatusCode.OK);
    await response.WriteAsJsonAsync(result);

    return response;
}
}
