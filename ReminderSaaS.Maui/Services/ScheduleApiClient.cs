using ReminderSaaS.Shared.Contracts.Schedules;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ReminderSaaS.Maui.Services;

/// <summary>
/// HTTP client implementation for schedule API operations.
/// </summary>
public class ScheduleApiClient : IScheduleApiClient
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {
            new JsonStringEnumConverter(),
            new JsonConverter_DateOnly(),
            new JsonConverter_TimeOnly()
        }
    };

    public ScheduleApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(10);
    }

    /// <summary>
    /// Gets schedules for a specific month and year.
    /// </summary>
    public async Task<List<ScheduleDto>> GetSchedulesByMonthAsync(int year, int month)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[ScheduleApiClient] Requesting schedules for {year}-{month:D2}");
            var response = await _httpClient.GetAsync($"/api/schedules?year={year}&month={month}");
            
            System.Diagnostics.Debug.WriteLine($"[ScheduleApiClient] Response status: {response.StatusCode}");
            response.EnsureSuccessStatusCode();

            var schedules = await response.Content.ReadFromJsonAsync<List<ScheduleDto>>(JsonOptions) ?? new();
            System.Diagnostics.Debug.WriteLine($"[ScheduleApiClient] Successfully retrieved {schedules.Count} schedules");
            
            foreach (var schedule in schedules)
            {
                System.Diagnostics.Debug.WriteLine($"[ScheduleApiClient]   - {schedule.Title} on {schedule.Date}");
            }
            
            return schedules;
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ScheduleApiClient] Error getting schedules (HTTP): {ex.Message}\nInner: {ex.InnerException?.Message}");
            return new(); // Return empty list on network error
        }
        catch (TaskCanceledException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ScheduleApiClient] Error getting schedules (Timeout): {ex.Message}");
            return new(); // Return empty list on timeout
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ScheduleApiClient] Error getting schedules: {ex.Message}\nStack: {ex.StackTrace}");
            return new(); // Return empty list on other errors
        }
    }

    /// <summary>
    /// Gets a schedule by its ID.
    /// </summary>
    public async Task<ScheduleDto?> GetScheduleByIdAsync(Guid id)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[ScheduleApiClient] Requesting schedule by ID: {id}");
            System.Diagnostics.Debug.WriteLine($"[ScheduleApiClient] Full URL: {_httpClient.BaseAddress}/api/schedules/{id}");
            
            var response = await _httpClient.GetAsync($"/api/schedules/{id}");
            
            System.Diagnostics.Debug.WriteLine($"[ScheduleApiClient] Response status for ID {id}: {response.StatusCode}");
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                System.Diagnostics.Debug.WriteLine($"[ScheduleApiClient] Schedule not found: {id}");
                return null;
            }

            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"[ScheduleApiClient] Response content: {responseContent}");
            
            var schedule = await response.Content.ReadFromJsonAsync<ScheduleDto>(JsonOptions);
            
            if (schedule != null)
            {
                System.Diagnostics.Debug.WriteLine($"[ScheduleApiClient] Successfully retrieved schedule: {schedule.Title} ({schedule.Id})");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[ScheduleApiClient] Schedule returned null for ID: {id}");
            }
            
            return schedule;
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ScheduleApiClient] Error getting schedule (HTTP): {ex.Message}\nInner: {ex.InnerException?.Message}");
            return null;
        }
        catch (TaskCanceledException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ScheduleApiClient] Error getting schedule (Timeout): {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ScheduleApiClient] Error getting schedule: {ex.Message}\nStack: {ex.StackTrace}");
            return null;
        }
    }

    /// <summary>
    /// Creates a new schedule.
    /// </summary>
    public async Task<Guid> CreateScheduleAsync(CreateScheduleDto dto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/schedules", dto);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            if (result != null && result.TryGetValue("id", out var idObj))
            {
                return Guid.Parse(idObj.ToString()!);
            }

            throw new InvalidOperationException("Failed to create schedule");
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error creating schedule (HTTP): {ex.Message}");
            throw;
        }
        catch (TaskCanceledException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error creating schedule (Timeout): {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error creating schedule: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Updates an existing schedule.
    /// </summary>
    public async Task UpdateScheduleAsync(UpdateScheduleDto dto)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"/api/schedules/{dto.Id}", dto);
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating schedule (HTTP): {ex.Message}");
            throw;
        }
        catch (TaskCanceledException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating schedule (Timeout): {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating schedule: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Deletes a schedule.
    /// </summary>
    public async Task DeleteScheduleAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/api/schedules/{id}");
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deleting schedule (HTTP): {ex.Message}");
            throw;
        }
        catch (TaskCanceledException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deleting schedule (Timeout): {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deleting schedule: {ex.Message}");
            throw;
        }
    }
}

// Custom JSON converters for DateOnly and TimeOnly
public class JsonConverter_DateOnly : JsonConverter<DateOnly>
{
    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (DateOnly.TryParse(value, out var dateOnly))
            return dateOnly;
        
        throw new JsonException($"Unable to convert \"{value}\" to type DateOnly.");
    }

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("yyyy-MM-dd"));
    }
}

public class JsonConverter_TimeOnly : JsonConverter<TimeOnly>
{
    public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (TimeOnly.TryParse(value, out var timeOnly))
            return timeOnly;
        
        throw new JsonException($"Unable to convert \"{value}\" to type TimeOnly.");
    }

    public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("HH:mm:ss"));
    }
}
