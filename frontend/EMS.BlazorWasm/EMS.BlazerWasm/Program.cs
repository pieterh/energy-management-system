using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

using MudBlazor.Services;

using EMS.BlazorWasm;
using EMS.BlazorWasm.Client.Services;
using EMS.BlazorWasm.Client.Services.Auth;
using EMS.BlazorWasm.Client.Services.Chargepoint;
using EMS.BlazorWasm.Services.Auth;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

#if DEBUG
var baseAddres = new Uri("http://localhost:5005");
#else
var baseAddres =  new Uri(builder.HostEnvironment.BaseAddress);
#endif

builder.Services.AddSingleton<EMS.BlazorWasm.Services.ILocalStorage, EMS.BlazorWasm.Services.LocalStorageService>();
builder.Services.AddHttpClient<IUserService, UserService>(client =>
{
    client.BaseAddress = baseAddres;
});

builder.Services.AddScoped<IAccessTokenProvider, TokenProvider>();
builder.Services.AddScoped<CustomAuthorizationMessageHandler>();
builder.Services.AddHttpClient("ServerAPI", client => client.BaseAddress = baseAddres)
    .AddHttpMessageHandler<CustomAuthorizationMessageHandler>();


builder.Services.AddScoped<IChargepointService, ChargepointService>((sp) => {
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("ServerAPI");
    var instance = ActivatorUtilities.CreateInstance<ChargepointService>(sp, httpClient);
    return instance;
});



builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationProvider>();

builder.Services.AddAuthorizationCore();
builder.Services.AddMudServices();

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

await builder.Build().RunAsync();
