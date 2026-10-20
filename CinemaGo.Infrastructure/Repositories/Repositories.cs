using CinemaGo.Domain.Entities;
using CinemaGo.Domain.Enums;
using CinemaGo.Domain.Interfaces.Repositories;
using CinemaGo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace CinemaGo.Infrastructure.Repositories;

public class RoleRepository : GenericRepository<Role>, IRoleRepository
{
    public RoleRepository(CinemaDbContext c) : base(c) { }
    public Task<Role?> GetByNameAsync(string name, CancellationToken ct = default)
        => _set.FirstOrDefaultAsync(r => r.Name == name, ct);
}

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(CinemaDbContext c) : base(c) { }

    public Task<User?> GetWithRoleAsync(int id, CancellationToken ct = default)
        => _set.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<List<User>> GetAllWithRoleAsync(CancellationToken ct = default)
        => _set.AsNoTracking().Include(u => u.Role).OrderBy(u => u.UserName).ToListAsync(ct);

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => _set.Include(u => u.Role).FirstOrDefaultAsync(u => u.Email == email, ct);

    public Task<User?> GetByUserNameAsync(string userName, CancellationToken ct = default)
        => _set.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserName == userName, ct);
}

public class GenreRepository : GenericRepository<Genre>, IGenreRepository
{
    public GenreRepository(CinemaDbContext c) : base(c) { }

    public Task<Genre?> GetByNameAsync(string name, CancellationToken ct = default)
        => _set.FirstOrDefaultAsync(g => g.Name == name, ct);

    public Task<List<Genre>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default)
        => _set.Where(g => ids.Contains(g.Id)).ToListAsync(ct);
}

public class MovieRepository : GenericRepository<Movie>, IMovieRepository
{
    public MovieRepository(CinemaDbContext c) : base(c) { }

    public Task<Movie?> GetWithGenresAsync(int id, CancellationToken ct = default)
        => _set.Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
               .FirstOrDefaultAsync(m => m.Id == id, ct);

    public Task<List<Movie>> GetAllWithGenresAsync(CancellationToken ct = default)
        => _set.AsNoTracking().Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
               .OrderBy(m => m.Title).ToListAsync(ct);
}

public class HallRepository : GenericRepository<Hall>, IHallRepository
{
    public HallRepository(CinemaDbContext c) : base(c) { }
    public Task<Hall?> GetByNameAsync(string name, CancellationToken ct = default)
        => _set.FirstOrDefaultAsync(h => h.Name == name, ct);
}

public class SeatRepository : GenericRepository<Seat>, ISeatRepository
{
    public SeatRepository(CinemaDbContext c) : base(c) { }

    public Task<List<Seat>> GetByHallAsync(int hallId, CancellationToken ct = default)
        => _set.AsNoTracking().Where(s => s.HallId == hallId)
               .OrderBy(s => s.RowNumber).ThenBy(s => s.SeatNumber).ToListAsync(ct);

    public Task<bool> PositionExistsAsync(int hallId, int row, int number, int? excludeId, CancellationToken ct = default)
        => _set.AnyAsync(s => s.HallId == hallId && s.RowNumber == row && s.SeatNumber == number
                              && (excludeId == null || s.Id != excludeId), ct);
}

public class SessionRepository : GenericRepository<Session>, ISessionRepository
{
    public SessionRepository(CinemaDbContext c) : base(c) { }

    public Task<Session?> GetWithDetailsAsync(int id, CancellationToken ct = default)
        => _set.Include(s => s.Movie).Include(s => s.Hall).FirstOrDefaultAsync(s => s.Id == id, ct);

    public Task<List<Session>> GetAllWithDetailsAsync(CancellationToken ct = default)
        => _set.AsNoTracking().Include(s => s.Movie).Include(s => s.Hall)
               .OrderBy(s => s.StartsAt).ToListAsync(ct);
}

public class BookingRepository : GenericRepository<Booking>, IBookingRepository
{
    public BookingRepository(CinemaDbContext c) : base(c) { }

    public Task<Booking?> GetWithDetailsAsync(int id, CancellationToken ct = default)
        => _set.Include(b => b.Session).Include(b => b.Seat).Include(b => b.Ticket)
               .FirstOrDefaultAsync(b => b.Id == id, ct);

    public Task<List<Booking>> GetByUserAsync(int userId, CancellationToken ct = default)
        => _set.AsNoTracking().Where(b => b.UserId == userId)
               .OrderByDescending(b => b.CreatedAt).ToListAsync(ct);

    public Task<bool> HasActiveBookingAsync(int sessionId, int seatId, CancellationToken ct = default)
        => _set.AnyAsync(b => b.SessionId == sessionId && b.SeatId == seatId
                              && b.Status == BookingStatus.Active, ct);
}

public class TicketRepository : GenericRepository<Ticket>, ITicketRepository
{
    public TicketRepository(CinemaDbContext c) : base(c) { }

    public Task<Ticket?> GetByBookingIdAsync(int bookingId, CancellationToken ct = default)
        => _set.FirstOrDefaultAsync(t => t.BookingId == bookingId, ct);

    public Task<Ticket?> GetByCodeAsync(string code, CancellationToken ct = default)
        => _set.FirstOrDefaultAsync(t => t.TicketCode == code, ct);
}