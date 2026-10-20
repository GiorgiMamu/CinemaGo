using CinemaGo.Domain.Common;
using CinemaGo.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
namespace CinemaGo.Domain.Entities;

public class Booking : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int SessionId { get; set; }
    public Session Session { get; set; } = null!;
    public int SeatId { get; set; }
    public Seat Seat { get; set; } = null!;
    public BookingStatus Status { get; set; } = BookingStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Ticket? Ticket { get; set; }
}