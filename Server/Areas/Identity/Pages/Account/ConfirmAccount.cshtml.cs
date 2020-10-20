using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using MVCBlazorChatApp.Server.Helpers.StatusMessages;
using MVCBlazorChatApp.Server.Models;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVCBlazorChatApp.Server.Areas.Identity.Pages.Account
{
    public class ConfirmAccountModel : PageModel
    {
        private readonly UserManager<ApplicationUser> userManager;

        public ConfirmAccountModel(UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
        }

        public string ActivationLink { get; set; }

        public async Task<IActionResult> OnGet()
        {
            string Username = TempData.Get<string>("Username");

            if (Username == null)
                return RedirectToPage("/Index");

            ApplicationUser user = await userManager.FindByNameAsync(Username);

            if (user == null)
                return RedirectToPage("/Index");

            if (user.EmailConfirmed)
            {
                TempData.Put<StatusMessage>("Message", new StatusMessage { MessageStatus = MessageStatus.Failure, Message = "User already activated." });
                return RedirectToPage("/Index");
            }

            string UserToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
            string EncodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(UserToken));

            ActivationLink = Url.PageLink("ConfirmAccount", "VerifyAccount", new { Email = user.Email, Token = EncodedToken });

            return Page();
        }

        public async Task<IActionResult> OnGetVerifyAccount(string Email, string Token)
        {
            if (!ModelState.IsValid)
                return RedirectToPage("/Index");

            var user = await userManager.FindByEmailAsync(Email);

            if (user == null)
                return RedirectToPage("/Index");

            string DecodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(Token));

            var Result = await userManager.ConfirmEmailAsync(user, DecodedToken);

            if (Result.Succeeded)
            {
                TempData.Put<StatusMessage>("Message"
                    , new StatusMessage { MessageStatus = MessageStatus.Success, Message = "Account verified successfully." });
            }
            else
            {
                string Errors = string.Empty;

                foreach (var error in Result.Errors.Select(e => e.Description))
                {
                    Errors += $"- {error} <br />";
                }

                TempData.Put<StatusMessage>("Message"
                   , new StatusMessage { MessageStatus = MessageStatus.Failure, Message = Errors });
            }

            return RedirectToPage("/Index");
        }
    }
}
