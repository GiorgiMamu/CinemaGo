using CinemaGo.Domain.Entities;
namespace CinemaGo.Domain.Interfaces.Repositories;

public interface IGenreRepository : IGenericRepository<Genre>
{
    Task<Genre?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<List<Genre>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
    Task<bool> IsUsedAsync(int genreId, CancellationToken ct = default);
}