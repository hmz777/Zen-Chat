using MessagePack;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using MVCBlazorChatApp.Client.Models;
using MVCBlazorChatApp.Client.Services.MarkdownCompilerService;
using MVCBlazorChatApp.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVCBlazorChatApp.Client.Services.ChatService
{
    public class SignalRService : IChatService
    {
        private readonly NavigationManager NavigationManager;
        private readonly IMarkdownCompilerService markdownCompilerService;
        private HubConnection HubConnection;
        private bool ServiceInitialized;
        private bool ServiceStarted;

        public SignalRService(NavigationManager navigationManager, IMarkdownCompilerService markdownCompilerService)
        {
            this.NavigationManager = navigationManager;
            this.markdownCompilerService = markdownCompilerService;
        }

        public ValueTask DisposeAsync()
        {
            if (ServiceInitialized)
                return HubConnection.DisposeAsync();

            throw new Exception("Service not initialized.");
        }

        public void InitializeService(string Route)
        {
            if (!ServiceInitialized)
            {
                HubConnection = new HubConnectionBuilder()
                .WithUrl(NavigationManager.ToAbsoluteUri(Route))
                .WithAutomaticReconnect(new TimeSpan[] {
                    TimeSpan.FromSeconds(0),
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(10),
                    TimeSpan.FromSeconds(30),
                    TimeSpan.FromSeconds(60),
                    TimeSpan.FromSeconds(90)
                })
                .AddMessagePackProtocol(options =>
                {
                    options.SerializerOptions = MessagePackSerializerOptions.Standard
                    .WithCompression(MessagePackCompression.Lz4Block)
                    .WithSecurity(MessagePackSecurity.UntrustedData);
                })
                .Build();

                ServiceInitialized = true;
            }
        }

        public Task StartAsync()
        {
            if (!ServiceInitialized)
                throw new Exception("Service not initialized.");

            ServiceStarted = true;
            return HubConnection.StartAsync();
        }

        public Task StopAsync()
        {
            if (!ServiceInitialized)
                throw new Exception("Service not initialized.");

            ServiceStarted = false;
            return HubConnection.StopAsync();
        }

        public bool IsConnected()
        {
            if (ServiceInitialized && ServiceStarted)
                return HubConnection.State == HubConnectionState.Connected;

            return false;
        }

        public void OnClosed(Func<Exception, Task> Method)
        {
            if (ServiceInitialized)
                HubConnection.Closed += Method;
            else
                throw new Exception("Service not initialized.");
        }

        public void OnReconnected(Func<string, Task> Method)
        {
            if (ServiceInitialized)
                HubConnection.Reconnected += Method;
            else
                throw new Exception("Service not initialized.");
        }

        public void OnReconnecting(Func<Exception, Task> Method)
        {
            if (ServiceInitialized)
                HubConnection.Reconnecting += Method;
            else
                throw new Exception("Service not initialized.");
        }

        public void RegisterHandler<T>(string MethodName, Func<T, Task> Handler)
        {
            if (ServiceInitialized)
                HubConnection.On<T>(MethodName, Handler);
            else
                throw new Exception("Service not initialized.");
        }

        public void RegisterHandler<T>(string MethodName, Action<T> Handler)
        {
            if (ServiceInitialized)
                HubConnection.On<T>(MethodName, Handler);
            else
                throw new Exception("Service not initialized.");
        }

        public void RegisterHandler<T1, T2>(string MethodName, Func<T1, T2, Task> Handler)
        {
            if (ServiceInitialized)
                HubConnection.On<T1, T2>(MethodName, Handler);
            else
                throw new Exception("Service not initialized.");
        }

        public void RegisterHandler<T1, T2>(string MethodName, Action<T1, T2> Handler)
        {
            if (ServiceInitialized)
                HubConnection.On<T1, T2>(MethodName, Handler);
            else
                throw new Exception("Service not initialized.");
        }

        public Task<IEnumerable<UserModel>> RegisterUserAsync(UserModel UserModel, string Room, string ConnectionId = null)
        {
            UserModel.ConnectionId = ConnectionId ?? HubConnection.ConnectionId;

            return HubConnection.InvokeAsync<IEnumerable<UserModel>>("RegisterUser", UserModel, Room);
        }

        public string CompileMarkdown(string Text)
        {
            return markdownCompilerService.CompileMarkdown(Text);
        }

        //Also, try the compilation on message receive.
        public Task SendAsync(UserModel UserModel, MessageModel MessageModel)
        {
            return HubConnection.SendAsync("SendGroupMessage", UserModel, CompileMarkdown(MessageModel.Message.Trim()));
        }
    }
}
