namespace CinemaGo.Domain.DTOs;

public class SessionRequest
{
    public int MovieId { get; set; }
    public int HallId { get; set; }
    public DateTime StartsAt { get; set; }
    public decimal StandardPrice { get; set; }
    public decimal VipPrice { get; set; }
}