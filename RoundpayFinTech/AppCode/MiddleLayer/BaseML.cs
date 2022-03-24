using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public class BaseML
    {
        #region Gloabl Variables

        protected readonly IHttpContextAccessor _accessor;
        protected readonly IHostingEnvironment _env;
        protected readonly IDAL _dal;
        protected readonly IConnectionConfiguration _c;
        protected readonly ISession _session;
        protected readonly LoginResponse _lr;
        #endregion
        public BaseML(IHttpContextAccessor accessor, IHostingEnvironment env, bool IsInSession = true)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            if (IsInSession)
            {
                _session = _accessor.HttpContext.Session;
                _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
            }
            else
            {
                _lr = new LoginResponse();
            }
        }
    }
}
