using AttaEduSystem.DataAccess.IRepositories;
using AttaEduSystem.Models.DTOs;
using AttaEduSystem.Models.DTOs.LearningClass;
using AttaEduSystem.Models.Entities;
using AttaEduSystem.Services.Helpers.Responses;
using AttaEduSystem.Services.IServices;
using AttaEduSystem.Utilities.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Security.Claims;

namespace AttaEduSystem.Services.Services;

public class LearningClassService : ILearningClassService
{
    private const string ActiveStatus = "Active";
    private const string InactiveStatus = "Inactive";
    private const string LegacyActiveStatus = "1";
    private const string LegacyInactiveStatus = "0";
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;

    public LearningClassService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    public async Task<ResponseDto> CreateClass(CreateLearningClassDto dto, ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return ErrorResponse.Build("Unauthorized", 401);

        var creator = await _userManager.FindByIdAsync(userId);
        if (creator == null)
            return ErrorResponse.Build("User not found", 404);

        var studentIdsFromCreate = dto.StudentUserIds?.Where(x => !string.IsNullOrWhiteSpace(x)).ToList() ?? new List<string>();
        if (studentIdsFromCreate.Any())
            return ErrorResponse.Build("Students must join class by enroll key. Please remove studentUserIds when creating class.", 400);

        var learningClass = new LearningClass
        {
            LearningClassId = Guid.NewGuid(),
            Name = dto.Name.Trim(),
            SchoolYear = dto.SchoolYear,
            Description = dto.Description,
            Schedule = dto.Schedule,
            OwnerUserId = userId,
            EnrollKey = await GenerateUniqueEnrollKeyAsync(),
            CreatedBy = userId,
            CreatedTime = StaticOperationStatus.Timezone.Vietnam,
            Status = ActiveStatus
        };

        await _unitOfWork.LearningClass.AddAsync(learningClass);

        var membersToAdd = new List<LearningClassMember>();
        var memberUserIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { userId };
        membersToAdd.Add(BuildMember(learningClass.LearningClassId, userId, StaticUserRoles.Teacher, userId));

        var teacherIds = dto.TeacherUserIds?.Distinct().Where(x => !string.IsNullOrWhiteSpace(x)).ToList() ?? new List<string>();
        var studentIds = dto.StudentUserIds?.Distinct().Where(x => !string.IsNullOrWhiteSpace(x)).ToList() ?? new List<string>();

        foreach (var teacherId in teacherIds.Where(t => t != userId))
        {
            if (memberUserIds.Contains(teacherId))
                continue;
            if (await IsUserInRoleAsync(teacherId, StaticUserRoles.Teacher))
            {
                membersToAdd.Add(BuildMember(learningClass.LearningClassId, teacherId, StaticUserRoles.Teacher, userId));
                memberUserIds.Add(teacherId);
            }
            else
            {
                return ErrorResponse.Build($"Teacher user id is invalid or not in Teacher role: {teacherId}", 400);
            }
        }

        foreach (var studentId in studentIds)
        {
            if (memberUserIds.Contains(studentId))
                continue;
            if (await IsUserInRoleAsync(studentId, StaticUserRoles.Student))
            {
                membersToAdd.Add(BuildMember(learningClass.LearningClassId, studentId, StaticUserRoles.Student, userId));
                memberUserIds.Add(studentId);
            }
            else
            {
                return ErrorResponse.Build($"Student user id is invalid or not in Student role: {studentId}", 400);
            }
        }

        await _unitOfWork.LearningClassMember.AddRangeAsync(membersToAdd);
        try
        {
            await _unitOfWork.SaveAsync();
        }
        catch (DbUpdateException ex)
        {
            return ErrorResponse.Build($"Failed to create class due to invalid data: {ex.InnerException?.Message ?? ex.Message}", 400);
        }

        return await GetClassDetail(learningClass.LearningClassId, user);
    }

    public async Task<ResponseDto> GetMyClasses(ClaimsPrincipal user, string? keyword = null, string? status = null)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return ErrorResponse.Build("Unauthorized", 401);

        var fullName = user.FindFirstValue("FullName");
        var roles = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
        var isAdmin = roles.Contains(StaticUserRoles.Admin) || roles.Contains("Admin");

