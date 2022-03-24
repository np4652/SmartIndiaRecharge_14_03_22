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
    public class ProcUserDayBook : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUserDayBook(IDAL dal) => _dal = dal;
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
                            OID = row["_OID"] is DBNull ? 0 : Convert.ToInt32(row["_OID"]),
                            Operator = row["_Operator"] is DBNull ? "" : row["_Operator"].ToString(),
                            TotalHits = Convert.ToInt32(row["TotalHits"] is DBNull ? 0 : row["TotalHits"]),
                            TotalAmount = Convert.ToDecimal(row["TotalAmount"] is DBNull ? 0 : row["TotalAmount"]),
                            SuccessHits = Convert.ToInt32(row["SuccessHits"] is DBNull ? 0 : row["SuccessHits"]),
                            SuccessAmount = Convert.ToDecimal(row["SuccessAmount"] is DBNull ? 0 : row["SuccessAmount"]),
                            RefundHits = Convert.ToInt32(row["RefundHits"] is DBNull ? 0 : row["RefundHits"]),
                            RefundAmount = Convert.ToDecimal(row["RefundAmount"] is DBNull ? 0 : row["RefundAmount"]),
                            FailedHits = Convert.ToInt32(row["FailedHits"] is DBNull ? 0 : row["FailedHits"]),
                            FailedAmount = Convert.ToDecimal(row["FailedAmount"] is DBNull ? 0 : row["FailedAmount"]),
                            PendingHits = Convert.ToInt32(row["PendingHits"] is DBNull ? 0 : row["PendingHits"]),
                            PendingAmount = Convert.ToDecimal(row["PendingAmount"] is DBNull ? 0 : row["PendingAmount"]),
                            Commission = Convert.ToDecimal(row["Commission"] is DBNull ? 0 : row["Commission"]),
                            Incentive = Convert.ToDecimal(row["Incentive"] is DBNull ? 0 : row["Incentive"]),
                            CircleComm = Convert.ToDecimal(row["CircleComm"] is DBNull ? 0 : row["CircleComm"]),
                            SelfCommission = Convert.ToDecimal(row["SelfCommission"] is DBNull ? 0 : row["SelfCommission"]),
                            TeamCommission = Convert.ToDecimal(row["TeamCommission"] is DBNull ? 0 : row["TeamCommission"])
                        };
                        _res.Add(daybook);
                    }
                }
            }
            catch (Exception)
            {
            }
            return _res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_UserDayBook";
    }
}
