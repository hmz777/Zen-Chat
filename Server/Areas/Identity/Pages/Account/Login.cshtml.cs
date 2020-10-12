using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MVCBlazorChatApp.Server.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        public LoginDto LoginDto { get; set; }

        public void OnGet()
        {
        }

        public void OnPost()
        {

        }
    }

    public class LoginDto
    {
        [Required]
        [Display(Name = "Username or email")]
        public string UsernameOrEmail { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }
}