        List<LearningClass> classes;
        if (isAdmin)
        {
            classes = await _unitOfWork.LearningClass.GetAllWithMembersAsync();
        }
        else
        {
            classes = await _unitOfWork.LearningClass.GetByOwnerAsync(userId);
            var memberClasses = await _unitOfWork.LearningClassMember.GetByUserAsync(userId);
            var memberClassIds = memberClasses.Select(m => m.LearningClassId).ToHashSet();
            var missingClassIds = memberClassIds.Where(id => classes.All(c => c.LearningClassId != id)).ToList();

            foreach (var classId in missingClassIds)
            {
                var item = await _unitOfWork.LearningClass.GetByIdWithMembersAsync(classId);
                if (item != null)
                    classes.Add(item);
            }
        }

        var summaries = new List<LearningClassSummaryDto>();
        foreach (var item in classes.OrderByDescending(c => c.CreatedTime))
        {
            summaries.Add(await BuildSummaryDto(item));
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var normalizedKeyword = keyword.Trim().ToLowerInvariant();
            summaries = summaries
                .Where(x => x.Name.ToLowerInvariant().Contains(normalizedKeyword) ||
                            (x.SchoolYear ?? "").ToLowerInvariant().Contains(normalizedKeyword))
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            summaries = summaries
                .Where(x => string.Equals(x.Status, status, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        return SuccessResponse.Build("Classes retrieved successfully", 200, summaries);
    }

    public async Task<ResponseDto> GetClassDetail(Guid classId, ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return ErrorResponse.Build("Unauthorized", 401);

        var learningClass = await _unitOfWork.LearningClass.GetByIdWithMembersAsync(classId);
        if (learningClass == null)
            return ErrorResponse.Build("Class not found", 404);

        if (!await CanManageOrViewClass(user, learningClass, userId))
            return ErrorResponse.Build("You do not have permission to view this class", 403);
        var canManage = await CanManageClass(user, learningClass, userId);

        var summary = await BuildSummaryDto(learningClass);
        var detail = new LearningClassDetailDto
        {
            LearningClassId = summary.LearningClassId,
            Name = summary.Name,
            SchoolYear = summary.SchoolYear,
            Description = summary.Description,
            Schedule = summary.Schedule,
            Status = summary.Status,
            StudentCount = summary.StudentCount,
            TeacherCount = summary.TeacherCount,
            ExamRoomCount = summary.ExamRoomCount,
            OwnerUserId = learningClass.OwnerUserId,
            OwnerName = learningClass.OwnerUser?.FullName ?? learningClass.OwnerUser?.UserName ?? "Unknown",
            EnrollKey = canManage ? learningClass.EnrollKey : null,
            Members = learningClass.Members
                .OrderBy(m => m.Role)
                .ThenBy(m => m.User?.FullName)
                .Select(m => new LearningClassMemberDto
                {
                    LearningClassMemberId = m.LearningClassMemberId,
                    UserId = m.UserId,
                    FullName = m.User?.FullName ?? "Unknown",
                    Email = m.User?.Email ?? "",
                    Role = m.Role,
                    JoinedAt = m.JoinedAt
                }).ToList()
        };

        return SuccessResponse.Build("Class detail retrieved successfully", 200, detail);
    }

    public async Task<ResponseDto> UpdateClass(Guid classId, UpdateLearningClassDto dto, ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return ErrorResponse.Build("Unauthorized", 401);

        var learningClass = await _unitOfWork.LearningClass.GetByIdWithMembersAsync(classId);
        if (learningClass == null)
            return ErrorResponse.Build("Class not found", 404);

        if (!await CanManageClass(user, learningClass, userId))
            return ErrorResponse.Build("You do not have permission to update this class", 403);

        if (!string.IsNullOrWhiteSpace(dto.Name))
            learningClass.Name = dto.Name.Trim();
        if (dto.SchoolYear != null)
            learningClass.SchoolYear = dto.SchoolYear;
        if (dto.Description != null)
            learningClass.Description = dto.Description;
        if (dto.Schedule != null)
            learningClass.Schedule = dto.Schedule;
        if (!string.IsNullOrWhiteSpace(dto.Status))
            learningClass.Status = NormalizeClassStatus(dto.Status);

        learningClass.UpdatedBy = userId;
        learningClass.UpdatedTime = StaticOperationStatus.Timezone.Vietnam;
        _unitOfWork.LearningClass.Update(learningClass);
        await _unitOfWork.SaveAsync();

        return await GetClassDetail(classId, user);
    }

    public async Task<ResponseDto> DeleteClass(Guid classId, ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return ErrorResponse.Build("Unauthorized", 401);

        var learningClass = await _unitOfWork.LearningClass.GetByIdWithMembersAsync(classId);
        if (learningClass == null)
            return ErrorResponse.Build("Class not found", 404);

        if (!await CanManageClass(user, learningClass, userId))
            return ErrorResponse.Build("You do not have permission to delete this class", 403);

        learningClass.Status = InactiveStatus;
        learningClass.UpdatedBy = userId;
        learningClass.UpdatedTime = StaticOperationStatus.Timezone.Vietnam;
        _unitOfWork.LearningClass.Update(learningClass);
        await _unitOfWork.SaveAsync();

        return SuccessResponse.Build("Class deleted successfully", 200);
    }

    public async Task<ResponseDto> AddMembers(Guid classId, AddLearningClassMembersDto dto, ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return ErrorResponse.Build("Unauthorized", 401);

        var learningClass = await _unitOfWork.LearningClass.GetByIdWithMembersAsync(classId);
        if (learningClass == null)
            return ErrorResponse.Build("Class not found", 404);

        if (!await CanManageClass(user, learningClass, userId))
            return ErrorResponse.Build("You do not have permission to update members", 403);

        var existingUserIds = learningClass.Members.Select(m => m.UserId).ToHashSet();
        var createdBy = userId;
        var membersToAdd = new List<LearningClassMember>();

        var teacherIds = dto.TeacherUserIds?.Distinct().Where(x => !string.IsNullOrWhiteSpace(x)).ToList() ?? new List<string>();
        var studentIds = dto.StudentUserIds?.Distinct().Where(x => !string.IsNullOrWhiteSpace(x)).ToList() ?? new List<string>();
        if (studentIds.Any())
            return ErrorResponse.Build("Students must join by enroll key. Adding students directly is disabled.", 400);

        foreach (var teacherId in teacherIds)
        {
            if (existingUserIds.Contains(teacherId))
                continue;
            if (!await IsUserInRoleAsync(teacherId, StaticUserRoles.Teacher))
                return ErrorResponse.Build($"Teacher user id is invalid or not in Teacher role: {teacherId}", 400);

            membersToAdd.Add(BuildMember(classId, teacherId, StaticUserRoles.Teacher, createdBy));
            existingUserIds.Add(teacherId);
        }

        foreach (var studentId in studentIds)
        {
            if (existingUserIds.Contains(studentId))
                continue;
            if (!await IsUserInRoleAsync(studentId, StaticUserRoles.Student))
                return ErrorResponse.Build($"Student user id is invalid or not in Student role: {studentId}", 400);

            membersToAdd.Add(BuildMember(classId, studentId, StaticUserRoles.Student, createdBy));
            existingUserIds.Add(studentId);
        }

        if (!membersToAdd.Any())
            return ErrorResponse.Build("No valid members to add", 400);

        await _unitOfWork.LearningClassMember.AddRangeAsync(membersToAdd);
        try
        {
            await _unitOfWork.SaveAsync();
        }
        catch (DbUpdateException ex)
        {
            return ErrorResponse.Build($"Failed to add members due to invalid data: {ex.InnerException?.Message ?? ex.Message}", 400);
        }

        return await GetClassDetail(classId, user);
    }

    public async Task<ResponseDto> JoinClassByEnrollKey(JoinClassByEnrollKeyDto dto, ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return ErrorResponse.Build("Unauthorized", 401);

        if (!await IsUserInRoleAsync(userId, StaticUserRoles.Student))
            return ErrorResponse.Build("Only students can join class by enroll key", 403);

        var enrollKey = dto.EnrollKey.Trim().ToUpperInvariant();
        var learningClass = await _unitOfWork.LearningClass.GetAsync(c =>
            c.EnrollKey.ToUpper() == enrollKey && !IsInactiveStatus(c.Status));
        if (learningClass == null)
            return ErrorResponse.Build("Invalid enroll key", 404);

        var existingMember = await _unitOfWork.LearningClassMember.GetByClassAndUserAsync(learningClass.LearningClassId, userId);
        if (existingMember != null)
            return SuccessResponse.Build("You already joined this class", 200, new { learningClass.LearningClassId, learningClass.Name });

        var member = BuildMember(learningClass.LearningClassId, userId, StaticUserRoles.Student, userId);
        await _unitOfWork.LearningClassMember.AddAsync(member);
        await _unitOfWork.SaveAsync();

        return SuccessResponse.Build("Joined class successfully", 200, new { learningClass.LearningClassId, learningClass.Name });
    }

    public async Task<ResponseDto> RegenerateEnrollKey(Guid classId, ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return ErrorResponse.Build("Unauthorized", 401);

        var learningClass = await _unitOfWork.LearningClass.GetByIdWithMembersAsync(classId);
        if (learningClass == null)
            return ErrorResponse.Build("Class not found", 404);

        if (!await CanManageClass(user, learningClass, userId))
            return ErrorResponse.Build("You do not have permission to regenerate enroll key", 403);

        learningClass.EnrollKey = await GenerateUniqueEnrollKeyAsync();
        learningClass.UpdatedBy = userId;
        learningClass.UpdatedTime = StaticOperationStatus.Timezone.Vietnam;
        _unitOfWork.LearningClass.Update(learningClass);
        await _unitOfWork.SaveAsync();

        return SuccessResponse.Build("Enroll key regenerated successfully", 200, new
        {
            learningClass.LearningClassId,
            learningClass.EnrollKey
        });
    }

    public async Task<ResponseDto> RemoveMember(Guid classId, string memberUserId, ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return ErrorResponse.Build("Unauthorized", 401);

        var learningClass = await _unitOfWork.LearningClass.GetByIdWithMembersAsync(classId);
        if (learningClass == null)
            return ErrorResponse.Build("Class not found", 404);

        if (!await CanManageClass(user, learningClass, userId))
            return ErrorResponse.Build("You do not have permission to update members", 403);

        if (learningClass.OwnerUserId == memberUserId)
            return ErrorResponse.Build("Owner teacher cannot be removed from class", 400);

        var member = await _unitOfWork.LearningClassMember.GetByClassAndUserAsync(classId, memberUserId);
        if (member == null)
            return ErrorResponse.Build("Member not found in class", 404);

        _unitOfWork.LearningClassMember.Remove(member);
        await _unitOfWork.SaveAsync();

        return await GetClassDetail(classId, user);
    }

    public async Task<ResponseDto> GetAdminClassOverview()
    {
        var classes = await _unitOfWork.LearningClass.GetAllWithMembersAsync();
        var summaries = new List<LearningClassSummaryDto>();
        foreach (var item in classes.OrderByDescending(c => c.CreatedTime))
        {
            summaries.Add(await BuildSummaryDto(item));
        }

        return SuccessResponse.Build("Admin class overview retrieved successfully", 200, new
        {
            TotalClasses = summaries.Count,
            TotalStudents = summaries.Sum(x => x.StudentCount),
            TotalTeachers = summaries.Sum(x => x.TeacherCount),
            Classes = summaries
        });
    }

    public async Task<ResponseDto> GetAdminClassDashboard(int? month, int? year, Guid? classId, string? subject)
    {
        var now = StaticOperationStatus.Timezone.Vietnam;
        var selectedMonth = month ?? now.Month;
        var selectedYear = year ?? now.Year;

        if (selectedMonth < 1 || selectedMonth > 12)
            return ErrorResponse.Build("Month must be between 1 and 12", 400);

        var classes = (await _unitOfWork.LearningClass.GetAllWithMembersAsync())
            .Where(c => !IsInactiveStatus(c.Status))
            .ToList();

        if (classId.HasValue)
            classes = classes.Where(c => c.LearningClassId == classId.Value).ToList();

        if (!classes.Any())
        {
            return SuccessResponse.Build("Admin class dashboard retrieved successfully", 200, new
            {
                Filters = new
                {
                    Month = selectedMonth,
                    Year = selectedYear,
                    ClassId = classId,
                    Subject = subject
                },
                Kpi = new
                {
                    TotalStudents = 0,
                    ExamsCreated = 0,
                    AverageScore = 0.0,
                    CompletionRate = 0.0
                },
                TopStudents = Array.Empty<object>(),
                PerformanceChart = Array.Empty<object>(),
                ScoreDistribution = Array.Empty<object>(),
                ClassOptions = Array.Empty<object>(),
                SubjectOptions = Array.Empty<string>()
            });
        }

        var selectedStudentIds = classes
            .SelectMany(c => c.Members.Where(m => m.Role == StaticUserRoles.Student).Select(m => m.UserId))
            .Distinct()
            .ToList();

        var selectedTeacherIds = classes
            .SelectMany(c => c.Members.Where(m => m.Role == StaticUserRoles.Teacher).Select(m => m.UserId))
            .Distinct()
            .ToList();

        var allAttempts = (await _unitOfWork.ExamAttempt.GetAllAsync(
            a => selectedStudentIds.Contains(a.UserId),
            includeProperties: "ExamPaper,User")).ToList();

        var filteredAttempts = allAttempts
            .Where(a => a.StartedAt.Month == selectedMonth && a.StartedAt.Year == selectedYear)
            .ToList();

        if (!string.IsNullOrWhiteSpace(subject))
        {
            filteredAttempts = filteredAttempts
                .Where(a => string.Equals(a.ExamPaper?.Subject, subject, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var examPapers = (await _unitOfWork.ExamPaper.GetAllAsync(
            p => selectedTeacherIds.Contains(p.CreatedBy!))).ToList();

        var filteredExamPapers = examPapers
            .Where(p => p.CreatedTime.HasValue &&
                        p.CreatedTime.Value.Month == selectedMonth &&
                        p.CreatedTime.Value.Year == selectedYear)
            .ToList();

        if (!string.IsNullOrWhiteSpace(subject))
        {
            filteredExamPapers = filteredExamPapers
                .Where(p => string.Equals(p.Subject, subject, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var totalStudents = selectedStudentIds.Count;
        var examsCreated = filteredExamPapers.Count;
        var averageScore = filteredAttempts.Any() ? Math.Round(filteredAttempts.Average(a => a.Score), 2) : 0;
        var completionRate = filteredAttempts.Any()
            ? Math.Round(filteredAttempts.Count(a => a.CompletedAt.HasValue) * 100.0 / filteredAttempts.Count, 2)
            : 0;

        var topStudents = filteredAttempts
            .GroupBy(a => a.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                FullName = g.First().User?.FullName ?? g.First().User?.UserName ?? "Unknown",
                AvgScore = Math.Round(g.Average(x => x.Score), 2),
                AttemptCount = g.Count()
            })
            .OrderByDescending(x => x.AvgScore)
            .ThenByDescending(x => x.AttemptCount)
            .Take(5)
            .ToList();

        var performanceChart = filteredAttempts
            .GroupBy(a => ((a.StartedAt.Day - 1) / 7) + 1)
            .OrderBy(g => g.Key)
            .Select(g => new
            {
                Label = $"Tuần {g.Key}",
                AvgScore = Math.Round(g.Average(x => x.Score), 2),
                AttemptCount = g.Count()
            })
            .ToList();

        var scoreDistribution = new[]
        {
            new { Range = "0 - <5", Count = filteredAttempts.Count(a => a.Score < 5) },
            new { Range = "5 - <7", Count = filteredAttempts.Count(a => a.Score >= 5 && a.Score < 7) },
            new { Range = "7 - <8.5", Count = filteredAttempts.Count(a => a.Score >= 7 && a.Score < 8.5) },
            new { Range = "8.5 - 10", Count = filteredAttempts.Count(a => a.Score >= 8.5) }
        };

        var classOptions = classes
            .Select(c => new
            {
                c.LearningClassId,
                c.Name,
                StudentCount = c.Members.Count(m => m.Role == StaticUserRoles.Student),
                TeacherCount = c.Members.Count(m => m.Role == StaticUserRoles.Teacher)
            })
            .OrderBy(c => c.Name)
            .ToList();

        var subjectOptions = allAttempts
            .Select(a => a.ExamPaper?.Subject)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct()
            .OrderBy(s => s)
            .ToList();

        return SuccessResponse.Build("Admin class dashboard retrieved successfully", 200, new
        {
            Filters = new
            {
                Month = selectedMonth,
                Year = selectedYear,
                ClassId = classId,
                Subject = subject
            },
            Kpi = new
            {
                TotalStudents = totalStudents,
                ExamsCreated = examsCreated,
                AverageScore = averageScore,
                CompletionRate = completionRate
            },
            TopStudents = topStudents,
            PerformanceChart = performanceChart,
            ScoreDistribution = scoreDistribution,
            ClassOptions = classOptions,
            SubjectOptions = subjectOptions
        });
    }

    private async Task<bool> IsUserInRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return false;
        return await _userManager.IsInRoleAsync(user, role);
    }

    private async Task<bool> CanManageClass(ClaimsPrincipal user, LearningClass learningClass, string userId)
    {
        if (learningClass.OwnerUserId == userId)
            return true;

        var roles = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
        if (roles.Contains(StaticUserRoles.Admin) || roles.Contains("Admin"))
            return true;

        var member = await _unitOfWork.LearningClassMember.GetByClassAndUserAsync(learningClass.LearningClassId, userId);
        return member != null && member.Role == StaticUserRoles.Teacher;
    }

    private async Task<bool> CanManageOrViewClass(ClaimsPrincipal user, LearningClass learningClass, string userId)
    {
        if (await CanManageClass(user, learningClass, userId))
            return true;

        var member = await _unitOfWork.LearningClassMember.GetByClassAndUserAsync(learningClass.LearningClassId, userId);
        return member != null;
    }

    private static LearningClassMember BuildMember(Guid classId, string userId, string role, string createdBy)
    {
        return new LearningClassMember
        {
            LearningClassMemberId = Guid.NewGuid(),
            LearningClassId = classId,
            UserId = userId,
            Role = role,
            JoinedAt = StaticOperationStatus.Timezone.Vietnam,
            CreatedBy = createdBy,
            CreatedTime = StaticOperationStatus.Timezone.Vietnam,
            Status = ActiveStatus
        };
    }

    private async Task<LearningClassSummaryDto> BuildSummaryDto(LearningClass learningClass)
    {
        var examRooms = await _unitOfWork.ExamRoom.GetAllAsync(r => r.CreatedBy == learningClass.OwnerUserId);
        return new LearningClassSummaryDto
        {
            LearningClassId = learningClass.LearningClassId,
            Name = learningClass.Name,
            SchoolYear = learningClass.SchoolYear,
            Description = learningClass.Description,
            Schedule = learningClass.Schedule,
            Status = NormalizeClassStatus(learningClass.Status),
            StudentCount = learningClass.Members.Count(m => m.Role == StaticUserRoles.Student),
            TeacherCount = learningClass.Members.Count(m => m.Role == StaticUserRoles.Teacher),
            ExamRoomCount = examRooms.Count(),
            HasEnrollKey = !string.IsNullOrWhiteSpace(learningClass.EnrollKey)
        };
    }

    private async Task<string> GenerateUniqueEnrollKeyAsync()
    {
        for (var i = 0; i < 10; i++)
        {
            var candidate = GenerateEnrollKey();
            var exists = await _unitOfWork.LearningClass.GetAsync(c => c.EnrollKey == candidate);
            if (exists == null)
                return candidate;
        }

        return Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
    }

    private static string GenerateEnrollKey()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        Span<byte> randomBytes = stackalloc byte[8];
        RandomNumberGenerator.Fill(randomBytes);
        var keyChars = new char[8];
        for (var i = 0; i < 8; i++)
        {
            keyChars[i] = chars[randomBytes[i] % chars.Length];
        }

        return new string(keyChars);
    }

    private static string NormalizeClassStatus(string? status)
    {
        if (string.Equals(status, LegacyActiveStatus, StringComparison.OrdinalIgnoreCase))
            return ActiveStatus;

        if (string.Equals(status, LegacyInactiveStatus, StringComparison.OrdinalIgnoreCase))
            return InactiveStatus;

        return string.IsNullOrWhiteSpace(status) ? ActiveStatus : status;
    }

    private static bool IsInactiveStatus(string? status)
    {
        return string.Equals(status, InactiveStatus, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(status, LegacyInactiveStatus, StringComparison.OrdinalIgnoreCase);
    }
}
