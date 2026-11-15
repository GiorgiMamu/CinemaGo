using System.ComponentModel.DataAnnotations;
namespace CinemaGo.Domain.DTOs;

public class DeleteAccountRequest
{
    [Required(ErrorMessage = "Password is required to delete your account.")]
    public string Password { get; set; } = string.Empty;
}