using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetAPIOpCodePivot : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetAPIOpCodePivot(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@OpTypeID", req.CommonInt),
                new SqlParameter("@LoginID", req.LoginID),
            };
            var aPIOpCodes = new List<APIOpCode>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    string[] cols = { "OID", "OpType", "Operator" };
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        var aPIOpCode = new APIOpCode
                        {
                            OID = Convert.ToInt32(dt.Rows[i]["OID"]),
                            OpType = dt.Rows[i]["OpType"].ToString(),
                            Operator = dt.Rows[i]["Operator"].ToString()
                        };
                        IDictionary<string, string> dict = new Dictionary<string, string>();
                        foreach (DataColumn api in dt.Columns)
                        {
                            if (!cols.Contains(api.ColumnName) && api.ColumnName.Contains("~"))
                            {
                                dict.Add(new KeyValuePair<string, string>(api.ColumnName, (dt.Rows[i][api.ColumnName] is DBNull ? "" : dt.Rows[i][api.ColumnName].ToString())));
                            }
                        }
                        aPIOpCode.APIs = dict;
                        aPIOpCodes.Add(aPIOpCode);
                    }
                }
            }
            catch (Exception)
            {
            }
            return aPIOpCodes;
            
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetAPIOpCodePivot";
    }
}
