using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcUpdateDeptMapNumber : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateDeptMapNumber(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (WhatsappAPIDetail)obj;
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            SqlParameter[] param = {
                 new SqlParameter("@LoginID",req.LoginID),
                 new SqlParameter("@LT",req.LT),
                 new SqlParameter("@_DeptId",req.DEPID??string.Empty),
                 new SqlParameter("@MobileNum",req.Mobileno??string.Empty),
                 new SqlParameter("@ID",req.ID)
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
                    LoginTypeID = req.LT,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }
        public object Call() => throw new NotImplementedException();
        // public string GetName() => "proc_SMSAPI_CU";
        public string GetName() => "Proc_UpdateDeptMapNumber";

    }
}
