using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcEKYCByBankAccount : IProcedure
    {
        private readonly IDAL _dal;
        public ProcEKYCByBankAccount(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (EKYCByBankAccountModelProcReq)obj;

            SqlParameter[] param = {
                new SqlParameter("@UserID",req.UserID),
                new SqlParameter("@AccountNo",req.AccountNumber??string.Empty),
                new SqlParameter("@IFSC",req.IFSC??string.Empty),
                new SqlParameter("@AccountHolder",req.AccountHolder??string.Empty),
                new SqlParameter("@LiveID",req.LiveID??string.Empty),
                new SqlParameter("@APIID",req.APIID),
                new SqlParameter("@BankID",req.BankID),
                new SqlParameter("@Bank",req.Bank??string.Empty),
                new SqlParameter("@IsExternal",req.IsExternal),
                new SqlParameter("@ChildUserID",req.ChildUserID),
                new SqlParameter("@APIStatus",req.APIStatus)
            };
            var res = new ResponseStatus
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
                        res.CommonInt = dt.Rows[0]["_EKYCID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_EKYCID"]);
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
                    LoginTypeID = 1,
                    UserId = req.UserID
                });
            }
            return res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_EKYCByBankAccount";
    }
}
