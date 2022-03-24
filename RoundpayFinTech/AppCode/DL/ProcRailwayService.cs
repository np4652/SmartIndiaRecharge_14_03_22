using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcRailwayService : IProcedure
    {
        private readonly IDAL _dal;
        public ProcRailwayService(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (RailwayServiceProcReq)obj;
            SqlParameter[] param = {
                    new SqlParameter("@OutletID",req.OutletID),
                    new SqlParameter("@IRSaveID",req.IRSaveID),
                    new SqlParameter("@UserID",req.UserID),
                    new SqlParameter("@APICode",req.APICode??string.Empty),
                    new SqlParameter("@AccountNo",req.AccountNo??string.Empty),
                    new SqlParameter("@AmountR",req.AmountR),
                    new SqlParameter("@APIRequestID",req.APIRequestID??string.Empty),
                    new SqlParameter("@VendorID",req.VendorID??string.Empty),
                    new SqlParameter("@IsExternal",req.IsExternal),
                    new SqlParameter("@RequestIP",req.RequestIP??string.Empty)
            };
            var res = new RailwayServiceProcRes
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
                        res.TID = dt.Rows[0]["TID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["TID"]);
                        res.TransactionID = dt.Rows[0]["TransactionID"] is DBNull ? string.Empty : dt.Rows[0]["TransactionID"].ToString();
                        res.DebitURLRail = dt.Rows[0]["_DebitURLRail"] is DBNull ? string.Empty : dt.Rows[0]["_DebitURLRail"].ToString();
                        res.Balance = dt.Rows[0]["Balance"] is DBNull ? 0M : Convert.ToInt32(dt.Rows[0]["Balance"]);
                        res.IsExternal = dt.Rows[0]["_IsExternal"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsExternal"]);
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
                    UserId = req.UserID
                });
            }
            return res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "Proc_RailwayService";
    }
}
