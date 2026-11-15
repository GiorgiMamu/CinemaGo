using CinemaGo.Domain.Entities;
using CinemaGo.Domain.Interfaces.Repositories;
using CinemaGo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace CinemaGo.Infrastructure.Repositories;

public class HallRepository : GenericRepository<Hall>, IHallRepository
{
    public HallRepository(CinemaDbContext c) : base(c) { }

    public Task<Hall?> GetByNameAsync(string name, CancellationToken ct = default)
        => _set.FirstOrDefaultAsync(h => h.Name == name, ct);
}