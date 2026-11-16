using CinemaGo.Domain.Enums;
namespace CinemaGo.Domain.DTOs;

public class BookingResponse
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string HallName { get; set; } = string.Empty;
    public DateTime StartsAt { get; set; }
    public int SeatId { get; set; }
    public int RowNumber { get; set; }
    public int SeatNumber { get; set; }
    public bool IsVip { get; set; }
    public BookingStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public TicketSummaryResponse? Ticket { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
}