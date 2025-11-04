using Microsoft.JSInterop;

namespace Frontend.Services
{
    public interface ILocalStorageService
    {
        Task<string?> GetItemAsync(string key);
        Task SetItemAsync(string key, string value);
        Task RemoveItemAsync(string key);
    }

    public class LocalStorageService(IJSRuntime jsRuntime) : ILocalStorageService
    {
        private readonly IJSRuntime _jsRuntime = jsRuntime;

        public async Task<string?> GetItemAsync(string key) =>
            await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
        public async Task SetItemAsync(string key, string value) =>
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value);
        public async Task RemoveItemAsync(string key) =>
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
    }
}
