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
    public class ProcGetRange: IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetRange(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@RangeID",req.CommonInt),
            };
            var res = new List<RangeModel>();
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    if (req.CommonInt == 0)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            var rangeDetail = new RangeModel
                            {
                                ID = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                                MinRange = row["_MinRange"] is DBNull ? 0 : Convert.ToInt32(row["_MinRange"]),
                                MaxRange = row["_MaxRange"] is DBNull ? 0 : Convert.ToInt32(row["_MaxRange"]),
                                OpType = row["_OpType"] is DBNull ? string.Empty : Convert.ToString(row["_OpType"]),
                                OpTypeId = row["_OpTypeId"] is DBNull ? 0 : Convert.ToInt32(row["_OpTypeId"]),
                                ModifyDate = row["_ModifyDate"] is DBNull ? "" : row["_ModifyDate"].ToString(),
                            };
                            res.Add(rangeDetail);
                        }
                    }
                    else
                    {
                        var rangeDetail = new RangeModel
                        {
                            ID = dt.Rows[0]["_ID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_ID"]),
                            MinRange = dt.Rows[0]["_MinRange"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_MinRange"]),
                            MaxRange = dt.Rows[0]["_MaxRange"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_MaxRange"]),
                            OpType = dt.Rows[0]["_OpType"] is DBNull ? "" : dt.Rows[0]["_OpType"].ToString(),
                            OpTypeId = dt.Rows[0]["_OpTypeId"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_OpTypeId"]),
                            ModifyDate = dt.Rows[0]["_ModifyDate"] is DBNull ? "" : dt.Rows[0]["_ModifyDate"].ToString(),
                        };
                        return rangeDetail;
                    }
                }
            }
            catch (Exception)
            {
            }
            if (req.CommonInt == 0)
                return res;
            else
                return new RangeModel();
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_GetRange";

    }
}

