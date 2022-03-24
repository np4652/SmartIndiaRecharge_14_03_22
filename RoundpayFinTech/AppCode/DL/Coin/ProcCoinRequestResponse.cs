using Fintech.AppCode.DB;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL.Coin
{
    public class ProcCoinRequestResponse
    {
        private readonly IDAL _dal;
        public ProcCoinRequestResponse(IDAL dal) => _dal = dal;

        public void SaveAPIReqResp(string Method,string Req, string Resp) {
            SqlParameter[] param = { 
                new SqlParameter("@Method",Method??string.Empty),
                new SqlParameter("@Req",Req??string.Empty),
                new SqlParameter("@Resp",Resp??string.Empty)
            };
            _dal.Execute("INSERT INTO Log_CoinAPIReqResp(_Method,_Request,_Response,_EntryDate)VALUES(@Method,@Req,@Resp,getdate())", param);
        }
    }
}
