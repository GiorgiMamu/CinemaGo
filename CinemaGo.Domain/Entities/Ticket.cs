using CinemaGo.Domain.Common;
using CinemaGo.Domain.Enums;
namespace CinemaGo.Domain.Entities;

public class Ticket : BaseEntity
{
    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;
    public string TicketCode { get; set; } = string.Empty;
    public decimal FinalPrice { get; set; }
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    public TicketStatus Status { get; set; } = TicketStatus.Valid;
    public DateTime? UsedAt { get; set; }
}