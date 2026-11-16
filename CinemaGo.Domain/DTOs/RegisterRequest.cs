using System.ComponentModel.DataAnnotations;
using CinemaGo.Domain.Constants;
namespace CinemaGo.Domain.DTOs;

public class RegisterRequest
{
    [Required(ErrorMessage = ValidationMessages.Required), StringLength(50, MinimumLength = 3, ErrorMessage = ValidationMessages.LengthBetween)]
    public string UserName { get; set; } = string.Empty;
    [Required(ErrorMessage = ValidationMessages.Required), EmailAddress(ErrorMessage = ValidationMessages.Email), StringLength(256, ErrorMessage = ValidationMessages.MaxLength)]
    public string Email { get; set; } = string.Empty;
    [Required(ErrorMessage = ValidationMessages.Required), StringLength(100, MinimumLength = 8, ErrorMessage = ValidationMessages.LengthBetween)]
    public string Password { get; set; } = string.Empty;
    [Required(ErrorMessage = ValidationMessages.Required), Compare(nameof(Password), ErrorMessage = ValidationMessages.Compare)]
    public string ConfirmPassword { get; set; } = string.Empty;
}