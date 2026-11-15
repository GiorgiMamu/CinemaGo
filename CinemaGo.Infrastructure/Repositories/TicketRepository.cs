using CinemaGo.Domain.DTOs;
using CinemaGo.Domain.Entities;
using CinemaGo.Domain.Enums;
using CinemaGo.Domain.Interfaces.Repositories;
using CinemaGo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace CinemaGo.Infrastructure.Repositories;

public class TicketRepository : GenericRepository<Ticket>, ITicketRepository
{
    public TicketRepository(CinemaDbContext c) : base(c) { }

    private IQueryable<Ticket> WithDetails() => _set.AsNoTracking()
        .Include(t => t.Booking).ThenInclude(b => b.Session).ThenInclude(s => s.Movie)
        .Include(t => t.Booking).ThenInclude(b => b.Session).ThenInclude(s => s.Hall)
        .Include(t => t.Booking).ThenInclude(b => b.Seat);

    public Task<Ticket?> GetWithDetailsAsync(int id, CancellationToken ct = default)
        => WithDetails().FirstOrDefaultAsync(t => t.Id == id, ct);

    public Task<Ticket?> GetByCodeWithDetailsAsync(string code, CancellationToken ct = default)
        => WithDetails().FirstOrDefaultAsync(t => t.TicketCode == code, ct);

    // complex LINQ: GroupBy + Count + Sum (revenue per movie, active bookings only)
    public Task<List<RevenueByMovieResponse>> GetRevenueByMovieAsync(DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        var q = _set.AsNoTracking().Where(t => t.Status != TicketStatus.Cancelled);
        if (from.HasValue) q = q.Where(t => t.IssuedAt >= from);
        if (to.HasValue) q = q.Where(t => t.IssuedAt <= to);

        return q.GroupBy(t => new { t.Booking.Session.MovieId, t.Booking.Session.Movie.Title })
                .Select(g => new RevenueByMovieResponse
                {
                    MovieId = g.Key.MovieId,
                    MovieTitle = g.Key.Title,
                    TicketsSold = g.Count(),
                    TotalRevenue = g.Sum(t => t.FinalPrice)
                })
                .OrderByDescending(r => r.TotalRevenue)
                .ToListAsync(ct);
    }

    // tracked: CheckIn modifies it
    public Task<Ticket?> GetByCodeTrackedAsync(string code, CancellationToken ct = default)
        => _set.Include(t => t.Booking).ThenInclude(b => b.Session).ThenInclude(s => s.Movie)
               .Include(t => t.Booking).ThenInclude(b => b.Session).ThenInclude(s => s.Hall)
               .Include(t => t.Booking).ThenInclude(b => b.Seat)
               .FirstOrDefaultAsync(t => t.TicketCode == code, ct);
}