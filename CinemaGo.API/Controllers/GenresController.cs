using CinemaGo.Application.Interfaces.Services;
using CinemaGo.Domain.Constants;
using CinemaGo.Domain.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace CinemaGo.API.Controllers;

public class GenresController : BaseApiController
{
    private readonly IGenreService _service;
    public GenresController(IGenreService service) => _service = service;

    /// <summary>List all genres.</summary>
    [HttpGet, AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<GenreResponse>>>> GetAll(CancellationToken ct)
        => Success(await _service.GetAllAsync(ct), SuccessMessages.Retrieved);

    /// <summary>Get one genre.</summary>
    [HttpGet("{id:int}"), AllowAnonymous]
    public async Task<ActionResult<ApiResponse<GenreResponse>>> Get(int id, CancellationToken ct)
        => Success(await _service.GetByIdAsync(id, ct), SuccessMessages.Retrieved);

    /// <summary>Create a genre (Admin).</summary>
    [HttpPost, Authorize(Roles = RoleNames.AdminOrAbove)]
    public async Task<ActionResult<ApiResponse<GenreResponse>>> Create([FromBody] GenreRequest request, CancellationToken ct)
        => SuccessCreated(await _service.CreateAsync(request, ct), SuccessMessages.Created("Genre"));

    /// <summary>Rename a genre (Admin).</summary>
    [HttpPut("{id:int}"), Authorize(Roles = RoleNames.AdminOrAbove)]
    public async Task<ActionResult<ApiResponse<GenreResponse>>> Update(int id, [FromBody] GenreRequest request, CancellationToken ct)
        => Success(await _service.UpdateAsync(id, request, ct), SuccessMessages.Updated("Genre"));

    /// <summary>Delete a genre (Admin).</summary>
    [HttpDelete("{id:int}"), Authorize(Roles = RoleNames.AdminOrAbove)]
    public async Task<ActionResult<ApiResponse>> Delete(int id, CancellationToken ct)
    { await _service.DeleteAsync(id, ct); return Success(SuccessMessages.Deleted("Genre")); }
}