using MessagePack;
using MVCBlazorChatApp.Server.Helpers;
using System.ComponentModel.DataAnnotations;
using KeyAttribute = MessagePack.KeyAttribute;

namespace MVCBlazorChatApp.Client.Models
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class UserModel
    {
        public UserModel()
        {
            Color = HelperMethods.GenerateRandomColor();
        }

        [Required(ErrorMessage = "Please supply a username.")]
        [StringLength(15, MinimumLength = 1, ErrorMessage = "Username length must be between {2} and {1}.")]
        public string Username { get; set; }
        public string Color { get; set; }
        public string Room { get; set; }
        public string ConnectionId { get; set; }
    }
}