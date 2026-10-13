using System;
using System.Collections.Generic;
using UnityEngine;

namespace fefek5.Toys.Runtime
{
    /// <summary>
    /// A <see cref="Dictionary{TKey,TValue}"/> Unity can serialize and draw in the inspector. It is a real dictionary,
    /// so it goes wherever one is expected; the pairs are kept in a serialized list next to it.
    /// </summary>
    /// <remarks>
    /// A list entry the dictionary cannot hold — a null key, or a key that already came earlier in the list — is kept
    /// in the list, so adding or retyping a key in the inspector loses nothing, but it is left out of the dictionary
    /// and the inspector marks it. The first entry with a key wins. The comparer is not serialized: after loading,
    /// the dictionary uses the default one.
    /// </remarks>
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [Serializable]
        private struct Entry
        {
            public TKey key;
            public TValue value;

            public Entry(TKey key, TValue value)
            {
                this.key = key;
                this.value = value;
            }
        }

        [SerializeField] private List<Entry> entries = new();

        [NonSerialized] private HashSet<TKey> _keptKeys;

        #region Constructors

        public SerializableDictionary() { }

        public SerializableDictionary(int capacity) : base(capacity) { }

        public SerializableDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary) { }

        public SerializableDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection) : base(collection) { }

        #endregion

        #region Serialization

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            // Bring the list up to date with the dictionary in place, so the order of the entries and the ones the
            // dictionary does not hold survive. A key removed from the dictionary takes all its entries with it.
            _keptKeys ??= new HashSet<TKey>(Comparer);

            for (var i = 0; i < entries.Count;)
            {
                var key = entries[i].key;

                if (key is null || _keptKeys.Contains(key))
                    i++;
                else if (TryGetValue(key, out var value))
                {
                    entries[i] = new Entry(key, value);
                    _keptKeys.Add(key);
                    i++;
                }
                else
                    entries.RemoveAt(i);
            }

            if (_keptKeys.Count < Count)
                foreach (var (key, value) in this)
                    if (!_keptKeys.Contains(key))
                        entries.Add(new Entry(key, value));

            _keptKeys.Clear();
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            Clear();

            foreach (var entry in entries)
                if (entry.key is not null)
                    TryAdd(entry.key, entry.value);
        }

        #endregion
    }
}
