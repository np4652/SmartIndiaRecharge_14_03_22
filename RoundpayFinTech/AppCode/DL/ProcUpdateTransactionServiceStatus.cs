using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcUpdateTransactionServiceStatus : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcUpdateTransactionServiceStatus(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var _req = (TransactionStatus)obj;
            SqlParameter[] param = {
                new SqlParameter("@UserID", _req.UserID),
                new SqlParameter("@TID", _req.TID),
                new SqlParameter("@APIID", _req.APIID),
                new SqlParameter("@Status", _req.Status),
                new SqlParameter("@LiveID", _req.OperatorID == null ? "" : (_req.OperatorID.Length > 50 ? _req.OperatorID.Substring(0, 50) : _req.OperatorID)),
                new SqlParameter("@VendorID", _req.VendorID == null ? "" : (_req.VendorID.Length > 50 ? _req.VendorID.Substring(0, 50) : _req.VendorID)),
                new SqlParameter("@APIOpCode", _req.APIOpCode == null ? "" : (_req.APIOpCode.Length > 100 ? _req.APIOpCode.Substring(0, 100) : _req.APIOpCode)),
                new SqlParameter("@APIName", _req.APIName == null ? "" : (_req.APIName.Length > 50 ? _req.APIName.Substring(0, 50) : _req.APIName)),
                new SqlParameter("@APICommType", _req.APICommType),
                new SqlParameter("@APIComAmt", _req.APIComAmt),
                new SqlParameter("@SwitchingID", _req.SwitchingID)
            };
            var res = new _CallbackData
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };

            try
            {
                DataTable dt = await _dal.GetByProcedureAsync(GetName(), param).ConfigureAwait(false);
                if (dt.Columns != null && dt.Columns.Contains("TransactionID"))
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? string.Empty : dt.Rows[0]["Msg"].ToString();
                    res.UserID = Convert.ToInt32(dt.Rows[0]["UserID"] is DBNull ? 0 : dt.Rows[0]["UserID"]);
                    res.UpdateUrl = dt.Rows[0]["UpdateURL"] is DBNull ? string.Empty : dt.Rows[0]["UpdateURL"].ToString();
                    res.TransactionID = dt.Rows[0]["TransactionID"] is DBNull ? string.Empty : dt.Rows[0]["TransactionID"].ToString();
                    res.RequestMode = Convert.ToInt32(dt.Rows[0]["RequestMode"] is DBNull ? 0 : dt.Rows[0]["RequestMode"]);
                    res.AccountNo = dt.Rows[0]["AccountNo"] is DBNull ? string.Empty : dt.Rows[0]["AccountNo"].ToString();
                    res.MobileNo = dt.Rows[0]["MobileNo"] is DBNull ? string.Empty : dt.Rows[0]["MobileNo"].ToString();
                    res.RequestedAmount = dt.Rows[0]["RequestedAmount"] is DBNull ? 0 : Convert.ToDecimal
                        (dt.Rows[0]["RequestedAmount"]);
                    res.TransactionID = dt.Rows[0]["TransactionID"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["TransactionID"]);
                    res.WID = dt.Rows[0]["WID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["WID"]);
                    res.Company = dt.Rows[0]["Company"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["Company"]);
                    res.CompanyDomain = dt.Rows[0]["CompanyDomain"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["CompanyDomain"]);
                    res.SupportEmail = dt.Rows[0]["SupportEmail"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["SupportEmail"]);
                    res.AccountEmail = dt.Rows[0]["AccountEmail"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["AccountEmail"]);
                    res.AccountContact = dt.Rows[0]["AccountContact"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["AccountContact"]);
                    res.FCMID = dt.Rows[0]["FCMID"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["FCMID"]);
                    res.Operator = dt.Rows[0]["Operator"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["Operator"]);
                    res.Balance = dt.Rows[0]["Balance"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["Balance"]);
                    res.TransactionStatus = dt.Rows[0]["_Status"] is DBNull ? 1 : Convert.ToInt32(dt.Rows[0]["_Status"]);
                    res.LiveID = dt.Rows[0]["_LiveID"] is DBNull ? string.Empty: dt.Rows[0]["_LiveID"].ToString();
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
                    UserId = _req.UserID
                });
            }
            return res;
        }
        public Task<object> Call() => throw new NotImplementedException();
        public string GetName() => "proc_UpdateTransactionServiceStatus";
    }
}