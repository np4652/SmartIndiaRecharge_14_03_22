using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetNotificationSendDetail : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetNotificationSendDetail(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            var req = (CommonReq)obj;

            SqlParameter[] param = {
                new SqlParameter("@UT",req.LoginTypeID),
                new SqlParameter("@UserID",req.LoginID),
                new SqlParameter("@FormatID",req.CommonInt),
                new SqlParameter("@TID",req.CommonInt2)
            };
            var res = new NotificationServiceReq
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };

            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = dt.Rows[0][0] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
                    if (res.Statuscode == ErrorCodes.One)
                    {
                        res.WID = dt.Rows[0]["WID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["WID"]);
                        res.FCMID = dt.Rows[0]["FCMID"] is DBNull ? "" : dt.Rows[0]["FCMID"].ToString();
                        res.UserID = req.LoginID;
                        res.EmailID = dt.Rows[0]["EmailID"] is DBNull ? "" : dt.Rows[0]["EmailID"].ToString();
                        res.Mobile = dt.Rows[0]["UMobile"] is DBNull ? "" : dt.Rows[0]["UMobile"].ToString();
                        res.RequestMode = dt.Rows[0]["RequestModeID"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["RequestModeID"]);
                        res.RechargeStatus = dt.Rows[0]["RechargeStatus"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["RechargeStatus"]);
                        res.replacementModelForSMS = new ReplacementModelForSMS
                        {
                            Mobile = dt.Rows[0]["AccountNo"] is DBNull ? "" : dt.Rows[0]["AccountNo"].ToString(),
                            Amount = dt.Rows[0]["Amount"] is DBNull ? "" : dt.Rows[0]["Amount"].ToString(),
                            OperatorName = dt.Rows[0]["OperatorName"] is DBNull ? "" : dt.Rows[0]["OperatorName"].ToString(),
                            FromMobileNo = dt.Rows[0]["FromMobileNo"] is DBNull ? "" : dt.Rows[0]["FromMobileNo"].ToString(),
                            ToMobileNo = dt.Rows[0]["ToMobileNo"] is DBNull ? "" : dt.Rows[0]["ToMobileNo"].ToString(),
                            BalanceAmount = dt.Rows[0]["BalanceAmount"] is DBNull ? "" : dt.Rows[0]["BalanceAmount"].ToString(),
                            TransactionID = dt.Rows[0]["TransactionID"] is DBNull ? "" : dt.Rows[0]["TransactionID"].ToString(),
                            Company = dt.Rows[0]["Company"] is DBNull ? "" : dt.Rows[0]["Company"].ToString(),
                            CompanyDomain = dt.Rows[0]["CompanyDomain"] is DBNull ? "" : dt.Rows[0]["CompanyDomain"].ToString(),
                            OTP = dt.Rows[0]["OTP"] is DBNull ? "" : dt.Rows[0]["OTP"].ToString(),
                            FromUserName = dt.Rows[0]["FromUserName"] is DBNull ? "" : dt.Rows[0]["FromUserName"].ToString(),
                            ToUserName = dt.Rows[0]["ToUserName"] is DBNull ? "" : dt.Rows[0]["ToUserName"].ToString(),
                            UserName = dt.Rows[0]["UserName"] is DBNull ? "" : dt.Rows[0]["UserName"].ToString(),
                            CompanyMobile = dt.Rows[0]["CompanyMobile"] is DBNull ? "" : dt.Rows[0]["CompanyMobile"].ToString(),
                            CompanyEmail = dt.Rows[0]["CompanyEmail"] is DBNull ? "" : dt.Rows[0]["CompanyEmail"].ToString(),
                            LoginID = dt.Rows[0]["LoginID"] is DBNull ? "" : dt.Rows[0]["LoginID"].ToString(),
                            LiveID = dt.Rows[0]["LiveID"] is DBNull ? "" : dt.Rows[0]["LiveID"].ToString()
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "proc_GetNotificationSendDetail";
        }
    }
}
