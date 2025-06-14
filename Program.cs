using Blog_Web;
using Blog_Web.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using System.Text.Json;
using Blazored.LocalStorage;
using Syncfusion.Blazor;


var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddSyncfusionBlazor();


// builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthService>();

// Load environment-specific appsettings
var environment = builder.HostEnvironment.Environment; 
var configFileName = $"appsettings.{environment}.json";

var tempHttpClient = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
var configStream = await tempHttpClient.GetStreamAsync(configFileName);
var config = await JsonSerializer.DeserializeAsync<AppConfiguration>(configStream);

if (config == null || string.IsNullOrEmpty(config.ApiBase))
    throw new Exception("Failed to load API base URL from config.");

builder.Services.AddSingleton(config);
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(config.ApiBase) });

await builder.Build().RunAsync();
