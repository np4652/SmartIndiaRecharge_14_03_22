using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Interfaces
{
    interface ISettlementaccountML
    {

        IResponseStatus UpdateSettlementaccount(SattlementAccountModels couponvoucher);
        IEnumerable<SattlementAccountModels> GetSettlementaccountList(int Id, int LoginID);
        SattlementAccountModels GeSettlementAccountByID(CommonReq req);
        IResponseStatus SetDefaultSettlementaccount(SattlementAccountModels sattlementsccount);
        IResponseStatus SetDeleteSettlementaccount(CommonReq req);
        Task<IResponseStatus> AcceptOrRejectBankupdateRequest(GetEditUser RequestData);
        SattlementAccountStatus GetApproved_VeriyfiedStatus();
        IResponseStatus UpdateUTRByUser(int ID, string UTR, int UserID);
    }
}
