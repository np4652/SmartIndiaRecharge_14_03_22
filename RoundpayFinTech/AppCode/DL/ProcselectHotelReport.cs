using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Reports.Filter;
using RoundpayFinTech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.ThirdParty.HotelAPI;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;


namespace RoundpayFinTech.AppCode.DL
{
    public class ProcselectHotelReport : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcselectHotelReport(IDAL dal) => _dal = dal;
        public string GetName() => "proc_selectHotelReport";
        public async Task<object> Call(object obj)
        {
            var _req = (_RechargeReportFilter)obj;
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
                new SqlParameter("@TopRows", _req.TopRows == 0 ? 50 : _req.TopRows),
                new SqlParameter("@CCID", _req.CCID),
                new SqlParameter("@CCMobileNo", _req.CCMobileNo??""),
                new SqlParameter("@UserID", _req.UserID),
                new SqlParameter("@OPTypeID", _req.OPTypeID),
                new SqlParameter("@RequestModeID", _req.RequestModeID),
                new SqlParameter("@IsRecent", _req.IsRecent),
                new SqlParameter("@CircleID", _req.CircleID),
                new SqlParameter("@SwitchID", _req.SwitchID),
                new SqlParameter("@LiveID", _req.LiveID??string.Empty)
            };
            var _alist = new List<BookingDetails>();
            try
            {
                var ds = await _dal.GetByProcedureAdapterDSAsync(GetName(), param).ConfigureAwait(false);
                var dt = ds.Tables.Count > 0 ? ds.Tables[0] : new System.Data.DataTable();
                if (!dt.Columns.Contains("Msg"))
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        var item = new BookingDetails
                        {

                            HotelName = dt.Rows[i]["_HotelName"].ToString(),
                            Token = dt.Rows[i]["_TokenID"].ToString(),
                            TransactionID = dt.Rows[i]["_TransactionID"].ToString(),
                            Destination = dt.Rows[i]["_Destination"].ToString(),
                            TotalGuest = Convert.ToInt32(dt.Rows[i]["_TotalGuest"]),
                            BookingID = dt.Rows[i]["_BookingId"].ToString(),
                            Credited = Convert.ToInt32(dt.Rows[i]["_TotalAmount"]),
                            Debit = Convert.ToInt32(dt.Rows[i]["_TotalAmount"]),
                            Comm = Convert.ToInt32(dt.Rows[i]["_TotalAmount"]),
                            ClosingBalance = Convert.ToInt32(dt.Rows[i]["_TotalAmount"]),
                            CancellationStatus = dt.Rows[i]["_CancellationStatus"].ToString(),
                            TID = Convert.ToInt32(dt.Rows[i]["_TID"]),
                            RequestModeID = Convert.ToInt32(dt.Rows[i]["_RequestModeID"]),
                            OID = dt.Rows[i]["_OID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_OID"]),
                            CommType = dt.Rows[i]["_CommType"] is DBNull ? false : Convert.ToBoolean(dt.Rows[i]["_CommType"]),
                            Commission = dt.Rows[i]["_CommAmount"] is DBNull ? 0M : Convert.ToDecimal(dt.Rows[i]["_CommAmount"]),
                            Operator = dt.Rows[i]["_Operator"] is DBNull ? "" : dt.Rows[i]["_Operator"].ToString(),
                            LastBalance = dt.Rows[i]["_LastBalance"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[i]["_LastBalance"]),
                            RequestedAmount = Convert.ToDecimal(dt.Rows[i]["_RequestedAmount"]),
                            Amount = Convert.ToDecimal(dt.Rows[i]["_Amount"]),
                            Balance = Convert.ToDecimal(dt.Rows[i]["_Balance"]),
                            EntryDate = dt.Rows[i]["_EntryDate"] is DBNull ? "-" : dt.Rows[i]["_EntryDate"].ToString(),
                            CheckIn = dt.Rows[i]["_CheckInDate"] is DBNull ? "-" : dt.Rows[i]["_CheckInDate"].ToString(),
                            CheckOut = dt.Rows[i]["_CheckOutDate"] is DBNull ? "-" : dt.Rows[i]["_CheckOutDate"].ToString(),
                             _Type = Convert.ToInt32(dt.Rows[i]["_Type"])
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
