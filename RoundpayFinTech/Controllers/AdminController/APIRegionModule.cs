using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fintech.AppCode.Configuration;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RoundpayFinTech.AppCode;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.Models;

namespace RoundpayFinTech.Controllers
{
    public partial class AdminController
    {
        #region APIRegion
        [HttpGet("Recharge-API")]
        [Route("Home/Recharge-API")]
        public IActionResult RechargeAPI()
        {
            return View();
        }
        [HttpPost]
        [Route("Home/Recharge-API/{id}")]
        [Route("Recharge-API/{id}")]
        public IActionResult RechargeAPI(int id)
        {
            var ML = new APIML(_accessor, _env);
            var lst = ML.GetGroup();
            var userML = new UserML(_accessor, _env);
            var aPIDetailModel = new APIDetailModel
            {
                aPIDetail = new APIDetail(),
                selectLists = new SelectList(ML.GetGroup(), "GroupID", "GroupName"),
                IsWLAPIAllowed = _lr.IsWLAPIAllowed,
                IsAdmin = _lr.RoleID == Role.Admin
            };

            if (id > 0 && !userML.IsEndUser() && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                aPIDetailModel.aPIDetail = ML.GetAPIDetailByID(id);
            }
            return PartialView("Partial/_RechargeAPI", aPIDetailModel);
        }
        [HttpPost]
        [Route("Home/API-Group/{id}")]
        [Route("API-Group/{id}")]
        public IActionResult APIGroup(int id)
        {
            var ML = new APIML(_accessor, _env);
            var lst = ML.GetGroup(id);
            return Json(lst);
        }
        [HttpPost]
        [Route("Home/RechargeAPIs")]
        [Route("RechargeAPIs")]
        public IActionResult RechargeAPIList()
        {
            IUserML userML = new UserML(_lr);
            if (_lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.AddEditAPI))
                using (var ML = new APIML(_accessor, _env))
                {
                    var aPIDetailModel = new APIDetailModel
                    {
                        APIs = ML.GetAPIDetail(),
                        IsWLAPIAllowed = _lr.IsWLAPIAllowed
                    };
                    return PartialView("Partial/_RechargeAPIs", aPIDetailModel);
                }
            return Ok();
        }
        [HttpPost]
        [Route("Home/APIForBalance")]
        [Route("APIForBalance")]
        public IActionResult APIListForBalance()
        {
            IUserML userML = new UserML(_lr);
            if (_lr.UserID == 1 && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.AddEditAPI))
            {
                IAPIML ML = new APIML(_accessor, _env);
                return PartialView("Partial/_RechargeAPIBalance", ML.GetAPIDetailForBalance());
            }
            return Ok();
        }
        [HttpPost]
        [Route("Home/_APIForBalance")]
        [Route("_APIForBalance")]
        public async Task<IActionResult> _APIListForBalance(int id)
        {
            IUserML userML = new UserML(_lr);
            if (_lr.UserID == 1 && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.AddEditAPI))
            {
                IAPIML ML = new APIML(_accessor, _env);
                var res = await ML.GetBalanceFromAPI(id).ConfigureAwait(false);
                var SB = new StringBuilder();
                SB.AppendFormat(@"<div class=""row"">
    <div class=""col-md-12"">
        <button type=""button"" class=""close"" aria-label=""Close"">
            <span aria-hidden=""true"">×</span>
        </button>
        <h6>Balance Detail</h6>
        <hr>
    </div>
    <div class=""container-fluid"">
        <div class=""form-horizontal bv-form"">
            <div class=""form-group row gutter has-feedback"">
                <div class=""col-md-6"">
                    <span>Start At</span>
                    <span id=""lbltranid"" class=""text-monospace"">{0}</span>
                </div>
                <div class=""col-md-6"">
                    <span>End At</span>
                    <span id=""lbltranid"" class=""text-monospace"">{1}</span>
                </div>
            </div>
            <hr class=""mt-0 mb-0"">
			<div class=""form-group row"">
				<div class=""col-lg-12"">
					<span> Request </span>
					<textarea disabled=""disabled"" class=""form-control"" style=""border-style:None;"">{2}</textarea>
				</div>
			</div>
			<div class=""form-group row"">
				<div class=""col-lg-12"">
					<span> Response </span>
					<textarea disabled=""disabled"" class=""form-control"" style=""border-style:None;"">{3}</textarea>
				</div>
			</div>
            </div>
        </div>
        <div class=""float-right ml-3"">
            <button class=""btn btn-outline-danger btn-sm"" id=""mdlCancel"">Cancel</button>
        </div>

</div>", res.StartAt, res.EndAt, res.Request, res.Response);
                res.Template = SB.ToString();
                return Json(res);
            }

            return Ok();
        }
        [HttpPost]
        [Route("Home/Recharge-API")]
        [Route("Recharge-API")]
        public IActionResult RechargeAPI([FromBody] APIDetail aPIDetail)
        {
            IResponseStatus _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Incomplete request!"
            };
            if (_lr.LoginTypeID.In(LoginType.ApplicationUser, LoginType.CustomerCare))
            {
                if (aPIDetail != null)
                {
                    IAPIML aPIML = new APIML(_accessor, _env);
                    _res = aPIML.SaveAPI(aPIDetail);
                }
            }
            return Json(_res);
        }
        [HttpPost]
        [Route("Home/api-com")]
        [Route("api-com")]
        public IActionResult ApiCommFilter(int APIID)
        {
            IAPIML aPIML = new APIML(_accessor, _env);
            var selectAPIList = new SelectAPIList
            {
                APIID = APIID,
                selectList = new SelectList(aPIML.GetAPIDetail(), "ID", "Name")
            };
            return PartialView("Partial/_APICommissionDetail", selectAPIList);
        }
        [HttpPost]
        [Route("Home/api-comm")]
        [Route("api-comm")]
        public IActionResult ApiCommissions(int APIID)
        {
            IAPIML aPIML = new APIML(_accessor, _env);
            var apiModel = new APIDetailModel
            {
                SlabComs = aPIML.GetAPICommission(APIID),
                IsWLAPIAllowed = _lr.IsWLAPIAllowed
            };
            return PartialView("Partial/_APICommList", apiModel);
        }
        [HttpPost]
        [Route("Home/api-comm-update")]
        [Route("api-comm-update")]
        public IActionResult APICommUpdate([FromBody] SlabCommission apiCommission)
        {
            ISlabML slabML = new SlabML(_accessor, _env);
            IResponseStatus _resp = slabML.UpdateAPICommission(apiCommission);
            return Json(_resp);
        }
        [HttpPost]
        [Route("Home/API-Range-Slab-Detail")]
        [Route("API-Range-Slab-Detail")]
        public IActionResult APIRangeSlabDetail(int APIID, int OTypeID = 0)
        {
            IUserML userML = new UserML(_accessor, _env);
            IAPIML apiML = new APIML(_accessor, _env);
            RangeDetailModel slabDetailModel = apiML.GetAPICommissionRange(APIID, OTypeID);
            slabDetailModel.OpTypeID = OTypeID;
            slabDetailModel.IsAdmin = true;
            IOperatorML opml = new OperatorML(_accessor, _env);
            slabDetailModel.OpTypes = opml.GetOptypeInRange().ToList();
            var DMRModels = userML.GetDMRModelList();
            if (DMRModels.Any())
            {
                slabDetailModel.DMRModelSelect = new SelectList(DMRModels, "ID", "Name");
            }
            if (slabDetailModel.IsAdmin)
            {
                return PartialView("Partial/_RangeAPICommission", slabDetailModel);
            }
            return Ok();
        }
        [HttpPost]
        [Route("Home/RangeAPICommSave")]
        [Route("RangeAPICommSave")]
        public IActionResult RangeAPICommSave([FromBody] RangeCommission slabCommission)
        {
            ISlabML slabML = new SlabML(_accessor, _env);
            //UpdateRangeAPIComm
            IResponseStatus _resp = slabML.UpdateRangeAPIComm(slabCommission);

            return Json(_resp);
        }
        [HttpPost]
        [Route("/api-sw-sts")]
        public IActionResult ChangeAPISwSts(int ID)
        {
            IUserML userML = new UserML(_accessor, _env);
            var res = userML.ChangeAPISwSts(ID);
            return Json(res);
        }
        [HttpPost]
        [Route("open-apioptype")]
        public IActionResult OpenAPIOpTypeInteface(string APICode)
        {
            IOperatorML opML = new OperatorML(_accessor, _env);
            var res = opML.GetAPIOpType(APICode);
            return PartialView("Partial/_UpdateAPIOpTypeInterface", res);
        }
        [HttpPost]
        [Route("update-apioptype")]
        public IActionResult UpdateAPIOpType(int OpTypeID, string APICode)
        {
            IOperatorML opML = new OperatorML(_accessor, _env);
            var res = opML.UpdateAxisBankBillerList(OpTypeID, APICode);
            return Json(res);
        }
        #region SMSAPIRegion
        [HttpGet("SMSAPI")]
        [Route("Home/SMSAPI")]
        public IActionResult SMSAPI()
        {
            return View();
        }
        [HttpPost]
        [Route("Home/SMSAPI/{id}")]
        [Route("SMSAPI/{id}")]
        public IActionResult SMSAPI(int id)
        {
            var model = new SMSAPIDetail();
            ISMSAPIML ML = new APIML(_accessor, _env);
            model = ML.GetSMSAPIDetailByID(id);
            return PartialView("Partial/_SMSAPI", model);
        }
        [HttpPost]
        [Route("Home/SMSAPIs")]
        [Route("SMSAPIs")]
        public IActionResult SMSAPIList()
        {
            ISMSAPIML ML = new APIML(_accessor, _env);
            return PartialView("Partial/_SMSAPIs", ML.GetSMSAPIDetail());
        }
        [HttpPost]
        [Route("Home/SMS-API")]
        [Route("SMS-API")]
        public IActionResult SMSAPI([FromBody] APIDetail aPIDetail)
        {
            IResponseStatus _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Incomplete request!"
            };
            if (_lr.LoginTypeID.In(LoginType.ApplicationUser, LoginType.CustomerCare))
            {
                if (aPIDetail != null)
                {
                    ISMSAPIML aPIML = new APIML(_accessor, _env);
                    _res = aPIML.SaveSMSAPI(aPIDetail);
                }
            }
            return Json(_res);
        }
        #endregion
        #endregion
    }
}
