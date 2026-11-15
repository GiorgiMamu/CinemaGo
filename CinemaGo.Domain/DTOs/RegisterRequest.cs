using System.ComponentModel.DataAnnotations;
namespace CinemaGo.Domain.DTOs;

public class RegisterRequest
{
    [Required(ErrorMessage = "User name is required."), StringLength(50, MinimumLength = 3, ErrorMessage = "User name must be 3-50 characters long.")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required."), EmailAddress(ErrorMessage = "Email is not valid."), StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required."), StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be 8-100 characters long.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm the password."), Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}