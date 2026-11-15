using System.ComponentModel.DataAnnotations;
namespace CinemaGo.Domain.DTOs;

public class GenreRequest
{
    [Required(ErrorMessage = "Genre name is required."), StringLength(100, ErrorMessage = "Genre name cannot exceed 100 characters.")]
    public string Name { get; set; } = string.Empty;
}