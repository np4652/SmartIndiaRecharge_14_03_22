using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Recharge;
using System.Threading.Tasks;
using Validators;

namespace RoundpayFinTech.Controllers
{
    
    public partial class APIController : IAPIOutletService
    {
        [HttpPost("API/Outlet/Register")]
        public async Task<IActionResult> RegisterOutlet([FromBody] APIRequestOutlet aPIRequest)
        {
            IOutletAPIUserMiddleLayer ML = new APIUserML(_accessor, _env);
            var res = ML.SaveOutletOfAPIUser(aPIRequest);
            if (aPIRequest != null)
            {
                if (aPIRequest.kycDoc != null)
                {
                    aPIRequest.kycDoc.PAN = string.IsNullOrEmpty(aPIRequest.kycDoc.PAN) ? "NO DATA" : "DATA FOUND";
                    aPIRequest.kycDoc.PASSBOOK = string.IsNullOrEmpty(aPIRequest.kycDoc.PASSBOOK) ? "NO DATA" : "DATA FOUND";
                    aPIRequest.kycDoc.PHOTO = string.IsNullOrEmpty(aPIRequest.kycDoc.PHOTO) ? "NO DATA" : "DATA FOUND";
                    aPIRequest.kycDoc.GSTRegistration = string.IsNullOrEmpty(aPIRequest.kycDoc.GSTRegistration) ? "NO DATA" : "DATA FOUND";
                    aPIRequest.kycDoc.AADHAR = string.IsNullOrEmpty(aPIRequest.kycDoc.AADHAR) ? "NO DATA" : "DATA FOUND";
                    aPIRequest.kycDoc.BusinessAddressProof = string.IsNullOrEmpty(aPIRequest.kycDoc.BusinessAddressProof) ? "NO DATA" : "DATA FOUND";
                    aPIRequest.kycDoc.CancelledCheque = string.IsNullOrEmpty(aPIRequest.kycDoc.CancelledCheque) ? "NO DATA" : "DATA FOUND";
                    aPIRequest.kycDoc.DrivingLicense = string.IsNullOrEmpty(aPIRequest.kycDoc.DrivingLicense) ? "NO DATA" : "DATA FOUND";
                    aPIRequest.kycDoc.ServiceAggreement = string.IsNullOrEmpty(aPIRequest.kycDoc.ServiceAggreement) ? "NO DATA" : "DATA FOUND";
                    aPIRequest.kycDoc.ShopImage = string.IsNullOrEmpty(aPIRequest.kycDoc.ShopImage) ? "NO DATA" : "DATA FOUND";
                    aPIRequest.kycDoc.VoterID = string.IsNullOrEmpty(aPIRequest.kycDoc.VoterID) ? "NO DATA" : "DATA FOUND";
                }
            }
            var aPIReqResp = new APIReqResp
            {
                UserID = aPIRequest.UserID,
                Request = JsonConvert.SerializeObject(aPIRequest),
                Response = JsonConvert.SerializeObject(res),
                Method = "RegisterOutlet"
            };
            await _apiML.SaveAPILog(aPIReqResp).ConfigureAwait(false);
            return Json(res);
        }
        [HttpPost("API/Outlet/Update")]
        public async Task<IActionResult> UpdateOutlet([FromBody] APIRequestOutlet aPIRequest)
        {
            IOutletAPIUserMiddleLayer ML = new APIUserML(_accessor, _env);
            var res = ML.UpdateOutletOfAPIUser(aPIRequest);
            if (aPIRequest != null)
            {
                if (aPIRequest.kycDoc != null)
                {
                    aPIRequest.kycDoc.PAN = string.IsNullOrEmpty(aPIRequest.kycDoc.PAN) ? "NO DATA" : "DATA FOUND";
                    aPIRequest.kycDoc.PASSBOOK = string.IsNullOrEmpty(aPIRequest.kycDoc.PASSBOOK) ? "NO DATA" : "DATA FOUND";
                    aPIRequest.kycDoc.PHOTO = string.IsNullOrEmpty(aPIRequest.kycDoc.PHOTO) ? "NO DATA" : "DATA FOUND";
                    aPIRequest.kycDoc.GSTRegistration = string.IsNullOrEmpty(aPIRequest.kycDoc.GSTRegistration) ? "NO DATA" : "DATA FOUND";
                    aPIRequest.kycDoc.AADHAR = string.IsNullOrEmpty(aPIRequest.kycDoc.AADHAR) ? "NO DATA" : "DATA FOUND";
                    aPIRequest.kycDoc.BusinessAddressProof = string.IsNullOrEmpty(aPIRequest.kycDoc.BusinessAddressProof) ? "NO DATA" : "DATA FOUND";
                    aPIRequest.kycDoc.CancelledCheque = string.IsNullOrEmpty(aPIRequest.kycDoc.CancelledCheque) ? "NO DATA" : "DATA FOUND";
                    aPIRequest.kycDoc.DrivingLicense = string.IsNullOrEmpty(aPIRequest.kycDoc.DrivingLicense) ? "NO DATA" : "DATA FOUND";
                    aPIRequest.kycDoc.ServiceAggreement = string.IsNullOrEmpty(aPIRequest.kycDoc.ServiceAggreement) ? "NO DATA" : "DATA FOUND";
                    aPIRequest.kycDoc.ShopImage = string.IsNullOrEmpty(aPIRequest.kycDoc.ShopImage) ? "NO DATA" : "DATA FOUND";
                    aPIRequest.kycDoc.VoterID = string.IsNullOrEmpty(aPIRequest.kycDoc.VoterID) ? "NO DATA" : "DATA FOUND";
                }
            }
            var aPIReqResp = new APIReqResp
            {
                UserID = aPIRequest.UserID,
                Request = JsonConvert.SerializeObject(aPIRequest),
                Response = JsonConvert.SerializeObject(res),
                Method = "UpdateOutlet"
            };
            await _apiML.SaveAPILog(aPIReqResp);
            return Json(res);
        }
        [HttpPost("API/Outlet/CheckStatus")]
        public async Task<IActionResult> OutletStatuscheck([FromBody] APIRequestOutlet aPIRequest)
        {
            IOutletAPIUserMiddleLayer ML = new APIUserML(_accessor, _env);
            var res = ML.CheckOutletStatus(aPIRequest);
            var aPIReqResp = new APIReqResp
            {
                UserID = aPIRequest.UserID,
                Request = JsonConvert.SerializeObject(aPIRequest),
                Response = JsonConvert.SerializeObject(res),
                Method = "OutletStatuscheck"
            };
            await _apiML.SaveAPILog(aPIReqResp);
            return Json(res);
        }

