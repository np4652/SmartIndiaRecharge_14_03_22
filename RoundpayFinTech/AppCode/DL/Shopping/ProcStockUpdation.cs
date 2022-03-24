using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;


namespace RoundpayFinTech.AppCode.DL.Shopping
{
    public class ProcStockUpdation : IProcedure
    {
        private readonly IDAL _dal;
        public ProcStockUpdation(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            SqlParameter[] param = {
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@ProductDetailID",req.CommonInt),
                new SqlParameter("@Quantity",req.CommonInt2),
                new SqlParameter("@Remark",req.CommonStr??"")
            };

            try
            {
                var dt = _dal.Get(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    res.Msg = Convert.ToString(dt.Rows[0]["Msg"]);
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
        public object Call() => throw new NotImplementedException();
        public string GetName() => @"insert into tbl_ProductStockDetails(_VariantId,_Qty,_TrType,_Remarks,_CreatedBy,_CreatedDate) values(@ProductDetailID,@Quantity,1,@Remark,@LoginID,getDate());select 1,'Stock updated'Msg";
    }
}
