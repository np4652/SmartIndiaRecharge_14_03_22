using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetUserSettlement : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcGetUserSettlement(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var _req = (SettlementFilter)obj;
            SqlParameter[] param = {
                 new SqlParameter("@LT", _req.LT),
                 new SqlParameter("@LoginID", _req.LoginID),
                 new SqlParameter("@FromDate", _req.FromDate),
                 new SqlParameter("@ToDate", _req.ToDate),
                 new SqlParameter("@Mobile", _req.Mobile??""),
                 new SqlParameter("@TopRows", _req.TopRow),
                 new SqlParameter("@UserID", _req.UserID),
                 new SqlParameter("@WalletTypeID", _req.WalletTypeID),
            };
            var _resList = new List<UserSettlement>();
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param).ConfigureAwait(false);
                if (dt.Rows.Count > 0)
                {
                    foreach(DataRow dr in dt.Rows)
                    {
                        var us = new UserSettlement
                        {
                            ID=Convert.ToInt32(dr["_ID"]),
                            UserID=Convert.ToInt32(dr["_UserID"]),
                            UserName=Convert.ToString(dr["_UserName"]),
                            Prefix=Convert.ToString(dr["_Prefix"]),
                            MobileNo = Convert.ToString(dr["_MobileNo"]),
                            TransactionDate = Convert.ToString(dr["_TransactionDate"]),
                            Opening = Convert.ToDecimal(dr["_Opening"]),
                            FundTransfered = Convert.ToDecimal(dr["_FundTransfered"]),
                            FundRecieved = Convert.ToDecimal(dr["_FundRecieved"]),
                            Refund = Convert.ToDecimal(dr["_Refund"]),
                            Commission = Convert.ToDecimal(dr["_Commission"]),
                            CCFComm = Convert.ToDecimal(dr["_CCFComm"]),
                            FundDeducted = Convert.ToDecimal(dr["_FundDeducted"]),
                            FundCredited = Convert.ToDecimal(dr["_FundCredited"]),
                            Surcharge = Convert.ToDecimal(dr["_Surcharge"]),
                            SuccessPrepaid = Convert.ToDecimal(dr["_SuccessPrepaid"]),
                            SuccessPostpaid = Convert.ToDecimal(dr["_SuccessPostpaid"]),
                            SuccessDTH = Convert.ToDecimal(dr["_SuccessDTH"]),
                            SuccessBill = Convert.ToDecimal(dr["_SuccessBill"]),
                            SuccessDMT = Convert.ToDecimal(dr["_SuccessDMT"]),
                            SuccessDMTCharge = Convert.ToDecimal(dr["_SuccessDMTCharge"]),
                            OtherCharge = Convert.ToDecimal(dr["_OtherCharge"]),
                            CCFCommDebited = Convert.ToDecimal(dr["_CCFCommDebited"]),
                            Closing = Convert.ToDecimal(dr["_Closing"]),
                            SuccessOther = Convert.ToDecimal(dr["_SuccessOther"]),
                            WalletID = Convert.ToInt32(dr["_WalletID"]),
                            EntryDate = Convert.ToString(dr["_EntryDate"]),
                            
                        };
                        us.Expected = us.Opening + us.FundRecieved + us.FundCredited + us.Commission + us.Refund - us.FundTransfered - us.FundDeducted - us.SuccessBill - us.SuccessDMT - us.SuccessDMTCharge - us.SuccessDTH - us.SuccessOther - us.SuccessPostpaid - us.SuccessPrepaid;
                        us.Difference = us.Expected - us.Closing;
                        _resList.Add(us);
                    }
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = _req.LT,
                    UserId = _req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _resList;
        }
        public Task<object> Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetUserSettlement";
    }
}
