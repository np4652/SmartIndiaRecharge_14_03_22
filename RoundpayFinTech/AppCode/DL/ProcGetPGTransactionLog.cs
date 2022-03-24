using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetPGTransactionLog : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetPGTransactionLog(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@TID",req.CommonInt),
                new SqlParameter("@TransactionID",req.CommonStr??string.Empty),
                new SqlParameter("@VendorID",req.CommonStr2??string.Empty)
            };
            var res = new TransactionPGLog();
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.TID = dt.Rows[0]["_TID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_TID"]);
                    res.PGID = dt.Rows[0]["_PGID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_PGID"]);
                    res.TransactionID = dt.Rows[0]["_TransactionID"] is DBNull ? string.Empty : dt.Rows[0]["_TransactionID"].ToString();
                    res.Checksum = dt.Rows[0]["_Checksum"] is DBNull ? string.Empty : dt.Rows[0]["_Checksum"].ToString();
                    res.StatuscheckURL = dt.Rows[0]["_StatuscheckURL"] is DBNull ? string.Empty : dt.Rows[0]["_StatuscheckURL"].ToString();
                    res.VendorID = dt.Rows[0]["_VendorID"] is DBNull ? string.Empty : dt.Rows[0]["_VendorID"].ToString();
                    res.MerchantID = dt.Rows[0]["_MerchantID"] is DBNull ? string.Empty : dt.Rows[0]["_MerchantID"].ToString();
                    res.MerchantKEY = dt.Rows[0]["_MerchantKEY"] is DBNull ? string.Empty : dt.Rows[0]["_MerchantKEY"].ToString();
                    res.Amount = dt.Rows[0]["_Amount"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_Amount"]);
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.CommonInt,
                    UserId = req.CommonInt
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_GetPGTransactionLog";
    }
}
