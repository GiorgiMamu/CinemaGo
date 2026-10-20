using CinemaGo.Application.Exceptions;
using CinemaGo.Application.Interfaces.Services;
using CinemaGo.Application.Mappings;
using CinemaGo.Domain.DTOs;
using CinemaGo.Domain.Entities;
using CinemaGo.Domain.Enums;
using CinemaGo.Domain.Interfaces.Repositories;
namespace CinemaGo.Application.Services;

public class GenreService : IGenreService
{
    private readonly IGenreRepository _repo;
    private readonly IUnitOfWork _uow;
    public GenreService(IGenreRepository repo, IUnitOfWork uow) { _repo = repo; _uow = uow; }

    public async Task<List<GenreResponse>> GetAllAsync(CancellationToken ct = default)
        => (await _repo.GetAllAsync(ct)).Select(g => g.ToResponse()).ToList();

    public async Task<GenreResponse> GetByIdAsync(int id, CancellationToken ct = default)
        => (await _repo.GetByIdAsync(id, ct) ?? throw new NotFoundException(nameof(Genre), id)).ToResponse();

    public async Task<GenreResponse> CreateAsync(GenreRequest r, CancellationToken ct = default)
    {
        if (await _repo.GetByNameAsync(r.Name, ct) is not null)
            throw new ConflictException($"Genre '{r.Name}' already exists.");
        var genre = new Genre { Name = r.Name };
        await _repo.AddAsync(genre, ct);
        await _uow.SaveChangesAsync(ct);
        return genre.ToResponse();
    }

    public async Task<GenreResponse> UpdateAsync(int id, GenreRequest r, CancellationToken ct = default)
    {
        var genre = await _repo.GetByIdAsync(id, ct) ?? throw new NotFoundException(nameof(Genre), id);
        var same = await _repo.GetByNameAsync(r.Name, ct);
        if (same is not null && same.Id != id) throw new ConflictException($"Genre '{r.Name}' already exists.");
        genre.Name = r.Name;
        await _uow.SaveChangesAsync(ct);
        return genre.ToResponse();
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var genre = await _repo.GetByIdAsync(id, ct) ?? throw new NotFoundException(nameof(Genre), id);
        _repo.Remove(genre);
        await _uow.SaveChangesAsync(ct);
    }
}

public class MovieService : IMovieService
{
    private readonly IMovieRepository _movies;
    private readonly IGenreRepository _genres;
    private readonly IUnitOfWork _uow;
    public MovieService(IMovieRepository movies, IGenreRepository genres, IUnitOfWork uow)
    { _movies = movies; _genres = genres; _uow = uow; }

    public async Task<List<MovieResponse>> GetAllAsync(CancellationToken ct = default)
        => (await _movies.GetAllWithGenresAsync(ct)).Select(m => m.ToResponse()).ToList();

    public async Task<MovieResponse> GetByIdAsync(int id, CancellationToken ct = default)
        => (await _movies.GetWithGenresAsync(id, ct) ?? throw new NotFoundException(nameof(Movie), id)).ToResponse();

    public async Task<MovieResponse> CreateAsync(MovieRequest r, CancellationToken ct = default)
    {
        await EnsureGenresExistAsync(r.GenreIds, ct);
        var movie = new Movie { Title = r.Title, DurationMinutes = r.DurationMinutes, AgeRating = r.AgeRating };
        foreach (var gid in r.GenreIds.Distinct())
            movie.MovieGenres.Add(new MovieGenre { GenreId = gid });
        await _movies.AddAsync(movie, ct);
        await _uow.SaveChangesAsync(ct);
        return await GetByIdAsync(movie.Id, ct);   // reload so Genre.Name is populated
    }

    public async Task<MovieResponse> UpdateAsync(int id, MovieRequest r, CancellationToken ct = default)
    {
        var movie = await _movies.GetWithGenresAsync(id, ct) ?? throw new NotFoundException(nameof(Movie), id);
        await EnsureGenresExistAsync(r.GenreIds, ct);

        movie.Title = r.Title;
        movie.DurationMinutes = r.DurationMinutes;
        movie.AgeRating = r.AgeRating;

        // diff the join rows instead of Clear() + re-add
        var wanted = r.GenreIds.Distinct().ToHashSet();
        var toRemove = movie.MovieGenres.Where(mg => !wanted.Contains(mg.GenreId)).ToList();
        foreach (var mg in toRemove) movie.MovieGenres.Remove(mg);
        var existing = movie.MovieGenres.Select(mg => mg.GenreId).ToHashSet();
        foreach (var gid in wanted.Where(g => !existing.Contains(g)))
            movie.MovieGenres.Add(new MovieGenre { MovieId = id, GenreId = gid });

        await _uow.SaveChangesAsync(ct);
        return await GetByIdAsync(id, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var movie = await _movies.GetByIdAsync(id, ct) ?? throw new NotFoundException(nameof(Movie), id);
        _movies.Remove(movie);   // if the movie has sessions, Restrict makes the database refuse
        await _uow.SaveChangesAsync(ct);
    }

    private async Task EnsureGenresExistAsync(List<int> ids, CancellationToken ct)
    {
        var distinct = ids.Distinct().ToList();
        var found = await _genres.GetByIdsAsync(distinct, ct);
        if (found.Count != distinct.Count)
            throw new NotFoundException(nameof(Genre), string.Join(",", distinct.Except(found.Select(g => g.Id))));
    }
}

public class HallService : IHallService
{
    private readonly IHallRepository _repo;
    private readonly IUnitOfWork _uow;
    public HallService(IHallRepository repo, IUnitOfWork uow) { _repo = repo; _uow = uow; }

