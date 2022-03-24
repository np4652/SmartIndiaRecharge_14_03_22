using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetUserWiseLimit: IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetUserWiseLimit(IDAL dal) => _dal = dal;
        public object Call() => throw new System.NotImplementedException();

        public string GetName() => "proc_GetUserWiseLimit";

        public object Call(object obj)
        {
            CommonReq req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@UserID", req.LoginID)            
            };
            var resp = new List<UserWiseLimitResp>();

            var dt = _dal.GetByProcedure(GetName(),param);
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    var data = new UserWiseLimitResp
                    {
                        OID = dr["_OID"] is DBNull ? 0 : Convert.ToInt32(dr["_OID"]),
                        UserLimitID = dr["_UlID"] is DBNull ? 0 : Convert.ToInt32(dr["_UlID"]),
                        OperatorName = dr["_Name"] is DBNull ? string.Empty : dr["_Name"].ToString(),
                        OpType = dr["_OpType"] is DBNull ? string.Empty : dr["_OpType"].ToString(),
                        UsedLimit = dr["_UsedLimit"] is DBNull ? 0 : Convert.ToDecimal(dr["_UsedLimit"])
                    };
                    resp.Add(data);
                }
            }
            return resp;
        }

    }
}
