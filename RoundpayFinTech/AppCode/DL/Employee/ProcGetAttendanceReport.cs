using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.Employee;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL.Employee
{
    public class ProcGetAttendanceReport : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcGetAttendanceReport(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@month", _req.CommonInt),
                new SqlParameter("@year", _req.CommonInt3),
                //new SqlParameter("@DtFrom", _req.CommonStr ?? DateTime.Now.ToString("dd MMM yyyy")),
                //new SqlParameter("@DtTill", _req.CommonStr2 ?? DateTime.Now.ToString("dd MMM yyyy")),
                new SqlParameter("@Criteria", _req.CommonInt2),
                new SqlParameter("@CValue", _req.CommonStr3)
            };
            var _alist = new AttendanceReportModel();
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param).ConfigureAwait(false);
                dt.Columns.RemoveAt(0);
                _alist.dtReport = dt;
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = _req.LoginTypeID,
                    UserId = _req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _alist;
        }

        public Task<object> Call() => throw new NotImplementedException();

        public string GetName() => "Proc_GetEmployeeAttendance";
    }
}
