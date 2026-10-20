using System;
using System.Collections.Generic;
using System.Text;
using CinemaGo.Domain.Common;
namespace CinemaGo.Domain.Entities;

public class Ticket : BaseEntity
{
    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;
    public string TicketCode { get; set; } = string.Empty;
    public decimal FinalPrice { get; set; }
}