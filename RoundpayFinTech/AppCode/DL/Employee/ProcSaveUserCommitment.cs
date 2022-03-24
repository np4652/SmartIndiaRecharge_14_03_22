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
    public class ProcSaveUserCommitment : IProcedure
    {
        private readonly IDAL _dal;
        public ProcSaveUserCommitment(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (UserCommitment)obj;
            SqlParameter[] param =
            {
                new SqlParameter("@LoginID",req.EmpID),
                new SqlParameter("@UserID",req.UserID),
                new SqlParameter("@Commitment",req.Commitment),
                new SqlParameter("@Longitute",req.Longitute),
                new SqlParameter("@Latitude",req.Latitude)
            };

            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _res.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _res.Msg = Convert.ToString(dt.Rows[0]["Msg"]);
                }
            }
            catch (Exception ex)
            {
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _res;
        }
        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_SaveUserCommitment";
    }
}
