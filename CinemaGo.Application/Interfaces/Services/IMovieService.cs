using CinemaGo.Domain.DTOs;
namespace CinemaGo.Application.Interfaces.Services;

public interface IMovieService
{
    Task<PagedResult<MovieResponse>> GetPagedAsync(MovieFilter filter, CancellationToken ct = default);
    Task<MovieResponse> GetByIdAsync(int id, CancellationToken ct = default);
    Task<MovieResponse> CreateAsync(MovieRequest request, CancellationToken ct = default);
    Task<MovieResponse> UpdateAsync(int id, MovieRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<MovieResponse> UploadPosterAsync(int id, FileUpload file, CancellationToken ct = default);
    Task<MovieResponse> DeletePosterAsync(int id, CancellationToken ct = default);
}
