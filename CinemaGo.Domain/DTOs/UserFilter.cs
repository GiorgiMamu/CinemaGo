namespace CinemaGo.Domain.DTOs;

public class UserFilter : PageRequest
{
    public string? Search { get; set; }
    public string? Role { get; set; }
}