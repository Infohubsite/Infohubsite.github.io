using Frontend.Models.Converted;
using Microsoft.AspNetCore.WebUtilities;
using System.Diagnostics.CodeAnalysis;

namespace Frontend.Services
{
    public interface ICacheService
    {
        void UpsertDefinition(EntityDefinition definition);
        void UpsertDefinitions(IEnumerable<EntityDefinition> definitions);
        bool RemoveDefinition(Guid definitionId);
        void RemoveDefinitions();
        IEnumerable<EntityDefinition> GetDefinitions();
        bool TryGetDefinition(Guid definitionId, [MaybeNullWhen(false)] out EntityDefinition definition);


        void AddInstance(EntityInstance instance);
        void AddInstances(IEnumerable<EntityInstance> instances);
        bool RemoveInstance(Guid instanceId);
        bool RemoveInstances(Guid definitionId);
        IEnumerable<EntityInstance> GetInstances(Guid definitionId);
        bool TryGetInstance(Guid instanceId, [MaybeNullWhen(false)] out EntityInstance instance);
    }
    public class CacheService : ICacheService
    {
        private readonly Dictionary<Guid, EntityDefinition> _definitions = [];
        private readonly GroupedCache<Guid, Guid, EntityInstance> _instances = new((i) => i.EntityDefinitionId, (i) => i.Id);

        public void UpsertDefinition(EntityDefinition definition) => _definitions[definition.Id] = definition;
        public void UpsertDefinitions(IEnumerable<EntityDefinition> definitions)
        {
            foreach (EntityDefinition def in definitions)
                UpsertDefinition(def);
        }
        public bool RemoveDefinition(Guid definitionId)
        {
            bool rem = _definitions.Remove(definitionId);
            if (rem)
                _instances.RemoveGroup(definitionId);
            return rem;
        }
        public void RemoveDefinitions() => _definitions.Clear();
        public IEnumerable<EntityDefinition> GetDefinitions() => _definitions.Values;
        public bool TryGetDefinition(Guid definitionId, [MaybeNullWhen(false)] out EntityDefinition definition) => _definitions.TryGetValue(definitionId, out definition);

        public void AddInstance(EntityInstance instance) => _instances.Add(instance);
        public void AddInstances(IEnumerable<EntityInstance> instances)
        {
            foreach (EntityInstance instance in instances)
                AddInstance(instance);
        }
        public bool RemoveInstance(Guid instanceId) => _instances.Remove(instanceId);
        public bool RemoveInstances(Guid definitionId) => _instances.RemoveGroup(definitionId);
        public IEnumerable<EntityInstance> GetInstances(Guid id) => _instances.GetGroup(id);
        public bool TryGetInstance(Guid instanceId, [MaybeNullWhen(false)] out EntityInstance instance) => _instances.TryGetValue(instanceId, out instance);
    }

    public class GroupedCache<TGroupKey, TKey, TValue>(Func<TValue, TGroupKey> groupKeySelector, Func<TValue, TKey> keySelector) where TGroupKey : notnull where TKey : notnull
    {
        private readonly Dictionary<TGroupKey, HashSet<TKey>> _groups = [];
        private readonly Dictionary<TKey, TValue> _items = [];

        private readonly Func<TValue, TGroupKey> _groupKeySelector = groupKeySelector;
        private readonly Func<TValue, TKey> _keySelector = keySelector;

        public int Count => _items.Count;
        public ICollection<TKey> Keys => _items.Keys;
        public ICollection<TValue> Values => _items.Values;

        public void Add(TValue value)
        {
            TKey key = _keySelector(value);
            TGroupKey groupKey = _groupKeySelector(value);

            _items.Add(key, value);

            if (!_groups.TryGetValue(groupKey, out HashSet<TKey>? groupList))
            {
                groupList = [];
                _groups.Add(groupKey, groupList);
            }
            groupList.Add(key);
        }
        public void AddRange(IEnumerable<TValue> values)
        {
            foreach (TValue value in values)
                Add(value);
        }

        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) => _items.TryGetValue(key, out value);
        public IEnumerable<TValue> GetGroup(TGroupKey groupKey)
        {
            if (_groups.TryGetValue(groupKey, out HashSet<TKey>? keys))
                foreach (var key in keys)
                    if (_items.TryGetValue(key, out TValue? val))
                        yield return val;
        }
        public bool Remove(TKey key)
        {
            if (!_items.TryGetValue(key, out TValue? value)) return false;

            _items.Remove(key);

            TGroupKey groupKey = _groupKeySelector(value);
            if (_groups.TryGetValue(groupKey, out HashSet<TKey>? groupList))
            {
                groupList.Remove(key);
                if (groupList.Count == 0)
                    _groups.Remove(groupKey);
            }

            return true;
        }
        public bool RemoveGroup(TGroupKey groupKey)
        {
            if (!_groups.TryGetValue(groupKey, out HashSet<TKey>? keysToRemove)) return false;

            foreach (TKey key in keysToRemove)
                _items.Remove(key);

            _groups.Remove(groupKey);
            return true;
        }
        public void Clear()
        {
            _groups.Clear();
            _items.Clear();
        }
    }
}
