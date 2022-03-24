using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.Coin;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcCoinCreditService : IProcedure
    {
        private readonly IDAL _dal;
        public ProcCoinCreditService(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CoinCreditServiceProcRequest)obj;
            SqlParameter[] param = {
                new SqlParameter("@UserID",req.UserID),
                new SqlParameter("@SenderAddrss",req.SenderAddrss??string.Empty),
                new SqlParameter("@ReceiverAddress",req.ReceiverAddress??string.Empty),
                new SqlParameter("@AmountInCoin",req.AmountInCoin),
                new SqlParameter("@AmountType",req.AmountType??string.Empty),
                new SqlParameter("@AmountInINR",req.AmountInINR),
                new SqlParameter("@OID",req.OID),
            };

            var res = new CoinCreditServiceProcResponse { 
            Statuscode=ErrorCodes.Minus1,
            Msg=ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0) {
                    res.Statuscode = dt.Rows[0][0] is DBNull ? ErrorCodes.Minus1 : Convert.ToInt32(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? string.Empty : dt.Rows[0]["Msg"].ToString();
                    if (res.Statuscode == ErrorCodes.One) { 
                        res.TID= dt.Rows[0]["_TID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_TID"]);
                        res.TransactionID= dt.Rows[0]["_TransactionID"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["_TransactionID"]);
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

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_CoinCreditService";
    }
}
