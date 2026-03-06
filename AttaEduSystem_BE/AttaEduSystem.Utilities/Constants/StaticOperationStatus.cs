namespace AttaEduSystem.Utilities.Constants
{
    public class StaticOperationStatus
    {
        public static class BaseEntity
        {
            public const string Active = "1";
            public const string Inactive = "0";
        }

        public static class File
        {
            public const string FileEmpty = "File is empty";
            public const string FileRetrieved = "File retrieved successfully";
            public const string FileUploaded = "File uploaded successfully";
            public const string FileNotFound = "File not found";
            public const string ImageNotFound = "Image not found";
            public const string ImageUploaded = "Image uploaded successfully";
            public const string VideoUploaded = "Video uploaded successfully";
            public const string VideoNotFound = "Video not found";
        }

        public static class User
        {
            public const string UserNotFound = "User not found";
            public const string UserNotAuthorized = "User is not authorized";
        }

        public static class StatusCode
        {
            public const int Ok = 200;
            public const int Created = 201;
            public const int NoContent = 204;
            public const int BadRequest = 400;
            public const int Unauthorized = 401;
            public const int Forbidden = 403;
            public const int NotFound = 404;
            public const int InternalServerError = 500;
        }

        public static class Timezone
        {
            public static DateTime Vietnam => DateTime.UtcNow.AddHours(7.0);
        }

        public static class Database
        {
            public const int Success = 1;
            public const int Failure = 0;
        }

        public static class Token
        {
            public const string TokenStored = "Token stored successfully";
            public const string TokenNotFound = "Token not found";
            public const string TokenExpired = "Token expired";
            public const string TokenRefreshed = "Token refreshed successfully";
            public const string TokenInvalid = "Token is invalid";
        }

        // Add more categories and constants as needed

        public static class Teacher
        {
            public const string TeacherNotFound = "Teacher not found";
            public const string TeacherCreated = "Teacher created successfully";
            public const string TeacherUpdated = "Teacher updated successfully";
            public const string TeacherDeleted = "Teacher deleted successfully";
        }

        public static class ExamPaper
        {
            public const string Draft = "Draft";
            public const string Ready = "Ready";
            public const string Removed = "Removed";
        }

        public static class ExamRoom
        {
            public const string Waiting = "Waiting";       // Phòng đang chờ, chưa đến giờ
            public const string InProgress = "InProgress"; // Đang diễn ra
            public const string Finished = "Finished";     // Đã kết thúc
            public const string Cancelled = "Cancelled";   // Đã hủy
        }

        public static class ExamRoomParticipant
        {
            public const string Joined = "Joined";       // Đã vào phòng chờ
            public const string InProgress = "InProgress"; // Đang làm bài
            public const string Submitted = "Submitted";   // Đã nộp bài
            public const string Left = "Left";            // Đã rời phòng
        }
    }
}
