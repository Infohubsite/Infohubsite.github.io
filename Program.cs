using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Frontend
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            string? apiBaseUrl = builder.Configuration.GetValue<string>("ApiBaseUrl") ?? throw new InvalidOperationException($"{nameof(apiBaseUrl)} cannot be null");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) }); //builder.HostEnvironment.BaseAddress


            await builder.Build().RunAsync();
        }
    }
}
