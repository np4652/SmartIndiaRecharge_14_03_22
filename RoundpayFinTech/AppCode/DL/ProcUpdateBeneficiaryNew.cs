using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcUpdateBeneficiaryNew : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateBeneficiaryNew(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (BeneficiaryModel)obj;
            SqlParameter[] param = {
                new SqlParameter("@Name",req.Name??string.Empty),
                new SqlParameter("@Account",req.Account??string.Empty),
                new SqlParameter("@SenderNo",req.SenderNo??string.Empty),
                new SqlParameter("@IFSC",req.IFSC??string.Empty),
                new SqlParameter("@BankID",req.BankID),
                new SqlParameter("@BankName",req.BankName??string.Empty),
                new SqlParameter("@APICode",req.APICode??string.Empty),
                new SqlParameter("@BeneID",req.BeneID??string.Empty),
                new SqlParameter("@ID",req.ID),
                new SqlParameter("@SelfRefID",req.SelfRefID),
                new SqlParameter("@ReffID",req.ReffID??string.Empty),
                new SqlParameter("@TransMode",req.TransMode??1)

            };
            req.Statuscode = ErrorCodes.Minus1;
            req.Msg = ErrorCodes.TempError;
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    req.Statuscode = dt.Rows[0][0] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0][0]);
                    req.Msg = dt.Rows[0]["Msg"] is DBNull ? string.Empty : dt.Rows[0]["Msg"].ToString();
                    req.ErrorCode = dt.Rows[0]["_ErrorCode"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_ErrorCode"]);
                    if (req.Statuscode == ErrorCodes.One)
                    {
                        req.ID = dt.Rows[0]["_ID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_ID"]);
                        req.BankID = dt.Rows[0]["_BankID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_BankID"]);
                        req.Name = dt.Rows[0]["_Name"] is DBNull ? string.Empty : dt.Rows[0]["_Name"].ToString();
                        req.Account = dt.Rows[0]["_AccountNumber"] is DBNull ? string.Empty : dt.Rows[0]["_AccountNumber"].ToString();
                        req.SenderNo = dt.Rows[0]["_SenderMobileNo"] is DBNull ? string.Empty : dt.Rows[0]["_SenderMobileNo"].ToString();
                        req.IFSC = dt.Rows[0]["_IFSC"] is DBNull ? string.Empty : dt.Rows[0]["_IFSC"].ToString();
                        req.BankName = dt.Rows[0]["_BankName"] is DBNull ? string.Empty : dt.Rows[0]["_BankName"].ToString();
                        req.APICode = dt.Rows[0]["_APICode"] is DBNull ? string.Empty : dt.Rows[0]["_APICode"].ToString();
                        req.SelfRefID = dt.Rows[0]["_SelfRefID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_SelfRefID"]);
                        req.ReffID = dt.Rows[0]["_ReffID"] is DBNull ? string.Empty : dt.Rows[0]["_ReffID"].ToString();
                        req.TransMode = dt.Rows[0]["_TransMode"] is DBNull ? 1 : Convert.ToInt32(dt.Rows[0]["_TransMode"]);
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
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = req.BankID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return req;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_UpdateBeneficiaryNew";
    }
}
