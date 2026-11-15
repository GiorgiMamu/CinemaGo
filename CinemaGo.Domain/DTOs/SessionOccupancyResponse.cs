namespace CinemaGo.Domain.DTOs;

public class SessionOccupancyResponse
{
    public int SessionId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string HallName { get; set; } = string.Empty;
    public DateTime StartsAt { get; set; }
    public int TotalSeats { get; set; }
    public int BookedSeats { get; set; }
    public double OccupancyPercent => TotalSeats == 0 ? 0 : Math.Round(BookedSeats * 100.0 / TotalSeats, 1);
}