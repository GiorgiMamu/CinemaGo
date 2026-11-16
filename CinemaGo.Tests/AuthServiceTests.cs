using System.IdentityModel.Tokens.Jwt;
using CinemaGo.Application.Interfaces;
using CinemaGo.Application.Services;
using CinemaGo.Domain.Constants;
using CinemaGo.Domain.DTOs;
using CinemaGo.Domain.Exceptions;
using CinemaGo.Domain.Interfaces.External;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
namespace CinemaGo.Tests;

public class AuthServiceTests
{
    private readonly IIdentityService _identity = Substitute.For<IIdentityService>();
    private readonly IEmailSender _email = Substitute.For<IEmailSender>();
    private readonly ICurrentUserService _current = Substitute.For<ICurrentUserService>();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        var config = Substitute.For<IConfiguration>();
        config["Jwt:Key"].Returns("unit-test-secret-key-0123456789-ABCDEF!");
        config["Jwt:Issuer"].Returns("test");
        config["Jwt:Audience"].Returns("test");
        config["Jwt:ExpirationInMinutes"].Returns("60");
        _identity.GetSecurityStampAsync("u1", Arg.Any<CancellationToken>()).Returns("stamp-1");
        _sut = new AuthService(_identity, _email, _current, config, NullLogger<AuthService>.Instance);
    }

    private static UserResponse User(bool active = true, bool confirmed = true) => new()
    { Id = "u1", UserName = "john", Email = "j@x.com", IsActive = active, EmailConfirmed = confirmed, Roles = { "Customer" } };

    [Fact]
    public async Task Register_CreatesInactiveCustomer_AndEmailsTheCode()
    {
        _identity.CreateUserAsync("john", "j@x.com", "Valid_Pass1", RoleNames.Customer, false, Arg.Any<CancellationToken>())
            .Returns(User(active: false, confirmed: false));
        _identity.GenerateEmailConfirmationCodeAsync("u1", Arg.Any<CancellationToken>()).Returns("123456");

        var result = await _sut.RegisterAsync(new RegisterRequest { UserName = "john", Email = "j@x.com", Password = "Valid_Pass1" }, TestContext.Current.CancellationToken);

        Assert.False(result.IsActive);
        await _email.Received(1).SendAsync("j@x.com", Arg.Any<string>(), Arg.Is<string>(b => b.Contains("123456")), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Register_WhenEmailSendingFails_StillSucceeds()
    {
        _identity.CreateUserAsync(default!, default!, default!, default!, default, TestContext.Current.CancellationToken).ReturnsForAnyArgs(User(false, false));
        _identity.GenerateEmailConfirmationCodeAsync("u1", Arg.Any<CancellationToken>()).Returns("123456");
        _email.SendAsync(default!, default!, default!, TestContext.Current.CancellationToken).ReturnsForAnyArgs<Task>(_ => throw new Exception("smtp down"));

        var result = await _sut.RegisterAsync(new RegisterRequest { UserName = "john", Email = "j@x.com", Password = "Valid_Pass1" }, TestContext.Current.CancellationToken);
        Assert.Equal("u1", result.Id);
    }

    [Fact]
    public async Task Login_WrongPassword_ThrowsUnauthorized()
    {
        _identity.FindByEmailAsync("j@x.com", Arg.Any<CancellationToken>()).Returns(User());
        _identity.CheckPasswordAsync("u1", "bad", Arg.Any<CancellationToken>()).Returns(false);
        await Assert.ThrowsAsync<UnauthorizedException>(() => _sut.LoginAsync(new LoginRequest { Email = "j@x.com", Password = "bad" }, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Login_NotActivated_ThrowsForbiddenWithActivationCode()
    {
        _identity.FindByEmailAsync("j@x.com", Arg.Any<CancellationToken>()).Returns(User(active: false, confirmed: false));
        _identity.CheckPasswordAsync("u1", "Good_123", Arg.Any<CancellationToken>()).Returns(true);

        var ex = await Assert.ThrowsAsync<ForbiddenException>(() => _sut.LoginAsync(new LoginRequest { Email = "j@x.com", Password = "Good_123" }, TestContext.Current.CancellationToken));
        Assert.Equal(Errors.Auth.AccountNotActivated.Code, ex.ErrorCode);
    }

    [Fact]
    public async Task Login_DisabledByAdmin_ThrowsForbidden()
    {
        _identity.FindByEmailAsync("j@x.com", Arg.Any<CancellationToken>()).Returns(User(active: false, confirmed: true));
        _identity.CheckPasswordAsync("u1", "Good_123", Arg.Any<CancellationToken>()).Returns(true);

        var ex = await Assert.ThrowsAsync<ForbiddenException>(() => _sut.LoginAsync(new LoginRequest { Email = "j@x.com", Password = "Good_123" }, TestContext.Current.CancellationToken));
        Assert.Equal(Errors.Auth.AccountDisabled.Code, ex.ErrorCode);
    }

    [Fact]
    public async Task Login_Success_TokenCarriesRoleAndSecurityStamp()
    {
        _identity.FindByEmailAsync("j@x.com", Arg.Any<CancellationToken>()).Returns(User());
        _identity.CheckPasswordAsync("u1", "Good_123", Arg.Any<CancellationToken>()).Returns(true);

        var result = await _sut.LoginAsync(new LoginRequest { Email = "j@x.com", Password = "Good_123" }, TestContext.Current.CancellationToken);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(result.Token);
        Assert.Contains(jwt.Claims, c => c.Value == "Customer");
        Assert.Contains(jwt.Claims, c => c.Type == ClaimNames.SecurityStamp && c.Value == "stamp-1");
    }

    [Fact]
    public async Task ForgotPassword_UnknownEmail_SendsNothingAndDoesNotThrow()
    {
        _identity.FindByEmailAsync("nobody@x.com", Arg.Any<CancellationToken>()).Returns((UserResponse?)null);
        await _sut.ForgotPasswordAsync(new EmailRequest { Email = "nobody@x.com" }, TestContext.Current.CancellationToken);
        await _email.DidNotReceiveWithAnyArgs().SendAsync(default!, default!, default!, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ForgotPassword_KnownEmail_SendsCode()
    {
        _identity.FindByEmailAsync("j@x.com", Arg.Any<CancellationToken>()).Returns(User());
        _identity.GeneratePasswordResetCodeAsync("u1", Arg.Any<CancellationToken>()).Returns("654321");
        await _sut.ForgotPasswordAsync(new EmailRequest { Email = "j@x.com" }, TestContext.Current.CancellationToken);
        await _email.Received(1).SendAsync("j@x.com", Arg.Any<string>(), Arg.Is<string>(b => b.Contains("654321")), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ConfirmEmail_UnknownEmail_ThrowsInvalidCode()
    {
        _identity.FindByEmailAsync("nobody@x.com", Arg.Any<CancellationToken>()).Returns((UserResponse?)null);
        var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.ConfirmEmailAsync(new ConfirmEmailRequest { Email = "nobody@x.com", Code = "123456" }, TestContext.Current.CancellationToken));
        Assert.Equal(Errors.Auth.InvalidCode.Code, ex.ErrorCode);
    }

    [Fact]
    public async Task Logout_InvalidatesTheUsersSessions()
    {
        _current.UserId.Returns("u1");
        await _sut.LogoutAsync(TestContext.Current.CancellationToken);
        await _identity.Received(1).InvalidateSessionsAsync("u1", Arg.Any<CancellationToken>());
    }
}