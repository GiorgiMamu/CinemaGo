using CinemaGo.Domain.DTOs;
namespace CinemaGo.Application.Interfaces.Services;

public interface IBookingService
{
    Task<List<BookingResponse>> CreateAsync(CreateBookingRequest request, CancellationToken ct = default);
    Task CancelAsync(int bookingId, CancellationToken ct = default);
    Task<PagedResult<BookingResponse>> GetMyAsync(PageRequest page, CancellationToken ct = default);
    Task<BookingResponse> GetByIdAsync(int bookingId, CancellationToken ct = default);
}