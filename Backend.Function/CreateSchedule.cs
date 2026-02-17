using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using ReminderSaaS.Application.Schedules;
using ReminderSaaS.Shared.Contracts.Schedules;
using System.Net;

namespace Backend.Function;

/// <summary>
/// Azure Function to create a new schedule.
/// Route: POST /api/schedules
/// </summary>
public class CreateSchedule
{
    private readonly ILogger<CreateSchedule> _logger;
    private readonly IScheduleService _scheduleService;

    public CreateSchedule(ILogger<CreateSchedule> logger, IScheduleService scheduleService)
    {
        _logger = logger;
        _scheduleService = scheduleService;
    }

    [Function("CreateSchedule")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "schedules")] HttpRequestData req)
    {
        _logger.LogInformation("CreateSchedule function triggered");

        try
        {
            var createDto = await req.ReadFromJsonAsync<CreateScheduleDto>();

            if (createDto == null)
            {
                _logger.LogWarning("Invalid or missing request body");
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                await response.WriteAsJsonAsync(new { error = "Invalid or missing request body" });
                return response;
            }

            _logger.LogInformation("Creating schedule: {Title} for {Date}", createDto.Title, createDto.Date);

            // Call the service to create the schedule
            var scheduleId = await _scheduleService.CreateAsync(createDto);

            _logger.LogInformation("Successfully created schedule with ID: {Id}", scheduleId);
            var response_ok = req.CreateResponse(HttpStatusCode.Created);
            await response_ok.WriteAsJsonAsync(new { id = scheduleId });
            return response_ok;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CreateSchedule function");
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteAsJsonAsync(new { error = "An error occurred while creating the schedule" });
            return response;
        }
    }
}
