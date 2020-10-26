using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MVCBlazorChatApp.Shared.Models;

namespace MVCBlazorChatApp.Server.Helpers.StatusMessages
{
    public class StatusMessage
    {
        public MessageStatus MessageStatus { get; set; }
        public object Message { get; set; }
        public string Link { get; set; }
    }
}
