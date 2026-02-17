using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using ReminderSaaS.Application.Schedules;
using System.Net;

namespace Backend.Function;

/// <summary>
/// Azure Function to delete a schedule.
/// Route: DELETE /api/schedules/{id}
/// </summary>
public class DeleteScheduleFunction
{
    private readonly ILogger<DeleteScheduleFunction> _logger;
    private readonly IScheduleService _scheduleService;

    public DeleteScheduleFunction(ILogger<DeleteScheduleFunction> logger, IScheduleService scheduleService)
    {
        _logger = logger;
        _scheduleService = scheduleService;
    }

    [Function("DeleteSchedule")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "schedules/{id}")] HttpRequestData req,
        string id)
    {
        _logger.LogInformation("DeleteSchedule function triggered with ID: {Id}", id);

        try
        {
            if (!Guid.TryParse(id, out var scheduleId))
            {
                _logger.LogWarning("Invalid schedule ID format: {Id}", id);
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                await response.WriteAsJsonAsync(new { error = "Invalid schedule ID format" });
                return response;
            }

            // Check if schedule exists before deleting
            var schedule = await _scheduleService.GetByIdAsync(scheduleId);
            if (schedule == null)
            {
                _logger.LogInformation("Schedule not found with ID: {Id}", scheduleId);
                var response = req.CreateResponse(HttpStatusCode.NotFound);
                await response.WriteAsJsonAsync(new { error = "Schedule not found" });
                return response;
            }

            await _scheduleService.DeleteAsync(scheduleId);

            _logger.LogInformation("Schedule deleted successfully with ID: {Id}", scheduleId);
            var response_ok = req.CreateResponse(HttpStatusCode.OK);
            await response_ok.WriteAsJsonAsync(new { message = "Schedule deleted successfully" });
            return response_ok;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DeleteSchedule function");
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteAsJsonAsync(new { error = "An error occurred while deleting the schedule" });
            return response;
        }
    }
}
