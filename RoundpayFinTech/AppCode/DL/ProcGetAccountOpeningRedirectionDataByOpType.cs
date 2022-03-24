using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetAccountOpeningRedirectionDataByOpType : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetAccountOpeningRedirectionDataByOpType(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = { 
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@opTypeID",req.CommonInt)
            };
            var res = new List<AccountOpData>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(),param);
                if (dt.Rows.Count > 0) {
                    if (!dt.Columns.Contains("Msg")) {
                        foreach (DataRow item in dt.Rows)
                        {
                            res.Add(new AccountOpData
                            {
                                OID = item["_OID"] is DBNull ? 0 : Convert.ToInt32(item["_OID"]),
                                Content = item["_Content"] is DBNull ? string.Empty : item["_Content"].ToString(),
                                RedirectURL = item["_RedirectURL"] is DBNull ? string.Empty : item["_RedirectURL"].ToString(),
                                Name= item["_Name"] is DBNull ? string.Empty : item["_Name"].ToString()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_GetAccountOpeningRedirectionDataByOpType";
    }
}
