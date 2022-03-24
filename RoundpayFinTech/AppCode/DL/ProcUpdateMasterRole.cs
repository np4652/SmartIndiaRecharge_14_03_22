using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
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
    public class ProcUpdateMasterRole : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateMasterRole(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            
            var req = (CommonReq)obj;
            SqlParameter[] param =
                 {
                new SqlParameter("@Id",req.CommonInt),
                new SqlParameter("@RegCharge",req.CommonInt2),
                new SqlParameter("@LT",req.CommonInt3),
                new SqlParameter("@LoginId",req.CommonInt4)
            };
            var res = new ResponseStatus
            {
                Statuscode=ErrorCodes.Minus1,
                Msg=ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? "": dt.Rows[0]["Msg"].ToString();
                }
            }
            catch (Exception ex)
            {
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.CommonInt,
                    UserId = req.CommonInt2
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "Proc_UpdateMasterRole";
    }
}
