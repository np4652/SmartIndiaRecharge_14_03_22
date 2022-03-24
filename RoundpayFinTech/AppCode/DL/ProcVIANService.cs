using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcVIANService : IProcedure
    {
        private readonly IDAL _dal;
        public ProcVIANService(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (VIANCallbackRequest)obj;
            SqlParameter[] param = {
                new SqlParameter("@MVBID",req.MVBID),
                new SqlParameter("@MVBToken",req.MVBToken??string.Empty),
                new SqlParameter("@AgentID",req.AgentID??string.Empty),
                new SqlParameter("@VIAN",req.VIAN??string.Empty),
                new SqlParameter("@Amount",req.Amount),
                new SqlParameter("@Operation",req.Operation??string.Empty),
                new SqlParameter("@IPAddress",req.IPAddress??string.Empty),
                new SqlParameter("@Browser",req.Browser??string.Empty)
            };
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.GetByProcedureAdapter(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = dt.Rows[0][0] is DBNull ? res.Statuscode : Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? string.Empty : dt.Rows[0]["Msg"].ToString();
                    if (res.Statuscode == ErrorCodes.One)
                    {
                        res.CommonStr = dt.Rows[0]["_TransactionID"] is DBNull ? string.Empty : dt.Rows[0]["_TransactionID"].ToString();
                        res.CommonStr2 = dt.Rows[0]["UserName"] is DBNull ? string.Empty : dt.Rows[0]["UserName"].ToString();
                        res.CommonStr3 = dt.Rows[0]["MobileNo"] is DBNull ? string.Empty : dt.Rows[0]["MobileNo"].ToString();
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
                    UserId = 1
                });
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_VIANService";
    }
}
