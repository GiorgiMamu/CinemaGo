namespace CinemaGo.Domain.DTOs;

public class TicketSummaryResponse
{
    public int Id { get; set; }
    public string TicketCode { get; set; } = string.Empty;
    public decimal FinalPrice { get; set; }
    public DateTime IssuedAt { get; set; }
    public CinemaGo.Domain.Enums.TicketStatus Status { get; set; }
}