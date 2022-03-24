using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetTDSSummary : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetTDSSummary(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (GSTReportFilter)obj;
            SqlParameter[] param = {
             new SqlParameter("@LT",req.LoginTypeID),
             new SqlParameter("@LoginID",req.LoginID),
             new SqlParameter("@MobileNo",req.MobileNo??string.Empty),
             new SqlParameter("@InvoiceMonth",req.GSTMonth ?? DateTime.Now.ToString("MMM yyyy"))
            };
            var res = new List<CalculatedGSTEntry>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow item in dt.Rows)
                    {
                        res.Add(new CalculatedGSTEntry
                        {
                            InvoiceMonth = Convert.ToDateTime(item["_GSTMonth"] is DBNull ? string.Empty : Convert.ToString(item["_GSTMonth"])).ToString("dd MMM yyyy"),
                            Name = item["_Name"] is DBNull ? string.Empty : Convert.ToString(item["_Name"]),
                            OutletName = item["_OutletName"] is DBNull ? string.Empty : Convert.ToString(item["_OutletName"]),
                            Mobile = item["_MobileNo"] is DBNull ? string.Empty : Convert.ToString(item["_MobileNo"]),
                            State = item["_State"] is DBNull ? string.Empty : Convert.ToString(item["_State"]),
                            PAN = item["_PAN"] is DBNull ? string.Empty : Convert.ToString(item["_PAN"]),
                            GSTIN = item["_GSTIN"] is DBNull ? string.Empty : Convert.ToString(item["_GSTIN"]),
                            Discount = item["_CommAmount"] is DBNull ? 0 : Convert.ToDecimal(item["_CommAmount"]),
                            TDSAmount = item["_TDSAmount"] is DBNull ? 0 : Convert.ToDecimal(item["_TDSAmount"])
                        });
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return res;
        }
        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_GetTDSSummary";
    }
}