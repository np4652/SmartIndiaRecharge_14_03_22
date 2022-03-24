using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RoundpayFinTech.AppCode;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;

namespace APIOnly.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class InvoiceController : Controller
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        //private readonly ISession _session;
        //private readonly LoginResponse _lr;
        // private readonly IUserML userML;

        public InvoiceController(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            //_session = _accessor.HttpContext.Session;
            // _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
            //userML = new UserML(_accessor, _env);
        }
        public IActionResult Index()
        {
            if (IsInValidSession())
            {
                return Redirect("~/");
            }
            return View();
        }
        [HttpGet("Invoice/Operatorwise")]
        public IActionResult Operatorwise(string m, string mth)
        {
            int t = 1;
            IInvoiceReportML reportML = new ReportML(_accessor, _env);
            var lst = reportML.GetInvoiceData(m, mth, t);
            return View(lst);
        }

        [HttpGet("Invoice/Servicewise")]
        public IActionResult Servicewise(string m, string mth)
        {
            int t = 2;
            IInvoiceReportML reportML = new ReportML(_accessor, _env);
            var lst = reportML.GetInvoiceData(m, mth, t);
            return View(lst);
        }

        [HttpGet("Invoice/Surcharge")]
        public IActionResult Surcharge(string m, string mth)
        {
            int t = 3;
            IInvoiceReportML reportML = new ReportML(_accessor, _env);
            var lst = reportML.GetInvoiceData(m, mth, t);
            return View(lst);
        }
        [HttpGet("Invoice/P2A")]
        public IActionResult P2A(string m, string mth)
        {
            int t = 4;
            IInvoiceReportML reportML = new ReportML(_accessor, _env);
            var lst = reportML.GetInvoiceData(m, mth, t);
            return View(lst);
        }
        [HttpGet("Invoice/DebitNote")]
        public IActionResult DebitNote(string m, string mth)
        {
            int t = 5;
            IInvoiceReportML reportML = new ReportML(_accessor, _env);
            var lst = reportML.GetInvoiceData(m, mth, t);
            return View(lst);
        }
        [HttpGet("Invoice/RCM")]
        public IActionResult RCM(string m, string mth)
        {
            int t = 6;
            IInvoiceReportML reportML = new ReportML(_accessor, _env);
            var lst = reportML.GetInvoiceData(m, mth, t);
            return View(lst);
        }
        
        [HttpGet("Invoice/Summary")]
        public IActionResult InvoiceSummary(string m, string mth)
        {
            IInvoiceReportML reportML = new ReportML(_accessor, _env);
            var lst = reportML.GetInvoiceSummary(m, mth);
            return View(lst);
        }

        [HttpPost]
        [Route("Invoice/Upload-Invoice")]
        [Route("Upload-Invoice")]
        public IActionResult UploadInvoice(string m, IFormFile file, bool isRCM)
        {
            IUserML userML = new UserML(_accessor, _env);
            var res = userML.UploadInvoice(file, m, isRCM);
            return Json(res);
        }
        private bool IsInValidSession()
        {
            string AppSessionID = HttpContext.Session.GetString(SessionKeys.AppSessionID);
            return (string.IsNullOrEmpty(AppSessionID) || AppSessionID.Length != Numbers.THIRTY_TWO);
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (IsInValidSession() && (context.RouteData.Values["Action"].ToString() != "Index" || context.RouteData.Values["Controller"].ToString() != "Invoice"))
            {
                context.Result = new RedirectResult("~/UserLogin");
            }
            else
            {
                base.OnActionExecuting(context);
            }
        }
    }
}
