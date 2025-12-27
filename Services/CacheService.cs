using Frontend.Models.Converted;
using System.Diagnostics.CodeAnalysis;

namespace Frontend.Services
{
    public interface ICacheService
    {
        Dictionary<Guid, EntityDefinition> EntityDefinitionCache { get; set; }
        Dictionary<Guid, Guid, EntityInstance> EntityInstancesCache { get; set; }
    }
    public class CacheService : ICacheService
    {
        public Dictionary<Guid, EntityDefinition> EntityDefinitionCache { get; set; } = [];
        public Dictionary<Guid, Guid, EntityInstance> EntityInstancesCache { get; set; } = new();
    }
    public class Dictionary<TKeyA, TKeyB, TValue> where TKeyA : notnull where TKeyB : notnull
    {
        private readonly Dictionary<TKeyA, List<TValue>> _dictA = [];
        private readonly Dictionary<TKeyB, (TKeyA KeyA, TValue Value)> _dictB = [];

        public int Count => _dictB.Count;
        public ICollection<TKeyA> KeysA => _dictA.Keys;
        public ICollection<TKeyB> KeysB => _dictB.Keys;
        public ICollection<TValue> Values => [.. _dictB.Values.Select(x => x.Value)];

        public void Add(TKeyA keyA, TKeyB keyB, TValue value)
        {
            if (_dictB.ContainsKey(keyB))
                throw new ArgumentException("An item with the same unique key (KeyB) already exists.", nameof(keyB));

            _dictB.Add(keyB, (keyA, value));

            if (!_dictA.TryGetValue(keyA, out List<TValue>? list))
            {
                list = [];
                _dictA.Add(keyA, list);
            }
            list.Add(value);
        }
        public void AddRange(IEnumerable<(TKeyA keyA, TKeyB keyB, TValue value)> adds)
        {
            foreach ((TKeyA keyA, TKeyB keyB, TValue value) in adds)
                Add(keyA, keyB, value);
        }

        public bool TryGetValue(TKeyA keyA, [MaybeNullWhen(false)] out List<TValue> values) => _dictA.TryGetValue(keyA, out values);
        public bool TryGetValue(TKeyB keyB, [MaybeNullWhen(false)] out TValue value)
        {
            bool ret = _dictB.TryGetValue(keyB, out (TKeyA KeyA, TValue Value) entry);
            value = entry.Value;
            return ret;
        }
        public bool Removes(TKeyA keyA)
        {
            if (!_dictA.TryGetValue(keyA, out List<TValue>? valuesToRemove)) return false;

            foreach (TKeyB keyB in _dictB
                .Where(kvp => kvp.Value.KeyA.Equals(keyA))
                .Select(kvp => kvp.Key))
                _dictB.Remove(keyB);

            _dictA.Remove(keyA);

            return true;
        }
        public bool Remove(TKeyB keyB)
        {
            if (!_dictB.TryGetValue(keyB, out (TKeyA KeyA, TValue Value) entry)) return false;

            _dictB.Remove(keyB);

            if (_dictA.TryGetValue(entry.KeyA, out List<TValue>? list))
            {
                list.Remove(entry.Value);
                if (list.Count == 0)
                    _dictA.Remove(entry.KeyA);
            }

            return true;
        }
        public bool ContainsList(TKeyA keyA) => _dictA.ContainsKey(keyA);
        public bool Contains(TKeyB keyB) => _dictB.ContainsKey(keyB);
        public void Clear()
        {
            _dictA.Clear();
            _dictB.Clear();
        }
    }
}
