using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoundpayFinTech.Models;
using RoundpayFinTech.AppCode.MiddleLayer;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Interfaces;
using System.Linq;
using Fintech.AppCode.Model;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Configuration;
using Fintech.AppCode;
using RoundpayFinTech.AppCode;
using Microsoft.AspNetCore.Mvc.Rendering;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Data;
using Fintech.AppCode.HelperClass;
using OfficeOpenXml;
using RoundpayFinTech.AppCode.Model.Report;

namespace RoundpayFinTech.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class FOSController : Controller
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly ISession _session;
        private readonly LoginResponse _lr;
        private readonly ILoginML loginML;
        public FOSController(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            _session = _accessor.HttpContext.Session;
            _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
            loginML = new LoginML(_accessor, _env);
        }

        public IActionResult Index()
        {
            if (loginML.IsInValidSession())
            {
                return RedirectToAction("Index", "Login");
            }
            return View();
        }

        [HttpGet]
        [Route("Admin/AssignRetailer")]
        [Route("AssignRetailer")]
        public IActionResult AssignRetailer()
        {
            CommonFilter f = new CommonFilter { 
            RoleID=0
            };
           
            IUserML userML = new UserML(_accessor, _env);
            IFOSML _IFOSML = new FOSML(_accessor, _env);
            var FosList = userML.GetFOSList(f);
            FosList.userReports = FosList.userReports.Where(w => w.RoleID == FixedRole.FOS).ToList();
            var userListModel = new UserListModel
            {
                selectListItems = new SelectList(FosList.userReports, "ID", "Name"),
                userBalnace = userML.GetUserBalnace(0)
            };
            return View("UserList/AssignRetailer", userListModel);
        }
        [HttpPost]
        [Route("Admin/R_List")]
        [Route("R_List")]
        public IActionResult RetailerList([FromBody]CommonFilter f)
        {
         
            IFOSML _IFOSML = new FOSML(_accessor, _env);
            IUserML userML = new UserML(_accessor, _env);
            var res = userML.GetFOSList(f);
            res.userReports = res.userReports.Where(w => w.RoleID == FixedRole.Retailor).ToList();
            if (f.MapStatus == MapStatus.Unassigned)
                res.userReports = res.userReports.Where(w => w.RoleID == Role.Retailor_Seller & w.FOSId == MapStatus.Unassign).ToList();
            else if(f.MapStatus == MapStatus.Assigned)
            {
                if (f.FOSID > MapStatus.Unassign)
                    res.userReports = res.userReports.Where(w => w.RoleID == Role.Retailor_Seller & w.FOSId == f.FOSID).ToList();
                else
                    res.userReports = res.userReports.Where(w => w.RoleID == Role.Retailor_Seller & w.FOSId != MapStatus.Unassign).ToList();
            }
            else
            {
                res.userReports = res.userReports.Where(w => w.RoleID == Role.Retailor_Seller).ToList();
            }
            var userListModel = new UserListModel
            {
                userList = res,
                userBalnace = userML.GetUserBalnace(0)
            };
            return PartialView("UserList/Partial/_AssignRetailer", userListModel);
        }

        [HttpGet]
        [Route("Admin/R_List")]
        [Route("R_List")]
        public IActionResult RetailerListExport(CommonFilter f)
        {
            IFOSML _IFOSML = new FOSML(_accessor, _env);
            DataTable dataTable = ConverterHelper.O.ToDataTable(_IFOSML.GetFOSUserExcel(f));

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("RetailerList");
                worksheet.Cells["A1"].LoadFromDataTable(dataTable, PrintHeaders: true);
                worksheet.Row(1).Height = 20;
                worksheet.Row(1).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Row(1).Style.Font.Bold = true;
                for (var col = 1; col < dataTable.Columns.Count + 1; col++)
                {
                    worksheet.Column(col).AutoFit();
                }
                ExportToExcel exportToExcel = new ExportToExcel
                {
                    Contents = package.GetAsByteArray(),
                    FileName = "RetailerList.xlsx"
                };
                return File(exportToExcel.Contents, DOCType.XlsxContentType, exportToExcel.FileName);
            }
        }

        [HttpPost]
        [Route("Admin/Update_R_List")]
        [Route("Update_R_List")]
        public IActionResult UpdateFOSRetailer([FromBody]UserRequest req)
        {
            IFOSML _IFOSML = new FOSML(_accessor, _env);
            var res = _IFOSML.AssignRetailerToFOS(req);
            return Json(res);
        }

        [HttpGet]
        [Route("Home/FOSUserList")]
        [Route("FOSUserList")]
        public IActionResult UserList()
         {
            IUserML userML = new UserML(_accessor, _env);
            IFOSML _IFOSML = new FOSML(_accessor, _env);
            var Roles = _IFOSML.GetRole().Roles.Where(w => w.ID == FixedRole.FOS);
            var userListModel = new UserListModel
            {
                IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser,
                selectListItems = new SelectList(Roles, "ID", "Role"),
                userBalnace = userML.GetUserBalnace(0)
            };
            return View("UserList/FOSUserList", userListModel);
        }
        [HttpPost]
        [Route("Admin/FOS_U_List")]
        [Route("FOS_U_List")]
        public IActionResult FOSUList([FromBody]CommonFilter f)
        {
            f.RoleID = 0;
            IUserML userML = new UserML(_accessor, _env);
            IFOSML _IFOSML = new FOSML(_accessor, _env);
            var res = _IFOSML.GetListFOS(f);
            var userListModel = new UserListModel
            {
                IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser,
                userList = res,
                userBalnace = userML.GetUserBalnace(0)
            };
            return PartialView("UserList/Partial/_FOSUserList", userListModel);
        }
        [HttpPost]
        [Route("Admin/FOS_U_List_Child")]
        [Route("FOS_U_List_Child")]
        public IActionResult UserListChild(int ID, bool IsUp)
        {
            IUserML userML = new UserML(_accessor, _env);
            var res = userML.GetListChild(ID, IsUp);
            res.userReports = res.userReports.Where(w => w.RoleID != FixedRole.FOS).ToList();
            var userListModel = new UserListModel
            {
                IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser,
                userList = res,
                userBalnace = userML.GetUserBalnace(0)
            };
            return PartialView("UserList/Partial/_FOSUserList", userListModel);
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (loginML.IsInValidSession() && (context.RouteData.Values["Action"].ToString() != "Index" || context.RouteData.Values["Controller"].ToString() != "FOS"))
            {
                context.Result = new RedirectResult("~/");
            }
            else
            {
                base.OnActionExecuting(context);
            }
        }
    }
}