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
    public class ProcGetB2CCoupons : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetB2CCoupons(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var LoginID = (int)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID",LoginID)
            };
            var res = new List<B2CMemberCouponDetail>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(),param);
                if (dt.Rows.Count > 0) {
                    if (!dt.Columns.Contains("Msg")) {
                        foreach (DataRow item in dt.Rows)
                        {
                            res.Add(new B2CMemberCouponDetail
                            {
                                ID=item["_ID"] is DBNull?0:Convert.ToInt32(item["_ID"]),
                                CouponCode = item["_CouponCode"] is DBNull ? string.Empty : Convert.ToString(item["_CouponCode"]),
                                CouponExpiry = item["_CouponExpiry"] is DBNull ? string.Empty : Convert.ToString(item["_CouponExpiry"]),
                                RedeemDate = item["_RedeemDate"] is DBNull ? string.Empty : Convert.ToString(item["_RedeemDate"]),
                                CouponValue = item["_CouponValue"] is DBNull ? 0 : Convert.ToDecimal(item["_CouponValue"]),
                                IsRedeemed = item["_IsRedeemed"] is DBNull ? false: Convert.ToBoolean(item["_IsRedeemed"])
                                
                            });
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            return res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_GetB2CCoupons";
    }
}
