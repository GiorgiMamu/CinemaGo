using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CinemaGo.Application.Interfaces;
using CinemaGo.Application.Services;
using CinemaGo.Application.Validators;
using CinemaGo.Domain.DTOs;
using CinemaGo.Domain.Exceptions;
using CinemaGo.Domain.Interfaces.External;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
namespace CinemaGo.Tests;


public class ValidatorTests
{
    [Theory]
    [InlineData("alllowercase1", false)]
    [InlineData("NoDigitsHere", false)]
    [InlineData("Valid_Pass1", true)]
    public void Register_PasswordStrength(string password, bool expectedValid)
    {
        var request = new RegisterRequest { UserName = "john", Email = "j@x.com", Password = password, ConfirmPassword = password };
        Assert.Equal(expectedValid, new RegisterRequestValidator().Validate(request).IsValid);
    }

    [Fact]
    public void Session_StartInThePast_IsInvalid()
    {
        var request = new SessionRequest { MovieId = 1, HallId = 1, StartsAt = DateTime.UtcNow.AddDays(-1), StandardPrice = 10, VipPrice = 20 };
        Assert.False(new SessionRequestValidator().Validate(request).IsValid);
    }

    [Fact]
    public void Session_VipCheaperThanStandard_IsInvalid()
    {
        var request = new SessionRequest { MovieId = 1, HallId = 1, StartsAt = DateTime.UtcNow.AddDays(1), StandardPrice = 20, VipPrice = 10 };
        Assert.False(new SessionRequestValidator().Validate(request).IsValid);
    }

    [Fact]
    public void FileUpload_FakeImage_IsRejected()
    {
        var bytes = System.Text.Encoding.ASCII.GetBytes("this is plain text, not an image");
        var file = new FileUpload { Content = new MemoryStream(bytes), FileName = "poster.png", Length = bytes.Length };
        Assert.False(new FileUploadValidator().Validate(file).IsValid);
    }

    [Fact]
    public void FileUpload_RealPngHeader_IsAccepted()
    {
        var bytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 0, 0, 0 };
        var file = new FileUpload { Content = new MemoryStream(bytes), FileName = "poster.png", Length = bytes.Length };
        Assert.True(new FileUploadValidator().Validate(file).IsValid);
    }
    [Fact]
    public void ChangePassword_SameAsCurrent_IsInvalid()
    {
        var r = new ChangePasswordRequest { CurrentPassword = "Valid_Pass1", NewPassword = "Valid_Pass1", ConfirmNewPassword = "Valid_Pass1" };
        Assert.False(new ChangePasswordRequestValidator().Validate(r).IsValid);
    }

    [Fact]
    public void ResetPassword_WeakPassword_IsInvalid()
    {
        var r = new ResetPasswordRequest { Email = "j@x.com", Code = "123456", NewPassword = "alllowercase", ConfirmNewPassword = "alllowercase" };
        Assert.False(new ResetPasswordRequestValidator().Validate(r).IsValid);
    }
}