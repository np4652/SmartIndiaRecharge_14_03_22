using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetB2CMemberTypeByMember : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetB2CMemberTypeByMember(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var LoginID = (int)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID",LoginID)
            };
            var res = new List<MembershipmasterB2C>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    if (!dt.Columns.Contains("Msg")) {
                        foreach (DataRow row in dt.Rows)
                        {
                            res.Add(new MembershipmasterB2C
                            {
                                IsIDActive = row["_IsIDActive"] is DBNull ? false : Convert.ToBoolean(row["_IsIDActive"]),
                                IsCouponAllowed = row["_IsCouponAllowed"] is DBNull ? false : Convert.ToBoolean(row["_IsCouponAllowed"]),
                                ID = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                                MemberShipType = row["_MemberShipType"] is DBNull ? string.Empty : row["_MemberShipType"].ToString(),
                                Remark = row["_Remark"] is DBNull ? string.Empty : row["_Remark"].ToString(),
                                CouponCount = row["_CouponCount"] is DBNull ? 0 : Convert.ToInt32(row["_CouponCount"]),
                                CouponValue = row["_CouponValue"] is DBNull ? 0 : Convert.ToInt32(row["_CouponValue"]),
                                CouponValidityDays = row["_CouponValidityDays"] is DBNull ? 0 : Convert.ToInt32(row["_CouponValidityDays"]),
                                Cost = row["_Cost"] is DBNull ? 0 : Convert.ToDecimal(row["_Cost"]),
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

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_GetB2CMemberTypeByMember";
    }
}
