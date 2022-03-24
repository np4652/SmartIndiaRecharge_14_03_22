using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcUpdatePaytmTransaction : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdatePaytmTransaction(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (UpdatePGTransactionRequest)obj;
            SqlParameter[] param = {
                new SqlParameter("@PGID",req.PGID),
                new SqlParameter("@TID",req.TID),
                new SqlParameter("@Type",req.Type),
                new SqlParameter("@VendorID",req.VendorID??string.Empty),
                new SqlParameter("@LiveID",req.LiveID??string.Empty),
                new SqlParameter("@PaymentModeSpKey",req.PaymentModeSpKey??string.Empty),
                new SqlParameter("@Remark",req.Remark??string.Empty),
                new SqlParameter("@RequestIP",req.RequestIP??string.Empty),
                new SqlParameter("@Browser",req.Browser??string.Empty),
                new SqlParameter("@Signature",req.Signature??string.Empty)
            };
            var res = new AlertReplacementModel
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = dt.Rows[0][0] is DBNull ? res.Statuscode : Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? string.Empty : dt.Rows[0]["Msg"].ToString();
                    if(res.Statuscode == ErrorCodes.One)
                    {
                        res.UserID = Convert.ToInt32(dt.Rows[0]["UserID"]);
                        res.WID = dt.Rows[0]["WID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["WID"]);
                        res.LoginID = dt.Rows[0]["UserID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["UserID"]);
                        res.UserMobileNo = dt.Rows[0]["MobileNo"] is DBNull ? string.Empty : dt.Rows[0]["MobileNo"].ToString();
                        res.UserEmailID = dt.Rows[0]["EmailID"] is DBNull ? string.Empty : dt.Rows[0]["EmailID"].ToString();
                        res.SupportNumber = dt.Rows[0]["SupportNumber"] is DBNull ? string.Empty : dt.Rows[0]["SupportNumber"].ToString(); 
                        res.SupportEmail = dt.Rows[0]["SupportEmail"] is DBNull ? string.Empty : dt.Rows[0]["SupportEmail"].ToString();
                        res.AccountsContactNo = dt.Rows[0]["AccountNumber"] is DBNull ? string.Empty : dt.Rows[0]["AccountNumber"].ToString();
                        res.AccountEmail = dt.Rows[0]["AccountEmail"] is DBNull ? string.Empty : dt.Rows[0]["AccountEmail"].ToString(); 
                        res.Company = dt.Rows[0]["CompanyName"] is DBNull ? string.Empty : dt.Rows[0]["CompanyName"].ToString();
                        res.CompanyDomain = dt.Rows[0]["CompanyDomain"] is DBNull ? string.Empty : dt.Rows[0]["CompanyDomain"].ToString();
                        res.CompanyAddress = dt.Rows[0]["CompanyAddress"] is DBNull ? string.Empty : dt.Rows[0]["CompanyAddress"].ToString(); 
                        res.OutletName = dt.Rows[0]["OutletName"] is DBNull ? string.Empty : dt.Rows[0]["OutletName"].ToString();
                        res.BrandName = dt.Rows[0]["BrandName"] is DBNull ? string.Empty : dt.Rows[0]["BrandName"].ToString();
                        res.UserName = dt.Rows[0]["UserName"] is DBNull ? string.Empty : dt.Rows[0]["UserName"].ToString();
                        res.WhatsappNo = dt.Rows[0]["_WhatsappNo"] is DBNull ? string.Empty : dt.Rows[0]["_WhatsappNo"].ToString();
                        res.TelegramNo = dt.Rows[0]["_TelegramNo"] is DBNull ? string.Empty : dt.Rows[0]["_TelegramNo"].ToString();
                        res.HangoutNo = dt.Rows[0]["_HangoutNo"] is DBNull ? string.Empty : dt.Rows[0]["_HangoutNo"].ToString(); 
                        //res.TID = dt.Rows[0]["_TID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_TID"] ); 
                        res.BalanceAmount = dt.Rows[0]["BalanceAmount"] is DBNull ? string.Empty : dt.Rows[0]["BalanceAmount"].ToString();
                        res.Amount = dt.Rows[0]["Amount"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["Amount"]);
                        res.UserFCMID = dt.Rows[0]["UserFCMID"] is DBNull ? string.Empty : dt.Rows[0]["UserFCMID"].ToString();
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
                    LoginTypeID = req.PGID,
                    UserId = req.TID
                });
            }
            return res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_UpdatePaytmTransaction";
    }
}
