using Frontend.Model;
using Microsoft.AspNetCore.Components.Authorization;
using System.Diagnostics.CodeAnalysis;
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
            this._authenticated = false;
            ClaimsIdentity identity = new();
            //this._httpClient.DefaultRequestHeaders.Authorization = null;

            try
            {
                /*HttpResponseMessage? userResponse = await this._httpClient.GetAsync("Auth/Profile");

                userResponse.EnsureSuccessStatusCode();

                string userJson = await userResponse.Content.ReadAsStringAsync();
                UserProfile? userProfile = JsonSerializer.Deserialize<UserProfile>(userJson, this.jsonSerializerOptions);

                if (userProfile != null)
                {
                    List<Claim> claims = [
                        new Claim(ClaimTypes.Name, userProfile.Username)
                        ];

                    claims.AddRange(userProfile.Claims.Where(c => c.Key != ClaimTypes.Name).Select(c => new Claim(c.Key, c.Value)));

                    HttpResponseMessage roleResponse = await this._httpClient.GetAsync("Auth/Role");

                    roleResponse.EnsureSuccessStatusCode();

                    claims.Add(new Claim(ClaimTypes.Role, await roleResponse.Content.ReadAsStringAsync()));

                    var id = new ClaimsIdentity(claims, nameof(CustomAuthenticationStateProvider));
                    user = new ClaimsPrincipal(id);
                    this._authenticated = true;
                }*/

                string token = await this._localStorage.GetItemAsync<string>("authToken");
                if (!string.IsNullOrEmpty(token))
                {
                    //this._httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    identity = new(ParseClaimsFromJwt(token), "jwt");
                    _authenticated = true;
                }
            }
            catch { } // sneaky peaky like

            return new AuthenticationState(new(identity));
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
                        PasswordHash = ComputeSha256Hash(password)
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
            /*const string Empty = "{}";
            var emptyContent = new StringContent(Empty, Encoding.UTF8, "application/json");
            await this._httpClient.PostAsync("Auth/Logout", emptyContent);*/
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
        private static string ComputeSha256Hash(string rawData)
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawData));

            StringBuilder builder = new();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2")); // "x2" for lowercase hex
            }
            return builder.ToString();
        }

        private static List<Claim> ParseClaimsFromJwt(string jwt)
        {
            List<Claim> claims = [];
            string payload = jwt.Split('.')[1];
            byte[] jsonBytes = ParseBase64WithoutPadding(payload);
            Dictionary<string, object>? dict = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
            if (dict != null)
                claims.AddRange(dict.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString() ?? "")));
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
