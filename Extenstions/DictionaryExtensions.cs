using Frontend.Models.Converted;
using System.Diagnostics.CodeAnalysis;

namespace Frontend.Extenstions
{
    public static class DictionaryExtensions
    {
        public static bool TryGetInstance<TKey>(this Dictionary<TKey, List<EntityInstance>> dict, Guid instanceId, [NotNullWhen(true)] out EntityInstance? instance) where TKey : notnull
        {
            instance = dict.Values.SelectMany(l => l).FirstOrDefault(i => i.Id == instanceId);
            return instance != null;
        }
    }
}
