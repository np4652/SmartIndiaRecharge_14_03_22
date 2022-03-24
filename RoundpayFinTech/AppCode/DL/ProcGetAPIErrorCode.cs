using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetAPIErrorCode : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetAPIErrorCode(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var APIErrCode = (APIErrorCode)obj;
            SqlParameter[] param = {
                new SqlParameter("@GroupCode",APIErrCode.GroupCode??string.Empty),
                new SqlParameter("@APICode",(APIErrCode.APICode??string.Empty).Replace(",","").Replace(" ","").ToUpper()),
                new SqlParameter("@ErrorType",APIErrCode.ErrorType)
            };
            var res = new APIErrorCode();
            try
            {
                const string Query = "select t._ID, t._GroupCode, t._ECode, t._APICode, t._EntryDate, t._ModifyDate from tbl_APIErrorCode t,tbl_ErrorCode e where ','+t._APICode+',' like '%,'+@APICode+',%' and t._GroupCode=@GroupCode and t._APICode is not null and t._ECode=e._Code and (e._Type=@ErrorType or @ErrorType=0)";
                var dt = _dal.Get(Query, param);
                if (dt.Rows.Count > 0)
                {

                    res.ID = dt.Rows[0]["_ID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_ID"]);
                    res.GroupCode = dt.Rows[0]["_GroupCode"] is DBNull ? "" : dt.Rows[0]["_GroupCode"].ToString();
                    res.ECode = dt.Rows[0]["_ECode"] is DBNull ? "" : dt.Rows[0]["_ECode"].ToString();
                    res.APICode = dt.Rows[0]["_APICode"] is DBNull ? "" : dt.Rows[0]["_APICode"].ToString();
                }
            }
            catch (Exception)
            {
            }
            return res;
        }

        public object Call()
        {
            var lst = new List<APIErrorCode>();
            try
            {
                var dt = _dal.Get(GetName());
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow item in dt.Rows)
                    {
                        lst.Add(new APIErrorCode
                        {
                            ID = item["_ID"] is DBNull ? 0 : Convert.ToInt32(item["_ID"]),
                            GroupCode = item["_GroupCode"] is DBNull ? "" : item["_GroupCode"].ToString(),
                            ECode = item["_ECode"] is DBNull ? "" : item["_ECode"].ToString(),
                            APICode = item["_APICode"] is DBNull ? "" : item["_APICode"].ToString(),
                        });
                    }
                }
            }
            catch (Exception)
            {
            }
            return lst;
        }

        public string GetName() => "select _ID, _GroupCode, _ECode, _APICode, _EntryDate, _ModifyDate from tbl_APIErrorCode ";
    }
}
