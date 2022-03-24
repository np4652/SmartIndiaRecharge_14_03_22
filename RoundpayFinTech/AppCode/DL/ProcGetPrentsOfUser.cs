using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetPrentsOfUser : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcGetPrentsOfUser(IDAL dal) => _dal = dal;
        
        public async Task<object> Call(object obj)
        {
            CommonReq req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@UserID",req.CommonInt)
            };
            List<UserInfo> resp = new List<UserInfo>();
            try
            {
                DataTable dt = await _dal.GetByProcedureAsync(GetName(),param);
                if (dt.Rows.Count > 0) {
                    foreach (DataRow row in dt.Rows)
                    {
                        var userInfo = new UserInfo
                        {
                            UserID = row["_ID"] is DBNull?0:Convert.ToInt32(row["_ID"]),
                            Prefix = row["_Prefix"] is DBNull ?"" : row["_Prefix"].ToString(),
                            Role = row["_Role"] is DBNull ? "" : row["_Role"].ToString(),
                            RoleID = row["_RoleID"] is DBNull ? 0 : Convert.ToInt32(row["_RoleID"]),
                            OutletName = row["_OutletName"] is DBNull ? "" : row["_OutletName"].ToString(),
                            MobileNo = row["_MobileNo"] is DBNull ? "" : row["_MobileNo"].ToString()
                        };
                        resp.Add(userInfo);
                    }
                }
            }
            catch (Exception)
            {
            }
            return resp;
        }

        public Task<object> Call() => throw new NotImplementedException();


        public string GetName() => "proc_GetPrentsOfUser";
    }
}
