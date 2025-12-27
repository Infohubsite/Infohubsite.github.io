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
        public async Task<Result<LoginResponse>> Login(string username, string password) => await HandleAsync(SendResult<LoginResponse, LoginResponseDto>, new(HttpMethod.Post, "/Auth/Login")
        {
            Content = JsonContent.Create(new { username, password })
        }, "Error while logging in.", (ex) => _logger.LogError(ex, "Could not log in."));
        public async Task<Result<LoginResponse>> Renew() => await HandleAsync(SendResult<LoginResponse, LoginResponseDto>, new(HttpMethod.Get, "/Auth/Renew"), "Error while renewing auth token.", (ex) => _logger.LogError(ex, "Could not renew auth token."));
        #endregion
        #region ADMIN
        public async Task<Result> RefreshCredentials() => await HandleAsync(SendResult, new(HttpMethod.Post, "/Admin/RefreshCredentials"), "Error while refreshing credentials.", (ex) => _logger.LogError(ex, "Could not refresh credentials."));
        #endregion
        #region ENTITY DEFINITIONS
        public async Task<Result<EntityDefinition>> CreateEntityDefinition(CreateEntityDefinitionDto newEntity) => await HandleAsync(SendResult<EntityDefinition, EntityDefinitionDto>, new(HttpMethod.Post, "/EntityDefinitions")
        {
            Content = JsonContent.Create(newEntity)
        }, "Error while creating entity definition.", (ex) => _logger.LogError(ex, "Could not create entity definition."));
        public async Task<Result> DeleteEntityDefinition(Guid entityId) => await HandleAsync(SendResult, new(HttpMethod.Delete, $"/EntityDefinitions/{entityId}"), $"Error while deleting entity definition '{entityId}'.", (ex) => _logger.LogError(ex, "Could not delete entity definition with ID '{entityId}'.", entityId));
        public async Task<Result<List<EntityDefinition>>> GetEntityDefinitions() => await HandleAsync(SendResultList<EntityDefinition, EntityDefinitionDto>, new(HttpMethod.Get, "/EntityDefinitions"), "Error while fetching entity definitions.", (ex) => _logger.LogError(ex, "Could not fetch entity definitions."));
        public async Task<Result<EntityDefinition>> GetEntityDefinition(Guid entityId) => await HandleAsync(SendResult<EntityDefinition, EntityDefinitionDto>, new(HttpMethod.Get, $"/EntityDefinitions/{entityId}"), $"Error while fetching entity definition '{entityId}'.", (ex) => _logger.LogError(ex, "Could not fetch entity definition with ID '{entityId}'.", entityId));
        public async Task<Result> UpdateEntityDefinition(Guid entityId, UpdateEntityDefinitionDto updateEntity) => await HandleAsync(SendResult, new(HttpMethod.Put, $"/EntityDefinitions/{entityId}")
        {
            Content = JsonContent.Create(updateEntity)
        }, $"Error while updating entity definition '{entityId}'.", (ex) => _logger.LogError(ex, "Could not update entity definition with ID '{entityId}'.", entityId));
        #endregion
        #region ENTITY INSTANCES
        public async Task<Result<EntityInstance>> CreateInstance(Guid entityId, CreateInstanceDto newInstance) => await HandleAsync(SendResult<EntityInstance, EntityInstanceDto>, new(HttpMethod.Post, $"/Instances/{entityId}")
        {
            Content = JsonContent.Create(newInstance)
        }, $"Error while creating instance for entity '{entityId}'.", (ex) => _logger.LogError(ex, "Could not create instance for entity '{entityId}'.", entityId));
        public async Task<Result> DeleteInstance(Guid instanceId, bool force = false) => await HandleAsync(SendResult, new(HttpMethod.Delete, $"/Instances/{instanceId}{(force ? "?force=true" : "")}"), $"Error while deleting instance '{instanceId}'.", (ex) => _logger.LogError(ex, "Could not delete instance with ID '{instanceId}'.", instanceId));
        public async Task<Result<EntityInstance>> GetInstance(Guid instanceId) => await HandleAsync(SendResult<EntityInstance, EntityInstanceDto>, new(HttpMethod.Get, $"/Instances/Instance/{instanceId}"), $"Error while fetching instance '{instanceId}'.", (ex) => _logger.LogError(ex, "Could not fetch instance with ID '{instanceId}'.", instanceId));
        public async Task<Result<List<EntityInstance>>> GetInstances(Guid entityId) => await HandleAsync(SendResultList<EntityInstance, EntityInstanceDto>, new(HttpMethod.Get, $"/Instances/{entityId}"), $"Error while fetching instances for entity '{entityId}'.", (ex) => _logger.LogError(ex, "Could not fetch instances for entity with ID '{entityId}'.", entityId));
        public async Task<Result> UpdateInstance(Guid instanceId, UpdateInstanceDto updateInstance) => await HandleAsync(SendResult, new(HttpMethod.Put, $"/Instances/{instanceId}")
        {
            Content = JsonContent.Create(updateInstance)
        }, $"Error while updating instance '{instanceId}'.", (ex) => _logger.LogError(ex, "Could not update instance with ID '{instanceId}'.", instanceId));
        #endregion
        #region FILES
        public async Task<Result<KoofrDownloadDto>> GetDownload(string fileId) => await HandleAsync(SendResult<KoofrDownloadDto>, new(HttpMethod.Get, $"Files/{fileId}"), $"Error while fetching download token for fileId '{fileId}'.", (ex) => _logger.LogError(ex, "Could not fetch download token for fileId '{fileId}'.", fileId));
        public async Task<Result<KoofrUploadDto>> GetUpload() => await HandleAsync(SendResult<KoofrUploadDto>, new(HttpMethod.Get, "Files"), "Error while fetching upload token.", (ex) => _logger.LogError(ex, "Could not fetch upload token."));
        #endregion
    }
}
