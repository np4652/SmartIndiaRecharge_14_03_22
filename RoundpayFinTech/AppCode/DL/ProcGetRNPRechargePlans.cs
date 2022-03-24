using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.Model.Reports;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.ROffer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetRNPRechargePlans : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetRNPRechargePlans(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@OID", _req.CommonInt),
                new SqlParameter("@Circle", _req.CommonInt2)
            };

            var _res = new List<RNPRechPlansPanel>();
            
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        _res.Add(new RNPRechPlansPanel {
                            OID = dr["_OID"] is DBNull ? 0 : Convert.ToInt32(dr["_OID"]),
                            CircleID = dr["_CircleID"] is DBNull ? 0 : Convert.ToInt32(dr["_CircleID"]),
                            RechargePlanType = dr["_RechargePlanType"] is DBNull ? string.Empty : dr["_RechargePlanType"].ToString(),
                            RAmount = dr["_RAmount"] is DBNull ? string.Empty : dr["_RAmount"].ToString(),
                            Details = dr["_Details"] is DBNull ? string.Empty : dr["_Details"].ToString(),
                            Validity = dr["_Validity"] is DBNull ? string.Empty : dr["_Validity"].ToString(),
                            EntryDate = dr["_EntryDate"] is DBNull ? string.Empty : Convert.ToDateTime(dr["_EntryDate"]).ToString("dd MMM yyyy")
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
                    LoginTypeID = _req.LoginTypeID,
                    UserId = _req.LoginID
                });
            }
            return _res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "Proc_GetRNPRechargePlans";
    }
}
