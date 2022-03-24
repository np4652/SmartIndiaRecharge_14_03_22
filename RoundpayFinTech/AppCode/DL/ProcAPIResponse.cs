using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcAPIResponse : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcAPIResponse(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var _req = (_CallbackData)obj;
            _req.Statuscode = ErrorCodes.Minus1;
            SqlParameter[] param = {
                 new SqlParameter("@Type", _req.TransactionStatus),
                new SqlParameter("@TID", _req.TID),
                new SqlParameter("@VendorID", (_req.VendorID ?? string.Empty).Length > 50 ? (_req.VendorID ?? "").Substring(0, 50) : (_req.VendorID ?? "")),
                new SqlParameter("@LiveID", _req.LiveID ?? string.Empty),
                new SqlParameter("@Msg", _req.Msg == null ? string.Empty : (_req.Msg.Length > 200 ? _req.Msg.Substring(0, 200) : _req.Msg)),
                new SqlParameter("@RequestPage", _req.RequestPage == null ? string.Empty : _req.RequestPage.Length > 50 ? _req.RequestPage.Substring(0, 50) : _req.RequestPage),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@LT", _req.LT),
                new SqlParameter("@APIBalance", _req.APIBalance),
                new SqlParameter("@IP", _req.RequestIP ?? string.Empty),
                new SqlParameter("@Browser", _req.Browser ?? string.Empty),
                new SqlParameter("@APIID", _req.APIID)
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
                        _req.WID = Convert.ToInt32(dt.Rows[0]["_WID"] is DBNull ? 0 : dt.Rows[0]["_WID"]);
                        _req.IsBBPS = Convert.ToBoolean(dt.Rows[0]["_IsBBPS"] is DBNull ? false : dt.Rows[0]["_IsBBPS"]);
                        _req.UpdateUrl = dt.Rows[0]["UpdateURL"] is DBNull ? string.Empty : dt.Rows[0]["UpdateURL"].ToString();
                        _req.TransactionID = dt.Rows[0]["TransactionID"] is DBNull ? string.Empty : dt.Rows[0]["TransactionID"].ToString();
                        _req.CustomerNumber = dt.Rows[0]["_CustomerNumber"] is DBNull ? string.Empty : dt.Rows[0]["_CustomerNumber"].ToString();
                        _req.RequestMode = Convert.ToInt32(dt.Rows[0]["RequestMode"] is DBNull ? 0 : dt.Rows[0]["RequestMode"]);
                        _req.IsCallbackFound = dt.Rows[0]["IsCallbackFound"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["IsCallbackFound"]);
                        //New lines
                        _req.AccountNo = dt.Rows[0]["AccountNo"] is DBNull ? string.Empty : dt.Rows[0]["AccountNo"].ToString();
                        _req.Balance = dt.Rows[0]["Balance"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["Balance"]);
                        _req.RequestedAmount = dt.Rows[0]["RequestedAmount"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["RequestedAmount"]);
                        _req.FCMID = dt.Rows[0]["FCMID"] is DBNull ? string.Empty : dt.Rows[0]["FCMID"].ToString();
                        _req.Operator = dt.Rows[0]["Operator"] is DBNull ? string.Empty : dt.Rows[0]["Operator"].ToString();
                        _req.MobileNo = dt.Rows[0]["MobileNo"] is DBNull ? string.Empty : dt.Rows[0]["MobileNo"].ToString();
                        _req.EmailID = dt.Rows[0]["EmailID"] is DBNull ? string.Empty : dt.Rows[0]["EmailID"].ToString();
                        _req.WID = dt.Rows[0]["WID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["WID"]);
                        _req.Company = dt.Rows[0]["Company"] is DBNull ? string.Empty : dt.Rows[0]["Company"].ToString();
                        _req.CompanyDomain = dt.Rows[0]["CompanyDomain"] is DBNull ? string.Empty : dt.Rows[0]["CompanyDomain"].ToString();
                        _req.SupportNumber = dt.Rows[0]["SupportNumber"] is DBNull ? string.Empty : dt.Rows[0]["SupportNumber"].ToString();
                        _req.SupportEmail = dt.Rows[0]["SupportEmail"] is DBNull ? string.Empty : dt.Rows[0]["SupportEmail"].ToString();
                        _req.AccountContact = dt.Rows[0]["AccountContact"] is DBNull ? string.Empty : dt.Rows[0]["AccountContact"].ToString();
                        _req.AccountEmail = dt.Rows[0]["AccountEmail"] is DBNull ? string.Empty : dt.Rows[0]["AccountEmail"].ToString();
                        _req.CompanyAddress = dt.Rows[0]["CompanyAddress"] is DBNull ? string.Empty : dt.Rows[0]["CompanyAddress"].ToString();
                        _req.UserName = dt.Rows[0]["UserName"] is DBNull ? string.Empty : dt.Rows[0]["UserName"].ToString();
                        _req.UserMobileNo = dt.Rows[0]["UserMobileNo"] is DBNull ? string.Empty : dt.Rows[0]["UserMobileNo"].ToString();
                        _req.BrandName = dt.Rows[0]["BrandName"] is DBNull ? string.Empty : dt.Rows[0]["BrandName"].ToString();
                        _req.OutletName = dt.Rows[0]["OutletName"] is DBNull ? string.Empty : dt.Rows[0]["OutletName"].ToString();
                        _req.IsInternalSender = dt.Rows[0]["_IsInternalSender"] is DBNull ? false: Convert.ToBoolean(dt.Rows[0]["_IsInternalSender"]);
                        _req.Optional2 = dt.Rows[0]["_Optional2"] is DBNull ? string.Empty : dt.Rows[0]["_Optional2"].ToString();
                        _req.Optional3 = dt.Rows[0]["_Optional3"] is DBNull ? string.Empty : dt.Rows[0]["_Optional3"].ToString();
                        _req.Optional4 = dt.Rows[0]["_Optional4"] is DBNull ? string.Empty : dt.Rows[0]["_Optional4"].ToString();
                        _req.O10 = dt.Rows[0]["_O10"] is DBNull ? string.Empty : dt.Rows[0]["_O10"].ToString();
                        _req.UserWhatsappNo = Convert.ToString(dt.Rows[0]["UserWhatsappNo"] is DBNull ? string.Empty : dt.Rows[0]["UserWhatsappNo"].ToString());
                        _req.UserTelegram = Convert.ToString(dt.Rows[0]["UserTelegram"] is DBNull ? string.Empty : dt.Rows[0]["UserTelegram"].ToString());
                        _req.UserHangout = Convert.ToString(dt.Rows[0]["UserHangout"] is DBNull ? string.Empty : dt.Rows[0]["UserHangout"].ToString());
                        _req.RefundStatus = dt.Rows[0]["RefundStatus"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["RefundStatus"]);
                        _req.ConversationID = dt.Rows[0]["_ConversationID"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["_ConversationID"]);
                        _req.UserEmailID = dt.Rows[0]["UserEmailID"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["UserEmailID"]);
                        if(dt.Columns.Contains("_SCode"))
                            _req.SCode = dt.Rows[0]["_SCode"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["_SCode"]);
                        if(dt.Columns.Contains("_TotalToken"))
                            _req.TotalToken = dt.Rows[0]["_TotalToken"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_TotalToken"]);
                        //New LInes End
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
                    UserId = _req.UserID
                });
            }
            return _req;
        }
        public Task<object> Call() => throw new NotImplementedException();
        public string GetName() => "proc_APIResponse";

        public void UpdateAPIContext(string APIContext, int TID) {
            SqlParameter[] param = { 
                new SqlParameter("@APIContext",APIContext??string.Empty),
                new SqlParameter("@TID",TID)
            };
            string query = "update tbl_Transaction set _APIContext=@APIContext where _TID=@TID";
            try
            {
                _dal.Execute(query,param);
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = 0,
                    UserId = TID
                });
            }
        }
    }
}
