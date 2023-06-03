using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml;
using System.Xml.Xsl;

using HFM.Core.Serializers;
using HFM.Preferences;

namespace HFM.Core.Artifacts.SlotMarkup;

[ExcludeFromCodeCoverage]
public readonly record struct SlotHtmlContent(string Name, StringBuilder Content);

[ExcludeFromCodeCoverage]
public readonly record struct SlotHtmlBuilderResult(ICollection<SlotHtmlContent> SlotSummaryHtml, ICollection<SlotHtmlContent> SlotDetailHtml);

public class SlotHtmlBuilder
{
    // TODO: Move to deployment class
    //public static IEnumerable<string> StaticCssFileNames { get; } = new[] { "HFM.css" };

    private readonly IPreferences _preferences;

    public SlotHtmlBuilder(IPreferences preferences)
    {
        _preferences = preferences;
    }

    public SlotHtmlBuilderResult Build(SlotXmlBuilderResult xmlBuilderResult)
    {
        var cssFileName = _preferences.Get<string>(Preference.CssFile)!;

        //var cssFiles = CopyCssFiles(path, cssFileName).ToList();
        var slotSummaryHtml = EnumerateSlotSummaryHtml(xmlBuilderResult.SlotSummary, cssFileName).ToList();
        var slotDetailHtml = EnumerateSlotDetailHtml(xmlBuilderResult.SlotDetails, cssFileName).ToList();
        return new(slotSummaryHtml, slotDetailHtml);
    }

    // TODO: Move to deployment class
    //private IEnumerable<string> CopyCssFiles(string path, string cssFileName)
    //{
    //    var applicationPath = _preferences.Get<string>(Preference.ApplicationPath);
    //    var cssFolderName = _preferences.Get<string>(Preference.CssFolderName);
    //
    //    string cssFilePath = Path.Combine(applicationPath, cssFolderName, cssFileName);
    //    if (File.Exists(cssFilePath))
    //    {
    //        var destFileName = Path.Combine(path, cssFileName);
    //        File.Copy(cssFilePath, destFileName, true);
    //        yield return destFileName;
    //    }
    //
    //    foreach (var name in StaticCssFileNames)
    //    {
    //        cssFilePath = Path.Combine(applicationPath, cssFolderName, name);
    //        if (File.Exists(cssFilePath))
    //        {
    //            var destFileName = Path.Combine(path, name);
    //            File.Copy(cssFilePath, destFileName, true);
    //            yield return destFileName;
    //        }
    //    }
    //}

    private XmlReaderSettings XmlReaderSettings { get; } = new() { DtdProcessing = DtdProcessing.Ignore };

    private IEnumerable<SlotHtmlContent> EnumerateSlotSummaryHtml(SlotSummary slotSummary, string cssFileName)
    {
        var summaryXml = LoadXml(slotSummary);

        var sb = Transform(summaryXml, GetXsltFileName(Preference.WebOverview), cssFileName);
        yield return new("index", sb);

        sb = Transform(summaryXml, GetXsltFileName(Preference.WebSummary), cssFileName);
        yield return new("summary", sb);
    }

    private IEnumerable<SlotHtmlContent> EnumerateSlotDetailHtml(IEnumerable<SlotDetail> slotDetails, string cssFileName)
    {
        string slotXslt = GetXsltFileName(Preference.WebSlot);

        foreach (var f in slotDetails)
        {
            var slotXml = LoadXml(f);
            var sb = Transform(slotXml, slotXslt, cssFileName);
            yield return new(f.SlotData!.Name!, sb);
        }
    }

    private XmlDocument LoadXml<T>(T input)
    {
        using var memoryStream = new MemoryStream();
        var serializer = new DataContractSerializer<T>();
        serializer.Serialize(memoryStream, input);
        memoryStream.Position = 0;

        using var reader = XmlReader.Create(memoryStream, XmlReaderSettings);
        var xml = new XmlDocument();
        xml.Load(reader);
        return xml;
    }

    private StringBuilder Transform(XmlNode xmlDoc, string xsltFilePath, string cssFileName)
    {
        var xslt = new XslCompiledTransform();
        using (var reader = XmlReader.Create(xsltFilePath, XmlReaderSettings))
        {
            xslt.Load(reader, null, new XmlUrlResolver());
        }

        using var sw = new StringWriter();
        xslt.Transform(xmlDoc, null, sw);

        var sb = sw.GetStringBuilder();
        sb.Replace("$CSSFILE", cssFileName);
        return sb;
    }

    private string GetXsltFileName(Preference p)
    {
        var xslt = _preferences.Get<string>(p)!;

        if (Path.IsPathRooted(xslt))
        {
            if (File.Exists(xslt))
            {
                return xslt;
            }
        }
        else
        {
            string applicationPath = _preferences.Get<string>(Preference.ApplicationPath)!;
            string xsltFolderName = _preferences.Get<string>(Preference.XsltFolderName)!;
            string xsltFileName = Path.Combine(applicationPath, xsltFolderName, xslt);
            if (File.Exists(xsltFileName))
            {
                return xsltFileName;
            }
        }

        throw new FileNotFoundException($"XSLT File '{xslt}' cannot be found.");
    }
}
