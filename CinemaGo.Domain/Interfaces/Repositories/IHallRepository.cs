using CinemaGo.Domain.Entities;
namespace CinemaGo.Domain.Interfaces.Repositories;

public interface IHallRepository : IGenericRepository<Hall>
{
    Task<Hall?> GetByNameAsync(string name, CancellationToken ct = default);
}