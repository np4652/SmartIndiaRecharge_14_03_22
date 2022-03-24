using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcUpdateMiniBank : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateMiniBank(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var request = (MiniBankTransactionServiceResp)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",request.LT),
                new SqlParameter("@LoginID",request.LoginID),
                new SqlParameter("@TID",request.TID),
                new SqlParameter("@Type",request.Statuscode),
                new SqlParameter("@VendorID",request.VendorID??string.Empty),
                new SqlParameter("@LiveID",request.LiveID??string.Empty),
                new SqlParameter("@IPAddress",request.IPAddress??string.Empty),
                new SqlParameter("@Browser",request.Browser??string.Empty),
                new SqlParameter("@Remark",request.Remark??string.Empty),
                new SqlParameter("@Request",request.Req??string.Empty),
                new SqlParameter("@Response",request.Resp??string.Empty),
                new SqlParameter("@RequestPage",request.RequestPage??string.Empty),
                new SqlParameter("@Amount",request.Amount),
                new SqlParameter("@CardNumber",request.CardNumber??string.Empty),
                new SqlParameter("@CardBalance",request.BankBalance??"0.0"),
                new SqlParameter("@BankName",request.BankName??string.Empty),
                new SqlParameter("@APIRequestID",request.APIRequestID??string.Empty)
            };
            var responseStatus = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    responseStatus.Statuscode = dt.Rows[0][0] is DBNull ? ErrorCodes.Minus1 : Convert.ToInt32(dt.Rows[0][0]);
                    responseStatus.Msg = dt.Rows[0]["Msg"] is DBNull ? string.Empty : dt.Rows[0]["Msg"].ToString();
                    if (responseStatus.Statuscode == ErrorCodes.One)
                    {
                        responseStatus.CommonStr= dt.Rows[0]["_UpdateCallback"] is DBNull ? string.Empty : dt.Rows[0]["_UpdateCallback"].ToString();
                        responseStatus.CommonInt = dt.Rows[0]["_UserID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_UserID"]);
                        responseStatus.CommonStr2 = dt.Rows[0]["_TransactionID"] is DBNull ? string.Empty : dt.Rows[0]["_TransactionID"].ToString();
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
                    LoginTypeID = request.LT,
                    UserId = request.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return responseStatus;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "proc_UpdateMiniBank";
    }
    public class ProcMiniBankGetDBStatus : IProcedure
    {
        private readonly IDAL _dal;
        public ProcMiniBankGetDBStatus(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var TID = (int)obj;
            SqlParameter[] param = {
                new SqlParameter("@TID",TID)
            };
            try
            {
                var dt = _dal.Get(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    return dt.Rows[0]["_APICode"] is DBNull ? string.Empty : dt.Rows[0]["_APICode"].ToString();
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
                    UserId = TID
                });
            }
            return string.Empty;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName() => "declare @APIID int;select @APIID=_APIID from tbl_Transaction(nolock) where _TID=@TID;select _APICode from tbl_API where _ID =@APIID;";
    }
}
