namespace CinemaGo.Domain.DTOs;

public class SeatAvailabilityResponse
{
    public int SeatId { get; set; }
    public int RowNumber { get; set; }
    public int SeatNumber { get; set; }
    public bool IsVip { get; set; }
    public decimal Price { get; set; }
    public bool IsBooked { get; set; }
}