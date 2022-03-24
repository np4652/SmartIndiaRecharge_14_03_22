using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetAPIOpCodeCircle : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetAPIOpCodeCircle(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@APIID",req.CommonInt),
                new SqlParameter("@OID",req.CommonInt2)
            };
            var res = new List<APIOpCode>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow item in dt.Rows)
                    {
                        res.Add(new APIOpCode
                        {
                            APIID=item["_APIID"] is DBNull ? 0 :Convert.ToInt32(item["_APIID"]),
                            OID = item["_OID"] is DBNull ? 0 : Convert.ToInt32(item["_OID"]),
                            CircleID = item["_CircleID"] is DBNull ? 0 : Convert.ToInt32(item["_CircleID"]),
                            Circle = item["_Circle"] is DBNull ? string.Empty : item["_Circle"].ToString(),
                            OpCode = item["_OpCode"] is DBNull ? string.Empty : item["_OpCode"].ToString(),
                            ModifyDate = item["_ModifyDate"] is DBNull ? string.Empty : item["_ModifyDate"].ToString()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_GetAPIOpCodeCircle";
    }
}
