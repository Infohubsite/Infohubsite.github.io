using Frontend.Models.DTOs;

namespace Frontend.Services
{
    public interface ICacheService
    {
        Dictionary<Guid, EntityDefinitionDto> EntityDefinitionCache { get; set; }
        Dictionary<Guid, List<EntityInstanceDto>> EntityInstancesCache { get; set; }
    }
    public class CacheService : ICacheService
    {
        public Dictionary<Guid, EntityDefinitionDto> EntityDefinitionCache { get; set; } = [];
        public Dictionary<Guid, List<EntityInstanceDto>> EntityInstancesCache { get; set; } = [];
    }
}
