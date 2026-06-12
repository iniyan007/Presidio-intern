using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;

namespace TravelTourManagement.API.Filters;

public class RequireEmailVerificationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var user = context.HttpContext.User;
        if (user.Identity?.IsAuthenticated == true)
        {
            var path = context.HttpContext.Request.Path.Value;
            if (path != null && 
                !path.Contains("/send-otp") && 
                !path.Contains("/verify-otp") && 
                !path.Contains("/login") && 
                !path.Contains("/register"))
            {
                var isVerified = user.FindFirst("EmailVerified")?.Value;
                if (isVerified != "True")
                {
                    context.Result = new ObjectResult(new { message = "Email verification required to access this resource." })
                    {
                        StatusCode = StatusCodes.Status403Forbidden
                    };
                    return;
                }
            }
        }
        await next();
    }
}
