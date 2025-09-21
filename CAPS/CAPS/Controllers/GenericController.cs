using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CAPS.Controllers
{
    public class GenericController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!bool.Parse(HttpContext?.Session?.GetString("isAdmin") ?? "false"))
            {
                context.Result = RedirectToAction("Login", "Admin");
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
