using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MDProject.Helpers.Attributes;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MVCBlazorChatApp.Server.Helpers.StatusMessages;
using MVCBlazorChatApp.Server.Models;
using MVCBlazorChatApp.Shared.Models;

namespace MVCBlazorChatApp.Server.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IMapper mapper;

        public RegisterModel(UserManager<ApplicationUser> userManager, IMapper mapper)
        {
            this.userManager = userManager;
            this.mapper = mapper;
        }

        public RegisterDTO RegisterDTO { get; set; }

        public void OnGet()
        {
            ViewData["Account"] = true;
        }

        public async Task<IActionResult> OnPost(RegisterDTO registerDTO)
        {
            //Short circuit the process temporarily
            return new OkObjectResult(new StatusMessage { MessageStatus = MessageStatus.Failure, Message = "Accounts feature is not available yet." });

            if (!ModelState.IsValid)
                return new BadRequestObjectResult(new StatusMessage { MessageStatus = MessageStatus.Failure, Message = "Invalid login." });

            ApplicationUser NewUser = mapper.Map<ApplicationUser>(registerDTO);

            IdentityResult Result = await userManager.CreateAsync(NewUser, registerDTO.Password);

            if (Result.Succeeded)
            {
                TempData.Put<string>("Username", NewUser.UserName);

                return new OkObjectResult(new StatusMessage
                {
                    MessageStatus = MessageStatus.Success,
                    Message = "Account registration successfull.",
                    Link = Url.PageLink("ConfirmAccount")
                });
            }
            else
            {
                string[] ErrorMessages = Result.Errors.Select(e => e.Description).ToArray();

                return new OkObjectResult(new StatusMessage { MessageStatus = MessageStatus.Failure, Message = ErrorMessages });
            }
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

        [Display(Name = "Privacy Policy")]
        [MustBeTrue(ErrorMessage = "Please accept our privacy policy.")]
        public bool PrivacyPolicy { get; set; }
    }
}
