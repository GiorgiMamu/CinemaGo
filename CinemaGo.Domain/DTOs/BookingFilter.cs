using CinemaGo.Domain.Enums;
namespace CinemaGo.Domain.DTOs;

public class BookingFilter : PageRequest
{
    public string? UserId { get; set; }
    public int? SessionId { get; set; }
    public int? MovieId { get; set; }
    public BookingStatus? Status { get; set; }   // Active or Cancelled
    public DateTime? From { get; set; }          // booked on or after
    public DateTime? To { get; set; }            // booked on or before
}