using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using RoundpayFinTech.AppCode;
using RoundpayFinTech.AppCode.HelperClass;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Report;
using RoundpayFinTech.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.Controllers
{
    public partial class AdminController
    {
        [HttpGet]

        [Route("ReferSetting")]
        public IActionResult ReferSetting()
        {
            return View();
        }


        [HttpGet]
        [Route("ReferSettingData1")]
        public IActionResult Master_topup_Commission()
        {
           IReferSettingML ml = new ReferSettingML(_accessor, _env);
            MasterTopupCommissionViewModel response = new MasterTopupCommissionViewModel
            {
                Master_Topup_Commission = (List<Master_Topup_Commission>)ml.GetMasterTopupCommission(),
                ReferralCommission = ml.ReferralCommissions().FirstOrDefault()
            };
            return PartialView("Partial/_MastertopupCommission", response ?? new MasterTopupCommissionViewModel());
        }
        [HttpGet]
        [Route("ReferSettingData2")]
        public IActionResult Master_Role()
        {

            IReferSettingML ml = new ReferSettingML(_accessor, _env);


            return PartialView("Partial/_MasterRole", ml.GetMasterRole());
        }


        [HttpPost]
        [Route("MastertopupCommissionUpdate")]
        public IActionResult MastertopupCommissionUpdate(Master_Topup_Commission data)
        {
            IReferSettingML ml = new ReferSettingML(_accessor, _env);
            var resp = ml.UpdateMaster_Topup_Commission(data);
            return Json(resp);

            return Ok();
        }

        [HttpPost]
        [Route("UpdateMaster_Role")]
        public IActionResult UpdateMaster_Role(Master_Role data)
        {
            IReferSettingML ml = new ReferSettingML(_accessor, _env);
            var resp = ml.UpdateMaster_Role(data);
            return Json(resp);

            return Ok();
        }

        [HttpGet]
        [Route("ReferralCommissions")]
        public IActionResult ReferralCommissions()
        {
            IReferSettingML ml = new ReferSettingML(_accessor, _env);

            return PartialView("Partial/_ReferalCommissionSignup", ml.ReferralCommissions());
        }
        [HttpPost]
        [Route("ReferralCommissionUpdate")]
        public IActionResult ReferralCommissionUpdate(ReferralCommission data)
        {
            IReferSettingML ml = new ReferSettingML(_accessor, _env);
            var resp = ml.ReferralCommissionsUpdate(data);
            return Json(resp);
            return Ok();
        }

    }
}
