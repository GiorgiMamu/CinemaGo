using CinemaGo.Domain.DTOs;
namespace CinemaGo.Application.Interfaces.Services;

public interface ITicketService
{
    Task<TicketResponse> GetByIdAsync(int id, CancellationToken ct = default);
    Task<TicketResponse> GetByBookingIdAsync(int bookingId, CancellationToken ct = default);
}