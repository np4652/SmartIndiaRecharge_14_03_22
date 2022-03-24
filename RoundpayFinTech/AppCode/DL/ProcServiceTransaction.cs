using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.StaticModel;
using System;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcServiceTransaction : IProcedure
    {
        private readonly IDAL _dal;
        public ProcServiceTransaction(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (ServiceTransactionRequest)obj;
            SqlParameter[] param = {
                new SqlParameter("@AccountNo", _req.AccountNo ?? ""),
                new SqlParameter("@AmountR", _req.AmountR),
                new SqlParameter("@APIRequestID", _req.APIRequestID ?? ""),
                new SqlParameter("@RequestModeID", _req.RequestModeID),
                new SqlParameter("@RequestIP", _req.RequestIP??string.Empty),
                new SqlParameter("@APIID", _req.APIID),
                new SqlParameter("@OPType", _req.OPType),
                new SqlParameter("@IMEI", _req.IMEI??""),
                new SqlParameter("@VendorID", _req.VenderID??""),
                new SqlParameter("@Token", _req.Token??string.Empty),
                new SqlParameter("@RefferenceID", _req.RefferenceID),
                new SqlParameter("@OTP", HashEncryption.O.DevEncrypt(_req.OTP??string.Empty)),
                new SqlParameter("@RequestSession", _req.RequestSession??string.Empty),
                new SqlParameter("@IsBalance", _req.IsBalance)
           };
            var _res = new ResponseStatusBalnace
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _res.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _res.Msg = dt.Rows[0]["Msg"] is DBNull ? string.Empty : dt.Rows[0]["Msg"].ToString();
                    if (_res.Statuscode == ErrorCodes.One)
                    {
                        _res.TransactionID = dt.Rows[0]["TransactionID"] is DBNull ? string.Empty : dt.Rows[0]["TransactionID"].ToString();
                        _res.Balance = dt.Rows[0]["Balance"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["Balance"]);
                       // _res.UserMobile = dt.Rows[0]["UserMobile"] is DBNull ? string.Empty : dt.Rows[0]["UserMobile"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                _res.Msg = ex.ToString();
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_ServiceTransaction";
    }
}