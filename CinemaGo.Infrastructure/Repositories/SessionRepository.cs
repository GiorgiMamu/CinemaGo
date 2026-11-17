using CinemaGo.Domain.DTOs;
using CinemaGo.Domain.Entities;
using CinemaGo.Domain.Enums;
using CinemaGo.Domain.Interfaces.Repositories;
using CinemaGo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace CinemaGo.Infrastructure.Repositories;

public class SessionRepository : GenericRepository<Session>, ISessionRepository
{
    public SessionRepository(CinemaDbContext c) : base(c) { }

    public Task<Session?> GetWithDetailsAsync(int id, CancellationToken ct = default)
        => _set.Include(s => s.Movie).Include(s => s.Hall).FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<(List<Session> Items, int Total)> GetPagedAsync(SessionFilter f, CancellationToken ct = default)
    {
        var q = _set.AsNoTracking().Include(s => s.Movie).Include(s => s.Hall).AsQueryable();

        if (f.MovieId.HasValue) q = q.Where(s => s.MovieId == f.MovieId);
        if (f.HallId.HasValue) q = q.Where(s => s.HallId == f.HallId);
        if (f.From.HasValue) q = q.Where(s => s.StartsAt >= f.From);
        if (f.To.HasValue) q = q.Where(s => s.StartsAt <= f.To);
        if (f.OnlyUpcoming) { var now = DateTime.UtcNow; q = q.Where(s => s.StartsAt > now); }
        if (!f.IncludeCancelled) q = q.Where(s => s.Status != SessionStatus.Cancelled);

        q = q.OrderBy(s => s.StartsAt);
        var total = await q.CountAsync(ct);
        var items = await q.Skip((f.Page - 1) * f.PageSize).Take(f.PageSize).ToListAsync(ct);
        return (items, total);
    }

    public Task<List<Session>> GetByHallInWindowAsync(int hallId, DateTime from, DateTime to, int? excludeId, CancellationToken ct = default)
        => _set.AsNoTracking().Include(s => s.Movie)
               .Where(s => s.HallId == hallId && s.Status != SessionStatus.Cancelled
                           && s.StartsAt >= from && s.StartsAt <= to
                           && (excludeId == null || s.Id != excludeId))
               .ToListAsync(ct);

    // complex LINQ: projection with Count over two navigation collections
    public Task<List<SessionOccupancyResponse>> GetOccupancyAsync(DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        var q = _set.AsNoTracking().Where(s => s.Status != SessionStatus.Cancelled);
        if (from.HasValue) q = q.Where(s => s.StartsAt >= from);
        if (to.HasValue) q = q.Where(s => s.StartsAt <= to);

        return q.OrderBy(s => s.StartsAt)
                .Select(s => new SessionOccupancyResponse
                {
                    SessionId = s.Id,
                    MovieTitle = s.Movie.Title,
                    HallName = s.Hall.Name,
                    StartsAt = s.StartsAt,
                    TotalSeats = s.Hall.Seats.Count(),
                    BookedSeats = s.Bookings.Count(b => b.Status == BookingStatus.Active)
                }).ToListAsync(ct);
    }
    public Task<List<Session>> GetStartedScheduledAsync(DateTime now, CancellationToken ct = default)
    => _set.Include(s => s.Movie)
           .Where(s => s.Status == SessionStatus.Scheduled && s.StartsAt <= now).ToListAsync(ct);
   

    
}