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
    public class ProcUpdateRailwayService : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateRailwayService(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (UpdateRailServiceProcReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@TID",req.TID),
                new SqlParameter("@IRSaveID",req.IRSaveID),
                new SqlParameter("@Type",req.Type),
                new SqlParameter("@AmountR",req.AmountR),
                new SqlParameter("@AccountNo",req.AccountNo??string.Empty)
            };
            var res = new _CallbackData
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = dt.Rows[0][0] is DBNull ? res.Statuscode : Convert.ToInt32(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? string.Empty : dt.Rows[0]["Msg"].ToString();
                    if (res.Statuscode == ErrorCodes.One)
                    {
                        res.UpdateUrl = dt.Rows[0]["_CallbackURL"] is DBNull ? string.Empty : dt.Rows[0]["_CallbackURL"].ToString();
                        res.TransactionID = dt.Rows[0]["TransactionID"] is DBNull ? string.Empty : dt.Rows[0]["TransactionID"].ToString();
                        res.UserID = dt.Rows[0]["_UserID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_UserID"]);
                    }
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
        public string GetName() => "Proc_UpdateRailwayService";
    }
}
