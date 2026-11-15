using CinemaGo.Domain.Enums;
namespace CinemaGo.Domain.DTOs;

public class TicketResponse
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public string TicketCode { get; set; } = string.Empty;
    public decimal FinalPrice { get; set; }
    public DateTime IssuedAt { get; set; }
    public BookingStatus BookingStatus { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string HallName { get; set; } = string.Empty;
    public DateTime StartsAt { get; set; }
    public int RowNumber { get; set; }
    public int SeatNumber { get; set; }
    public bool IsVip { get; set; }
    public CinemaGo.Domain.Enums.TicketStatus Status { get; set; }
    public DateTime? UsedAt { get; set; }
}