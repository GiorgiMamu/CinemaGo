using System.ComponentModel.DataAnnotations;
using CinemaGo.Domain.Constants;
namespace CinemaGo.Domain.DTOs;

public class ChangePasswordRequest
{
    [Required(ErrorMessage = ValidationMessages.Required)]
    public string CurrentPassword { get; set; } = string.Empty;
    [Required(ErrorMessage = ValidationMessages.Required), StringLength(100, MinimumLength = 8, ErrorMessage = ValidationMessages.LengthBetween)]
    public string NewPassword { get; set; } = string.Empty;
    [Required(ErrorMessage = ValidationMessages.Required), Compare(nameof(NewPassword), ErrorMessage = ValidationMessages.Compare)]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}