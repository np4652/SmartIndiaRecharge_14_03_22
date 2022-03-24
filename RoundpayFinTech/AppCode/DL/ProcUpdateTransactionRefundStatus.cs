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
    public class ProcUpdateTransactionRefundStatus : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcUpdateTransactionRefundStatus(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var _req = (RefundRequestData)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LT),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@TID", _req.TID),
                new SqlParameter("@RefundStatus", _req.RefundStatus),
                new SqlParameter("@Remark", _req.Remark ?? ""),
                new SqlParameter("@IP", _req.RequestIP ?? ""),
                new SqlParameter("@Browser", _req.Browser ?? ""),
                new SqlParameter("@RequestMode", _req.RequestMode),
                new SqlParameter("@RefAmount", _req.Amount)
            };
            
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param).ConfigureAwait(false);
                if (dt.Rows.Count > 0)
                {
                    _req.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _req.Msg = dt.Rows[0]["Msg"].ToString();
                    if (_req.Statuscode == ErrorCodes.One)
                    {
                        _req.UserID = Convert.ToInt32(dt.Rows[0]["UserID"] is DBNull ? 0 : dt.Rows[0]["UserID"]);
                        _req.ChargedAmount = Convert.ToDecimal(dt.Rows[0]["ChargedAmount"] is DBNull ? 0 : dt.Rows[0]["ChargedAmount"]);
                        _req.Account = dt.Rows[0]["AccountNo"] is DBNull ? "" : dt.Rows[0]["AccountNo"].ToString();
                        _req.IsSameDay = Convert.ToBoolean(dt.Rows[0]["IsSameDay"] is DBNull ? false : dt.Rows[0]["IsSameDay"]);
                        _req.UserMobileNo = dt.Rows[0]["UserMobileNo"] is DBNull ? "" : dt.Rows[0]["UserMobileNo"].ToString();
                        _req.Amount = dt.Rows[0]["Amount"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["Amount"]);
                        _req.BalanceAmount = dt.Rows[0]["Balance"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["Balance"]);
                        _req.UserFCMID = dt.Rows[0]["_UserFCMID"] is DBNull ? "" : dt.Rows[0]["_UserFCMID"].ToString();
                        _req.LiveID = dt.Rows[0]["LiveID"] is DBNull ? "" : dt.Rows[0]["LiveID"].ToString();
                        _req.Company = dt.Rows[0]["Company"] is DBNull ? "" : dt.Rows[0]["Company"].ToString();
                        _req.SupportNumber = dt.Rows[0]["SupportNumber"] is DBNull ? "" : dt.Rows[0]["SupportNumber"].ToString();
                        _req.SupportEmail = dt.Rows[0]["SupportEmail"] is DBNull ? "" : dt.Rows[0]["SupportEmail"].ToString();
                        _req.AccountContact = dt.Rows[0]["AccountNumber"] is DBNull ? "" : dt.Rows[0]["AccountNumber"].ToString();
                        _req.AccountEmail = dt.Rows[0]["AccountEmail"] is DBNull ? "" : dt.Rows[0]["AccountEmail"].ToString();
                        _req.CompanyDomain = dt.Rows[0]["CompanyDomain"] is DBNull ? "" : dt.Rows[0]["CompanyDomain"].ToString();
                        _req.OperatorName = dt.Rows[0]["Operator"] is DBNull ? "" : dt.Rows[0]["Operator"].ToString();
                        _req.WID = dt.Rows[0]["WID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["WID"]);
                        _req.UserEmailID = Convert.ToString(dt.Rows[0]["_UserEmailID"]);
                        _req.CompanyAddress = Convert.ToString(dt.Rows[0]["CompanyAddress"]);
                        _req.BrandName = Convert.ToString(dt.Rows[0]["BrandName"]);
                        _req.OutletName = Convert.ToString(dt.Rows[0]["OutletName"]);
                        _req.UserName = Convert.ToString(dt.Rows[0]["UserName"]);
                        _req.UserWhatsappNo = Convert.ToString(dt.Rows[0]["UserWhatsappNo"]);
                        _req.UserTelegram = dt.Rows[0]["UserTelegram"] is DBNull?string.Empty: Convert.ToString(dt.Rows[0]["UserTelegram"]);
                        _req.UserHangout = dt.Rows[0]["UserHangout"] is DBNull?string.Empty: Convert.ToString(dt.Rows[0]["UserHangout"]);
                        _req.ConversationID = dt.Rows[0]["_ConversationID"] is DBNull?string.Empty: Convert.ToString(dt.Rows[0]["_ConversationID"]);
                        DataColumnCollection columns = dt.Columns;
                        if (columns.Contains("_TransactionID"))
                        {
                            _req.TransactionID = Convert.ToString(dt.Rows[0]["_TransactionID"]);
                        }                        
                        if (dt.Columns.Contains("_CallbackURL"))
                        {
                            _req.CallbackURL = dt.Rows[0]["_CallbackURL"] is DBNull ? string.Empty : dt.Rows[0]["_CallbackURL"].ToString();
                            _req.TransactionID = dt.Rows[0]["_TransactionID"] is DBNull ? string.Empty : dt.Rows[0]["_TransactionID"].ToString();
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
                    LoginTypeID = _req.LT,
                    UserId = _req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _req;
        }
        public Task<object> Call() => throw new NotImplementedException();
        public string GetName() => "proc_UpdateTransactionRefundStatus";
    }
}
