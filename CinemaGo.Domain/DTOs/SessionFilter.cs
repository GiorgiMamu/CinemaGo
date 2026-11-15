namespace CinemaGo.Domain.DTOs;

public class SessionFilter : PageRequest
{
    public int? MovieId { get; set; }
    public int? HallId { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public bool OnlyUpcoming { get; set; }
    public bool IncludeCancelled { get; set; }
}