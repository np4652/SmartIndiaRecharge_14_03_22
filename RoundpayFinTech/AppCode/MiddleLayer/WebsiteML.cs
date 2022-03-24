using Fintech.AppCode;
using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using RoundpayFinTech.AppCode.Configuration;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public class WebsiteML : IWebsiteML
    {
        private readonly IConfiguration Configuration;
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly ISession _session;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        private readonly IRequestInfo _rinfo;
        private readonly WebsiteInfo _WInfo;
        private readonly LoginResponse _lr;
        private readonly IUserML userML;
        public WebsiteML(IHttpContextAccessor accessor, IHostingEnvironment env, bool InSession = true)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());

            if (InSession)
            {
                _session = _accessor.HttpContext.Session;
                _rinfo = new RequestInfo(_accessor, _env);
                _WInfo = new LoginML(_accessor, _env).GetWebsiteInfo();
                _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
                userML = new UserML(_lr);

                bool IsProd = _env.IsProduction();
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory());
                builder.AddJsonFile((IsProd ? "appsettings.json" : "appsettings.Development.json"));
                builder.AddEnvironmentVariables();
                Configuration = builder.Build();
            }
        }
        public IEnumerable<WebsiteModel> GetWebsite()
        {
            var res = new List<WebsiteModel>();
            if (_lr.LoginTypeID == LoginType.ApplicationUser)
            {
                var req = new CommonReq
                {
                    LoginID = _lr.UserID,
                    LoginTypeID = _lr.LoginTypeID,
                    CommonBool1 = false
                };
                IProcedure proc = new ProcGetWebsites(_dal);
                // res = (WebsiteModel)proc.Call(req);
                res = (List<WebsiteModel>)proc.Call(req);

            }
            return res;
        }
        public IResponseStatus UpdateWebsiteList(WebsiteModel Web)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser || (_lr.LoginTypeID == LoginType.CustomerCare && userML.IsCustomerCareAuthorised(ActionCodes.WhitelabelSetting)))
            {
                var req = new WebsiteModel
                {
                    LoginID = _lr.UserID,
                    LoginTypeID = _lr.LoginTypeID,
                    DomainName = Web.DomainName,
                    WID = Web.WID,
                    IsActive = Web.IsActive,
                    IsWebsiteUpdate = Web.IsWebsiteUpdate,

                };
                IProcedure proc = new ProcUpdateWebsites(_dal);
                res = (ResponseStatus)proc.Call(req);
            }
            return res;
        }
        public IEnumerable<Theme> GetThemes()
        {
            int WID = _WInfo != null ? _WInfo.WID : 0;
            IProcedure proc = new ProcGetTheme(_dal);
            var res = (IEnumerable<Theme>)proc.Call(WID);
            return res;
        }
        public ResponseStatus ChangeTheme(int ThemeId)
        {
            var req = new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID,
                CommonInt = _lr.WID,
                CommonInt2 = ThemeId
            };
            IProcedure proc = new ProcChangeTheme(_dal);
            var res = (ResponseStatus)proc.Call(req);
            return res;
        }

        public ResponseStatus ChangeSiteTemplate(int LoginId, int TemplateId, int WID)
        {
            var req = new CommonReq
            {
                LoginID = LoginId,
                CommonInt = WID,
                CommonInt2 = TemplateId
            };
            IProcedure proc = new ProcChangeSiteTemplate(_dal);
            var res = (ResponseStatus)proc.Call(req);
            return res;
        }

        public ResponseStatus UpdateAppPackageID(int WID, string appPackage)
        {
            var req = new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                CommonInt = WID,
                CommonStr = appPackage
            };
            IProcedure proc = new ProcUpdateAppPackageID(_dal);
            var res = (ResponseStatus)proc.Call(req);
            return res;
        }

        public WebsiteInfo GetWebsiteInfo(int WID=0)
        {
            string domain = _rinfo.GetDomain(Configuration);
            ProcGetWebsiteInfo procGetWebsiteInfo = new ProcGetWebsiteInfo(_dal);
            var _wi = (WebsiteInfo)procGetWebsiteInfo.Call(new CommonReq { CommonStr = domain, CommonInt = _lr != null ? _lr.WID : WID });

            var cInfo = _rinfo.GetCurrentReqInfo();
            _wi.AbsoluteHost = cInfo.Scheme + "://" + cInfo.Host + (cInfo.Port > 0 ? ":" + cInfo.Port : "");
            _wi.WID = _lr != null ? _lr.WID : WID;
            return _wi;
        }
        public HomeDisplay Template(int ThemeID)
        {
            int WID = _WInfo != null ? _WInfo.WID : 0;
            var req = new CommonReq
            {
                CommonInt = WID,
                CommonInt2 = ThemeID
            };
            IProcedure proc = new ProcGetDisplayHtml(_dal);
            var res = (HomeDisplay)proc.Call(req);
            return res;
        }
        public IResponseStatus UpdateDisplayHtml(HomeDisplayRequest req)
        {
            req.WID = _WInfo != null ? _WInfo.WID : 0;
            IProcedure proc = new ProcUpdateDisplayHtml(_dal);
            var res = (ResponseStatus)proc.Call(req);
            return res;
        }
        public IResponseStatus UpdateIsWLAPIAllowed(WebsiteModel Web)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser || (_lr.LoginTypeID == LoginType.CustomerCare && userML.IsCustomerCareAuthorised(ActionCodes.WhitelabelSetting)))
            {
                var req = new WebsiteModel
                {
                    LoginID = _lr.UserID,
                    LoginTypeID = _lr.LoginTypeID,
                    WID = Web.WID,
                };
                IProcedure proc = new ProcUpdateIsWLAPIAllowed(_dal);
                res = (ResponseStatus)proc.Call(req);
            }
            return res;
        }
        public ResponseStatus WLAllowTheme(int ThemeId, bool IsWLAllowed)
        {
            var req = new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID,
                CommonBool = IsWLAllowed,
                CommonInt2 = ThemeId
            };
            IProcedure proc = new ProcWLAllowedTheme(_dal);
            var res = (ResponseStatus)proc.Call(req);
            return res;
        }
        public GetApiDocument GetAPIDocument()
        {
            var model = new GetApiDocument();
            if (_lr != null && _lr.RoleID.In(Role.Admin, Role.MasterWL, Role.APIUser) && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                var commonReq = new CommonReq
                {
                    CommonInt = _lr.UserID,
                    CommonInt2 = _lr.LoginTypeID,
                    CommonInt3 = _lr.WID,
                };
                IProcedure _proc = new ProcGetAPIDocument(_dal);
                model = (GetApiDocument)_proc.Call(commonReq);

            }
            return model;
        }
        public ResponseStatus UpdateRefferalContent(int WID, string appPackage)
        {
            var req = new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID,
                CommonInt = WID,
                CommonStr = appPackage
            };
            IProcedure proc = new ProcUpdateRefferalContent(_dal);
            var res = (ResponseStatus)proc.Call(req);
            return res;
        }

        public string GetAppWebsiteContent(int WID)
        {
            var res = new WebsiteModel();
            string webContent = string.Empty;
            IProcedure proc = new ProcGetWebsiteContentByWID(_dal);
            res = (WebsiteModel)proc.Call(new CommonReq
            {
                CommonInt = WID
            });
            webContent = res.RefferalContent;
            return webContent;
        }

        public async Task<IResponseStatus> UpdateWebsiteContentAsync(CommonReq req)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            IProcedureAsync proc = new ProcUpdateWebsiteContent(_dal);
            res =  (ResponseStatus)await proc.Call(req);
            return res;
        }

        public async Task<SiteTemplateSection> GetWebsiteContentAsync(int wId)
        {
            IProcedureAsync proc = new ProcGetWebsiteContentAsync(_dal);
            SiteTemplateSection res = (SiteTemplateSection)await proc.Call(wId).ConfigureAwait(true);
            return res;
        }
        public async Task<List<websiteBanks>> GetWebsiteBankDetails()
        {
            IProcedureAsync proc = new ProcGetWebsiteBankDetails(_dal);
            var res = (List<websiteBanks>)await proc.Call();
            return res;
        }
    }
}
