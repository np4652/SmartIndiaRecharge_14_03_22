using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetDenominationSwitchReport : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetDenominationSwitchReport(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@OpTypeID",req.CommonInt)
            };
            var DSwitchList = new List<DSwitchReport>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        DSwitchList.Add(new DSwitchReport
                        {
                            OID = row["_OID"] is DBNull ? 0 : Convert.ToInt32(row["_OID"]),
                            APIID = row["_APIID"] is DBNull ? 0 : Convert.ToInt32(row["_APIID"]),
                            Operator = Convert.ToString(row["_Operator"]),
                            API = Convert.ToString(row["_API"]),
                            Denoms = Convert.ToString(row["DenomIDs"]),
                            DenomR = Convert.ToString(row["DenomRanges"]),
                            CircleName = Convert.ToString(row["_CircleName"])
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
            return DSwitchList;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetDenominationSwitchReport";
    }
}
