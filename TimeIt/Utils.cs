namespace TimeIt;

internal static class DateTimeDeconstruction
{
    private const long TicksPerDay = TicksPerHour * 24;
    private const long TicksPerHour = TicksPerMinute * 60;
    private const long TicksPerMicroSeconds = 10;
    private const long TicksPerMillisecond = 10_000;
    private const long TicksPerMinute = TicksPerSecond * 60;
    private const long TicksPerSecond = TicksPerMillisecond * 1_000;

    public static void Deconstruct(this TimeSpan timeSpan, out int days, out int hours, out int minutes, out int seconds, out int ticks)
    {
        if (timeSpan == TimeSpan.MaxValue)
        {
            days = hours = minutes = seconds = ticks = 0;
            return;
        }
        var t = timeSpan.Ticks;
        days = (int)(t / (TicksPerHour * 24));
        hours = (int)((t / TicksPerHour) % 24);
        minutes = (int)((t / TicksPerMinute) % 60);
        seconds = (int)((t / TicksPerSecond) % 60);
        ticks = (int)(t % 10_000_000);
    }
}