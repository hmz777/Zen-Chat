using MessagePack;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using MVCBlazorChatApp.Client.Models;
using MVCBlazorChatApp.Shared.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace MVCBlazorChatApp.Client.Shared
{
    public partial class ChatSection : ComponentBase, IAsyncDisposable
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
            await Submit();
            ResetForm();
        }

        private async Task Submit()
        {
            if (!IsConnected)
                await ShowNotificationAsync(
                    new MessageModel
                    {
                        MessageStatus = MessageStatus.Failure,
                        Message = "We're having trouble connecting you to our server. Try refreshing the page or wait for a reconnect."
                    });

            await Send();
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

        public void SetupSignalR()
        {
            hubConnection = new HubConnectionBuilder()
                .WithUrl(NavigationManager.ToAbsoluteUri("/chathub"))
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

            AttachCallbacks();
        }

        public void AttachCallbacks()
        {
            hubConnection.On<UserModel, string>("ReceiveMessage", ReceiveMessageAsync);
            hubConnection.On<UserModel>("AddUser", AddUserAsync);
            hubConnection.On<UserModel>("RemoveUser", RemoveUserAsync);
            hubConnection.On<MessageModel>("ReceiveNotification", ShowNotificationAsync);
            hubConnection.Reconnecting += OnReconnectingAsync;
            hubConnection.Reconnected += OnReconnectedAsync;
            hubConnection.Closed += OnClosedAsync;
        }

        public async Task InitializeSignalR(string Username)
        {
            UserModel.Username = Username;

            await hubConnection.StartAsync();

            await RegisterUser(hubConnection.ConnectionId);
        }

        public bool IsConnected =>
            hubConnection.State == HubConnectionState.Connected;

        Task Send() =>
                    hubConnection.SendAsync("SendGroupMessage", UserModel, MessageModel.Message);

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
            await RegisterUser(ConnectionId ?? hubConnection.ConnectionId);

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

        public async Task RegisterUser(string ConnectionId)
        {
            UserModel.ConnectionId = ConnectionId;

            GroupUsers = await hubConnection.InvokeAsync<IEnumerable<UserModel>>("RegisterUser", UserModel, Room);

            if (GroupUsers == null)
                await hubConnection.StopAsync();
            else
                await AddUserListAsync(GroupUsers);
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

            await CheckPermissionAndTryPublishNotification(Message);
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

            string ValidMessage = WebUtility.HtmlEncode(Message);

            if (MessageStatus == MessageStatus.None && Color != null)
            {
                return $"<div class=\"message-box\"><div class=\"message-header\"><div style=\"background:{Color}\" class=\"name\" title=\"{Username}\">{Username}</div><span title=\"{Date}\" class=\"date\">{Date}</span></div><pre class=\"message\">{ValidMessage}</pre></div>";
            }
            else
            {
                switch (MessageStatus)
                {
                    case MessageStatus.Success:
                        return $"<div class=\"message-box\"><div class=\"message-header\"><div class=\"name\" title=\"{Username}\"><i class=\"las la-shield-alt\"></i> {Username}</div><span title=\"{Date}\" class=\"date\">{Date}</span></div><p class=\"message message--success\">{ValidMessage}</p></div>";
                    case MessageStatus.Failure:
                        return $"<div class=\"message-box\"><div class=\"message-header\"><div class=\"name\" title=\"{Username}\"><i class=\"las la-shield-alt\"></i> {Username}</div><span title=\"{Date}\" class=\"date\">{Date}</span></div><p class=\"message message--failure\">{ValidMessage}</p></div>";
                    case MessageStatus.Information:
                        return $"<div class=\"message-box\"><div class=\"message-header\"><div class=\"name\" title=\"{Username}\"><i class=\"las la-shield-alt\"></i> {Username}</div><span title=\"{Date}\" class=\"date\">{Date}</span></div><p class=\"message message--information\">{ValidMessage}</p></div>";
                    case MessageStatus.Warning:
                        return $"<div class=\"message-box\"><div class=\"message-header\"><div class=\"name\" title=\"{Username}\"><i class=\"las la-shield-alt\"></i> {Username}</div><span title=\"{Date}\" class=\"date\">{Date}</span></div><p class=\"message message--warning\">{ValidMessage}</p></div>";
                    default:
                        return $"<div class=\"message-box\"><div class=\"message-header\"><div class=\"name\" title=\"{Username}\"><i class=\"las la-shield-alt\"></i> {Username}</div><span title=\"{Date}\" class=\"date\">{Date}</span></div><p class=\"message\">{ValidMessage}</p></div>";
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
            await hubConnection.DisposeAsync();
        }

        #endregion
    }
}