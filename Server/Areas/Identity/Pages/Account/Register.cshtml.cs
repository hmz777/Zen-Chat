using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MDProject.Helpers.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MVCBlazorChatApp.Server.Models;

namespace MVCBlazorChatApp.Server.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        public RegisterDTO RegisterDTO { get; set; }

        public void OnGet()
        {
        }

        public void OnPost()
        {
        }
    }

    public class RegisterDTO
    {
        [Required(ErrorMessage = "This field is required.")]
        [StringLength(10, MinimumLength = 6, ErrorMessage = "Username must be between {2} and {1}.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least {2} characters.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        [Display(Name = "Email address")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        public Gender Gender { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        [Display(Name = "Privacy Policy")]
        [MustBeTrue]
        public bool PrivacyPolicy { get; set; }
    }
}
