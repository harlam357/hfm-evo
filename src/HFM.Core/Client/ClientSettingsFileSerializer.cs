using System.Runtime.Serialization;
using System.Xml;

using HFM.Core.Internal;
using HFM.Core.Serializers;

namespace HFM.Core.Client;

public class ClientSettingsFileSerializer : IFileSerializer<List<ClientSettings>>
{
    public const string DefaultFileExtension = "hfmx";

    public string FileExtension => DefaultFileExtension;

    public const string DefaultFileTypeFilter = "HFM Configuration Files|*.hfmx";

    public string FileTypeFilter => DefaultFileTypeFilter;

    public List<ClientSettings>? Deserialize(string path)
    {
        List<ClientSettings>? settingsCollection;

        using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
        using (var reader = XmlReader.Create(fileStream))
        {
            var serializer = new DataContractSerializer(typeof(List<ClientSettings>));
            settingsCollection = (List<ClientSettings>?)serializer.ReadObject(reader);
            Decrypt(settingsCollection);
        }

        if (RequiresGuids(settingsCollection))
        {
            Serialize(path, settingsCollection);
        }

        return settingsCollection;
    }

    public void Serialize(string path, List<ClientSettings>? value)
    {
        // for configurations without Guid values, add them when saving
        GenerateRequiredGuids(value);

        // copy the values before encrypting, otherwise the ClientSettings
        // objects will retain the encrypted value from here on out...
        var valueCopy = ProtoBuf.Serializer.DeepClone(value);

        using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
        using var xmlWriter = XmlWriter.Create(fileStream, new XmlWriterSettings { Indent = true });
        var serializer = new DataContractSerializer(typeof(List<ClientSettings>));
        Encrypt(valueCopy);
        serializer.WriteObject(xmlWriter, valueCopy);
    }

    private static bool RequiresGuids(IEnumerable<ClientSettings>? collection) =>
        collection?.Any(x => x.Guid == Guid.Empty) ?? false;

    private static void GenerateRequiredGuids(List<ClientSettings>? collection)
    {
        if (collection is null) return;

        for (int i = 0; i < collection.Count; i++)
        {
            var settings = collection[i];
            if (settings.Guid == Guid.Empty)
            {
                settings = settings with { Guid = Guid.NewGuid() };
            }
            collection[i] = settings;
        }
    }

    private const string InitializationVector = "CH/&QE;NsT.2z+Me";
    private const string SymmetricKey = "usPP'/Cb5?NWC*60";

    private static void Encrypt(List<ClientSettings>? collection)
    {
        var cryptography = new Cryptography(SymmetricKey, InitializationVector);
        EncryptDecryptCollection(collection, cryptography.EncryptValue);
    }

    private static void Decrypt(List<ClientSettings>? collection)
    {
        var cryptography = new Cryptography(SymmetricKey, InitializationVector);
        EncryptDecryptCollection(collection, cryptography.DecryptValue);
    }

    private static void EncryptDecryptCollection(List<ClientSettings>? collection, Func<string, string> encryptDecryptFunc)
    {
        if (collection is null) return;

        for (int i = 0; i < collection.Count; i++)
        {
            var settings = collection[i];
            if (String.IsNullOrWhiteSpace(settings.Password))
            {
                continue;
            }

            settings = settings with { Password = encryptDecryptFunc(settings.Password) };
            collection[i] = settings;
        }
    }
}
