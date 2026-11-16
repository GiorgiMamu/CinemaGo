using System.ComponentModel.DataAnnotations;
using CinemaGo.Domain.Constants;
namespace CinemaGo.Domain.DTOs;

public class SessionRequest
{
    [Range(1, int.MaxValue, ErrorMessage = ValidationMessages.RangeBetween)] public int MovieId { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = ValidationMessages.RangeBetween)] public int HallId { get; set; }
    [Required(ErrorMessage = ValidationMessages.Required)] public DateTime StartsAt { get; set; }
    [Range(typeof(decimal), "0", "100000", ErrorMessage = ValidationMessages.RangeBetween)] public decimal StandardPrice { get; set; }
    [Range(typeof(decimal), "0", "100000", ErrorMessage = ValidationMessages.RangeBetween)] public decimal VipPrice { get; set; }
}