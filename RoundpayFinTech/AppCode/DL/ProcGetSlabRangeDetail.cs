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
    public class ProcGetSlabRangeDetail : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetSlabRangeDetail(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@OID",req.CommonInt),
            };
            var respList = new List<SlabRangeDetail>();
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0 && Convert.ToInt32(dt.Rows[0][0]) == 1)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        respList.Add(new SlabRangeDetail
                        {
                            OID = dr["_OID"] is DBNull ? 0 : Convert.ToInt32(dr["_OID"]),
                            SlabID = dr["_SlabID"] is DBNull ? 0 : Convert.ToInt32(dr["_SlabID"]),
                            RangeId = dr["_RangeId"] is DBNull ? 0 : Convert.ToInt32(dr["_RangeId"]),
                            MinRange = dr["_MinRange"] is DBNull ? 0 : Convert.ToInt32(dr["_MinRange"]),
                            MaxRange = dr["_MaxRange"] is DBNull ? 0 : Convert.ToInt32(dr["_MaxRange"]),
                            Comm = dr["_Comm"] is DBNull ? 0 : Convert.ToDecimal(dr["_Comm"]),
                            MaxComm = dr["_MaxComm"] is DBNull ? 0 : Convert.ToDecimal(dr["_MaxComm"]),
                            FixedCharge = dr["_FixedCharge"] is DBNull ? 0 : Convert.ToDecimal(dr["_FixedCharge"]),
                            CommType = dr["_CommType"] is DBNull ? false : Convert.ToBoolean(dr["_CommType"]),
                            AmtType = dr["_AmtType"] is DBNull ? false : Convert.ToBoolean(dr["_AmtType"]),
                            Operator = dr["Operator"] is DBNull?string.Empty: Convert.ToString(dr["Operator"]),
                            DMRModelID = dr["_DMRModelID"] is DBNull?0: Convert.ToInt32(dr["_DMRModelID"])
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetName(),
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                });
            }
            return respList;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetSlabRangeDetail";
    }
}
