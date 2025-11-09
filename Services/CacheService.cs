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
        private readonly Dictionary<TKeyB, TValue> _dictB = [];
        private readonly Dictionary<TKeyB, TKeyA> _keyBToKeyAMap = [];

        public int Count => _dictB.Count;
        public ICollection<TKeyA> KeysA => _dictA.Keys;
        public ICollection<TKeyB> KeysB => _dictB.Keys;
        public ICollection<TValue> Values => _dictB.Values;
        public void Add(TKeyA keyA, TKeyB keyB, TValue value)
        {
            if (_dictB.ContainsKey(keyB))
                throw new ArgumentException("An item with the same unique key (KeyB) already exists.", nameof(keyB));

            _dictB.Add(keyB, value);
            _keyBToKeyAMap.Add(keyB, keyA);

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
        public bool TryGetValue(TKeyB keyB, [MaybeNullWhen(false)] out TValue value) => _dictB.TryGetValue(keyB, out value);
        public bool Removes(TKeyA keyA)
        {
            if (!_dictA.TryGetValue(keyA, out List<TValue>? valuesToRemove)) return false;

            List<TKeyB> keysBToRemove = [.. _keyBToKeyAMap
                .Where(kvp => kvp.Value.Equals(keyA))
                .Select(kvp => kvp.Key)];

            foreach (TKeyB keyB in keysBToRemove)
            {
                _dictB.Remove(keyB);
                _keyBToKeyAMap.Remove(keyB);
            }

            _dictA.Remove(keyA);

            return true;
        }
        public bool Remove(TKeyB keyB)
        {
            if (!_dictB.TryGetValue(keyB, out TValue? valueToRemove) || !_keyBToKeyAMap.TryGetValue(keyB, out TKeyA? keyA)) return false;

            _dictB.Remove(keyB);
            _keyBToKeyAMap.Remove(keyB);

            if (_dictA.TryGetValue(keyA, out List<TValue>? list))
            {
                list.Remove(valueToRemove);
                if (list.Count == 0)
                    _dictA.Remove(keyA);
            }

            return true;
        }
        public bool ContainsList(TKeyA keyA) => _dictA.ContainsKey(keyA);
        public bool Contains(TKeyB keyB) => _dictB.ContainsKey(keyB);
        public void Clear()
        {
            _dictA.Clear();
            _dictB.Clear();
            _keyBToKeyAMap.Clear();
        }
    }
}
