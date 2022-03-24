using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.Employee;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL.Employee
{
    public class ProcGetEmpByID : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetEmpByID(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            EmpReq _EmpRequset = (EmpReq)obj;
            SqlParameter[] param ={
             new SqlParameter("@LoginID", _EmpRequset.LoginID),
             new SqlParameter("@LtID", _EmpRequset.LTID),
             new SqlParameter("@EmpID", _EmpRequset.EmpID),
             new SqlParameter("@MobileNo", _EmpRequset.MobileNo ?? "")
        };
            IResponseStatus _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _res.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _res.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
                    if (_res.Statuscode == ErrorCodes.One)
                    {
                        _res.CommonInt2 = dt.Rows[0]["EmpID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["EmpID"]);
                        _res.CommonStr = dt.Rows[0]["MobileNo"].ToString();
                        _res.CommonStr2 = dt.Rows[0]["Name"].ToString();
                        _res.CommonInt = dt.Rows[0]["EmpRoleID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["EmpRoleID"]);
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
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _res;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetEmpByID";
    }
}
