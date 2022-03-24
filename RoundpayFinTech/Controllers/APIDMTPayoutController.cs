using Fintech.AppCode.HelperClass;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.ProcModel;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Configuration;

namespace RoundpayFinTech.Controllers
{
    public partial class APIController : IAPIPayoutDMTService
    {
        [HttpPost("API/Payout")]
        public async Task<IActionResult> DMTPayout([FromBody] PayoutTransactionRequest payoutRequest)
        {
            if (ApplicationSetting.IsPayoutAPIResale) {
                var xpressRequestML = new XPressServiceML(_accessor, _env);
                var res = await xpressRequestML.DoPayout(payoutRequest).ConfigureAwait(false);
                SaveDMRLog(JsonConvert.SerializeObject(payoutRequest), JsonConvert.SerializeObject(res));
                return Json(res);
            }
            return Forbid();
        }

        [HttpPost("API/Payout/VerifyBeneficiary")]
        public async Task<IActionResult> DMTPayoutVerify([FromBody] PayoutTransactionRequest payoutRequest)
        {
            if (ApplicationSetting.IsPayoutAPIResale)
            {
                var xpressRequestML = new XPressServiceML(_accessor, _env);
                var res = await xpressRequestML.DoPayoutVerify(payoutRequest).ConfigureAwait(false);
                SaveDMRLog(JsonConvert.SerializeObject(payoutRequest), JsonConvert.SerializeObject(res));
                return Json(res);
            }
            return Forbid();
        }
    }
}
