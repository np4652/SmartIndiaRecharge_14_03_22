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
  
    public class ProcGetHoliday : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetHoliday(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            var _resp = new List<BankHoliday>();
            SqlParameter[] param = {
                new SqlParameter("@LT", req.LoginTypeID),
                new SqlParameter("@LoginID", req.LoginID)
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        var userList = new BankHoliday
                        {
                            //ID = Convert.ToInt32(dr["_ID"] ? 0 : Convert.ToInt32(dr["_ID"]),
                            ID = dr["_ID"] is DBNull ? 0 : Convert.ToInt32(dr["_ID"]),
                            Date = dr["_Date"] is DBNull ? string.Empty : dr["_Date"].ToString(),
                            Entrydate = dr["_EntryDate"] is DBNull ? string.Empty : dr["_EntryDate"].ToString(),
                            ModifyDate = dr["_ModifyDate"] is DBNull ? string.Empty : dr["_ModifyDate"].ToString(),
                            Remark = dr["_Remark"] is DBNull ? string.Empty : dr["_Remark"].ToString()
                        };
                        _resp.Add(userList);
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
            return _resp;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetHolidays";
    }

    public class ProcGetUpcomingHoliday : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetUpcomingHoliday(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            var _resp = new List<BankHoliday>();
            SqlParameter[] param = {
                new SqlParameter("@LT", req.LoginTypeID),
                new SqlParameter("@LoginID", req.LoginID)
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        var userList = new BankHoliday
                        {
                            Date = dr["_Date"] is DBNull ? string.Empty : dr["_Date"].ToString(),
                            Remark = dr["_Remark"] is DBNull ? string.Empty : dr["_Remark"].ToString()
                        };
                        _resp.Add(userList);
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
            return _resp;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetUpcomingHolidays";
    }
}
