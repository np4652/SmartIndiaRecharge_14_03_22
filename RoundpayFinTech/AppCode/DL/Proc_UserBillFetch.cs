using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Microsoft.AspNetCore.Mvc;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class Proc_UserBillFetch : IProcedure
    {
        private readonly Fintech.AppCode.DB.IDAL _dal;
        public Proc_UserBillFetch(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var req = (UserBillFetchReport)obj;
            SqlParameter[] param = {
                new SqlParameter("@ApiID", req.APIID),
                new SqlParameter("@OpID", req.OPID),
                new SqlParameter("@FromDate", req.FromDate),
                new SqlParameter("@ToDate", req.ToDate)
            };
            var resp = new List<UserBillFetchReport>();
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        var UserBillFetchReport = new UserBillFetchReport
                        {
                            UserID = row["_UserID"] is DBNull ? 0 : Convert.ToInt32(row["_UserID"]),
                            Name = row["_Name"] is DBNull ? string.Empty : row["_Name"].ToString(),
                            MobileNo = row["_MobileNo"] is DBNull ? string.Empty : row["_MobileNo"].ToString(),
                            Success = row["Success"] is DBNull ? 0 : Convert.ToInt32(row["Success"]),
                            Failed = row["Failed"] is DBNull ? 0 : Convert.ToInt32(row["Failed"]),
                        };
                        resp.Add(UserBillFetchReport);
                    }
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,                   
                    Error = ex.Message
                });
            }
            return resp;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "Proc_UserBillFetch";
    }
}

