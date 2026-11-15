namespace CinemaGo.Domain.DTOs;

public class ApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public static ApiResponse Ok(string message) => new() { Success = true, Message = message };
}