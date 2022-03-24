using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Recharge;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcBBPSComplainReport : IProcedure
    {
        private readonly IDAL _dal;
        public ProcBBPSComplainReport(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@UserID",req.LoginID),
                new SqlParameter("@LoginTypeID",req.LoginTypeID)
            };
            var res = new List<BBPSComplainReport>();
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        var com = new BBPSComplainReport();
                        com.ID = dt.Rows[i]["_ID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_ID"]);
                        com.ComplainType = dt.Rows[i]["_ComplainType"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_ComplainType"]);
                        com.ParticipationType = dt.Rows[i]["_ParticipationType"] is DBNull ? "" : dt.Rows[i]["_ParticipationType"].ToString();
                        com.TransactionID= dt.Rows[i]["_TransactionID"] is DBNull ? "" : dt.Rows[i]["_TransactionID"].ToString();
                        com.ComplainID = dt.Rows[i]["ComplainID"] is DBNull ? "" : dt.Rows[i]["ComplainID"].ToString();
                        com.ComplainTypeID = dt.Rows[i]["ComplainTypeID"] is DBNull ? "" : dt.Rows[i]["ComplainTypeID"].ToString();
                        com.ComplainDate = dt.Rows[i]["ComplainDate"] is DBNull ? "" : dt.Rows[i]["ComplainDate"].ToString();
                        com.ComplainStatus = dt.Rows[i]["ComplainStatus"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["ComplainStatus"]);
                        com.Remark = dt.Rows[i]["_Remark"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[i]["_Remark"]);
                        com.API = dt.Rows[i]["_API"] is DBNull ? string.Empty : Convert.ToString(dt.Rows[i]["_API"]);
                        res.Add(com);
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
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = req.UserID
                });
            }
            return res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_BBPSComplainReport";
    }
}
