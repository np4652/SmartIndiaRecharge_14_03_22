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
    public class ProcGetOperatorByService : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetOperatorByService(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            CommonReq req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@ServiceID",req.CommonStr)
            };
            var res = new List<OperatorDetail>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        OperatorDetail operatorDetail = new OperatorDetail
                        {
                            OID = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                            Name = row["_Name"] is DBNull ? "" : row["_Name"].ToString(),
                            OPID = row["_OPID"] is DBNull ? "" : row["_OPID"].ToString(),
                            OpType = row["_Type"] is DBNull ? 0 : Convert.ToInt32(row["_Type"])
                        };
                        res.Add(operatorDetail);
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
            return res;
        }

        public object Call() => throw new NotImplementedException();
        
        public string GetName() => "proc_GetOperatorByService";
    }
}
