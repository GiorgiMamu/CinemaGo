using CinemaGo.Domain.Enums;
namespace CinemaGo.Domain.DTOs;

public class SessionResponse
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string? PosterUrl { get; set; }
    public int HallId { get; set; }
    public string HallName { get; set; } = string.Empty;
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public decimal StandardPrice { get; set; }
    public decimal VipPrice { get; set; }
    public SessionStatus Status { get; set; }
    public DateTime? CancelledAt { get; set; }
}
