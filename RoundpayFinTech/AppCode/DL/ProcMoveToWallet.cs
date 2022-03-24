using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Configuration;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcMoveToWallet : IProcedure
    {
        private readonly IDAL _dal;
        public ProcMoveToWallet(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            var _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@LoginId", _req.LoginID),
                new SqlParameter("@ActionType", _req.CommonInt),
                new SqlParameter("@RequestedAmount", _req.CommonDecimal),
                new SqlParameter("@TransMode", _req.CommonStr2)
              };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt32(dt.Rows[0][0].ToString());
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
                    if (res.Statuscode == ErrorCodes.One && dt.Columns.Contains("_WalletRequestID") && ApplicationSetting.IsRealSettlement)
                    {
                        res.CommonInt = dt.Rows[0]["_WalletRequestID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_WalletRequestID"]);
                        res.CommonBool = dt.Rows[0]["_InRealTime"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_InRealTime"]);
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
                    LoginTypeID = _req.LoginTypeID,
                    UserId = _req.LoginID
                });
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "Proc_MoveToWallet";
    }
}
