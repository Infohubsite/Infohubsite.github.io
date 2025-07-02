using Frontend.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Polly;

namespace Frontend
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddSingleton<INotificationService, NotificationService>();
            builder.Services.AddTransient<GlobalExceptionHandler>();
            builder.Services.AddTransient<AuthenticationHeaderHandler>();
            builder.Services.AddHttpClient("Default", client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["Origins:Backend"] ?? throw new InvalidOperationException("Backend origin URL ('Origins:Backend') is not configured."));
            })
                .AddHttpMessageHandler<AuthenticationHeaderHandler>()
                .AddPolicyHandler(Policy<HttpResponseMessage>
                    .Handle<HttpRequestException>(ex => ex.StatusCode == null)
                    .WaitAndRetryAsync(
                        3,
                        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                        onRetry: (outcome, timespan, retryAttempt, context) =>
                        {
                            Console.WriteLine($"[Polly] Network error detected. Delaying for {timespan.TotalSeconds}s before retry {retryAttempt}/{3}. Reason: {outcome.Exception.Message}");
                        }
                    ));
                //.AddHttpMessageHandler<GlobalExceptionHandler>();
            builder.Services.AddHttpClient("OriginClient", client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["Origins:Frontend"] ?? throw new InvalidOperationException("Frontend origin URL ('Origins:Frontend') is not configured."));
            });
            builder.Services.AddHttpClient("WakeupClient", client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["Origins:Backend"] ?? throw new InvalidOperationException("Backend origin URL ('Origins:Backend') is not configured."));
            });

            builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("Default"));

            builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();

            builder.Services.AddAuthorizationCore();
            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
            builder.Services.AddScoped(sp => (IAccountManagement)sp.GetRequiredService<AuthenticationStateProvider>());

            builder.Services.AddMudServices();

            await builder.Build().RunAsync();
        }
    }
}
