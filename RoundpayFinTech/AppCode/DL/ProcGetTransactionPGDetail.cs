using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetTransactionPGDetail : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetTransactionPGDetail(IDAL dal) => _dal = dal;
        public string GetName() => "proc_GetTransactionPGDetail";
        public object Call(object obj)
        {
            var _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@TID", _req.CommonInt),
                new SqlParameter("@VendorID", _req.CommonStr??string.Empty)
            };
            var _resp = new PGTransactionParam
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _resp.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _resp.Msg = Convert.ToString(dt.Rows[0]["Msg"]);
                    if (_resp.Statuscode == ErrorCodes.One)
                    {
                        _resp.StatusCheckURL = Convert.ToString(dt.Rows[0]["_StatusCheckURL"]);
                        _resp.MerchantID = Convert.ToString(dt.Rows[0]["_MerchantID"]);
                        _resp.MerchantKey = Convert.ToString(dt.Rows[0]["_MerchantKey"]);
                        _resp.MerchantID = Convert.ToString(dt.Rows[0]["_MerchantID"]);
                        _resp.TransactionID = Convert.ToString(dt.Rows[0]["_TransactionID"]);
                        _resp.TID = Convert.ToInt32(dt.Rows[0]["_TID"]);
                        _resp.VendorID = Convert.ToString(dt.Rows[0]["_VendorID"]);
                        _resp.OPID = Convert.ToString(dt.Rows[0]["_PaymentMode"]);
                        _resp.Checksum = Convert.ToString(dt.Rows[0]["_Checksum"]);
                        _resp.Signature = Convert.ToString(dt.Rows[0]["_Signature"]);
                        _resp.Remark = Convert.ToString(dt.Rows[0]["_Remark"]);
                        _resp.PAYMENTMODE = Convert.ToString(dt.Rows[0]["_PaymentMode"]);
                        _resp.Amount = dt.Rows[0]["_Amount"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_Amount"]);
                        _resp.PGID = Convert.ToInt32(dt.Rows[0]["_PGID"]);
                        _resp.UPGID = Convert.ToInt32(dt.Rows[0]["_UPGID"]);
                        _resp.RequestMode = Convert.ToInt32(dt.Rows[0]["_RequestMode"]);
                        _resp.Status = Convert.ToInt32(dt.Rows[0]["_Status"]);
                    }
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = _req.LoginTypeID,
                    UserId = _req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _resp;
        }
        public object Call() => throw new NotImplementedException();
    }
}