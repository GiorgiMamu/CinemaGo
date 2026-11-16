using System.ComponentModel.DataAnnotations;
using CinemaGo.Domain.Constants;
namespace CinemaGo.Domain.DTOs;

public class CreateBookingRequest
{
    [Range(1, int.MaxValue, ErrorMessage = ValidationMessages.RangeBetween)] public int SessionId { get; set; }
    [Required(ErrorMessage = ValidationMessages.Required),
     MinLength(1, ErrorMessage = ValidationMessages.MinItems),
     MaxLength(10, ErrorMessage = ValidationMessages.MaxItems)]
    public List<int> SeatIds { get; set; } = new();
    [StringLength(450, ErrorMessage = ValidationMessages.MaxLength)]
    public string? OnBehalfOfUserId { get; set; }
}