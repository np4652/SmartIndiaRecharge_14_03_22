using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.ROffer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetRNPRechargePlansFTB : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetRNPRechargePlansFTB(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;

            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.UserID),
                new SqlParameter("@OID",req.CommonInt),
                new SqlParameter("@CircleID",req.CommonInt2)
            };
            var data = new List<RNPRechPlansPanel>();
            try
            {
                DataTable dt = _dal.GetByProcedureAdapter(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        data.Add(new RNPRechPlansPanel
                        {
                            RechargePlanType = dr["_RechargePlanType"] is DBNull ? string.Empty : dr["_RechargePlanType"].ToString(),
                            RAmount = dr["_RAmount"] is DBNull ? string.Empty : dr["_RAmount"].ToString(),
                            Validity = dr["_Validity"] is DBNull ? string.Empty : dr["_Validity"].ToString(),
                            Details = dr["_Details"] is DBNull ? string.Empty : dr["_Details"].ToString()
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
                    UserId = req.LoginID
                });
            }
            return data;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetRNPRechargePlansFTB";
    }
}
