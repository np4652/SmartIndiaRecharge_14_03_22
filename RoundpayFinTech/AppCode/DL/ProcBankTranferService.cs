using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcBankTranferService : IProcedure
    {
        private readonly IDAL _dal;
        public ProcBankTranferService(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (BankServiceReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@WalletRequestID",req.WalletRequestID),
                new SqlParameter("@RequestModeID",req.RequestModeID),
                new SqlParameter("@RequestIP",req.RequestIP??string.Empty)
            };
            var res = new BankServiceResp
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
                    if (res.Statuscode == ErrorCodes.One)
                    {
                        res.UserID = dt.Rows[0]["_UserID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_UserID"]);
                        res.TID= dt.Rows[0]["_TID"] is DBNull ?0 : Convert.ToInt32(dt.Rows[0]["_TID"]);
                        res.Amount= dt.Rows[0]["_Amount"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["_Amount"]);
                        res.APIID = dt.Rows[0]["_APIID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_APIID"]);
                        res.OID = dt.Rows[0]["_OID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_OID"]);
                        res.TransactionID = dt.Rows[0]["_TransactionID"] is DBNull ? string.Empty : dt.Rows[0]["_TransactionID"].ToString();
                        res.APICode = dt.Rows[0]["_APICode"] is DBNull ? string.Empty : dt.Rows[0]["_APICode"].ToString();
                        res.APIOpCode = dt.Rows[0]["_APIOpCode"] is DBNull ? string.Empty : dt.Rows[0]["_APIOpCode"].ToString();
                        res.AccountNumber = dt.Rows[0]["_AccountNumber"] is DBNull ? string.Empty : dt.Rows[0]["_AccountNumber"].ToString();
                        res.AccountHolder = dt.Rows[0]["_AccountHolder"] is DBNull ? string.Empty : dt.Rows[0]["_AccountHolder"].ToString();
                        res.IFSC = dt.Rows[0]["_IFSC"] is DBNull ? string.Empty : dt.Rows[0]["_IFSC"].ToString();
                        res.OutletMobile = dt.Rows[0]["_OutletMobile"] is DBNull ? string.Empty : dt.Rows[0]["_OutletMobile"].ToString();
                        res.TransactionMode = dt.Rows[0]["_TransactionMode"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_TransactionMode"]);
                        res.BankName = dt.Rows[0]["_BankName"] is DBNull ? string.Empty : dt.Rows[0]["_BankName"].ToString();
                        res.EmailID = dt.Rows[0]["_EmailID"] is DBNull ? string.Empty : dt.Rows[0]["_EmailID"].ToString();
                        res.BankID = dt.Rows[0]["_BankID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_BankID"]);
                        res.BrandName = dt.Rows[0]["_BrandName"] is DBNull ? string.Empty : dt.Rows[0]["_BrandName"].ToString();
                        res.APIOutletID = dt.Rows[0]["_APIOutletID"] is DBNull ? string.Empty : dt.Rows[0]["_APIOutletID"].ToString();
                        res.WebsiteName = dt.Rows[0]["_WebsiteName"] is DBNull ? string.Empty : dt.Rows[0]["_WebsiteName"].ToString();
                        res.APIGroupCode = dt.Rows[0]["_APIGroupCode"] is DBNull ? string.Empty : dt.Rows[0]["_APIGroupCode"].ToString();
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
                    LoginTypeID = 1,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_BankTranferService_Active";
    }
}
