using AutoMapper;
using CinemaGo.Application.Common;
using CinemaGo.Application.Interfaces.Services;
using CinemaGo.Domain.Constants;
using CinemaGo.Domain.DTOs;
using CinemaGo.Domain.Entities;
using CinemaGo.Domain.Enums;
using CinemaGo.Domain.Exceptions;
using CinemaGo.Domain.Interfaces.External;
using CinemaGo.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
namespace CinemaGo.Application.Services;

public class SessionService : ISessionService
{
    private readonly ISessionRepository _sessions;
    private readonly IMovieRepository _movies;
    private readonly IHallRepository _halls;
    private readonly ISeatRepository _seats;
    private readonly IBookingRepository _bookings;
    private readonly IUnitOfWork _uow;
    private readonly IIdentityService _identity;
    private readonly IEmailSender _email;
    private readonly IMapper _mapper;
    private readonly ILogger<SessionService> _logger;

    public SessionService(ISessionRepository sessions, IMovieRepository movies, IHallRepository halls, ISeatRepository seats,
        IBookingRepository bookings, IUnitOfWork uow, IIdentityService identity, IEmailSender email,
        IMapper mapper, ILogger<SessionService> logger)
    {
        _sessions = sessions; _movies = movies; _halls = halls; _seats = seats; _bookings = bookings;
        _uow = uow; _identity = identity; _email = email; _mapper = mapper; _logger = logger;
    }

    public async Task<PagedResult<SessionResponse>> GetPagedAsync(SessionFilter filter, CancellationToken ct = default)
    {
        var (items, total) = await _sessions.GetPagedAsync(filter, ct);
        return new PagedResult<SessionResponse>
        {
            Items = _mapper.Map<List<SessionResponse>>(items),
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalCount = total
        };
    }

    public async Task<SessionResponse> GetByIdAsync(int id, CancellationToken ct = default)
        => _mapper.Map<SessionResponse>(await _sessions.GetWithDetailsAsync(id, ct) ?? throw new NotFoundException(nameof(Session), id));

    // free / occupied seats of one session, with the price of each seat
    public async Task<List<SeatAvailabilityResponse>> GetSeatsAsync(int sessionId, CancellationToken ct = default)
    {
        var session = await _sessions.GetWithDetailsAsync(sessionId, ct) ?? throw new NotFoundException(nameof(Session), sessionId);
        var seats = await _seats.GetByHallAsync(session.HallId, ct);
        var booked = (await _bookings.GetActiveSeatIdsAsync(sessionId, ct)).ToHashSet();

        return seats.Select(s => new SeatAvailabilityResponse
        {
            SeatId = s.Id,
            RowNumber = s.RowNumber,
            SeatNumber = s.SeatNumber,
            IsVip = s.IsVip,
            Price = s.IsVip ? session.VipPrice : session.StandardPrice,
            IsBooked = booked.Contains(s.Id)
        }).ToList();
    }

    public async Task<SessionResponse> CreateAsync(SessionRequest r, CancellationToken ct = default)
    {
        var movie = await _movies.GetByIdAsync(r.MovieId, ct) ?? throw new NotFoundException(nameof(Movie), r.MovieId);
        if (!await _halls.ExistsAsync(r.HallId, ct)) throw new NotFoundException(nameof(Hall), r.HallId);
        await EnsureNoOverlapAsync(r.HallId, r.StartsAt, movie.DurationMinutes, null, ct);

        var session = _mapper.Map<Session>(r);
        session.Status = SessionStatus.Scheduled;
        await _sessions.AddAsync(session, ct);
        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Session created: {SessionId} (hall {HallId}, {StartsAt:u})", session.Id, session.HallId, session.StartsAt);
        return await GetByIdAsync(session.Id, ct);
    }

    public async Task<SessionResponse> UpdateAsync(int id, SessionRequest r, CancellationToken ct = default)
    {
        var session = await _sessions.GetByIdAsync(id, ct) ?? throw new NotFoundException(nameof(Session), id);
        if (session.Status == SessionStatus.Cancelled) throw new ConflictException(Errors.Session.CancelledNoEdit);

        var movie = await _movies.GetByIdAsync(r.MovieId, ct) ?? throw new NotFoundException(nameof(Movie), r.MovieId);
        if (!await _halls.ExistsAsync(r.HallId, ct)) throw new NotFoundException(nameof(Hall), r.HallId);

        // seats belong to a hall, so the hall cannot change once someone has booked
        if (r.HallId != session.HallId && (await _bookings.GetActiveSeatIdsAsync(id, ct)).Count > 0)
            throw new ConflictException(Errors.Session.HallLocked);

        await EnsureNoOverlapAsync(r.HallId, r.StartsAt, movie.DurationMinutes, id, ct);

        var oldStart = session.StartsAt;
        var oldMovieId = session.MovieId;
        _mapper.Map(r, session);
        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Session updated: {SessionId}", id);
        if (session.StartsAt != oldStart || session.MovieId != oldMovieId) await NotifyRescheduledAsync(id, oldStart, ct);
        return await GetByIdAsync(id, ct);
    }

