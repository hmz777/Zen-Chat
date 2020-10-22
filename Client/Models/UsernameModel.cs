using System.ComponentModel.DataAnnotations;

namespace MVCBlazorChatApp.Client.Models
{
    public class UsernameModel
    {
        [Required(ErrorMessage = "Please supply a username.")]
        [StringLength(15, MinimumLength = 1, ErrorMessage = "Username length must be between {2} and {1}.")]
        public string Username { get; set; }
    }
}