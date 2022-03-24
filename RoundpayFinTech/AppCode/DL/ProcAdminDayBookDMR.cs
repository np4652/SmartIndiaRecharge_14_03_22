using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.Reports.Filter;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcAdminDayBookDMR : IProcedure
    {
        private readonly IDAL _dal;
        public ProcAdminDayBookDMR(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (ForDateFilter)obj;
            SqlParameter[] param = {
                new SqlParameter("@FromDate",req.FromDate ?? DateTime.Now.ToString("dd MMM yyyy")),
                new SqlParameter("@ToDate",req.ToDate ?? DateTime.Now.ToString("dd MMM yyyy"))
            };
            var res = new List<DMRDaybook>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        var daybook = new DMRDaybook
                        {
                            TotalHits = Convert.ToInt32(row["TotalHits"] is DBNull ? 0 : row["TotalHits"]),
                            TotalAmount = Convert.ToDecimal(row["Total"] is DBNull ? 0 : row["Total"]),
                            CCF = Convert.ToDecimal(row["_CCF"] is DBNull ? 0 : row["_CCF"]),
                            BaseAmount = Convert.ToDecimal(row["_BaseAmount"] is DBNull ? 0 : row["_BaseAmount"]),
                            Surcharge = Convert.ToDecimal(row["_Surcharge"] is DBNull ? 0 : row["_Surcharge"]),
                            GSTOnSurcharge = Convert.ToDecimal(row["_GstOnSurcharge"] is DBNull ? 0 : row["_GstOnSurcharge"]),
                            AmountAfterSurcharge = Convert.ToDecimal(row["_AmtAfterSurcharge"] is DBNull ? 0 : row["_AmtAfterSurcharge"]),
                            RefundGST = Convert.ToDecimal(row["_RefundGST"] is DBNull ? 0 : row["_RefundGST"]),
                            AmountWithTDS = Convert.ToDecimal(row["_AmtWithTDS"] is DBNull ? 0 : row["_AmtWithTDS"]),
                            TDS = Convert.ToDecimal(row["_TDS"] is DBNull ? 0 : row["_TDS"]),
                            CreditedAmount = Convert.ToDecimal(row["_Credited_Amount"] is DBNull ? 0 : row["_Credited_Amount"]),
                            APISurcharge = Convert.ToDecimal(row["_SurchargeAPI"] is DBNull ? 0 : row["_SurchargeAPI"]),
                            APIGSTOnSurcharge = Convert.ToDecimal(row["_GstOnSurchargeAPI"] is DBNull ? 0 : row["_GstOnSurchargeAPI"]),
                            APIAmountAfterSurcharge = Convert.ToDecimal(row["_AmtAfterSurchargeAPI"] is DBNull ? 0 : row["_AmtAfterSurchargeAPI"]),
                            APIRefundGST = Convert.ToDecimal(row["_RefundGSTAPI"] is DBNull ? 0 : row["_RefundGSTAPI"]),
                            APIAmountWithTDS = Convert.ToDecimal(row["_AmtWithTDSAPI"] is DBNull ? 0 : row["_AmtWithTDSAPI"]),
                            APITDS = Convert.ToDecimal(row["_TDSAPI"] is DBNull ? 0 : row["_TDSAPI"]),
                            APICreditedAmount = Convert.ToDecimal(row["_Credited_AmountAPI"] is DBNull ? 0 : row["_Credited_AmountAPI"]),                            
                            TeamCommission = Convert.ToDecimal(row["_TeamCommission"] is DBNull ? 0 : row["_TeamCommission"])
                        };
                        daybook.Profit = daybook.APICreditedAmount - daybook.CreditedAmount - daybook.TeamCommission;
                        res.Add(daybook);
                    }
                }
            }
            catch (Exception ex)
            { }
            return res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_AdminDayBook_DMR";
    }
}
