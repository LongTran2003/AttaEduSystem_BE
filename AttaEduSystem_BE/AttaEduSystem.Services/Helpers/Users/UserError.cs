using System.Security.Claims;


namespace AttaEduSystem.Services.Helpers.Users
{
    public class UserError
    {
        public static bool NotExists(ClaimsPrincipal User)
        {
            return User is null || (
                User.FindFirstValue(ClaimTypes.NameIdentifier) is null &&
                User.FindFirstValue("FullName") is null);
        }
    }
}
