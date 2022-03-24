using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcFundProcess : IProcedure
    {
        private readonly IDAL _dal;
        public ProcFundProcess(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var req = (FundProcessReq)obj;
            var res = new AlertReplacementModel
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            SqlParameter[] param = {
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@UserID",req.fundProcess.UserID),
                new SqlParameter("@Amount",req.fundProcess.Amount),
                new SqlParameter("@OType",req.fundProcess.OType),
                new SqlParameter("@Remark",req.fundProcess.Remark),
                new SqlParameter("@RequestMode",req.fundProcess.RequestMode),
                new SqlParameter("@WalletType",req.fundProcess.WalletType),
                new SqlParameter("@IP",req.CommonStr),
                new SqlParameter("@Browser",req.CommonStr2),
                new SqlParameter("@PaymentId",req.fundProcess.PaymentId),
                new SqlParameter("@SecurityKey",HashEncryption.O.Encrypt(req.fundProcess.SecurityKey??"")),
                new SqlParameter("@IsMarkCredit",req.fundProcess.IsMarkCredit)
              // new SqlParameter("@IsDebitApproval",req.IsDebitWithApproval)
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
                    if (res.Statuscode == ErrorCodes.One && dt.Columns.Contains("_TID"))
                    {
                        res.LoginID = Convert.ToInt32(dt.Rows[0]["LoginID"]);
                        res.LoginUserName = Convert.ToString(dt.Rows[0]["LoginName"]);
                        res.LoginMobileNo = Convert.ToString(dt.Rows[0]["LoginMobileNo"]);
                        res.LoginEmailID = Convert.ToString(dt.Rows[0]["LoginEmailID"]);
                        res.OutletName = Convert.ToString(dt.Rows[0]["OutletName"]);
                        res.UserName = Convert.ToString(dt.Rows[0]["UserName"]);
                        res.UserID = Convert.ToInt32(dt.Rows[0]["UserID"]);
                        res.UserMobileNo = Convert.ToString(dt.Rows[0]["MobileNo"]);
                        res.UserEmailID = Convert.ToString(dt.Rows[0]["EmailID"]);
                        res.TransactionID = Convert.ToString(dt.Rows[0]["TransactionID"]);
                        res.LoginCurrentBalance = dt.Rows[0]["CBalance_L"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["CBalance_L"]);
                        res.Amount = dt.Rows[0]["Amount"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["Amount"]);
                        res.UserCurrentBalance = dt.Rows[0]["CBalance_U"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["CBalance_U"]);
                        res.TID = dt.Rows[0]["_TID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_TID"]);
                        res.WID = dt.Rows[0]["WID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["WID"]);
                        res.UserFCMID = Convert.ToString(dt.Rows[0]["_UserFCMID"]);
                        res.LoginFCMID = Convert.ToString(dt.Rows[0]["_LoginFCMID"]);
                        res.SupportNumber = Convert.ToString(dt.Rows[0]["SupportNumber"]);
                        res.SupportEmail = Convert.ToString(dt.Rows[0]["SupportEmail"]);
                        res.AccountsContactNo = Convert.ToString(dt.Rows[0]["AccountNumber"]);
                        res.AccountEmail = Convert.ToString(dt.Rows[0]["AccountEmail"]);
                        res.Company = Convert.ToString(dt.Rows[0]["CompanyName"]);
                        res.CompanyDomain = Convert.ToString(dt.Rows[0]["CompnayDomain"]);
                        res.LoginPrefix = Convert.ToString(dt.Rows[0]["LoginPrefix"]);
                        res.UserPrefix = Convert.ToString(dt.Rows[0]["UserPrefix"]);
                        res.BrandName = Convert.ToString(dt.Rows[0]["BrandName"]);
                        res.OutletName = Convert.ToString(dt.Rows[0]["OutletName"]);
                        res.CompanyAddress = Convert.ToString(dt.Rows[0]["CompanyAddress"]);
                        res.UserPrefix = Convert.ToString(dt.Rows[0]["UserPrefix"]);
                        res.WhatsappNo = Convert.ToString(dt.Rows[0]["_WhatsappNo"]);
                        res.TelegramNo = Convert.ToString(dt.Rows[0]["_TelegramNo"]);
                        res.HangoutNo = Convert.ToString(dt.Rows[0]["_HangoutNo"]);
                        res.WhatsappNoL = Convert.ToString(dt.Rows[0]["_WhatsappNoL"]);
                        res.TelegramNoL = Convert.ToString(dt.Rows[0]["_TelegramNoL"]);
                        res.HangoutNoL = Convert.ToString(dt.Rows[0]["_HangoutNoL"]);
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
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_FundProcess";
    }


    public class ProcFundProcessDebitApprova : IProcedure
    {
        private readonly IDAL _dal;
        public ProcFundProcessDebitApprova(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var req = (FundProcessReq)obj;
            var res = new AlertReplacementModel
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            SqlParameter[] param = {
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@UserID",req.fundProcess.UserID),
                new SqlParameter("@Amount",req.fundProcess.Amount),
                new SqlParameter("@OType",req.fundProcess.OType),
                new SqlParameter("@Remark",req.fundProcess.Remark),
                new SqlParameter("@RequestMode",req.fundProcess.RequestMode),
                new SqlParameter("@WalletType",req.fundProcess.WalletType),
                new SqlParameter("@IP",req.CommonStr),
                new SqlParameter("@Browser",req.CommonStr2),
                new SqlParameter("@PaymentId",req.fundProcess.PaymentId),
                new SqlParameter("@SecurityKey",HashEncryption.O.Encrypt(req.fundProcess.SecurityKey??"")),
                new SqlParameter("@IsMarkCredit",req.fundProcess.IsMarkCredit)
              // new SqlParameter("@IsDebitApproval",req.IsDebitWithApproval)
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
                   
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_FundProcessDebitApproval";
    }
}
