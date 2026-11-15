using System.ComponentModel.DataAnnotations;
namespace CinemaGo.Domain.DTOs;

public class ResetPasswordRequest
{
    [Required(ErrorMessage = "Email is required."), EmailAddress(ErrorMessage = "Email is not valid.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "The code is required."), RegularExpression(@"^\d{6}$", ErrorMessage = "The code must be exactly 6 digits.")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "New password is required."), StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be 8-100 characters long.")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm the new password."), Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}