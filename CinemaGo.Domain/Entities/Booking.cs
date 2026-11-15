using CinemaGo.Domain.Common;
using CinemaGo.Domain.Enums;
namespace CinemaGo.Domain.Entities;

public class Booking : BaseEntity
{
    public string UserId { get; set; } = string.Empty;   // FK to Users (ApplicationUser lives in Infrastructure)
    public int SessionId { get; set; }
    public Session Session { get; set; } = null!;
    public int SeatId { get; set; }
    public Seat Seat { get; set; } = null!;
    public BookingStatus Status { get; set; } = BookingStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CancelledAt { get; set; }
    public Ticket? Ticket { get; set; }
    public void Cancel(DateTime now)
    {
        Status = BookingStatus.Cancelled;
        CancelledAt = now;
        if (Ticket is not null) Ticket.Status = TicketStatus.Cancelled;
    }
}