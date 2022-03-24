using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetPlansAPIByAPICode : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetPlansAPIByAPICode(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            CommonReq req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@APICode",req.str),
                new SqlParameter("@AccountNo",req.CommonStr),
                new SqlParameter("@OID",req.CommonInt),
                new SqlParameter("@CircleID",req.CommonInt2),
                new SqlParameter("@PackageID",req.CommonStr2??string.Empty),
                new SqlParameter("@Language",req.CommonStr3??string.Empty)
            };
            var resp = new APIDetail();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    if (!dt.Columns.Contains("Msg")) {
                        resp = new APIDetail
                        {
                            ID = dt.Rows[0]["_ID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_ID"]),
                            URL = dt.Rows[0]["_URL"] is DBNull ? "" : dt.Rows[0]["_URL"].ToString()
                        };
                    }
                }
            }
            catch (Exception ex){
            }
            return resp;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "proc_GetPlansAPIByAPICode";
        }
    }
}
