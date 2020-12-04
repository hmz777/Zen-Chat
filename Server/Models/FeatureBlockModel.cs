using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVCBlazorChatApp.Server.Models
{
    public class FeatureBlockModel
    {
        public string Text { get; set; }
        public string ImageUrl { get; set; }
        public string ImageAlt { get; set; }
        public string Markdown { get; set; }
        public bool IsRight { get; set; }
        public string Animation { get; set; }
    }
}
