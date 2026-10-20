using CinemaGo.Domain.Entities;
namespace CinemaGo.Domain.Interfaces.Repositories;

public interface ISessionRepository : IGenericRepository<Session>
{
    Task<Session?> GetWithDetailsAsync(int id, CancellationToken ct = default);
    Task<List<Session>> GetAllWithDetailsAsync(CancellationToken ct = default);
}