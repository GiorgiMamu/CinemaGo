using System.Text.Json.Serialization;
namespace CinemaGo.Domain.DTOs;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    // only present on failures; omitted from the JSON when null
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IDictionary<string, string[]>? Errors { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Code { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TraceId { get; set; }

    public static ApiResponse<T> Ok(T data, string message)
        => new() { Success = true, Message = message, Data = data };

    public static ApiResponse<T> Fail(string message, string code, string traceId, IDictionary<string, string[]>? errors = null)
        => new() { Success = false, Message = message, Data = default, Code = code, TraceId = traceId, Errors = errors };
}