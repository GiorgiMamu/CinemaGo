using CinemaGo.Application.Interfaces.Services;
using CinemaGo.Domain.Constants;
using CinemaGo.Domain.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace CinemaGo.API.Controllers;

[Authorize(Roles = RoleNames.AdminOrAbove)]
public class UsersController : BaseApiController
{
    private readonly IUserService _service;
    public UsersController(IUserService service) => _service = service;

    /// <summary>List users with search, role filter and paging (Admin and SuperAdmin).</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<UserResponse>>>> GetAll([FromQuery] UserFilter filter, CancellationToken ct)
        => Success(await _service.GetPagedAsync(filter, ct), SuccessMessages.Retrieved);

    /// <summary>Create an Admin account, active immediately (SuperAdmin only).</summary>
    [HttpPost("admins"), Authorize(Roles = RoleNames.SuperAdmin)]
    public async Task<ActionResult<ApiResponse<UserResponse>>> CreateAdmin([FromBody] CreateAdminRequest request, CancellationToken ct)
        => SuccessCreated(await _service.CreateAdminAsync(request, ct), SuccessMessages.Created("Admin"));

    /// <summary>Change a user's role between Admin and Customer (SuperAdmin only).</summary>
    [HttpPut("{id}/role"), Authorize(Roles = RoleNames.SuperAdmin)]
    public async Task<ActionResult<ApiResponse<UserResponse>>> ChangeRole(string id, [FromBody] ChangeRoleRequest request, CancellationToken ct)
        => Success(await _service.ChangeRoleAsync(id, request, ct), SuccessMessages.RoleChanged);

    /// <summary>Activate or deactivate an account. Admins can manage customers; SuperAdmin can manage admins too.</summary>
    [HttpPatch("{id}/active")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> SetActive(string id, [FromBody] SetActiveRequest request, CancellationToken ct)
        => Success(await _service.SetActiveAsync(id, request, ct), SuccessMessages.StatusChanged);

    /// <summary>Delete a user. Admins can delete customers; SuperAdmin can delete admins and customers.</summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> Delete(string id, CancellationToken ct)
    { await _service.DeleteAsync(id, ct); return Success(SuccessMessages.Deleted("User")); }
}