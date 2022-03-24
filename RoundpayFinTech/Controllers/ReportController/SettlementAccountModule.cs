using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Mvc;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.Controllers
{
    public partial class ReportController
    {
        #region Bank update Request Section
        [HttpGet]
        [Route("BankDetailsUpdateRequest")]
        public IActionResult UpdateUserPartialRequest()
        {
            ISettlementaccountML req = new SettlementaccountML(_accessor, _env);
            var res = req.GetApproved_VeriyfiedStatus();

            return View(res);
        }
        [HttpPost]
        [Route("_UpdateUserPartialRequest")]
        public IActionResult _UpdateUserPartialRequest([FromBody] SattlementAccountModels data)
        {
            IReportML rml = new ReportML(_accessor, _env);
            IEnumerable<SattlementAccountModels> list = (List<SattlementAccountModels>)rml.GetUserBankUpdateRequest1(data);
            return PartialView("PartialView/_UpdateUserPartialRequest", list);
        }

        [HttpPost]
        [Route("Home/R-A-UserpartialUpdate")]
        [Route("R-A-UserpartialUpdate")]
        public async Task<IActionResult> _RAUserpartialUpdate(char Status, int RequestID, string Name)
        {
            IResponseStatus _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidParam

            };
            GetEditUser RequestData = new GetEditUser
            {
                RequestID = RequestID,
                Name = Name,

            };
            if (Status == 'A' || Status == 'a')
            {
                RequestData.RequestStatus = "Approved";
            }
            if (Status == 'R' || Status == 'r')
            {
                RequestData.RequestStatus = "Rejected";
            }

            IReportML rmL = new ReportML(_accessor, _env);
            _res = await rmL.AcceptOrRejectBankupdateRequest(RequestData);
            return Json(_res);
        }

        #endregion
    }
}
