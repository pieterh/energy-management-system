using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

using MudBlazor.Services;

using EMS.BlazorWasm;
using EMS.BlazorWasm.Client.Services;
using EMS.BlazorWasm.Client.Services.Auth;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddSingleton<EMS.BlazorWasm.Services.ILocalStorage, EMS.BlazorWasm.Services.LocalStorageService>();
builder.Services.AddHttpClient<IUserService, UserService>(client =>
{
#if DEBUG
    client.BaseAddress = new Uri("http://localhost:5005");
#else
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
#endif

});
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationProvider>();
builder.Services.AddAuthorizationCore();
builder.Services.AddMudServices();

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

await builder.Build().RunAsync();
