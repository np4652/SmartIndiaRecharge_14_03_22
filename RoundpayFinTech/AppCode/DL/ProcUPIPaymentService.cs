using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcUPIPaymentService : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUPIPaymentService(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (UPIPaymentProcReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@UserID",req.UserID),
                new SqlParameter("@AccountNo",req.AccountNo??string.Empty),
                new SqlParameter("@AmountR",req.AmountR),
                new SqlParameter("@APIRequestID",req.APIRequestID??string.Empty),
                new SqlParameter("@IPAddress",req.IPAddress??string.Empty),
                new SqlParameter("@RequestMode",req.RequestMode)
            };
            var res = new UPIPaymentProcRes
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
                    res.ErrorCode = dt.Rows[0]["_ErrorCode"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_ErrorCode"]);
                    if (res.Statuscode == ErrorCodes.One)
                    {
                        res.TID = dt.Rows[0]["TID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["TID"]);
                        res.TransactionID = dt.Rows[0]["TransactionID"] is DBNull ? string.Empty : dt.Rows[0]["TransactionID"].ToString();
                        res.Balance = dt.Rows[0]["Balance"] is DBNull ? 0M : Convert.ToDecimal(dt.Rows[0]["Balance"]);
                        res.APICode = dt.Rows[0]["_APICode"] is DBNull ? string.Empty : dt.Rows[0]["_APICode"].ToString();
                        res.Latlong = dt.Rows[0]["_Latlong"] is DBNull ? string.Empty : dt.Rows[0]["_Latlong"].ToString();
                        res.APIID = dt.Rows[0]["_APIID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_APIID"]);
                        res.EmailID = dt.Rows[0]["_EmailID"] is DBNull ? string.Empty : dt.Rows[0]["_EmailID"].ToString();
                        res.OutletMobile = dt.Rows[0]["_OutletMobile"] is DBNull ? string.Empty : dt.Rows[0]["_OutletMobile"].ToString();
                        res.OpCode = dt.Rows[0]["_OpCode"] is DBNull ? string.Empty : dt.Rows[0]["_OpCode"].ToString();
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
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = req.UserID
                });
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "Proc_UPIPaymentService";
    }
}
