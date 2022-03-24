using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Shopping;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL.Shopping
{
    public class ProcOnChangeFilter : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcOnChangeFilter(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var req = (AddToCartRequest)obj;
            var res = new OnChangeFilter
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            SqlParameter[] param = {
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@PID",req.ProductID),
                new SqlParameter("@ProductDetailID",req.ProductDeatilID),
                new SqlParameter("@FilterIds",req.FilterIds??"")
            };
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
                    res.ProductDetailID = Convert.ToInt16(dt.Rows[0]["_ProductDetailID"]);
                    res.Quantity = Convert.ToInt16(dt.Rows[0]["_Quantity"]);
                    res.SellingPrice = Convert.ToDecimal(dt.Rows[0]["_SellingPrice"]);
                    res.Mrp= Convert.ToDecimal(dt.Rows[0]["_MRP"]);
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
        public string GetName() => "Proc_OnChangeFilter";
    }
}