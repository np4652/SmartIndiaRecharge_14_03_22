using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Reports.Filter;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcTransactionUserOpDateWise : IProcedure
    {
        private readonly IDAL _dal;
        public ProcTransactionUserOpDateWise(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (ForDateFilter)obj;
            SqlParameter[] param = {
            new SqlParameter("@LT",req.LT),
            new SqlParameter("@LoginID",req.LoginID),
            new SqlParameter("@FromDate",req.FromDate??DateTime.Now.ToString("dd MMM yyyy")),
            new SqlParameter("@ToDate ",req.ToDate??DateTime.Now.ToString("dd MMM yyyy")),
            new SqlParameter("@OutletNo",req.UserMob??string.Empty),
            new SqlParameter("@UserID",req.UserID),
            };
            List<SalesSummary> res = new List<SalesSummary>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow item in dt.Rows)
                    {
                        res.Add(new SalesSummary
                        {
                            UserID = item["_UserID"] is DBNull ? 0 : Convert.ToInt32(item["_UserID"]),
                            OutletName =item["_UserName"] is DBNull?string.Empty: item["_UserName"].ToString(),
                            EntryDate = item["_EntryDate"] is DBNull ? string.Empty : item["_EntryDate"].ToString(),
                            OID = item["_OID"] is DBNull ? 0 : Convert.ToInt32(item["_OID"]),
                            Operator = item["_Operator"] is DBNull ? string.Empty : item["_Operator"].ToString(),
                            Amount = item["_Amount"] is DBNull ? 0M :Convert.ToDecimal(item["_Amount"]),
                            AmountR = item["_RequestedAmount"] is DBNull ? 0M :Convert.ToDecimal(item["_RequestedAmount"]),
                            FailedAmount = item["_FailedAmount"] is DBNull ? 0M : Convert.ToDecimal(item["_FailedAmount"]),
                            FailedAmountR = item["_FailedRequestedAmount"] is DBNull ? 0M : Convert.ToDecimal(item["_FailedRequestedAmount"])
                        });
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

        public string GetName() => "proc_TransactionUserOpDateWise";
    }
}
