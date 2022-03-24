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
using RoundpayFinTech.AppCode.StaticModel.MoneyTransfer;
using RoundpayFinTech.AppCode.ThirdParty.Roundpay;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public sealed class GeneralInsuranceML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        private readonly IRequestInfo _info;
        private readonly IConfiguration Configuration;
        public GeneralInsuranceML(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            _info = new RequestInfo(_accessor, _env);
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile((_env.IsProduction() ? "appsettings.json" : "appsettings.Development.json"));
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();

        }
        public GIAppSetting GetGIAppSetting(string _AppCode)
        {
            var setting = new GIAppSetting();
            try
            {
                setting = new GIAppSetting
                {
                    _URL = Configuration["GI:" + _AppCode + ":URL"]
                };
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GetGIAppSetting",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                });
            }
            return setting;
        }

        public GenerateURLResp GenerateTokenAndURLForRedirection(ValidateAPIOutletResp validateAPIOutletResp, int RequestMode)
        {
            var res = new GenerateURLResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.NODATA
            };
            IProcedure proc = new ProcGeneralInsuranceTokenGeneration(_dal);
            var Procres = (GeneralInsuranceDBResponse)proc.Call(new TokenGenerationModel
            {
                APIID = validateAPIOutletResp.APIID,
                OID = validateAPIOutletResp.OID,
                OutletID = validateAPIOutletResp.OutletID,
                UserID = validateAPIOutletResp.UserID,
                APIRequestID = string.Empty,
                RequestMode = RequestMode,
                TransactionID = validateAPIOutletResp.TransactionID,
                VendorID = string.Empty
            });
            if (validateAPIOutletResp.APICode == APICode.APIWALE)
            {
                IRoundpayApiML roundpayApiML = new RoundpayApiML(_accessor, _env);
                res = roundpayApiML.GenerateToken(Procres);
            }
            if (validateAPIOutletResp.APICode == APICode.GIBL)
            {
                //res.RedirectURL=
                res.RedirectURL = "/GIRedirect?code=" + APICode.GIBL + "&prodid=" + Procres.RechType + "&tranid=" + (Procres.Token??string.Empty) + "&outlet=" + (Procres.AgentID??string.Empty);
                res.Statuscode = ErrorCodes.One;
                res.Msg = ErrorCodes.SUCCESS;
            }
            return res;
        }
        public IResponseStatus DoGITransaction(GIUpdateRequestModel req)
        {

            req.RequestIP = _info.GetRemoteIP();
            IProcedure proc = new ProcTransactionServiceGI(_dal);
            return (ResponseStatus)proc.Call(req);
        }
        public IResponseStatus DoGIDebit(GIUpdateRequestModel req)
        {
            req.RequestIP = _info.GetRemoteIP();

            IProcedure proc = new ProcTransactionServiceGI_Debit(_dal);
            return (ResponseStatus)proc.Call(req);
        }
        public IResponseStatus DoGIUpdateTransaction(GIUpdateRequestModel req)
        {
            req.RequestIP = _info.GetRemoteIP();

            IProcedure proc = new ProcUpdateTransactionServiceGI_Debit(_dal);
            return (ResponseStatus)proc.Call(req);
        }

       
    }
}
