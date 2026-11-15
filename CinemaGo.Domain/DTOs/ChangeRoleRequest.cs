using System.ComponentModel.DataAnnotations;
namespace CinemaGo.Domain.DTOs;

public class ChangeRoleRequest
{
    [Required(ErrorMessage = "Role is required.")]
    public string Role { get; set; } = string.Empty;
}