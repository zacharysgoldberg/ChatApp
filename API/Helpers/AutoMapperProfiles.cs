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

		CreateMap<AppUser, ContactDTO>()
			.ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src => src.Photo.Url));

		CreateMap<MemberDTO, ContactDTO>();

		CreateMap<Photo, PhotoDTO>();

		CreateMap<MemberUpdateDTO, AppUser>();

		CreateMap<Message, MessageDTO>()
			.ForMember(dest => dest.SenderPhotoUrl, opt => opt.MapFrom(src => src.Sender.Photo.Url))
			.ForMember(dest => dest.RecipientPhotoUrl, opt => opt.MapFrom(src => src.Recipient.Photo.Url));

		CreateMap<GroupMessage, GroupMessageDTO>()
			.ForMember(dest => dest.SenderUsername, opt => opt.MapFrom(src => src.Sender.UserName))
			.ForMember(dest => dest.SenderPhotoUrl, opt => opt.MapFrom(src => src.Sender.Photo.Url))
			.ForMember(dest => dest.Contacts, opt => opt.MapFrom(src => src.Users));
	}
}
