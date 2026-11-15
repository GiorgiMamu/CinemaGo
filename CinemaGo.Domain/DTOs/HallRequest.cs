using System.ComponentModel.DataAnnotations;
namespace CinemaGo.Domain.DTOs;

public class HallRequest
{
    [Required(ErrorMessage = "Hall name is required."), StringLength(100, ErrorMessage = "Hall name cannot exceed 100 characters.")]
    public string Name { get; set; } = string.Empty;
}