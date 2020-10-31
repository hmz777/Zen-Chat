using MVCBlazorChatApp.Client.Models;
using Microsoft.AspNetCore.SignalR.Client;
using MVCBlazorChatApp.Shared.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Forms;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace MVCBlazorChatApp.Client.Shared
{
    public partial class ChatSection : ComponentBase, IDisposable
    {
        [Inject] private NavigationManager NavigationManager { get; set; }
        [Inject] private IJSRuntime JSRuntime { get; set; }
        [Parameter] public string UIMode { get; set; }
        [Parameter] public string Room { get; set; }
        private EditContext editContext;
        private MessageModel MessageModel { get; set; } = new MessageModel();
        private UserModel UserModel { get; set; }
        public IEnumerable<UserModel> GroupUsers { get; set; }
        private HubConnection hubConnection;

        #region Component Methods

        protected override void OnInitialized()
        {
            UserModel = new UserModel()
            {
                Room = Room
            };

            editContext = new EditContext(MessageModel);

            SetupSignalR();
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
            if (!IsConnected)
                await ShowNotificationAsync(
                    MessageStatus.Failure,
                    "We're having trouble connecting you to our server. Try refreshing the page or wait for a reconnect.");

            await Send();

            MessageModel = new MessageModel();
            editContext = new EditContext(MessageModel);
        }

        #endregion

        #region SignalR Methods

        public void SetupSignalR()
        {
            hubConnection = new HubConnectionBuilder()
                .WithUrl(NavigationManager.ToAbsoluteUri("/chathub"))
                .Build();

            AttachCallbacks();
        }

        public void AttachCallbacks()
        {
            hubConnection.On<UserModel, string>("ReceiveMessage", ReceiveMessageAsync);
            hubConnection.On<UserModel>("AddUser", AddUserAsync);
            hubConnection.On<UserModel>("RemoveUser", RemoveUserAsync);
            hubConnection.On<MessageStatus, string>("ReceiveNotification", ShowNotificationAsync);
            hubConnection.Reconnecting += OnReconnectingAsync;
            hubConnection.Reconnected += OnReconnectedAsync;
            hubConnection.Closed += OnClosedAsync;
        }

        public async Task InitializeSignalR(string Username)
        {
            UserModel.Username = Username;

            await hubConnection.StartAsync();

            UserModel.ConnectionId = hubConnection.ConnectionId;

            GroupUsers = await hubConnection.InvokeAsync<IEnumerable<UserModel>>("RegisterUser", UserModel, Room);

            await AddUserListAsync(GroupUsers);
        }

        public bool IsConnected =>
            hubConnection.State == HubConnectionState.Connected;

        Task Send() =>
                    hubConnection.SendAsync("SendGroupMessage", UserModel, MessageModel.Message);

        public async Task OnReconnectingAsync(Exception exception)
        {
            await ShowNotificationAsync(MessageStatus.Failure,
                "Connection lost.");
            await ShowNotificationAsync(MessageStatus.Warning,
                "Reconnecting...");
        }

        public async Task OnReconnectedAsync(string connectionId)
        {
            await ShowNotificationAsync(MessageStatus.Success,
                "Reconnect successful.");
        }

        public async Task OnClosedAsync(Exception exception)
        {
            if (exception != null)
                await ShowNotificationAsync(MessageStatus.Failure,
                "Connection closed due to an error.");
            else
                await ShowNotificationAsync(MessageStatus.Failure,
                "Connection closed.");
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

        #endregion

        #region Admin Tools

        /// <summary>
        /// Add administrative message.
        /// </summary>
        /// <param name="MessageStatus"></param>
        /// <param name="Message"></param>
        /// <returns></returns>
        public async Task AddMessageAsync(MessageStatus MessageStatus, string Message)
        {
            await JSRuntime.InvokeVoidAsync("AddMessage",
                RenderMessage(Username: "Z-Bot",
                Message: Message,
                MessageStatus: MessageStatus));
        }

        /// <summary>
        /// Add administrative message.
        /// </summary>
        /// <param name="MessageStatus"></param>
        /// <param name="Message"></param>
        /// <returns></returns>
        public async Task ShowNotificationAsync(MessageStatus MessageStatus, string Message)
        {
            await JSRuntime.InvokeVoidAsync("ShowNotification",
                    Message,
                    (int)MessageStatus,
                    5000,
                    new { x = "right", y = "top" },
                    true);
        }

        #endregion

        #region Markup Methods

        public string RenderMessage(string Username, string Message, string Color = null, MessageStatus MessageStatus = MessageStatus.None)
        {
            if (MessageStatus == MessageStatus.None && Color != null)
            {
                return $"<div class=\"message-box\"><div class=\"message-header\"><div style=\"background:{Color}\" class=\"name\" title=\"{Username}\">{Username}</div><span class=\"date\">{DateTime.UtcNow}</span></div><pre class=\"message\">{Message}</pre></div>";
            }
            else
            {
                switch (MessageStatus)
                {
                    case MessageStatus.Success:
                        return $"<div class=\"message-box\"><div class=\"message-header\"><div class=\"name\" title=\"{Username}\"><i class=\"las la-shield-alt\"></i> {Username}</div><span class=\"date\">{DateTime.UtcNow}</span></div><p class=\"message message--success\">{Message}</p></div>";
                    case MessageStatus.Failure:
                        return $"<div class=\"message-box\"><div class=\"message-header\"><div class=\"name\" title=\"{Username}\"><i class=\"las la-shield-alt\"></i> {Username}</div><span class=\"date\">{DateTime.UtcNow}</span></div><p class=\"message message--failure\">{Message}</p></div>";
                    case MessageStatus.Information:
                        return $"<div class=\"message-box\"><div class=\"message-header\"><div class=\"name\" title=\"{Username}\"><i class=\"las la-shield-alt\"></i> {Username}</div><span class=\"date\">{DateTime.UtcNow}</span></div><p class=\"message message--information\">{Message}</p></div>";
                    case MessageStatus.Warning:
                        return $"<div class=\"message-box\"><div class=\"message-header\"><div class=\"name\" title=\"{Username}\"><i class=\"las la-shield-alt\"></i> {Username}</div><span class=\"date\">{DateTime.UtcNow}</span></div><p class=\"message message--warning\">{Message}</p></div>";
                    default:
                        return $"<div class=\"message-box\"><div class=\"message-header\"><div class=\"name\" title=\"{Username}\"><i class=\"las la-shield-alt\"></i> {Username}</div><span class=\"date\">{DateTime.UtcNow}</span></div><p class=\"message\">{Message}</p></div>";
                }
            }
        }

        public string RenderUser(string Username, string Color)
        {
            return $"<li title=\"{Username}\" data-name=\"{Username}\" style=\"background:{Color}\">{Username}</li>";
        }

        public string RenderUserList(IEnumerable<UserModel> UserList)
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

        #endregion

        public void Dispose()
        {
            _ = hubConnection.DisposeAsync();
        }
    }
}