using Fintech.AppCode;
using Fintech.AppCode.Configuration;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RoundpayFinTech.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class BaseController : Controller
    {
        #region GLobal Variable
        protected readonly IHttpContextAccessor _accessor;
        protected readonly IHostingEnvironment _env;
        protected readonly ISession _session;
        protected readonly LoginResponse _lr;
        protected readonly ILoginML loginML;
        #endregion

        public BaseController(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            _session = _accessor.HttpContext.Session;
            _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
            loginML = new LoginML(_accessor, _env);            
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (loginML.IsInValidSession() && (context.RouteData.Values["Action"].ToString() != "Index" || context.RouteData.Values["Controller"].ToString() != "Admin"))
            {
                context.Result = new RedirectResult("~/");
            }
            else
            {
                base.OnActionExecuting(context);
            }
        }
    }
}
