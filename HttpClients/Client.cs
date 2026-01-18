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
        protected Task<Result<T>> HandleAsync<T>(Func<HttpRequestMessage, Task<Result<T>>> func, HttpRequestMessage request, string notif, Action<Exception> custom) => Result<T>.HandleAsync(() => func(request), (ex) => Fail(ex, notif, custom));
        protected Task<Result> HandleAsync(Func<HttpRequestMessage, Task<Result>> func, HttpRequestMessage request, string notif, Action<Exception> custom) => Result.HandleAsync(() => func(request), (ex) => Fail(ex, notif, custom));
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
        private Task<HttpResponseMessage> Send(HttpRequestMessage request) => _httpClient.SendAsync(request);
        #endregion
    }
}
