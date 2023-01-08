namespace HFM.Core.Internal;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public static class DateTimeExtensions
{
    public static string ToShortStringOrEmpty(this DateTime dateTime) =>
        ToStringOrEmpty(dateTime, $"{dateTime.ToShortDateString()} {dateTime.ToShortTimeString()}");

    private static string ToStringOrEmpty(this IEquatable<DateTime> date, string formattedValue) =>
        date.Equals(DateTime.MinValue) ? String.Empty : formattedValue;
}
