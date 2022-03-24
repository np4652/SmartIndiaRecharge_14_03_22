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
    public class ProcBillFetchReport : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcBillFetchReport(IDAL dal) => _dal = dal;
        public string GetName() => "Proc_BillFetchReport";
        public async Task<object> Call(object obj)
        {
            var _req = (_BillFetchReportFilter)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LT),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@Status", _req.Status),
                new SqlParameter("@OPTypeID", _req.OPTypeID),
                new SqlParameter("@BillNumber", _req.BillNumber),
                new SqlParameter("@AccountNo", _req.AccountNo ?? ""),
                new SqlParameter("@OutletNo", _req.OutletNo ?? ""),
                 new SqlParameter("@OID", _req.OID),
                new SqlParameter("@FromDate", string.IsNullOrEmpty(_req.FromDate) ? DateTime.Now.ToString("dd MMM yyyy") : _req.FromDate),
                new SqlParameter("@ToDate", string.IsNullOrEmpty(_req.ToDate) ? DateTime.Now.ToString("dd MMM yyyy") : _req.ToDate),
                 new SqlParameter("@TopRows", _req.TopRows == 0 ? 50 : _req.TopRows)
            };
            var _alist = new List<ProcBillFetchReportResponse>();
            try
            {
                var ds = await _dal.GetByProcedureAdapterDSAsync(GetName(), param).ConfigureAwait(false);
                var dt = ds.Tables.Count > 0 ? ds.Tables[0] : new System.Data.DataTable();
                if (!dt.Columns.Contains("Msg"))
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        var item = new ProcBillFetchReportResponse
                        {

                            ResultCode = ErrorCodes.One,
                            Msg = "Success",
                            ID = Convert.ToInt32(dt.Rows[i]["_ID"]),
                            BillNumber = dt.Rows[i]["_ID"].ToString(),
                            OutletNo = dt.Rows[i]["_OutletNo"].ToString(),
                            Outlet = dt.Rows[i]["Outlet"] is DBNull ? "" : dt.Rows[i]["Outlet"].ToString(),
                            Account = dt.Rows[i]["_AccountNumber"].ToString(),
                            Operator = dt.Rows[i]["Operator"] is DBNull ? "" : dt.Rows[i]["Operator"].ToString(),
                            EntryDate = dt.Rows[i]["_EntryDate"] is DBNull ? "-" : dt.Rows[i]["_EntryDate"].ToString(),
                            PaymentStatus = dt.Rows[i]["_IsProcessed"].ToString(),
                            Status = dt.Rows[i]["_Status"].ToString(),
                            Reason = dt.Rows[i]["_Reason"] is DBNull ? "" : dt.Rows[i]["_Reason"].ToString(),
                            CustomerName = dt.Rows[i]["_CustomerName"].ToString(),
                            Amount = Convert.ToDecimal(dt.Rows[i]["_Amount"]),
                            API = dt.Rows[i]["API"] is DBNull ? "" : dt.Rows[i]["API"].ToString(),

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
                        item.Type_ = RechargeRespType.GetRechargeStatusText(item._Type);
                        if (_req.IsExport && item._Type == RechargeRespType.REQUESTSENT)
                        {
                            item.Type_ = RechargeRespType._PENDING;
                        }

                        _alist.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = _req.LT,
                    UserId = _req.LoginID
                });
            }
            return _alist;
        }

        public Task<object> Call() => throw new NotImplementedException();
    }
}
