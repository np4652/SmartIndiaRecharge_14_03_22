using System;
using System.Linq;
using Fintech.AppCode;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Mvc;
using RoundpayFinTech.AppCode.Configuration;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.Models;
using Validators;

namespace RoundpayFinTech.Controllers
{
    public partial class AdminController
    {
        #region Denomination
        [HttpGet]
        [Route("Home/Denomination-master")]
        [Route("Denomination-master")]
        public IActionResult DenominationMaster()
        {
            var AdminOrAPIUser = _lr.LoginTypeID == LoginType.ApplicationUser ? (_lr.RoleID == Role.Admin ? 1 : 0) : (_lr.RoleID == Role.APIUser ? 2 : 0);
            return View(AdminOrAPIUser);
        }

        [HttpPost]
        [Route("Home/denomination-master")]
        [Route("denomination-master")]
        public IActionResult _DenominationMaster()
        {
            IOperatorML IOML = new OperatorML(_accessor, _env);
            var list = IOML.GetDenomination();
            ViewBag.AdminOrAPIUser = _lr.LoginTypeID == LoginType.ApplicationUser ? (_lr.RoleID == Role.Admin ? 1 : 0) : (_lr.RoleID == Role.APIUser ? 2 : 0);
            return PartialView("Partial/_DenominationMaster", list);
        }


        [HttpPost]
        [Route("Home/Denomination-Edit/{id}")]
        [Route("Denomination-Edit/{id}")]
        public IActionResult _DenominationEdit(int ID)
        {
            IOperatorML IOML = new OperatorML(_accessor, _env);
            var denomiDetail = IOML.GetDenomination(ID);
            return PartialView("Partial/_DenominationEdit", denomiDetail);
        }

        [HttpPost]
        [Route("Home/Denomination-Edit")]
        [Route("Denomination-Edit")]
        public IActionResult _DenominationEdit([FromBody] DenominationModal denomDetail)
        {
            IOperatorML IOML = new OperatorML(_accessor, _env);
            IResponseStatus _resp = IOML.SaveDenom(denomDetail);
            return Json(_resp);
        }
        [HttpGet]
        [Route("Admin/DenominationRange-Master")]
        [Route("DenominationRange-Master")]
        public IActionResult DenominationRangeMaster()
        {
            var AdminOrAPIUser = _lr.LoginTypeID == LoginType.ApplicationUser ? (_lr.RoleID == Role.Admin ? 1 : 0) : (_lr.RoleID == Role.APIUser ? 2 : 0);
            return View(AdminOrAPIUser);
        }

        [HttpPost]
        [Route("Home/DenominationRange-Master")]
        [Route("DenominationRange-Master")]
        public IActionResult _DenominationRangeMaster()
        {
            IOperatorML IOML = new OperatorML(_accessor, _env);
            var list = IOML.GetDenominationRange();
            list.AdminOrAPIUser = _lr.LoginTypeID == LoginType.ApplicationUser ? (_lr.RoleID == Role.Admin ? 1 : 0) : (_lr.RoleID == Role.APIUser ? 2 : 0);
            return PartialView("Partial/_DenominationRangeMaster", list);
        }

        [HttpPost]
        [Route("Admin/DenominationRange-Edit/{id}")]
        [Route("DenominationRange-Edit/{id}")]
        public IActionResult _DenominationRangeEdit(int ID)
        {
            IOperatorML IOML = new OperatorML(_accessor, _env);
            var denomRange = IOML.GetDenominationRange(ID);
            return PartialView("Partial/_DenominationRangeEdit", denomRange);
        }

