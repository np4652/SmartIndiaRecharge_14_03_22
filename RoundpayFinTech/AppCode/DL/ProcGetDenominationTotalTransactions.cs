using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetDenominationTotalTransactions : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetDenominationTotalTransactions(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@OID",req.CommonInt2),
                new SqlParameter("@UserID",req.CommonInt),
                new SqlParameter("@FromDate",req.CommonStr??DateTime.Now.ToString("dd MMM yyy")),
                new SqlParameter("@ToDate",req.CommonStr2??DateTime.Now.ToString("dd MMM yyy"))
            };
            var _res = new List<Daybook>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        var daybook = new Daybook
                        {
                            
                            Denomination = Convert.ToInt32(row["_RequestedAmount"] is DBNull ? 0 : row["_RequestedAmount"]),
                            TotalHits = Convert.ToInt32(row["TotalHits"] is DBNull ? 0 : row["TotalHits"]),
                            TotalAmount = Convert.ToDecimal(row["TotalAmount"] is DBNull ? 0 : row["TotalAmount"]),
                            SuccessHits = Convert.ToInt32(row["SuccessHits"] is DBNull ? 0 : row["SuccessHits"]),
                            SuccessAmount = Convert.ToDecimal(row["SuccessAmount"] is DBNull ? 0 : row["SuccessAmount"]),
                            FailedHits = Convert.ToInt32(row["FailedHits"] is DBNull ? 0 : row["FailedHits"]),
                            FailedAmount = Convert.ToDecimal(row["FailedAmount"] is DBNull ? 0 : row["FailedAmount"]),
                            APICommission = Convert.ToDecimal(row["APICommission"] is DBNull ? 0 : row["APICommission"]),
                            Commission = Convert.ToDecimal(row["Commission"] is DBNull ? 0 : row["Commission"]),
                            Incentive = Convert.ToDecimal(row["Incentive"] is DBNull ? 0 : row["Incentive"]),
                            GSTTaxAmount = Convert.ToDecimal(row["GSTTaxAmount"] is DBNull ? 0 : row["GSTTaxAmount"]),
                            TDSAmount = Convert.ToDecimal(row["TDSAmount"] is DBNull ? 0 : row["TDSAmount"]),
                            //TeamCommission = Convert.ToDecimal(row["TeamCommission"] is DBNull ? 0 : row["TeamCommission"])
                        };
                        //daybook.Profit = daybook.APICommission - daybook.Commission - daybook.TeamCommission - daybook.Incentive;
                        _res.Add(daybook);
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

        public string GetName() => "Proc_GetDenominationTotalTransactions";
    }
}
