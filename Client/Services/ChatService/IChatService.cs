using MVCBlazorChatApp.Client.Models;
using MVCBlazorChatApp.Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MVCBlazorChatApp.Client.Services.ChatService
{
    public interface IChatService
    {
        void InitializeService(string Route);
        Task StartAsync();
        Task StopAsync();

        bool IsConnected();
        Task SendAsync(UserModel UserModel, MessageModel MessageModel);
        Task<IEnumerable<UserModel>> RegisterUserAsync(UserModel UserModel, string Room, string ConnectionId = null);

        void OnReconnecting(Func<Exception, Task> Method);
        void OnReconnected(Func<string, Task> Method);
        void OnClosed(Func<Exception, Task> Method);

        void RegisterHandler<T>(string MethodName, Func<T, Task> Handler);
        void RegisterHandler<T>(string MethodName, Action<T> Handler);
        void RegisterHandler<T1, T2>(string MethodName, Func<T1, T2, Task> Handler);
        void RegisterHandler<T1, T2>(string MethodName, Action<T1, T2> Handler);

        ValueTask DisposeAsync();
    }
}
