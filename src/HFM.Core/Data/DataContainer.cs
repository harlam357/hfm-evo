namespace HFM.Core.Data;

public interface IDataContainer<T> where T : class
{
    T? Data { get; set; }
    void Read();
    void Write();
}

public abstract class DataContainer<T> : IDataContainer<T> where T : class
{
    public T? Data { get; set; }

    public abstract void Read();

    public abstract void Write();
}
