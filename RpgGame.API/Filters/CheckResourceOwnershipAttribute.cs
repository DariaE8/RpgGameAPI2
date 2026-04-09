using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using RpgGame.Core.Exceptions;

namespace RpgGame.API.Filters
{
    public class CheckResourceOwnershipAttribute : ActionFilterAttribute
    {
        private readonly string _resourceType;

        public CheckResourceOwnershipAttribute(string resourceType)
        {
            _resourceType = resourceType;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // логгер через контекст 
            var logger = context.HttpContext.RequestServices
                .GetService<ILogger<CheckResourceOwnershipAttribute>>();

            var user = context.HttpContext.User.Identity?.Name ?? "Anonymous";
            var resourceId = context.ActionArguments.ContainsKey("id") 
                ? context.ActionArguments["id"]?.ToString()
                : "unknown";

            logger?.LogInformation("🔐 Ownership check: User {User} accessing {ResourceType} {ResourceId}", 
                user, _resourceType, resourceId);

            base.OnActionExecuting(context);
        }
    }
}