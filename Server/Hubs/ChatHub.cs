using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MVCBlazorChatApp.Client.Models;
using MVCBlazorChatApp.Server.Data;
using MVCBlazorChatApp.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVCBlazorChatApp.Server.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(UserModel User, string Message)
        {
            await Clients.All.SendAsync("ReceiveMessage", User, Message);
        }

        public async Task SendGroupMessage(UserModel User, string Message)
        {
            await Clients.Group(User.Room).SendAsync("ReceiveMessage", User, Message);
        }

        public async Task AddToGroup(UserModel User, string GroupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName);
            await AddUserToList(User);

            await SendGroupNotification(User, $"{User.Username} connected.");
        }

        public async Task RemoveFromGroup(UserModel User, string GroupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName);
            await RemoveUserFromList(User);

            await SendGroupNotification(User, $"{User.Username} disconnected.");
        }

        public async Task SendGroupNotification(UserModel User, string Message)
        {
            await Clients.Group(User.Room).SendAsync("ReceiveNotification", MessageStatus.Information, Message);
        }

        public async Task AddUserToList(UserModel User)
        {
            await Clients.Group(User.Room).SendAsync("AddUser", User);
        }

        public async Task RemoveUserFromList(UserModel User)
        {
            await Clients.Group(User.Room).SendAsync("RemoveUser", User);
        }
    }
}
