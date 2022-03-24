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
    public class ProcGenerateInvoice : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGenerateInvoice(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@MobileNo",req.CommonStr??string.Empty),
                new SqlParameter("@InvoiceType",req.CommonInt),
                new SqlParameter("@InvoiceMonth",req.CommonStr2??DateTime.Now.ToString("01 MMM yyyy"))
            };
            var res = new List<InvoiceResponse>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    if (!dt.Columns.Contains("Msg"))
                    {
                        foreach (DataRow item in dt.Rows)
                        {
                            res.Add(new InvoiceResponse
                            {
                                InvoiceRefID = item["_InvoiceRefID"] is DBNull?0:Convert.ToInt32(item["_InvoiceRefID"]),
                                UserID = item["_UserID"] is DBNull ? 0 : Convert.ToInt32(item["_UserID"]),
                                WID = item["_WID"] is DBNull ? 0 : Convert.ToInt32(item["_WID"]),
                                MobileNo = item["_MobileNo"] is DBNull ? string.Empty : Convert.ToString(item["_MobileNo"]),
                                OutletName = item["_OutletName"] is DBNull ? string.Empty : Convert.ToString(item["_OutletName"]),
                                Name = item["_Name"] is DBNull ? string.Empty : Convert.ToString(item["_Name"]),
                                EmailID = item["_EmailID"] is DBNull ? string.Empty : Convert.ToString(item["_EmailID"]),
                                GSTIN = item["_GSTIN"] is DBNull ? string.Empty : Convert.ToString(item["_GSTIN"]),
                                PAN = item["_PAN"] is DBNull ? string.Empty : Convert.ToString(item["_PAN"]),
                                Address = item["_Address"] is DBNull ? string.Empty : Convert.ToString(item["_Address"]),
                                Pincode = item["_Pincode"] is DBNull ? string.Empty : Convert.ToString(item["_Pincode"]),
                                Operator = item["_Operator"] is DBNull ? string.Empty : Convert.ToString(item["_Operator"]),
                                HSNCode = item["_HSNCode"] is DBNull ? string.Empty : Convert.ToString(item["_HSNCode"]),
                                State = item["_State"] is DBNull ? string.Empty : Convert.ToString(item["_State"]),
                                Requested = item["_Requested"] is DBNull ? 0: Convert.ToDecimal(item["_Requested"]),
                                Amount = item["_Amount"] is DBNull ? 0: Convert.ToDecimal(item["_Amount"]),
                                CommAmount = item["_CommAmount"] is DBNull ? 0: Convert.ToDecimal(item["_CommAmount"]),
                                NetAmount = item["_NetAmount"] is DBNull ? 0: Convert.ToDecimal(item["_NetAmount"])
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

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_GenerateInvoice";
    }
}
