namespace HFM.Core;

public sealed class ArtifactFolder : IDisposable
{
    public string Path { get; }

    public ArtifactFolder() : this(Environment.CurrentDirectory)
    {

    }

    public ArtifactFolder(string basePath)
    {
        Path = GetRandomPath(basePath);
        Directory.CreateDirectory(Path);
    }

    public string GetRandomFilePath() => GetRandomPath(Path);

    public static string GetRandomPath(string path) => System.IO.Path.Combine(path, System.IO.Path.GetRandomFileName());

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, true);
            }
        }
        catch (Exception)
        {
            // do nothing
        }
    }
}
