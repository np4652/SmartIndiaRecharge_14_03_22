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
    public class ProcGetMembershipMaster : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetMembershipMaster(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@MembershipID",req.CommonInt)
            };
            var res = new List<MembershipMaster>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    if (req.CommonInt == -1)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            var membershipMaster = new MembershipMaster
                            {
                               
                                ID = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                                MemberShipType = row["_MemberShipType"] is DBNull ? "" : row["_MemberShipType"].ToString(),
                                EntryDate = row["_EntryDate"] is DBNull ? "" : row["_EntryDate"].ToString(),
                                ModifyDate = row["_ModifyDate"] is DBNull ? "" : row["_ModifyDate"].ToString(),
                                IsActive = row["_IsActive"] is DBNull ? false : Convert.ToBoolean(row["_IsActive"]),
                                Remark = row["_Remark"] is DBNull ? "" : row["_Remark"].ToString(),
                                CouponCount = row["_CouponCount"] is DBNull ? 0 :Convert.ToInt32( row["_CouponCount"]),
                                CouponValue = row["_CouponValue"] is DBNull ? 0 : Convert.ToInt32(row["_CouponValue"]),
                                IsCouponAllowed = row["_IsCouponAllowed"] is DBNull ? false:  Convert.ToBoolean(row["_IsCouponAllowed"]),
                                CouponValidityDays = row["_CouponValidityDays"] is DBNull ? 0 : Convert.ToInt32(row["_CouponValidityDays"]),
                                Cost=row["_Cost"] is DBNull ? 0 : Convert.ToDecimal(row["_Cost"]),
                                SlabID= row["_SlabID"] is DBNull ? 0 : Convert.ToInt32(row["_SlabID"]),
                                SlabName = row["_Slab"] is DBNull ? "" : row["_Slab"].ToString(),
                                IsAdminDefined = row["_IsAdminDefined"] is DBNull ? false : Convert.ToBoolean(row["_IsAdminDefined"]),
                                MinInterval = row["_MinInterval"] is DBNull ? 0 : Convert.ToInt32(row["_MinInterval"]),
                                ReferralIncome= row["_ReferralIncome"] is DBNull ? 0.00 : Convert.ToDouble(row["_ReferralIncome"]),
                                PackageValidity = row["_PackageValidity"] is DBNull ? 0 : Convert.ToInt32(row["_PackageValidity"])
                            };
                            res.Add(membershipMaster);
                        }
                    }
                    else
                    {
                        return new MembershipMaster
                        {
                            ID = dt.Rows[0]["_ID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_ID"]),
                            MemberShipType = dt.Rows[0]["_MemberShipType"] is DBNull ? "" : dt.Rows[0]["_MemberShipType"].ToString(),
                            EntryDate = dt.Rows[0]["_EntryDate"] is DBNull ? "" : dt.Rows[0]["_EntryDate"].ToString(),
                            ModifyDate = dt.Rows[0]["_ModifyDate"] is DBNull ? "" : dt.Rows[0]["_ModifyDate"].ToString(),
                            IsActive = dt.Rows[0]["_IsActive"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsActive"]),
                            Remark = dt.Rows[0]["_Remark"] is DBNull ? "" : dt.Rows[0]["_Remark"].ToString(),
                            CouponCount = dt.Rows[0]["_CouponCount"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_CouponCount"]),
                            CouponValue = dt.Rows[0]["_CouponValue"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_CouponValue"]),
                            IsCouponAllowed = dt.Rows[0]["_IsCouponAllowed"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsCouponAllowed"]),
                            CouponValidityDays = dt.Rows[0]["_CouponValidityDays"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_CouponValidityDays"]),
                            Cost = dt.Rows[0]["_Cost"] is DBNull ? 0: Convert.ToDecimal(dt.Rows[0]["_Cost"]),
                            SlabID= dt.Rows[0]["_SlabID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_SlabID"]),
                            SlabName = dt.Rows[0]["_Slab"] is DBNull ? "" : dt.Rows[0]["_Slab"].ToString(),
                            IsAdminDefined = dt.Rows[0]["_IsAdminDefined"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsAdminDefined"]),
                            MinInterval = dt.Rows[0]["_MinInterval"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_MinInterval"]),
                            ReferralIncome = dt.Rows[0]["_ReferralIncome"] is DBNull ? 0 : Convert.ToDouble(dt.Rows[0]["_ReferralIncome"]),
                            PackageValidity= dt.Rows[0]["_PackageValidity"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_PackageValidity"])
                        };
                    }

                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                });
            }
            if (req.CommonInt == -1)
                return res;
            else
                return new MembershipMaster();
        }

        object IProcedure.Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_GetMemberShipMaster";
    }
}
