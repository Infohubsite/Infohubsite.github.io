using Frontend.Common;
using Frontend.HttpClients;
using Frontend.Models.Converted;
using Shared.DTO.Client;

namespace Frontend.Services
{
    public abstract record LockContext
    {
        public abstract Guid InstanceId { get; init; }
    }

    public sealed record Lock(Guid InstanceId, Action<Result> Callback) : LockContext;
    public sealed record Unlock(Guid InstanceId) : LockContext;

    public interface IEntityInstanceService
    {
        Task<Result<List<EntityInstance>>> GetInstancesAsync(Guid entityId, bool refresh = false);
        Task<Result<EntityInstance>> GetInstanceAsync(Guid instanceId, bool refresh = false);
        Task<Result> DeleteInstanceAsync(Guid instanceId, int version, bool force = false);
        Task<Result<EntityInstance>> CreateInstanceAsync(Guid entityId, CreateInstanceDto newInstance);
        Task<Result<EntityInstance>> UpdateInstanceAsync(Guid instanceId, UpdateInstanceDto updateDto);
        Task<Result> LockInstance(LockContext context);
    }

    public class EntityInstanceService(DefaultClient client) : IEntityInstanceService
    {
        private readonly DefaultClient Client = client;

        public Task<Result<List<EntityInstance>>> GetInstancesAsync(Guid entityId, bool _) => Client.GetInstances(entityId);
        public Task<Result<EntityInstance>> GetInstanceAsync(Guid instanceId, bool _) => Client.GetInstance(instanceId);
        public Task<Result> DeleteInstanceAsync(Guid instanceId, int version, bool force = false) => Client.DeleteInstance(instanceId, version, force);
        public Task<Result<EntityInstance>> CreateInstanceAsync(Guid entityId, CreateInstanceDto newInstance) => Client.CreateInstance(entityId, newInstance);
        public Task<Result<EntityInstance>> UpdateInstanceAsync(Guid instanceId, UpdateInstanceDto updateDto) => Client.UpdateInstance(instanceId, updateDto);
        public Task<Result> LockInstance(LockContext context) => Client.LockInstance(context.InstanceId, context is Unlock);
    }

    public class CacheEntityInstanceService : IEntityInstanceService, IDisposable
    {
        private readonly IEntityInstanceService EIS;
        private readonly ICacheService CS;
        private readonly HashSet<Guid> Cached = [];

        public CacheEntityInstanceService(IEntityInstanceService eIS, ICacheService cS)
        {
            EIS = eIS;
            CS = cS;
            _heartbeat = HeartbeatLoop();
        }

        private Lock? _lockContext;
        private readonly CancellationTokenSource _cts = new();
        private readonly Task _heartbeat;
        private async Task HeartbeatLoop()
        {
            using PeriodicTimer timer = new(TimeSpan.FromSeconds(55));
            while (await timer.WaitForNextTickAsync(_cts.Token))
            {
                if (_lockContext == null) continue;

                Result result = await EIS.LockInstance(_lockContext);
                if (result.IsSuccess) continue;

                _lockContext.Callback(result);
            }
        }


        public async Task<Result<List<EntityInstance>>> GetInstancesAsync(Guid entityId, bool refresh = false)
        {
            if (!refresh && Cached.Contains(entityId))
                return Result<List<EntityInstance>>.Success([.. CS.GetInstances(entityId)]);
            Result<List<EntityInstance>> result = await EIS.GetInstancesAsync(entityId);
            CS.RemoveInstances(entityId);
            if (result.IsSuccess)
            {
                Cached.Add(entityId);
                CS.AddInstances(result.Value);
            }
            else
                Cached.Remove(entityId);
            return result;
        }
        public async Task<Result<EntityInstance>> GetInstanceAsync(Guid instanceId, bool refresh = false)
        {
            if (!refresh && CS.TryGetInstance(instanceId, out EntityInstance? instance))
                return Result<EntityInstance>.Success(instance);

            Result<EntityInstance> result = await EIS.GetInstanceAsync(instanceId);
            CS.RemoveInstance(instanceId);
            if (result.IsSuccess)
                CS.AddInstance(result.Value);
            return result;
        }
        public async Task<Result> DeleteInstanceAsync(Guid instanceId, int version, bool force = false)
        {
            Result result = await EIS.DeleteInstanceAsync(instanceId, version, force);
            if (result.IsSuccess)
                CS.RemoveInstance(instanceId);
            return result;
        }
        public async Task<Result<EntityInstance>> CreateInstanceAsync(Guid entityId, CreateInstanceDto newInstance)
        {
            Result<EntityInstance> result = await EIS.CreateInstanceAsync(entityId, newInstance);
            if (result.IsSuccess)
                CS.AddInstance(result.Value);
            return result;
        }
        public async Task<Result<EntityInstance>> UpdateInstanceAsync(Guid instanceId, UpdateInstanceDto updateDto)
        {
            Result<EntityInstance> result = await EIS.UpdateInstanceAsync(instanceId, updateDto);

            if (result.IsSuccess)
            {
                CS.RemoveInstance(instanceId);
                CS.AddInstance(result.Value);
                _lockContext = null; // Unlock after successful update
            }

            return result;
        }
        public async Task<Result> LockInstance(LockContext context)
        {
            Result result = await EIS.LockInstance(context);

            switch (context)
            {
                case Lock lockContext when result.IsSuccess:
                    _lockContext = lockContext;
                    break;
                case Unlock when result.IsSuccess || result.IsSuccess || result.StatusCode == System.Net.HttpStatusCode.Locked:
                    _lockContext = null;
                    break;
            }

            return result;
        }

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
