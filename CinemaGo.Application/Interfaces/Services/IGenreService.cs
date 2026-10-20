using CinemaGo.Domain.DTOs;
namespace CinemaGo.Application.Interfaces.Services;

public interface IGenreService
{
    Task<List<GenreResponse>> GetAllAsync(CancellationToken ct = default);
    Task<GenreResponse> GetByIdAsync(int id, CancellationToken ct = default);
    Task<GenreResponse> CreateAsync(GenreRequest request, CancellationToken ct = default);
    Task<GenreResponse> UpdateAsync(int id, GenreRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}