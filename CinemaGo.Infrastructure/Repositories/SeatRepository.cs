using CinemaGo.Domain.Entities;
using CinemaGo.Domain.Interfaces.Repositories;
using CinemaGo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace CinemaGo.Infrastructure.Repositories;

public class SeatRepository : GenericRepository<Seat>, ISeatRepository
{
    public SeatRepository(CinemaDbContext c) : base(c) { }

    public Task<List<Seat>> GetByHallAsync(int hallId, CancellationToken ct = default)
        => _set.AsNoTracking().Where(s => s.HallId == hallId)
               .OrderBy(s => s.RowNumber).ThenBy(s => s.SeatNumber).ToListAsync(ct);

    // tracked on purpose: BookingService attaches bookings to these seats
    public Task<List<Seat>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default)
        => _set.Where(s => ids.Contains(s.Id)).ToListAsync(ct);

    public Task<bool> PositionExistsAsync(int hallId, int row, int number, int? excludeId, CancellationToken ct = default)
        => _set.AnyAsync(s => s.HallId == hallId && s.RowNumber == row && s.SeatNumber == number
                              && (excludeId == null || s.Id != excludeId), ct);
}