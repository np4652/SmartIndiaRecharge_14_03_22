using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetApiDenomUser : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetApiDenomUser(IDAL dal) => _dal = dal;
        public string GetName() => "proc_GetApiDenomUser";
        public object Call(object obj)
        {
            var req = (APIDenominationReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@APIID",req.APIId),
                new SqlParameter("@OID",req.OID),
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@UserID",req.UserID),
                new SqlParameter("@CircleID",req.CircleID)
            };
            var res = new APIDenomination {
                DenomDetail=new List<APIDenomModal>(),
                DenomRangeDetail=new List<APIDRangeModal>()
            };
            try
            {
                var ds = _dal.GetByProcedureAdapterDS(GetName(), param);
                if (ds != null)
                {
                    var dt = ds.Tables[0];
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            res.DenomDetail.Add(new APIDenomModal
                            {
                                DnomID = row["_DnomID"] is DBNull ? 0 : Convert.ToInt32(row["_DnomID"]),
                                Amount = row["_Amount"] is DBNull ? 0 : Convert.ToInt32(row["_Amount"]),
                                IsDnomActive = row["_IsDnomActive"] is DBNull ? false : Convert.ToBoolean(row["_IsDnomActive"]),
                                HitCountD = row["_HitCountD"] is DBNull ? 0 : Convert.ToInt32(row["_HitCountD"]),
                                MaxCountD = row["_MaxCountD"] is DBNull ? 0 : Convert.ToInt32(row["_MaxCountD"]),
                            });
                        }
                    }

                    var dtRange = ds.Tables[1];
                    if (dtRange.Rows.Count > 0)
                    {
                        foreach (DataRow row in dtRange.Rows)
                        {
                            res.DenomRangeDetail.Add(new APIDRangeModal
                            {
                                DRangeID = row["_DRangeID"] is DBNull ? 0 : Convert.ToInt32(row["_DRangeID"]),
                                HitCountDR = row["_HitCountDR"] is DBNull ? 0 : Convert.ToInt32(row["_HitCountDR"]),
                                MaxCountDR = row["_MaxCountDR"] is DBNull ? 0 : Convert.ToInt32(row["_MaxCountDR"]),
                                DRange = Convert.ToString(row["_DRange"]),
                                IsDRangeActive = row["_IsDRangeActive"] is DBNull ? false : Convert.ToBoolean(row["_IsDRangeActive"])
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return res;
        }

        public object Call() => throw new NotImplementedException();
    }
}

