using AttaEduSystem.Models.DTOs.Authentication;
using AttaEduSystem.Models.DTOs.ExamFormat;
using AttaEduSystem.Models.DTOs.ExamPaper;
using AttaEduSystem.Models.DTOs.Student;
using AttaEduSystem.Models.Entities;
using AttaEduSystem.Services.IServices;
using AttaEduSystem.Utilities.Constants;
using AutoMapper;
using System.Text.Json;

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

            // ExamPaper mapping
            CreateMap<UploadExamPaperDto, ExamPaper>()
                .ForMember(dest => dest.ExamPaperId, opt => opt.Ignore())
                .ForMember(dest => dest.OriginalImageUrl, opt => opt.Ignore())
                .ForMember(dest => dest.ScannedText, opt => opt.Ignore())
                .ForMember(dest => dest.ExamFormat, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedTime, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedTime, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore());

            CreateMap<ExamPaper, ScanExamPaperResponseDto>()
                .ForMember(dest => dest.ExamPaperId, opt => opt.MapFrom(src => src.ExamPaperId))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.OriginalImageUrl))
                .ForMember(dest => dest.ExamFormat, opt => opt.MapFrom(src => src.ExamFormat))
                //.ForMember(dest => dest.ExamFormatParsed,
                //        opt => opt.MapFrom(src => DeserializeExamFormat(src.ExamFormat)))
                .ForMember(dest => dest.ScannedBy, opt => opt.MapFrom(src => src.CreatedBy))
                .ForMember(dest => dest.ScannedAt, opt => opt.MapFrom(src => src.CreatedTime ?? StaticOperationStatus.Timezone.Vietnam));

            CreateMap<ExamPaper, GetExamPaperDto>()
                .ForMember(dest => dest.ExamPaperId, opt => opt.MapFrom(src => src.ExamPaperId))
                .ForMember(dest => dest.ExamFormat, opt => opt.MapFrom(src => src.ExamFormat))
                //.ForMember(dest => dest.ExamFormatParsed,
                //        opt => opt.MapFrom(src => DeserializeExamFormat(src.ExamFormat)))
                .ForMember(dest => dest.CreatedBy,
                        opt => opt.MapFrom(src => src.CreatedBy ?? string.Empty))
                .ForMember(dest => dest.CreatedTime,
                        opt => opt.MapFrom(src => src.CreatedTime ?? StaticOperationStatus.Timezone.Vietnam));




        }
            //private static ExamFormatSchema? DeserializeExamFormat(string? json)
            //{
            //    if (string.IsNullOrWhiteSpace(json)) return null;
            //        try
            //        {
            //            return JsonSerializer.Deserialize<ExamFormatSchema>(json);
            //        }
            //        catch
            //        {
            //            return null; // tránh vỡ DTO nếu JSON lỗi
            //        }
            //}

    }
}
