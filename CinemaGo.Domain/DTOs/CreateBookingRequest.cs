using System.ComponentModel.DataAnnotations;
namespace CinemaGo.Domain.DTOs;

public class CreateBookingRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "A valid session id is required.")]
    public int SessionId { get; set; }

    [Required(ErrorMessage = "Select at least one seat."),
     MinLength(1, ErrorMessage = "Select at least one seat."),
     MaxLength(10, ErrorMessage = "You can book at most 10 seats at once.")]
    public List<int> SeatIds { get; set; } = new();
}