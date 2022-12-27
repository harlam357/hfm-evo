using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace HFM.Console;

internal static class Application
{
    internal static string? Path { get; private set; }

    internal static string? DataFolderPath { get; private set; }

    internal static void SetPaths(string path, string dataFolderPath)
    {
        Path = path;
        DataFolderPath = dataFolderPath;
    }

    private static string? _Version;
    /// <summary>
    /// Gets a string in the format Major.Minor.Build.
    /// </summary>
    internal static string Version => _Version ??= CreateVersionString("{0}.{1}.{2}");

    private static string CreateVersionString(string format)
    {
        var fvi = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
        return String.Format(CultureInfo.InvariantCulture, format, fvi.FileMajorPart, fvi.FileMinorPart, fvi.FileBuildPart, fvi.FilePrivatePart);
    }
}
