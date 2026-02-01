namespace Sorrend.Core.Utilities;

public static class DateTimeExtensions
{
    public static DateTimeOffset GetYearStart(this DateTimeOffset value)
        => new(value.Year, 1, 1, 0, 0, 0, value.Offset);

    public static DateTimeOffset GetMonthStart(this DateTimeOffset value)
        => new(value.Year, value.Month, 1, 0, 0, 0, value.Offset);

    public static DateTimeOffset GetDayStart(this DateTimeOffset value)
        => new(value.Year, value.Month, value.Day, 0, 0, 0, value.Offset);
}
