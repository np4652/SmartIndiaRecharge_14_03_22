using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.Coin;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL.Coin
{
    public class ProcCheckCoinGenerationAbility : IProcedure
    {
        private readonly IDAL _dal;
        public ProcCheckCoinGenerationAbility(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@SPKey",req.CommonStr??string.Empty),
                new SqlParameter("@OID",req.CommonInt)
            };
            var res = new CoinAddressDetail
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
                    if (res.Statuscode == ErrorCodes.One) { 
                        res.CoinAddress= dt.Rows[0]["_CoinAddress"] is DBNull ? string.Empty : dt.Rows[0]["_CoinAddress"].ToString();
                        res.CoinToken= dt.Rows[0]["_CoinToken"] is DBNull ? string.Empty : dt.Rows[0]["_CoinToken"].ToString();
                        res.CoinHexAddress= dt.Rows[0]["_CoinHexAddress"] is DBNull ? string.Empty : dt.Rows[0]["_CoinHexAddress"].ToString();
                        res.CoinSmartContractBalance= dt.Rows[0]["_CoinSmartContractBalance"] is DBNull ? 0L : Convert.ToInt64(dt.Rows[0]["_CoinSmartContractBalance"]);
                        res.CoinLastBalance= dt.Rows[0]["_CoinLastBalance"] is DBNull ? 0L : Convert.ToInt64(dt.Rows[0]["_CoinLastBalance"]);
                        res.OID= dt.Rows[0]["_OID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_OID"]);
                        res.IsFrozen= dt.Rows[0]["_IsFrozen"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsFrozen"]);
                        res.CoinReceiverAddress = dt.Rows[0]["_CoinReceiverAddress"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["_CoinReceiverAddress"]);
                        res.SPKey = dt.Rows[0]["_SPKey"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[0]["_SPKey"]);
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

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_CheckCoinGenerationAbility";
    }
}
