namespace CinemaGo.Application.Common;

public static class SessionTimeRules
{
    public static readonly TimeSpan CleaningBuffer = TimeSpan.FromMinutes(15);

    public static DateTime GetEnd(DateTime start, int durationMinutes)
        => start.AddMinutes(durationMinutes) + CleaningBuffer;

    // half-open intervals: back-to-back sessions (end == next start) do not overlap
    public static bool Overlaps(DateTime startA, DateTime endA, DateTime startB, DateTime endB)
        => startA < endB && startB < endA;
}