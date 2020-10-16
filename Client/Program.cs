using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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

            builder.Services.AddApiAuthorization(options => {

                options.AuthenticationPaths.LogInPath = "Auth/login";
                options.AuthenticationPaths.LogInCallbackPath = "Auth/login-callback";
                options.AuthenticationPaths.LogInFailedPath = "Auth/login-failed";
                options.AuthenticationPaths.LogOutPath = "Auth/logout";
                options.AuthenticationPaths.LogOutCallbackPath = "Auth/logout-callback";
                options.AuthenticationPaths.LogOutFailedPath = "Auth/logout-failed";
                options.AuthenticationPaths.LogOutSucceededPath = "Auth/logged-out";
                options.AuthenticationPaths.ProfilePath = "Auth/profile";
                options.AuthenticationPaths.RegisterPath = "Auth/register";

            });

            await builder.Build().RunAsync();
        }
    }
}
