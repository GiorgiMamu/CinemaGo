using System.ComponentModel.DataAnnotations;
using CinemaGo.Domain.Constants;
namespace CinemaGo.Domain.DTOs;

public class ConfirmEmailRequest
{
    [Required(ErrorMessage = ValidationMessages.Required), EmailAddress(ErrorMessage = ValidationMessages.Email)]
    public string Email { get; set; } = string.Empty;
    [Required(ErrorMessage = ValidationMessages.Required), RegularExpression(@"^\d{6}$", ErrorMessage = ValidationMessages.SixDigitCode)]
    public string Code { get; set; } = string.Empty;
}