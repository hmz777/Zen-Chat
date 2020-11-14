using MessagePack;
using System.ComponentModel.DataAnnotations;
using KeyAttribute = MessagePack.KeyAttribute;

namespace MVCBlazorChatApp.Client.Models
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class MessageModel
    {
        [Required(ErrorMessage = "Message can not be empty!")]
        public string Message { get; set; } = string.Empty;
    }
}