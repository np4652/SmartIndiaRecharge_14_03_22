using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.Reports.Filter;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.Model.ProcModel
{
   
    public class ProcUserSalesSummaryRoleWiseAdmin : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUserSalesSummaryRoleWiseAdmin(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            var _req = (ForDateFilter)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LT),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@FromDate", _req.FromDate ?? DateTime.Now.ToString("dd MMM yyyy")),
                new SqlParameter("@ToDate", _req.ToDate ?? DateTime.Now.ToString("dd MMM yyyy"))
            };
            var _res = new List<SalesSummary>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        var salesSummary = new SalesSummary
                        {
                            EntryDate = row["_EntryDate"] is DBNull ? "" : row["_EntryDate"].ToString(),
                            UserID = Convert.ToInt32(row["_UserID"] is DBNull ? 0 : row["_UserID"]),
                            OutletMobile = row["_MobileNo"] is DBNull ? "" : row["_MobileNo"].ToString(),
                            OutletName = row["_OutletName"] is DBNull ? "" : row["_OutletName"].ToString(),
                            OID = Convert.ToInt32(row["_OID"] is DBNull ? 0 : row["_OID"]),
                            Operator = row["_Operator"] is DBNull ? "" : row["_Operator"].ToString(),
                            SCount = row["RCount"] is DBNull ? 0 : Convert.ToInt32(row["RCount"]),
                            Amount = Convert.ToDecimal(row["_Amount"] is DBNull ? 0 : row["_Amount"]),
                            AmountR = Convert.ToDecimal(row["RAmount"] is DBNull ? 0 : row["RAmount"]),
                            FCount = row["FCount"] is DBNull ? 0 : Convert.ToInt32(row["FCount"]),
                            FailedAmount = Convert.ToDecimal(row["_FailedAmount"] is DBNull ? 0 : row["_FailedAmount"]),
                            FailedAmountR = Convert.ToDecimal(row["FailedRAmount"] is DBNull ? 0 : row["FailedRAmount"]),
                            GSTAmount = Convert.ToDecimal(row["_GSTTaxAmount"] is DBNull ? 0 : row["_GSTTaxAmount"]),
                            TDSAmount = Convert.ToDecimal(row["_TDSAmount"] is DBNull ? 0 : row["_TDSAmount"])
                        };
                        _res.Add(salesSummary);
                    }
                }
            }
            catch (Exception)
            { }
            return _res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "proc_UserSalesSummaryRoleWiseAdmin";
        }
    }
}
