using Frontend.Common;
using Frontend.Services;
using Shared.Interface;

namespace Frontend.HttpClients
{
    public abstract class Client<TSelf>(HttpClient httpClient, ILogger<TSelf> logger, INotificationService notifs) where TSelf : Client<TSelf>
    {
        protected readonly HttpClient _httpClient = httpClient;
        protected readonly ILogger<TSelf> _logger = logger;
        protected readonly INotificationService _notifs = notifs;

        #region HELPER_METHODS
        protected async Task<Result<T>> HandleAsync<T>(Func<HttpRequestMessage, Task<Result<T>>> func, HttpRequestMessage request, string notif, Action<Exception> custom) => await Result<T>.HandleAsync(async () => await func(request), async (ex) => await Fail(ex, notif, custom));
        protected async Task<Result> HandleAsync(Func<HttpRequestMessage, Task<Result>> func, HttpRequestMessage request, string notif, Action<Exception> custom) => await Result.HandleAsync(async () => await func(request), async (ex) => await Fail(ex, notif, custom));
        private async Task Fail(Exception exception, string notif, Action<Exception> custom)
        {
            Task task = _notifs.Show(notif + "\nError: " + exception.Message);
            custom(exception);
            await task;
        }

        protected async Task<Result> SendResult(HttpRequestMessage request) => await Result.From(await Send(request));
        protected async Task<Result<T>> SendResult<T>(HttpRequestMessage request) => await Result<T>.From(await Send(request));
        protected async Task<Result<TTo>> SendResult<TTo, TDto>(HttpRequestMessage request) where TTo : IConverterFrom<TTo, TDto> => ResultConverter.ConvertFrom<TTo, TDto>(await SendResult<TDto>(request));
        protected async Task<Result<List<TTo>>> SendResultList<TTo, TDto>(HttpRequestMessage request) where TTo : IConverterFrom<TTo, TDto> => ResultConverter.ConvertListFrom<TTo, TDto>(await SendResult<List<TDto>>(request));
        private async Task<HttpResponseMessage> Send(HttpRequestMessage request) => await _httpClient.SendAsync(request);
        #endregion
    }
}
