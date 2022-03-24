using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetApiRequestReponse : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcGetApiRequestReponse(IDAL dal) => _dal = dal;

        public async Task<object> Call(object obj)
        {
            CommonReq req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", req.LoginTypeID),
                new SqlParameter("@LoginID", req.LoginID),
                new SqlParameter("@TID", req.CommonInt)
            };
            
            var list = new List<ProcApiURlRequestResponse>();
            ProcApiURlRequestResponse _res = new ProcApiURlRequestResponse()
            {
               ResultCode = ErrorCodes.Minus1,
                Msg = "Something went wrong!"
            };
            try
            {
                DataTable dt = await _dal.GetByProcedureAsync(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        ProcApiURlRequestResponse item = new ProcApiURlRequestResponse();
                        item.ResultCode = 1;
                        item._Request = dt.Rows[i]["_Request"].ToString();
                        item._Response = dt.Rows[i]["_Response"].ToString();
                        item._ResponseTime = dt.Rows[i]["_ModifyDate"].ToString();
                        list.Add(item);
                    }
                }
                else
                {
                    list.Add(_res);
                }
            }
            catch (Exception ex)
            { }
            return list;
        }

        public Task<object> Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "Proc_GetAPiUrlResponse";
    }
}
