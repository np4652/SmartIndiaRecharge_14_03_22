using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetDenominationRange : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetDenominationRange(IDAL dal) => _dal = dal;
        public string GetName() => "proc_GetDenominationRange";
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@DenomRangeID",req.CommonInt),
            };
            DenominationRangeList _list = new DenominationRangeList {
                Detail = new List<DenominationRange>()
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    if (req.CommonInt == 0)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            var rangeDetail = new DenominationRange
                            {
                                ID = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                                Min = row["_Min"] is DBNull ? 0 : Convert.ToInt32(row["_Min"]),
                                Max = row["_Max"] is DBNull ? 0 : Convert.ToInt32(row["_Max"]),
                                EntryBy = row["_EntryBy"] is DBNull ? 0 : Convert.ToInt32(row["_EntryBy"]),
                                EntryDate = row["_EntryDate"] is DBNull ? "" : row["_EntryDate"].ToString(),
                                ModifyBy = row["_ModifyBy"] is DBNull ? 0 : Convert.ToInt32(row["_ModifyBy"]),
                                ModifyDate = row["_ModifyDate"] is DBNull ? "" : row["_ModifyDate"].ToString()
                            };
                            //res.Add(rangeDetail);
                            _list.Detail.Add(rangeDetail);
                        }
                    }
                    else
                    {
                        var rangeDetail = new DenominationRange
                        {
                            ID = dt.Rows[0]["_ID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_ID"]),
                            Min = dt.Rows[0]["_Min"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_Min"]),
                            Max = dt.Rows[0]["_Max"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_Max"]),
                            EntryBy = dt.Rows[0]["_EntryBy"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_EntryBy"]),
                            EntryDate = dt.Rows[0]["_EntryDate"] is DBNull ? "" : dt.Rows[0]["_EntryDate"].ToString(),
                            ModifyBy = dt.Rows[0]["_ModifyBy"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_ModifyBy"]),
                            ModifyDate = dt.Rows[0]["_ModifyDate"] is DBNull ? "" : dt.Rows[0]["_ModifyDate"].ToString(),
                        };
                        return rangeDetail;
                    }
                }
            }
            catch (Exception ex)
            {
            }
            if (req.CommonInt == 0)
                //  return res;
                return _list;
            else
                return new DenominationRange();
        }

        public object Call() => throw new NotImplementedException();
    }
}
