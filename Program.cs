using Frontend.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Frontend
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.Services.AddAuthorizationCore();
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddTransient<CustomHttpHandler>();
            builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
            builder.Services.AddScoped(sp => (IAccountManagement)sp.GetRequiredService<AuthenticationStateProvider>());
            builder.Services.AddScoped(sp => (IHttpClient)sp.GetRequiredService<AuthenticationStateProvider>());

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.Configuration["Origins:Frontend"] ?? throw new InvalidOperationException($"Origins:Frontend cannot be null")) });

            builder.Services.AddHttpClient("Auth", opt => opt.BaseAddress = new Uri(builder.Configuration["Origins:Backend"] ?? throw new InvalidOperationException($"Origins:Backend cannot be null")))
                .AddHttpMessageHandler<CustomHttpHandler>();

            await builder.Build().RunAsync();
        }
    }
}
