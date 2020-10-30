using MVCBlazorChatApp.Server.Helpers;
using System.ComponentModel.DataAnnotations;

namespace MVCBlazorChatApp.Client.Models
{
    public class UserModel
    {
        public UserModel()
        {
            Color = HelperMethods.GenerateRandomColor();
        }

        public string Color { get; set; }

        [Required(ErrorMessage = "Please supply a username.")]
        [StringLength(15, MinimumLength = 1, ErrorMessage = "Username length must be between {2} and {1}.")]
        public string Username { get; set; }

        public string Room { get; set; }

        public string ConnectionId { get; set; }
    }
}