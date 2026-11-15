using System.ComponentModel.DataAnnotations;
namespace CinemaGo.Domain.DTOs;

public class SeatRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "A valid hall id is required.")]
    public int HallId { get; set; }

    [Range(1, 100, ErrorMessage = "Row number must be between 1 and 100.")]
    public int RowNumber { get; set; }

    [Range(1, 100, ErrorMessage = "Seat number must be between 1 and 100.")]
    public int SeatNumber { get; set; }

    public bool IsVip { get; set; }
}