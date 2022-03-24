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
    public class ProcUpdatePSAFromMachine : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdatePSAFromMachine(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (_CallbackData)obj;
            SqlParameter[] param = { 
                new SqlParameter("@OutletAPIID",req.TID),
                new SqlParameter("@PSAID",req.AccountNo??string.Empty),
                new SqlParameter("@Status",req.Statuscode),
                new SqlParameter("@Remark",req.LiveID??string.Empty),
                new SqlParameter("@IP",req.RequestIP??string.Empty),
                new SqlParameter("@Browser",req.Browser??string.Empty)
            };
            var res = new ResponseStatus {
            Statuscode=ErrorCodes.Minus1,
            Msg=ErrorCodes.TempError            
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = dt.Rows[0][0] is DBNull ? res.Statuscode : Convert.ToInt32(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? string.Empty : dt.Rows[0]["Msg"].ToString();
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
                    UserId = 1
                });
            }
            return res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_UpdatePSAFromMachine";
    }
}
