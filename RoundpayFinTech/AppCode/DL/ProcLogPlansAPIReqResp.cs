using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcLogPlansAPIReqResp : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcLogPlansAPIReqResp(IDAL dal)
        {
            _dal = dal;
        }
        public async Task<object> Call(object obj)
        {
            CommonReq req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@Method",req.CommonStr??""),
                new SqlParameter("@Req",req.CommonStr2??""),
                new SqlParameter("@Resp",req.CommonStr3??"")
            };
            try
            {
                await _dal.ExecuteAsync(GetName(), param).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
            }
            return 0;
        }

        public Task<object> Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "insert into Log_PlansAPIReqResp(_Method,_Req,_Resp,_EntryDate) values(@Method,@Req,@Resp,getdate())";
        }
    }
}
