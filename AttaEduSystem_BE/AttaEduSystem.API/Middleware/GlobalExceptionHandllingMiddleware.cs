using AttaEduSystem.Models.DTOs;

namespace AttaEduSystem.API.Middleware
{
    public class GlobalExceptionHandllingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandllingMiddleware> _logger;

        public GlobalExceptionHandllingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandllingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                int statusCode;

                if (ex is ArgumentException)
                {
                    statusCode = StatusCodes.Status400BadRequest; // Lỗi request không hợp lệ
                }
                else if (ex is UnauthorizedAccessException)
                {
                    statusCode = StatusCodes.Status401Unauthorized; // Không có quyền truy cập
                }
                else if (ex is KeyNotFoundException)
                {
                    statusCode = StatusCodes.Status404NotFound; // Không tìm thấy tài nguyên
                }
                else if (ex is InvalidOperationException)
                {
                    statusCode = StatusCodes.Status403Forbidden; // Hành động không được phép
                }
                else
                {
                    statusCode = StatusCodes.Status500InternalServerError; // Lỗi server mặc định
                }

                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/json";

                var response = new ResponseDto
                {
                    IsSuccess = false,
                    StatusCode = statusCode,
                    Message = ex.StackTrace
                };

                await context.Response.WriteAsJsonAsync(response);
            }
        }
    }
}
