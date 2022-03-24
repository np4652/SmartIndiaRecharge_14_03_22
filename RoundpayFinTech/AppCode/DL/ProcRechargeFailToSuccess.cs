using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Reports.Filter;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;


namespace RoundpayFinTech.AppCode.DL
{
    public class ProcRechargeFailToSuccess : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcRechargeFailToSuccess(IDAL dal) => _dal = dal;

        public async Task<object> Call(object obj)
        {
            _RechargeReportFilter _req = (_RechargeReportFilter)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LT),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@Type", _req.Status),
                new SqlParameter("@OutletNo", _req.OutletNo ?? ""),
                new SqlParameter("@TransactionID", _req.TransactionID ?? ""),
                new SqlParameter("@TID", _req.TID),
                new SqlParameter("@APIRequestID", _req.APIRequestID ?? ""),
                new SqlParameter("@AccountNo", _req.AccountNo ?? ""),
                new SqlParameter("@OID", _req.OID),
                new SqlParameter("@APIID", _req.APIID),
                new SqlParameter("@VendorID", _req.VendorID ?? ""),
                new SqlParameter("@FromDate", string.IsNullOrEmpty(_req.FromDate) ? DateTime.Now.ToString("dd MMM yyyy") : _req.FromDate),
                new SqlParameter("@ToDate", string.IsNullOrEmpty(_req.ToDate) ? DateTime.Now.ToString("dd MMM yyyy") : _req.ToDate),
                new SqlParameter("@TopRows", _req.TopRows == 0 ? 50 : _req.TopRows)
            };
            List<ProcRechargeReportResponse> _alist = new List<ProcRechargeReportResponse>();
            try
            {
                DataTable dt = await _dal.GetByProcedureAsync(GetName(), param);

                if (!dt.Columns.Contains("Msg"))
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        var item = new ProcRechargeReportResponse
                        {
                            ResultCode = ErrorCodes.One,
                            Msg = "Success",
                            TID = Convert.ToInt32(dt.Rows[i]["_TID"]),
                            TransactionID = dt.Rows[i]["_TransactionID"].ToString(),
                            _Type = Convert.ToInt32(dt.Rows[i]["_Type"]),
                            RefundStatus = Convert.ToInt32(dt.Rows[i]["_RefundStatus"]),
                            UserID = Convert.ToInt32(dt.Rows[i]["_UserID"]),
                            OutletNo = dt.Rows[i]["_OutletNo"].ToString(),
                            Outlet = dt.Rows[i]["_OutletName"] is DBNull ? "" : dt.Rows[i]["_OutletName"].ToString(),
                            Account = dt.Rows[i]["_Account"].ToString(),
                            OID = dt.Rows[i]["_OID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_OID"]),
                            CommType = dt.Rows[i]["_CommType"] is DBNull ? false : Convert.ToBoolean(dt.Rows[i]["_CommType"]),
                            Commission = dt.Rows[i]["_CommAmount"] is DBNull ? 0M : Convert.ToDecimal(dt.Rows[i]["_CommAmount"]),
                            Operator = dt.Rows[i]["_Operator"] is DBNull ? "" : dt.Rows[i]["_Operator"].ToString(),
                            LastBalance = dt.Rows[i]["_LastBalance"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[i]["_LastBalance"]),
                            RequestedAmount = Convert.ToDecimal(dt.Rows[i]["_RequestedAmount"]),
                            Amount = Convert.ToDecimal(dt.Rows[i]["_Amount"]),
                            Balance = Convert.ToDecimal(dt.Rows[i]["_Balance"]),
                            EntryDate = dt.Rows[i]["_EntryDate"] is DBNull ? "-" : dt.Rows[i]["_EntryDate"].ToString(),
                            API = dt.Rows[i]["_API"] is DBNull ? "" : dt.Rows[i]["_API"].ToString(),
                            LiveID = dt.Rows[i]["_LiveID"] is DBNull ? "" : dt.Rows[i]["_LiveID"].ToString(),
                            VendorID = dt.Rows[i]["_VendorID"] is DBNull ? "" : dt.Rows[i]["_VendorID"].ToString(),
                            Optional1 = dt.Rows[i]["_Optional1"] is DBNull ? "" : dt.Rows[i]["_Optional1"].ToString(),
                            Optional2 = dt.Rows[i]["_Optional2"] is DBNull ? "" : dt.Rows[i]["_Optional2"].ToString(),
                            Optional3 = dt.Rows[i]["_Optional3"] is DBNull ? "" : dt.Rows[i]["_Optional3"].ToString(),
                            Optional4 = dt.Rows[i]["_Optional4"] is DBNull ? "" : dt.Rows[i]["_Optional4"].ToString(),
                            ModifyDate = dt.Rows[i]["_ModifyDate"] is DBNull ? "-" : dt.Rows[i]["_ModifyDate"].ToString(),
                            ApiRequestID = dt.Rows[i]["_ApiRequestID"] is DBNull ? "" : dt.Rows[i]["_ApiRequestID"].ToString(),
                            IsWTR = dt.Columns.Contains("IsWTR") ? (dt.Rows[i]["IsWTR"] is DBNull ? false : Convert.ToBoolean(dt.Rows[i]["IsWTR"])) : false
                        };
                        item.SlabCommType = item.CommType ? "COM" : "SUR";
                        item.Type_ = RechargeRespType.GetRechargeStatusText(item._Type);
                        if (_req.IsExport && item._Type == RechargeRespType.REQUESTSENT)
                        {
                            item.Type_ = RechargeRespType._PENDING;
                        }

                        item.RefundStatus_ = RefundType.GetRefundTypeText(item.RefundStatus);
                        _alist.Add(item);
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
                    LoginTypeID = 1,
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _alist;
        }

        public Task<object> Call() => throw new NotImplementedException();

        public string GetName() => "Proc_RechargeFailToSuccess";
    }
}
