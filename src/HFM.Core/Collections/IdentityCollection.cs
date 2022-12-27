using System.Collections;
using System.Collections.ObjectModel;

namespace HFM.Core.Collections;

public abstract class IdentityCollection<T> : IEnumerable<T> where T : IItemIdentifier
{
    private readonly ItemIdentifierKeyedCollection _inner = new();

    public int DefaultId => _inner.Count > 0 ? _inner.First().Id : ItemIdentifier.NoId;

    public int CurrentId { get; set; } = ItemIdentifier.NoId;

    public T? Current => this[CurrentId] ?? this[DefaultId];

    public void Add(T item) => _inner.Add(item);

    public int Count => _inner.Count;

    public T? this[int id] => (T?)(_inner.Contains(id) ? _inner[id] : null);

    public bool ContainsId(int id) => _inner.Contains(id);

    public IEnumerator<T> GetEnumerator() => _inner.Cast<T>().GetEnumerator();

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private class ItemIdentifierKeyedCollection : KeyedCollection<int, IItemIdentifier>
    {
        public ItemIdentifierKeyedCollection() : base(EqualityComparer<int>.Default, 1)
        {

        }

        protected override int GetKeyForItem(IItemIdentifier item) => item.Id;
    }
}
