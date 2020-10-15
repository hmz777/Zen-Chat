using AutoMapper;
using MVCBlazorChatApp.Server.Areas.Identity.Pages.Account;
using MVCBlazorChatApp.Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVCBlazorChatApp.Server.Helpers.MappingProfiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ApplicationUser, RegisterDTO>().ReverseMap();
        }
    }
}
