using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.Model.Reports;
using Microsoft.AspNetCore.Http;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.App;
using RoundpayFinTech.AppCode.Model.MoneyTransfer;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface ISwitchingML
    {
        List<CircleAPISwitchDetail> GetCircleMultiSwitchedDetail(int CircleID);
        IResponseStatus UserwiseLimitCU(UserLimitCUReq userLimitCUReq);
        IEnumerable<CircleSwitch> GetCircleBlocked();
        IEnumerable<PriorityApiSwitch> GetAPISwitching(int OpTypeID);
        IResponseStatus SwitchAPI(APISwitched switched);
        IEnumerable<APIOpCode> GetAPISwitchByUser(int ID, int OpTypeID);
        IResponseStatus SwitchUserwiseAPI(SwitchAPIUser switchAPIUser);
        IEnumerable<CircleSwitch> GetCircleSwitches(int APIID);
        IResponseStatus UpdateCircleSwitch(Circle circle);
        IResponseStatus UpdateCircleBlock(Circle circle);
        IResponseStatus UpdateOperatorStatus(UpdateDownStatusReq updateDownStatusReq);
        IEnumerable<Userswitch> Userswitches(string MobileNo, int OpTypeID);
        IResponseStatus SetAPI(APISwitched switched);
        IResponseStatus UpdateDenomination(APIDenominationReq req);
        IResponseStatus UpdateDenominationUser(APIDenominationReq req);
        IEnumerable<DSRDesign> GetDSwitchReport(int OpTypeID);
        IEnumerable<DSRDesign> GetDSwitchReportUser(int OpTypeID, int UserID, string MobileNo);
        IEnumerable<UserWiseLimitResp> GetUserLimitByUser(int ID);
        IResponseStatus BlockUsersForSwitching(int UserID, int SwithID);

        IResponseStatus RemoveDenominationUser(int ID);
        IResponseStatus RemoveDenomination(int ID);
    }
}
