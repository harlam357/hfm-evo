﻿using System.Runtime.Serialization;
using System.Xml;

using HFM.Preferences.Data;

namespace HFM.Preferences;

public partial class XmlPreferencesProvider : PreferencesProvider
{
    public XmlPreferencesProvider(string applicationPath, string applicationDataFolderPath, string applicationVersion)
        : base(applicationPath, applicationDataFolderPath, applicationVersion)
    {

    }

    protected override PreferenceData? OnRead()
    {
        string path = Path.Combine(ApplicationDataFolderPath, "config.xml");
        if (File.Exists(path))
        {
            try
            {
                using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
                var serializer = new DataContractSerializer(typeof(PreferenceData));
                var data = (PreferenceData?)serializer.ReadObject(fileStream);
                return data;
            }
            catch (Exception)
            {
                return null;
            }
        }

        return null;
    }

    private void EnsureApplicationDataFolderExists()
    {
        if (!Directory.Exists(ApplicationDataFolderPath))
        {
            Directory.CreateDirectory(ApplicationDataFolderPath);
        }
    }

    protected override void OnWrite(PreferenceData data)
    {
        EnsureApplicationDataFolderExists();

        string path = Path.Combine(ApplicationDataFolderPath, "config.xml");
        WriteConfigXml(path, data);
    }

    public static void WriteConfigXml(string path, PreferenceData data)
    {
        using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
        using var xmlWriter = XmlWriter.Create(fileStream, new XmlWriterSettings { Indent = true });
        var serializer = new DataContractSerializer(typeof(PreferenceData));
        serializer.WriteObject(xmlWriter, data);
    }
}
