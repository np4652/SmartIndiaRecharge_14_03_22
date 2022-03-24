using Fintech.AppCode;
using Fintech.AppCode.Configuration;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.ThirdParty.WhatsappAPI;
using System;
using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace RoundpayFinTech.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class CustomerCareController : Controller
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly ISession _session;
        private readonly LoginResponse _lr;
        private readonly ILoginML loginML;

        public GetWhatsappContactListModel WhatsappResponse { get; private set; }

        public CustomerCareController(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            _session = _accessor.HttpContext.Session;
            _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
            loginML = new LoginML(_accessor, _env);
        }
        public IActionResult Index()
        {
           
            return View();
        }
        #region UserSubcriptionCustomer
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (_lr == null)
            {
                context.Result = new RedirectResult("~/");
            }

            else if (loginML.IsInValidSession() && (context.RouteData.Values["Action"].ToString() != "Index" || context.RouteData.Values["Controller"].ToString() != "CustomerCare") || (_lr.RoleID != Role.Retailor_Seller && _lr.LoginTypeID == LoginType.ApplicationUser) || context.RouteData.Values["Controller"].ToString() != "Admin")
            {
                if (loginML.IsInValidSession() && (context.RouteData.Values["Action"].ToString() != "Index" || context.RouteData.Values["Controller"].ToString() != "Admin"))
                {
                    context.Result = new RedirectResult("~/");
                }
                else
                {
                    base.OnActionExecuting(context);
                }
            }
            else
            {
                base.OnActionExecuting(context);
            }
        }
        [HttpGet]
        [Route("Home/UserSubcriptionCustomer")]
        [Route("UserSubcriptionCustomer")]
        public IActionResult UserSubcriptionCustomer()
        {
            return View();
        }
        [HttpPost]
        [Route("Home/_UserSubcriptionCustomer")]
        [Route("_UserSubcriptionCustomer")]
        public IActionResult _UserSubcriptionCustomer(int TopRows, string Request, string MobileNo, string Day)
        {
            IUserML UserML = new UserML(_accessor, _env);
            CommonReq _req = new CommonReq
            {

                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID,
                CommonInt = TopRows,
                CommonStr = Request,
                CommonStr2 = MobileNo,
                CommonStr3 = Day,
            };
            GetinTouctListModel var = UserML.ProcGetUserSubscriptionCusCare(_req);
            return PartialView("Partial/_UserSubcriptionCustomer", var);
        }

        [HttpPost]
        [Route("Home/UpdateStatus/{Status}/{itemid}")]
        [Route("UpdateStatus/{Status}/{itemid}")]
        public IActionResult UpdateStatus(string status, int itemid)
        {
            CommonReq _req = new CommonReq
            {
                CommonInt = itemid,
                CommonStr = status,
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID
            };
            IUserML userML = new UserML(_accessor, _env);
            IResponseStatus _res = userML.UpdateuserSubscriptionStatusCuscare(_req);
            return Json(_res);
        }
        [HttpPost]
        [Route("Home/UpdateCustomerRemark")]
        [Route("UpdateCustomerRemark")]
        public IActionResult UpdateCustomerRemark(int ID, string Remarks, string NextFdate)
        {

            CommonReq _req = new CommonReq
            {
                CommonInt = ID,
                CommonStr = Remarks,
                CommonStr2 = NextFdate,
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID
            };
            IUserML userML = new UserML(_accessor, _env);
            IResponseStatus _res = userML.UpdateuserSubscriptionRemarksCuscare(_req);
            return Json(_res);
        }
        #endregion

        #region WhatsAppConversation
        #region send Text,Image,Video To Users
        [HttpPost]
        [Route("CustomerCare/SendWhatsappSessionMessage")]
        [Route("SendWhatsappSessionMessage")]
        public IActionResult SendWhatsappSessionMessage(WhatsappConversation wc)
        {
            IWhatsappML whatsappML = new WhatsappML(_accessor, _env);
            wc.LoginTypeID = _lr.LoginTypeID;
            wc.CCName = _lr.Name;
            wc.CCID = _lr.UserID;
            wc.Id = 0;
            wc.Type = "text";
           whatsappML.SendWhatsappSessionMessageAllAPI(wc);
            return Json(1);
        }
        #endregion
        #region ChatArea
        [HttpPost]
        [Route("CustomerCare/WpChatArea")]
        [Route("WpChatArea")]
        public IActionResult WpChatArea(int ID, int AID,int sndnoid)
        {
            IWhatsappML whatsappML = new WhatsappML(_accessor, _env);
            var _req = new CommonReq
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID,
                CommonInt3 = ID,
                CommonInt = AID,
                CommonInt2 = sndnoid
            };

            GetWhatsappContactListModel var = whatsappML.GetWhatsappConversation(_req);
            ViewBag.wpchtarea = var;
            return PartialView("Partial/_WpChatArea", var);
        }
        #endregion

        #region WhatsAppConversationNew
        [HttpGet]
        [Route("CustomerCare/WhatsAppConversationNew")]
        [Route("WhatsAppConversationNew")]
        public async Task<IActionResult> WhatsAppConversationNewAsync(int ID)
        {
            IWhatsappML whatsappML = new WhatsappML(_accessor, _env);
            var _req = new CommonReq
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID

            };
            ViewBag.CustmoreName = _lr.Name;
            ViewData["LayoutID"] = ID;
            return View("WhatsAppConversationNew", whatsappML.GetWhatsappSenderNoList(_req));
        }
        //Whatsapp New messages
        [HttpPost]
        [Route("CustomerCare/WhatsappNewMessage")]
        [Route("WhatsappNewMessage")]
        public IActionResult WhatsappNewMessage(string Mobileno)
        {
            IWhatsappML whatsappML = new WhatsappML(_accessor, _env);
            var _req = new CommonReq
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID,
                CommonStr = Mobileno
            };
            var resultresponse = new WhatsappResponse();
            resultresponse = whatsappML.SelectNewMsg(_req);
            return Json(resultresponse);
        }
        #endregion

        #region ContactSearch USerContacySearch
        // user contact no contact 
        [HttpPost]
        [Route("CustomerCare/ContactSearch")]
        [Route("ContactSearch")]
        public IActionResult ContactSearch(string SearchValue, string TabFill, int sid, bool isFirstCall = true)
        {
            IWhatsappML whatsappML = new WhatsappML(_accessor, _env);
            var _req = new CommonReq
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID,
                CommonStr = SearchValue,
                CommonInt = TabFill== WhatsappTab.UnSeen ? 1:0,
                CommonStr2 = "",
                CommonInt2 = sid,
                CommonBool = false,
                CommonInt3 = TabFill == WhatsappTab.Task ? 1 : 0,
                CommonStr3 = TabFill == WhatsappTab.NoReply ? "received" : ""
            };
            GetWhatsappContactListModel getWhatsappContactListModel = whatsappML.ProcGetWhatsappContactsSearch(_req);
            if (isFirstCall)
                return PartialView("Partial/_ContactSearch", getWhatsappContactListModel);
            else
                return Json(getWhatsappContactListModel);
        }

        // user Search contact 
        [HttpPost]
        [Route("CustomerCare/UserContactSearch")]
        [Route("UserContactSearch")]
        public IActionResult UserContactSearch(string SearchValue)
        {
            IWhatsappML whatsappML = new WhatsappML(_accessor, _env);
            var _req = new CommonReq
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID,
                CommonInt = _lr.RoleID,
                CommonStr = SearchValue
            };
            GetWhatsappContactListModel getWhatsappContactListModel = whatsappML.ProcSearchUserContacts(_req);
            return PartialView("Partial/_UserContactSearch", getWhatsappContactListModel);
        }

        // user sync contact serarch
        [HttpPost]
        [Route("CustomerCare/SyncContacts")]
        [Route("SyncContacts")]
        public IActionResult SyncContacts(int UID)
        {
            IWhatsappML whatsappML = new WhatsappML(_accessor, _env);
            var _req = new CommonReq
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID,
                CommonInt = UID
            };
            WhatsappResponse res = whatsappML.ProcSyncUserWatsappContacts(_req);
            return Json(res);
        }
        #endregion
        #region GetWatsappImage and Video
        public async Task<IActionResult> GetWatsappImage(string url, string ac)
        {
            IWhatsappML whatsappML = new WhatsappML(_accessor, _env);
            byte[] resulttresponse = await whatsappML.GetWatsappFile(url, ac).ConfigureAwait(false);
            return File(resulttresponse, "image/jpg");
        }
        public async Task<IActionResult> GetWatsappVideo(string url)
        {
            IWhatsappML whatsappML = new WhatsappML(_accessor, _env);
            byte[] resultresponse = await whatsappML.GetWatsappVideoFile(url).ConfigureAwait(false);
            return File(resultresponse, "video/m4v");
        }
        #endregion

        #region forwardMessages
        [HttpPost]
        [Route("CustomerCare/ForwardContacts")]
        [Route("ForwardContacts")]
        public IActionResult ForwardContacts(string SearchValue, string ac, bool onedaychat)
        {
            IWhatsappML whatsappML = new WhatsappML(_accessor, _env);
            var _req = new CommonReq
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID,
                CommonStr = SearchValue,
                CommonStr2 = ac,
                CommonBool = onedaychat
            };
            GetWhatsappContactListModel getWhatsappContactListModel = whatsappML.ProcGetWhatsappContactsSearch(_req);

            return PartialView("Partial/_ForwardMessage", getWhatsappContactListModel);
        }
        [HttpPost]
        [Route("CustomerCare/SendForwardMessage")]
        [Route("SendForwardMessage")]
        public async Task<IActionResult> SendForwardMessage(string CTS)
        {
            IWhatsappML whatsappML = new WhatsappML(_accessor, _env);
            var res=await whatsappML.SendWhatsappForwardMessageAllAPI(CTS);
            return Json(1);
        }

        [HttpPost]
        [Route("CustomerCare/SaveWhatsappTask")]
        [Route("SaveWhatsappTask")]
        public async Task<IActionResult> SaveWhatsappTask(int CID,int Task)
        {
            IWhatsappML whatsappML = new WhatsappML(_accessor, _env);
            return Json(whatsappML.UpdateWhatsappTask(CID,Task));
        }

        [HttpPost]
        [Route("CustomerCare/GetSenderNoForService")]
        [Route("GetSenderNoForService")]
        public async Task<IActionResult> GetSenderNoForService()
        {
            IWhatsappML whatsappML = new WhatsappML(_accessor, _env);
            return Json(whatsappML.WhatsappSenderNoService(false));
        }
        [HttpPost]
        [Route("CustomerCare/ActWhatsappSenderNoService")]
        [Route("ActWhatsappSenderNoService")]
        public async Task<IActionResult> ActWhatsappSenderNoService()
        {
            IWhatsappML whatsappML = new WhatsappML(_accessor, _env);
            return Json(whatsappML.WhatsappSenderNoService(true));
        }


        #endregion
        #endregion
    }
}