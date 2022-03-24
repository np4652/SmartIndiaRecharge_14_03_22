using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcSaveApiOptionalParam : IProcedure
    {
        private readonly IDAL _dal;
        public ProcSaveApiOptionalParam(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var req = (ApiOperatorOptionalMappingModel)obj;
            SqlParameter[] param = {
                new SqlParameter("@OID",req._OID),
                new SqlParameter("@ApiID",req._APIID),
                new SqlParameter("@Key1",req._Key1??string.Empty),
                new SqlParameter("@Key2",req._Key2??string.Empty),
                new SqlParameter("@Key3",req._Key3??string.Empty),
                new SqlParameter("@Key4",req._Key4??string.Empty),
                new SqlParameter("@Value1",req._Value1??string.Empty),
                new SqlParameter("@Value2",req._Value2??string.Empty),
                new SqlParameter("@Value3",req._Value3??string.Empty),
                new SqlParameter("@Value4",req._Value4??string.Empty),
                new SqlParameter("@LoginID",req.LoginID)
            };
            var resp = new ResponseStatus();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count>0)
                {
                    resp.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    resp.Msg = dt.Rows[0][1].ToString();
                }
            }
            catch (Exception er)
            {
            }
            return resp;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_Save_ApiOperatorOptionalMapping";
    }
}
