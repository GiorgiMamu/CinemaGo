using CinemaGo.Application.Interfaces.Services;
using CinemaGo.Domain.Constants;
using CinemaGo.Domain.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace CinemaGo.API.Controllers;

public class HallsController : BaseApiController
{
    private readonly IHallService _halls;
    private readonly ISeatService _seats;
    public HallsController(IHallService halls, ISeatService seats) { _halls = halls; _seats = seats; }

    /// <summary>List all halls.</summary>
    [HttpGet, AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<HallResponse>>>> GetAll(CancellationToken ct)
        => Success(await _halls.GetAllAsync(ct), SuccessMessages.Retrieved);

    /// <summary>Get one hall.</summary>
    [HttpGet("{id:int}"), AllowAnonymous]
    public async Task<ActionResult<ApiResponse<HallResponse>>> Get(int id, CancellationToken ct)
        => Success(await _halls.GetByIdAsync(id, ct), SuccessMessages.Retrieved);

    /// <summary>List the seats of a hall ordered by row and number.</summary>
    [HttpGet("{id:int}/seats"), AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<SeatResponse>>>> GetSeats(int id, CancellationToken ct)
        => Success(await _seats.GetByHallAsync(id, ct), SuccessMessages.Retrieved);

    /// <summary>Create a hall (Admin).</summary>
    [HttpPost, Authorize(Roles = RoleNames.AdminOrAbove)]
    public async Task<ActionResult<ApiResponse<HallResponse>>> Create([FromBody] HallRequest request, CancellationToken ct)
        => SuccessCreated(await _halls.CreateAsync(request, ct), SuccessMessages.Created("Hall"));

    /// <summary>Rename a hall (Admin).</summary>
    [HttpPut("{id:int}"), Authorize(Roles = RoleNames.AdminOrAbove)]
    public async Task<ActionResult<ApiResponse<HallResponse>>> Update(int id, [FromBody] HallRequest request, CancellationToken ct)
        => Success(await _halls.UpdateAsync(id, request, ct), SuccessMessages.Updated("Hall"));

    /// <summary>Delete a hall (Admin). Fails with 409 if sessions or bookings reference it.</summary>
    [HttpDelete("{id:int}"), Authorize(Roles = RoleNames.AdminOrAbove)]
    public async Task<ActionResult<ApiResponse>> Delete(int id, CancellationToken ct)
    { await _halls.DeleteAsync(id, ct); return Success(SuccessMessages.Deleted("Hall")); }
}