using Frontend.HttpClients;

namespace Frontend.Services
{
    public interface IAdminService
    {
        Task<bool> RefreshCredentials();
    }
    public class AdminService(DefaultClient client) : IAdminService
    {
        public readonly DefaultClient _client = client;

        public async Task<bool> RefreshCredentials() => (await _client.RefreshCredentials()).IsSuccess;
    }
}
