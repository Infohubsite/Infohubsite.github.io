using Microsoft.JSInterop;

namespace Frontend.Services
{
    public interface ILocalStorageService
    {
        Task<T> GetItemAsync<T>(string key);
        Task SetItemAsync<T>(string key, T value);
        Task RemoveItemAsync(string key);
    }

    public class LocalStorageService(IJSRuntime jsRuntime) : ILocalStorageService
    {
        private readonly IJSRuntime _jsRuntime = jsRuntime;
        
        public async Task<T> GetItemAsync<T>(string key) =>
            await this._jsRuntime.InvokeAsync<T>("localStorage.getItem", key);
        public async Task SetItemAsync<T>(string key, T value) =>
            await this._jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value);
        public async Task RemoveItemAsync(string key) =>
            await this._jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
    }
}
