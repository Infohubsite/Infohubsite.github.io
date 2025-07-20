using Frontend.Component.User.Instances;
using Frontend.Models.DTOs;
using Microsoft.AspNetCore.WebUtilities;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;

namespace Frontend.Services
{
    public interface IEntityInstanceService
    {
        Task<List<EntityInstanceDto>?> GetInstancesAsync(Guid entityId, bool refresh = false);
        Task<EntityInstanceDto?> GetInstanceAsync(Guid instanceId, bool refresh = false);
        Task<(bool Success, HttpResponseMessage? Response)> DeleteInstanceAsync(Guid instanceId, bool force = false);
        Task<(EntityInstanceDto? Instance, HttpResponseMessage? Response)> CreateInstanceAsync(Guid entityId, CreateInstanceDto newInstance);
    }

    public class EntityInstanceService(IHttpClientFactory httpClientFactory, ILogger<EntityInstanceService> logger, INotificationService notifs) : IEntityInstanceService
    {
        private readonly HttpClient _httpClient = httpClientFactory.CreateClient("Default");
        private readonly ILogger<EntityInstanceService> _logger = logger;
        private readonly INotificationService _notifs = notifs;

        public async Task<List<EntityInstanceDto>?> GetInstancesAsync(Guid entityId, bool refresh = false)
        {
            try
            {
                HttpResponseMessage response = await this._httpClient.GetAsync($"/Instances/{entityId}");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<List<EntityInstanceDto>>();
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Could not fetch instances for entity '{EntityId}'", entityId);
                await this._notifs.Show($"Error while fetching instances for entity '{entityId}' Error: {ex}");
                return null;
            }
        }
        public async Task<EntityInstanceDto?> GetInstanceAsync(Guid instanceId, bool refresh = false)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"/Instances/Instance/{instanceId}");
                if (response.StatusCode == HttpStatusCode.NotFound)
                    return null;
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<EntityInstanceDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not fetch instance with ID '{InstanceId}'", instanceId);
                await _notifs.Show($"Error while fetching instance '{instanceId}'. Error: {ex.Message}");
                return null;
            }
        }
        public async Task<(bool Success, HttpResponseMessage? Response)> DeleteInstanceAsync(Guid instanceId, bool force = false)
        {
            try
            {
                string url = $"/Instances/{instanceId}";
                if (force)
                    url = QueryHelpers.AddQueryString(url, "force", "true");

                HttpResponseMessage response = await _httpClient.DeleteAsync(url);

                return (response.IsSuccessStatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not delete instance with id {Id}.", instanceId);
                await _notifs.Show($"Error while deleting instance '{instanceId}'. Error: {ex.Message}");
                return (false, null);
            }
        }
        public async Task<(EntityInstanceDto? Instance, HttpResponseMessage? Response)> CreateInstanceAsync(Guid entityId, CreateInstanceDto newInstance)
        {
            try
            {
                foreach (var a in newInstance.Data)
                    Console.WriteLine(a.ToString());
                HttpResponseMessage response = await _httpClient.PostAsJsonAsync($"instances/{entityId}", newInstance);
                Console.WriteLine(await response.Content.ReadAsStringAsync());
                if (!response.IsSuccessStatusCode)
                    return (null, response);

                return (await response.Content.ReadFromJsonAsync<EntityInstanceDto>(), null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not create instance for entity '{EntityId}'.", entityId);
                await _notifs.Show($"Error while creating instance for entity '{entityId}'. Error: {ex.Message}");
                return (null, null);
            }
        }
    }

    public class CacheEntityInstanceService(IEntityInstanceService eis, ICacheService cs) : IEntityInstanceService
    {
        private readonly IEntityInstanceService EIS = eis;
        private readonly ICacheService CS = cs;

        public async Task<List<EntityInstanceDto>?> GetInstancesAsync(Guid entityId, bool refresh = false)
        {
            if (!refresh && CS.EntityInstancesCache.TryGetValue(entityId, out List<EntityInstanceDto>? value))
                return value;
            List<EntityInstanceDto>? entities = await EIS.GetInstancesAsync(entityId);
            if (entities != null)
                CS.EntityInstancesCache[entityId] = entities;
            return entities;
        }
        public async Task<EntityInstanceDto?> GetInstanceAsync(Guid instanceId, bool refresh = false)
        {
            if (!refresh)
                foreach (var list in CS.EntityInstancesCache.Values)
                {
                    EntityInstanceDto? instance = list.FirstOrDefault(i => i.Id == instanceId);
                    if (instance != null)
                        return instance;
                }

            EntityInstanceDto? entity = await EIS.GetInstanceAsync(instanceId);
            if (entity != null && CS.EntityInstancesCache.TryGetValue(entity.EntityDefinitionId, out List<EntityInstanceDto>? instances))
            {
                instances.RemoveAll(i => i.Id == entity.Id);
                instances.Add(entity);
            }
            return entity;
        }
        public async Task<(bool Success, HttpResponseMessage? Response)> DeleteInstanceAsync(Guid instanceId, bool force = false)
        {
            (bool Success, HttpResponseMessage? StatusCode) result = await EIS.DeleteInstanceAsync(instanceId, force);
            if (result.Success)
            {
                var entry = CS.EntityInstancesCache.FirstOrDefault(kvp => kvp.Value.Any(instance => instance.Id == instanceId));
                if (entry.Value != null)
                {
                    entry.Value.RemoveAll(instance => instance.Id == instanceId);
                    if (entry.Value.Count == 0)
                        CS.EntityInstancesCache.Remove(entry.Key);
                }
            }
            return result;
        }
        public async Task<(EntityInstanceDto? Instance, HttpResponseMessage? Response)> CreateInstanceAsync(Guid entityId, CreateInstanceDto newInstance)
        {
            (EntityInstanceDto? Instance, HttpResponseMessage? StatusCode) result = await EIS.CreateInstanceAsync(entityId, newInstance);
            if (result.Instance != null)
                if (CS.EntityInstancesCache.TryGetValue(entityId, out var instanceList))
                    instanceList.Add(result.Instance);
                else
                    CS.EntityInstancesCache[entityId] = [result.Instance];
            return result;
        }
    }
}
