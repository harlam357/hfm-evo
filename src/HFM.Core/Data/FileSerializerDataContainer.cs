using HFM.Core.Serializers;

namespace HFM.Core.Data;

public abstract class FileSerializerDataContainer<T> : DataContainer<T> where T : class, new()
{
    private readonly object _serializeLock = new();

    public override void Read()
    {
        T? data;

        lock (_serializeLock)
        {
            data = FileSerializer.Deserialize(FilePath);
        }

        Data = data ?? new T();
    }

    public override void Write()
    {
        lock (_serializeLock)
        {
            FileSerializer.Serialize(FilePath, Data);
        }
    }

    public abstract IFileSerializer<T> FileSerializer { get; }

    public abstract string FilePath { get; }
}
