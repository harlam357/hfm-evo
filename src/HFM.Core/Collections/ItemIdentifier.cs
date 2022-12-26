namespace HFM.Core.Collections;

public interface IItemIdentifier
{
    int Id { get; }
}

public static class ItemIdentifier
{
    public const int NoId = -1;
}
