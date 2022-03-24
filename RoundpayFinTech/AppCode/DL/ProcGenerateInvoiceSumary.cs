using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGenerateInvoiceSumary : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGenerateInvoiceSumary(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@MobileNo",req.CommonStr??string.Empty),
                new SqlParameter("@InvoiceMonth",req.CommonStr2 ?? DateTime.Now.ToString("01 MMM yyyy"))
            };
            var res = new List<InvoiceSummaryResponse>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    if (!dt.Columns.Contains("Msg"))
                    {
                        foreach (DataRow item in dt.Rows)
                        {
                            res.Add(new InvoiceSummaryResponse
                            {
                                BillingModel = item["_BillingModel"] is DBNull ?string.Empty : item["_BillingModel"].ToString(),
                                OpTypeID = item["_OpTypeID"] is DBNull ? 0 : Convert.ToInt32(item["_OpTypeID"]),
                                OpType = item["_OpType"] is DBNull ? string.Empty : Convert.ToString(item["_OpType"]),
                                Requested = item["_Requested"] is DBNull ? 0 : Convert.ToDecimal(item["_Requested"]),
                                Amount = item["_Amount"] is DBNull ? 0 : Convert.ToDecimal(item["_Amount"]),
                                CommAmount = item["_CommAmount"] is DBNull ? 0 : Convert.ToDecimal(item["_CommAmount"]),
                                TDSAmount = item["_TDSAmount"] is DBNull ? 0 : Convert.ToDecimal(item["_TDSAmount"]),
                                GSTTaxAmount = item["_GSTTaxAmount"] is DBNull ? 0 : Convert.ToDecimal(item["_GSTTaxAmount"])
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return res;
        }
        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_GenerateInvoiceSumary";
    }
}
