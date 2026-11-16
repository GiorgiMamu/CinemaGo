using System.ComponentModel.DataAnnotations;
using CinemaGo.Domain.Constants;
namespace CinemaGo.Domain.DTOs;

public class LoginRequest
{
    [Required(ErrorMessage = ValidationMessages.Required), EmailAddress(ErrorMessage = ValidationMessages.Email)]
    public string Email { get; set; } = string.Empty;
    [Required(ErrorMessage = ValidationMessages.Required)]
    public string Password { get; set; } = string.Empty;
}