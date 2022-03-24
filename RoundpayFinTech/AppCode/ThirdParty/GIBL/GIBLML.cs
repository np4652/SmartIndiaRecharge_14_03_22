using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.StaticModel.MoneyTransfer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.ThirdParty.GIBL
{
    public class GIBLML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        public GIBLML(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
        }

        public GIUpdateRequestModel SerializeDebitRequest(string reqJson)
        {
            var resp = new GIUpdateRequestModel
            {

            };
            if (reqJson.Contains("ptype"))
            {
                var apiResp = JsonConvert.DeserializeObject<GIBLPremDeductRequest>(reqJson);
                if (!string.IsNullOrEmpty(apiResp.ptype))
                {
                    resp.APIOpCode = apiResp.ptype;
                    resp.APICode = APICode.GIBL;
                    resp.AccountNo = apiResp.refno.ToString();
                    resp.ActualAmount = string.IsNullOrEmpty(apiResp.pamt) ? 0M : Convert.ToDecimal(apiResp.pamt);
                    resp.APIOutletID = apiResp.urc;
                    resp.TransactionID = apiResp.ak;
                    resp.VendorID = apiResp.refno.ToString();
                }
            }

            return resp;
        }

        public GIUpdateRequestModel SerializeUpdateRequest(string reqJson)
        {
            var resp = new GIUpdateRequestModel
            {

            };
            if (reqJson.Contains("payout"))
            {
                var apiResp = JsonConvert.DeserializeObject<GIBLPolicyConfirmReq>(reqJson);
                if (!string.IsNullOrEmpty(apiResp.ptype))
                {
                    resp.APIOpCode = apiResp.ptype;
                    resp.APICode = APICode.GIBL;
                    resp.AccountNo = apiResp.refno.ToString();
                    resp.ActualAmount = string.IsNullOrEmpty(apiResp.pamt) ? 0M : Convert.ToDecimal(apiResp.pamt);
                    resp.APIOutletID = apiResp.urc;
                    resp.TransactionID = apiResp.ak;
                    resp.Status = apiResp.pstatus == "1" ? RechargeRespType.SUCCESS : RechargeRespType.FAILED;
                    resp.Payout = string.IsNullOrEmpty(apiResp.payout)?0M:Convert.ToDecimal(apiResp.payout);
                    resp.ActualAmount = string.IsNullOrEmpty(apiResp.pamt)?0M:Convert.ToDecimal(apiResp.pamt);
                    resp.LiveID= apiResp.refno.ToString();
                }
            }
            return resp;
        }
    }
}
