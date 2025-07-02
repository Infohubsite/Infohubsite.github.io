namespace Frontend.Services
{
    public class GlobalExceptionHandler(INotificationService notificationService) : DelegatingHandler
    {
        private readonly INotificationService _notificationService = notificationService;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                return await base.SendAsync(request, cancellationToken);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == null)
            {
                await _notificationService.Show("Network error: Could not connect to the server. Please check your internet connection.");
                throw;
            }
        }
    }
}