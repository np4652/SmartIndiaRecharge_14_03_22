using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
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
    public class ProcAPIUserSalesSummary : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcAPIUserSalesSummary(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var _req = (ForDateFilter)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LT),
                new SqlParameter("@FromDate", _req.FromDate ?? DateTime.Now.ToString("dd MMM yyyy")),
                new SqlParameter("@ToDate", _req.ToDate ?? DateTime.Now.ToString("dd MMM yyyy")),
                new SqlParameter("@UMobile", _req.UserMob ?? string.Empty),
                new SqlParameter("@LoginID", _req.LoginID)
            };
            var _res = new List<SalesSummary>();
            try
            {
                DataTable dt = await _dal.GetByProcedureAdapterAsync(GetName(), param).ConfigureAwait(false);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        _res.Add(new SalesSummary
                        {
                            EntryDate = row["_EntryDate"] is DBNull ? "" : row["_EntryDate"].ToString(),
                            UserID = Convert.ToInt32(row["_UserID"] is DBNull ? 0 : row["_UserID"]),
                            OutletMobile = row["_MobileNo"] is DBNull ? "" : row["_MobileNo"].ToString(),
                            OutletName = row["_OutletName"] is DBNull ? "" : row["_OutletName"].ToString(),
                            OID = Convert.ToInt32(row["_OID"] is DBNull ? 0 : row["_OID"]),
                            ServiceID = Convert.ToInt32(row["_ServiceID"] is DBNull ? 0 : row["_ServiceID"]),
                            Operator = row["_Operator"] is DBNull ? "" : row["_Operator"].ToString(),
                            Amount = Convert.ToDecimal(row["_Amount"] is DBNull ? 0 : row["_Amount"]),
                            AmountR = Convert.ToDecimal(row["RAmount"] is DBNull ? 0 : row["RAmount"]),
                            FailedAmount = Convert.ToDecimal(row["_FailedAmount"] is DBNull ? 0 : row["_FailedAmount"]),
                            FailedAmountR = Convert.ToDecimal(row["FailedRAmount"] is DBNull ? 0 : row["FailedRAmount"]),
                            GSTAmount = Convert.ToDecimal(row["_GSTTaxAmount"] is DBNull ? 0 : row["_GSTTaxAmount"]),
                            TDSAmount = Convert.ToDecimal(row["_TDSAmount"] is DBNull ? 0 : row["_TDSAmount"])
                        });
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return _res;
        }

        public Task<object> Call() => throw new NotImplementedException();
        public string GetName() => "proc_APIUserSalesSummary";
    }
}
