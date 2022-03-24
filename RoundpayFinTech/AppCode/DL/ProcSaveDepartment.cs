using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.DepartmentModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcSaveDepartment : IProcedure
    {
        private readonly IDAL _dal;

        public ProcSaveDepartment(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            Department req = (Department)obj;

            SqlParameter[] param = {
                new SqlParameter("@ID",req.ID),
                new SqlParameter("@Name",req.Name),
                new SqlParameter("@Remark",req.Remark??"")
            };
            var resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    resp.Statuscode = dt.Rows[0][0] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0][0]);
                    resp.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
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
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return resp;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "proc_SaveDepartment";
        }
    }
    public class ProcSaveDepartmentRole : IProcedure
    {
        private readonly IDAL _dal;

        public ProcSaveDepartmentRole(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            DepartmentRole req = (DepartmentRole)obj;

            SqlParameter[] param =
            {
                new SqlParameter("@DeptID",req.DepartmentID),
                new SqlParameter("@ID",req.ID),
                new SqlParameter("@Name",req.Name),
                new SqlParameter("@Remark",req.Remark??""),
                new SqlParameter("@Prefix",req.Prefix),
            };
            var resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    resp.Statuscode = dt.Rows[0][0] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0][0]);
                    resp.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
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
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return resp;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "proc_SaveDepartmentRole";
        }
    }
}
