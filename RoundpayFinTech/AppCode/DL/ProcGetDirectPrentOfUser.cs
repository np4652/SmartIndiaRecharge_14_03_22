using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetDirectPrentOfUser : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetDirectPrentOfUser(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            CommonReq req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID)
            };
            UserInfo resp = new UserInfo();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    resp.UserID = dt.Rows[0]["_ID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_ID"]);
                    resp.RoleID = dt.Rows[0]["_RoleID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_RoleID"]);
                    resp.OutletName = dt.Rows[0]["_OutletName"] is DBNull ? "" : dt.Rows[0]["_OutletName"].ToString();
                    resp.MobileNo = dt.Rows[0]["_MobileNo"] is DBNull ? "" : dt.Rows[0]["_MobileNo"].ToString();
                }
            }
            catch (Exception)
            {
            }
            return resp;
        }

        public object Call() => throw new NotImplementedException();


        public string GetName() => "proc_GetDirectPrentOfUser";

    }
}
