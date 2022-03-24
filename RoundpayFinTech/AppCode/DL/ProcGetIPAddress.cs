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
    public class ProcGetIPAddress : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetIPAddress(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@ID",req.CommonInt),
                new SqlParameter("@UserID",req.CommonInt2),
                new SqlParameter("@MobileNo",req.CommonStr??"")
            };
            var res = new List<IPAddressModel>();
            try
            {
                var dt = _dal.GetByProcedure(GetName(),param);
                if (dt.Rows.Count > 0) {
                    if (!dt.Columns.Contains("Msg")) {
                        foreach (DataRow item in dt.Rows)
                        {
                            res.Add(new IPAddressModel {
                                ID = item["_ID"] is DBNull ? 0 : Convert.ToInt32(item["_ID"]),
                                IP = item["_IP"] is DBNull ? string.Empty : item["_IP"].ToString(),
                                IPType = item["_IPTypeID"] is DBNull ? 0 : Convert.ToInt32(item["_IPTypeID"]),
                                UserID = item["_UserID"] is DBNull ? 0 : Convert.ToInt32(item["_UserID"]),
                                OutletName= item["_OutletName"] is DBNull ? string.Empty : item["_OutletName"].ToString(),
                                MobileNo = item["_MobileNo"] is DBNull ? string.Empty : item["_MobileNo"].ToString(),
                                LastModified = item["_ModifyDate"] is DBNull ? string.Empty : item["_ModifyDate"].ToString(),
                                Source = item["_Source"] is DBNull ? string.Empty : item["_Source"].ToString(),
                                IsActive= item["_IsActive"] is DBNull ? false : Convert.ToBoolean(item["_IsActive"])
                            });
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            return res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetIPAddress";
        public bool ValidateCallBackIPFromDB(string IP) 
        {
            try
            {
                SqlParameter[] param = { 
                    new SqlParameter("@IP",IP)
                };
                var dt = _dal.Get("select 1 _IsFound from tbl_IPAddress where _IP = @IP and _IPTypeID=2", param);
                if (dt.Rows.Count > 0)
                    return dt.Rows[0]["_IsFound"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsFound"]);
            }
            catch (Exception ex)
            {
            }
            return false;
        }
    }
}
