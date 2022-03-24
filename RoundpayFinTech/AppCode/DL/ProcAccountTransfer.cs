using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcAccountTransfer : IProcedure
    {

        private readonly IDAL _dal;
        public ProcAccountTransfer(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (DMRTransactionRequest)obj;
            SqlParameter[] param = {
                new SqlParameter("@UserID", _req.UserID),
                new SqlParameter("@OutletID", _req.OutletID),
                new SqlParameter("@AccountNo", _req.AccountNo ?? string.Empty),
                new SqlParameter("@AmountR", _req.AmountR),
                new SqlParameter("@APIRequestID", _req.APIRequestID ?? string.Empty),
                new SqlParameter("@RequestModeID", _req.RequestModeID),
                new SqlParameter("@RequestIP", _req.RequestIP??string.Empty),
                new SqlParameter("@Bank", _req.Bank??string.Empty),
                new SqlParameter("@SenderMobile", _req.SenderNo),
                new SqlParameter("@APIID", _req.APIID),
                new SqlParameter("@IFSC", _req.IFSC),
                new SqlParameter("@TransMode", _req.TransMode),
                new SqlParameter("@GroupID", _req.GroupID),
                new SqlParameter("@IMEI", _req.IMEI??string.Empty),
                new SqlParameter("@ExtraParam",_req.BeneName??string.Empty),
                new SqlParameter("@SecurityKey",HashEncryption.O.Encrypt(_req.SecureKey??string.Empty)),
                new SqlParameter("@BankID",_req.BankID),
                new SqlParameter("@OID",_req.OID),
                new SqlParameter("@APISenderLimit",_req.APISenderLimit),
                new SqlParameter("@AmountWithoutSplit",_req.AmountWithoutSplit),
                new SqlParameter("@CBA",_req.CBA),
                new SqlParameter("@TransactedAmount",_req.TransactedAmount)
           };
            var _res = new DMRTransactionResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError,
                ErrorCode = ErrorCodes.Unknown_Error
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _res.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _res.Msg = dt.Rows[0]["Msg"].ToString();
                    if (dt.Columns.Contains("_ErrorCode"))
                        _res.ErrorCode = dt.Rows[0]["_ErrorCode"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_ErrorCode"]);
                    if (_res.Statuscode == ErrorCodes.One)
                    {
                        _res.TID = dt.Rows[0]["TID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["TID"]);
                        _res.TransactionID = dt.Rows[0]["TransactionID"] is DBNull ? string.Empty : dt.Rows[0]["TransactionID"].ToString();
                        _res.Balance = dt.Rows[0]["Balance"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["Balance"]);
                        _res.BankCode = dt.Rows[0]["_BankCode"] is DBNull ? string.Empty : dt.Rows[0]["_BankCode"].ToString();
                        _res.IFSC = dt.Rows[0]["_IFSC"] is DBNull ? string.Empty : dt.Rows[0]["_IFSC"].ToString();
                        _res.EKOBankID = dt.Rows[0]["_EKO_BankID"] is DBNull ? string.Empty : dt.Rows[0]["_EKO_BankID"].ToString();
                        _res.PanNo = dt.Rows[0]["_PAN"] is DBNull ? string.Empty : dt.Rows[0]["_PAN"].ToString();
                        _res.Pincode = dt.Rows[0]["_PinCode"] is DBNull ? string.Empty : dt.Rows[0]["_PinCode"].ToString();
                        _res.LatLong = dt.Rows[0]["_Latlong"] is DBNull ? string.Empty : dt.Rows[0]["_Latlong"].ToString();
                        _res.IsIMPS = Convert.ToInt16(dt.Rows[0]["_IsIMPS"] is DBNull ? 0 : dt.Rows[0]["_IsIMPS"]);
                        _res.IsNEFT = Convert.ToInt16(dt.Rows[0]["_IsNEFT"] is DBNull ? 0 : dt.Rows[0]["_IsNEFT"]);
                        _res.IsInternalSender = Convert.ToBoolean(dt.Rows[0]["_IsInternalSender"] is DBNull ? 0 : dt.Rows[0]["_IsInternalSender"]);
                        _res.BrandName = dt.Rows[0]["_BrandName"] is DBNull ? string.Empty : dt.Rows[0]["_BrandName"].ToString();
                        _res.TransMode = dt.Rows[0]["_TransMode"] is DBNull ? string.Empty : dt.Rows[0]["_TransMode"].ToString();
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
                    LoginTypeID = 1,
                    UserId = _req.UserID
                });
            }
            return _res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_AccountTransfer";
    }
}
