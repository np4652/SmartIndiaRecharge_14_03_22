using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
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
    public class ProcDTHConnectionService : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcDTHConnectionService(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var req = (DTHConnectionServiceRequest)obj;
            SqlParameter[] param = {
                new SqlParameter("@UserID",req.UserID),
                new SqlParameter("@PID",req.PID),
                new SqlParameter("@RequestModeID",req.RequestModeID),
                new SqlParameter("@CustomerNumber",req.CustomerNumber??string.Empty),
                new SqlParameter("@Customer",req.Customer??string.Empty),
                new SqlParameter("@Address",req.Address??string.Empty),
                new SqlParameter("@Pincode",req.Pincode??string.Empty),
                new SqlParameter("@APIRequestID",req.APIRequestID??string.Empty),
                new SqlParameter("@RequestIP",req.RequestIP??string.Empty),
                new SqlParameter("@IMEI",req.IMEI??string.Empty),
                new SqlParameter("@SecurityKey",HashEncryption.O.Encrypt(req.SecurityKey??string.Empty)),
                new SqlParameter("@Gender",req.Gender??string.Empty),
                new SqlParameter("@AreaID",req.AreaID),
            };
            var res = new DTHConnectionServiceResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param).ConfigureAwait(false);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = dt.Rows[0][0] is DBNull ? res.Statuscode : Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? string.Empty : dt.Rows[0]["Msg"].ToString();
                    if (res.Statuscode == ErrorCodes.One)
                    {
                        res.TID = dt.Rows[0]["TID"] is DBNull ? res.Statuscode : Convert.ToInt32(dt.Rows[0]["TID"]);
                        res.Balance = dt.Rows[0]["Balance"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["Balance"]);
                        res.TransactionID = dt.Rows[0]["TransactionID"] is DBNull ? string.Empty : dt.Rows[0]["TransactionID"].ToString();
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

        public Task<object> Call() => throw new NotImplementedException();

        public string GetName() => "proc_DTHConnectionService"; //
    }
}
