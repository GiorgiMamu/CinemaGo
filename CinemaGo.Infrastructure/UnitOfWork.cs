using CinemaGo.Domain.Constants;
using CinemaGo.Domain.Exceptions;
using CinemaGo.Domain.Interfaces.Repositories;
using CinemaGo.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
namespace CinemaGo.Infrastructure;

public class UnitOfWork : IUnitOfWork
{
    private readonly CinemaDbContext _context;
    public UnitOfWork(CinemaDbContext context) => _context = context;

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        try
        {
            return await _context.SaveChangesAsync(ct);
        }
        // 2601 / 2627 = unique index or constraint violated (e.g. two users booking the same seat)
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            throw new DuplicateKeyException(ex);
        }
        // 547 = foreign key violation (e.g. deleting a movie that still has sessions)
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 547 })
        {
            throw new ConflictException(Errors.General.HasDependencies, ex);
        }
    }
}