using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Report;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcPrepareResendTransactions : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcPrepareResendTransactions(IDAL dal) => _dal = dal;
        public async Task<object> Call(object obj)
        {
            var _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@APIID",_req.CommonInt),
                new SqlParameter("@TIDs", _req.str?? ""),
                new SqlParameter("@IP", _req.CommonStr?? ""),
                new SqlParameter("@Browser", _req.CommonStr2 ?? "")
            };
            var _lst = new List<PrepairedTransactionReq>();
            var _res = new PrepairedTransactionReq
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
                        foreach (DataRow row in dt.Rows)
                        {
                            _res = new PrepairedTransactionReq
                            {
                                TID = row["TID"] is DBNull ? 0 : Convert.ToInt32(row["TID"]),
                                WID = row["_WID"] is DBNull ? 0 : Convert.ToInt32(row["_WID"]),
                                TransactionID = row["_TransactionID"] is DBNull ? string.Empty : row["_TransactionID"].ToString(),
                                Operator = row["_Operator"] is DBNull ? string.Empty : row["_Operator"].ToString(),
                                CustomerNumber = row["_CustomerNumber"] is DBNull ? string.Empty : row["_CustomerNumber"].ToString(),
                                O15 = row["_O15"] is DBNull ? string.Empty : row["_O15"].ToString(),
                                IsBBPS = row["_IsBBPS"] is DBNull ? false : Convert.ToBoolean(row["_IsBBPS"]),
                                UserID = row["_UserID"] is DBNull ? 0 : Convert.ToInt32(row["_UserID"]),
                                AccountNo = row["_Account"] is DBNull?string.Empty: row["_Account"].ToString(),
                                RequestedAmount = row["_RequestedAmount"] is DBNull?0: Convert.ToDecimal(row["_RequestedAmount"]),
                                OID = row["_OID"] is DBNull?0: Convert.ToInt32(row["_OID"]),
                                Optional1 = row["_Optional1"] is DBNull ? string.Empty : row["_Optional1"].ToString(),
                                Optional2 = row["_Optional2"] is DBNull ? string.Empty : row["_Optional2"].ToString(),
                                Optional3 = row["_Optional3"] is DBNull ? string.Empty : row["_Optional3"].ToString(),
                                Optional4 = row["_Optional4"] is DBNull ? string.Empty : row["_Optional4"].ToString(),
                                aPIDetail = new APIDetail()
                            };
                            _res.aPIDetail.ID = Convert.ToInt32(row["_APIID"]);
                            _res.aPIDetail.Name = row["_APIName"].ToString();
                            _res.aPIDetail.URL = row["_URL"] is DBNull ? "" : row["_URL"].ToString();
                            _res.aPIDetail.APIType = Convert.ToInt16(row["_APIType"] is DBNull ? false : row["_APIType"]);
                            _res.aPIDetail.Name = row["_APIName"] is DBNull ? "" : row["_APIName"].ToString();
                            _res.aPIDetail.RequestMethod = row["_RequestMethod"] is DBNull ? "" : row["_RequestMethod"].ToString();
                            _res.aPIDetail.StatusName = row["_StatusName"] is DBNull ? "" : row["_StatusName"].ToString();
                            _res.aPIDetail.SuccessCode = row["_SuccessCode"] is DBNull ? "" : row["_SuccessCode"].ToString();
                            _res.aPIDetail.LiveID = row["_LiveID"] is DBNull ? "" : row["_LiveID"].ToString();
                            _res.aPIDetail.VendorID = row["_VendorID"] is DBNull ? "" : row["_VendorID"].ToString();
                            _res.aPIDetail.ResponseTypeID = Convert.ToInt32(row["_ResponseTypeID"] is DBNull ? false : row["_ResponseTypeID"]);
                            _res.aPIDetail.APIOpCode = row["APIOPCode"] is DBNull ? "" : row["APIOPCode"].ToString();
                            _res.aPIDetail.CommType = row["APIComType"] is DBNull ? false : Convert.ToBoolean(row["APIComType"]);
                            _res.aPIDetail.MsgKey = row["_MsgKey"] is DBNull ? "" : row["_MsgKey"].ToString();
                            _res.aPIDetail.ErrorCodeKey = row["_ErrorCodeKey"] is DBNull ? "" : row["_ErrorCodeKey"].ToString();
                            _res.aPIDetail.GroupID = row["_GroupID"] is DBNull ? 0 : Convert.ToInt32(row["_GroupID"]);
                            _res.aPIDetail.GroupCode = row["_GroupCode"] is DBNull ? "" : row["_GroupCode"].ToString();
                            _lst.Add(_res);
                        }
                    }
                    else
                    {
                        _lst.Add(_res);
                    }
                }
                else
                {
                    _lst.Add(_res);
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = _req.LoginTypeID,
                    UserId = _req.LoginID
                });
                _lst.Add(_res);
            }
            return _lst;
        }
        public Task<object> Call() => throw new NotImplementedException();
        public string GetName() => "proc_PrepareResendTransactions";
    }
}