using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Shopping;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL.Shopping
{
    public class ProcPlaceOrder : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcPlaceOrder(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var req = (PlaceOrder)obj;
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            SqlParameter[] param = {
                new SqlParameter("@LoginID",req.UserID),
                new SqlParameter("@AddressID",req.AddressID),
                new SqlParameter("@PDeduction",req.PDeduction),
                new SqlParameter("@SDeduction",req.SDeduction)
            };
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
                    if(Convert.ToInt16(dt.Rows[0][0]) == 1)
                    {
                        res.CommonStr = dt.Rows[0]["OrderId"].ToString();
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
                    UserId = req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }
        public Task<object> Call() => throw new NotImplementedException();
        public string GetName() => "proc_PlaceOrder";
    }
}