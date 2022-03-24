using System.Threading.Tasks;
using Fintech.AppCode.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;

namespace RoundpayFinTech.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class LapuController : Controller
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        public LapuController(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
        }
        public async Task<IActionResult> Index(string APIName)
        {
            ICallbackML callback = new CallbackML(_accessor, _env);
            return Content(await callback.GetLapuTransactions(APIName), "text/xml");
        }
        public async Task<IActionResult> Social(string APIName, int SocialAlertType)
        {
            ICallbackML callback = new CallbackML(_accessor, _env);
            return Content(await callback.GetLapuSocialAlert(APIName, SocialAlertType), "text/xml");
        }
        public async Task<IActionResult> PSADetail(string APIName)
        {
            ICallbackML callback = new CallbackML(_accessor, _env);
            return Json(await callback.GetPSADetailMachine(APIName).ConfigureAwait(false));
        }
        public IActionResult UpdatePSADetail(int OutletAPIID, string PSAID, int Status, string Remark)
        {
            ICallbackML callback = new CallbackML(_accessor, _env);
            return Json(callback.UpdatePSAFromMachine(new _CallbackData
            {
                TID = OutletAPIID,
                AccountNo = PSAID,
                Statuscode = Status,
                LiveID = Remark
            }));
        }
    }
}