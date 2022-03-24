using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model.Reports;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcASCSummary : IProcedure
    {
        private readonly IDAL _dal;
        public ProcASCSummary(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            ProcUserLedgerRequest _req = (ProcUserLedgerRequest)obj;
            SqlParameter[] param = {
                new SqlParameter("@UserID", _req.LoginID),
                new SqlParameter("@FromDate", _req.FromDate_F??DateTime.Now.ToString("dd MMM yyyy")),
                new SqlParameter("@ToDate", _req.ToDate_F??DateTime.Now.ToString("dd MMM yyyy")),
                new SqlParameter("@AreaID", _req.AreaID),
                new SqlParameter("@UType", _req.UType),
                new SqlParameter("@IsArea", _req.IsArea)
            };

            var _res = new List<AccountSummary>();
            
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        _res.Add(new AccountSummary{
                            OutletName = dr["_OutletName"] is DBNull ? string.Empty : dr["_OutletName"].ToString(),
                            Mobile = dr["_Mobile"] is DBNull ? string.Empty : dr["_Mobile"].ToString(),
                            UserID = dr["_UserID"] is DBNull ? 0 : Convert.ToInt32(dr["_UserID"]),
                            Opening = dr["_Opening"] is DBNull ? 0 : Convert.ToDecimal(dr["_Opening"]),
                            Sales = dr["_Sales"] is DBNull ? 0 : Convert.ToDecimal(dr["_Sales"]),
                            CCollection = dr["_CCollection"] is DBNull ? 0 : Convert.ToDecimal(dr["_CCollection"]),
                            Closing = dr["_Closing"] is DBNull ? 0 : Convert.ToDecimal(dr["_Closing"]),
                            Lsale = dr["_LSale"] is DBNull ? 0 : Convert.ToDecimal(dr["_LSale"]),
                            LCollection = dr["_LCollection"] is DBNull ? 0 : Convert.ToDecimal(dr["_LCollection"]),
                            LSDate = dr["_LSDate"] is DBNull ? "NA" : dr["_LSDate"].ToString(),
                            LCDate = dr["_LCDate"] is DBNull ? "NA" : dr["_LCDate"].ToString(),
                            Area = dr["_Area"] is DBNull ? string.Empty : dr["_Area"].ToString(),
                            IsPrepaid = dr["_IsPrepaid"] is DBNull ? false : Convert.ToBoolean(dr["_IsPrepaid"]),
                            IsUtility = dr["_IsUtility"] is DBNull ? false : Convert.ToBoolean(dr["_IsUtility"]),
                            Balance = dr["_Balance"] is DBNull ? 0 : Convert.ToDecimal(dr["_Balance"]),
                            UBalance = dr["_UBalance"] is DBNull ? 0 : Convert.ToDecimal(dr["_UBalance"])
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
                    LoginTypeID = _req.LT,
                    UserId = _req.LoginID
                });
            }
            return _res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_ASCSummary";
    }
}
