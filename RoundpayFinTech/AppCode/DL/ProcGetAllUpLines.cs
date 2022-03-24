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
  
    public class ProcGetAllUpLines : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetAllUpLines(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            var _resp = new List<UserInfo>();
            SqlParameter[] param = {
                new SqlParameter("@LT", req.LoginTypeID),
                new SqlParameter("@LoginID", req.LoginID),
                new SqlParameter("@UserID", req.UserID),
                new SqlParameter("@LRoleID", req.CommonInt)
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        var userList = new UserInfo
                        {
                            Name = dr["_Name"] is DBNull ? string.Empty : dr["_Name"].ToString(),
                            OutletName = dr["_OutletName"] is DBNull ? string.Empty : dr["_OutletName"].ToString(),
                            MobileNo = dr["_MobileNo"] is DBNull ? string.Empty : dr["_MobileNo"].ToString(),
                            Role = dr["_Role"] is DBNull ? string.Empty : dr["_Role"].ToString()
                        };
                        _resp.Add(userList);
                    }
                    
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                });
            }
            return _resp;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetAllUpLines";
    }
}
