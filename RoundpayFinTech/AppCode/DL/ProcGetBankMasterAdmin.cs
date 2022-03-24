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
    public class ProcGetBankMasterAdmin : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetBankMasterAdmin(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@TopRows",req.CommonInt),
                new SqlParameter("@KeyWords",req.CommonStr)
            };
            var res = new List<BankMaster>();
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {

                    foreach (DataRow row in dt.Rows)
                    {
                        var BankDetailsDetail = new BankMaster
                        {
                            ID = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                            BankName = row["_Bank"] is DBNull ? "" : row["_Bank"].ToString(),
                            IFSC = row["_IFSC"] is DBNull ? "" : row["_IFSC"].ToString(),
                            AccountLimit = row["_ACNo_Limit"] is DBNull ? 0 : Convert.ToInt32(row["_ACNo_Limit"]),
                            Code = row["_Code"] is DBNull ? "" : row["_Code"].ToString(),
                            IIN = row["_IIN"] is DBNull ? 0 : Convert.ToInt32(row["_IIN"]),
                            IsIMPS = row["_IsIMPS"] is DBNull ? false : Convert.ToBoolean(row["_IsIMPS"]),
                            IsNEFT = row["_IsNEFT"] is DBNull ? false : Convert.ToBoolean(row["_IsNEFT"]),
                            IsACVerification = row["_IsACVerification"] is DBNull ? false : Convert.ToBoolean(row["_IsACVerification"]),
                            ISAEPSStatus = row["_AEPSStatus"] is DBNull ? false : Convert.ToBoolean(row["_AEPSStatus"]),
                            BankType = row["_BankType"] is DBNull ? "" : row["_BankType"].ToString(),
                            Logo = row["_Logo"] is DBNull ? "" : row["_Logo"].ToString(),
                        };
                        res.Add(BankDetailsDetail);
                    }

                }
            }
            catch (Exception ex)
            {
            }
            return res;

        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "Proc_GetBankMasterAdmin";
    }

}
