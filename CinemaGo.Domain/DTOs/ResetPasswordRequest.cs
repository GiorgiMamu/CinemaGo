using System.ComponentModel.DataAnnotations;
using CinemaGo.Domain.Constants;
namespace CinemaGo.Domain.DTOs;

public class ResetPasswordRequest
{
    [Required(ErrorMessage = ValidationMessages.Required), EmailAddress(ErrorMessage = ValidationMessages.Email)]
    public string Email { get; set; } = string.Empty;
    [Required(ErrorMessage = ValidationMessages.Required), RegularExpression(@"^\d{6}$", ErrorMessage = ValidationMessages.SixDigitCode)]
    public string Code { get; set; } = string.Empty;
    [Required(ErrorMessage = ValidationMessages.Required), StringLength(100, MinimumLength = 8, ErrorMessage = ValidationMessages.LengthBetween)]
    public string NewPassword { get; set; } = string.Empty;
    [Required(ErrorMessage = ValidationMessages.Required), Compare(nameof(NewPassword), ErrorMessage = ValidationMessages.Compare)]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}