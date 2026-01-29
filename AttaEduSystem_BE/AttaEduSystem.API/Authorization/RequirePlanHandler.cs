using AttaEduSystem.DataAccess.IRepositories;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AttaEduSystem.API.Authorization
{
    public class RequirePlanHandler : AuthorizationHandler<RequirePlanRequirement>
    {
        private readonly IUnitOfWork _unitOfWork;

        public RequirePlanHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            RequirePlanRequirement requirement)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return;

            var subscription =
                await _unitOfWork.UserSubscription.GetActiveByUserIdAsync(userId);

            if (subscription == null || subscription.Plan == null)
                return;

            if (string.Equals(subscription.Plan.Code, requirement.RequiredPlanCode,
                    StringComparison.OrdinalIgnoreCase))
            {
                context.Succeed(requirement);
            }
        }
    }
}
