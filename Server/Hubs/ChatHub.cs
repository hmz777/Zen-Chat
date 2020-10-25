using Microsoft.AspNetCore.SignalR;
using MVCBlazorChatApp.Client.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVCBlazorChatApp.Server.Hubs
{
    public class ChatHub : Hub
    {
        public async override Task OnConnectedAsync()
        {
        }

        public async Task SendMessage(UserModel User, string Message)
        {
            await Clients.All.SendAsync("ReceiveMessage", User, Message);
        }

        public async Task SendGroupMessage(UserModel User, string message)
        {
            await Clients.Group(User.Room).SendAsync("ReceiveNotification", User, message);
        }

        public async Task<bool> AddToGroup(UserModel User, string GroupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName);

            await SendGroupMessage(User, "User connected...");

            return true;
        }

        public async Task<bool> RemoveFromGroup(UserModel User, string GroupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName);

            await SendGroupMessage(User, "User disconnected...");

            return true;
        }
    }
}
