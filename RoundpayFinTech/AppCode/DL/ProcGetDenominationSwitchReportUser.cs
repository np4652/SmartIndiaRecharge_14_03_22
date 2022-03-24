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
    public class ProcGetDenominationSwitchReportUser : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetDenominationSwitchReportUser(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@MobileNo",req.CommonStr3??string.Empty),
                new SqlParameter("@UserID",req.CommonInt),
                new SqlParameter("@OpTypeID",req.CommonInt2)
            };
            List<DSwitchReport> DSwitchList = new List<DSwitchReport>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        var _Read = new DSwitchReport
                        {
                            OID = row["_OID"] is DBNull ? 0 : Convert.ToInt32(row["_OID"]),
                            APIID = row["_APIID"] is DBNull ? 0 : Convert.ToInt32(row["_APIID"]),
                            RoleID = row["_RoleID"] is DBNull ? 0 : Convert.ToInt32(row["_RoleID"]),
                            UserID = row["_UserID"] is DBNull ? 0 : Convert.ToInt32(row["_UserID"]),
                            Operator = row["_Operator"] is DBNull ? string.Empty : Convert.ToString(row["_Operator"]),
                            OutletName = row["_OutletName"] is DBNull ? string.Empty : Convert.ToString(row["_OutletName"]),
                            MobileNo = row["_MobileNo"] is DBNull ? string.Empty : Convert.ToString(row["_MobileNo"]),
                            Prefix = row["_Prefix"] is DBNull ? string.Empty : Convert.ToString(row["_Prefix"]),
                            Role = row["_Role"] is DBNull ? string.Empty : Convert.ToString(row["_Role"]),
                            API = row["_API"] is DBNull ? string.Empty : Convert.ToString(row["_API"]),
                            Denoms = row["DenomIDs"] is DBNull ? string.Empty : Convert.ToString(row["DenomIDs"]),
                            DenomR = row["DenomRanges"] is DBNull ? string.Empty : Convert.ToString(row["DenomRanges"]),
                            CircleName = row["_CircleName"] is DBNull ? string.Empty : Convert.ToString(row["_CircleName"]),
                        };
                        DSwitchList.Add(_Read);
                    }
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return DSwitchList;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetDenominationSwitchReport_User";
    }
}
