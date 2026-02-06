using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FrontOffice.Auth;

public class JwtAuthorizeAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var isAuthenticated = context.HttpContext.User.Identity?.IsAuthenticated ?? false;

        if (!isAuthenticated)
        {
            context.Result = new RedirectToActionResult("Login", "Auth", null);
            return;
        }

        base.OnActionExecuting(context);
    }
}