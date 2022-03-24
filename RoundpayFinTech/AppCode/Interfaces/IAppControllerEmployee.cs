using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoundpayFinTech.AppCode.Model.App;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface IAppControllerEmployee
    {
        Task<IActionResult> GetEmployees([FromBody] EmployeeFilterRequest appRequest);
        Task<IActionResult> GetEmployeeeUser([FromBody] EmployeeUserFilterRequest appRequest);
        Task<IActionResult> GetComparisionChart([FromBody] AppSessionReq appRequest);
        Task<IActionResult> GetLastdayVsTodayChart([FromBody] AppSessionReq appRequest);
        Task<IActionResult> GetTargetSegment([FromBody] AppSessionReq appRequest);
        Task<IActionResult> GetUserCommitment([FromBody] AppSessionReq appRequest);
        Task<IActionResult> GetEmpDownlineUser([FromBody] AppSessionReq appRequest);
        Task<IActionResult> GetPSTReportEmp([FromBody] PSTRequest appRequest);
        Task<IActionResult> GetTertiaryReportEmp([FromBody] TertiaryRequest appRequest);
        Task<IActionResult> GetEmployeeTargetReport([FromBody] EmpTargetRequest appRequest);
        Task<IActionResult> SetUserCommitment([FromBody] UserCommitmentRequest appRequest);
        Task<IActionResult> RechargeReportForEmployee([FromBody] AppRechargeReportReq appRechargeReportReq);
        Task<IActionResult> LedgerReportForEmployee([FromBody] AppLedgerReq appLedgerReq);
        Task<IActionResult> UserDaybookForEmployee([FromBody] AppReportCommon appReportCommon);
        Task<IActionResult> DMTReportForEmployee([FromBody] AppDMTReportReq appDMTReportReq);
        Task<IActionResult> GetUserCommitmentChart([FromBody] AppSessionReq appRequest);
        Task<IActionResult> GetEmpTodayLivePST([FromBody] AppSessionReq appRequest);
        Task<IActionResult> FundDCReportForEmployee([FromBody] AppFundDCReq appFundDCReq);
        //--------------------------Ahmed Gulzar--------------------------//
        //--------------------------Sep 26, 2020--------------------------//
        IActionResult CreateMeeting([FromBody] AppSessionReq appRequest);
        IActionResult PostCreateMeeting(string CreateMeetingRequest, IFormFile file);
        IActionResult GetUserByMobile([FromBody] GetUserByMobileRequest appRequest);
        IActionResult GetMeetingDetail([FromBody] CommonFilterRequest appRequest);
        IActionResult PostDailyClosing([FromBody] DailyClosingRequest appRequest);
        Task<IActionResult> GetMeetingReport([FromBody] CommonFilterRequest appRequest);
        Task<IActionResult> GetMeetingSubReport([FromBody] CommonFilterRequest appRequest);
        IActionResult GetAreabyPincode([FromBody] GetAreabyPincodeRequest appRequest);
        //--------------------------Sep 26, 2020--------------------------//
        //--------------------------Sep 28, 2020--------------------------//
        Task<IActionResult> GetMapPoints([FromBody] CommonFilterRequest appRequest);
        //--------------------------Sep 28, 2020--------------------------//
        //Task<IActionResult> GetCharts([FromBody] AppSessionReq appRequest);
    }
}
