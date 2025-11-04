using Frontend.Models;
using Frontend.Services;
using System.Net.Http.Json;

namespace Frontend.HttpClients
{
    public class OriginClient(HttpClient httpClient, ILogger<OriginClient> logger, INotificationService notifs) : Client<OriginClient>(httpClient, logger, notifs)
    {
        public async Task<BuildInfo?> GetBuildInfo() => await _httpClient.GetFromJsonAsync<BuildInfo>($"build-info.json?v={DateTime.UtcNow.Ticks}");
    }
}
