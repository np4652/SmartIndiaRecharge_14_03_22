using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Reports.Filter;
using RoundpayFinTech.AppCode.StaticModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcDisplayLive : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcDisplayLive(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var _req = (_RechargeReportFilter)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LT),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@Type", _req.Status)
            };
            var _alist = new List<ProcRechargeReportResponse>();
            try
            {
                var ds = await _dal.GetByProcedureAdapterDSAsync(GetName(), param).ConfigureAwait(false);
                var dt = ds.Tables.Count > 0 ? ds.Tables[0] : new System.Data.DataTable();
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
                            IsWTR = dt.Columns.Contains("IsWTR") ? (dt.Rows[i]["IsWTR"] is DBNull ? false : Convert.ToBoolean(dt.Rows[i]["IsWTR"])) : false,
                            CCName = dt.Rows[i]["_Customercare"] is DBNull ? "" : dt.Rows[i]["_Customercare"].ToString(),
                            CCMobile = dt.Rows[i]["_CCMobileNo"] is DBNull ? "" : dt.Rows[i]["_CCMobileNo"].ToString(),
                            Display1 = dt.Rows[i]["_Display1"] is DBNull ? "" : dt.Rows[i]["_Display1"].ToString(),
                            Display2 = dt.Rows[i]["_Display2"] is DBNull ? "" : dt.Rows[i]["_Display2"].ToString(),
                            Display3 = dt.Rows[i]["_Display3"] is DBNull ? "" : dt.Rows[i]["_Display3"].ToString(),
                            Display4 = dt.Rows[i]["_Display4"] is DBNull ? "" : dt.Rows[i]["_Display4"].ToString(),
                            SwitchingID = dt.Rows[i]["_SwitchingID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_SwitchingID"]),
                            CircleName = dt.Rows[i]["_CircleName"] is DBNull ? "" : dt.Rows[i]["_CircleName"].ToString(),
                            _ServiceID = dt.Rows[i]["_ServiceID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_ServiceID"]),
                            RequestModeID = dt.Rows[i]["_RequestModeID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_RequestModeID"])
                        };
                        if (item.RequestModeID == RequestMode.API)
                        {
                            item.RequestMode = nameof(RequestMode.API);
                        }
                        if (item.RequestModeID == RequestMode.APPS)
                        {
                            item.RequestMode = nameof(RequestMode.APPS);
                        }
                        if (item.RequestModeID == RequestMode.PANEL)
                        {
                            item.RequestMode = nameof(RequestMode.PANEL);
                        }
                        item.SwitchingName = SwitchingType.GetSwitchType(item.SwitchingID);
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
            return _alist;
        }

        public Task<object> Call() => throw new NotImplementedException();

        public string GetName() => "Proc_DisplayLive";
    }
}
