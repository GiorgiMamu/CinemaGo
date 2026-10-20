using CinemaGo.Domain.Interfaces.Repositories;
namespace CinemaGo.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly CinemaDbContext _context;
    public UnitOfWork(CinemaDbContext context) => _context = context;
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);
}