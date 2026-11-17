using AutoMapper;
using CinemaGo.Application.Common;
using CinemaGo.Application.Interfaces.Services;
using CinemaGo.Domain.Constants;
using CinemaGo.Domain.DTOs;
using CinemaGo.Domain.Entities;
using CinemaGo.Domain.Exceptions;
using CinemaGo.Domain.Interfaces.External;
using CinemaGo.Domain.Interfaces.Repositories;
using FluentValidation;
using Microsoft.Extensions.Logging;
namespace CinemaGo.Application.Services;

public class MovieService : IMovieService
{
    private readonly IMovieRepository _movies;
    private readonly IGenreRepository _genres;
    private readonly IFileStorage _storage;
    private readonly IValidator<FileUpload> _fileValidator;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ILogger<MovieService> _logger;

    public MovieService(IMovieRepository movies, IGenreRepository genres, IFileStorage storage,
        IValidator<FileUpload> fileValidator, IUnitOfWork uow, IMapper mapper, ILogger<MovieService> logger)
    {
        _movies = movies; _genres = genres; _storage = storage; _fileValidator = fileValidator;
        _uow = uow; _mapper = mapper; _logger = logger;
    }

    public async Task<PagedResult<MovieResponse>> GetPagedAsync(MovieFilter filter, CancellationToken ct = default)
    {
        var (items, total) = await _movies.GetPagedAsync(filter, ct);
        return new PagedResult<MovieResponse>
        {
            Items = _mapper.Map<List<MovieResponse>>(items),
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalCount = total
        };
    }

    public async Task<MovieResponse> GetByIdAsync(int id, CancellationToken ct = default)
        => _mapper.Map<MovieResponse>(await _movies.GetWithGenresAsync(id, ct) ?? throw new NotFoundException(nameof(Movie), id));

    public async Task<MovieResponse> CreateAsync(MovieRequest r, CancellationToken ct = default)
    {
        var existingMovies = await _movies.GetAllAsync(ct);

        if (existingMovies.Any(m =>
            string.Equals(m.Title.Trim(), r.Title.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new ConflictException(Errors.General.AlreadyExists(nameof(Movie), r.Title));

        await EnsureGenresExistAsync(r.GenreIds, ct);

        var movie = _mapper.Map<Movie>(r);

        foreach (var genreId in r.GenreIds.Distinct())
            movie.MovieGenres.Add(new MovieGenre { GenreId = genreId });

        await _movies.AddAsync(movie, ct);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Movie created: {MovieId}", movie.Id);

        return await GetByIdAsync(movie.Id, ct);
    }

    public async Task<MovieResponse> UpdateAsync(int id, MovieRequest r, CancellationToken ct = default)
    {
        var movie = await _movies.GetWithGenresAsync(id, ct)
            ?? throw new NotFoundException(nameof(Movie), id);

        var existingMovies = await _movies.GetAllAsync(ct);

        if (existingMovies.Any(m =>
            m.Id != id &&
            string.Equals(m.Title.Trim(), r.Title.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new ConflictException(Errors.General.AlreadyExists(nameof(Movie), r.Title));

        await EnsureGenresExistAsync(r.GenreIds, ct);

        _mapper.Map(r, movie);

        var wanted = r.GenreIds.Distinct().ToHashSet();

        foreach (var link in movie.MovieGenres.Where(mg => !wanted.Contains(mg.GenreId)).ToList())
            movie.MovieGenres.Remove(link);

        var existing = movie.MovieGenres.Select(mg => mg.GenreId).ToHashSet();

        foreach (var genreId in wanted.Where(g => !existing.Contains(g)))
            movie.MovieGenres.Add(new MovieGenre { MovieId = id, GenreId = genreId });

        await _uow.SaveChangesAsync(ct);

        return await GetByIdAsync(id, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var movie = await _movies.GetByIdAsync(id, ct) ?? throw new NotFoundException(nameof(Movie), id);
        var poster = movie.PosterPath;
        _movies.Remove(movie);
        await _uow.SaveChangesAsync(ct);   // throws ConflictException if the movie still has sessions
        _storage.Delete(poster);           // only after the database delete succeeded
        _logger.LogInformation("Movie deleted: {MovieId}", id);
    }

    public async Task<MovieResponse> UploadPosterAsync(int id, FileUpload file, CancellationToken ct = default)
    {
        var movie = await _movies.GetWithGenresAsync(id, ct) ?? throw new NotFoundException(nameof(Movie), id);

        var result = await _fileValidator.ValidateAsync(file, ct);
        if (!result.IsValid) throw new ValidationFailedException(result.ToErrorDictionary());

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var newPath = await _storage.SaveAsync(file.Content, "movies", extension, ct);
        var oldPath = movie.PosterPath;
        movie.PosterPath = newPath;

        try { await _uow.SaveChangesAsync(ct); }
        catch { _storage.Delete(newPath); throw; }   // no orphan file if the database save fails

        _storage.Delete(oldPath);
        _logger.LogInformation("Poster uploaded for movie {MovieId}: {Path}", id, newPath);
        return _mapper.Map<MovieResponse>(movie);
    }

    public async Task<MovieResponse> DeletePosterAsync(int id, CancellationToken ct = default)
    {
        var movie = await _movies.GetWithGenresAsync(id, ct) ?? throw new NotFoundException(nameof(Movie), id);
        if (string.IsNullOrWhiteSpace(movie.PosterPath)) throw new NotFoundException("Poster of movie", id);
        var oldPath = movie.PosterPath;
        movie.PosterPath = null;
        await _uow.SaveChangesAsync(ct);
        _storage.Delete(oldPath);
        return _mapper.Map<MovieResponse>(movie);
    }

    private async Task EnsureGenresExistAsync(List<int> ids, CancellationToken ct)
    {
        var distinct = ids.Distinct().ToList();
        var found = await _genres.GetByIdsAsync(distinct, ct);
        if (found.Count != distinct.Count)
            throw new NotFoundException(nameof(Genre), string.Join(",", distinct.Except(found.Select(g => g.Id))));
    }
}