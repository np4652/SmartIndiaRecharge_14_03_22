using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcUserwiseSwitchReport : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUserwiseSwitchReport(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@MobileNo",req.CommonStr??""),
                new SqlParameter("@UserID",req.CommonInt),
                new SqlParameter("@OpTypeID",req.CommonInt2)
            };
            var res = new List<Userswitch>();
            try
            {
                var dt = _dal.GetByProcedure(GetName(),param);
                if (dt.Rows.Count > 0) {
                    if (!dt.Columns.Contains("Msg")) {
                        foreach (DataRow item in dt.Rows)
                        {
                            res.Add(new Userswitch
                            {
                                SwichID = item["SwitchID"] is DBNull?0:Convert.ToInt32(item["SwitchID"]),
                                APIID = item["_APIID"] is DBNull ? 0 : Convert.ToInt32(item["_APIID"]),
                                OID = item["_OID"] is DBNull ? 0 : Convert.ToInt32(item["_OID"]),
                                APIName = item["APIName"] is DBNull ? "" : item["APIName"].ToString(),
                                Operator = item["Operator"] is DBNull ? "" : item["Operator"].ToString(),
                                UserID = item["UserID"] is DBNull ? 0 : Convert.ToInt32(item["UserID"]),
                                Prefix = item["_Prefix"] is DBNull ? "" : item["_Prefix"].ToString(),
                                Role = item["_Role"] is DBNull ? "" : item["_Role"].ToString(),
                                MobileNo = item["_MobileNo"] is DBNull ? "" : item["_MobileNo"].ToString(),
                                OutletName = item["_OutletName"] is DBNull ? "" : item["_OutletName"].ToString()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return res;
        }
        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_UserwiseSwitchReport";
    }
}
