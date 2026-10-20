using CinemaGo.Domain.Entities;
namespace CinemaGo.Domain.Interfaces.Repositories;

public interface ISeatRepository : IGenericRepository<Seat>
{
    Task<List<Seat>> GetByHallAsync(int hallId, CancellationToken ct = default);
    Task<bool> PositionExistsAsync(int hallId, int row, int number, int? excludeId, CancellationToken ct = default);
}