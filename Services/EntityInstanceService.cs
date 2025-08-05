using Frontend.Common;
using Frontend.Component.User.Instances;
using Frontend.Extenstions;
using Frontend.Models.DTOs;
using System.Net;
using System.Net.Http.Json;
using static Frontend.Common.Result;

namespace Frontend.Services
{
    public interface IEntityInstanceService
    {
        Task<Result<List<EntityInstanceDto>?>> GetInstancesAsync(Guid entityId, bool refresh = false);
        Task<Result<EntityInstanceDto?>> GetInstanceAsync(Guid instanceId, bool refresh = false);
        Task<Result> DeleteInstanceAsync(Guid instanceId, bool force = false);
        Task<Result<EntityInstanceDto?>> CreateInstanceAsync(Guid entityId, CreateInstanceDto newInstance);
        Task<Result> UpdateInstanceAsync(Guid instanceId, UpdateInstanceDto updateDto);
    }

    public class EntityInstanceService(IHttpClientFactory httpClientFactory, ILogger<EntityInstanceService> logger, INotificationService notifs) : IEntityInstanceService
    {
        private readonly HttpClient _httpClient = httpClientFactory.CreateClient("Default");
        private readonly ILogger<EntityInstanceService> _logger = logger;
        private readonly INotificationService _notifs = notifs;

        public async Task<Result<List<EntityInstanceDto>?>> GetInstancesAsync(Guid entityId, bool refresh = false)
        {
            try
            {
                HttpResponseMessage response = await this._httpClient.GetAsync($"/Instances/{entityId}"/*?limit={limit}&offset={offset}"*/);
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    this._logger.LogWarning("Tried to fetch entity instances for nonexistant entity definition");
                    return Fail(response.StatusCode, "Entity not found");
                }

                return await From<List<EntityInstanceDto>>(response);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Could not fetch instances for entity '{EntityId}'", entityId);
                await this._notifs.Show($"Error while fetching instances for entity '{entityId}' Error: {ex}");
                return Fail(ex.Message);
            }
        }
        public async Task<Result<EntityInstanceDto?>> GetInstanceAsync(Guid instanceId, bool refresh = false)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"/Instances/Instance/{instanceId}");
                return await From<EntityInstanceDto>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not fetch instance with ID '{InstanceId}'", instanceId);
                await _notifs.Show($"Error while fetching instance '{instanceId}'. Error: {ex.Message}");
                return Fail(ex.Message);
            }
        }
        public async Task<Result> DeleteInstanceAsync(Guid instanceId, bool force = false)
        {
            try
            {
                return await From(await _httpClient.DeleteAsync($"/Instances/{instanceId}{(force ? "?force=true" : "")}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not delete instance with id {Id}.", instanceId);
                await _notifs.Show($"Error while deleting instance '{instanceId}'. Error: {ex.Message}");
                return Fail(ex.Message);
            }
        }
        public async Task<Result<EntityInstanceDto?>> CreateInstanceAsync(Guid entityId, CreateInstanceDto newInstance)
        {
            try
            {
                return await From<EntityInstanceDto>(await _httpClient.PostAsJsonAsync($"/Instances/{entityId}", newInstance));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not create instance for entity '{EntityId}'.", entityId);
                await _notifs.Show($"Error while creating instance for entity '{entityId}'. Error: {ex.Message}");
                return Fail(ex.Message);
            }
        }
        public async Task<Result> UpdateInstanceAsync(Guid instanceId, UpdateInstanceDto updateDto)
        {
            try
            {
                return await From(await _httpClient.PutAsJsonAsync($"/Instances/{instanceId}", updateDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not update instance with id '{Id}'.", instanceId);
                await _notifs.Show($"Error while updating instance '{instanceId}'. Error: {ex.Message}");
                return Fail(ex.Message);
            }
        }
    }

    public class CacheEntityInstanceService(IEntityInstanceService eis, ICacheService cs) : IEntityInstanceService
    {
        private readonly IEntityInstanceService EIS = eis;
        private readonly ICacheService CS = cs;

        public async Task<Result<List<EntityInstanceDto>?>> GetInstancesAsync(Guid entityId, bool refresh = false)
        {
            if (!refresh && CS.EntityInstancesCache.TryGetValue(entityId, out List<EntityInstanceDto>? value))
                return Success<List<EntityInstanceDto>?>([.. value.OrderBy(i => i.Id)]);
            Result<List<EntityInstanceDto>?> result = await EIS.GetInstancesAsync(entityId);
            if (result.Value != null) CS.EntityInstancesCache[entityId] = result.Value;
            return result;
        }
        public async Task<Result<EntityInstanceDto?>> GetInstanceAsync(Guid instanceId, bool refresh = false)
        {
            if (!refresh && CS.EntityInstancesCache.TryGetInstance(instanceId, out EntityInstanceDto? instance))
                return Success<EntityInstanceDto?>(instance);

            Result<EntityInstanceDto?> result = await EIS.GetInstanceAsync(instanceId);
            if (result.Value != null && CS.EntityInstancesCache.TryGetValue(result.Value.EntityDefinitionId, out List<EntityInstanceDto>? instances))
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
                KeyValuePair<Guid, List<EntityInstanceDto>> entry = CS.EntityInstancesCache.FirstOrDefault(kvp => kvp.Value.Any(instance => instance.Id == instanceId));
                if (entry.Value != null)
                {
                    entry.Value.RemoveAll(instance => instance.Id == instanceId);
                    if (entry.Value.Count == 0)
                        CS.EntityInstancesCache.Remove(entry.Key);
                }
            }
            return result;
        }
        public async Task<Result<EntityInstanceDto?>> CreateInstanceAsync(Guid entityId, CreateInstanceDto newInstance)
        {
            Result<EntityInstanceDto?> result = await EIS.CreateInstanceAsync(entityId, newInstance);
            if (result.Value != null)
                if (CS.EntityInstancesCache.TryGetValue(entityId, out var instanceList))
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
                EntityInstanceDto? instance = CS.EntityInstancesCache.Values.SelectMany(l => l).FirstOrDefault(i => i.Id == instanceId);
                if (instance != null) instance.Data = updateDto.Data.Where(kvp => kvp.Value != null).ToDictionary(kvp => kvp.Key, kvp => kvp.Value!);
            }
            return result;
        }
    }
}
