using CinemaGo.Application.Interfaces.Services;
using CinemaGo.Domain.Constants;
using CinemaGo.Domain.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
namespace CinemaGo.API.Controllers;

[EnableRateLimiting("auth")]
public class AuthController : BaseApiController
{
    private readonly IAuthService _auth;
    private readonly IUserService _users;
    public AuthController(IAuthService auth, IUserService users) { _auth = auth; _users = users; }

    /// <summary>Register a new customer. The account is inactive until the e-mailed 6-digit code is confirmed.</summary>
    [HttpPost("register"), AllowAnonymous]
    public async Task<ActionResult<ApiResponse<UserResponse>>> Register([FromBody] RegisterRequest request, CancellationToken ct)
        => SuccessCreated(await _auth.RegisterAsync(request, ct), SuccessMessages.RegisteredCheckEmail);

    /// <summary>Activate the account with the 6-digit code sent by e-mail.</summary>
    [HttpPost("confirm-email"), AllowAnonymous]
    public async Task<ActionResult<ApiResponse>> ConfirmEmail([FromBody] ConfirmEmailRequest request, CancellationToken ct)
    { await _auth.ConfirmEmailAsync(request, ct); return Success(SuccessMessages.AccountActivated); }

    /// <summary>Send a new activation code (same answer whether or not the e-mail exists).</summary>
    [HttpPost("resend-confirmation"), AllowAnonymous]
    public async Task<ActionResult<ApiResponse>> ResendConfirmation([FromBody] EmailRequest request, CancellationToken ct)
    { await _auth.ResendConfirmationAsync(request, ct); return Success(SuccessMessages.CodeSentIfExists); }

    /// <summary>Log in with e-mail and password and receive a JWT. Locks the account for 15 minutes after 5 wrong passwords.</summary>
    [HttpPost("login"), AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request, CancellationToken ct)
        => Success(await _auth.LoginAsync(request, ct), SuccessMessages.LoggedIn);

    /// <summary>Log out: the current token (and all other tokens of this user) stop working immediately.</summary>
    [HttpPost("logout"), Authorize]
    public async Task<ActionResult<ApiResponse>> Logout(CancellationToken ct)
    { await _auth.LogoutAsync(ct); return Success(SuccessMessages.LoggedOut); }

    /// <summary>Get the profile of the logged-in user.</summary>
    [HttpGet("me"), Authorize]
    public async Task<ActionResult<ApiResponse<UserResponse>>> Me(CancellationToken ct)
        => Success(await _auth.GetCurrentUserAsync(ct), SuccessMessages.Retrieved);

    /// <summary>Request a password-reset code by e-mail (same answer whether or not the e-mail exists).</summary>
    [HttpPost("forgot-password"), AllowAnonymous]
    public async Task<ActionResult<ApiResponse>> ForgotPassword([FromBody] EmailRequest request, CancellationToken ct)
    { await _auth.ForgotPasswordAsync(request, ct); return Success(SuccessMessages.CodeSentIfExists); }

    /// <summary>Set a new password using the e-mailed 6-digit code.</summary>
    [HttpPost("reset-password"), AllowAnonymous]
    public async Task<ActionResult<ApiResponse>> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken ct)
    { await _auth.ResetPasswordAsync(request, ct); return Success(SuccessMessages.PasswordReset); }

    /// <summary>Change the password of the logged-in user (current password required). All tokens are invalidated.</summary>
    [HttpPost("change-password"), Authorize]
    public async Task<ActionResult<ApiResponse>> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
    { await _auth.ChangePasswordAsync(request, ct); return Success(SuccessMessages.PasswordChanged); }

    /// <summary>Delete my own account (password required). Future bookings are cancelled; personal data is anonymized.</summary>
    [HttpDelete("account"), Authorize]
    public async Task<ActionResult<ApiResponse>> DeleteAccount([FromBody] DeleteAccountRequest request, CancellationToken ct)
    { await _users.DeleteMyAccountAsync(request, ct); return Success(SuccessMessages.AccountDeleted); }
}