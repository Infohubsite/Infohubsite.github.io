using Frontend.Models.DTOs;
using System.Net.Http.Json;

namespace Frontend.Services
{
    public interface IEntityDefinitionService
    {
        Task<List<EntityDefinitionDto>?> GetEntityDefinitionsAsync(bool reload = false);
        Task<EntityDefinitionDto?> GetEntityDefinitionAsync(Guid entityId, bool reload = false);
        Task<EntityDefinitionDto?> CreateEntityAsync(CreateEntityWithFieldsDto newEntity);
        Task<bool> DeleteEntityAsync(Guid id);
        Task<bool> UpdateEntityAsync(Guid id, string newName);
    }

    public class EntityDefinitionService(IHttpClientFactory httpClientFactory, ILogger<EntityDefinitionService> logger, INotificationService notificationService) : IEntityDefinitionService
    {
        private readonly HttpClient _httpClient = httpClientFactory.CreateClient("Default");
        private readonly ILogger<EntityDefinitionService> _logger = logger;
        private readonly INotificationService _notificationService = notificationService;

        public async Task<List<EntityDefinitionDto>?> GetEntityDefinitionsAsync(bool reload = false)
        {
            try
            {
                HttpResponseMessage response = await this._httpClient.GetAsync("/EntityDefinitions");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<EntityDefinitionDto>>();
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Could not fetch entity definitions");
                await this._notificationService.Show($"Error while getting entity definitions: {ex}");
                return null;
            }
        }
        public async Task<EntityDefinitionDto?> GetEntityDefinitionAsync(Guid entityId, bool reload = false)
        {
            try
            {
                HttpResponseMessage response = await this._httpClient.GetAsync($"/EntityDefinitions/{entityId}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<EntityDefinitionDto>();
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Could not fetch entity definition");
                await this._notificationService.Show($"Error while getting entity definition: {ex}");
                return null;
            }
        }
        public async Task<EntityDefinitionDto?> CreateEntityAsync(CreateEntityWithFieldsDto newEntity)
        {
            try
            {
                HttpResponseMessage response = await this._httpClient.PostAsJsonAsync("/EntityDefinitions", newEntity);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<EntityDefinitionDto>();
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Could not create entity definition '{Name}'", newEntity.Name);
                await this._notificationService.Show($"Error while creating new entity definition: {ex}");
                return null;
            }
        }
        public async Task<bool> DeleteEntityAsync(Guid id)
        {
            try
            {
                HttpResponseMessage response = await this._httpClient.DeleteAsync($"/EntityDefinitions/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Could not delete entity definition with Id '{Id}'", id);
                await this._notificationService.Show($"Error while deleting entity with Id '{id}': {ex}");
                return false;
            }
        }
        public async Task<bool> UpdateEntityAsync(Guid id, string newName)
        {
            try
            {
                HttpResponseMessage response = await this._httpClient.PutAsJsonAsync($"/EntityDefinitions/{id}", new { name = newName });
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Could not update entity definition with Id '{Id}'", id);
                await this._notificationService.Show($"Error while updating entity with Id '{id}': {ex}");
                return false;
            }
        }
    }

    public class CacheEntityDefinitionService(IEntityDefinitionService eds, ICacheService cs) : IEntityDefinitionService
    {
        private readonly IEntityDefinitionService EDS = eds;
        private readonly ICacheService CS = cs;

        public async Task<List<EntityDefinitionDto>?> GetEntityDefinitionsAsync(bool reload = false)
        {
            if (!reload && CS.EntityDefinitionCache.Count > 1)
                return [.. CS.EntityDefinitionCache.Values];
            List<EntityDefinitionDto>? entities = await EDS.GetEntityDefinitionsAsync();
            if (entities != null)
                CS.EntityDefinitionCache = entities.ToDictionary(e => e.Id, e => e);
            return entities;
        }
        public async Task<EntityDefinitionDto?> GetEntityDefinitionAsync(Guid entityId, bool reload = false)
        {
            if (!reload && CS.EntityDefinitionCache.TryGetValue(entityId, out EntityDefinitionDto? value))
                return value;
            EntityDefinitionDto? entity = await EDS.GetEntityDefinitionAsync(entityId);
            if (entity != null)
                CS.EntityDefinitionCache[entityId] = entity;
            return entity;
        }
        public async Task<EntityDefinitionDto?> CreateEntityAsync(CreateEntityWithFieldsDto newEntity)
        {
            EntityDefinitionDto? entity = await EDS.CreateEntityAsync(newEntity);
            if (entity != null)
                CS.EntityDefinitionCache[entity.Id] = entity;
            return entity;
        }
        public async Task<bool> DeleteEntityAsync(Guid id)
        {
            bool success = await EDS.DeleteEntityAsync(id);
            if (success)
                CS.EntityDefinitionCache.Remove(id);
            return success;
        }
        public async Task<bool> UpdateEntityAsync(Guid id, string newName)
        {
            bool success = await EDS.UpdateEntityAsync(id, newName);
            if (success)
                if (CS.EntityDefinitionCache.TryGetValue(id, out EntityDefinitionDto? entity))
                    entity.Name = newName;
                else
                    await GetEntityDefinitionAsync(id, true);

            return success;
        }
    }
}
