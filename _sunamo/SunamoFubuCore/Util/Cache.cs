namespace SunamoFubuCsProjFile;


[Serializable]
internal class Cache<TKey, TValue> : IEnumerable<TValue>
{
    internal static Type type = typeof(Cache<TKey, TValue>);
    private readonly object _locker = new object();
    private readonly IDictionary<TKey, TValue> _values;
    private Func<TValue, TKey> _getKey = delegate
    {
        ThrowEx.NotImplementedMethod();
        return (dynamic)null;
    };
    private Action<TValue> _onAddition = x => { };
    private Func<TKey, TValue> _onMissing = delegate (TKey key)
    {
        var message = string.Format("Key '{0}' could not be found", key);
        throw new KeyNotFoundException(message);
    };
    internal Cache()
    : this(new Dictionary<TKey, TValue>())
    {
    }
    internal Cache(Func<TKey, TValue> onMissing)
    : this(new Dictionary<TKey, TValue>(), onMissing)
    {
    }
    internal Cache(IDictionary<TKey, TValue> dictionary, Func<TKey, TValue> onMissing)
    : this(dictionary)
    {
        _onMissing = onMissing;
    }
    internal Cache(IDictionary<TKey, TValue> dictionary)
    {
        _values = dictionary;
    }
    internal Action<TValue> OnAddition
    {
        set => _onAddition = value;
    }
    internal Func<TKey, TValue> OnMissing
    {
        set => _onMissing = value;
    }
    internal Func<TValue, TKey> GetKey
    {
        get => _getKey;
        set => _getKey = value;
    }
    internal int Count => _values.Count;
    internal TValue First
    {
        get
        {
            foreach (var pair in _values) return pair.Value;
            return default;
        }
    }
    internal TValue this[TKey key]
    {
        get
        {
            FillDefault(key);
            return _values[key];
        }
        set
        {
            _onAddition(value);
            if (_values.ContainsKey(key))
                _values[key] = value;
            else
                _values.Add(key, value);
        }
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable<TValue>)this).GetEnumerator();
    }
    internal IEnumerator<TValue> GetEnumerator()
    {
        return _values.Values.GetEnumerator();
    }
    /// <summary>
    ///     Guarantees that the Cache has the default value for a given key.
    ///     If it does not already exist, it's created.
    /// </summary>
    /// <param name="key"></param>
    internal void FillDefault(TKey key)
    {
        Fill(key, _onMissing);
    }
    internal void Fill(TKey key, Func<TKey, TValue> onMissing)
    {
        if (!_values.ContainsKey(key))
            lock (_locker)
            {
                if (!_values.ContainsKey(key))
                {
                    var value = onMissing(key);
                    _onAddition(value);
                    _values.Add(key, value);
                }
            }
    }
    internal void Fill(TKey key, TValue value)
    {
        if (_values.ContainsKey(key)) return;
        _values.Add(key, value);
    }
    internal void Each(Action<TValue> action)
    {
        foreach (var pair in _values) action(pair.Value);
    }
    internal void Each(Action<TKey, TValue> action)
    {
        foreach (var pair in _values) action(pair.Key, pair.Value);
    }
    internal bool Has(TKey key)
    {
        return _values.ContainsKey(key);
    }
    internal bool Exists(Predicate<TValue> predicate)
    {
        var returnValue = false;
        Each(delegate (TValue value) { returnValue |= predicate(value); });
        return returnValue;
    }
    internal TValue Find(Predicate<TValue> predicate)
    {
        foreach (var pair in _values)
            if (predicate(pair.Value))
                return pair.Value;
        return default;
    }
    internal TKey[] GetAllKeys()
    {
        return _values.Keys.ToArray();
    }
    internal TValue[] GetAll()
    {
        return _values.Values.ToArray();
    }
    internal void Remove(TKey key)
    {
        if (_values.ContainsKey(key)) _values.Remove(key);
    }
    internal void ClearAll()
    {
        _values.Clear();
    }
    internal bool WithValue(TKey key, Action<TValue> callback)
    {
        if (Has(key))
        {
            callback(this[key]);
            return true;
        }
        return false;
    }
    internal IDictionary<TKey, TValue> ToDictionary()
    {
        return new Dictionary<TKey, TValue>(_values);
    }
}