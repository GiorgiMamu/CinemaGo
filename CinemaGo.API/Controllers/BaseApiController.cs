using CinemaGo.Domain.DTOs;
using Microsoft.AspNetCore.Mvc;
namespace CinemaGo.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected ActionResult<ApiResponse<T>> Success<T>(T data, string message) => Ok(ApiResponse<T>.Ok(data, message));
    protected ActionResult<ApiResponse<T>> SuccessCreated<T>(T data, string message)
        => StatusCode(StatusCodes.Status201Created, ApiResponse<T>.Ok(data, message));
    protected ActionResult<ApiResponse> Success(string message) => Ok(ApiResponse.Ok(message));
}