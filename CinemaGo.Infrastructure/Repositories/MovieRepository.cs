using CinemaGo.Domain.DTOs;
using CinemaGo.Domain.Entities;
using CinemaGo.Domain.Interfaces.Repositories;
using CinemaGo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace CinemaGo.Infrastructure.Repositories;

public class MovieRepository : GenericRepository<Movie>, IMovieRepository
{
    public MovieRepository(CinemaDbContext c) : base(c) { }

    public Task<Movie?> GetWithGenresAsync(int id, CancellationToken ct = default)
        => _set.Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre).FirstOrDefaultAsync(m => m.Id == id, ct);

    public async Task<(List<Movie> Items, int Total)> GetPagedAsync(MovieFilter f, CancellationToken ct = default)
    {
        var q = _set.AsNoTracking().Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre).AsQueryable();

        if (!string.IsNullOrWhiteSpace(f.Search)) q = q.Where(m => m.Title.Contains(f.Search));
        if (f.GenreId.HasValue) q = q.Where(m => m.MovieGenres.Any(mg => mg.GenreId == f.GenreId));
        if (f.MaxAgeRating.HasValue) q = q.Where(m => m.AgeRating <= f.MaxAgeRating);

        q = (f.SortBy?.ToLowerInvariant(), f.Descending) switch
        {
            ("duration", false) => q.OrderBy(m => m.DurationMinutes),
            ("duration", true) => q.OrderByDescending(m => m.DurationMinutes),
            ("rating", false) => q.OrderBy(m => m.AgeRating),
            ("rating", true) => q.OrderByDescending(m => m.AgeRating),
            (_, true) => q.OrderByDescending(m => m.Title),
            _ => q.OrderBy(m => m.Title)
        };

        var total = await q.CountAsync(ct);
        var items = await q.Skip((f.Page - 1) * f.PageSize).Take(f.PageSize).ToListAsync(ct);
        return (items, total);
    }
}