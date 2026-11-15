namespace CinemaGo.Domain.Constants;

public static class RoleNames
{
    public const string SuperAdmin = "SuperAdmin";
    public const string Admin = "Admin";
    public const string Customer = "Customer";

    // for [Authorize(Roles = RoleNames.AdminOrAbove)]
    public const string AdminOrAbove = SuperAdmin + "," + Admin;
}