using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Shopping;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcAddToCart : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcAddToCart(IDAL dal) => _dal = dal;        
        public async Task<object> Call(object obj)
        {
            var req = (AddToCartRequest)obj;
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            SqlParameter[] param = {
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@PID",req.ProductID),
                new SqlParameter("@ProductDetailID",req.ProductDeatilID),
                //new SqlParameter("@FilterIds",req.FilterIds??""),
                //new SqlParameter("@OptionIds",req.OptionIds??""),
                new SqlParameter("@Quantity",req.Quantity)
            };
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
                    if(res.Statuscode==ErrorCodes.One)
                        res.CommonInt = dt.Rows[0]["_Count"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_Count"]);
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
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }
        public Task<object> Call() => throw new NotImplementedException();
        public string GetName() => "proc_AddToCart";
    }
}