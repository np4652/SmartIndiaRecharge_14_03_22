using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.Employee;
using RoundpayFinTech.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface IEmpML
    {
        Task<LeadStats> GetLeadStats();
        IResponseStatus UnAssignEmployee(int UserID);
        IResponseStatus GetEmpDetailByID(string MobileNo);
        EmpRegModel GetEmpReffDeatil(string MobileNo);
        EmployeeList GetEmployee(CommonFilter filter);
        List<EList> GetAllEmpInBulk();
        EmpReg GetEmployeeByID(int EmpID);
        ResponseStatus CallCreateEmp(EmpInfo _req);
        List<RoleMaster> GetEmpRole(int GetEmpRole);
        ResponseStatus ShowEmpPass(int EmpID);
        ResponseStatus ResendEmpPass(int Id);
        ResponseStatus ChangeEmpStatus(int Id, int Is);
        ResponseStatus AssignUserToEmp(int EmpID, string mobileNo);
        List<EList> SelectEmpByRoleBulk(int Id, bool OnlyChild = false);
        IEnumerable<EList> PossibleAMForEmp(int EmpId);
        ResponseStatus ChangeEmpAssignee(int Id, int ReportingTo);
        List<EmpTarget> GetEmpTarget(int EmpID, int OID);
        IResponseStatus SaveEmpTarget(EmpTarget req);
        ResponseStatus ChangeEmpOtpStatus(int Id, int Is);
        EmployeeListU GetEmployeeeUser(CommonFilter filter);
        EmployeeListU GetEmployeeUserChild(int ReffID, bool IsUp);
        AlertReplacementModel ChangePassword(ChangePassword ChPass);

        #region Reports
        Task<List<PSTReportEmp>> GetPSTReportEmp(CommonFilter filter);
        List<PSTReport> PSTDeatilReport(CommonFilter filter);
        Task<List<TertiaryReport>> GetTertiaryReportEmp(CommonReq filter);
        Task<IEnumerable<EmployeeTargetReport>> GetEmpTargetReport(CommonReq filter);
        IEnumerable<ComparisionChart> GetComparisionChart(int UserID = 0);
        IEnumerable<LastDayVsTodayData> GetLastdayVsTodayChart(int UserID = 0);
        IEnumerable<TargetSegment> GetTargetSegment(int UserID = 0);
        List<UserCommitment> GetUserCommitment(int UserID = 0);
        ResponseStatus SetUserCommitment(UserCommitment req);
        List<EmpDownlineUser> GetEmpDownlineUser(int UserID = 0);
        IEnumerable<TodayLivePST> GetEmpTodayLivePST(int UserID = 0);
        IEnumerable<EmpUserList> TodayOutletsListForEmp(int userId = 0);
        IEnumerable<UserPackageDetail> TodaySellPackages();
        GetinTouctListModel GetUserSubscription(CommonReq req);
        Task<List<MeetingReportModel>> GetMeetingReport(CommonReq filter);
        Task<List<MeetingAddOnReportModel>> GetMeetingDetailReport(CommonReq filter);
        #endregion

        IResponseStatus UpdationAgainstLead(LeadDetail req);

        IEnumerable<TodayTransactorsModal> TodayTransactors(int type, int userId = 0);

        IResponseStatus CreateLead(CreateLead req);

        IEnumerable<PSTComparisionTable> GetPSTComparisionTable(int UserID = 0);
        IResponseStatus TransferLead(int Id, int TransferTo);
        IEnumerable<PSTDataList> GetLastSevenDayPSTDataForEmp(int UserID = 0);
        //--------------------------Sep 26, 2020--------------------------//
        IResponseStatus CreateMeeting(Meetingdetails req);
        ReasonAndPurpuse ReasonandPurpuse();
        Meetingdetails GetUserdatabyMobileNo(string MobileNo, int UserId = 0, int LoginTypeId = 0);
        IEnumerable<PincodeDetail> GetAreabypincode(int Pincode, int UserId = 0, int LoginTypeId = 0);
        List<Meetingdetails> GetMeetingdetails(CommonReq filter = null);
        IResponseStatus DailyClosing(DailyClosingModel req);
        bool IsClosingDone(int UserId = 0, int LoginType = 0);
        //--------------------------Sep 26, 2020--------------------------//
        //--------------------------Sep 28, 2020--------------------------//
        Task<List<MapPointsModel>> GetMapPoints(CommonReq filter);
        //--------------------------Sep 28, 2020--------------------------//
        Task<AttendanceReportModel> GetAttendanceReport(CommonReq filter);
    }
}
