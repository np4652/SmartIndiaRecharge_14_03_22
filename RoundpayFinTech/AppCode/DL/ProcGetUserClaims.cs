using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.App;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetUserClaims : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetUserClaims(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (MNPClaimDataReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@TopRows",req.TopRows),
                new SqlParameter("@UserID",req.UserID),
                new SqlParameter("@ToDate",req.ToDate),
                new SqlParameter("@FromDate",req.FromDate),
                new SqlParameter("@IsRecent",req.IsRecent)
            };
            var res = new List<MNPClaims>();
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        res.Add(new MNPClaims
                        {
                            OpName = dr["_OpName"] is DBNull ? string.Empty : dr["_OpName"].ToString(),
                            Status = dr["_Status"] is DBNull ? string.Empty : dr["_Status"].ToString(),
                            MNPMobile = dr["_MobileNo"] is DBNull ? string.Empty : dr["_MobileNo"].ToString(),
                            ReferenceID = dr["_ReferenceID"] is DBNull ? string.Empty : dr["_ReferenceID"].ToString(),
                            Amount = dr["_Amount"] is DBNull ? 0 : Convert.ToDecimal(dr["_Amount"]),
                            ApprovedDate = dr["_ApproveDate"] is DBNull ? "NA" : dr["_ApproveDate"].ToString()
                        });
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
                    UserId = req.UserID
                });
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_GetUserClaims";
    }
}
