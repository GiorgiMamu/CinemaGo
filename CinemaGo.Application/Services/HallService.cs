using AutoMapper;
using CinemaGo.Application.Interfaces.Services;
using CinemaGo.Domain.Constants;
using CinemaGo.Domain.DTOs;
using CinemaGo.Domain.Entities;
using CinemaGo.Domain.Exceptions;
using CinemaGo.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
namespace CinemaGo.Application.Services;

public class HallService : IHallService
{
    private readonly IHallRepository _repo;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ILogger<HallService> _logger;

    public HallService(IHallRepository repo, IUnitOfWork uow, IMapper mapper, ILogger<HallService> logger)
    { _repo = repo; _uow = uow; _mapper = mapper; _logger = logger; }

    public async Task<List<HallResponse>> GetAllAsync(CancellationToken ct = default)
        => _mapper.Map<List<HallResponse>>((await _repo.GetAllAsync(ct)).OrderBy(h => h.Name));

    public async Task<HallResponse> GetByIdAsync(int id, CancellationToken ct = default)
        => _mapper.Map<HallResponse>(await _repo.GetByIdAsync(id, ct) ?? throw new NotFoundException(nameof(Hall), id));

    public async Task<HallResponse> CreateAsync(HallRequest r, CancellationToken ct = default)
    {
        if (await _repo.GetByNameAsync(r.Name, ct) is not null)
            throw new ConflictException(ErrorMessages.AlreadyExists("Hall", r.Name));
        var hall = _mapper.Map<Hall>(r);
        await _repo.AddAsync(hall, ct);
        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Hall created: {HallId}", hall.Id);
        return _mapper.Map<HallResponse>(hall);
    }

    public async Task<HallResponse> UpdateAsync(int id, HallRequest r, CancellationToken ct = default)
    {
        var hall = await _repo.GetByIdAsync(id, ct) ?? throw new NotFoundException(nameof(Hall), id);
        var same = await _repo.GetByNameAsync(r.Name, ct);
        if (same is not null && same.Id != id) throw new ConflictException(ErrorMessages.AlreadyExists("Hall", r.Name));
        _mapper.Map(r, hall);
        await _uow.SaveChangesAsync(ct);
        return _mapper.Map<HallResponse>(hall);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var hall = await _repo.GetByIdAsync(id, ct) ?? throw new NotFoundException(nameof(Hall), id);
        _repo.Remove(hall);
        await _uow.SaveChangesAsync(ct);   // ConflictException if sessions or bookings reference it
        _logger.LogInformation("Hall deleted: {HallId}", id);
    }
}