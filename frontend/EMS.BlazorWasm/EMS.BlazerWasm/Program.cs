using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

using EMS.BlazorWasm;
using EMS.BlazorWasm.Client.Services;
using EMS.BlazorWasm.Client.Services.Auth;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddSingleton<EMS.BlazorWasm.Services.ILocalStorage, EMS.BlazorWasm.Services.LocalStorageService>();
builder.Services.AddHttpClient<IUserService, UserService>(client =>
{
    //client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
    client.BaseAddress = new Uri("http://localhost:5005");
});
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationProvider>();
builder.Services.AddAuthorizationCore();

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

//replaced by AddHttpClient
//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();
