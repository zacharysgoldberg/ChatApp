using API.DTOs;
using API.Entities;
using AutoMapper;

namespace API.Helpers;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<AppUser, MemberDTO>()
            .ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src => src.Photo.Url));

        CreateMap<AppUser, Contact>();

        CreateMap<Contact, ContactDTO>();

        CreateMap<Photo, PhotoDTO>();
        
        CreateMap<MemberUpdateDTO, AppUser>();
    }
}
