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
    public class ProcSlabCommissionSetting : IProcedure
    {
        private readonly IDAL _dal;
        public ProcSlabCommissionSetting(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            var res = new List<SlabCommissionSettingRes>();

            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@OID",req.CommonInt)
            };

            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        var data = new SlabCommissionSettingRes
                        {
                            ID = dr["_ID"] is DBNull ? 0 : Convert.ToInt32(dr["_ID"]),
                            SlabID = dr["_SLABID"] is DBNull ? 0 : Convert.ToInt32(dr["_SLABID"]),
                            OID = dr["_OID"] is DBNull ? 0 : Convert.ToInt32(dr["_OID"]),
                            SlabName = Convert.ToString(dr["_Slab"]),
                            Commission = dr["_Comm"] is DBNull ? 0 : Convert.ToDecimal(dr["_Comm"]),
                            CommissionType = dr["_CommType"] is DBNull ? 0 : Convert.ToInt32(dr["_CommType"]),
                            AmountType = dr["_AmtType"] is DBNull ? 0 : Convert.ToInt32(dr["_AmtType"]),
                            RComm = dr["_RComm"] is DBNull ? 0 : Convert.ToDecimal(dr["_RComm"]),
                            RCommType = dr["_RCommType"] is DBNull ? 0 : Convert.ToInt32(dr["_RCommType"]),
                            RAmtType = dr["_RAmtType"] is DBNull ? 0 : Convert.ToInt32(dr["_RAmtType"]),
                        };
                        res.Add(data);
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

        public string GetName() => "Proc_SlabCommissionSetting";
    }
}