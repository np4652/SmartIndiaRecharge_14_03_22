using Fintech.AppCode;
using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System.Collections.Generic;
using System.IO;
using Validators;
using System.Linq;
using System.Threading.Tasks;
using RoundpayFinTech.AppCode.Model.App;
using System.Text;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public class ReferSettingML : IReferSettingML
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

        public ReferSettingML(IHttpContextAccessor accessor, IHostingEnvironment env, bool IsSession = true)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            _rinfo = new RequestInfo(_accessor, _env);
            if (IsSession)
            {
                _session = _accessor.HttpContext.Session;
                _WInfo = new LoginML(_accessor, _env).GetWebsiteInfo();
                _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
                userML = new UserML(_lr);
            }
            bool IsProd = _env.IsProduction();
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile((IsProd ? "appsettings.json" : "appsettings.Development.json"));
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();

        }
        public IEnumerable<Master_Topup_Commission> GetMasterTopupCommission()
        {
            var resp = new List<Master_Topup_Commission>();

            if (_lr.LoginTypeID == LoginType.ApplicationUser && !userML.IsEndUser())
            {
                var req = new CommonReq
                {
                    LoginID = _lr.UserID,
                    LoginTypeID = _lr.LoginTypeID,

                };
                IProcedure proc = new procReferSetting(_dal);
                resp = (List<Master_Topup_Commission>)proc.Call(req);
            }
            return resp;

        }

        public IEnumerable<Master_Role> GetMasterRole()
        {
            var resp = new List<Master_Role>();

            if (_lr.LoginTypeID == LoginType.ApplicationUser && !userML.IsEndUser())
            {
                var req = new CommonReq
                {
                    LoginID = _lr.UserID,
                    LoginTypeID = _lr.LoginTypeID,

                };
                IProcedure proc = new procMasterRole(_dal);
                resp = (List<Master_Role>)proc.Call(req);
            }
            return resp;

        }


        public IResponseStatus UpdateMaster_Topup_Commission(Master_Topup_Commission para)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            para.LoginTypeID = _lr.LoginTypeID;
            para.LoginID = _lr.UserID;
            para.Ip = _rinfo.GetRemoteIP();
            para.Browser = _rinfo.GetBrowser();
            IProcedure _proc = new ProcUpdate_Topup_Commission(_dal);
            _resp = (ResponseStatus)_proc.Call(para);

            return _resp;
        }

        public IResponseStatus UpdateMaster_Role(Master_Role para)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            para.LoginTypeID = _lr.LoginTypeID;
            para.LoginID = _lr.UserID;
            para.Ip = _rinfo.GetRemoteIP();
            para.Browser = _rinfo.GetBrowser();
            IProcedure _proc = new ProcUpdateMaster_Role(_dal);
            _resp = (ResponseStatus)_proc.Call(para);
            return _resp;
        }
        public IEnumerable<ReferralCommission> ReferralCommissions()
        {
            var resp = new List<ReferralCommission>();

            if (_lr.LoginTypeID == LoginType.ApplicationUser && !userML.IsEndUser())
            {
                var req = new CommonReq
                {
                    LoginID = _lr.UserID,
                    LoginTypeID = _lr.LoginTypeID,

                };
                IProcedure proc = new ProcReferralCommission(_dal);
                resp = (List<ReferralCommission>)proc.Call(req);
            }
            return resp;

        }
        public IResponseStatus ReferralCommissionsUpdate(ReferralCommission data)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };            
            IProcedure _proc = new ProcUpdate_referral_Commission(_dal);
            _resp = (ResponseStatus)_proc.Call(data);
            return _resp;
        }
    }
}
