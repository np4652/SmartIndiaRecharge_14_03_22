using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL.Employee
{
    public class ProcChangeAssignee : IProcedure
    {
        private readonly IDAL _dal;
        public ProcChangeAssignee(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            CommonReq req = (CommonReq)obj;
            SqlParameter[] param =
            {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@ReportingTo",req.CommonInt2),
                new SqlParameter("@Id",req.CommonInt)
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

        public string GetName() => "Proc_ChangeAssignee";
    }
}
