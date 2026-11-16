using System.ComponentModel.DataAnnotations;
using CinemaGo.Domain.Constants;
namespace CinemaGo.Domain.DTOs;

public class ChangeRoleRequest
{
    [Required(ErrorMessage = ValidationMessages.Required)]
    public string Role { get; set; } = string.Empty;
}