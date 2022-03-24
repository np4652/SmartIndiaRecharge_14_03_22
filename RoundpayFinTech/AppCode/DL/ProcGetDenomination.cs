using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetDenomination : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetDenomination(IDAL dal) => _dal = dal;
        public string GetName() => "proc_GetDenomination";
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@DenomID",req.CommonInt),
            };
            var res = new List<DenominationModal>();
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    if (req.CommonInt == 0)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            var rangeDetail = new DenominationModal
                            {
                                ID = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                                Amount = row["_Amount"] is DBNull ? 0 : Convert.ToInt32(row["_Amount"]),
                                EntryBy = row["_EntryBy"] is DBNull ? 0 : Convert.ToInt32(row["_EntryBy"]),
                                EntryDate = row["_EntryDate"] is DBNull ? "" : row["_EntryDate"].ToString(),
                                ModifyBy = row["_ModifyBy"] is DBNull ? 0 : Convert.ToInt32(row["_ModifyBy"]),
                                ModifyDate = row["_ModifyDate"] is DBNull ? "" : row["_ModifyDate"].ToString(),
                                Remark = row["_Remark"] is DBNull ? "" : row["_Remark"].ToString(),
                            };
                            res.Add(rangeDetail);
                        }
                    }
                    else
                    {
                        var rangeDetail = new DenominationModal
                        {
                            ID = dt.Rows[0]["_ID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_ID"]),
                            Amount = dt.Rows[0]["_Amount"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_Amount"]),
                            EntryBy = dt.Rows[0]["_EntryBy"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_EntryBy"]),
                            EntryDate = dt.Rows[0]["_EntryDate"] is DBNull ? "" : dt.Rows[0]["_EntryDate"].ToString(),
                            ModifyBy = dt.Rows[0]["_ModifyBy"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_ModifyBy"]),
                            ModifyDate = dt.Rows[0]["_ModifyDate"] is DBNull ? "" : dt.Rows[0]["_ModifyDate"].ToString(),
                            Remark = dt.Rows[0]["_Remark"] is DBNull ? "" : dt.Rows[0]["_Remark"].ToString(),
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
                return new DenominationModal();
        }

        public object Call() => throw new NotImplementedException();
    }
}
