using AttaEduSystem.Models.DTOs.Authentication;
using AttaEduSystem.Models.DTOs.Student;
using AttaEduSystem.Models.Entities;
using AutoMapper;

namespace AttaEduSystem.Services.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // SignUpStudentDTO to ApplicationUser
            CreateMap<SignUpStudentDto, ApplicationUser>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => src.BirthDate))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => string.Empty))
                .ForMember(dest => dest.LockoutEnabled, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.EmailConfirmed, opt => opt.MapFrom(src => false));

            CreateMap<SignUpStudentDto, Student>();

            // SignUpTeacherDTO to ApplicationUser
            CreateMap<SignUpTeacherDto, ApplicationUser>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => src.BirthDate))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => string.Empty))
                .ForMember(dest => dest.LockoutEnabled, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.EmailConfirmed, opt => opt.MapFrom(src => false));

            // Student to GetStudentDto
            CreateMap<Student, GetStudentDto>()
                .ForMember(dest => dest.StudentId, opt => opt.MapFrom(src => src.StudentId))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.ApplicationUser.FullName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.ApplicationUser.Email))
                .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => src.ApplicationUser.BirthDate))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.ApplicationUser.Gender))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.ApplicationUser.PhoneNumber))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.ApplicationUser.Address))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ApplicationUser.ImageUrl));

            // UpdateUserProfileDto to ApplicationUser
            CreateMap<UpdateUserProfileDto, ApplicationUser>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
                .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => src.BirthDate))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageUrl))
                .ReverseMap();


            ////// Add more mappings as needed
        }
    }
}
