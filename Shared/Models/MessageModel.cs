using MessagePack;
using MVCBlazorChatApp.Shared.Models;
using System.ComponentModel.DataAnnotations;

namespace MVCBlazorChatApp.Shared.Models
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class MessageModel
    {
        [Required(ErrorMessage = "Message can not be empty.")]
        public string Message { get; set; } = string.Empty;

        public MessageStatus MessageStatus { get; set; }
    }
}