using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class procAutoBillingProcess : IProcedureAsync
    {
        private readonly IDAL _dal;
        public procAutoBillingProcess(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var userId = (int)obj;
            var list = new List<AlertReplacementModel>();

            SqlParameter[] param = {
                new SqlParameter("@UserID", userId)
            };
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param).ConfigureAwait(true);
                if (dt.Rows.Count > 0)
                {
                    foreach (var row in dt.Rows)
                    {
                        var res = new AlertReplacementModel();
                        res.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                        res.Msg = dt.Rows[0]["_Msg"].ToString();
                        if (res.Statuscode == ErrorCodes.One && dt.Columns.Contains("_TID"))
                        {
                            res.LoginID = Convert.ToInt32(dt.Rows[0]["LoginID"]);
                            res.LoginUserName = Convert.ToString(dt.Rows[0]["LoginUserName"]);
                            res.LoginMobileNo = Convert.ToString(dt.Rows[0]["LoginMobileNo"]);
                            res.LoginEmailID = Convert.ToString(dt.Rows[0]["LoginEmailID"]);
                            res.OutletName = Convert.ToString(dt.Rows[0]["OutletName"]);
                            res.UserName = Convert.ToString(dt.Rows[0]["UserName"]);
                            res.UserID = Convert.ToInt32(dt.Rows[0]["UserID"]);
                            res.UserMobileNo = Convert.ToString(dt.Rows[0]["UserMobileNo"]);
                            res.UserEmailID = Convert.ToString(dt.Rows[0]["UserEmailID"]);
                            res.TransactionID = Convert.ToString(dt.Rows[0]["TransactionID"]);
                            res.LoginCurrentBalance = dt.Rows[0]["LoginCurrentBalance"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["LoginCurrentBalance"]);
                            res.Amount = dt.Rows[0]["Amount"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["Amount"]);
                            res.UserCurrentBalance = dt.Rows[0]["UserCurrentBalance"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["UserCurrentBalance"]);
                            res.TID = dt.Rows[0]["TID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["TID"]);
                            res.WID = dt.Rows[0]["WID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["WID"]);
                            res.UserFCMID = Convert.ToString(dt.Rows[0]["UserFCMID"]);
                            res.LoginFCMID = Convert.ToString(dt.Rows[0]["LoginFCMID"]);
                            res.SupportNumber = Convert.ToString(dt.Rows[0]["SupportNumber"]);
                            res.SupportEmail = Convert.ToString(dt.Rows[0]["SupportEmail"]);
                            res.AccountsContactNo = Convert.ToString(dt.Rows[0]["AccountsContactNo"]);
                            res.AccountEmail = Convert.ToString(dt.Rows[0]["AccountEmail"]);
                            res.Company = Convert.ToString(dt.Rows[0]["Company"]);
                            res.CompanyDomain = Convert.ToString(dt.Rows[0]["CompanyDomain"]);
                            res.LoginPrefix = Convert.ToString(dt.Rows[0]["LoginPrefix"]);
                            res.UserPrefix = Convert.ToString(dt.Rows[0]["UserPrefix"]);
                            res.BrandName = Convert.ToString(dt.Rows[0]["BrandName"]);
                            res.CompanyAddress = Convert.ToString(dt.Rows[0]["CompanyAddress"]);
                            res.WhatsappNo = Convert.ToString(dt.Rows[0]["WhatsappNo"]);
                            res.TelegramNo = Convert.ToString(dt.Rows[0]["TelegramNo"]);
                            res.HangoutNo = Convert.ToString(dt.Rows[0]["HangoutNo"]);
                            res.WhatsappNoL = Convert.ToString(dt.Rows[0]["WhatsappNoL"]);
                            res.TelegramNoL = Convert.ToString(dt.Rows[0]["TelegramNoL"]);
                            res.HangoutNoL = Convert.ToString(dt.Rows[0]["HangoutNoL"]);
                        }
                        list.Add(res);
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
            return list;
        }
        public async Task<object> Call() => throw new NotImplementedException();
        public string GetName() => "proc_AutoBilling_Process";
    }
}
