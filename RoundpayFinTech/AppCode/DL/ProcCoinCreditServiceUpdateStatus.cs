using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.Coin;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcCoinCreditServiceUpdateStatus : IProcedure
    {
        private readonly IDAL _dal;
        public ProcCoinCreditServiceUpdateStatus(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req =(CoinCreditServiceUpdateProcReq)obj;
            SqlParameter[] param = { 
                new SqlParameter("@TID",req.TID),
                new SqlParameter("@Status",req.Status),
                new SqlParameter("@VendorID",req.VendorID??string.Empty),
                new SqlParameter("@LiveID",req.LiveID??string.Empty),
                new SqlParameter("@IP",req.IP??string.Empty),
                new SqlParameter("@Browser",req.Browser??string.Empty)
            };
            var res = new ResponseStatus
            {
                Statuscode=ErrorCodes.Minus1,
                Msg=ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(),param);
                if (dt.Rows.Count > 0) {
                    res.Statuscode = dt.Rows[0][0] is DBNull ? res.Statuscode : Convert.ToInt16(dt.Rows[0][0]);
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
                    UserId = req.TID
                });
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_CoinCreditServiceUpdateStatus";
    }
}
