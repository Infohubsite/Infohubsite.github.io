using Frontend.Services;

namespace Frontend.HttpClients
{
    public class WakeupClient(HttpClient httpClient, ILogger<WakeupClient> logger, INotificationService notifs) : Client<WakeupClient>(httpClient, logger, notifs)
    {
        public async Task<HttpResponseMessage> Wakeup() => await this._httpClient.GetAsync("/Healthz");
    }
}
