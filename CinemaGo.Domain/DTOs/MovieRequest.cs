using System.ComponentModel.DataAnnotations;
namespace CinemaGo.Domain.DTOs;

public class MovieRequest
{
    [Required(ErrorMessage = "Title is required."), StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
    public string? Description { get; set; }

    [Range(1, 600, ErrorMessage = "Duration must be between 1 and 600 minutes.")]
    public int DurationMinutes { get; set; }

    [Range(0, 21, ErrorMessage = "Age rating must be between 0 and 21.")]
    public int AgeRating { get; set; }

    [Required(ErrorMessage = "At least one genre is required."), MinLength(1, ErrorMessage = "Select at least one genre.")]
    public List<int> GenreIds { get; set; } = new();
}