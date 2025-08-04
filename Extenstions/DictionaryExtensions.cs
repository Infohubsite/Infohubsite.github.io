using Frontend.Models.DTOs;
using System.Diagnostics.CodeAnalysis;

namespace Frontend.Extenstions
{
    public static class DictionaryExtensions
    {
        public static bool TryGetInstance<TKey>(this Dictionary<TKey, List<EntityInstanceDto>> dict, Guid instanceId, [NotNullWhen(true)] out EntityInstanceDto? instance) where TKey : notnull
        {
            instance = dict.Values.SelectMany(l => l).FirstOrDefault(i => i.Id == instanceId);
            return instance != null;
        }
    }
}
