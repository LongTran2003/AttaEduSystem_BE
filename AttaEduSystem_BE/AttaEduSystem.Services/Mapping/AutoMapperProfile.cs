using AttaEduSystem.Models.DTOs.Admin;
using AttaEduSystem.Models.DTOs.Authentication;
using AttaEduSystem.Models.DTOs.Billing;
using AttaEduSystem.Models.DTOs.ExamFormat;
using AttaEduSystem.Models.DTOs.ExamPaper;
using AttaEduSystem.Models.DTOs.ExamResult;
using AttaEduSystem.Models.DTOs.ExamTaking;
using AttaEduSystem.Models.DTOs.Folder;
using AttaEduSystem.Models.DTOs.GeminiAi;
using AttaEduSystem.Models.DTOs.Openai;
using AttaEduSystem.Models.DTOs.Payment;
using AttaEduSystem.Models.DTOs.Profile;
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

            // Admin: ApplicationUser to GetUserDto
            CreateMap<ApplicationUser, GetUserDto>()
                .ForMember(dest => dest.Roles, opt => opt.Ignore()) // Roles cần query riêng (manual mapping), ignore để tránh lỗi
                .ForMember(dest => dest.StudentCode, opt => opt.Ignore()) // Map manually
                .ForMember(dest => dest.TeacherCode, opt => opt.Ignore()) // Map manually
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore()) // Chưa có logic nên ignore
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()); // Chưa có logic nên ignore

            // Admin: UpdateUserDto to ApplicationUser
            CreateMap<UpdateUserDto, ApplicationUser>()
                .ForMember(dest => dest.FullName, opt => opt.Condition(src => !string.IsNullOrEmpty(src.FullName)))
                .ForMember(dest => dest.PhoneNumber, opt => opt.Condition(src => !string.IsNullOrEmpty(src.PhoneNumber)))
                .ForMember(dest => dest.Address, opt => opt.Condition(src => !string.IsNullOrEmpty(src.Address)))
                .ForMember(dest => dest.Gender, opt => opt.Condition(src => !string.IsNullOrEmpty(src.Gender)))
                .ForMember(dest => dest.BirthDate, opt => opt.Condition(src => src.BirthDate.HasValue))
                .ForMember(dest => dest.ImageUrl, opt => opt.Condition(src => !string.IsNullOrEmpty(src.ImageUrl)))
                .ForMember(dest => dest.Status, opt => opt.Condition(src => !string.IsNullOrEmpty(src.Status)));

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
                .ForMember(dest => dest.ScannedBy, opt => opt.MapFrom(src => src.Creator != null ? src.Creator.FullName : "Unknown"))
                .ForMember(dest => dest.ScannedAt, opt => opt.MapFrom(src => src.CreatedTime ?? StaticOperationStatus.Timezone.Vietnam));

            CreateMap<ExamPaper, GetExamPaperDto>()
                .ForMember(dest => dest.ExamPaperId, opt => opt.MapFrom(src => src.ExamPaperId))
                .ForMember(dest => dest.ExamFormat, opt => opt.MapFrom(src => src.ExamFormat))
                .ForMember(dest => dest.CreatedBy,
                        opt => opt.MapFrom(src => src.Creator != null ? src.Creator.FullName : "Unknown"))
                .ForMember(dest => dest.CreatedTime,
                        opt => opt.MapFrom(src => src.CreatedTime ?? StaticOperationStatus.Timezone.Vietnam));

            // GeneratedExamPaper mapping
            CreateMap<GeneratedExamPaper, GenerateExamResponseDto>()
                .ForMember(dest => dest.GeneratedExamId, opt => opt.MapFrom(src => src.GeneratedExamPaperId))
                .ForMember(dest => dest.GeneratedContent, opt => opt.MapFrom(src => src.GeneratedContentJson))
                .ForMember(dest => dest.AiModelUsed, opt => opt.MapFrom(src => src.AiModelUsed))
                .ForMember(dest => dest.PromptSnapshot, opt => opt.MapFrom(src => src.PromptSnapshot))
                .ForMember(dest => dest.GeneratedAt, opt => opt.MapFrom(src => src.CreatedTime ?? StaticOperationStatus.Timezone.Vietnam));

            CreateMap<GeneratedExamPaper, GeneratedExamDto>() // nếu bạn muốn DTO riêng để list/view
                .ForMember(dest => dest.GeneratedExamId, opt => opt.MapFrom(src => src.GeneratedExamPaperId))
                .ForMember(dest => dest.GeneratedAt, opt => opt.MapFrom(src => src.CreatedTime ?? StaticOperationStatus.Timezone.Vietnam));

            // Map từ QuestionItem -> Entity ExamQuestion
            CreateMap<QuestionItem, Models.Entities.ExamQuestion>()
                .ForMember(dest => dest.QuestionId, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.QuestionIdLabel, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
                .ForMember(dest => dest.Points, opt => opt.MapFrom(src => src.Points))
                .ForMember(dest => dest.QuestionType, opt => opt.MapFrom(src => 
                    !string.IsNullOrEmpty(src.Type) ? src.Type : 
                        ((src.Options != null && src.Options.Any()) ? "MultipleChoice" : "Essay")))

                .ForMember(dest => dest.Options, opt => opt.MapFrom(src => MapOptions(src.Options)));

            // ExamSolution mapping
            CreateMap<ExamSolution, ExamSolutionResponseDto>()
                .ForMember(dest => dest.SolvedAt, opt => opt.MapFrom(src => src.CreatedTime));

            // Payment mapping
            CreateMap<Payment, GetAllPaymentDto>()
                .ForMember(dest => dest.PaymentTransactionId, opt => opt.MapFrom(src => src.PaymentTransactionId))
                .ForMember(dest => dest.OrderNumber, opt => opt.MapFrom(src => (long?)src.OrderNumber))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));

            // SubscriptionPlan mapping
            CreateMap<SubscriptionPlan, GetSubscriptionPlanDto>();

            CreateMap<UserSubscription, GetUserSubscriptionDto>()
                .ForMember(dest => dest.Plan, opt => opt.MapFrom(src => src.Plan));
            
            // ExamTaking mapping
                    // 1. Map Entity -> History DTO
            CreateMap<ExamAttempt, ExamHistoryDto>()
                .ForMember(dest => dest.ExamTitle, 
                    opt => opt.MapFrom(src => src.ExamPaper != null ? src.ExamPaper.Title : "Unknown Exam"))
                .ForMember(dest => dest.Subject, 
                    opt => opt.MapFrom(src => src.ExamPaper != null ? src.ExamPaper.Subject : "General"))
                .ForMember(dest => dest.Duration, 
                    opt => opt.MapFrom(src => src.CompletedAt.HasValue 
                        ? $"{(src.CompletedAt.Value - src.StartedAt).TotalMinutes:0} mins" 
                        : "N/A"));
                    
                    // 2. Map Entity Detail -> Result Detail DTO
            CreateMap<ExamAttemptDetail, ExamResultDetailDto>()
                .ForMember(dest => dest.QuestionContent, 
                    opt => opt.MapFrom(src => src.ExamQuestion != null ? src.ExamQuestion.Content : "Question removed"))
                .ForMember(dest => dest.QuestionIndex, 
                    opt => opt.MapFrom(src => src.ExamQuestion != null ? src.ExamQuestion.OrderIndex : 0))
                .ForMember(dest => dest.CorrectAnswer, 
                    opt => opt.MapFrom(src => src.ExamQuestion != null ? src.ExamQuestion.CorrectAnswer : ""))
                .ForMember(dest => dest.UserAnswer, 
                    opt => opt.MapFrom(src => src.UserAnswer ?? ""))
                // Nếu sau này có Explanation thì map thêm vào đây
                .ForMember(dest => dest.Explanation, opt => opt.Ignore());
            
                    // 3. Map Entity Attempt -> Result DTO (Bao gồm cả list Details)
            CreateMap<ExamAttempt, ExamResultDto>()
                .ForMember(dest => dest.ExamTitle, 
                    opt => opt.MapFrom(src => src.ExamPaper != null ? src.ExamPaper.Title : "Unknown"))
                .ForMember(dest => dest.CompletedAt, 
                    opt => opt.MapFrom(src => src.CompletedAt ?? DateTime.UtcNow))
                .ForMember(dest => dest.Details, 
                    opt => opt.MapFrom(src => src.Details.OrderBy(d => d.ExamQuestion.OrderIndex)));
            
            //  Question mapping
                    // 1. Map QuestionOption -> DTO
            CreateMap<QuestionOption, QuestionOptionDto>();

                    // 2. Map ExamQuestion -> DTO
            CreateMap<ExamQuestion, ExamQuestionResponseDto>()
                // Map options nếu có
                .ForMember(dest => dest.Options, opt => opt.MapFrom(src => src.Options)); 

                    // 3. Cập nhật Map ExamPaper -> GetExamPaperDto
            CreateMap<ExamPaper, GetExamPaperDto>()
                // ... các trường cũ ...
                .ForMember(dest => dest.Questions, opt => opt.MapFrom(src => src.Questions));
            
            // ExamFolder mapping
            CreateMap<ExamFolder, FolderDto>()
                .ForMember(dest => dest.FolderId, opt => opt.MapFrom(src => src.FolderId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.ColorCode, opt => opt.MapFrom(src => src.ColorCode))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
                .ForMember(dest => dest.CreatedTime, opt => opt.MapFrom(src => src.CreatedTime))
                // Map số lượng đề thi đang có trong folder
                .ForMember(dest => dest.ExamCount, opt => opt.MapFrom(src => src.ExamPapers.Count));

            CreateMap<CreateFolderDto, ExamFolder>();



        }


        // --- Helper để tách chuỗi Options: "A. Nội dung" -> Label: A, Content: Nội dung ---
        private List<QuestionOption> MapOptions(List<string>? sourceOptions)
        {
            var result = new List<QuestionOption>();
            if (sourceOptions == null) return result;

            foreach (var optStr in sourceOptions)
            {
                var parts = optStr.Split(new[] { '.', ')' }, 2);
                result.Add(new QuestionOption
                {
                    OptionId = Guid.NewGuid(),
                    Label = parts.Length > 0 ? parts[0].Trim() : "",
                    Content = parts.Length > 1 ? parts[1].Trim() : optStr,
                    IsCorrect = false // Mặc định false, user sẽ chỉnh sau
                });
            }
            return result;
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
