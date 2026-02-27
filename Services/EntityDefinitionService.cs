using Frontend.Common;
using Frontend.Extenstions;
using Frontend.HttpClients;
using Frontend.Models.Converted;
using Frontend.Models.Interactive;
using Shared.DTO.Client;

namespace Frontend.Services
{
    public interface IEntityDefinitionService
    {
        Task<Result<List<EntityDefinition>>> GetEntityDefinitionsAsync(bool reload = false);
        Task<Result<EntityDefinition>> GetEntityDefinitionAsync(Guid entityId, bool reload = false);
        Task<Result<EntityDefinition>> CreateEntityAsync(EntityDefinitionI newEntity);
        Task<Result> DeleteEntityAsync(Guid entityId);
        Task<Result> UpdateEntityAsync(Guid entityId, EntityDefinitionI updateEntity);
    }

    public class EntityDefinitionService(DefaultClient client) : IEntityDefinitionService
    {
        private readonly DefaultClient Client = client;

        public Task<Result<List<EntityDefinition>>> GetEntityDefinitionsAsync(bool _) => Client.GetEntityDefinitions();
        public Task<Result<EntityDefinition>> GetEntityDefinitionAsync(Guid entityId, bool _) => Client.GetEntityDefinition(entityId);
        public Task<Result<EntityDefinition>> CreateEntityAsync(EntityDefinitionI newEntity) => Client.CreateEntityDefinition(newEntity.ConvertTo<CreateEntityDefinitionDto, EntityDefinitionI>());
        public Task<Result> DeleteEntityAsync(Guid entityId) => Client.DeleteEntityDefinition(entityId);
        public Task<Result> UpdateEntityAsync(Guid entityId, EntityDefinitionI updateEntity) => Client.UpdateEntityDefinition(entityId, updateEntity.ConvertTo<UpdateEntityDefinitionDto, EntityDefinitionI>());
    }

    public class CacheEntityDefinitionService(IEntityDefinitionService eds, ICacheService cs) : IEntityDefinitionService
    {
        private bool First = true;
        private readonly IEntityDefinitionService EDS = eds;
        private readonly ICacheService CS = cs;

        private Task<Result<List<EntityDefinition>>>? _loadingTask;

        public async Task<Result<List<EntityDefinition>>> GetEntityDefinitionsAsync(bool reload = false)
        {
            if (_loadingTask != null)
                return await _loadingTask;

            if (!reload && !First)
                return Result<List<EntityDefinition>>.Success([.. CS.GetDefinitions().OrderBy(i => i.Name)]);

            First = false;
            _loadingTask = new Func<Task<Result<List<EntityDefinition>>>>(async () => {
                Result<List<EntityDefinition>> result = await EDS.GetEntityDefinitionsAsync();
                if (result.IsSuccess)
                {
                    CS.RemoveDefinitions();
                    CS.UpsertDefinitions(result.Value);
                }
                return result;
            })();

            try
            {
                return await _loadingTask;
            }
            finally
            {
                _loadingTask = null;
            }
        }

        public async Task<Result<EntityDefinition>> GetEntityDefinitionAsync(Guid entityId, bool reload = false)
        {
            if (!reload && CS.TryGetDefinition(entityId, out EntityDefinition? value))
                return Result<EntityDefinition>.Success(value);
            Result<EntityDefinition> result = await EDS.GetEntityDefinitionAsync(entityId);
            if (result.IsSuccess)
                CS.UpsertDefinition(result.Value);
            return result;
        }
        public async Task<Result<EntityDefinition>> CreateEntityAsync(EntityDefinitionI newEntity)
        {
            Result<EntityDefinition> result = await EDS.CreateEntityAsync(newEntity);
            if (result.IsSuccess)
                CS.UpsertDefinition(result.Value);
            return result;
        }
        public async Task<Result> DeleteEntityAsync(Guid entityId)
        {
            Result result = await EDS.DeleteEntityAsync(entityId);
            if (result.IsSuccess)
                CS.RemoveDefinition(entityId);
            return result;
        }
        public async Task<Result> UpdateEntityAsync(Guid entityId, EntityDefinitionI updateEntity)
        {
            Result result = await EDS.UpdateEntityAsync(entityId, updateEntity);
            if (result.IsSuccess)
                if (CS.TryGetDefinition(entityId, out EntityDefinition? entity)) entity.Name = updateEntity.Name;
                else await GetEntityDefinitionAsync(entityId, true);

            return result;
        }
    }
}
