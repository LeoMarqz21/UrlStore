using AutoMapper;
using Microsoft.AspNetCore.Identity;
using UrlStore.Models;
using UrlStore.Models.ViewModels;

namespace UrlStore.Services
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            //        De             ->  A
            CreateMap<RegisterViewModel, User>();
            CreateMap<LoginViewModel, User>();
            CreateMap<User, ProfileViewModel>();
            CreateMap<ProfileViewModel, User>();
        }
    }
}
