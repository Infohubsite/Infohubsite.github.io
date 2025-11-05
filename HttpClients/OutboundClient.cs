using Frontend.Services;

namespace Frontend.HttpClients
{
    public class OutboundClient(HttpClient httpClient, ILogger<OutboundClient> logger, INotificationService notifs) : Client<OutboundClient>(httpClient, logger, notifs)
    {
        // maybe remove later?
        // TODO
    }
}