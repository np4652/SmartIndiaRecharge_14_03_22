using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetApiOperatorOptionalMapping: IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetApiOperatorOptionalMapping(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (GetApiOptionalParam)obj;
            SqlParameter[] param = {
                new SqlParameter("@OID",req._OID),
                new SqlParameter("@ApiID",req._APIID),
                new SqlParameter("@LoginID",req.LoginID),
            };
            var resp = new ApiOperatorOptionalMappingModel();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count>0)
                {
                    resp.StatusCode = ErrorCodes.One;
                    resp.Msg = ErrorCodes.SUCCESS;
                    resp._APIID = Convert.ToInt32(dt.Rows[0]["_APIID"]);
                    resp._OID = Convert.ToInt32(dt.Rows[0]["_OID"]);
                    resp._Key1 =  dt.Rows[0]["_Key1"].ToString();
                    resp._Key2 =  dt.Rows[0]["_Key2"].ToString();
                    resp._Key3 = dt.Rows[0]["_Key3"].ToString();
                    resp._Key4 =  dt.Rows[0]["_Key4"].ToString();
                    resp._Value1 = dt.Rows[0]["_Value1"].ToString();
                    resp._Value2 = dt.Rows[0]["_Value2"].ToString();
                    resp._Value3 =  dt.Rows[0]["_Value3"].ToString();
                    resp._Value4 =  dt.Rows[0]["_Value4"].ToString();
                    resp._EntryDate = dt.Rows[0]["_EntryDate"].ToString();
                }
            }
            catch (Exception er)
            {
            }
            return resp;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_Get_ApiOperatorOptionalMapping";
    }
}
