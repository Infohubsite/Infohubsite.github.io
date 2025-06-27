using Frontend.Model;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Frontend.Services
{
    public class CustomAuthenticationStateProvider(IHttpClientFactory httpClientFactory, ILogger<CustomAuthenticationStateProvider> logger) : AuthenticationStateProvider, IAccountManagement, IHttpClient
    {
        private readonly ILogger<CustomAuthenticationStateProvider> _logger = logger;

        private bool _authenticated = false;
        private readonly ClaimsPrincipal Unauthenticated = new(new ClaimsIdentity());

        private readonly HttpClient _httpClient = httpClientFactory.CreateClient("Auth");

        private readonly JsonSerializerOptions jsonSerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            this._authenticated = false;
            ClaimsPrincipal user = this.Unauthenticated;

            try
            {
                HttpResponseMessage? userResponse = await this._httpClient.GetAsync("Auth/Profile");

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
                }
            }
            catch { }

            return new AuthenticationState(user);
        }

        public async Task<FormResult> LoginAsync(string username, string password)
        {
            HttpResponseMessage? result = null;
            try
            {
                result = await this._httpClient.PostAsJsonAsync(
                    "Auth/Login",
                    new
                    {
                        Username = username,
                        PasswordHash = ComputeSha256Hash(password)
                    });
                if (result.IsSuccessStatusCode)
                {
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
                ErrorList = result == null ? ["Couldn't receve a response from the server"] : [result.ToString()]
            };
        }
        public async Task LogoutAsync()
        {
            const string Empty = "{}";
            var emptyContent = new StringContent(Empty, Encoding.UTF8, "application/json");
            await this._httpClient.PostAsync("Auth/Logout", emptyContent);
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

        public Task<HttpResponseMessage> GetAsync(string? responseUri) => this._httpClient.GetAsync(responseUri);
        public Task<HttpResponseMessage> GetAsync(string? requestUri, CancellationToken cancellationToken) => this._httpClient.GetAsync(requestUri, cancellationToken);
        public Task<HttpResponseMessage> GetAsync(string? requestUri, HttpCompletionOption completionOption) => this._httpClient.GetAsync(requestUri, completionOption);
        public Task<HttpResponseMessage> GetAsync(string? requestUri, HttpCompletionOption completionOption, CancellationToken cancellationToken) => this._httpClient.GetAsync(requestUri, completionOption, cancellationToken);
        public Task<HttpResponseMessage> GetAsync(Uri? responseUri) => this._httpClient.GetAsync(responseUri);
        public Task<HttpResponseMessage> GetAsync(Uri? requestUri, CancellationToken cancellationToken) => this._httpClient.GetAsync(requestUri, cancellationToken);
        public Task<HttpResponseMessage> GetAsync(Uri? requestUri, HttpCompletionOption completionOption) => this._httpClient.GetAsync(requestUri, completionOption);
        public Task<HttpResponseMessage> GetAsync(Uri? requestUri, HttpCompletionOption completionOption, CancellationToken cancellationToken) => this._httpClient.GetAsync(requestUri, completionOption, cancellationToken);

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
    }
}
