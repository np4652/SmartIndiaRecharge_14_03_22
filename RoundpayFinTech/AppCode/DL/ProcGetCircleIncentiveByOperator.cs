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
    public class ProcGetCircleIncentiveByOperator : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetCircleIncentiveByOperator(IDAL dal) => _dal = dal;
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
                new SqlParameter("@CurrentUserID",_req.CommonInt),
                new SqlParameter("@OID",_req.CommonInt2)
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
                            Circle = row["_Name"] is DBNull ? string.Empty : row["_Name"].ToString(),
                            TotalHits = Convert.ToInt32(row["TotalHits"] is DBNull ? 0 : row["TotalHits"]),
                            SuccessAmount = Convert.ToDecimal(row["_RequestedAmount"] is DBNull ? 0 : row["_RequestedAmount"]),
                            Incentive = Convert.ToDecimal(row["Incentive"] is DBNull ? 0 : row["Incentive"])
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

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_GetCircleIncentiveByOperator";
    }
}
