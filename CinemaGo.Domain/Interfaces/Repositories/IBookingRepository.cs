using CinemaGo.Domain.DTOs;
using CinemaGo.Domain.Entities;
namespace CinemaGo.Domain.Interfaces.Repositories;

public interface IBookingRepository : IGenericRepository<Booking>
{
    Task<Booking?> GetWithDetailsAsync(int id, CancellationToken ct = default);
    Task<(List<Booking> Items, int Total)> GetPagedByUserAsync(string userId, PageRequest page, CancellationToken ct = default);
    Task<List<int>> GetActiveSeatIdsAsync(int sessionId, CancellationToken ct = default);
    Task<List<Booking>> GetActiveBySessionAsync(int sessionId, CancellationToken ct = default);
    Task<List<Booking>> GetActiveFutureByUserAsync(string userId, CancellationToken ct = default);

}
