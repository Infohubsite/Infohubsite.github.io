using Frontend.Model;
using Frontend.Utils;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Frontend.Services
{
    public interface IAccountManagement
    {
        public Task<FormResult> LoginAsync(string username, string password);
        public Task LogoutAsync();
        public Task<bool> CheckAuthenticatedAsync();
        public Task<string> GetRoleAsync();
    }

    public class CustomAuthenticationStateProvider(IHttpClientFactory httpClientFactory, ILogger<CustomAuthenticationStateProvider> logger, ILocalStorageService localStorage) : AuthenticationStateProvider, IAccountManagement
    {
        private readonly ILogger<CustomAuthenticationStateProvider> _logger = logger;
        private readonly ILocalStorageService _localStorage = localStorage;

        private bool _authenticated = false;
        private readonly ClaimsPrincipal Unauthenticated = new(new ClaimsIdentity());

        private readonly HttpClient _httpClient = httpClientFactory.CreateClient("Default");

        private readonly JsonSerializerOptions jsonSerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private class LoginResponse
        {
            public string Token { get; set; } =string.Empty;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            _authenticated = false;
            var identity = new ClaimsIdentity();

            try
            {
                var token = await _localStorage.GetItemAsync<string>("authToken");

                if (string.IsNullOrEmpty(token))
                    throw new GetOutException();

                Dictionary<string, object>? dict = ParsePayloadFromJwt(token);
                if (dict == null || !dict.TryGetValue("exp", out object? expObject) || expObject == null)
                {
                    await _localStorage.RemoveItemAsync("authToken"); // clear the fucked up token
                    throw new GetOutException();
                }

                if (!long.TryParse(expObject.ToString(), out long expValue))
                {
                    await _localStorage.RemoveItemAsync("authToken"); // clear the fucked up token
                    throw new GetOutException();
                }

                if (DateTimeOffset.FromUnixTimeSeconds(expValue) <= DateTimeOffset.UtcNow)
                {
                    await _localStorage.RemoveItemAsync("authToken"); // clear expired token
                    throw new GetOutException();
                }

                identity = new ClaimsIdentity(
                    ParseClaimsFromJwt(dict),
                    "jwt"
                );
                _authenticated = true;
            }
            catch (GetOutException) { }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during authentication state retrieval.");
                await _localStorage.RemoveItemAsync("authToken");
            }

            return new AuthenticationState(new ClaimsPrincipal(identity));
        }

        public async Task<FormResult> LoginAsync(string username, string password)
        {
            HttpResponseMessage? response = null;
            try
            {
                response = await this._httpClient.PostAsJsonAsync(
                    "Auth/Login",
                    new
                    {
                        Username = username,
                        PasswordHash = password
                    });
                if (response.IsSuccessStatusCode)
                {
                    await this._localStorage.SetItemAsync("authToken", ((await response.Content.ReadFromJsonAsync<LoginResponse>()) ?? throw new NullReferenceException("Token response from server could not be converted to LoginRespose object")).Token);
                    NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
                    return new FormResult { Succeeded = true };
                }
            }
            catch (Exception ex)
            {
                this._logger.LogWarning(ex, "Exception in '{MethodName}'", nameof(LoginAsync));
                return new FormResult
                {
                    Succeeded = false,
                    ErrorList = [ex.ToString()]
                };
                throw;
            }

            return new FormResult
            {
                Succeeded = false,
                ErrorList = response == null ? ["Invalid server response"] : [response.ToString()]
            };
        }
        public async Task LogoutAsync()
        {
            await this._localStorage.RemoveItemAsync("authToken");
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
        public async Task<bool> CheckAuthenticatedAsync()
        {
            await GetAuthenticationStateAsync();
            return this._authenticated;
        }
        public async Task<string> GetRoleAsync()
        {
            try
            {
                HttpResponseMessage result = await this._httpClient.GetAsync("Auth/Role");
                return await result.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                this._logger.LogWarning(ex, "Exception in '{MethodName}'", nameof(LoginAsync));
                //throw;
            }

            return string.Empty;
        }

        private static Dictionary<string, object>? ParsePayloadFromJwt(string jwt)
        {
            string payload = jwt.Split('.')[1];
            byte[] jsonBytes = ParseBase64WithoutPadding(payload);
            return JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
        }
        private static IEnumerable<Claim> ParseClaimsFromJwt(Dictionary<string, object> jwtPayload)
        {
            return jwtPayload.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString() ?? ""));
        }
        private static byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }

            return Convert.FromBase64String(base64);
        }
    }
}
