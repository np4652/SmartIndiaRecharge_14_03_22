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
using RoundpayFinTech.AppCode.Interface;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public class MLMSlabML : I_MlmSlabML
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

        public MLMSlabML(IHttpContextAccessor accessor, IHostingEnvironment env, bool IsSession = true)
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
      
        public MLM_SlabDetailModel MLM_GetSlabDetail(int SlabID, int OpTypeID)
        {
            var res = new MLM_SlabDetailModel();
            if (_lr.LoginTypeID == LoginType.ApplicationUser || (userML.IsCustomerCareAuthorised(ActionCodes.AddEditSLAB)))
            {
                var req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonInt = SlabID,
                    CommonInt2 = OpTypeID,
                    IsListType = _lr.IsAdminDefined || (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser)                    
                };
                IProcedure proc = new Proc_MLM_GetSlabDetail(_dal);
                res = (MLM_SlabDetailModel)proc.Call(req);
                if (res.IsAdminDefined)
                {
                    List<OperatorDetail> operatorDetails = res.mlmSlabDetails
                        .GroupBy(x => new { x.OID, x.Operator, x.OperatorType, x.IsBBPS })
                        .Select(g => new OperatorDetail { OID = g.Key.OID, Operator = g.Key.Operator, OperatorType = g.Key.OperatorType, IsBBPS = g.Key.IsBBPS })
                        .ToList();
                    res.Operators = operatorDetails;
                }
            }
            return res;
        }

        public IResponseStatus MLM_UpdateSlabDetail(MLM_SlabCommission mlmslabCommission)
        {
            IResponseStatus _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if ((_lr.LoginTypeID == LoginType.ApplicationUser && !userML.IsEndUser()) || userML.IsCustomerCareAuthorised(ActionCodes.AddEditSLAB))
            {
                var req = new MLMSlabRequest
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonStr = _rinfo.GetRemoteIP(),
                    CommonStr2 = _rinfo.GetBrowser(),
                    Commission = mlmslabCommission
                };
                IProcedure proc = new MLM_Proc_UpdateSlabDetail(_dal);
                var res = (AlertReplacementModel)proc.Call(req);
                _resp.Statuscode = res.Statuscode;
                _resp.Msg = res.Msg;
                if (_lr.RoleID == Role.Admin && LoginType.ApplicationUser == _lr.LoginTypeID)
                {
                    IAlertML ml = new AlertML(_accessor, _env);
                    Parallel.Invoke(() => ml.MarginRevisedSMS(res),
                        () => ml.MarginRevisedEmail(res),
                        () => ml.WebNotification(res),
                        () => ml.MarginRevisedNotification(res, true));
                }
            }
            return _resp;
        }
    }
}
