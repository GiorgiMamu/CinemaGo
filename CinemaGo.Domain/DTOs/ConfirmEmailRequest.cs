using System.ComponentModel.DataAnnotations;
namespace CinemaGo.Domain.DTOs;

public class ConfirmEmailRequest
{
    [Required(ErrorMessage = "Email is required."), EmailAddress(ErrorMessage = "Email is not valid.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "The code is required."), RegularExpression(@"^\d{6}$", ErrorMessage = "The code must be exactly 6 digits.")]
    public string Code { get; set; } = string.Empty;
}