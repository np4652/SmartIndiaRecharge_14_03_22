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
    public class ProcInvoiceSettingsOps : IProcedure
    {
        private readonly IDAL _dal;
        public ProcInvoiceSettingsOps(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@IsDisable", _req.CommonBool),
                new SqlParameter("@InvoiceID", _req.CommonInt),
                new SqlParameter("@OpsCode", _req.CommonInt2)
            };


            var _res = new List<InvoiceSettings>();
            try
            {
                DataTable dt = _dal.GetByProcedureAdapter(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    if (dt.Columns.Contains("_InvoiceMonth"))
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            InvoiceSettings invoiceSettings = new InvoiceSettings
                            {
                                ID = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                                InvoiceMonth = row["_InvoiceMonth"] is DBNull ? "" : Convert.ToDateTime(row["_InvoiceMonth"]).ToString("MMM yyyy"),
                                IsDisable = row["_IsDisable"] is DBNull ? false : Convert.ToBoolean(row["_IsDisable"]),
                                _EntryDate = row["_EntryDate"] is DBNull ? "" : row["_EntryDate"].ToString(),
                                _ModifyDate = row["_ModifyDate"] is DBNull ? "" : row["_ModifyDate"].ToString(),
                                _ModifyBy = row["_ModifyBy"] is DBNull ? 0 : Convert.ToInt32(row["_ModifyBy"])
                            };
                            _res.Add(invoiceSettings);
                        }
                    }
                    else
                    {
                        _res.Add(new InvoiceSettings { StatusCode = (dt.Rows[0][0] is DBNull ? -1 : Convert.ToInt32(dt.Rows[0][0])), Msg = (dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString()) });
                    }
                }
            }
            catch (Exception)
            {
            }
            return _res;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_InvoiceSettingsOps";
    }
}