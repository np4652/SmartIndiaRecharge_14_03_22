using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetCallbackData : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetCallbackData(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", req.LoginID),
                new SqlParameter("@Top", req.CommonInt),
                new SqlParameter("@FilterText", req.CommonStr?? "")
            };
            
            var _lst = new List<CallbackRequests>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                foreach (DataRow row in dt.Rows)
                {
                    var _res = new CallbackRequests
                    {
                        ID = row["_ID"] is DBNull?0: Convert.ToInt32(row["_ID"]),
                        Content = row["_Content"] is DBNull?"": row["_Content"].ToString(),
                        RequestIP = row["_RequestIP"] is DBNull ? "" : row["_RequestIP"].ToString(),
                        EntryDate = row["_EntryDate"] is DBNull ? "" : row["_EntryDate"].ToString(),
                        Method = row["_Method"] is DBNull ? "" : row["_Method"].ToString(),
                        Path = row["_Path"] is DBNull ? "" : row["_Path"].ToString()
                    };
                    _lst.Add(_res);
                }
            }
            catch (Exception)
            { }
            return _lst;

        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_GetCallbackData";
    }
}