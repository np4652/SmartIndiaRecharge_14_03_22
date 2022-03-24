using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
namespace RoundpayFinTech.AppCode.DL
{
    public class ProcUserDayBookDMR : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUserDayBookDMR(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (CommonReq)obj;
            _req.CommonStr = _req.CommonStr ?? DateTime.Now.ToString("dd MMM yyyy");
            SqlParameter[] param = {
                new SqlParameter("@FromDate", _req.CommonStr),
                new SqlParameter("@ToDate", _req.CommonStr3 ?? _req.CommonStr),
                new SqlParameter("@UserID", _req.LoginID),
                new SqlParameter("@UserMob", _req.CommonStr2 ?? string.Empty),
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@CurrentUserID",_req.CommonInt)
            };
            
            var _res = new List<DMRDaybook>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        var daybook = new DMRDaybook
                        {
                            TotalHits = Convert.ToInt32(row["TotalHits"] is DBNull ? 0 : row["TotalHits"]),
                            TotalAmount = Convert.ToDecimal(row["Total"] is DBNull ? 0 : row["Total"]),
                            CCF = Convert.ToDecimal(row["_CCF"] is DBNull ? 0 : row["_CCF"]),
                            BaseAmount = Convert.ToDecimal(row["_BaseAmount"] is DBNull ? 0 : row["_BaseAmount"]),
                            Surcharge = Convert.ToDecimal(row["_Surcharge"] is DBNull ? 0 : row["_Surcharge"]),
                            GSTOnSurcharge = Convert.ToDecimal(row["_GstOnSurcharge"] is DBNull ? 0 : row["_GstOnSurcharge"]),
                            AmountAfterSurcharge = Convert.ToDecimal(row["_AmtAfterSurcharge"] is DBNull ? 0 : row["_AmtAfterSurcharge"]),
                            RefundGST = Convert.ToDecimal(row["_RefundGST"] is DBNull ? 0 : row["_RefundGST"]),
                            AmountWithTDS = Convert.ToDecimal(row["_AmtWithTDS"] is DBNull ? 0 : row["_AmtWithTDS"]),
                            TDS = Convert.ToDecimal(row["_TDS"] is DBNull ? 0 : row["_TDS"]),
                            CreditedAmount = Convert.ToDecimal(row["_Credited_Amount"] is DBNull ? 0 : row["_Credited_Amount"]),
                            SelfCommission = Convert.ToDecimal(row["SelfCommission"] is DBNull ? 0 : row["SelfCommission"]),
                            TeamCommission = Convert.ToDecimal(row["_TeamCommission"] is DBNull ? 0 : row["_TeamCommission"])
                        };
                        _res.Add(daybook);
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return _res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_UserDayBook_DMR";
    }
}