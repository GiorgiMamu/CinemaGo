using CinemaGo.Domain.Enums;
namespace CinemaGo.Domain.DTOs;

public class BookingResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int SessionId { get; set; }
    public int SeatId { get; set; }
    public BookingStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}