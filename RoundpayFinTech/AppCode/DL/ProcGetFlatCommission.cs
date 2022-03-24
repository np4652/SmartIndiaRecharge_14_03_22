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
    public class ProcGetFlatCommission : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetFlatCommission(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
            new SqlParameter("@LT",req.LoginTypeID),
            new SqlParameter("@LoginID",req.LoginID),
            new SqlParameter("@UserID",req.CommonInt)
            };
            var res = new List<FlatCommissionDetail>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow item in dt.Rows)
                    {
                        res.Add(new FlatCommissionDetail
                        {
                            RoleID = item["_ID"] is DBNull ? 0 : Convert.ToInt16(item["_ID"]),
                            Role = item["_Role"] is DBNull ? string.Empty : Convert.ToString(item["_Role"]),
                            CommRate = item["_CommRate"] is DBNull ? 0 : Convert.ToDecimal(item["_CommRate"]),
                            LastModified = item["_ModifyDate"] is DBNull ? string.Empty : Convert.ToString(item["_ModifyDate"]),
                            UserID = item["_UserID"] is DBNull ? 0 : Convert.ToInt32(item["_UserID"]),
                        });
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_GetFlatCommission";
    }
}