    // one SaveChanges = session + all bookings change atomically
    // DELETE /api/sessions/{id}: the row stays (Status + CancelledAt), bookings and tickets are cancelled in the same SaveChanges
    public async Task CancelAsync(int id, CancellationToken ct = default)
    {
        var session = await _sessions.GetWithDetailsAsync(id, ct) ?? throw new NotFoundException(nameof(Session), id);
        if (session.Status == SessionStatus.Cancelled) throw new ConflictException(Errors.Session.AlreadyCancelled);

        var now = DateTime.UtcNow;
        session.Status = SessionStatus.Cancelled;
        session.CancelledAt = now;
        var bookings = await _bookings.GetActiveBySessionAsync(id, ct);   // includes tickets
        foreach (var booking in bookings) booking.Cancel(now);
        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Session {SessionId} cancelled with {Count} booking(s)", id, bookings.Count);

        // notifications run after the commit and must never undo the cancellation
        var users = (await _identity.GetByIdsAsync(bookings.Select(b => b.UserId).Distinct(), ct)).ToDictionary(u => u.Id);
        foreach (var group in bookings.GroupBy(b => b.UserId))
        {
            if (!users.TryGetValue(group.Key, out var user)) continue;
            try
            {
                var (subject, body) = EmailTemplates.SessionCancelled(user.UserName, session.Movie.Title, session.StartsAt, group.Count());
                await _email.SendAsync(user.Email, subject, body, ct);
            }
            catch (Exception ex) { _logger.LogWarning(ex, "Could not send cancellation e-mail to user {UserId}", user.Id); }
        }
    }

    // called every 5 minutes by a background job: Scheduled -> Finished once the movie has ended
    public async Task<int> FinishPastSessionsAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var candidates = await _sessions.GetStartedScheduledAsync(now, ct);
        var finished = candidates.Where(s => s.StartsAt.AddMinutes(s.Movie.DurationMinutes) <= now).ToList();
        foreach (var session in finished) session.Status = SessionStatus.Finished;
        if (finished.Count > 0) await _uow.SaveChangesAsync(ct);
        return finished.Count;
    }

    private async Task EnsureNoOverlapAsync(int hallId, DateTime start, int durationMinutes, int? excludeId, CancellationToken ct)
    {
        var end = SessionTimeRules.GetEnd(start, durationMinutes);
        // movies are shorter than a day, so only sessions starting within +-1 day can collide
        var neighbours = await _sessions.GetByHallInWindowAsync(hallId, start.AddDays(-1), end.AddDays(1), excludeId, ct);

        var clash = neighbours.Any(o => SessionTimeRules.Overlaps(
            start, end, o.StartsAt, SessionTimeRules.GetEnd(o.StartsAt, o.Movie.DurationMinutes)));
        if (clash) throw new ConflictException(Errors.Session.Overlap);
    }
    private async Task NotifyRescheduledAsync(int sessionId, DateTime oldStart, CancellationToken ct)
    {
        var session = await _sessions.GetWithDetailsAsync(sessionId, ct);
        var bookings = await _bookings.GetActiveBySessionAsync(sessionId, ct);
        if (session is null || bookings.Count == 0) return;

        var users = (await _identity.GetByIdsAsync(bookings.Select(x => x.UserId).Distinct(), ct)).ToDictionary(u => u.Id);
        foreach (var group in bookings.GroupBy(x => x.UserId))
        {
            if (!users.TryGetValue(group.Key, out var user)) continue;
            try
            {
                var (subject, body) = EmailTemplates.SessionRescheduled(user.UserName, session.Movie.Title, oldStart, session.StartsAt, group.Count());
                await _email.SendAsync(user.Email, subject, body, ct);
            }
            catch (Exception ex) { _logger.LogWarning(ex, "Could not queue the reschedule e-mail for user {UserId}", user.Id); }
        }
    }

}