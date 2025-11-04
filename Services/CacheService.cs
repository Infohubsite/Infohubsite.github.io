using Frontend.Models.Converted;

namespace Frontend.Services
{
    public interface ICacheService
    {
        Dictionary<Guid, EntityDefinition> EntityDefinitionCache { get; set; }
        Dictionary<Guid, List<EntityInstance>> EntityInstancesCache { get; set; }
    }
    public class CacheService : ICacheService
    {
        public Dictionary<Guid, EntityDefinition> EntityDefinitionCache { get; set; } = [];
        public Dictionary<Guid, List<EntityInstance>> EntityInstancesCache { get; set; } = [];
    }
}
