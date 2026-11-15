using System.ComponentModel.DataAnnotations;
namespace CinemaGo.Domain.DTOs;

public class EmailRequest
{
    [Required(ErrorMessage = "Email is required."), EmailAddress(ErrorMessage = "Email is not valid.")]
    public string Email { get; set; } = string.Empty;
}