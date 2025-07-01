namespace Frontend.Services
{
    public class AuthenticationHeaderHandler(ILocalStorageService localStorage) : DelegatingHandler
    {
        private readonly ILocalStorageService _localStorage = localStorage;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await this._semaphore.WaitAsync(cancellationToken);
            try
            {
                string token = await this._localStorage.GetItemAsync<string>("authToken");
                if (!string.IsNullOrEmpty(token))
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                return await base.SendAsync(request, cancellationToken);
            }
            finally
            {
                this._semaphore.Release();
            }
        }
    }
}
