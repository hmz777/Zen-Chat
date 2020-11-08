using Microsoft.AspNetCore.Mvc;

namespace MVCBlazorChatApp.Server.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class SettingsController : ControllerBase
    {
        public IActionResult SetDarkMode(bool Value)
        {
            string DModeValue = Value ? "true" : "false";

            Response.Cookies.Append("DMode", DModeValue);

            return Ok();
        }
    }
}