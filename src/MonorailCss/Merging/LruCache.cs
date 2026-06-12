namespace MonorailCss.Merging;

/// <summary>
/// A minimal thread-safe least-recently-used cache. Used by <see cref="ClassMerger"/> to bound
/// memoization of per-class signatures and merge results (arbitrary values make the key space
/// unbounded, so the unbounded ConcurrentDictionary memoizers used elsewhere don't fit here).
/// </summary>
internal sealed class LruCache<TKey, TValue>
    where TKey : notnull
{
    private readonly int _capacity;
    private readonly Dictionary<TKey, LinkedListNode<(TKey Key, TValue Value)>> _map;
    private readonly LinkedList<(TKey Key, TValue Value)> _order = new();
    private readonly Lock _sync = new();

    public LruCache(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(capacity, 1);
        _capacity = capacity;
        _map = new Dictionary<TKey, LinkedListNode<(TKey, TValue)>>(capacity);
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        lock (_sync)
        {
            if (_map.TryGetValue(key, out var node))
            {
                _order.Remove(node);
                _order.AddFirst(node);
                value = node.Value.Value;
                return true;
            }
        }

        value = default!;
        return false;
    }

    public void Set(TKey key, TValue value)
    {
        lock (_sync)
        {
            if (_map.TryGetValue(key, out var existing))
            {
                _order.Remove(existing);
                existing.Value = (key, value);
                _order.AddFirst(existing);
                return;
            }

            if (_map.Count >= _capacity)
            {
                var evicted = _order.Last!;
                _order.RemoveLast();
                _map.Remove(evicted.Value.Key);
            }

            _map[key] = _order.AddFirst((key, value));
        }
    }
}
