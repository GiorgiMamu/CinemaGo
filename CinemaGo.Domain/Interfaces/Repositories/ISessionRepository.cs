using CinemaGo.Domain.DTOs;
using CinemaGo.Domain.Entities;
namespace CinemaGo.Domain.Interfaces.Repositories;

public interface ISessionRepository : IGenericRepository<Session>
{
    Task<Session?> GetWithDetailsAsync(int id, CancellationToken ct = default);
    Task<(List<Session> Items, int Total)> GetPagedAsync(SessionFilter filter, CancellationToken ct = default);
    Task<List<Session>> GetByHallInWindowAsync(int hallId, DateTime from, DateTime to, int? excludeId, CancellationToken ct = default);
    Task<List<SessionOccupancyResponse>> GetOccupancyAsync(DateTime? from, DateTime? to, CancellationToken ct = default);
    Task<List<Session>> GetStartedScheduledAsync(DateTime now, CancellationToken ct = default);
}