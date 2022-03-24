using Fintech.AppCode.HelperClass;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.ProcModel;
using Newtonsoft.Json;
using System;
using RoundpayFinTech.AppCode.Model.Recharge;
using RoundpayFinTech.AppCode.Configuration;
using RoundpayFinTech.AppCode.MiddleLayer.Dmt_Api;
using RoundpayFinTech.AppCode.ThirdParty.Fingpay;

namespace RoundpayFinTech.Controllers
{
    
    public partial class APIController : IDMTTransactionService
    {
        [HttpPost("API/DMT/GetSender")]
        public async Task<IActionResult> GetSender([FromBody] DMTAPIRequest dMTAPIRequest)
        {
            var res = await _apiML.SenderLogin(dMTAPIRequest).ConfigureAwait(false);
            SaveDMRLog(JsonConvert.SerializeObject(dMTAPIRequest), JsonConvert.SerializeObject(res));
            return Json(res);
        }
        [HttpPost("API/DMT/CreateSender")]
        public async Task<IActionResult> CreateSender([FromBody] CreateSednerReq dMTAPIRequest)
        {
            var res = await _apiML.CreateSender(dMTAPIRequest).ConfigureAwait(false);
            SaveDMRLog(JsonConvert.SerializeObject(dMTAPIRequest), JsonConvert.SerializeObject(res));
            return Json(res);
        }
        [HttpPost("API/DMT/VerifySender")]
        public async Task<IActionResult> VerifySender([FromBody] DMTAPIRequest dMTAPIRequest)
        {
            var res = await _apiML.VerifySender(dMTAPIRequest).ConfigureAwait(false);
            SaveDMRLog(JsonConvert.SerializeObject(dMTAPIRequest), JsonConvert.SerializeObject(res));
            return Json(res);
        }
        [HttpPost("API/DMT/AddBeneficiary")]
        public async Task<IActionResult> AddBeneficiary([FromBody] CreateBeneReq dMTAPIRequest)
        {
            var res = await _apiML.CreateBeneficiary(dMTAPIRequest).ConfigureAwait(false);
            SaveDMRLog(JsonConvert.SerializeObject(dMTAPIRequest), JsonConvert.SerializeObject(res));
            return Json(res);
        }
        [HttpPost("API/DMT/DeleteBeneficiary")]
        public async Task<IActionResult> DeleteBeneficiary([FromBody] DeleteBeneReq dMTAPIRequest)
        {
            var res = await _apiML.DeleteBeneficiary(dMTAPIRequest).ConfigureAwait(false);
            SaveDMRLog(JsonConvert.SerializeObject(dMTAPIRequest), JsonConvert.SerializeObject(res));
            return Json(res);
        }
        [HttpPost("API/DMT/GetBeneficiary")]
        public async Task<IActionResult> GetBeneficiary([FromBody] DMTAPIRequest dMTAPIRequest)
        {
            var res = await _apiML.GetBeneficiary(dMTAPIRequest).ConfigureAwait(false);
            SaveDMRLog(JsonConvert.SerializeObject(dMTAPIRequest), JsonConvert.SerializeObject(res));
            return Json(res);
        }
        [HttpPost("API/DMT/GenerateBenficiaryOTP")]
        public async Task<IActionResult> GenerateBenficiaryOTP([FromBody] DMTAPIRequest dMTAPIRequest)
        {
            var res = await _apiML.GenerateBenficiaryOTP(dMTAPIRequest).ConfigureAwait(false);
            SaveDMRLog(JsonConvert.SerializeObject(dMTAPIRequest), JsonConvert.SerializeObject(res));
            return Json(res);
        }
        [HttpPost("API/DMT/ValidateBeneficiary")]
        public async Task<IActionResult> ValidateBeneficiary([FromBody] CreateBeneReq dMTAPIRequest)
        {
            var res = await _apiML.ValidateBeneficiary(dMTAPIRequest).ConfigureAwait(false);
            SaveDMRLog(JsonConvert.SerializeObject(dMTAPIRequest), JsonConvert.SerializeObject(res));
            return Json(res);
        }
        [HttpPost("API/DMT/VerifyAccount")]
        public async Task<IActionResult> VerifyAccount([FromBody] DMTTransactionReq dMTAPIRequest)
        {
            var res = await _apiML.VerifyAccount(dMTAPIRequest).ConfigureAwait(false);
            SaveDMRLog(JsonConvert.SerializeObject(dMTAPIRequest), JsonConvert.SerializeObject(res));
            return Json(res);
        }
        [HttpPost("API/DMT/SendMoney")]
        public async Task<IActionResult> SendMoney([FromBody] DMTTransactionReq dMTAPIRequest)
        {
            var res = await _apiML.SendMoney(dMTAPIRequest).ConfigureAwait(false);
            SaveDMRLog(JsonConvert.SerializeObject(dMTAPIRequest), JsonConvert.SerializeObject(res));
            return Json(res);
        }
        [HttpPost("API/DMT/SplitAmount")]
        public IActionResult SplitAmountMethod(int RAmount, int m, int x)
        {
            return Json(ConverterHelper.O.SplitAmounts(RAmount, m, x));
        }
        [HttpGet("API/MD5")]
        public IActionResult MD5Test(string s)
        {
            return Json(HashEncryption.O.AppEncryptPayment(s));
        }
        [HttpGet("API/TimeStamp")]
        public IActionResult TimeStamp()
        {
            return Json((DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000).ToString());
        }
        private void SaveDMRLog(string Req, string Res)
        {
            var req = new APIReqResp
            {
                Request = Req,
                Response = Res
            };
            _apiML.SaveDMRAPILog(req);
        }
        [HttpGet("API/ICICIRegister")]
        public async Task<IActionResult> ICICIRegister(int ProjectID)
        {
            if (ApplicationSetting.ProjectID != ProjectID)
                return Ok("This feature not for your project");
            ICICIPayoutML iCICIPayoutML = new ICICIPayoutML(_accessor, _env, 0);


            return Ok(await iCICIPayoutML.ICICIRegister().ConfigureAwait(false));


        }


        public IActionResult GEt(string ss)
        {


            var razorPayCallbackResp = JsonConvert.DeserializeObject<FPMiniBankStatusCheckResponse>(ss);
            return Json(razorPayCallbackResp);
        }

        public IActionResult GetB(bool IsRP)
        {
            ApplicationSetting.IsRPOnly = IsRP;
            return Ok();
        }
    }


}
