using System.ComponentModel.DataAnnotations;

namespace MVCBlazorChatApp.Client.Models
{
    public class MessageModel
    {
        [Required(ErrorMessage = "Message can not be empty!")]
        public string Message { get; set; } = string.Empty;
    }
}