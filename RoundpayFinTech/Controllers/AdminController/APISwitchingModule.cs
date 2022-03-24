using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Fintech.AppCode;
using Fintech.AppCode.Configuration;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.Model.Reports;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        #region APISwitching
        [HttpGet]
        [Route("Home/APISwitching")]
        [Route("APISwitching")]
        public IActionResult APISwitching()
        {
            IOperatorML opml = new OperatorML(_accessor, _env);

            var _OpTypes = opml.GetOptypeInSlab();

            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.UserID > 1 && _lr.IsWLAPIAllowed)
            {
                _OpTypes = _OpTypes.Where(x => x.ID.In(OPTypes.AllowToWhitelabel));
            }
            var apiModel = new APIDetailModel
            {
                IsWLAPIAllowed = _lr.IsWLAPIAllowed,
                IsAdmin = _lr.RoleID == Role.Admin,
                OpTypes = _OpTypes
            };
            return View(apiModel);
        }

        [HttpPost]
        [Route("Home/API-Switching")]
        [Route("API-Switching")]
        public IActionResult _APISwitching(int OpTypeID)
        {
            ISwitchingML swML = new SwitchingML(_accessor, _env);
            var apiModel = new APIDetailModel
            {
                IsWLAPIAllowed = _lr.IsWLAPIAllowed,
                IsAdmin = _lr.RoleID == Role.Admin,
                SwitchedPAPIs = swML.GetAPISwitching(OpTypeID == 0 ? OPTypes.Prepaid : OpTypeID),
                OpTypeID = OpTypeID
            };
            return PartialView("Partial/_APISwitching", apiModel);
        }

        [HttpPost]
        [Route("Home/switch-api")]
        [Route("switch-api")]
        public IActionResult SwitchAPI([FromBody] APISwitched aPISwitched)
        {
            ISwitchingML sml = new SwitchingML(_accessor, _env);
            IResponseStatus _res = sml.SwitchAPI(aPISwitched);
            return Json(_res);
        }



        [HttpGet]
        [Route("Home/UserwiseAPISwitch")]
        [Route("UserwiseAPISwitch")]
        public IActionResult UserwiseAPISwitch()
        {
            var switchingViewModel = new SwitchingViewModel();
            IOperatorML opml = new OperatorML(_accessor, _env);
            switchingViewModel.opTypes = opml.GetOptypeInSlab();
            ILoginML ml = new LoginML(_accessor, _env);
            WebsiteInfo _winfo = ml.GetWebsiteInfo();
            switchingViewModel.IsMultipleMobileAllowed = _winfo.IsMultipleMobileAllowed;
            return View(switchingViewModel);
        }

        [HttpPost]
        [Route("Home/User-API-Switch")]
        [Route("User-API-Switch")]
        public IActionResult _UserAPISwitch(int UserID, int OpTypeID)
        {
            ISwitchingML sml = new SwitchingML(_accessor, _env);
            IEnumerable<APIOpCode> list = sml.GetAPISwitchByUser(UserID, OpTypeID);
            ViewBag.UserID = UserID;
            return PartialView("Partial/_UserwiseAPISwitch", list);
        }
        [HttpPost]
        [Route("Home/user-switch-api")]
        [Route("user-switch-api")]
        public IActionResult UserSwitchAPI([FromBody] SwitchAPIUser switchAPI)
        {
            ISwitchingML swml = new SwitchingML(_accessor, _env);
            IResponseStatus _res = swml.SwitchUserwiseAPI(switchAPI);
            return Json(_res);
        }
        [HttpGet]
        [Route("Home/USwitchDetail")]
        [Route("USwitchDetail")]
        public IActionResult UserSwitchDetail()
        {
            IUserML userML = new UserML(_lr);
            if (_lr.LoginTypeID == LoginType.ApplicationUser && !userML.IsEndUser() || userML.IsCustomerCareAuthorised(ActionCodes.APISwitch))
            {
                var switchingViewModel = new SwitchingViewModel();
                IOperatorML opml = new OperatorML(_accessor, _env);
                switchingViewModel.opTypes = opml.GetOptypeInSlab();
                return View(switchingViewModel);
            }
            return Ok();
        }
        [HttpPost]
        [Route("Home/u-switch-detail")]
        [Route("u-switch-detail")]
        public IActionResult _UserSwitchDetail(string m, int OpTypeID)
        {
            IUserML userML = new UserML(_lr);
            if (_lr.LoginTypeID == LoginType.ApplicationUser && !userML.IsEndUser() || userML.IsCustomerCareAuthorised(ActionCodes.APISwitch))
            {
                ISwitchingML sml = new SwitchingML(_accessor, _env);
                var _res = sml.Userswitches(m, OpTypeID);
                return PartialView("Partial/_UserSwitchDetail", _res);
            }
            return Ok();
        }
        [HttpGet]
        [Route("Home/CircleSwitching")]
        [Route("CircleSwitching")]
        public IActionResult CircleSwitching()
        {
            IAPIML apiML = new APIML(_accessor, _env);
            var APIList = apiML.GetAPIDetail();
            APIList = APIList.Where(x => x.InSwitch).ToList();
            return View(APIList);
        }


        [HttpPost]
        [Route("Home/csw")]
        [Route("csw")]
        public IActionResult CircleSwitchingUpdate([FromBody] Circle circle)
        {
            ISwitchingML sML = new SwitchingML(_accessor, _env);
            IResponseStatus _res = sML.UpdateCircleSwitch(circle);
            return Json(_res);
        }
        [HttpPost]
        [Route("Home/circle-switched")]
        [Route("circle-switched")]
        public IActionResult _CircleSwitching(int ID)
        {
            ISwitchingML sML = new SwitchingML(_accessor, _env);
            IEnumerable<CircleSwitch> list = sML.GetCircleSwitches(ID);
            return PartialView("Partial/_CircleSwitching", list);
        }

        [HttpGet]
        [Route("Home/CircleSwitchingMultiAPI")]
        [Route("CircleSwitchingMultiAPI")]
        public async Task<IActionResult> CircleSwitchingMultiAPI()
        {
            IOperatorAppML opML = new OperatorML(_accessor, _env);
            return View(await opML.CircleList().ConfigureAwait(false));
        }
        [HttpPost]
        [Route("Home/circle-switched-multi")]
        [Route("circle-switched-multi")]
        public IActionResult _CircleSwitchingMulti(int ID)
        {
            ISwitchingML sML = new SwitchingML(_accessor, _env);
            List<CircleAPISwitchDetail> list = sML.GetCircleMultiSwitchedDetail(ID);
            return PartialView("Partial/_CircleSwitchingMultiAPI", list);
        }
        [HttpGet]
        [Route("Home/CircleBlocked")]
        [Route("CircleBlocked")]
        public IActionResult CircleBlocked()
        {
            IAPIML apiML = new APIML(_accessor, _env);
            var APIList = apiML.GetAPIDetail();
            APIList = APIList.Where(x => x.InSwitch).ToList();
            return View(APIList);
        }
        [HttpPost]
        [Route("Home/csb")]
        [Route("csb")]
        public IActionResult CircleBlockUpdate([FromBody] Circle circle)
        {
            ISwitchingML sML = new SwitchingML(_accessor, _env);
            IResponseStatus _res = sML.UpdateCircleBlock(circle);
            return Json(_res);
        }

        [HttpPost]
        [Route("Admin/circle-blocked")]
        [Route("circle-blocked")]
        public IActionResult _CircleBlocked()
        {
            ISwitchingML sML = new SwitchingML(_accessor, _env);
            IEnumerable<CircleSwitch> list = sML.GetCircleBlocked();
            return PartialView("Partial/_CircleBlocked", list);
        }

        [HttpGet]
        [Route("Admin/DenominationAPISwitching")]
        [Route("DenominationAPISwitching")]
        public IActionResult DenominationAPISwitching()
        {
            var switchingViewModel = new SwitchingViewModel();
            IOperatorML opml = new OperatorML(_accessor, _env);
            switchingViewModel.opTypes = opml.GetOptypeInSlab();
            return View(switchingViewModel);
        }

        [HttpPost]
        [Route("Admin/ADAPI-Switch")]
        [Route("ADAPI-Switch")]
        public IActionResult _DenominationAPISwitching(int OpTypeID)
        {
            IOperatorML opml = new OperatorML(_accessor, _env);
            IAPIML aPIML = new APIML(_accessor, _env);
            var list = new DenomAPISwitch
            {
                Operators = opml.GetOperators().Where(x => x.CommSettingType == CommStttingType.Traditional && x.OpType == OpTypeID).ToList(),
                APIList = aPIML.GetAPIDetail().Where(x => x.InSwitch).ToList()
            };
            return PartialView("Partial/_DenominationAPISwitching", list);
        }

        [HttpPost]
        [Route("Admin/ADAPI-Denomination")]
        [Route("ADAPI-Denomination")]
        public IActionResult _showDenomination(APIDenominationReq req)
        {
            IOperatorAppML ml = new OperatorML(_accessor, _env);
            req.Cirlces = ml.CircleListWithAll().Result;
            return PartialView("Partial/_showDenomination", req);
        }
        [HttpPost]
        [Route("Admin/ADAPI-Denomination-Body")]
        [Route("ADAPI-Denomination-Body")]
        public IActionResult _showDenominationBody(APIDenominationReq req)
        {
            IOperatorML opml = new OperatorML(_accessor, _env);
            var res = opml.GetApiDenom(req);
            res.OID = req.OID;
            res.APIId = req.APIId;
            res.OPName = req.OPName;
            res.APIName = req.APIName;
            return PartialView("Partial/_showDenominationBody", res);
        }
        [HttpPost]
        [Route("Admin/API-Denomination")]
        [Route("API-Denom-updation")]
        public IActionResult _updateDenomination(APIDenominationReq req)
        {
            ISwitchingML sML = new SwitchingML(_accessor, _env);
            var _res = sML.UpdateDenomination(req);
            return Json(_res);
        }
        [HttpGet]
        [Route("Report/D-Switch-Report")]
        [Route("D-Switch-Report")]
        public IActionResult DenominationSwitch()
        {
            var switchingViewModel = new SwitchingViewModel();
            IOperatorML opml = new OperatorML(_accessor, _env);
            switchingViewModel.opTypes = opml.GetOptypeInSlab();
            return View(switchingViewModel);
        }

        [HttpPost]
        [Route("Report/De-Sw-Report")]
        [Route("De-Sw-Report")]
        public IActionResult _DenominationSwitch(int OpTypeID)
        {
            ISwitchingML RML = new SwitchingML(_accessor, _env);
            var Res = RML.GetDSwitchReport(OpTypeID);
            return PartialView("Partial/_DenominationSwitch", Res);
        }

        [HttpPost]
        [Route("Home/block-AD")]
        [Route("block-AD")]
        public IActionResult BlockAmountDenomination([FromBody] APISwitched aPISwitched)
        {
            IOperatorML oML = new OperatorML(_accessor, _env);
            var _res = oML.UpdateBlockDenomination(aPISwitched);
            return Json(_res);
        }

        [HttpPost]
        [Route("Home/update-down-status")]
        [Route("update-down-status")]
        public IActionResult UpdateDownStatus([FromBody] UpdateDownStatusReq updateDownStatusReq)
        {
            ISwitchingML switchingML = new SwitchingML(_accessor, _env);
            var _res = switchingML.UpdateOperatorStatus(updateDownStatusReq);
            return Json(_res);
        }
        [Route("Home/change-backup-api")]
        [Route("change-backup-api")]
        public IActionResult ChangeBackupAPI([FromBody] APISwitched aPISwitched)
        {
            ISwitchingML sml = new SwitchingML(_accessor, _env);
            IResponseStatus _res = sml.SetAPI(aPISwitched);
            return Json(_res);
        }

        [HttpPost]
        public async Task<IActionResult> APIs(int opTypeId)
        {
            IAPIML api = new APIML(_accessor, _env);
            var APIs = new APIDetailViewModel
            {
                OpTypeId = opTypeId,
                APIs = await api.GetAllAPI(opTypeId).ConfigureAwait(true)
            };
            return PartialView("Partial/_APIs", APIs);
        }

        [HttpPost]
        public async Task<IActionResult> OpTypeWiseAPISwitch(OpTypeWiseAPISwitchingReq req)
        {
            req.LoginID = _lr.UserID;
            req.LT = _lr.LoginTypeID;
            IAPIML api = new APIML(_accessor, _env);
            var response = await api.UpdateOpTypeWiseAPISwitch(req).ConfigureAwait(true);
            return Json(response);
        }
        #endregion
    }
}
