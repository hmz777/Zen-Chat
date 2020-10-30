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
        public static HashSet<UserModel> ConnectedUsers { get; set; } = new HashSet<UserModel>();

        #region Connect/Disconnect

        public async Task<IEnumerable<UserModel>> RegisterUser(UserModel User, string GroupName)
        {
            if (ConnectedUsers.Any(u => u.Username == User.Username))
            {
                await SendCallerNotification(MessageStatus.Failure, $"\"{User.Username}\" is already taken.");
                return Enumerable.Empty<UserModel>();
            }


            ConnectedUsers.Add(User);
            await AddToGroup(User, GroupName);
            await AddUserToList(User);
            await SendGroupNotification(User, MessageStatus.Information, $"{User.Username} connected.");

            return ConnectedUsers.Where(u => u.Room == User.Room && u.ConnectionId != User.ConnectionId).ToList();
        }
        public async override Task OnDisconnectedAsync(Exception exception)
        {
            var User = ConnectedUsers.Where(user => user.ConnectionId == Context.ConnectionId).FirstOrDefault();

            if (User == null)
                return;

            ConnectedUsers.Remove(User);
            await RemoveFromGroup(User, User.Room);
            await RemoveUserFromList(User);
            await SendGroupNotification(User, MessageStatus.Information, $"{User.Username} disconnected.");
        }

        #endregion

        #region Group Methods

        public async Task SendGroupMessage(UserModel User, string Message)
        {
            await Clients.Group(User.Room).SendAsync("ReceiveMessage", User, Message);
        }

        public async Task SendGroupNotification(UserModel User, MessageStatus MessageStatus, string Message)
        {
            await Clients.Group(User.Room).SendAsync("ReceiveNotification", MessageStatus, Message);
        }

        public async Task AddToGroup(UserModel User, string GroupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName);
        }

        public async Task RemoveFromGroup(UserModel User, string GroupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName);
        }

        #endregion

        #region Mass Methods

        public async Task SendMessage(UserModel User, string Message)
        {
            await Clients.All.SendAsync("ReceiveMessage", User, Message);
        }

        public async Task SendCallerMessage(string Message)
        {
            await Clients.Caller.SendAsync("ReceiveMessage", Message);
        }

        public async Task SendCallerNotification(MessageStatus MessageStatus, string Message)
        {
            await Clients.Caller.SendAsync("ReceiveNotification", MessageStatus, Message);
        }

        #endregion

        #region User Section Methods

        public async Task AddUserToList(UserModel User)
        {
            await Clients.Group(User.Room).SendAsync("AddUser", User);
        }

        public async Task RemoveUserFromList(UserModel User)
        {
            await Clients.Group(User.Room).SendAsync("RemoveUser", User);
        }

        #endregion
    }
}