    public async Task<List<HallResponse>> GetAllAsync(CancellationToken ct = default)
        => (await _repo.GetAllAsync(ct)).Select(h => h.ToResponse()).ToList();

    public async Task<HallResponse> GetByIdAsync(int id, CancellationToken ct = default)
        => (await _repo.GetByIdAsync(id, ct) ?? throw new NotFoundException(nameof(Hall), id)).ToResponse();

    public async Task<HallResponse> CreateAsync(HallRequest r, CancellationToken ct = default)
    {
        if (await _repo.GetByNameAsync(r.Name, ct) is not null)
            throw new ConflictException($"Hall '{r.Name}' already exists.");
        var hall = new Hall { Name = r.Name };
        await _repo.AddAsync(hall, ct);
        await _uow.SaveChangesAsync(ct);
        return hall.ToResponse();
    }

    public async Task<HallResponse> UpdateAsync(int id, HallRequest r, CancellationToken ct = default)
    {
        var hall = await _repo.GetByIdAsync(id, ct) ?? throw new NotFoundException(nameof(Hall), id);
        var same = await _repo.GetByNameAsync(r.Name, ct);
        if (same is not null && same.Id != id) throw new ConflictException($"Hall '{r.Name}' already exists.");
        hall.Name = r.Name;
        await _uow.SaveChangesAsync(ct);
        return hall.ToResponse();
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var hall = await _repo.GetByIdAsync(id, ct) ?? throw new NotFoundException(nameof(Hall), id);
        _repo.Remove(hall);
        await _uow.SaveChangesAsync(ct);
    }
}

public class SeatService : ISeatService
{
    private readonly ISeatRepository _seats;
    private readonly IHallRepository _halls;
    private readonly IUnitOfWork _uow;
    public SeatService(ISeatRepository seats, IHallRepository halls, IUnitOfWork uow)
    { _seats = seats; _halls = halls; _uow = uow; }

    public async Task<List<SeatResponse>> GetByHallAsync(int hallId, CancellationToken ct = default)
    {
        if (!await _halls.ExistsAsync(hallId, ct)) throw new NotFoundException(nameof(Hall), hallId);
        return (await _seats.GetByHallAsync(hallId, ct)).Select(s => s.ToResponse()).ToList();
    }

    public async Task<SeatResponse> GetByIdAsync(int id, CancellationToken ct = default)
        => (await _seats.GetByIdAsync(id, ct) ?? throw new NotFoundException(nameof(Seat), id)).ToResponse();

    public async Task<SeatResponse> CreateAsync(SeatRequest r, CancellationToken ct = default)
    {
        if (!await _halls.ExistsAsync(r.HallId, ct)) throw new NotFoundException(nameof(Hall), r.HallId);
        if (await _seats.PositionExistsAsync(r.HallId, r.RowNumber, r.SeatNumber, null, ct))
            throw new ConflictException("Seat with this row/number already exists in the hall.");
        var seat = new Seat { HallId = r.HallId, RowNumber = r.RowNumber, SeatNumber = r.SeatNumber, IsVip = r.IsVip };
        await _seats.AddAsync(seat, ct);
        await _uow.SaveChangesAsync(ct);
        return seat.ToResponse();
    }

    public async Task<SeatResponse> UpdateAsync(int id, SeatRequest r, CancellationToken ct = default)
    {
        var seat = await _seats.GetByIdAsync(id, ct) ?? throw new NotFoundException(nameof(Seat), id);
        if (!await _halls.ExistsAsync(r.HallId, ct)) throw new NotFoundException(nameof(Hall), r.HallId);
        if (await _seats.PositionExistsAsync(r.HallId, r.RowNumber, r.SeatNumber, id, ct))
            throw new ConflictException("Seat with this row/number already exists in the hall.");
        seat.HallId = r.HallId; seat.RowNumber = r.RowNumber; seat.SeatNumber = r.SeatNumber; seat.IsVip = r.IsVip;
        await _uow.SaveChangesAsync(ct);
        return seat.ToResponse();
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var seat = await _seats.GetByIdAsync(id, ct) ?? throw new NotFoundException(nameof(Seat), id);
        _seats.Remove(seat);
        await _uow.SaveChangesAsync(ct);
    }
}

public class SessionService : ISessionService
{
    private readonly ISessionRepository _sessions;
    private readonly IMovieRepository _movies;
    private readonly IHallRepository _halls;
    private readonly IUnitOfWork _uow;
    public SessionService(ISessionRepository sessions, IMovieRepository movies, IHallRepository halls, IUnitOfWork uow)
    { _sessions = sessions; _movies = movies; _halls = halls; _uow = uow; }

