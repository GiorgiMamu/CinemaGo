using AutoMapper;
using CinemaGo.Application.Interfaces.Services;
using CinemaGo.Domain.Constants;
using CinemaGo.Domain.DTOs;
using CinemaGo.Domain.Entities;
using CinemaGo.Domain.Exceptions;
using CinemaGo.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
namespace CinemaGo.Application.Services;

public class SeatService : ISeatService
{
    private readonly ISeatRepository _seats;
    private readonly IHallRepository _halls;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ILogger<SeatService> _logger;

    public SeatService(ISeatRepository seats, IHallRepository halls, IUnitOfWork uow, IMapper mapper, ILogger<SeatService> logger)
    { _seats = seats; _halls = halls; _uow = uow; _mapper = mapper; _logger = logger; }

    public async Task<List<SeatResponse>> GetByHallAsync(int hallId, CancellationToken ct = default)
    {
        if (!await _halls.ExistsAsync(hallId, ct)) throw new NotFoundException(nameof(Hall), hallId);
        return _mapper.Map<List<SeatResponse>>(await _seats.GetByHallAsync(hallId, ct));
    }

    public async Task<SeatResponse> GetByIdAsync(int id, CancellationToken ct = default)
        => _mapper.Map<SeatResponse>(await _seats.GetByIdAsync(id, ct) ?? throw new NotFoundException(nameof(Seat), id));

    public async Task<SeatResponse> CreateAsync(SeatRequest r, CancellationToken ct = default)
    {
        if (!await _halls.ExistsAsync(r.HallId, ct)) throw new NotFoundException(nameof(Hall), r.HallId);
        if (await _seats.PositionExistsAsync(r.HallId, r.RowNumber, r.SeatNumber, null, ct))
            throw new ConflictException(ErrorMessages.SeatPositionTaken);
        var seat = _mapper.Map<Seat>(r);
        await _seats.AddAsync(seat, ct);
        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Seat created: {SeatId}", seat.Id);
        return _mapper.Map<SeatResponse>(seat);
    }

    public async Task<SeatResponse> UpdateAsync(int id, SeatRequest r, CancellationToken ct = default)
    {
        var seat = await _seats.GetByIdAsync(id, ct) ?? throw new NotFoundException(nameof(Seat), id);
        if (!await _halls.ExistsAsync(r.HallId, ct)) throw new NotFoundException(nameof(Hall), r.HallId);
        if (await _seats.PositionExistsAsync(r.HallId, r.RowNumber, r.SeatNumber, id, ct))
            throw new ConflictException(ErrorMessages.SeatPositionTaken);
        _mapper.Map(r, seat);
        await _uow.SaveChangesAsync(ct);
        return _mapper.Map<SeatResponse>(seat);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var seat = await _seats.GetByIdAsync(id, ct) ?? throw new NotFoundException(nameof(Seat), id);
        _seats.Remove(seat);
        await _uow.SaveChangesAsync(ct);   // ConflictException if the seat has bookings
    }
}