namespace CinemaGo.Application.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
    bool IsStaff { get; }   // Admin or SuperAdmin
}