    public async Task<List<SessionResponse>> GetAllAsync(CancellationToken ct = default)
        => (await _sessions.GetAllWithDetailsAsync(ct)).Select(s => s.ToResponse()).ToList();

    public async Task<SessionResponse> GetByIdAsync(int id, CancellationToken ct = default)
        => (await _sessions.GetWithDetailsAsync(id, ct) ?? throw new NotFoundException(nameof(Session), id)).ToResponse();

    public async Task<SessionResponse> CreateAsync(SessionRequest r, CancellationToken ct = default)
    {
        await EnsureReferencesAsync(r, ct);
        var session = new Session
        {
            MovieId = r.MovieId,
            HallId = r.HallId,
            StartsAt = r.StartsAt,
            StandardPrice = r.StandardPrice,
            VipPrice = r.VipPrice,
            Status = SessionStatus.Scheduled
        };
        await _sessions.AddAsync(session, ct);
        await _uow.SaveChangesAsync(ct);
        return await GetByIdAsync(session.Id, ct);
        // TODO (next stage): reject overlapping sessions in the same hall
    }

    public async Task<SessionResponse> UpdateAsync(int id, SessionRequest r, CancellationToken ct = default)
    {
        var session = await _sessions.GetByIdAsync(id, ct) ?? throw new NotFoundException(nameof(Session), id);
        await EnsureReferencesAsync(r, ct);
        session.MovieId = r.MovieId; session.HallId = r.HallId; session.StartsAt = r.StartsAt;
        session.StandardPrice = r.StandardPrice; session.VipPrice = r.VipPrice;
        await _uow.SaveChangesAsync(ct);
        return await GetByIdAsync(id, ct);
    }

    public async Task CancelAsync(int id, CancellationToken ct = default)
    {
        var session = await _sessions.GetByIdAsync(id, ct) ?? throw new NotFoundException(nameof(Session), id);
        if (session.Status == SessionStatus.Cancelled) throw new ConflictException("Session is already cancelled.");
        session.Status = SessionStatus.Cancelled;
        await _uow.SaveChangesAsync(ct);
        // TODO (extended): bulk-cancel bookings + simulated email
    }

    private async Task EnsureReferencesAsync(SessionRequest r, CancellationToken ct)
    {
        if (!await _movies.ExistsAsync(r.MovieId, ct)) throw new NotFoundException(nameof(Movie), r.MovieId);
        if (!await _halls.ExistsAsync(r.HallId, ct)) throw new NotFoundException(nameof(Hall), r.HallId);
    }
}

public class RoleService : IRoleService
{
    private readonly IRoleRepository _repo;
    public RoleService(IRoleRepository repo) => _repo = repo;

    public async Task<List<RoleResponse>> GetAllAsync(CancellationToken ct = default)
        => (await _repo.GetAllAsync(ct)).Select(r => r.ToResponse()).ToList();

    public async Task<RoleResponse> GetByIdAsync(int id, CancellationToken ct = default)
        => (await _repo.GetByIdAsync(id, ct) ?? throw new NotFoundException(nameof(Role), id)).ToResponse();
}

public class UserService : IUserService
{
    private readonly IUserRepository _repo;
    public UserService(IUserRepository repo) => _repo = repo;

    public async Task<List<UserResponse>> GetAllAsync(CancellationToken ct = default)
        => (await _repo.GetAllWithRoleAsync(ct)).Select(u => u.ToResponse()).ToList();

    public async Task<UserResponse> GetByIdAsync(int id, CancellationToken ct = default)
        => (await _repo.GetWithRoleAsync(id, ct) ?? throw new NotFoundException(nameof(User), id)).ToResponse();
}

public class BookingService : IBookingService
{
    private readonly IBookingRepository _repo;
    public BookingService(IBookingRepository repo) => _repo = repo;

    public async Task<BookingResponse> GetByIdAsync(int id, CancellationToken ct = default)
        => (await _repo.GetWithDetailsAsync(id, ct) ?? throw new NotFoundException(nameof(Booking), id)).ToResponse();

    public async Task<List<BookingResponse>> GetByUserAsync(int userId, CancellationToken ct = default)
        => (await _repo.GetByUserAsync(userId, ct)).Select(b => b.ToResponse()).ToList();
}

public class TicketService : ITicketService
{
    private readonly ITicketRepository _repo;
    public TicketService(ITicketRepository repo) => _repo = repo;

    public async Task<TicketResponse> GetByIdAsync(int id, CancellationToken ct = default)
        => (await _repo.GetByIdAsync(id, ct) ?? throw new NotFoundException(nameof(Ticket), id)).ToResponse();

    public async Task<TicketResponse> GetByBookingIdAsync(int bookingId, CancellationToken ct = default)
        => (await _repo.GetByBookingIdAsync(bookingId, ct) ?? throw new NotFoundException(nameof(Ticket), bookingId)).ToResponse();
}