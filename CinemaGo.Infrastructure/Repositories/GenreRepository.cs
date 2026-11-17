using CinemaGo.Domain.Entities;
using CinemaGo.Domain.Interfaces.Repositories;
using CinemaGo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace CinemaGo.Infrastructure.Repositories;

public class GenreRepository : GenericRepository<Genre>, IGenreRepository
{
    public GenreRepository(CinemaDbContext c) : base(c) { }

    public Task<Genre?> GetByNameAsync(string name, CancellationToken ct = default)
        => _set.FirstOrDefaultAsync(g => g.Name == name, ct);

    public Task<List<Genre>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default)
        => _set.Where(g => ids.Contains(g.Id)).ToListAsync(ct);
    public Task<bool> IsUsedAsync(int genreId, CancellationToken ct = default)
    => _context.Set<MovieGenre>().AnyAsync(mg => mg.GenreId == genreId, ct);
}