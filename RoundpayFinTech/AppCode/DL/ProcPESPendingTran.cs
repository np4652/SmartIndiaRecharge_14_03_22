using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcPESPendingTran : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcPESPendingTran(IDAL dal)
        {
            _dal = dal;
        }
        public async Task<object> Call(object obj)
        {
            CommonReq _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@OID", _req.CommonInt2)
            };

            var _res = new List<PendingTransaction>();
            try
            {
                DataTable dt = await _dal.GetByProcedureAsync(GetName(), param);
                if (dt.Rows.Count < 1)
                    return _res;
                if (Convert.ToInt32(dt.Rows[0][0]) == ErrorCodes.One)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        var transaction = new PendingTransaction
                        {
                            TID = Convert.ToInt32(row["_TID"]),
                            TransactionID = row["_TransactionID"].ToString(),
                            _Type = Convert.ToInt32(row["_Type"]),
                            AccountNo = row["_Account"].ToString(),
                            RequestedAmount = Convert.ToDecimal(row["_RequestedAmount"]),
                            EntryDate = row["_EntryDate"].ToString(),
                            APIID = Convert.ToInt32(row["_APIID"] is DBNull ? 0 : row["_APIID"]),
                            APIName = row["_API"] is DBNull ? "" : row["_API"].ToString(),
                            VendorID = row["_VendorID"] is DBNull ? "" : row["_VendorID"].ToString(),
                            OID = Convert.ToInt32(row["_OID"] is DBNull ? 0 : row["_OID"]),
                            Operator = row["_Operator"] is DBNull ? "" : row["_Operator"].ToString(),
                            OutletMobile = row["_MobileNo"] is DBNull ? "" : row["_MobileNo"].ToString(),
                            OutletName = row["_OutletName"] is DBNull ? "" : row["_OutletName"].ToString(),
                            Response = row["_Response"] is DBNull ? "" : row["_Response"].ToString(),
                            Optional1 = row["_Optional1"] is DBNull ? "" : row["_Optional1"].ToString(),
                            Optional2 = row["_Optional2"] is DBNull ? "" : row["_Optional2"].ToString(),
                            Optional3 = row["_Optional3"] is DBNull ? "" : row["_Optional3"].ToString(),
                            Optional4 = row["_Optional4"] is DBNull ? "" : row["_Optional4"].ToString(),
                            ModifyDate = row["_ModifyDate"] is DBNull ? "-" : row["_ModifyDate"].ToString(),
                            ApiRequestID = row["_ApiRequestID"] is DBNull ? "" : row["_ApiRequestID"].ToString(),
                            CCName = row["_Customercare"] is DBNull ? "" : row["_Customercare"].ToString(),
                            CCMobile = row["_CCMobileNo"] is DBNull ? "" : row["_CCMobileNo"].ToString(),
                            Display1 = row["_Display1"] is DBNull ? "" : row["_Display1"].ToString(),
                            Display2 = row["_Display2"] is DBNull ? "" : row["_Display2"].ToString(),
                            Display3 = row["_Display3"] is DBNull ? "" : row["_Display3"].ToString(),
                            Display4 = row["_Display4"] is DBNull ? "" : row["_Display4"].ToString(),
                        };
                        if (_req.CommonInt3 == ReportType.DMR)
                        {
                            transaction.SenderMobile = row["_SMobile"] is DBNull ? "" : row["_SMobile"].ToString();
                        }
                        transaction.Type_ = RechargeRespType.GetRechargeStatusText(transaction._Type);

                        _res.Add(transaction);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = _req.LoginTypeID,
                    UserId = _req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _res;
        }

        public Task<object> Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "sp_GetPESPendingTransaction";
        }
    }
}
