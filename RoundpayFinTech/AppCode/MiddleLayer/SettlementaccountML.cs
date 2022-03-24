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
using RoundpayFinTech.Models;
using RoundpayFinTech.AppCode.Configuration;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public class SettlementaccountML : ISettlementaccountML
    {
        private readonly IConfiguration Configuration;
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly ISession _session;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        private readonly IRequestInfo _rinfo;
        private readonly LoginResponse _lr;



        public SettlementaccountML(IHttpContextAccessor accessor, IHostingEnvironment env, bool IsSession = true)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            _rinfo = new RequestInfo(_accessor, _env);
            if (IsSession)
            {
                _session = _accessor.HttpContext.Session;
                _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
            }
            bool IsProd = _env.IsProduction();
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile((IsProd ? "appsettings.json" : "appsettings.Development.json"));
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();

        }
        public IEnumerable<SattlementAccountModels> GetSettlementaccountList(int UserID, int LoginID)
        {
            var resp = new List<SattlementAccountModels>();
            IProcedure _proc = new ProcGetSattlementAccountList(_dal);
            resp = (List<SattlementAccountModels>)_proc.Call(new CommonReq
            {
                LoginTypeID = LoginType.ApplicationUser,
                LoginID = LoginID,
                UserID = UserID
            });
            return resp;
        }
        public SattlementAccountModels GeSettlementAccountByID(CommonReq req)
        {
            IProcedure _proc = new ProcGetSettlementAccountbyID(_dal);
            return (SattlementAccountModels)_proc.Call(req);
        }

        public IResponseStatus UpdateSettlementaccount(SattlementAccountModels sattlementsccount)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            Validate validate = Validate.O;
            if (!validate.IsValidBankAccountNo(sattlementsccount.AccountNumber))
            {
                _resp.Msg = "Invalid Data!";
                return _resp;
            }
            sattlementsccount.ApprovalIp = _rinfo.GetRemoteIP();
            sattlementsccount.ApprovalStatus = ApplicationSetting.IsSattlemntAccountVerify == true ? 0 : 1;
            IProcedure _proc = new ProcUpdateSattlementAccount(_dal);
            _resp = (ResponseStatus)_proc.Call(sattlementsccount);
            return _resp;
        }

        public IResponseStatus SetDefaultSettlementaccount(SattlementAccountModels sattlementsccount)
        {
            IProcedure _proc = new ProcSetDefaultSattlementAccount(_dal);
            return (ResponseStatus)_proc.Call(sattlementsccount);
        }

        public IResponseStatus SetDeleteSettlementaccount(CommonReq req)
        {
            IProcedure _proc = new ProcDeleteSattlementAccount(_dal);
            return (ResponseStatus)_proc.Call(req);
        }

        public IResponseStatus UpdateUTRByUser(int ID,string UTR, int UserID)
        {
            IProcedure proc = new ProcUpdateUTRByAccountUser(_dal);
            return (ResponseStatus)proc.Call(new CommonReq
            {
                LoginID=UserID,
                CommonInt=ID,
                CommonStr=UTR,
                CommonStr2=_rinfo.GetRemoteIP(),
                CommonStr3=_rinfo.GetBrowser()
            });
        }
        public async Task<IResponseStatus> AcceptOrRejectBankupdateRequest(GetEditUser RequestData)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                RequestData.LT = _lr.LoginTypeID;
                RequestData.LoginID = _lr.UserID;
                IProcedureAsync _proc = new ProcApproveBankDetails(_dal);
                var alertParam = (AlertReplacementModel)await _proc.Call(RequestData);
                _res.Statuscode = alertParam.Statuscode;
                _res.Msg = alertParam.Msg;
                //alertParam.RequestStatus = RequestData.RequestStatus;
                //IAlertML alertMl = new AlertML(_accessor, _env);
                //Parallel.Invoke(() => Task.Run(async () => await alertMl.UserPartialApprovalSMS(alertParam).ConfigureAwait(false)),
                //() => Task.Run(async () => await alertMl.UserPartialApprovalEmail(alertParam).ConfigureAwait(false)),
                //() => Task.Run(async () => await alertMl.WebNotification(alertParam).ConfigureAwait(false)));
            }
            return _res;
        }

        public SattlementAccountStatus GetApproved_VeriyfiedStatus()
        {
            var resp = new SattlementAccountStatus();
            var req = new CommonReq();
            req.LoginTypeID = _lr.LoginTypeID;
            req.LoginID = _lr.UserID;
            IProcedure _proc = new ProcGetSattlementAccountAstatus(_dal);
            IProcedure _proc1 = new ProcGetSattlementAccountVstatus(_dal);
            resp.ApprovalStatus = (List<_Status>)_proc.Call(req);
            resp.VerificationStatus = (List<_Status>)_proc1.Call(req);

            return resp;
        }
    }
}
