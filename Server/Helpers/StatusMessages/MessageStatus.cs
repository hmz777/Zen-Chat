using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVCBlazorChatApp.Server.Helpers.StatusMessages
{
    public enum MessageStatus : int
    {
        Success = 1,
        Failure = 0,
        Warning = 2
    }
}
