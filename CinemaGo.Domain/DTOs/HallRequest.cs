using System.ComponentModel.DataAnnotations;
using CinemaGo.Domain.Constants;
namespace CinemaGo.Domain.DTOs;

public class HallRequest
{
    [Required(ErrorMessage = ValidationMessages.Required), StringLength(100, ErrorMessage = ValidationMessages.MaxLength)]
    public string Name { get; set; } = string.Empty;
}
