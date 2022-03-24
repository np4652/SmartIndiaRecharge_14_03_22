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
    public class ProcDeleteProductDetail : IProcedure
    {
        private readonly IDAL _dal;
        public ProcDeleteProductDetail(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            var res = new ResponseStatus
            {
                Statuscode=ErrorCodes.Minus1,
                Msg=ErrorCodes.TempError
            };
            SqlParameter[] param = {                
                new SqlParameter("@ProductDetailID",req.CommonInt),
                new SqlParameter("@IsDeleted",req.CommonBool)
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
        public string GetName() => "update tbl_ProductDetail set _IsDeleted=@IsDeleted where _ID=@ProductDetailID ;select 1 ,'Deleted successfully'Msg";
    }
}
