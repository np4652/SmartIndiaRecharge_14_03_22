using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.App;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcValidateUPIOrder : IProcedure
    {
        private readonly IDAL _dal;
        public ProcValidateUPIOrder(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (GenerateOrderUPIRequest)obj;
            SqlParameter[] param = {
                new SqlParameter("@Amount",req.Amount),
                new SqlParameter("@UserID",req.UserID),
                new SqlParameter("@SessionID",req.SessionID),
                new SqlParameter("@UPIID",req.UPIID??string.Empty),
                new SqlParameter("@RequestIP",req.RequestIP??string.Empty),
                new SqlParameter("@Browser",req.Browser??string.Empty),
                new SqlParameter("@IMEI",req.IMEI??string.Empty),
                new SqlParameter("@AppVersion",req.AppVersion??string.Empty),
                new SqlParameter("@OrderID",req.OrderID)
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
                        res.CommonStr = dt.Rows[0]["_OrderKey"] is DBNull ? string.Empty : dt.Rows[0]["_OrderKey"].ToString();
                    }
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
                    UserId = req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_ValidateUPIOrder";
    }
}
