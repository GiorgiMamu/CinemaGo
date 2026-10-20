namespace CinemaGo.Domain.DTOs;

public class SeatRequest
{
    public int HallId { get; set; }
    public int RowNumber { get; set; }
    public int SeatNumber { get; set; }
    public bool IsVip { get; set; }
}