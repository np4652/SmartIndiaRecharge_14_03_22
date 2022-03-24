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
    public class ProcGetP2AInvoiceUploaded : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetP2AInvoiceUploaded(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@MobileNo",req.CommonStr??string.Empty)
            };
            var res = new List<P2AInvoiceListModel>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow item in dt.Rows)
                    {
                        res.Add(new P2AInvoiceListModel
                        {
                            UserID = item["_UserID"] is DBNull ? 0 : Convert.ToInt32(item["_UserID"]),
                            MobileNo = item["_MobileNo"] is DBNull ? string.Empty : item["_MobileNo"].ToString(),
                            OutletName = item["_OutletName"] is DBNull ? string.Empty : item["_OutletName"].ToString(),
                            Name = item["_Name"] is DBNull ? string.Empty : item["_Name"].ToString(),
                            EmailID = item["_EmailID"] is DBNull ? string.Empty : item["_EmailID"].ToString(),
                            GSTIN = item["_GSTIN"] is DBNull ? string.Empty : item["_GSTIN"].ToString(),
                            PAN = item["_PAN"] is DBNull ? string.Empty : item["_PAN"].ToString(),
                            GSTMonth = item["_GSTMonth"] is DBNull ? string.Empty : item["_GSTMonth"].ToString(),
                            GSTAmount = item["_GSTAmount"] is DBNull ? 0M : Convert.ToDecimal(item["_GSTAmount"]),
                            P2AInvoiceURL = item["_P2AInvoiceURL"] is DBNull ? string.Empty : item["_P2AInvoiceURL"].ToString(),
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                });
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_GetP2AInvoiceUploaded";
    }
}
