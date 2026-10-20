using CinemaGo.Domain.Entities;
namespace CinemaGo.Domain.Interfaces.Repositories;

public interface IBookingRepository : IGenericRepository<Booking>
{
    Task<Booking?> GetWithDetailsAsync(int id, CancellationToken ct = default);
    Task<List<Booking>> GetByUserAsync(int userId, CancellationToken ct = default);
    Task<bool> HasActiveBookingAsync(int sessionId, int seatId, CancellationToken ct = default);
}