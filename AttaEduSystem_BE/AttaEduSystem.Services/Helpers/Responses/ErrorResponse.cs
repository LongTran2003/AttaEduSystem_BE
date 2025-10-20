using AttaEduSystem.Models.DTOs;

namespace AttaEduSystem.Services.Helpers.Responses
{
    public static class ErrorResponse
    {
        /// <summary>
        /// Helper method to build an error response, with a message and status code
        /// </summary>
        /// <param name="message">The error message to be returned</param>
        /// <param name="statusCode">The HTTP status code that represents the error</param>
        /// <returns>A ResponseDto object that represents an erroneous operation</returns>
        public static ResponseDto Build(string message, int statusCode)
        {
            return new ResponseDto
            {
                IsSuccess = false,
                Message = message,
                StatusCode = statusCode,
                Result = null
            };
        }
    }
}
