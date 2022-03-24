using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Shopping;
using System;
using System.Data;
using System.Data.SqlClient;


namespace RoundpayFinTech.AppCode.DL.Shopping
{
    public class ProcUpdateShoppingComm : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateShoppingComm(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (ShoppingCommissionReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@LT",req.LT),
                new SqlParameter("@SlabID",req.SlabID),
                new SqlParameter("@SubCategoryID",req.CategoryID),
                new SqlParameter("@Comm",req.Commission),
                new SqlParameter("@commType",req.CommType),
                new SqlParameter("@RoleID",req.RoleID)
            };
            var res = new ResponseStatus();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
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
                    LoginTypeID = req.LT,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_UpdateShoppingComm";
    }
}