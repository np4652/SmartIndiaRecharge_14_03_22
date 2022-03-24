using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGSTReportP2PAndP2A : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGSTReportP2PAndP2A(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (GSTReportFilter)obj;
            SqlParameter[] param = {
             new SqlParameter("@LT",req.LoginTypeID),
             new SqlParameter("@LoginID",req.LoginID),
             new SqlParameter("@MobileNo",req.MobileNo??string.Empty),
             new SqlParameter("@GSTMonth",req.GSTMonth ?? DateTime.Now.ToString("MMM yyyy")),
             new SqlParameter("@IsGSTVerified",req.IsGSTVerified),
             new SqlParameter("@BillingModel",req.BillingModel??"P2P")
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
                            InvoiceID = item["_InvoiceID"] is DBNull ? 0 : Convert.ToInt32(item["_InvoiceID"]),
                            Name = item["_Name"] is DBNull ? string.Empty : Convert.ToString(item["_Name"]),
                            OutletName = item["_OutletName"] is DBNull ? string.Empty : Convert.ToString(item["_OutletName"]),
                            Mobile = item["_MobileNo"] is DBNull ? string.Empty : Convert.ToString(item["_MobileNo"]),
                            EmailID = item["_EmailID"] is DBNull ? string.Empty : Convert.ToString(item["_EmailID"]),
                            State = item["_State"] is DBNull ? string.Empty : Convert.ToString(item["_State"]),
                            PAN = item["_PAN"] is DBNull ? string.Empty : Convert.ToString(item["_PAN"]),
                            GSTIN = item["_GSTIN"] is DBNull ? string.Empty : Convert.ToString(item["_GSTIN"]),
                            IsGSTVerified = item["_IsGSTVerified"] is DBNull ? false : Convert.ToBoolean(item["_IsGSTVerified"]),
                            IsHoldGST = item["_IsHoldGST"] is DBNull ? false : Convert.ToBoolean(item["_IsHoldGST"]),
                            ByAdminUser = item["_ByAdminUser"] is DBNull ? false : Convert.ToBoolean(item["_ByAdminUser"]),
                            RequestedAmount = item["_RequestedAmount"] is DBNull ? 0 : Convert.ToDecimal(item["_RequestedAmount"]),
                            Amount = item["_Amount"] is DBNull ? 0 : Convert.ToDecimal(item["_Amount"]),
                            Discount = item["_Discount"] is DBNull ? 0 : Convert.ToDecimal(item["_Discount"]),
                            GSTTaxAmount = item["_GSTTaxAmount"] is DBNull ? 0 : Convert.ToDecimal(item["_GSTTaxAmount"]),
                            TDSAmount = item["_TDSAmount"] is DBNull ? 0 : Convert.ToDecimal(item["_TDSAmount"]),
                            NetAmount = item["_NetAmount"] is DBNull ? 0 : Convert.ToDecimal(item["_NetAmount"]),
                            CompanyState = item["CompanyState"] is DBNull ? string.Empty : Convert.ToString(item["CompanyState"]),
                            BillingModel = item["_BillingModel"] is DBNull ? string.Empty : Convert.ToString(item["_BillingModel"])
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

        public string GetName() => "proc_GSTReportP2P_and_P2A";
    }
}
