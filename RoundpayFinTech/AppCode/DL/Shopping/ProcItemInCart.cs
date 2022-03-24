using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Shopping;
using System;
using System.Data.SqlClient;



namespace RoundpayFinTech.AppCode.DL.Shopping
{
    public class ProcItemInCart : IProcedure
    {
        private readonly IDAL _dal;
        public ProcItemInCart(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var LoginID = (int)obj;
            var res = new ItemInCart
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            SqlParameter[] param = {
                new SqlParameter("@LoginID",LoginID)
            };
            try
            {
                var dt =  _dal.Get(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.TQuantity = dt.Rows[0]["_Quantity"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_Quantity"]);
                    res.TCost = dt.Rows[0]["_TCost"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["_TCost"]);
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = ErrorCodes.SUCCESS;
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
                    UserId = LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }
        public object Call() => throw new NotImplementedException();

        public string GetName() => "select count(*) _Quantity,SUM(p._SellingPrice*c._Quantity) _TCost from tbl_Cart c inner join tbl_ProductDetail p on p._ID=c._ProductDetailID and _UserID=@LoginID";
    }

    public class ProcItemInWishlist : IProcedure
    {
        private readonly IDAL _dal;
        public ProcItemInWishlist(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var LoginID = (int)obj;
            var res = new ItemInCart
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            SqlParameter[] param = {
                new SqlParameter("@LoginID",LoginID)
            };
            try
            {
                var dt = _dal.Get(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.TQuantity = dt.Rows[0]["ItemCount"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["ItemCount"]);
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = ErrorCodes.SUCCESS;
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
                    UserId = LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }
        public object Call() => throw new NotImplementedException();

        public string GetName() => "select Count(*) as ItemCount from tbl_WishList where _UserID=@LoginID";
    }
}