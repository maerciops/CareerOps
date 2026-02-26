using CareerOps.Frontend;
using CareerOps.Frontend.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<ITokenProvider, LocalStorageTokenProvider>();
builder.Services.AddTransient<AuthenticatedHandler>();

builder.Services.AddHttpClient("CareerOps.API", client =>
{
    client.BaseAddress = new Uri("http://localhost:5010/");
})
.AddHttpMessageHandler<AuthenticatedHandler>();

builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("CareerOps.API"));

builder.Services.AddScoped<JobApiClient>();
builder.Services.AddMudServices();

await builder.Build().RunAsync();
