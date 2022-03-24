using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class GetBeneficaryByID : IProcedure
    {
        private readonly IDAL _dal;
        public GetBeneficaryByID(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (BeneficiaryModel)obj;
            SqlParameter[] param = {
                new SqlParameter("@ID",req.ID)

            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    req.ID = dt.Rows[0]["_ID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_ID"]);
                    req.BankID = dt.Rows[0]["_BankID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_BankID"]);
                    req.Name = dt.Rows[0]["_Name"] is DBNull ? string.Empty : dt.Rows[0]["_Name"].ToString();
                    req.MobileNo = dt.Rows[0]["_MobileNo"] is DBNull ? string.Empty : dt.Rows[0]["_MobileNo"].ToString();
                    req.Account = dt.Rows[0]["_AccountNumber"] is DBNull ? string.Empty : dt.Rows[0]["_AccountNumber"].ToString();
                    req.SenderNo = dt.Rows[0]["_SenderMobileNo"] is DBNull ? string.Empty : dt.Rows[0]["_SenderMobileNo"].ToString();
                    req.IFSC = dt.Rows[0]["_IFSC"] is DBNull ? string.Empty : dt.Rows[0]["_IFSC"].ToString();
                    req.BankName = dt.Rows[0]["_BankName"] is DBNull ? string.Empty : dt.Rows[0]["_BankName"].ToString();
                    req.APICode = dt.Rows[0]["_APICode"] is DBNull ? string.Empty : dt.Rows[0]["_APICode"].ToString();
                    req.TransMode = dt.Rows[0]["_TransMode"] is DBNull ? 1 : Convert.ToInt32(dt.Rows[0]["_TransMode"]);
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
        public string GetName() => "proc_GetBeneficaryByID";
    }
}
