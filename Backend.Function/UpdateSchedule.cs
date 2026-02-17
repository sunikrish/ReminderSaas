using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using ReminderSaaS.Application.Schedules;
using ReminderSaaS.Shared.Contracts.Schedules;
using System.Net;

namespace Backend.Function;

/// <summary>
/// Azure Function to update an existing schedule.
/// Route: PUT /api/schedules/{id}
/// </summary>
public class UpdateSchedule
{
    private readonly ILogger<UpdateSchedule> _logger;
    private readonly IScheduleService _scheduleService;

    public UpdateSchedule(ILogger<UpdateSchedule> logger, IScheduleService scheduleService)
    {
        _logger = logger;
        _scheduleService = scheduleService;
    }

    [Function("UpdateSchedule")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "schedules/{id}")] HttpRequestData req,
        string id)
    {
        _logger.LogInformation("UpdateSchedule function triggered with ID: {Id}", id);

        try
        {
            if (!Guid.TryParse(id, out var scheduleId))
            {
                _logger.LogWarning("Invalid schedule ID format: {Id}", id);
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                await response.WriteAsJsonAsync(new { error = "Invalid schedule ID format" });
                return response;
            }

            var updateDto = await req.ReadFromJsonAsync<UpdateScheduleDto>();

            if (updateDto == null)
            {
                _logger.LogWarning("Request body is empty or invalid");
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                await response.WriteAsJsonAsync(new { error = "Request body is required" });
                return response;
            }

            // Verify the ID in the URL matches the ID in the body
            if (updateDto.Id != scheduleId)
            {
                _logger.LogWarning("ID mismatch: URL ID {UrlId} vs Body ID {BodyId}", scheduleId, updateDto.Id);
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                await response.WriteAsJsonAsync(new { error = "ID in URL does not match ID in request body" });
                return response;
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(updateDto.Title))
            {
                _logger.LogWarning("Title is required");
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                await response.WriteAsJsonAsync(new { error = "Title is required" });
                return response;
            }

            await _scheduleService.UpdateAsync(updateDto);

            _logger.LogInformation("Schedule updated successfully with ID: {Id}", scheduleId);
            var response_ok = req.CreateResponse(HttpStatusCode.OK);
            await response_ok.WriteAsJsonAsync(new { message = "Schedule updated successfully" });
            return response_ok;
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Schedule not found");
            var response = req.CreateResponse(HttpStatusCode.NotFound);
            await response.WriteAsJsonAsync(new { error = ex.Message });
            return response;
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error in UpdateSchedule");
            var response = req.CreateResponse(HttpStatusCode.BadRequest);
            await response.WriteAsJsonAsync(new { error = ex.Message });
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UpdateSchedule function");
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteAsJsonAsync(new { error = "An error occurred while updating the schedule" });
            return response;
        }
    }
}
