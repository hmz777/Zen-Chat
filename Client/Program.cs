using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MVCBlazorChatApp.Client.Services.ChatService;
using MVCBlazorChatApp.Client.Services.MarkdownCompilerService;
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
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddHttpClient("MVCBlazorChatApp.ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
                .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();
            builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("MVCBlazorChatApp.ServerAPI"));
            builder.Services.AddSingleton<IMarkdownCompilerService, MarkdigCompilerService>(MCS => new MarkdigCompilerFactory().GetOrCreate());
            builder.Services.AddSingleton<IChatService, SignalRService>();
            builder.Services.AddApiAuthorization();
            builder.Services.AddBlazoredLocalStorage();


            await builder.Build().RunAsync();
        }
    }
}
