using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.Employee;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL.Employee
{
    public class ProcSaveEmpTarget : IProcedure
    {
        private readonly IDAL _dal;
        public ProcSaveEmpTarget(IDAL dal) => _dal = dal;
        public object Call(object obj) {
            var req = (EmpTarget)obj;
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@EmpID",req.EmpID),
                new SqlParameter("@OID",req.OID),
                new SqlParameter("@Target",req.Target),
                new SqlParameter("@Comm",req.Commission),
                new SqlParameter("@AmtType",req.AmtType),
                new SqlParameter("@IsEarned",req.IsEarned),
                new SqlParameter("@IsGift",req.IsGift),
                new SqlParameter("@TargetTypeID",req.TargetTypeID),
                new SqlParameter("@HikePer",req.HikePer),
                new SqlParameter("@IsHikeOnEarned",req.IsHikeOnEarned),
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

        public string GetName() => "proc_SaveEmpTarget";
    }
}
