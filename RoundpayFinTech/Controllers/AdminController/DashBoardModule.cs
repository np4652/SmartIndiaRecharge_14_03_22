using Fintech.AppCode.Model.Reports;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Mvc;
using RoundpayFinTech.AppCode.Model.ProcModel;

namespace RoundpayFinTech.Controllers
{
    public partial class AdminController
    {
        #region DashBoardSegment
        [HttpPost]
        [Route("TotalBalRoleWise")]
        public IActionResult TotalBalRoleWise()
        {
            var totalBalRoleWise = uow.reportML.TotalBalRoleWise();
            return PartialView("Partial/_RoleSummaryIndex", totalBalRoleWise);
        }

        [HttpPost]
        [Route("Dashboard")]
        public IActionResult Dashboard()
        {
            Dashboard dashboard = new Dashboard();
            if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser)
                dashboard = uow.userML.DashBoard();
            return Json(dashboard);
        }

        [HttpPost]
        [Route("Admin/FundCount")]
        [Route("FundCount")]
        public IActionResult FundCount()
        {
            int fundCount = 0;
            var userID = _lr.UserID;
            if (_lr.LoginTypeID == LoginType.CustomerCare)
            {
                if (uow.userML.IsCustomerCareAuthorised(ActionCodes.ShowFundRequest))
                {
                    userID = 1;
                }
                else
                {
                    userID = 0;
                }
            }
            if (userID > 0)
            {
                fundCount = uow.reportML.FundCount(userID);
            }
            return Json(fundCount);
        }

        [HttpPost]
        [Route("Admin/AdminAccountSummary")]
        [Route("AdminAccountSummary")]
        public IActionResult AdminAccountSummary()
        {
            AccountSummary accountSummary = new AccountSummary();
            if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser)
                accountSummary = uow.reportML.AdminAccountSummary();
            return PartialView("Partial/_AccountSummartIndex", accountSummary);
        }

        [HttpPost]
        [Route("TotalPending")]
        public IActionResult TotalPending()
        {
            Dashboard_Chart dashboardChart = new Dashboard_Chart();
            if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser)
                dashboardChart = uow.reportML.TotalPending();
            return PartialView("Partial/_index", dashboardChart);
        }

        #endregion
    }
}

