using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Reports.Filter;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcFundReceiveStatement : IProcedure
    {
        private readonly IDAL _dal;
        public ProcFundReceiveStatement(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = new List<ProcFundReceiveStatementResponse>();
            var req = (ULFundReceiveReportFilter)obj;
            SqlParameter[] param =
            {
                new SqlParameter("@ServiceID", req.ServiceID),
                new SqlParameter("@LoginId", req.LoginId),
                new SqlParameter("@Fdate", req.FDate ?? DateTime.Now.ToString("dd MMM yyyy")),
                new SqlParameter("@Tdate", req.TDate ?? DateTime.Now.ToString("dd MMM yyyy")),
                new SqlParameter("@MobileNo",req.MobileNo??""),
                new SqlParameter("@UserID",req.UserID),
                new SqlParameter("@IsSelf",req.IsSelf),
                new SqlParameter("@WalletTypeID",req.WalletTypeID),
                new SqlParameter("@OtherUserMob",req.OtherUserMob??""),
                new SqlParameter("@OtherUserID",req.OtherUserID),
                new SqlParameter("@LT",req.LT)
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt != null && dt.Rows.Count > 0)
                {
                    if (dt.Columns.Contains("_Description")) {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            var ll = new ProcFundReceiveStatementResponse
                            {
                                StatusCode = 1,
                                Description = dt.Rows[i]["_Description"] is DBNull ? "" : dt.Rows[i]["_Description"].ToString(),
                                Amount = dt.Rows[i]["_Amount"].ToString(),
                                CurrentAmount = dt.Rows[i]["_CurrentAmount"] is DBNull ? "" : dt.Rows[i]["_CurrentAmount"].ToString(),
                                EntryDate = dt.Rows[i]["_EntryDate"] is DBNull ? "" : dt.Rows[i]["_EntryDate"].ToString(),
                                TransactionID = dt.Rows[i]["_TransactionID"] is DBNull ? "" : dt.Rows[i]["_TransactionID"].ToString(),
                                Remark = dt.Rows[i]["_Remark"] is DBNull ? "" : dt.Rows[i]["_Remark"].ToString(),
                                UserName = dt.Rows[i]["_OutletName"] is DBNull ? "" : dt.Rows[i]["_OutletName"].ToString(),
                                MobileNo = dt.Rows[i]["_MobileNo"] is DBNull ? "" : dt.Rows[i]["_MobileNo"].ToString(),
                                ServiceTypeID = dt.Rows[i]["_ServiceTypeID"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[i]["_ServiceTypeID"]),
                                WalletID = dt.Rows[i]["_WalletID"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[i]["_WalletID"]),
                                OtherUser = dt.Rows[i]["OtherUser"] is DBNull ? "" : dt.Rows[i]["OtherUser"].ToString()
                            };
                            _req.Add(ll);
                        }
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
                    LoginTypeID = 1,
                    UserId = req.LT
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _req;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetUserFundReceiveStatement";
    }
}
