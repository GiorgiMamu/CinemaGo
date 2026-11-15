using System.ComponentModel.DataAnnotations;
namespace CinemaGo.Domain.DTOs;

public class SessionRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "A valid movie id is required.")]
    public int MovieId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "A valid hall id is required.")]
    public int HallId { get; set; }

    [Required(ErrorMessage = "Start time is required.")]
    public DateTime StartsAt { get; set; }

    [Range(typeof(decimal), "0", "100000", ErrorMessage = "Standard price must be between 0 and 100000.")]
    public decimal StandardPrice { get; set; }

    [Range(typeof(decimal), "0", "100000", ErrorMessage = "VIP price must be between 0 and 100000.")]
    public decimal VipPrice { get; set; }
}