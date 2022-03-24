using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcMakeRefundRequest : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcMakeRefundRequest(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var _req = (RefundRequestReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@TID", _req.refundRequest.TID),
                new SqlParameter("@TransactionID", _req.refundRequest.RPID),
                new SqlParameter("@IP", _req.CommonStr),
                new SqlParameter("@Browser", _req.CommonStr2)
            };

            var _res = new RefundRequestResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param).ConfigureAwait(false);
                if (dt.Rows.Count > 0)
                {
                    _res.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _res.Msg = dt.Rows[0]["Msg"].ToString();

                    if (_res.Statuscode == ErrorCodes.One)
                    {
                        _res.Account = dt.Rows[0]["_Account"] is DBNull ? string.Empty : dt.Rows[0]["_Account"].ToString();
                        _res.Amount = dt.Rows[0]["_Amount"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["_Amount"]);
                        _res.Type = dt.Rows[0]["_Type"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_Type"]);
                        _res.RefundStatus = dt.Rows[0]["_RefundStatus"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_RefundStatus"]);
                        _res.RefundRemark = dt.Rows[0]["_RefundRemark"] is DBNull ? string.Empty : dt.Rows[0]["_RefundRemark"].ToString();
                        _res.Balance = dt.Rows[0]["_Balance"] is DBNull ? 0 : Convert.ToDecimal(dt.Rows[0]["_Balance"]);
                        _res.DisputeURL = dt.Rows[0]["DisputeURL"] is DBNull ? string.Empty : dt.Rows[0]["DisputeURL"].ToString();
                        _res.TID = dt.Rows[0]["_TID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_TID"]);
                        _res.VendorID = dt.Rows[0]["_VendorID"] is DBNull ? string.Empty : dt.Rows[0]["_VendorID"].ToString();
                        _res.TransactionID = dt.Rows[0]["_TransactionID"] is DBNull ? string.Empty : dt.Rows[0]["_TransactionID"].ToString();
                        _res.APICode = dt.Rows[0]["_APICode"] is DBNull ? string.Empty : dt.Rows[0]["_APICode"].ToString();
                        _res.Optional2 = dt.Rows[0]["_Optional2"] is DBNull ? string.Empty : dt.Rows[0]["_Optional2"].ToString();
                        _res.ServiceID = dt.Rows[0]["_ServiceID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_ServiceID"]);
                        _res.APIID = dt.Rows[0]["_APIID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_APIID"]);
                        _res.UserID = dt.Rows[0]["_UserID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_UserID"]);
                        _res.APIOutletID = dt.Rows[0]["_APIOutletID"] is DBNull ? string.Empty : dt.Rows[0]["_APIOutletID"].ToString();
                        _res.IsBBPS = dt.Rows[0]["_IsBBPS"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsBBPS"]);
                        _res.TransactionReqID = dt.Rows[0]["_TransactionReqID"] is DBNull ? string.Empty : dt.Rows[0]["_TransactionReqID"].ToString();
                        _res.MobileNo = dt.Rows[0]["_MobileNo"] is DBNull ? string.Empty : dt.Rows[0]["_MobileNo"].ToString();
                        _res.GroupIID = dt.Rows[0]["_GroupID"] is DBNull ? string.Empty : dt.Rows[0]["_GroupID"].ToString();
                        _res.OutletID = dt.Rows[0]["OutletID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["OutletID"]);
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
            return _res;
        }

        public Task<object> Call() => throw new NotImplementedException();

        public string GetName() => "proc_MakeRefundRequest";
    }
}
