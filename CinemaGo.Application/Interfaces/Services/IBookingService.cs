using CinemaGo.Domain.DTOs;
namespace CinemaGo.Application.Interfaces.Services;

public interface IBookingService
{
    Task<BookingResponse> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<BookingResponse>> GetByUserAsync(int userId, CancellationToken ct = default);
}