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

        CreateMap<MemberDTO, ContactDTO>();

        CreateMap<Photo, PhotoDTO>();
        
        CreateMap<MemberUpdateDTO, AppUser>();

        CreateMap<Message, MessageDTO>()
            .ForMember(dest => dest.SenderPhotoUrl, 
                opt => opt.MapFrom(src => src.Sender.Photo.Url));
        CreateMap<Message, MessageDTO>()
            .ForMember(dest => dest.RecipientPhotoUrl, 
                opt => opt.MapFrom(src => src.Sender.Photo.Url));
    }
}
