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
    public class ProcGetRealTimeCommision : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetRealTimeCommision(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
            };
            var respList = new List<SlabRangeDetail>();
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
               
                    foreach (DataRow dr in dt.Rows)
                    {
                        respList.Add(new SlabRangeDetail
                        {
                            MinRange = dr["_MinRange"] is DBNull ? 0 : Convert.ToInt32(dr["_MinRange"]),
                            MaxRange = dr["_MaxRange"] is DBNull ? 0 : Convert.ToInt32(dr["_MaxRange"]),
                            Comm = dr["_Comm"] is DBNull ? 0 : Convert.ToDecimal(dr["_Comm"]),
                            CommType = dr["_CommType"] is DBNull ? false : Convert.ToBoolean(dr["_CommType"]),
                            AmtType = dr["_AmtType"] is DBNull ? false : Convert.ToBoolean(dr["_AmtType"]),
                            Operator = Convert.ToString(dr["_Name"])
                        });
                    }
                
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetName(),
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return respList;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetRealTimeCommision";
    }
}
