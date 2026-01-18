using Frontend.Common;
using Frontend.Models.Converted;
using Frontend.Services;
using Shared.DTO.Client;
using Shared.DTO.Server;
using System.Net.Http.Json;

namespace Frontend.HttpClients
{
    public class DefaultClient(HttpClient httpClient, ILogger<DefaultClient> logger, INotificationService notifs) : Client<DefaultClient>(httpClient, logger, notifs)
    {
        #region ACCOUNT
        public Task<Result<LoginResponse>> Login(string username, string password) => HandleAsync(SendResult<LoginResponse, LoginResponseDto>, new(HttpMethod.Post, "/Auth/Login")
        {
            Content = JsonContent.Create(new { username, password })
        }, "Error while logging in.", (ex) => _logger.LogError(ex, "Could not log in."));
        public Task<Result<LoginResponse>> Renew() => HandleAsync(SendResult<LoginResponse, LoginResponseDto>, new(HttpMethod.Get, "/Auth/Renew"), "Error while renewing auth token.", (ex) => _logger.LogError(ex, "Could not renew auth token."));
        #endregion
        #region ADMIN
        public Task<Result> RefreshCredentials() => HandleAsync(SendResult, new(HttpMethod.Post, "/Admin/RefreshCredentials"), "Error while refreshing credentials.", (ex) => _logger.LogError(ex, "Could not refresh credentials."));
        #endregion
        #region ENTITY DEFINITIONS
        public Task<Result<EntityDefinition>> CreateEntityDefinition(CreateEntityDefinitionDto newEntity) => HandleAsync(SendResult<EntityDefinition, EntityDefinitionDto>, new(HttpMethod.Post, "/EntityDefinitions")
        {
            Content = JsonContent.Create(newEntity)
        }, "Error while creating entity definition.", (ex) => _logger.LogError(ex, "Could not create entity definition."));
        public Task<Result> DeleteEntityDefinition(Guid entityId) => HandleAsync(SendResult, new(HttpMethod.Delete, $"/EntityDefinitions/{entityId}"), $"Error while deleting entity definition '{entityId}'.", (ex) => _logger.LogError(ex, "Could not delete entity definition with ID '{entityId}'.", entityId));
        public Task<Result<List<EntityDefinition>>> GetEntityDefinitions() => HandleAsync(SendResultList<EntityDefinition, EntityDefinitionDto>, new(HttpMethod.Get, "/EntityDefinitions"), "Error while fetching entity definitions.", (ex) => _logger.LogError(ex, "Could not fetch entity definitions."));
        public Task<Result<EntityDefinition>> GetEntityDefinition(Guid entityId) => HandleAsync(SendResult<EntityDefinition, EntityDefinitionDto>, new(HttpMethod.Get, $"/EntityDefinitions/{entityId}"), $"Error while fetching entity definition '{entityId}'.", (ex) => _logger.LogError(ex, "Could not fetch entity definition with ID '{entityId}'.", entityId));
        public Task<Result> UpdateEntityDefinition(Guid entityId, UpdateEntityDefinitionDto updateEntity) => HandleAsync(SendResult, new(HttpMethod.Put, $"/EntityDefinitions/{entityId}")
        {
            Content = JsonContent.Create(updateEntity)
        }, $"Error while updating entity definition '{entityId}'.", (ex) => _logger.LogError(ex, "Could not update entity definition with ID '{entityId}'.", entityId));
        #endregion
        #region ENTITY INSTANCES
        public Task<Result<EntityInstance>> CreateInstance(Guid entityId, CreateInstanceDto newInstance) => HandleAsync(SendResult<EntityInstance, EntityInstanceDto>, new(HttpMethod.Post, $"/Instances/{entityId}")
        {
            Content = JsonContent.Create(newInstance)
        }, $"Error while creating instance for entity '{entityId}'.", (ex) => _logger.LogError(ex, "Could not create instance for entity '{entityId}'.", entityId));
        public Task<Result> DeleteInstance(Guid instanceId, bool force = false) => HandleAsync(SendResult, new(HttpMethod.Delete, $"/Instances/{instanceId}{(force ? "?force=true" : "")}"), $"Error while deleting instance '{instanceId}'.", (ex) => _logger.LogError(ex, "Could not delete instance with ID '{instanceId}'.", instanceId));
        public Task<Result<EntityInstance>> GetInstance(Guid instanceId) => HandleAsync(SendResult<EntityInstance, EntityInstanceDto>, new(HttpMethod.Get, $"/Instances/Instance/{instanceId}"), $"Error while fetching instance '{instanceId}'.", (ex) => _logger.LogError(ex, "Could not fetch instance with ID '{instanceId}'.", instanceId));
        public Task<Result<List<EntityInstance>>> GetInstances(Guid entityId) => HandleAsync(SendResultList<EntityInstance, EntityInstanceDto>, new(HttpMethod.Get, $"/Instances/{entityId}"), $"Error while fetching instances for entity '{entityId}'.", (ex) => _logger.LogError(ex, "Could not fetch instances for entity with ID '{entityId}'.", entityId));
        public Task<Result<EntityInstance>> UpdateInstance(Guid instanceId, UpdateInstanceDto updateInstance) => HandleAsync(SendResult<EntityInstance, EntityInstanceDto>, new(HttpMethod.Put, $"/Instances/{instanceId}")
        {
            Content = JsonContent.Create(updateInstance)
        }, $"Error while updating instance '{instanceId}'.", (ex) => _logger.LogError(ex, "Could not update instance with ID '{instanceId}'.", instanceId));
        #endregion
        #region FILES
        public Task<Result<KoofrDownloadDto>> GetDownload(string fileId) => HandleAsync(SendResult<KoofrDownloadDto>, new(HttpMethod.Get, $"Files/{fileId}"), $"Error while fetching download token for fileId '{fileId}'.", (ex) => _logger.LogError(ex, "Could not fetch download token for fileId '{fileId}'.", fileId));
        public Task<Result<KoofrUploadDto>> GetUpload() => HandleAsync(SendResult<KoofrUploadDto>, new(HttpMethod.Get, "Files"), "Error while fetching upload token.", (ex) => _logger.LogError(ex, "Could not fetch upload token."));
        #endregion
    }
}
