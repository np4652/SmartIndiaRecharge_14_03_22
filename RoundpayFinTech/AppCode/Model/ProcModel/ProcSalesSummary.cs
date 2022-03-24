using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.Reports.Filter;
using RoundpayFinTech.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class ProcSalesSummary : IProcedure
    {
        private readonly IDAL _dal;
        public ProcSalesSummary(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            _RechargeReportFilter _req = (_RechargeReportFilter)obj;
            SqlParameter[] param = {
                new SqlParameter("@FromDate", _req.FromDate ?? DateTime.Now.ToString("dd MMM yyyy")),
                new SqlParameter("@ToDate", _req.ToDate ?? DateTime.Now.ToString("dd MMM yyyy")),
                new SqlParameter("@UMobile", _req.OutletNo ?? ""),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@LT", _req.LT)
            };


            var _res = new List<SalesSummary>();
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        var salesSummary = new SalesSummary
                        {
                            UserID = Convert.ToInt32(row["_UserID"] is DBNull ? 0 : row["_UserID"]),
                            OutletMobile = row["_MobileNo"] is DBNull ? "" : row["_MobileNo"].ToString(),
                            OutletName = row["_OutletName"] is DBNull ? "" : row["_OutletName"].ToString(),
                            OID = Convert.ToInt32(row["_OID"] is DBNull ? 0 : row["_OID"]),
                            Operator = row["_Operator"] is DBNull ? "" : row["_Operator"].ToString(),
                            Amount = Convert.ToDecimal(row["_Amount"] is DBNull ? 0 : row["_Amount"]),
                            AmountR = Convert.ToDecimal(row["_RequestedAmount"] is DBNull ? 0 : row["_RequestedAmount"]),
                            LoginComm = Convert.ToDecimal(row["_LoginComm"] is DBNull ? 0 : row["_LoginComm"])
                        };
                        _res.Add(salesSummary);
                    }
                }
            }
            catch (Exception)
            { }
            return _res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "proc_SalesSummary";
        }
    }
}
