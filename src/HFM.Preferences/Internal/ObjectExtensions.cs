namespace HFM.Preferences.Internal;

internal static class ObjectExtensions
{
    internal static T? Copy<T>(this T? value) =>
        value is null
            ? default
            : Copy(value, value.GetType());

    internal static T? Copy<T>(this T value, Type dataType) =>
        dataType.IsValueType || dataType == typeof(string) || value is null
            ? value
            : (T?)Activator.CreateInstance(dataType, value);
}
