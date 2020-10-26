﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVCBlazorChatApp.Shared.Models
{
    public enum MessageStatus : int
    {
        Success = 1,
        Failure = 0,
        Warning = 2,
        Information = 3
    }
}