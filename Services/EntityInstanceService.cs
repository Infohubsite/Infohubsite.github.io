using Frontend.Models.DTOs;
using Microsoft.AspNetCore.WebUtilities;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http.Json;

namespace Frontend.Services
{
    public interface IEntityInstanceService
    {
        Task<List<EntityInstanceDto>?> GetInstancesAsync(Guid entityId, bool refresh = false);
        Task<(bool Success, HttpStatusCode? StatusCode)> DeleteInstanceAsync(Guid instanceId, bool force = false);
        Task<(EntityInstanceDto? Instance, HttpStatusCode? StatusCode)> CreateInstanceAsync(Guid entityId, CreateInstanceDto newInstance);
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
        public async Task<(bool Success, HttpStatusCode? StatusCode)> DeleteInstanceAsync(Guid instanceId, bool force = false)
        {
            try
            {
                string url = $"/Instances/{instanceId}";
                if (force)
                    url = QueryHelpers.AddQueryString(url, "force", "true");

                HttpResponseMessage response = await _httpClient.DeleteAsync(url);

                return (response.IsSuccessStatusCode, response.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not delete instance with id {Id}.", instanceId);
                return (false, null);
            }
        }
        public async Task<(EntityInstanceDto? Instance, HttpStatusCode? StatusCode)> CreateInstanceAsync(Guid entityId, CreateInstanceDto newInstance)
        {
            try
            {
                foreach (var a in newInstance.Data)
                    Console.WriteLine(a.ToString());
                HttpResponseMessage response = await _httpClient.PostAsJsonAsync($"instances/{entityId}", newInstance);
                Console.WriteLine(await response.Content.ReadAsStringAsync());
                if (!response.IsSuccessStatusCode)
                    return (null, response.StatusCode);

                return (await response.Content.ReadFromJsonAsync<EntityInstanceDto>(), null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not create instance for entity '{EntityId}'.", entityId);
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
        public async Task<(bool Success, HttpStatusCode? StatusCode)> DeleteInstanceAsync(Guid instanceId, bool force = false)
        {
            (bool Success, HttpStatusCode? StatusCode) result = await EIS.DeleteInstanceAsync(instanceId, force);
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
        public async Task<(EntityInstanceDto? Instance, HttpStatusCode? StatusCode)> CreateInstanceAsync(Guid entityId, CreateInstanceDto newInstance)
        {
            (EntityInstanceDto? Instance, HttpStatusCode? StatusCode) result = await EIS.CreateInstanceAsync(entityId, newInstance);
            if (result.Instance != null)
                if (CS.EntityInstancesCache.TryGetValue(entityId, out var instanceList))
                    instanceList.Add(result.Instance);
                else
                    CS.EntityInstancesCache[entityId] = [result.Instance];
            return result;
        }
    }
}
