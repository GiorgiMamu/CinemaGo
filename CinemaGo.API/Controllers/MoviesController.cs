using CinemaGo.Application.Interfaces.Services;
using CinemaGo.Domain.Constants;
using CinemaGo.Domain.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace CinemaGo.API.Controllers;

public class MoviesController : BaseApiController
{
    private readonly IMovieService _service;
    public MoviesController(IMovieService service) => _service = service;

    /// <summary>Search and filter movies (title, genre, age rating) with sorting and paging.</summary>
    [HttpGet, AllowAnonymous]
    public async Task<ActionResult<ApiResponse<PagedResult<MovieResponse>>>> GetAll([FromQuery] MovieFilter filter, CancellationToken ct)
        => Success(await _service.GetPagedAsync(filter, ct), SuccessMessages.Retrieved);

    /// <summary>Get one movie with its genres and poster URL.</summary>
    [HttpGet("{id:int}"), AllowAnonymous]
    public async Task<ActionResult<ApiResponse<MovieResponse>>> Get(int id, CancellationToken ct)
        => Success(await _service.GetByIdAsync(id, ct), SuccessMessages.Retrieved);

    /// <summary>Create a movie (Admin).</summary>
    [HttpPost, Authorize(Roles = RoleNames.AdminOrAbove)]
    public async Task<ActionResult<ApiResponse<MovieResponse>>> Create([FromBody] MovieRequest request, CancellationToken ct)
        => SuccessCreated(await _service.CreateAsync(request, ct), SuccessMessages.Created("Movie"));

    /// <summary>Update a movie and its genres (Admin).</summary>
    [HttpPut("{id:int}"), Authorize(Roles = RoleNames.AdminOrAbove)]
    public async Task<ActionResult<ApiResponse<MovieResponse>>> Update(int id, [FromBody] MovieRequest request, CancellationToken ct)
        => Success(await _service.UpdateAsync(id, request, ct), SuccessMessages.Updated("Movie"));

    /// <summary>Delete a movie (Admin). Fails with 409 if the movie has sessions.</summary>
    [HttpDelete("{id:int}"), Authorize(Roles = RoleNames.AdminOrAbove)]
    public async Task<ActionResult<ApiResponse>> Delete(int id, CancellationToken ct)
    { await _service.DeleteAsync(id, ct); return Success(SuccessMessages.Deleted("Movie")); }

    /// <summary>Upload or replace the poster (JPG, PNG or WEBP, max 5 MB). The old file is removed.</summary>
    [HttpPost("{id:int}/poster"), Authorize(Roles = RoleNames.AdminOrAbove)]
    [Consumes("multipart/form-data"), RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<ActionResult<ApiResponse<MovieResponse>>> UploadPoster(int id, IFormFile file, CancellationToken ct)
    {
        await using var stream = file.OpenReadStream();
        var upload = new FileUpload { Content = stream, FileName = file.FileName, ContentType = file.ContentType, Length = file.Length };
        return Success(await _service.UploadPosterAsync(id, upload, ct), SuccessMessages.PosterUploaded);
    }

    /// <summary>Remove the poster (404 if the movie has none).</summary>
    [HttpDelete("{id:int}/poster"), Authorize(Roles = RoleNames.AdminOrAbove)]
    public async Task<ActionResult<ApiResponse<MovieResponse>>> DeletePoster(int id, CancellationToken ct)
        => Success(await _service.DeletePosterAsync(id, ct), SuccessMessages.PosterDeleted);
}