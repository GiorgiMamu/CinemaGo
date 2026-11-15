using AutoMapper;
using CinemaGo.Application.Common;
using CinemaGo.Application.Interfaces;
using CinemaGo.Application.Interfaces.Services;
using CinemaGo.Domain.Constants;
using CinemaGo.Domain.DTOs;
using CinemaGo.Domain.Entities;
using CinemaGo.Domain.Enums;
using CinemaGo.Domain.Exceptions;
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

    public BookingService(IBookingRepository bookings, ISessionRepository sessions, ISeatRepository seats,
        IUnitOfWork uow, ICurrentUserService currentUser, IMapper mapper, ILogger<BookingService> logger)
    {
        _bookings = bookings; _sessions = sessions; _seats = seats; _uow = uow;
        _currentUser = currentUser; _mapper = mapper; _logger = logger;
    }

    // all seats in ONE SaveChanges = one database transaction: everything is booked or nothing is
    public async Task<List<BookingResponse>> CreateAsync(CreateBookingRequest r, CancellationToken ct = default)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException();

        var session = await _sessions.GetWithDetailsAsync(r.SessionId, ct) ?? throw new NotFoundException(nameof(Session), r.SessionId);
        if (session.Status != SessionStatus.Scheduled || session.StartsAt <= DateTime.UtcNow)
            throw new ConflictException(ErrorMessages.SessionNotBookable, ErrorCodes.SessionNotBookable);

        var seatIds = r.SeatIds.Distinct().ToList();
        var seats = await _seats.GetByIdsAsync(seatIds, ct);
        if (seats.Count != seatIds.Count)
            throw new NotFoundException(nameof(Seat), string.Join(",", seatIds.Except(seats.Select(s => s.Id))));

        // business rule: the seat must belong to the hall of the session
        if (seats.Any(s => s.HallId != session.HallId))
            throw new BadRequestException(ErrorMessages.SeatNotInHall, ErrorCodes.SeatNotInHall);

        // business rule: one active booking per seat per session
        var taken = await _bookings.GetActiveSeatIdsAsync(session.Id, ct);
        if (seatIds.Intersect(taken).Any())
            throw new ConflictException(ErrorMessages.SeatAlreadyBooked, ErrorCodes.SeatTaken);

        var created = new List<Booking>();
        foreach (var seat in seats)
        {
            var booking = new Booking
            {
                UserId = userId,
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
            throw new ConflictException(ErrorMessages.SeatAlreadyBooked, ErrorCodes.SeatTaken);
        }

        _logger.LogInformation("User {UserId} booked {Count} seat(s) on session {SessionId}", userId, created.Count, session.Id);
        return _mapper.Map<List<BookingResponse>>(created);
    }

    public async Task CancelAsync(int bookingId, CancellationToken ct = default)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException();
        var booking = await _bookings.GetWithDetailsAsync(bookingId, ct) ?? throw new NotFoundException(nameof(Booking), bookingId);

        if (booking.UserId != userId) throw new ForbiddenException();   // nobody cancels someone else's booking
        if (booking.Status == BookingStatus.Cancelled)
            throw new ConflictException(ErrorMessages.BookingAlreadyCancelled, ErrorCodes.BookingAlreadyCancelled);
        if (booking.Session.StartsAt <= DateTime.UtcNow)
            throw new ConflictException(ErrorMessages.BookingNotCancellable, ErrorCodes.BookingNotCancellable);

        booking.Cancel(DateTime.UtcNow);
        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Booking {BookingId} cancelled by user {UserId}", bookingId, userId);
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

    public async Task<BookingResponse> GetByIdAsync(int bookingId, CancellationToken ct = default)
    {
        var booking = await _bookings.GetWithDetailsAsync(bookingId, ct) ?? throw new NotFoundException(nameof(Booking), bookingId);
        if (booking.UserId != _currentUser.UserId && !_currentUser.IsStaff) throw new ForbiddenException();
        return _mapper.Map<BookingResponse>(booking);
    }
}