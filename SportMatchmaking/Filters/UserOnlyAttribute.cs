using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SportMatchmaking.Filters
{
    public class UserOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userName = context.HttpContext.Session.GetString("UserName");
            var roleName = context.HttpContext.Session.GetString("RoleName");

            if (string.IsNullOrEmpty(userName))
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            if (roleName != "User")
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Auth", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}