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
    public class ProcBonafideAccount : IProcedure
    {
        private readonly IDAL _dal;
        public ProcBonafideAccount(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@TopRows",req.CommonInt),
                new SqlParameter("@OutletMobile",req.CommonStr),
                new SqlParameter("@Account",req.CommonStr2),
                new SqlParameter("@Status",req.CommonStr3)
            };
            var res = new List<BonafideAccount>();
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {

                    foreach (DataRow row in dt.Rows)
                    {
                        var BonafideAccountDetail = new BonafideAccount
                        {
                            ID = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                            Name = row["_Name"] is DBNull ? "" : row["_Name"].ToString(),
                            OutletName = row["_OutletName"] is DBNull ? "" : row["_OutletName"].ToString(),
                            MobileNo = row["_MobileNo"] is DBNull ? "" : row["_MobileNo"].ToString(),
                            AccountNo = row["_AccountNo"] is DBNull ? "" : row["_AccountNo"].ToString(),
                            IFSC = row["_IFSC"] is DBNull ? "" : row["_IFSC"].ToString(),
                            PyeeName = row["_PyeeName"] is DBNull ? "" : row["_PyeeName"].ToString(),
                            IsActive = row["_IsActive"] is DBNull ? false : Convert.ToBoolean(row["_IsActive"]),
                            ModifyDate = row["_ModifyDate"] is DBNull ? "" : row["_ModifyDate"].ToString(),
                            UPICount = row["_UPICount"] is DBNull ? 0 : Convert.ToInt32(row["_UPICount"]),
                        };
                        res.Add(BonafideAccountDetail);
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "Proc_BonafideAccount";
    }

}
