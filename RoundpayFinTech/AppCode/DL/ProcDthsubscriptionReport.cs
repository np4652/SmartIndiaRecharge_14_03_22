using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Reports.Filter;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcDthsubscriptionReport : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcDthsubscriptionReport(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var req = (_RechargeReportFilter)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", req.LT),
                new SqlParameter("@LoginID", req.LoginID),
                new SqlParameter("@FromDate", req.FromDate??DateTime.Now.ToString()),
                new SqlParameter("@ToDate", req.ToDate??DateTime.Now.ToString()),
                new SqlParameter("@Type", req.Status),
                new SqlParameter("@RequestMode", req.RequestModeID),
                new SqlParameter("@OID", req.OID),
                new SqlParameter("@OutletNo", req.UserOutletMobile??string.Empty),
                new SqlParameter("@TID", req.TID),
                new SqlParameter("@TransactionID", req.TransactionID??string.Empty),
                new SqlParameter("@LiveID", req.LiveID??string.Empty),
                new SqlParameter("@CustomerMobile", req.CMobileNo??string.Empty),
                new SqlParameter("@AccountNo", req.AccountNo??string.Empty),
                new SqlParameter("@PID", req.PID),
                new SqlParameter("@BookingStatus", req.BookingStatus),
                new SqlParameter("@UserID", req.UserID),
                new SqlParameter("@APIID", req.APIID),
                new SqlParameter("@TopRows", req.TopRows<1?50:req.TopRows),
                new SqlParameter("@APIRequestID", req.APIRequestID??string.Empty)
            };

            var res = new List<DTHSubscriptionReport>();
            try
            {
                DataTable dt = await _dal.GetByProcedureAsync(GetName(), param).ConfigureAwait(false);
                if (dt.Rows.Count > 0 && !dt.Columns.Contains("Msg"))
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        res.Add(new DTHSubscriptionReport
                        {
                            ID = Convert.ToInt32(dr["_ID"]),
                            TID = Convert.ToInt32(dr["_TID"]),
                            TransactionID = dr["_TransactionID"] is DBNull ? string.Empty : Convert.ToString(dr["_TransactionID"]),
                            OutletName = dr["_OutletName"] is DBNull ? string.Empty : Convert.ToString(dr["_OutletName"]),
                            MobileNo = dr["_MobileNo"] is DBNull ? string.Empty : Convert.ToString(dr["_MobileNo"]),
                            Account = dr["_Account"] is DBNull ? string.Empty : Convert.ToString(dr["_Account"]),
                            Opening = dr["_Opening"] is DBNull ? 0 : Convert.ToDecimal(dr["_Opening"]),
                            RequestedAmount = dr["_RequestedAmount"] is DBNull ? 0 : Convert.ToDecimal(dr["_RequestedAmount"]),
                            Amount = dr["_Amount"] is DBNull ? 0 : Convert.ToDecimal(dr["_Amount"]),
                            Balance = dr["_Balance"] is DBNull ? 0 : Convert.ToDecimal(dr["_Balance"]),
                            Commission = dr["_Commission"] is DBNull ? 0 : Convert.ToDecimal(dr["_Commission"]),
                            BookingStatus = dr["_BookingStatus"] is DBNull ? 0 : Convert.ToInt32(dr["_BookingStatus"]),
                            Status = dr["_Type"] is DBNull ? 0 : Convert.ToInt32(dr["_Type"]),
                            CustomerNumber = dr["CustomerNumber"] is DBNull ? string.Empty : Convert.ToString(dr["CustomerNumber"]),
                            CustomerName = dr["_CustomerName"] is DBNull ? string.Empty : Convert.ToString(dr["_CustomerName"]),
                            Address = dr["_Address"] is DBNull ? string.Empty : Convert.ToString(dr["_Address"]),
                            Pincode = dr["_Pincode"] is DBNull ? string.Empty : Convert.ToString(dr["_Pincode"]),
                            PID = dr["_PID"] is DBNull ? 0 : Convert.ToInt32(dr["_PID"]),
                            PackageName = dr["_PackageName"] is DBNull ? string.Empty : Convert.ToString(dr["_PackageName"]),
                            Operator = dr["_Operator"] is DBNull ? string.Empty : Convert.ToString(dr["_Operator"]),
                            OID = dr["_OID"] is DBNull ? 0 : Convert.ToInt32(dr["_OID"]),
                            API = dr["_API"] is DBNull ? string.Empty : Convert.ToString(dr["_API"]),
                            LiveID = dr["_LiveID"] is DBNull ? string.Empty : Convert.ToString(dr["_LiveID"]),
                            Remark = dr["_Remark"] is DBNull ? string.Empty : Convert.ToString(dr["_Remark"]),
                            EntryDate = dr["_EntryDate"] is DBNull ? string.Empty : Convert.ToString(dr["_EntryDate"]),
                            ModifyDate = dr["_ModifyDate"] is DBNull ? string.Empty : Convert.ToString(dr["_ModifyDate"]),
                            BookingStatus_ = dr["BookingStatus_"] is DBNull ? string.Empty : Convert.ToString(dr["BookingStatus_"]),
                            Status_ = dr["Type_"] is DBNull ? string.Empty : Convert.ToString(dr["Type_"]),
                            TechnicianName = dr["_TechnicianName"] is DBNull ? string.Empty : Convert.ToString(dr["_TechnicianName"]),
                            TechnicianMobile = dr["_TechnicianMobile"] is DBNull ? string.Empty : Convert.ToString(dr["_TechnicianMobile"]),
                            CustomerID = dr["_CustomerID"] is DBNull ? string.Empty : Convert.ToString(dr["_CustomerID"]),
                            STBID = dr["_STBID"] is DBNull ? string.Empty : Convert.ToString(dr["_STBID"]),
                            VCNO = dr["_VCNO"] is DBNull ? string.Empty : Convert.ToString(dr["_VCNO"]),
                            ApprovalTime = dr["_Approvaltime"] is DBNull ? string.Empty : Convert.ToString(dr["_Approvaltime"]),
                            InstallationTime = dr["_InstallTime"] is DBNull ? string.Empty : Convert.ToString(dr["_InstallTime"]),
                            InstalltionCharges = dr["_InstallCharges"] is DBNull ? string.Empty : Convert.ToString(dr["_InstallCharges"])

                        });
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
                    LoginTypeID = req.LT,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }
        public Task<object> Call() => throw new NotImplementedException();
        public string GetName() => "Proc_DthsubscriptionReport";
    }
}