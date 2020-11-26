using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MVCBlazorChatApp.Client.Services.ChatService;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace MVCBlazorChatApp.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Services.AddHttpClient("MVCBlazorChatApp.ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
                .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

            // Supply HttpClient instances that include access tokens when making requests to the server project
            builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("MVCBlazorChatApp.ServerAPI"));
            builder.Services.AddSingleton<IChatService, SignalRService>();
            builder.Services.AddApiAuthorization();

            //builder.Services.AddApiAuthorization(options =>
            //{
            //    options.AuthenticationPaths.LogInPath = "auth/login";
            //    options.AuthenticationPaths.LogInCallbackPath = "auth/login-callback";
            //    options.AuthenticationPaths.LogInFailedPath = "auth/login-failed";
            //    options.AuthenticationPaths.LogOutPath = "auth/logout";
            //    options.AuthenticationPaths.LogOutCallbackPath = "auth/logout-callback";
            //    options.AuthenticationPaths.LogOutFailedPath = "auth/logout-failed";
            //    options.AuthenticationPaths.LogOutSucceededPath = "auth/logged-out";
            //    options.AuthenticationPaths.ProfilePath = "auth/profile";
            //    options.AuthenticationPaths.RemoteRegisterPath = "/register";
            //    options.AuthenticationPaths.RemoteRegisterPath = "/settings";

            //});

            builder.Services.AddBlazoredLocalStorage();
            await builder.Build().RunAsync();
        }
    }
}
