using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcVerifyDMTAccount : IProcedure
    {
        private readonly IDAL _dal;
        public ProcVerifyDMTAccount(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (DMRTransactionRequest)obj;
            SqlParameter[] param = {
                new SqlParameter("@UserID", _req.UserID),
                new SqlParameter("@OutletID", _req.OutletID),
                new SqlParameter("@AccountNo", _req.AccountNo ?? string.Empty),
                new SqlParameter("@APIRequestID", _req.APIRequestID ?? string.Empty),
                new SqlParameter("@RequestModeID", _req.RequestModeID),
                new SqlParameter("@RequestIP", _req.RequestIP??string.Empty),
                new SqlParameter("@SenderID", _req.SenderNo),
                new SqlParameter("@APIID", _req.APIID),
                new SqlParameter("@IFSC", _req.IFSC),
                new SqlParameter("@IMEI", _req.IMEI??string.Empty),
                new SqlParameter("@BankID",_req.BankID),
                new SqlParameter("@OID",_req.OID),
                new SqlParameter("@AccountTableID",_req.AccountTableID),
                new SqlParameter("@IsInternal",_req.IsInternal)
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
                        _res.AccountHolder = dt.Rows[0]["_AccountHolder"] is DBNull ? string.Empty : dt.Rows[0]["_AccountHolder"].ToString();
                        _res.LiveID = dt.Rows[0]["_LiveID"] is DBNull ? string.Empty : dt.Rows[0]["_LiveID"].ToString();
                        _res.Bank = dt.Rows[0]["_Bank"] is DBNull ? string.Empty : dt.Rows[0]["_Bank"].ToString();
                        _res.IsIMPS = Convert.ToInt16(dt.Rows[0]["_IsIMPS"] is DBNull ? 0 : dt.Rows[0]["_IsIMPS"]);
                        _res.IsNEFT = Convert.ToInt16(dt.Rows[0]["_IsNEFT"] is DBNull ? 0 : dt.Rows[0]["_IsNEFT"]);
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
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = _req.UserID
                });
            }
            return _res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_VerifyDMTAccount";
    }
}
