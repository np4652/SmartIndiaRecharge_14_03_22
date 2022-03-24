using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.Models;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetInvoiceCompanyDetail : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetInvoiceCompanyDetail(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@WID",req.CommonInt),
                new SqlParameter("@InvoiceMonth",req.CommonStr??string.Empty)
            };

            var res = new InvoiceCompanyDetail();
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.CompanyName = dt.Rows[0]["_Name"] is DBNull ? string.Empty : dt.Rows[0]["_Name"].ToString();
                    res.CompanyAddress = dt.Rows[0]["_CompanyAddress"] is DBNull ? string.Empty : dt.Rows[0]["_CompanyAddress"].ToString();
                    res.CompanyMobile = dt.Rows[0]["_AccountMobileNo"] is DBNull ? string.Empty : dt.Rows[0]["_AccountMobileNo"].ToString();
                    res.CompanyMobile2 = dt.Rows[0]["_AccountsDepNo"] is DBNull ? string.Empty : dt.Rows[0]["_AccountsDepNo"].ToString();
                    res.CompanyEmail = dt.Rows[0]["_EmailID"] is DBNull ? string.Empty : dt.Rows[0]["_EmailID"].ToString();
                    res.CompanyState = dt.Rows[0]["StateName"] is DBNull ? string.Empty : dt.Rows[0]["StateName"].ToString();
                    res.CompanyPincode = dt.Rows[0]["_PinCode"] is DBNull ? string.Empty : dt.Rows[0]["_PinCode"].ToString();
                    res.CompanyPAN = dt.Rows[0]["_PAN"] is DBNull ? string.Empty : dt.Rows[0]["_PAN"].ToString();
                    res.CompanyGST = dt.Rows[0]["_GSTIN"] is DBNull ? string.Empty : dt.Rows[0]["_GSTIN"].ToString();
                    res.InvoicePrefix = dt.Rows[0]["_InvoicePrefix"] is DBNull ? string.Empty : dt.Rows[0]["_InvoicePrefix"].ToString();
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

        public string GetName() => "proc_GetInvoiceCompanyDetail";
    }
}
