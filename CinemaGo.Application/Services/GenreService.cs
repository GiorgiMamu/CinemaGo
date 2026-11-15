using AutoMapper;
using CinemaGo.Application.Interfaces.Services;
using CinemaGo.Domain.Constants;
using CinemaGo.Domain.DTOs;
using CinemaGo.Domain.Entities;
using CinemaGo.Domain.Exceptions;
using CinemaGo.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
namespace CinemaGo.Application.Services;

public class GenreService : IGenreService
{
    private readonly IGenreRepository _repo;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ILogger<GenreService> _logger;

    public GenreService(IGenreRepository repo, IUnitOfWork uow, IMapper mapper, ILogger<GenreService> logger)
    { _repo = repo; _uow = uow; _mapper = mapper; _logger = logger; }

    public async Task<List<GenreResponse>> GetAllAsync(CancellationToken ct = default)
        => _mapper.Map<List<GenreResponse>>((await _repo.GetAllAsync(ct)).OrderBy(g => g.Name));

    public async Task<GenreResponse> GetByIdAsync(int id, CancellationToken ct = default)
        => _mapper.Map<GenreResponse>(await _repo.GetByIdAsync(id, ct) ?? throw new NotFoundException(nameof(Genre), id));

    public async Task<GenreResponse> CreateAsync(GenreRequest r, CancellationToken ct = default)
    {
        if (await _repo.GetByNameAsync(r.Name, ct) is not null)
            throw new ConflictException(ErrorMessages.AlreadyExists("Genre", r.Name));
        var genre = _mapper.Map<Genre>(r);
        await _repo.AddAsync(genre, ct);
        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Genre created: {GenreId}", genre.Id);
        return _mapper.Map<GenreResponse>(genre);
    }

    public async Task<GenreResponse> UpdateAsync(int id, GenreRequest r, CancellationToken ct = default)
    {
        var genre = await _repo.GetByIdAsync(id, ct) ?? throw new NotFoundException(nameof(Genre), id);
        var same = await _repo.GetByNameAsync(r.Name, ct);
        if (same is not null && same.Id != id) throw new ConflictException(ErrorMessages.AlreadyExists("Genre", r.Name));
        _mapper.Map(r, genre);
        await _uow.SaveChangesAsync(ct);
        return _mapper.Map<GenreResponse>(genre);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var genre = await _repo.GetByIdAsync(id, ct) ?? throw new NotFoundException(nameof(Genre), id);
        _repo.Remove(genre);
        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Genre deleted: {GenreId}", id);
    }
}