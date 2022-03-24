using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcLogAPICommonReqResp : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcLogAPICommonReqResp(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var req = (LogAPICommonReqRespModel)obj;
            SqlParameter[] param = {
                new SqlParameter("@Request", req._Request),
                new SqlParameter("@Response", req._Response),
                new SqlParameter("@ClassName", req._ClassName),
                new SqlParameter("@Method", req._Method)
            };
            try
            {
                await _dal.ExecuteAsync(GetName(), param).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
            }
            return true;
        }

        public Task<object> Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "insert into Log_APICommonReqResp(_Request, _Response, _ClassName, _Method, _EntryDate)values(@Request, @Response, @ClassName, @Method, getdate())";
        }
    }
}
