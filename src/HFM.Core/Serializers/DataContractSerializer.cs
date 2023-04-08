using System.Runtime.Serialization;
using System.Xml;

namespace HFM.Core.Serializers;

public class DataContractSerializer<T> : ISerializer<T>
{
    public void Serialize(Stream stream, T? value)
    {
        using var xmlWriter = XmlWriter.Create(stream, new() { Indent = true });
        var serializer = new DataContractSerializer(typeof(T));
        serializer.WriteObject(xmlWriter, value);
    }

    public T? Deserialize(Stream stream)
    {
        var serializer = new DataContractSerializer(typeof(T));
        return (T?)serializer.ReadObject(stream);
    }
}
