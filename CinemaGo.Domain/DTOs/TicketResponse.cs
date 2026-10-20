namespace CinemaGo.Domain.DTOs;

public class TicketResponse
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public string TicketCode { get; set; } = string.Empty;
    public decimal FinalPrice { get; set; }
}