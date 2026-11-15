namespace CinemaGo.Application.Common;

public static class TicketCodeGenerator
{
    // e.g. CG-261105-9F3A1C7B (unique index in the database is the final guarantee)
    public static string Generate()
        => $"CG-{DateTime.UtcNow:yyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
}