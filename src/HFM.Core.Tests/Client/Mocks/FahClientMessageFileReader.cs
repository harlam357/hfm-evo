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
                using var textReader = new StreamReader(file);
                var identifier = new FahClientMessageIdentifier(FahClientMessageType.LogRestart, DateTime.UtcNow);
                var messageText = new StringBuilder(textReader.ReadToEnd());
                yield return new FahClientMessage(identifier, messageText);
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
            var lastDashIndex = fileName.LastIndexOf("-", StringComparison.Ordinal);
            if (lastDashIndex != -1)
            {
                string dateTimeSubstring = fileName[lastDashIndex..];
                if (DateTime.TryParse(dateTimeSubstring, out DateTime _))
                {
                    return dateTimeSubstring;
                }
            }
            return fileName;
        });

    internal static FahClientMessage ReadMessage(string path)
    {
        var extractor = new FahClientJsonMessageExtractor();
        return extractor.Extract(new StringBuilder(File.ReadAllText(path)));
    }
}
