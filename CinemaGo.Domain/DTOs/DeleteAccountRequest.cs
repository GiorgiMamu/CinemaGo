using System.ComponentModel.DataAnnotations;
using CinemaGo.Domain.Constants;
namespace CinemaGo.Domain.DTOs;

public class DeleteAccountRequest
{
    [Required(ErrorMessage = ValidationMessages.Required)]
    public string Password { get; set; } = string.Empty;
}
