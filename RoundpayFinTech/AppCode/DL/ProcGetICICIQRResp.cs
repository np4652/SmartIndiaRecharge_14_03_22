using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetICICIQRResp : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetICICIQRResp(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@ICICIQRRef",req.CommonStr??string.Empty)
            };
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = dt.Rows[0][0] is DBNull ? res.Statuscode : Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? string.Empty : dt.Rows[0]["Msg"].ToString();
                    if (res.Statuscode == ErrorCodes.One)
                    {
                        res.CommonStr = dt.Rows[0]["_ICICIQRRef"] is DBNull ? string.Empty : dt.Rows[0]["_ICICIQRRef"].ToString();
                        res.CommonStr2 = dt.Rows[0]["_TransactionID"] is DBNull ? string.Empty : dt.Rows[0]["_TransactionID"].ToString();
                        res.CommonStr3 = dt.Rows[0]["_MobileNo"] is DBNull ? string.Empty : dt.Rows[0]["_MobileNo"].ToString();
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
                    UserId = req.LoginID
                });
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_GetICICIQRResp";
    }
}
