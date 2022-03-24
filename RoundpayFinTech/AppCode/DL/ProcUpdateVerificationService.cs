using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcUpdateVerificationService : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateVerificationService(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (TransactionStatus)obj;
            SqlParameter[] param = {
                new SqlParameter("@TID", _req.TID),
                new SqlParameter("@Status", _req.Status),
                new SqlParameter("@LiveID", _req.OperatorID == null ? "" : (_req.OperatorID.Length > 50 ? _req.OperatorID.Substring(0, 50) : _req.OperatorID)),
                new SqlParameter("@VendorID", _req.VendorID == null ? "" : (_req.VendorID.Length > 50 ? _req.VendorID.Substring(0, 50) : _req.VendorID)),
                new SqlParameter("@Req", _req.Request??string.Empty),
                new SqlParameter("@Resp", _req.Response??string.Empty),
                new SqlParameter("@APIID", _req.APIID)
                
            };
            var res = new _CallbackData
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };

            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
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
                    res.LiveID = dt.Rows[0]["_LiveID"] is DBNull ? string.Empty : dt.Rows[0]["_LiveID"].ToString();
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

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_UpdateVerificationService";
    }
}
