using System.ComponentModel.DataAnnotations;
using CinemaGo.Domain.Constants;
namespace CinemaGo.Domain.DTOs;

public class MovieRequest
{
    [Required(ErrorMessage = ValidationMessages.Required), StringLength(200, ErrorMessage = ValidationMessages.MaxLength)]
    public string Title { get; set; } = string.Empty;
    [StringLength(2000, ErrorMessage = ValidationMessages.MaxLength)]
    public string? Description { get; set; }
    [Range(1, 600, ErrorMessage = ValidationMessages.RangeBetween)] public int DurationMinutes { get; set; }
    [Range(0, 21, ErrorMessage = ValidationMessages.RangeBetween)] public int AgeRating { get; set; }
    [Required(ErrorMessage = ValidationMessages.Required), MinLength(1, ErrorMessage = ValidationMessages.MinItems)]
    public List<int> GenreIds { get; set; } = new();
}