using CinemaGo.Domain.DTOs;
using CinemaGo.Domain.Entities;
using CinemaGo.Domain.Enums;
using CinemaGo.Domain.Interfaces.Repositories;
using CinemaGo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace CinemaGo.Infrastructure.Repositories;

public class BookingRepository : GenericRepository<Booking>, IBookingRepository
{
    public BookingRepository(CinemaDbContext c) : base(c) { }

    private IQueryable<Booking> WithDetails() => _set
        .Include(b => b.Session).ThenInclude(s => s.Movie)
        .Include(b => b.Session).ThenInclude(s => s.Hall)
        .Include(b => b.Seat)
        .Include(b => b.Ticket);

    public Task<Booking?> GetWithDetailsAsync(int id, CancellationToken ct = default)
        => WithDetails().FirstOrDefaultAsync(b => b.Id == id, ct);

    public async Task<(List<Booking> Items, int Total)> GetPagedByUserAsync(string userId, PageRequest page, CancellationToken ct = default)
    {
        var q = WithDetails().AsNoTracking().Where(b => b.UserId == userId);
        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(b => b.CreatedAt)
                           .Skip((page.Page - 1) * page.PageSize).Take(page.PageSize).ToListAsync(ct);
        return (items, total);
    }

    public Task<List<int>> GetActiveSeatIdsAsync(int sessionId, CancellationToken ct = default)
        => _set.AsNoTracking().Where(b => b.SessionId == sessionId && b.Status == BookingStatus.Active)
               .Select(b => b.SeatId).ToListAsync(ct);

    public Task<List<Booking>> GetActiveBySessionAsync(int sessionId, CancellationToken ct = default)
    => _set.Include(b => b.Ticket)
           .Where(b => b.SessionId == sessionId && b.Status == BookingStatus.Active).ToListAsync(ct);

    public Task<List<Booking>> GetActiveFutureByUserAsync(string userId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        return _set.Include(b => b.Ticket)
                   .Where(b => b.UserId == userId && b.Status == BookingStatus.Active && b.Session.StartsAt > now)
                   .ToListAsync(ct);
    }
    public async Task<(List<Booking> Items, int Total)> GetPagedAsync(BookingFilter f, CancellationToken ct = default)
    {
        var q = WithDetails().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(f.UserId)) q = q.Where(b => b.UserId == f.UserId);
        if (f.SessionId.HasValue) q = q.Where(b => b.SessionId == f.SessionId);
        if (f.MovieId.HasValue) q = q.Where(b => b.Session.MovieId == f.MovieId);
        if (f.Status.HasValue) q = q.Where(b => b.Status == f.Status);
        if (f.From.HasValue) q = q.Where(b => b.CreatedAt >= f.From);
        if (f.To.HasValue) q = q.Where(b => b.CreatedAt <= f.To);

        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(b => b.CreatedAt)
                           .Skip((f.Page - 1) * f.PageSize).Take(f.PageSize).ToListAsync(ct);
        return (items, total);
    }
}
