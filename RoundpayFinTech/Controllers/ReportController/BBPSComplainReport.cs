using Fintech.AppCode.Configuration;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Mvc;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model.Recharge;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.Controllers
{
    public partial class ReportController: IReportBBPSComplain
    {
        #region Complaints
        [Route("/BBPSComplaints")]
        public IActionResult BBPSComplaint()
        {
            if (_lr.RoleID.In(Role.Retailor_Seller, Role.APIUser))
            {
                return View();
            }
            return Ok();
        }
        [HttpPost]
        [Route("/bbps-complaints-services")]
        public IActionResult BBPSComplaintServices()
        {
            IOperatorML ml = new OperatorML(_accessor, _env);
            var _res = ml.GetOperators();
            if (_res != null && _res.Any())
                _res = _res.Where(x => x.IsBBPS);

            return Json(_res);
        }
        [HttpPost]
        [Route("/bbps-complaints-outlets")]
        public IActionResult BBPSComplaintOutlets()
        {
            ReportML ml = new ReportML(_accessor, _env);
            var _res = ml.GetRecentOutletUserListByUserID().Result.Select(x => new { _ID = x._ID, _Company = x._Company });
            return Json(_res);
        }
        [HttpPost]
        [Route("Raise-bbps-complain")]
        public IActionResult RaiseComplain([FromBody] GenerateBBPSComplainProcReq req)
        {
            var reportML = new ReportML(_accessor, _env);
            return Json(reportML.RaiseBBPSComplain(req));
        }
        [HttpPost]
        [Route("/bbps-complaints-list")]
        public IActionResult BBPSComplaintList()
        {
            var reportML = new ReportML(_accessor, _env);
            var res = reportML.BBPSComplainReport();
            return PartialView("PartialView/_bbpsComplaintList", res);
        }

        [HttpPost]
        [Route("track-bbps-complain")]
        public IActionResult TrackComplain(int TableID)
        {
            var reportML = new ReportML(_accessor, _env);
            var res = reportML.TrackBBPSComplain(TableID);
            return PartialView("PartialView/_BBPSComplainStatusPopUP", res);
        }

        [HttpGet]
        [Route("BBPSComplainReport")]
        public IActionResult BBPSComplainReport()
        {
            return View();
        }

        [HttpPost]
        [Route("bbps-complain-report")]
        [Route("Report/bbps-complain-report")]
        public IActionResult _BBPSComplainReport()
        {
            var reportML = new ReportML(_accessor, _env);
            var res = reportML.BBPSComplainReport();
            return PartialView("PartialView/_BBPSComplainReport", res);
        }
        #endregion
    }
}
