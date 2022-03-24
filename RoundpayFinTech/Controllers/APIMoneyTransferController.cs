using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.Configuration;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Recharge;
using System.Threading.Tasks;

namespace RoundpayFinTech.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class APIMoneyTransferController : Controller, IDMTTransactionServiceP
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IDMTAPIUserML dMTAPIUserML;
        public APIMoneyTransferController(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            dMTAPIUserML = new DMTAPIUserML(_accessor, _env);
        }
        [HttpPost]
        public async Task<IActionResult> GetSender([FromBody] DMTAPIRequest dMTAPIRequest)
        {
            if (ApplicationSetting.IsDMTWithPIPE)
            {
                var res = await dMTAPIUserML.GetSender(dMTAPIRequest).ConfigureAwait(false);
                SaveDMRLog(JsonConvert.SerializeObject(dMTAPIRequest), JsonConvert.SerializeObject(res), "GetSender");
                return Json(res);
            }
            return Forbid(JsonConvert.SerializeObject(new { Statuscode = ErrorCodes.Minus1, Message = "This method is forbidden" }));
        }
        [HttpPost]
        public async Task<IActionResult> CreateSender([FromBody] CreateSednerReq dMTAPIRequest)
        {
            if (ApplicationSetting.IsDMTWithPIPE)
            {
                var res = await dMTAPIUserML.CreateSender(dMTAPIRequest).ConfigureAwait(false);
                SaveDMRLog(JsonConvert.SerializeObject(dMTAPIRequest), JsonConvert.SerializeObject(res), "CreateSender");
                return Json(res);
            }
            return Forbid(JsonConvert.SerializeObject(new { Statuscode = ErrorCodes.Minus1, Message = "This method is forbidden" }));
        }
        [HttpPost]
        public async Task<IActionResult> SenderKYC(int UserID, string Token, int OutletID,string SenderMobile, string ReferenceID, string SPKey,string NameOnKYC,string AadharNo,string PANNo,IFormFile AadharFront,IFormFile AadharBack,IFormFile SenderPhoto, IFormFile PAN)
        {                                           
            if (ApplicationSetting.IsDMTWithPIPE) 
            {                                      
                var res = await dMTAPIUserML.SenderKYC(UserID, Token, OutletID, SenderMobile, ReferenceID, SPKey, NameOnKYC, AadharNo, PANNo, AadharFront, AadharBack, SenderPhoto, PAN).ConfigureAwait(false);
                SaveDMRLog("SenderKYC", JsonConvert.SerializeObject(res), "CreateSender");
                return Json(res);
            }
            return Forbid(JsonConvert.SerializeObject(new { Statuscode = ErrorCodes.Minus1, Message = "This method is forbidden" }));
        }
        [HttpPost]
        public async Task<IActionResult> VerifySender([FromBody] DMTAPIRequest dMTAPIRequest)
        {
            if (ApplicationSetting.IsDMTWithPIPE)
            {
                var res = await dMTAPIUserML.VerifySender(dMTAPIRequest).ConfigureAwait(false);
                SaveDMRLog(JsonConvert.SerializeObject(dMTAPIRequest), JsonConvert.SerializeObject(res), "VerifySender");
                return Json(res);
            }
            return Forbid(JsonConvert.SerializeObject(new { Statuscode = ErrorCodes.Minus1, Message = "This method is forbidden" }));
        }
        [HttpPost]
        public async Task<IActionResult> SenderResendOTP([FromBody] DMTAPIRequest dMTAPIRequest)
        {
            if (ApplicationSetting.IsDMTWithPIPE)
            {
                var res = await dMTAPIUserML.SenderResendOTP(dMTAPIRequest).ConfigureAwait(false);
                SaveDMRLog(JsonConvert.SerializeObject(dMTAPIRequest), JsonConvert.SerializeObject(res), "SenderResendOTP");
                return Json(res);
            }
            return Forbid(JsonConvert.SerializeObject(new { Statuscode = ErrorCodes.Minus1, Message = "This method is forbidden" }));
        }
        [HttpPost]
        public async Task<IActionResult> AddBeneficiary([FromBody] CreateBeneReq dMTAPIRequest)
        {
            if (ApplicationSetting.IsDMTWithPIPE)
            {
                var res = await dMTAPIUserML.CreateBeneficiary(dMTAPIRequest).ConfigureAwait(false);
                SaveDMRLog(JsonConvert.SerializeObject(dMTAPIRequest), JsonConvert.SerializeObject(res), "AddBeneficiary");
                return Json(res);
            }
            return Forbid(JsonConvert.SerializeObject(new { Statuscode = ErrorCodes.Minus1, Message = "This method is forbidden" }));
        }
        [HttpPost]
        public async Task<IActionResult> DeleteBeneficiary([FromBody] DeleteBeneReq dMTAPIRequest)
        {
            if (ApplicationSetting.IsDMTWithPIPE)
            {
                var res = await dMTAPIUserML.RemoveBeneficiary(dMTAPIRequest).ConfigureAwait(false);
                SaveDMRLog(JsonConvert.SerializeObject(dMTAPIRequest), JsonConvert.SerializeObject(res), "DeleteBeneficiary");
                return Json(res);
            }
            return Forbid(JsonConvert.SerializeObject(new { Statuscode = ErrorCodes.Minus1, Message = "This method is forbidden" }));
        }
        [HttpPost]
        public async Task<IActionResult> GenerateBenficiaryOTP([FromBody] DeleteBeneReq dMTAPIRequest)
        {
            if (ApplicationSetting.IsDMTWithPIPE)
            {
                var res = await dMTAPIUserML.GenerateOTPBeneficiary(dMTAPIRequest).ConfigureAwait(false);
                SaveDMRLog(JsonConvert.SerializeObject(dMTAPIRequest), JsonConvert.SerializeObject(res), "GenerateBenficiaryOTP");
                return Json(res);
            }
            return Forbid(JsonConvert.SerializeObject(new { Statuscode = ErrorCodes.Minus1, Message = "This method is forbidden" }));
        }
        [HttpPost]
        public async Task<IActionResult> ValidateRemoveBeneficiaryOTP([FromBody] DMTTransactionReq dMTAPIRequest)
        {
            if (ApplicationSetting.IsDMTWithPIPE)
            {
                var res = await dMTAPIUserML.ValidateRemoveBeneficiaryOTP(dMTAPIRequest).ConfigureAwait(false);
                SaveDMRLog(JsonConvert.SerializeObject(dMTAPIRequest), JsonConvert.SerializeObject(res), "ValidateRemoveBeneficiaryOTP");
                return Json(res);
            }
            return Forbid(JsonConvert.SerializeObject(new { Statuscode = ErrorCodes.Minus1, Message = "This method is forbidden" }));
        }
        [HttpPost]
        public async Task<IActionResult> GetBeneficiary([FromBody] DMTAPIRequest dMTAPIRequest)
        {
            if (ApplicationSetting.IsDMTWithPIPE)
            {
                var res = await dMTAPIUserML.GetBeneficiary(dMTAPIRequest).ConfigureAwait(false);
                SaveDMRLog(JsonConvert.SerializeObject(dMTAPIRequest), JsonConvert.SerializeObject(res), "GetBeneficiary");
                return Json(res);
            }
            return Forbid(JsonConvert.SerializeObject(new { Statuscode = ErrorCodes.Minus1, Message = "This method is forbidden" }));
        }
        [HttpPost]
        public async Task<IActionResult> ValidateBeneficiary([FromBody] DMTTransactionReq dMTAPIRequest)
        {
            if (ApplicationSetting.IsDMTWithPIPE)
            {
                var res = await dMTAPIUserML.ValidateBeneficiaryOTP(dMTAPIRequest).ConfigureAwait(false);
                SaveDMRLog(JsonConvert.SerializeObject(dMTAPIRequest), JsonConvert.SerializeObject(res), "ValidateBeneficiary");
                return Json(res);
            }
            return Forbid(JsonConvert.SerializeObject(new { Statuscode = ErrorCodes.Minus1, Message = "This method is forbidden" }));
        }
        [HttpPost]
        public async Task<IActionResult> VerifyAccount([FromBody] DMTTransactionReq dMTAPIRequest)
        {
            if (ApplicationSetting.IsDMTWithPIPE)
            {
                var res = await dMTAPIUserML.VerifyAccount(dMTAPIRequest).ConfigureAwait(false);
                SaveDMRLog(JsonConvert.SerializeObject(dMTAPIRequest), JsonConvert.SerializeObject(res), "VerifyAccount");
                return Json(res);
            }
            return Forbid(JsonConvert.SerializeObject(new { Statuscode = ErrorCodes.Minus1, Message = "This method is forbidden" }));
        }
        [HttpPost]
        public async Task<IActionResult> AccountTransfer([FromBody] DMTTransactionReq dMTAPIRequest)
        {
            if (ApplicationSetting.IsDMTWithPIPE)
            {
                var res = await dMTAPIUserML.AccountTransfer(dMTAPIRequest).ConfigureAwait(false);
                SaveDMRLog(JsonConvert.SerializeObject(dMTAPIRequest), JsonConvert.SerializeObject(res), "AccountTransfer");
                return Json(res);
            }
            return Forbid(JsonConvert.SerializeObject(new { Statuscode = ErrorCodes.Minus1, Message = "This method is forbidden" }));
        }
        [HttpPost("API/DoUPIPayment")]
        public async Task<IActionResult> DoUPIPayment([FromBody] DMTTransactionReq dMTAPIRequest)
        {
            if (ApplicationSetting.IsUPIPaymentResale) {
                var res = await dMTAPIUserML.DoUPIPayment(dMTAPIRequest).ConfigureAwait(false);
                SaveDMRLog(JsonConvert.SerializeObject(dMTAPIRequest), JsonConvert.SerializeObject(res), "DoUPIPayment");
                return Json(res);
            }
            return Forbid(JsonConvert.SerializeObject(new { Statuscode = ErrorCodes.Minus1, Message = "This method is forbidden" }));
        }
        private void SaveDMRLog(string Req, string Res, string Method)
        {

            var req = new APIReqResp
            {
                Request = Req,
                Response = Res,
                Method = Method
            };
            new APIUserML(_accessor, _env).SaveDMRAPILog(req);
        }
    }
}
