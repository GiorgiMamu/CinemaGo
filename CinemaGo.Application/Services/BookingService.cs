using AutoMapper;
using CinemaGo.Application.Common;
using CinemaGo.Application.Interfaces;
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

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookings;
    private readonly ISessionRepository _sessions;
    private readonly ISeatRepository _seats;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly ILogger<BookingService> _logger;
    private readonly IIdentityService _identity;
    private readonly IEmailSender _email;

    public BookingService(IBookingRepository bookings, ISessionRepository sessions, ISeatRepository seats,
        IUnitOfWork uow, ICurrentUserService currentUser, IMapper mapper, ILogger<BookingService> logger,
        IIdentityService identity, IEmailSender email)
    {
        _bookings = bookings; _sessions = sessions; _seats = seats; _uow = uow;
        _currentUser = currentUser; _mapper = mapper; _logger = logger; _identity = identity; _email = email;
    }

    // all seats in ONE SaveChanges = one database transaction: everything is booked or nothing is
    public async Task<List<BookingResponse>> CreateAsync(CreateBookingRequest r, CancellationToken ct = default)
    {
        var actorId = _currentUser.UserId ?? throw new UnauthorizedException();
        var ownerId = await ResolveOwnerAsync(actorId, r.OnBehalfOfUserId, ct);

        var session = await _sessions.GetWithDetailsAsync(r.SessionId, ct) ?? throw new NotFoundException(nameof(Session), r.SessionId);
        if (session.Status != SessionStatus.Scheduled || session.StartsAt <= DateTime.UtcNow)
            throw new ConflictException(Errors.Session.NotBookable);

        var seatIds = r.SeatIds.Distinct().ToList();
        var seats = await _seats.GetByIdsAsync(seatIds, ct);
        if (seats.Count != seatIds.Count)
            throw new NotFoundException(nameof(Seat), string.Join(",", seatIds.Except(seats.Select(s => s.Id))));

        // business rule: the seat must belong to the hall of the session
        if (seats.Any(s => s.HallId != session.HallId))
            throw new BadRequestException(Errors.Booking.SeatNotInHall);

        // business rule: one active booking per seat per session
        var taken = await _bookings.GetActiveSeatIdsAsync(session.Id, ct);
        if (seatIds.Intersect(taken).Any())
            throw new ConflictException(Errors.Booking.SeatTaken);

        var created = new List<Booking>();
        foreach (var seat in seats)
        {
            var booking = new Booking
            {
                UserId = ownerId,
                Session = session,
                Seat = seat,
                Status = BookingStatus.Active,
                CreatedAt = DateTime.UtcNow,
                Ticket = new Ticket
                {
                    TicketCode = TicketCodeGenerator.Generate(),
                    FinalPrice = seat.IsVip ? session.VipPrice : session.StandardPrice,   // VIP / standard pricing
                    IssuedAt = DateTime.UtcNow
                }
            };
            await _bookings.AddAsync(booking, ct);
            created.Add(booking);
        }

        try
        {
            await _uow.SaveChangesAsync(ct);
        }
        catch (DuplicateKeyException)
        {
            // concurrent booking: someone else committed the same seat between our check and our insert
            _logger.LogWarning("Concurrent booking conflict on session {SessionId}", session.Id);
            throw new ConflictException(Errors.Booking.SeatTaken);
        }

        _logger.LogInformation("User {OwnerId} booked {Count} seat(s) on session {SessionId} (made by {ActorId})",
            ownerId, created.Count, session.Id, actorId);
        await NotifyBookingConfirmedAsync(ownerId, created, ct);
        return _mapper.Map<List<BookingResponse>>(created);
    }

    // owner or staff; the booking stays with CancelledAt and its ticket becomes Cancelled
    public async Task CancelAsync(int bookingId, CancellationToken ct = default)
    {
        var actorId = _currentUser.UserId ?? throw new UnauthorizedException();
        var booking = await _bookings.GetWithDetailsAsync(bookingId, ct) ?? throw new NotFoundException(nameof(Booking), bookingId);

        var isOwner = booking.UserId == actorId;
        if (!isOwner && !_currentUser.IsStaff) throw new ForbiddenException();   // a customer never cancels someone else's booking

        if (booking.Status == BookingStatus.Cancelled)
            throw new ConflictException(Errors.Booking.AlreadyCancelled);
        if (booking.Ticket?.Status == TicketStatus.Used)
            throw new ConflictException(Errors.Ticket.AlreadyUsed);
        if (booking.Session.StartsAt <= DateTime.UtcNow)
            throw new ConflictException(Errors.Booking.NotCancellable);

        booking.Cancel(DateTime.UtcNow);
        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Booking {BookingId} cancelled by {ActorId} ({By})", bookingId, actorId, isOwner ? "owner" : "staff");

        await NotifyBookingCancelledAsync(booking, byStaff: !isOwner, ct);
    }

    public async Task<PagedResult<BookingResponse>> GetMyAsync(PageRequest page, CancellationToken ct = default)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException();
        var (items, total) = await _bookings.GetPagedByUserAsync(userId, page, ct);
        return new PagedResult<BookingResponse>
        {
            Items = _mapper.Map<List<BookingResponse>>(items),
            Page = page.Page,
            PageSize = page.PageSize,
            TotalCount = total
        };
    }

    // admin view: every user's bookings, with the customer's name and e-mail
    public async Task<PagedResult<BookingResponse>> GetAllAsync(BookingFilter filter, CancellationToken ct = default)
    {
        var (items, total) = await _bookings.GetPagedAsync(filter, ct);
        var responses = _mapper.Map<List<BookingResponse>>(items);

        // one query for all users on the page (no N+1); deleted accounts are anonymized and stay null
        var users = (await _identity.GetByIdsAsync(responses.Select(r => r.UserId).Distinct(), ct)).ToDictionary(u => u.Id);
        foreach (var response in responses)
            if (users.TryGetValue(response.UserId, out var user))
            {
                response.UserName = user.UserName;
                response.UserEmail = user.Email;
            }

        return new PagedResult<BookingResponse> { Items = responses, Page = filter.Page, PageSize = filter.PageSize, TotalCount = total };
    }

    public async Task<BookingResponse> GetByIdAsync(int bookingId, CancellationToken ct = default)
    {
        var booking = await _bookings.GetWithDetailsAsync(bookingId, ct) ?? throw new NotFoundException(nameof(Booking), bookingId);
        if (booking.UserId != _currentUser.UserId && !_currentUser.IsStaff) throw new ForbiddenException();
        return _mapper.Map<BookingResponse>(booking);
    }

    // ---------- helpers ----------
    // who owns the new bookings: the caller, or (staff only) the customer named in the request
    private async Task<string> ResolveOwnerAsync(string actorId, string? onBehalfOf, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(onBehalfOf) || onBehalfOf == actorId) return actorId;
        if (!_currentUser.IsStaff) throw new ForbiddenException();

        var customer = await _identity.FindByIdAsync(onBehalfOf, ct) ?? throw new NotFoundException("User", onBehalfOf);
        if (!customer.IsActive) throw new ConflictException(Errors.Auth.AccountDisabled);
        return customer.Id;
    }

    // a failed e-mail must never undo the cancellation
    // e-mails only enqueue; a failure must never undo a booking or a cancellation
    private async Task NotifyBookingConfirmedAsync(string ownerId, List<Booking> created, CancellationToken ct)
    {
        try
        {
            var user = await _identity.FindByIdAsync(ownerId, ct);
            if (user is null) return;
            var (subject, body) = EmailTemplates.BookingConfirmed(user.UserName, created);
            await _email.SendAsync(user.Email, subject, body, ct);
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Could not queue the booking e-mail for user {UserId}", ownerId); }
    }

    private async Task NotifyBookingCancelledAsync(Booking booking, bool byStaff, CancellationToken ct)
    {
        try
        {
            var user = await _identity.FindByIdAsync(booking.UserId, ct);
            if (user is null) return;
            var (subject, body) = EmailTemplates.BookingCancelled(user.UserName, booking, byStaff);
            await _email.SendAsync(user.Email, subject, body, ct);
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Could not queue the cancellation e-mail for booking {BookingId}", booking.Id); }
    }
}