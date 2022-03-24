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
    public class ProcGetSlabDetailForCircle : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetSlabDetailForCircle(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;

            SqlParameter[] param = {
                new SqlParameter("@SlabID",req.CommonInt),
                new SqlParameter("@OID",req.CommonInt2)
            };
            var res = new List<SlabCommission>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        res.Add(new SlabCommission
                        {
                            SlabID=row["_SlabID"] is DBNull? req.CommonInt:Convert.ToInt32(row["_SlabID"]),
                            OID = row["_OID"] is DBNull ? req.CommonInt2 : Convert.ToInt32(row["_OID"]),
                            CircleID = row["_CircleID"] is DBNull ? 0: Convert.ToInt32(row["_CircleID"]),
                            Circle = row["_Circle"] is DBNull ? string.Empty : row["_Circle"].ToString(),
                            Comm = row["_Comm"] is DBNull ? 0M : Convert.ToDecimal(row["_Comm"]),
                            CommType = row["_CommType"] is DBNull ? 0 : Convert.ToInt32(row["_CommType"]),
                            AmtType = row["_AmtType"] is DBNull ? 0 : Convert.ToInt32(row["_AmtType"]),
                            ModifyDate = row["_ModifyDate"] is DBNull ? string.Empty : row["_ModifyDate"].ToString()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_GetSlabDetailForCircle";
    }
}
