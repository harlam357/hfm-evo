namespace HFM.Core.Serializers;

public class DataContractFileSerializer<T> : IFileSerializer<T> where T : class, new()
{
    public string FileExtension => "xml";

    public string FileTypeFilter => "Xml Files|*.xml";

    public T? Deserialize(string path)
    {
        using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
        var serializer = new DataContractSerializer<T>();
        return serializer.Deserialize(fileStream);
    }

    public void Serialize(string path, T? value)
    {
        using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
        var serializer = new DataContractSerializer<T>();
        serializer.Serialize(fileStream, value);
    }
}