        [HttpPost]
        [Route("Home/DenominationRange-Edit")]
        [Route("DenominationRange-Edit")]
        public IActionResult _DenominationRangeEdit([FromBody] DenominationRange denomRange)
        {
            IOperatorML IOML = new OperatorML(_accessor, _env);
            IResponseStatus _resp = IOML.SaveDenomRange(denomRange);
            return Json(_resp);
        }
        [HttpPost]
        [Route("Home/Denomination-Slab-Detail")]
        [Route("Denomination-Slab-Detail")]
        public IActionResult _DenominationSlabDetail(int SlabID, bool IsAdminDefined, int OpTypeID)
        {
            IOperatorML opml = new OperatorML(_accessor, _env);
            var opList = opml.GetOperators().Where(x => x.OpType == OpTypeID);
            var denomList = opml.GetDenomination();
            var denomRangeList = opml.GetDenominationRange().Detail;
            var Model = new DenominationDetailModal
            {
                SlabID = SlabID,
                IsAdminDefined = IsAdminDefined,
                DenomList = denomList.ToList(),
                DenomRangeList = denomRangeList,
                Operator = opList.ToList(),
                OpTypes = opml.GetOptypeInSlab(),
                OpTypeID = OpTypeID
            };
            return PartialView("Partial/_DenominationSlabDetail", Model);
        }


        [HttpPost]
        [Route("Home/Denomination-Slab-Edit/{id}")]
        [Route("Denomination-Slab-Edit/{id}")]
        public IActionResult _DenominationSlabEdit(int ID)
        {
            IOperatorML IOML = new OperatorML(_accessor, _env);
            var denomiDetail = IOML.GetDenomination(ID);
            return PartialView("Partial/_DenominationEdit", denomiDetail);
        }


        [HttpPost]
        [Route("Home/Denomination-Slab-Edit")]
        [Route("Denomination-Slab-Edit")]
        public IActionResult _DenominationSlabEdit([FromBody] DenominationModal denomDetail)
        {
            IOperatorML IOML = new OperatorML(_accessor, _env);
            IResponseStatus _resp = IOML.SaveDenom(denomDetail);
            return Json(_resp);
        }

        [HttpPost]
        [Route("DenomSlabDetailRole")]
        public IActionResult _DenomSlabDetailRole(DenomDetailByRole param)
        {
            IOperatorML IOML = new OperatorML(_accessor, _env);
            var result = IOML.GetDenomDetailByRole(param);
            ViewBag.IsAdminDefined = param.IsAdminDefined;
            return PartialView("Partial/_DenominationSlabDetailRole", result);
        }

        [HttpPost]
        [Route("Home/Denom-Incentive-Edit")]
        [Route("Denom-Incentive-Edit")]
        public IActionResult _DenomIncentiveEdit(DenomDetailByRole param)
        {
            IOperatorML IOML = new OperatorML(_accessor, _env);
            IResponseStatus _resp = IOML.SaveDenomDetailByRole(param);
            return Json(_resp);
        }
        #endregion

        #region UserDenominationSwitchSection
        //For users
        [HttpGet("Admin/DenominationAPISwitchingU")]
        [HttpGet("DenominationAPISwitchingU")]
        public IActionResult DenominationAPISwitchingU()
        {
            if (ApplicationSetting.IsUserwiseDenominationSwitch)
            {
                var switchingViewModel = new SwitchingViewModel();
                IOperatorML opml = new OperatorML(_accessor, _env);
                switchingViewModel.opTypes = opml.GetOptypeInSlab();
                ILoginML ml = new LoginML(_accessor, _env);
                WebsiteInfo _winfo = ml.GetWebsiteInfo();
                switchingViewModel.IsMultipleMobileAllowed = _winfo.IsMultipleMobileAllowed;
                return View(switchingViewModel);
            }
            return Ok();
        }
        [HttpPost]
        [Route("Admin/ADAPI-Denomination-Body-U")]
        [Route("ADAPI-Denomination-Body-U")]
        public IActionResult ShowDenominationBodyU(APIDenominationReq req)
        {
            if (ApplicationSetting.IsUserwiseDenominationSwitch)
            {
                IOperatorML opml = new OperatorML(_accessor, _env);
                var res = opml.GetApiDenomUser(req);
                res.OID = req.OID;
                res.APIId = req.APIId;
                res.OPName = req.OPName;
                res.APIName = req.APIName;
                return PartialView("Partial/_showDenominationBodyU", res);
            }
            return Ok();
        }
        [HttpPost]
        [Route("Admin/ADAPI-Switch-U")]
        [Route("ADAPI-Switch-U")]
        public IActionResult _DenominationAPISwitchingU(int OpTypeID)
        {
            if (ApplicationSetting.IsUserwiseDenominationSwitch)
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
            return Ok();
        }
        [HttpPost]
        [Route("Admin/ADAPI-Denomination-U")]
        [Route("ADAPI-Denomination-U")]
        public IActionResult ShowDenominationU(APIDenominationReq req)
        {
            if (ApplicationSetting.IsUserwiseDenominationSwitch)
            {
                IOperatorAppML ml = new OperatorML(_accessor, _env);
                req.Cirlces = ml.CircleListWithAll().Result;
                return PartialView("Partial/_showDenominationU", req);
            }
            return Ok();
        }

