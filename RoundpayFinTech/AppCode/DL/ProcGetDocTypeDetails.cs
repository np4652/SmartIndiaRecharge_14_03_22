using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetDocTypeDetails : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetDocTypeDetails(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            CommonReq req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", req.LoginTypeID),
                new SqlParameter("@LoginID", req.LoginID)
            };
            

            var list = new List<DocTypeMaster>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DocTypeMaster item = new DocTypeMaster
                        {
                            StatusCode = 1,
                            Description = "Record Found",
                            ID = Convert.ToInt16(dt.Rows[i]["_ID"].ToString()),
                            ModifyDate = dt.Rows[i]["_ModifyDate"].ToString(),
                            Remark = dt.Rows[i]["_Remark"].ToString(),
                            DocName = dt.Rows[i]["_DocName"].ToString(),
                            IsOptional = Convert.ToBoolean(dt.Rows[i]["_IsOptional"].ToString())
                        };
                        list.Add(item);
                    }

                }
            }
            catch (Exception)
            { }
            return list;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "proc_GetDocTypeDetails";
        }
    }
}