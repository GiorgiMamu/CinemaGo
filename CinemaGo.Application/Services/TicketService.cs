using AutoMapper;
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

public class TicketService : ITicketService
{
    private readonly ITicketRepository _tickets;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ILogger<TicketService> _logger;

    public TicketService(ITicketRepository tickets, ICurrentUserService currentUser, IUnitOfWork uow, IMapper mapper, ILogger<TicketService> logger)
    { _tickets = tickets; _currentUser = currentUser; _uow = uow; _mapper = mapper; _logger = logger; }

    public async Task<TicketResponse> GetByIdAsync(int id, CancellationToken ct = default)
        => Authorize(await _tickets.GetWithDetailsAsync(id, ct) ?? throw new NotFoundException(nameof(Ticket), id));

    public async Task<TicketResponse> GetByCodeAsync(string code, CancellationToken ct = default)
        => Authorize(await _tickets.GetByCodeWithDetailsAsync(code, ct) ?? throw new NotFoundException(nameof(Ticket), code));

    // staff at the cinema door: a ticket works once, from 60 min before the start until the session ends
    public async Task<TicketResponse> CheckInAsync(string code, CancellationToken ct = default)
    {
        var ticket = await _tickets.GetByCodeTrackedAsync(code, ct) ?? throw new NotFoundException(nameof(Ticket), code);

        if (ticket.Status == TicketStatus.Cancelled) throw new ConflictException(Errors.Ticket.Cancelled);
        if (ticket.Status == TicketStatus.Used) throw new ConflictException(Errors.Ticket.AlreadyUsed);

        var session = ticket.Booking.Session;
        var now = DateTime.UtcNow;
        var end = session.StartsAt.AddMinutes(session.Movie.DurationMinutes);
        if (now < session.StartsAt.AddMinutes(-60) || now > end)
            throw new ConflictException(Errors.Ticket.NotValidNow);

        ticket.Status = TicketStatus.Used;
        ticket.UsedAt = now;
        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Ticket {TicketCode} checked in by {StaffId}", code, _currentUser.UserId);
        return _mapper.Map<TicketResponse>(ticket);
    }

    private TicketResponse Authorize(Ticket ticket)
    {
        if (ticket.Booking.UserId != _currentUser.UserId && !_currentUser.IsStaff) throw new ForbiddenException();
        return _mapper.Map<TicketResponse>(ticket);
    }
}