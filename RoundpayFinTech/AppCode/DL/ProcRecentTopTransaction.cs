using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcRecentTopTransaction : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcRecentTopTransaction(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@OpTypeID",req.CommonInt),
                new SqlParameter("@Top",req.CommonInt2<1?5:req.CommonInt2),
                new SqlParameter("@UserID",req.LoginID),
                new SqlParameter("@OutletID",req.CommonInt3)
            };
            var res = new List<ProcRechargeReportResponse>();
            try
            {
                var ds = await _dal.GetByProcedureAdapterDSAsync(GetName(), param).ConfigureAwait(false);
                var dt = ds.Tables.Count > 0 ? ds.Tables[0] : new System.Data.DataTable();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    var item = new ProcRechargeReportResponse
                    {
                        TID = Convert.ToInt32(dt.Rows[i]["_TID"] is DBNull ? 0 : dt.Rows[i]["_TID"]),
                        TransactionID = dt.Rows[i]["_TransactionID"] is DBNull ? string.Empty : dt.Rows[i]["_TransactionID"].ToString(),
                        _Type = dt.Rows[i]["_Type"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_Type"]),
                        Account = dt.Rows[i]["_Account"] is DBNull ? string.Empty : dt.Rows[i]["_Account"].ToString(),
                        OID = dt.Rows[i]["_OID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_OID"]),
                        CommType = dt.Rows[i]["_CommType"] is DBNull ? false : Convert.ToBoolean(dt.Rows[i]["_CommType"]),
                        Commission = dt.Rows[i]["_CommAmount"] is DBNull ? 0M : Convert.ToDecimal(dt.Rows[i]["_CommAmount"]),
                        Operator = dt.Rows[i]["_Operator"] is DBNull ? string.Empty : dt.Rows[i]["_Operator"].ToString(),
                        LastBalance = dt.Rows[i]["_LastBalance"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[i]["_LastBalance"]),
                        RequestedAmount = dt.Rows[i]["_RequestedAmount"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[i]["_RequestedAmount"]),
                        Amount = dt.Rows[i]["_Amount"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[i]["_Amount"]),
                        Balance = dt.Rows[i]["_Balance"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[i]["_Balance"]),
                        EntryDate = dt.Rows[i]["_EntryDate"] is DBNull ? "-" : dt.Rows[i]["_EntryDate"].ToString(),
                        LiveID = dt.Rows[i]["_LiveID"] is DBNull ? string.Empty : dt.Rows[i]["_LiveID"].ToString(),
                        Optional2 = dt.Rows[i]["_Optional2"] is DBNull ? string.Empty : dt.Rows[i]["_Optional2"].ToString()
                    };
                    item.SlabCommType = item.CommType ? "SUR" : "COM";
                    item.Type_ = RechargeRespType.GetRechargeStatusText(item._Type);
                    if (item._Type == RechargeRespType.REQUESTSENT)
                    {
                        item.Type_ = RechargeRespType._PENDING;
                    }
                    res.Add(item);
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = req.UserID
                });
            }
            return res;
        }

        public Task<object> Call() => throw new NotImplementedException();

        public string GetName() => "proc_RecentTopTransaction";
    }
}
