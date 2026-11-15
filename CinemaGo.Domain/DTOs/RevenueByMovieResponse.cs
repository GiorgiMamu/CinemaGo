namespace CinemaGo.Domain.DTOs;

public class RevenueByMovieResponse
{
    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public int TicketsSold { get; set; }
    public decimal TotalRevenue { get; set; }
}