        [HttpPost("API/Outlet/ServiceStatus")]
        public async Task<IActionResult> OutletServiceStatus([FromBody] APIRequestOutlet aPIRequest)
        {
            IOutletAPIUserMiddleLayer ML = new APIUserML(_accessor, _env);
            var res = ML.CheckOutletServiceStatus(aPIRequest);
            var aPIReqResp = new APIReqResp
            {
                UserID = aPIRequest.UserID,
                Request = JsonConvert.SerializeObject(aPIRequest),
                Response = JsonConvert.SerializeObject(res),
                Method = "OutletServiceStatus"
            };
            await _apiML.SaveAPILog(aPIReqResp).ConfigureAwait(false);
            return Json(res);
        }

        [HttpPost("API/Resource/Get")]
        public IActionResult GetResource([FromBody] APIRequestOutlet aPIRequest)
        {
            IOutletAPIUserMiddleLayer ML = new APIUserML(_accessor, _env);
            var res = ML.GetResources(aPIRequest);
            return Json(res);
        }
        [HttpGet("API/Service/KYCStatus")]
        public IActionResult ServiceKYCStatus(string SPKey) {
            OperatorML opML = new OperatorML(_accessor,_env);
           return Ok(opML.IsKYCRequiredForService(SPKey));
        }
        public IActionResult IsEmail(string s)
        {
            return Ok(Validate.O.IsDateIn_dd_MMM_yyyy_Format(s));
        }

        public IActionResult GetCheck(string s)
        {
            s = string.IsNullOrEmpty(s) ? string.Empty : s;
            return Ok(Validate.O.IsLongitudeInValid(s));
        }
    }
}
