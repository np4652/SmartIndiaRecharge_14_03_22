using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetDetailsForRechargePlans : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetDetailsForRechargePlans(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.CommonInt),
                new SqlParameter("@OID",req.CommonInt2),
            };
            var res = new OpRechargePlanResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            var cid = new List<OpRechCircleDetals>();
            var ptype = new List<OpRechPlanType>();

            try
            {
                DataSet ds = _dal.GetByProcedureAdapterDS(GetName(), param);
                if (ds.Tables.Count > 0)
                {
                    var dt = ds.Tables[0];
                    if (dt.Rows.Count > 0)
                    {
                        res.Statuscode = Convert.ToInt16(dt.Rows[0][0]);
                        res.Msg = dt.Rows[0]["Msg"] is DBNull ? string.Empty : dt.Rows[0]["Msg"].ToString();
                        if (res.Statuscode == ErrorCodes.One)
                        {
                            res.APICode = dt.Rows[0]["_APICode"] is DBNull ? "" : dt.Rows[0]["_APICode"].ToString();
                            res.URL = dt.Rows[0]["_URL"] is DBNull ? "" : dt.Rows[0]["_URL"].ToString();
                        }
                    }
                    if (ds.Tables.Count > 1)
                    {
                        DataTable cdt = ds.Tables[1];
                        foreach (DataRow dr in cdt.Rows)
                        {
                            cid.Add(new OpRechCircleDetals
                            {
                                CircleID = dr["_ID"] is DBNull ? 0 : Convert.ToInt32(dr["_ID"]),
                                CircleCode = dr["_SubMPlan"] is DBNull ? string.Empty : dr["_SubMPlan"].ToString()
                            });
                        }
                        res.CircleCodes = cid;
                    }
                    if (ds.Tables.Count > 2)
                    {
                        DataTable pty = ds.Tables[2];
                        foreach (DataRow dr in pty.Rows)
                        {
                            ptype.Add(new OpRechPlanType
                            {
                                TypeID = dr["_ID"] is DBNull ? 0 : Convert.ToInt32(dr["_ID"]),
                                PlanType = dr["_SubMPlan"] is DBNull ? string.Empty : dr["_SubMPlan"].ToString()
                            });
                        }
                        res.PlanType = ptype;
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
            return res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetDetailsForRechargePlans";
    }
}
