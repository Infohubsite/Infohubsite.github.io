using Frontend.Common;
using Frontend.Extenstions;
using Frontend.HttpClients;
using Frontend.Models.Converted;
using Shared.DTO.Client;

namespace Frontend.Services
{
    public interface IEntityInstanceService
    {
        Task<Result<List<EntityInstance>>> GetInstancesAsync(Guid entityId, bool refresh = false);
        Task<Result<EntityInstance>> GetInstanceAsync(Guid instanceId, bool refresh = false);
        Task<Result> DeleteInstanceAsync(Guid instanceId, bool force = false);
        Task<Result<EntityInstance>> CreateInstanceAsync(Guid entityId, CreateInstanceDto newInstance);
        Task<Result> UpdateInstanceAsync(Guid instanceId, UpdateInstanceDto updateDto);
    }

    public class EntityInstanceService(DefaultClient client) : IEntityInstanceService
    {
        private readonly DefaultClient Client = client;

        public async Task<Result<List<EntityInstance>>> GetInstancesAsync(Guid entityId, bool _) => await Client.GetInstances(entityId);
        public async Task<Result<EntityInstance>> GetInstanceAsync(Guid instanceId, bool _) => await Client.GetInstance(instanceId);
        public async Task<Result> DeleteInstanceAsync(Guid instanceId, bool force = false) => await Client.DeleteInstance(instanceId, force);
        public async Task<Result<EntityInstance>> CreateInstanceAsync(Guid entityId, CreateInstanceDto newInstance) => await Client.CreateInstance(entityId, newInstance);
        public async Task<Result> UpdateInstanceAsync(Guid instanceId, UpdateInstanceDto updateDto) => await Client.UpdateInstance(instanceId, updateDto);
    }

    public class CacheEntityInstanceService(IEntityInstanceService eis, ICacheService cs) : IEntityInstanceService
    {
        private readonly IEntityInstanceService EIS = eis;
        private readonly ICacheService CS = cs;

        public async Task<Result<List<EntityInstance>>> GetInstancesAsync(Guid entityId, bool refresh = false)
        {
            if (!refresh && CS.EntityInstancesCache.TryGetValue(entityId, out List<EntityInstance>? value))
                return Result<List<EntityInstance>>.Success([.. value.OrderBy(i => i.Id)]);
            Result<List<EntityInstance>> result = await EIS.GetInstancesAsync(entityId);
            if (result.Value != null) CS.EntityInstancesCache[entityId] = result.Value;
            return result;
        }
        public async Task<Result<EntityInstance>> GetInstanceAsync(Guid instanceId, bool refresh = false)
        {
            if (!refresh && CS.EntityInstancesCache.TryGetInstance(instanceId, out EntityInstance? instance))
                return Result<EntityInstance>.Success(instance);

            Result<EntityInstance> result = await EIS.GetInstanceAsync(instanceId);
            if (result.Value != null && CS.EntityInstancesCache.TryGetValue(result.Value.EntityDefinitionId, out List<EntityInstance>? instances))
            {
                instances.RemoveAll(i => i.Id == result.Value.Id);
                instances.Add(result.Value);
            }
            return result;
        }
        public async Task<Result> DeleteInstanceAsync(Guid instanceId, bool force = false)
        {
            Result result = await EIS.DeleteInstanceAsync(instanceId, force);
            if (result.IsSuccess)
            {
                KeyValuePair<Guid, List<EntityInstance>> entry = CS.EntityInstancesCache.FirstOrDefault(kvp => kvp.Value.Any(instance => instance.Id == instanceId));
                if (entry.Value != null)
                {
                    entry.Value.RemoveAll(instance => instance.Id == instanceId);
                    if (entry.Value.Count == 0)
                        CS.EntityInstancesCache.Remove(entry.Key);
                }
            }
            return result;
        }
        public async Task<Result<EntityInstance>> CreateInstanceAsync(Guid entityId, CreateInstanceDto newInstance)
        {
            Result<EntityInstance> result = await EIS.CreateInstanceAsync(entityId, newInstance);
            if (result.Value != null)
                if (CS.EntityInstancesCache.TryGetValue(entityId, out List<EntityInstance>? instanceList))
                    instanceList.Add(result.Value);
                else
                    CS.EntityInstancesCache[entityId] = [result.Value];
            return result;
        }
        public async Task<Result> UpdateInstanceAsync(Guid instanceId, UpdateInstanceDto updateDto)
        {
            Result result = await EIS.UpdateInstanceAsync(instanceId, updateDto);
            if (result.IsSuccess)
            {
                EntityInstance? instance = CS.EntityInstancesCache.Values.SelectMany(l => l).FirstOrDefault(i => i.Id == instanceId);
                if (instance != null) instance.Data = updateDto.Data.Where(kvp => kvp.Value != null).ToDictionary();
            }
            return result;
        }
    }
}
