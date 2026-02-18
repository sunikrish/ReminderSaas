using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using ReminderSaaS.Application.Schedules;
using System.Net;

namespace Backend.Function;

/// <summary>
/// Azure Function to retrieve a schedule by its ID.
/// Route: GET /api/schedules/{id}
/// </summary>
public class GetScheduleById
{
    private readonly ILogger<GetScheduleById> _logger;
    private readonly IScheduleService _scheduleService;

    public GetScheduleById(ILogger<GetScheduleById> logger, IScheduleService scheduleService)
    {
        _logger = logger;
        _scheduleService = scheduleService;
    }

    [Function("GetScheduleById")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "schedules/{id}")] HttpRequestData req,
        string id)
    {
        _logger.LogInformation("GetScheduleById function triggered for ID: {ScheduleId}", id);

        try
        {
            if (!Guid.TryParse(id, out var scheduleId))
            {
                _logger.LogWarning("Invalid schedule ID format: {Id}", id);
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                await response.WriteAsJsonAsync(new { error = "Invalid schedule ID format" });
                return response;
            }

            var schedule = await _scheduleService.GetByIdAsync(scheduleId);

            if (schedule == null)
            {
                _logger.LogWarning("Schedule not found for ID: {ScheduleId}", scheduleId);
                var response = req.CreateResponse(HttpStatusCode.NotFound);
                await response.WriteAsJsonAsync(new { error = "Schedule not found" });
                return response;
            }

            _logger.LogInformation("Schedule retrieved successfully: {ScheduleId}", scheduleId);
            var response_ok = req.CreateResponse(HttpStatusCode.OK);
            await response_ok.WriteAsJsonAsync(schedule);
            return response_ok;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetScheduleById function");
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteAsJsonAsync(new { error = "An error occurred while retrieving the schedule" });
            return response;
        }
    }
}
