using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using ReminderSaaS.Application.Schedules;
using System.Net;

namespace Backend.Function;

/// <summary>
/// Azure Function to retrieve schedules for a specific month.
/// Route: GET /api/schedules?year=2026&month=3
/// </summary>
public class GetSchedulesByMonth
{
    private readonly ILogger<GetSchedulesByMonth> _logger;
    private readonly IScheduleService _scheduleService;

    public GetSchedulesByMonth(ILogger<GetSchedulesByMonth> logger, IScheduleService scheduleService)
    {
        _logger = logger;
        _scheduleService = scheduleService;
    }

    [Function("GetSchedulesByMonth")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "schedules")] HttpRequestData req)
    {
        _logger.LogInformation("GetSchedulesByMonth function triggered");

        try
        {
            var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            var yearStr = query["year"];
            var monthStr = query["month"];

            if (!int.TryParse(yearStr, out var year) || !int.TryParse(monthStr, out var month))
            {
                _logger.LogWarning("Invalid year or month parameters");
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                await response.WriteAsJsonAsync(new { error = "Year and month must be valid integers" });
                return response;
            }

            if (month < 1 || month > 12)
            {
                _logger.LogWarning("Invalid month value: {Month}", month);
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                await response.WriteAsJsonAsync(new { error = "Month must be between 1 and 12" });
                return response;
            }

            var schedules = await _scheduleService.GetSchedulesByMonthAsync(year, month);

            var response_ok = req.CreateResponse(HttpStatusCode.OK);
            await response_ok.WriteAsJsonAsync(schedules);
            return response_ok;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetSchedulesByMonth function");
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteAsJsonAsync(new { error = "An error occurred while retrieving schedules" });
            return response;
        }
    }
}
