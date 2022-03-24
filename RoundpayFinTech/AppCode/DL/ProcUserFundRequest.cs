using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcUserFundRequest : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUserFundRequest(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (FundRequest)obj;
            SqlParameter[] param = {
                 new SqlParameter("@LoginID", _req.LoginID),
                 new SqlParameter("@Amount", _req.Amount),
                 new SqlParameter("@BankID", _req.BankId),
                 new SqlParameter("@PaymentModeID", _req.PaymentId),
                 new SqlParameter("@TransactionID", _req.TransactionId ?? ""),
                 new SqlParameter("@MobileNo", _req.MobileNo ?? ""),
                 new SqlParameter("@ChequeNo", _req.ChequeNo ?? ""),
                 new SqlParameter("@CardNumber", _req.CardNo ?? ""),
                 new SqlParameter("@AccountHolder", _req.AccountHolderName ?? ""),
                 new SqlParameter("@WalletTypeID", _req.WalletTypeID),
                 new SqlParameter("@Branch", _req.Branch),
                 new SqlParameter("@UPIID", _req.UPIID),
                 new SqlParameter("@RImage", _req.RImage==null?"":_req.RImage),
                 new SqlParameter("@UPIOrderID", _req.OrderID),
                 new SqlParameter("@IsAuto", _req.IsAuto)
            };
            var _res = new AlertReplacementModel
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _res.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _res.Msg = dt.Rows[0]["Msg"].ToString();
                    if (_res.Statuscode == ErrorCodes.One) {
                        _res.ID = dt.Rows[0]["ID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["ID"]);
                        _res.LoginID = dt.Rows[0]["LoginID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["LoginID"]);
                        _res.UserID = dt.Rows[0]["ToUserID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["ToUserID"]);
                        _res.WID = dt.Rows[0]["_WID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_WID"]);
                        _res.Amount = dt.Rows[0]["Amount"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["Amount"]);
                        _res.LoginUserName = Convert.ToString(dt.Rows[0]["_outletName"]);
                        _res.LoginMobileNo = Convert.ToString(dt.Rows[0]["_MobileNo"]);
                        _res.UserMobileNo = Convert.ToString(dt.Rows[0]["_ToMobileNo"]);
                        _res.UserEmailID = Convert.ToString(dt.Rows[0]["_ToEmail"]);
                        _res.LoginUserName = Convert.ToString(dt.Rows[0]["_Name"]);
                        _res.UserName = Convert.ToString(dt.Rows[0]["ToUserName"]);
                        _res.UserFCMID = Convert.ToString(dt.Rows[0]["_FCMID"]);
                        _res.Company = Convert.ToString(dt.Rows[0]["Company"]);
                        _res.CompanyDomain = Convert.ToString(dt.Rows[0]["CompanyDomain"]);
                        _res.SupportEmail = Convert.ToString(dt.Rows[0]["SupportEmail"]);
                        _res.SupportNumber = Convert.ToString(dt.Rows[0]["SupportNumber"]);
                        _res.AccountsContactNo = Convert.ToString(dt.Rows[0]["AccountNumber"]);
                        _res.AccountEmail = Convert.ToString(dt.Rows[0]["AccountEmail"]);
                        _res.CompanyAddress = Convert.ToString(dt.Rows[0]["CompanyAddress"]);
                        _res.BrandName = Convert.ToString(dt.Rows[0]["BrandName"]);
                        _res.OutletName = Convert.ToString(dt.Rows[0]["_OutletName"]);
                        _res.WhatsappNo = dt.Rows[0]["_WhatsappNo"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["_WhatsappNo"]);
                        _res.TelegramNo = dt.Rows[0]["_TelegramNo"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["_TelegramNo"]);
                        _res.HangoutNo = dt.Rows[0]["_HangoutId"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["_HangoutId"]);
                        _res.FormatID = MessageFormat.FundOrderAlert;
                        _res.NotificationTitle = "Fund Order";
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
                    LoginTypeID = _req.LoginTypeID,
                    UserId = _req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_FundOrder";
    }
}
