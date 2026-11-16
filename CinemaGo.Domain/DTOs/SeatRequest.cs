using System.ComponentModel.DataAnnotations;
using CinemaGo.Domain.Constants;
namespace CinemaGo.Domain.DTOs;

public class SeatRequest
{
    [Range(1, int.MaxValue, ErrorMessage = ValidationMessages.RangeBetween)] public int HallId { get; set; }
    [Range(1, 100, ErrorMessage = ValidationMessages.RangeBetween)] public int RowNumber { get; set; }
    [Range(1, 100, ErrorMessage = ValidationMessages.RangeBetween)] public int SeatNumber { get; set; }
    public bool IsVip { get; set; }
}
