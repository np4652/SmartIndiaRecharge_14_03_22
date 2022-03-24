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
using RoundpayFinTech.AppCode.Model.MoneyTransfer;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.StaticModel.MoneyTransfer;
using RoundpayFinTech.AppCode.ThirdParty.Hypto;
using RoundpayFinTech.AppCode.ThirdParty.Paytm;
using System.IO;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public class VerificationML : IVerificationML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration Configuration;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        private readonly IRequestInfo _info;
        private readonly UserML userML;
        public VerificationML(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            _info = new RequestInfo(_accessor, _env);
            userML = new UserML(_accessor, _env, false);
            var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile(_env.IsProduction() ? "appsettings.json" : "appsettings.Development.json");
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public VerificationOutput AccountVerification(VerificationInput verificationInput)
        {
            throw new System.NotImplementedException();
        }

        public VerificationOutput UPIVerification(VerificationInput verificationInput)
        {
            var vResp = new VerificationOutput
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Unknown_Error,
                APIRequestID = verificationInput.APIRequestID,
                AccountNo = verificationInput.AccountNo
            };
            if (verificationInput.UserID < 2)
            {
                vResp.Msg = string.Format("{0} {1}", ErrorCodes.InvalidParam, nameof(verificationInput.UserID));
                vResp.ErrorCode = ErrorCodes.Invalid_Parameter;
                return vResp;
            }
            if (verificationInput.RequestModeID == RequestMode.API && string.IsNullOrEmpty(verificationInput.APIRequestID))
            {
                vResp.Msg = string.Format("{0} {1}", ErrorCodes.InvalidParam, nameof(verificationInput.APIRequestID));
                vResp.ErrorCode = ErrorCodes.Invalid_Parameter;
                return vResp;
            }
            if (!(verificationInput.AccountNo ?? string.Empty).Contains("@"))
            {
                vResp.Msg = string.Format("{0} {1}", ErrorCodes.InvalidParam, nameof(verificationInput.AccountNo));
                vResp.ErrorCode = ErrorCodes.Invalid_Parameter;
                return vResp;
            }
            IProcedure procUPIHolder = new ProcGetSetUPIHolder(_dal);
            var resVO = (VerificationOutput)procUPIHolder.Call(new CommonReq
            {
                CommonStr = verificationInput.AccountNo
            });
            if (resVO.Statuscode == ErrorCodes.One)
            {
                vResp.Statuscode = RechargeRespType.SUCCESS;
                vResp.RPID = resVO.RPID;
                vResp.LiveID = resVO.Msg;
                vResp.AccountHolder = resVO.AccountHolder;
                return vResp;
            }
            IProcedure proc = new ProcVerificationService(_dal);
            var serviceRes = (VerificationServiceProcRes)proc.Call(new VerificationServiceProcReq
            {
                AccountNo = verificationInput.AccountNo,
                APIRequestID = verificationInput.APIRequestID,
                BankHandle = verificationInput.AccountNo.Split("@")[1],
                RequestMode = verificationInput.RequestModeID,
                IP = _info.GetRemoteIP(),
                OID = verificationInput.OID,
                SPKey = verificationInput.SPKey,
                UserID = verificationInput.UserID
            });
            vResp.Statuscode = serviceRes.Statuscode;
            vResp.Msg = serviceRes.Msg;
            vResp.ErrorCode = serviceRes.ErrorCode;
            if (serviceRes.Statuscode == ErrorCodes.Minus1)
                return vResp;
            if (serviceRes.TID < 1)
                return vResp;
            var VendorID = string.Empty;
            var Req = string.Empty;
            var Resp = string.Empty;
            if (serviceRes.APICode.Equals(APICode.HYPTO))
            {
                IVerificationAPI verificationAPI = new HyptoML(_accessor, _env, _dal);
                var resAPI = verificationAPI.VerifyUPIID(new VericationServiceReq
                {
                    AccountNo = verificationInput.AccountNo,
                    TID = serviceRes.TID
                });
                vResp.Statuscode = resAPI.Statuscode;
                vResp.RPID = serviceRes.TransactionID;
                vResp.LiveID = resAPI.LiveID;
                vResp.AccountHolder = resAPI.AccountHolder;
                VendorID = resAPI.VendorID;
                Req = resAPI.Req;
                Resp = resAPI.Resp;
            }
            if (serviceRes.APICode.Equals(APICode.PAYTM))
            {
                IMoneyTransferAPIML moneyTransferAPIML = new PaytmML(_accessor, _env, _dal);
                var APIRes = moneyTransferAPIML.VerifyAccount(new MTAPIRequest
                {
                    TransactionID = serviceRes.TransactionID,
                    TID = serviceRes.TID,
                    mBeneDetail = new MBeneDetail
                    {
                        AccountNo = verificationInput.AccountNo
                    }
                });
                vResp.Statuscode = APIRes.Statuscode;
                vResp.RPID = serviceRes.TransactionID;
                vResp.LiveID = APIRes.LiveID;
                vResp.AccountHolder = APIRes.AccountHolder;
                VendorID = APIRes.VendorID;
                Req = APIRes.Request;
                Resp = APIRes.Response;
            }
            if (vResp.Statuscode > 0)
            {
                IProcedure _updateProc = new ProcUpdateVerificationService(_dal);
                var CallbackData_ = (_CallbackData)_updateProc.Call(new TransactionStatus
                {
                    TID = serviceRes.TID,
                    Status = vResp.Statuscode,
                    OperatorID = vResp.LiveID,
                    VendorID = VendorID,
                    APIID = serviceRes.APIID,
                    Request = Req,
                    Response = Resp
                });
                if (vResp.Statuscode == RechargeRespType.SUCCESS)
                {
                    resVO = (VerificationOutput)procUPIHolder.Call(new CommonReq
                    {
                        CommonStr = verificationInput.AccountNo,
                        CommonStr2 = vResp.AccountHolder
                    });
                }
            }
            return vResp;
        }
    }
}