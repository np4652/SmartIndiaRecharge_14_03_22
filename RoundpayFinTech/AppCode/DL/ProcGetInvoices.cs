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
    public class ProcGetInvoices : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetInvoices(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@MobileNo", _req.CommonStr ?? string.Empty)
            };


            var _res = new List<InvoiceDetail>();
            try
            {
                DataTable dt = _dal.GetByProcedureAdapter(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    if (dt.Columns.Contains("_InvoiceMonth"))
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            InvoiceDetail invoiceDetail = new InvoiceDetail
                            {
                                InvoiceID = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                                InvoiceMonth = row["_InvoiceMonth"] is DBNull ? "" : Convert.ToDateTime(row["_InvoiceMonth"]).ToString("MMM yyyy"),
                                InvoiceURL = row["InvoiceURL"] is DBNull ? "" : row["InvoiceURL"].ToString(),
                                UploadedDate = row["UploadedDate"] is DBNull ? "" : row["UploadedDate"].ToString(),
                                UploadStatus = row["UploadStatus"] is DBNull ? 0 : Convert.ToInt32(row["UploadStatus"]),
                                Remark = row["Remark"] is DBNull ? "" : row["Remark"].ToString()
                            };
                            _res.Add(invoiceDetail);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            return _res;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_GetInvoices";
    }
}
