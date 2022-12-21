using System.Text;

using HFM.Client;

namespace HFM.Core.Client.Mocks;

internal static class FahClientMessageFileReader
{
    internal static IEnumerable<FahClientMessage> ReadAllMessages(string path)
    {
        var extractor = new FahClientJsonMessageExtractor();

        foreach (var file in EnumerateMessageFilesByReceivedDateTime(path))
        {
            if (Path.GetFileName(file) == "log.txt")
            {
                //using (var textReader = new StreamReader(file))
                //using (var reader = new FahClientLogTextReader(textReader))
                //{
                //    await Messages.Log.ReadAsync(reader);
                //}
            }
            else
            {
                yield return extractor.Extract(new StringBuilder(File.ReadAllText(file)));
            }
        }
    }

    private static IEnumerable<string> EnumerateMessageFilesByReceivedDateTime(string path) =>
        Directory.EnumerateFiles(path).OrderBy(x =>
        {
            var fileName = Path.GetFileNameWithoutExtension(x);
            return fileName[fileName.LastIndexOf("-", StringComparison.Ordinal)..];
        });

    internal static FahClientMessage ReadMessage(string path)
    {
        var extractor = new FahClientJsonMessageExtractor();
        return extractor.Extract(new StringBuilder(File.ReadAllText(path)));
    }
}
