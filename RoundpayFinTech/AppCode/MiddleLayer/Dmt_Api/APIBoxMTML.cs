using Fintech.AppCode.DB;
using Fintech.AppCode.StaticModel;
using Fintech.AppCode.WebRequest;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.MoneyTransfer;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.ThirdParty.APIBox;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.MiddleLayer.Dmt_Api
{
    public class APIBoxMTML : IMoneyTransferAPIML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration Configuration;
        private readonly int _APIID;
        private readonly IDAL _dal;
        private readonly APIBoxSetting apiSetting;
        public APIBoxMTML(IHttpContextAccessor accessor, IHostingEnvironment env, int APIID, IDAL dal)
        {
            _APIID = APIID;
            _accessor = accessor;
            _env = env;
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile((_env.IsProduction() ? "appsettings.json" : "appsettings.Development.json"));
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            _dal = dal;
            apiSetting = AppSetting();
        }
        public APIBoxSetting AppSetting()
        {
            var setting = new APIBoxSetting();
            try
            {
                setting = new APIBoxSetting
                {
                    BaseURL = Configuration["DMR:ABOX:BaseURL"],
                    Token = Configuration["DMR:ABOX:Token"],
                    VerificationAPICode = Configuration["DMR:ABOX:VerificationAPICode"],
                    WalletNo = Configuration["DMR:ABOX:WalletNo"]
                };
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "APIBoxSetting",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                });
            }
            return setting;
        }

        public MSenderCreateResp CreateSender(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }
        public MSenderLoginResponse GetSender(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }
        public MSenderCreateResp SenderEKYC(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }
        public MSenderCreateResp SenderKYC(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }
        public MSenderCreateResp SenderResendOTP(MTAPIRequest request)
        {

            throw new NotImplementedException();
        }
        public MSenderLoginResponse VerifySender(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }
        public MSenderLoginResponse CreateBeneficiary(MTAPIRequest request)
        {

            throw new NotImplementedException();
        }
        public MBeneficiaryResp GetBeneficiary(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }
        public MSenderCreateResp GenerateOTP(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }
        public MSenderLoginResponse ValidateBeneficiary(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }
        public MSenderLoginResponse RemoveBeneficiary(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }
        public MSenderLoginResponse RemoveBeneficiaryValidate(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }
        public DMRTransactionResponse VerifyAccount(MTAPIRequest request)
        {
            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = RechargeRespType._PENDING,
                VendorID = "",
                LiveID = ""
            };

            string response = string.Empty;
            StringBuilder sb = new StringBuilder(apiSetting.BaseURL);
            sb.Append("api/Action/validate?token={TOKEN}&skey=DMV&reqid={TID}&p1=new&p2={SENDERNO}&p3={ACCOUNTNO}&p4={IFSC}&p5={APIOUTLETID}&format=json");
            sb.Replace("{TOKEN}", apiSetting.Token ?? string.Empty);
            sb.Replace("{TID}", string.Format("TID{0}", request.TID.ToString()));
            sb.Replace("{SENDERNO}", apiSetting.WalletNo ?? string.Empty);
            sb.Replace("{ACCOUNTNO}", request.mBeneDetail.AccountNo ?? string.Empty);
            sb.Replace("{IFSC}", request.mBeneDetail.IFSC ?? string.Empty);
            sb.Replace("{APIOUTLETID}", request.APIOutletID ?? string.Empty);
            var _URL = sb.ToString();
            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_GET(_URL);
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<APIBoxModel>(response);
                    if (_apiRes != null)
                    {
                        if (_apiRes.response != null)
                        {
                            if (_apiRes.response.status_code == "TXN")
                            {
                                if (_apiRes.response.Status == "COMPLETED")
                                {
                                    res.Statuscode = RechargeRespType.SUCCESS;
                                    res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                    res.ErrorCode = ErrorCodes.Transaction_Successful;
                                    res.LiveID = _apiRes.response.UTR ?? string.Empty;
                                    res.VendorID = _apiRes.response.OrderId;
                                    res.BeneName = _apiRes.response.BeneficiaryName;
                                }
                            }
                            else
                            {
                                IErrorCodeML errorCodeML = new ErrorCodeML(_accessor, _env, false);
                                var eFromDB = errorCodeML.GetAPIErrorCodeDescription(request.APIGroupCode, _apiRes.response.status_code);
                                if (!string.IsNullOrEmpty(eFromDB.Code))
                                {
                                    res.Statuscode = eFromDB.Status;
                                    res.Msg = eFromDB.Error.Replace("{REPLACE}", _apiRes.response.desc);
                                    res.ErrorCode = Convert.ToInt32(eFromDB.Code.Trim());
                                    res.LiveID = res.Msg;
                                }
                                res.Statuscode = res.Statuscode == 0 ? RechargeRespType.PENDING : res.Statuscode;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "VerifyAccount",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            res.Request = _URL;
            res.Response = response;
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = request.APIID,
                Method = "VerifyAccount",
                RequestModeID = request.RequestMode,
                Request = res.Request,
                Response = res.Response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TID.ToString()
            });
            return res;
        }
        public DMRTransactionResponse AccountTransfer(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }
        public DMRTransactionResponse GetTransactionStatus(int TID, string TransactionID, int RequestMode, int UserID, int APIID, string VendorID)
        {
            throw new NotImplementedException();
        }
    }
}
