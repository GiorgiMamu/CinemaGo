using CinemaGo.Domain.Entities;
namespace CinemaGo.Domain.Interfaces.Repositories;

public interface IMovieRepository : IGenericRepository<Movie>
{
    Task<Movie?> GetWithGenresAsync(int id, CancellationToken ct = default);
    Task<List<Movie>> GetAllWithGenresAsync(CancellationToken ct = default);
}