        [HttpPost]
        [Route("Admin/API-Denom-updation-U")]
        [Route("API-Denom-updation-U")]
        public IActionResult UpdateDenominationUser(APIDenominationReq req)
        {
            if (ApplicationSetting.IsUserwiseDenominationSwitch)
            {
                ISwitchingML sML = new SwitchingML(_accessor, _env);
                var _res = sML.UpdateDenominationUser(req);
                return Json(_res);
            }
            return Ok();
        }

        [HttpGet]
        [Route("Report/D-Switch-Report-U")]
        [Route("D-Switch-Report-U")]
        public IActionResult DenominationSwitchU()
        {
            if (ApplicationSetting.IsUserwiseDenominationSwitch)
            {
                var switchingViewModel = new SwitchingViewModel();
                IOperatorML opml = new OperatorML(_accessor, _env);
                switchingViewModel.opTypes = opml.GetOptypeInSlab();
                return View(switchingViewModel);
            }
            return Ok();
        }

        [HttpPost]
        [Route("Report/De-Sw-Report-U")]
        [Route("De-Sw-Report-U")]
        public IActionResult _DenominationSwitchU(int OpTypeID, string MobileNo)
        {
            if (ApplicationSetting.IsUserwiseDenominationSwitch)
            {
                ISwitchingML RML = new SwitchingML(_accessor, _env);
                var UserID = 0;
                if (!string.IsNullOrEmpty(MobileNo))
                {
                    if (!Validate.O.IsNumeric(MobileNo))
                    {
                        UserID = MobileNo.Length > 4 ? Convert.ToInt32(MobileNo.Substring(2, MobileNo.Length)) : 0;
                    }
                }
                var Res = RML.GetDSwitchReportUser(OpTypeID, UserID, MobileNo);
                return PartialView("Partial/_DenominationSwitchU", Res);
            }
            return Ok();
        }


        [HttpPost]
        [Route("Report/De-Sw-Report-U-Remove")]
        [Route("De-Sw-Report-U-Remove")]
        public IActionResult DenominationSwitchRemoveU(int ID)
        {
            if (ApplicationSetting.IsUserwiseDenominationSwitch)
            {
                ISwitchingML sML = new SwitchingML(_accessor, _env);
                var _res = sML.RemoveDenominationUser(ID);
                return Json(_res);
            }
            return Ok();
        }

        [HttpPost]
        [Route("Report/De-Sw-Report-Remove")]
        [Route("De-Sw-Report-Remove")]
        public IActionResult DenominationSwitchRemove(int ID)
        {
            if (ApplicationSetting.IsUserwiseDenominationSwitch)
            {
                ISwitchingML sML = new SwitchingML(_accessor, _env);
                var _res = sML.RemoveDenomination(ID);
                return Json(_res);
            }
            return Ok();
        }

        #endregion
    }
}
