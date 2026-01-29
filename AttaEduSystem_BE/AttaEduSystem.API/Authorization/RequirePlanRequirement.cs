using Microsoft.AspNetCore.Authorization;

namespace AttaEduSystem.API.Authorization
{
    public class RequirePlanRequirement : IAuthorizationRequirement
    {
        public string RequiredPlanCode { get; }

        public RequirePlanRequirement(string requiredPlanCode)
        {
            RequiredPlanCode = requiredPlanCode;
        }
    }
}
