using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CinemaGo.Application.Common;
using CinemaGo.Application.Interfaces;
using CinemaGo.Application.Interfaces.Services;
using CinemaGo.Domain.Constants;
using CinemaGo.Domain.DTOs;
using CinemaGo.Domain.Exceptions;
using CinemaGo.Domain.Interfaces.External;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
namespace CinemaGo.Application.Services;

public class AuthService : IAuthService
{
    private readonly IIdentityService _identity;
    private readonly IEmailSender _email;
    private readonly ICurrentUserService _currentUser;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IIdentityService identity, IEmailSender email, ICurrentUserService currentUser,
        IConfiguration configuration, ILogger<AuthService> logger)
    { _identity = identity; _email = email; _currentUser = currentUser; _configuration = configuration; _logger = logger; }

    // the customer is created INACTIVE; no token until the account is activated and the user logs in
    public async Task<UserResponse> RegisterAsync(RegisterRequest r, CancellationToken ct = default)
    {
        var user = await _identity.CreateUserAsync(r.UserName, r.Email, r.Password, RoleNames.Customer, activate: false, ct);
        await SendActivationCodeAsync(user, ct);
        _logger.LogInformation("Customer registered (inactive): {UserId}", user.Id);
        return user;
    }

    public async Task ConfirmEmailAsync(ConfirmEmailRequest r, CancellationToken ct = default)
    {
        // unknown e-mail gives the same answer as a wrong code (no account enumeration)
        var user = await _identity.FindByEmailAsync(r.Email, ct)
            ?? throw new BadRequestException(ErrorMessages.InvalidCode, ErrorCodes.InvalidCode);
        await _identity.ConfirmEmailAsync(user.Id, r.Code, ct);
        _logger.LogInformation("Account activated: {UserId}", user.Id);
    }

    public async Task ResendConfirmationAsync(EmailRequest r, CancellationToken ct = default)
    {
        var user = await _identity.FindByEmailAsync(r.Email, ct);
        if (user is { EmailConfirmed: false }) await SendActivationCodeAsync(user, ct);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest r, CancellationToken ct = default)
    {
        var user = await _identity.FindByEmailAsync(r.Email, ct);
        if (user is null || !await _identity.CheckPasswordAsync(user.Id, r.Password, ct))
        {
            _logger.LogWarning("Failed login attempt");
            throw new UnauthorizedException(ErrorMessages.InvalidCredentials);
        }
        if (!user.IsActive)
        {
            _logger.LogWarning("Login blocked for inactive account {UserId}", user.Id);
            throw user.EmailConfirmed
                ? new ForbiddenException(ErrorMessages.AccountDisabled)
                : new ForbiddenException(ErrorMessages.AccountNotActivated, ErrorCodes.AccountNotActivated);
        }
        _logger.LogInformation("User logged in: {UserId}", user.Id);
        return await GenerateJwtTokenAsync(user, ct);
    }

    // logout = rotate the security stamp, so the token in use (and every other token of the user) stops working
    public async Task LogoutAsync(CancellationToken ct = default)
    {
        var id = _currentUser.UserId ?? throw new UnauthorizedException();
        await _identity.InvalidateSessionsAsync(id, ct);
        _logger.LogInformation("User logged out: {UserId}", id);
    }

    public async Task<UserResponse> GetCurrentUserAsync(CancellationToken ct = default)
    {
        var id = _currentUser.UserId ?? throw new UnauthorizedException();
        return await _identity.FindByIdAsync(id, ct) ?? throw new NotFoundException("User", id);
    }

    // always the same outcome, whether or not the e-mail exists
    public async Task ForgotPasswordAsync(EmailRequest r, CancellationToken ct = default)
    {
        var user = await _identity.FindByEmailAsync(r.Email, ct);
        if (user is not { EmailConfirmed: true, IsActive: true }) return;

        var code = await _identity.GeneratePasswordResetCodeAsync(user.Id, ct);
        var (subject, body) = EmailTemplates.PasswordReset(user.UserName, code);
        await TrySendAsync(user.Email, subject, body, ct);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest r, CancellationToken ct = default)
    {
        var user = await _identity.FindByEmailAsync(r.Email, ct)
            ?? throw new BadRequestException(ErrorMessages.InvalidCode, ErrorCodes.InvalidCode);
        await _identity.ResetPasswordAsync(user.Id, r.Code, r.NewPassword, ct);
        _logger.LogInformation("Password reset: {UserId}", user.Id);
    }

    public async Task ChangePasswordAsync(ChangePasswordRequest r, CancellationToken ct = default)
    {
        var id = _currentUser.UserId ?? throw new UnauthorizedException();
        await _identity.ChangePasswordAsync(id, r.CurrentPassword, r.NewPassword, ct);   // rotates the stamp: log in again
        _logger.LogInformation("Password changed: {UserId}", id);
    }

    // ---------- helpers ----------
    private async Task SendActivationCodeAsync(UserResponse user, CancellationToken ct)
    {
        var code = await _identity.GenerateEmailConfirmationCodeAsync(user.Id, ct);
        var (subject, body) = EmailTemplates.Activation(user.UserName, code);
        await TrySendAsync(user.Email, subject, body, ct);
    }

    // an SMTP problem must not break registration; the user can request a new code
    private async Task TrySendAsync(string to, string subject, string body, CancellationToken ct)
    {
        try { await _email.SendAsync(to, subject, body, ct); }
        catch (Exception ex) { _logger.LogError(ex, "Could not send e-mail '{Subject}'", subject); }
    }

    private async Task<AuthResponse> GenerateJwtTokenAsync(UserResponse user, CancellationToken ct)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured.");
        var expireMinutes = int.Parse(_configuration["Jwt:ExpirationInMinutes"] ?? "60");
        var expiresAt = DateTime.UtcNow.AddMinutes(expireMinutes);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimNames.SecurityStamp, await _identity.GetSecurityStampAsync(user.Id, ct))   // checked on every request
        };
        claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(_configuration["Jwt:Issuer"], _configuration["Jwt:Audience"], claims,
            expires: expiresAt, signingCredentials: credentials);

        return new AuthResponse { Token = new JwtSecurityTokenHandler().WriteToken(token), ExpiresAt = expiresAt, User = user };
    }
}