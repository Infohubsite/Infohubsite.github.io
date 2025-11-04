using Frontend.Common;
using Frontend.HttpClients;
using Frontend.Models;
using Frontend.Models.Converted;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Text.Json;

namespace Frontend.Services
{
    public interface IAccountManagement
    {
        public Task<FormResult> LoginAsync(string username, string password);
        public Task LogoutAsync();
        public Task<bool> CheckAuthenticatedAsync();
    }

    public class CustomAuthenticationStateProvider(DefaultClient client, ILogger<CustomAuthenticationStateProvider> logger, ILocalStorageService localStorage) : AuthenticationStateProvider, IAccountManagement
    {
        private readonly ILogger<CustomAuthenticationStateProvider> _logger = logger;
        private readonly ILocalStorageService _localStorage = localStorage;

        private bool _authenticated = false;
        private readonly ClaimsPrincipal Unauthenticated = new(new ClaimsIdentity());

        private readonly DefaultClient _client = client;

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            _authenticated = false;
            try
            {
                string? token = await _localStorage.GetItemAsync("authToken");

                if (string.IsNullOrEmpty(token)) return await NoAuth();

                Dictionary<string, object>? dict = ParsePayloadFromJwt(token);
                if (dict == null || !dict.TryGetValue("exp", out object? expObject) || expObject == null) return await NoAuth();
                if (!long.TryParse(expObject.ToString(), out long expValue)) return await NoAuth();
                if (DateTimeOffset.FromUnixTimeSeconds(expValue) <= DateTimeOffset.UtcNow) return await NoAuth();

                _authenticated = true;
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(dict), "jwt")));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during authentication state retrieval.");
                await _localStorage.RemoveItemAsync("authToken");
            }

            return new AuthenticationState(Unauthenticated);
        }

        public async Task<FormResult> LoginAsync(string username, string password)
        {
            Result<LoginResponse>? response = null;
            try
            {
                response = await _client.Login(username, password);
                if (response.IsSuccess)
                {
                    Task task = _localStorage.SetItemAsync("authToken", response.Value.Token);
                    NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
                    await task;
                    return new FormResult { Succeeded = true };
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return new FormResult { Succeeded = false, ErrorList = ["Incorrect credentials"] };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Exception in '{MethodName}'", nameof(LoginAsync));
                return new FormResult
                {
                    Succeeded = false,
                    ErrorList = [ex.ToString()]
                };
            }

            return new FormResult
            {
                Succeeded = false,
                ErrorList = response.Exception == null ? ["Invalid server response"] : [response.Exception.Message]
            };
        }
        public async Task LogoutAsync()
        {
            await _localStorage.RemoveItemAsync("authToken");
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
        public async Task<bool> CheckAuthenticatedAsync()
        {
            await GetAuthenticationStateAsync();
            return _authenticated;
        }

        private async Task<AuthenticationState> NoAuth()
        {
            await _localStorage.RemoveItemAsync("authToken");
            return new AuthenticationState(Unauthenticated);
        }

        private static Dictionary<string, object>? ParsePayloadFromJwt(string jwt)
        {
            string payload = jwt.Split('.')[1];
            byte[] jsonBytes = ParseBase64WithoutPadding(payload);
            return JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
        }
        private static List<Claim> ParseClaimsFromJwt(Dictionary<string, object> jwtPayload)
        {
            List<Claim> claims = [];
            foreach (KeyValuePair<string, object> kvp in jwtPayload)
            {
                string val = kvp.Value.ToString() ?? string.Empty;
                if (kvp.Key == ClaimTypes.Role && val.StartsWith('['))
                {
                    string[]? roles = JsonSerializer.Deserialize<string[]>(val);
                    if (roles != null)
                        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
                }
                else
                    claims.Add(new Claim(kvp.Key, val));
            }
            return claims;
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
