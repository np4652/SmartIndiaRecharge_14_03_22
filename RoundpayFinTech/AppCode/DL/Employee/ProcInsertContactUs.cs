using System;
using System.Data;
using System.Data.SqlClient;
using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.Employee;
using RoundpayFinTech.AppCode.Model.ProcModel;

namespace RoundpayFinTech.AppCode.DL.Employee
{
    public class ProcInsertContactUs : IProcedure
    {
        private readonly IDAL _dal;
        public ProcInsertContactUs(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var _req = (CreateLead)obj;
            SqlParameter[] param = {                
                new SqlParameter("@LognID", _req.LoginID),
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@Name", _req.Name),
                new SqlParameter("@Message", _req.Message),
                new SqlParameter("@UEmail", _req.UserEmail),
                new SqlParameter("@MobileNo", _req.MobileNo),
                new SqlParameter("@RequestmodeId", _req.RequestModeID),
                new SqlParameter("@RequestIp", _req.RequestIP),
                new SqlParameter("@Remark", _req.Remarks)
            };
            ResponseStatus _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _resp.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _resp.Msg = dt.Rows[0]["Msg"].ToString();
                }
            }
            catch (Exception ex)
            {
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = 3,
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _resp;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_InsertContactUs";
    }
}