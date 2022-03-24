using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Recharge;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcUpdateBBPSComplain : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateBBPSComplain(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (BBPSComplainAPIResponse)obj;
            SqlParameter[] param = {
                new SqlParameter("@TableID",req.TableID),
                new SqlParameter("@UserID",req.UserID),
                new SqlParameter("@ComplainStatus",req.Statuscode),
                new SqlParameter("@LiveID",req.LiveID??string.Empty),
                new SqlParameter("@Remark",req.Remark??string.Empty),
                new SqlParameter("@Request",req.Request??string.Empty),
                new SqlParameter("@Response",req.Response??string.Empty)
            };
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg=ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = dt.Rows[0][0] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0][0]);
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
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = req.UserID
                });
            }
            return res;
        }
        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_UpdateBBPSComplain";
    }
}
