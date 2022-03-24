using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcUserBulkAction : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUserBulkAction(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (BulkAct)obj;
            IResponseStatus res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            SqlParameter[] param = {
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@LTID",req.LTID),
                new SqlParameter("@ActionID",req.Act.ActionID),
                new SqlParameter("@WalletType",req.Act.WalletType),
                new SqlParameter("@Amount",req.Act.Amount),
                new SqlParameter("@Status",req.Act.Status),
                new SqlParameter("@IntoID",req.IntoID),//CurrentIntro
                new SqlParameter("@RoleID",req.Act.RoleID),
                new SqlParameter("@Users",req.Act.Users),
                new SqlParameter("@IsAll",req.Act.IsAll),
                new SqlParameter("@IsWhole",req.Act.IsWhole),
                new SqlParameter("@ToIntro",req.Act.ToIntro)
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LTID,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_UserBulkAction";
    }
}
