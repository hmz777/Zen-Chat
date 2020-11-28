using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using MVCBlazorChatApp.Client.Models;
using MVCBlazorChatApp.Client.Services.ChatService;
using MVCBlazorChatApp.Shared.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace MVCBlazorChatApp.Client.Shared
{
    public partial class ChatSection : ComponentBase, IAsyncDisposable
    {
        [Inject] private IChatService ChatService { get; set; }
        [Inject] private IJSRuntime JSRuntime { get; set; }
        [Parameter] public string UIMode { get; set; }
        [Parameter] public string Room { get; set; }
        private RingtoneComponent RingtoneComponent { get; set; }
        private EditContext editContext;
        private MessageModel MessageModel { get; set; } = new MessageModel();
        private UserModel UserModel { get; set; }
        public IEnumerable<UserModel> GroupUsers { get; set; }

        #region Component Methods

        protected override void OnInitialized()
        {
            UserModel = new UserModel()
            {
                Room = Room
            };

            editContext = new EditContext(MessageModel);

            ChatService.InitializeService("/chathub");

            AttachCallbacks();
        }

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            if (UIMode == "W-Mode")
            {
                await JSRuntime.InvokeVoidAsync("InitializeOS", "ChatContent", "os-theme-dark", "chat");
            }
            else
            {
                await JSRuntime.InvokeVoidAsync("InitializeOS", "ChatContent", "os-theme-light", "chat");
            }

            await JSRuntime.InvokeVoidAsync("InitializeEmojis", "emoji", DotNetObjectReference.Create(this));
        }

        private async Task ValidSubmit()
        {
            await Submit();
            ResetForm();
        }

        private async Task Submit()
        {
            if (!ChatService.IsConnected())
                await ShowNotificationAsync(
                    new MessageModel
                    {
                        MessageStatus = MessageStatus.Failure,
                        Message = "We're having trouble connecting you to our server. Try refreshing the page or wait for a reconnect."
                    });

            await ChatService.SendAsync(UserModel, MessageModel);
        }

        public void ResetForm()
        {
            MessageModel = new MessageModel();
            editContext = new EditContext(MessageModel);
        }

        private async Task InputFocus()
        {
            await JSRuntime.InvokeVoidAsync("SetTitle", $"Zen Chat - {Room}");
        }

        #endregion

        #region SignalR Methods

        public void AttachCallbacks()
        {
            ChatService.RegisterHandler<UserModel, string>("ReceiveMessage", ReceiveMessageAsync);
            ChatService.RegisterHandler<UserModel>("AddUser", AddUserAsync);
            ChatService.RegisterHandler<UserModel>("RemoveUser", RemoveUserAsync);
            ChatService.RegisterHandler<MessageModel>("ReceiveNotification", ShowNotificationAsync);
            ChatService.OnReconnecting(OnReconnectingAsync);
            ChatService.OnReconnected(OnReconnectedAsync);
            ChatService.OnClosed(OnClosedAsync);
        }

        public async Task StartService(string Username)
        {
            UserModel.Username = Username;

            await ChatService.StartAsync();

            GroupUsers = await ChatService.RegisterUserAsync(UserModel, Room);

            if (GroupUsers == null)
                await ChatService.StopAsync();
            else
                await AddUserListAsync(GroupUsers);
        }

        public async Task OnReconnectingAsync(Exception exception)
        {
            await ShowNotificationAsync(new MessageModel
            {
                MessageStatus = MessageStatus.Warning,
                Message = "Reconnecting..."
            });
        }

        public async Task OnReconnectedAsync(string ConnectionId)
        {
            await ChatService.RegisterUserAsync(UserModel, Room, ConnectionId);

            await ShowNotificationAsync(new MessageModel
            {
                MessageStatus = MessageStatus.Success,
                Message = "Reconnect successful."
            });
        }

        public async Task OnClosedAsync(Exception exception)
        {
            if (exception != null)
                await ShowNotificationAsync(new MessageModel
                {
                    MessageStatus = MessageStatus.Failure,
                    Message = "Connection closed due to an error."
                });
            else
                await ShowNotificationAsync(new MessageModel
                {
                    MessageStatus = MessageStatus.Failure,
                    Message = "Connection closed."
                });
        }

        #endregion

        #region Clientside Callbacks

        /// <summary>
        /// Callback for chat messages.
        /// </summary>
        /// <param name="UserModel"></param>
        /// <param name="Message"></param>
        /// <returns></returns>
        private async Task ReceiveMessageAsync(UserModel UserModel, string Message)
        {
            await JSRuntime.InvokeVoidAsync("AddMessage",
            RenderMessage(Username: UserModel.Username, Color: UserModel.Color, Message: Message));

            await ScrollChatIntoView();

            await UpdateTitle();

            if (!(await JSRuntime.InvokeAsync<bool>("AppHasFocus")))
            {
                await CheckPermissionAndTryPublishNotification(Message);
                await RingtoneComponent.Play();
            }
        }

        /// <summary>
        /// Callback for new connected users, It adds the new user to the list of users in the user section.
        /// </summary>
        /// <param name="userModel"></param>
        /// <returns></returns>
        public async Task AddUserAsync(UserModel UserModel)
        {
            await JSRuntime.InvokeVoidAsync("AddUser",
                    RenderUser(Username: UserModel.Username,
                    Color: UserModel.Color));
        }

        public async Task AddUserListAsync(IEnumerable<UserModel> UserList)
        {
            await JSRuntime.InvokeVoidAsync("AddUserList",
                    RenderUserList(UserList));
        }

        /// <summary>
        /// Callback for disconnected users, It removes the user from the list of users in the user section.
        /// </summary>
        /// <param name="userModel"></param>
        /// <returns></returns>
        public async Task RemoveUserAsync(UserModel UserModel)
        {
            await JSRuntime.InvokeVoidAsync("RemoveUser", UserModel.Username);
        }

        public async Task ScrollChatIntoView()
        {
            await JSRuntime.InvokeVoidAsync("ScrollChatSec", "ChatContent");
        }

        public async Task UpdateTitle(string Title = null)
        {
            await JSRuntime.InvokeVoidAsync("SetTitle", Title);
        }

        public async Task CheckPermissionAndTryPublishNotification(string Message)
        {
            await JSRuntime.InvokeVoidAsync("SendNotification", Message);
        }

        /// <summary>
        /// Add administrative message.
        /// </summary>
        /// <param name="MessageStatus"></param>
        /// <param name="Message"></param>
        /// <returns></returns>
        public async Task AddMessageAsync(MessageModel MessageModel)
        {
            await JSRuntime.InvokeVoidAsync("AddMessage",
                RenderMessage(Username: "Z-Bot",
                Message: MessageModel.Message,
                MessageStatus: MessageModel.MessageStatus));
        }

        /// <summary>
        /// Add administrative message.
        /// </summary>
        /// <param name="MessageStatus"></param>
        /// <param name="Message"></param>
        /// <returns></returns>
        public async Task ShowNotificationAsync(MessageModel MessageModel)
        {
            await JSRuntime.InvokeVoidAsync("ShowNotification",
                    MessageModel.Message,
                    (int)MessageModel.MessageStatus,
                    5000,
                    new { x = "right", y = "top" },
                    true);
        }

        #endregion

        #region Markup Methods

        public static string RenderMessage(string Username, string Message, string Color = null, MessageStatus MessageStatus = MessageStatus.None)
        {
            var Date = DateTime.UtcNow;

            if (MessageStatus == MessageStatus.None && Color != null)
            {
                return $"<div class=\"message-box\"><div class=\"message-header\"><span title=\"{Date}\" class=\"date\">{Date}</span><div style=\"background:{Color}\" class=\"name\" title=\"{Username}\">{Username}</div></div><pre class=\"message\">{Message}</pre></div>";
            }
            else
            {
                switch (MessageStatus)
                {
                    case MessageStatus.Success:
                        return $"<div class=\"message-box\"><div class=\"message-header\"><span title=\"{Date}\" class=\"date\">{Date}</span><div style=\"background:{Color}\" class=\"name\" title=\"{Username}\">{Username}</div></div><p class=\"message message--success\">{Message}</p></div>";
                    case MessageStatus.Failure:
                        return $"<div class=\"message-box\"><div class=\"message-header\"><span title=\"{Date}\" class=\"date\">{Date}</span><div style=\"background:{Color}\" class=\"name\" title=\"{Username}\">{Username}</div></div><p class=\"message message--failure\">{Message}</p></div>";
                    case MessageStatus.Information:
                        return $"<div class=\"message-box\"><div class=\"message-header\"><span title=\"{Date}\" class=\"date\">{Date}</span><div style=\"background:{Color}\" class=\"name\" title=\"{Username}\">{Username}</div></div><p class=\"message message--information\">{Message}</p></div>";
                    case MessageStatus.Warning:
                        return $"<div class=\"message-box\"><div class=\"message-header\"><span title=\"{Date}\" class=\"date\">{Date}</span><div style=\"background:{Color}\" class=\"name\" title=\"{Username}\">{Username}</div></div><p class=\"message message--warning\">{Message}</p></div>";
                    default:
                        return $"<div class=\"message-box\"><div class=\"message-header\"><span title=\"{Date}\" class=\"date\">{Date}</span><div style=\"background:{Color}\" class=\"name\" title=\"{Username}\">{Username}</div></div><p class=\"message\">{Message}</p></div>";
                }
            }
        }

        public static string RenderUser(string Username, string Color)
        {
            return $"<li title=\"{Username}\" data-name=\"{Username}\" style=\"background:{Color}\">{Username}</li>";
        }

        public static string RenderUserList(IEnumerable<UserModel> UserList)
        {
            string UserListMarkup = string.Empty;

            foreach (var user in UserList)
            {
                UserListMarkup += RenderUser(user.Username, user.Color);
            }

            return UserListMarkup;
        }

        #endregion

        #region CS Interop

        [JSInvokable]
        public void AddEmoji(string emoji)
        {
            MessageModel.Message += emoji;
        }

        [JSInvokable]
        public async Task ValidateAndSubmit()
        {
            if (editContext.Validate())
            {
                await Submit();
            }
        }

        #endregion

        #region Cleanup

        public async ValueTask DisposeAsync()
        {
            await ChatService.DisposeAsync();
        }

        #endregion
    }
}