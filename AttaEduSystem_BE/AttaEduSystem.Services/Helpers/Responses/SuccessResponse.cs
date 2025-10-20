using AttaEduSystem.Models.DTOs;

namespace AttaEduSystem.Services.Helpers.Responses
{
    public static class SuccessResponse
    {
        /// <summary>
        /// Helper method to build a success response, with a message, an HTTP status code, and a result object
        /// </summary>
        /// <param name="message">The message to be returned</param>
        /// <param name="statusCode">The HTTP status code represents the response's status</param>
        /// <param name="result">The result object to be returned</param>
        /// <returns>A ResponseDto object that represents an successful operation</returns>
        public static ResponseDto Build(string message, int statusCode, object result)
        {
            return new ResponseDto
            {
                IsSuccess = true,
                Message = message,
                StatusCode = statusCode,
                Result = result
            };
        }

        internal static ResponseDto Build(object message, int statusCode)
        {
            return new ResponseDto
            {
                IsSuccess = true,
                StatusCode = statusCode,
                Message = message?.ToString() ?? "",
                Result = null
            };
        }
    }
}
