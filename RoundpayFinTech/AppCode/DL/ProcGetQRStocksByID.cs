using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{

    public class ProcGetQRStocksByID : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetQRStocksByID(IDAL dal) => _dal = dal;
        public string GetName() => "proc_GetQRStocksByID";
        
        public object Call(object obj) {
            var _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@RefID", _req.CommonInt)
            };
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt != null && dt.Rows.Count > 0)
                {
                    _resp.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _resp.Msg = dt.Rows[0]["Msg"].ToString();
                    if (_resp.Statuscode == ErrorCodes.One)
                        _resp.CommonStr = dt.Rows[0]["_TransactionID"] is DBNull ? string.Empty : dt.Rows[0]["_TransactionID"].ToString();
                        _resp.CommonStr2 = dt.Rows[0]["_BankRefID"] is DBNull ? string.Empty : dt.Rows[0]["_BankRefID"].ToString();
                        _resp.CommonInt = dt.Rows[0]["_AssignedTo"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_AssignedTo"]);
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
            }
            return _resp;
        }

        public object Call() => throw new NotImplementedException();
        
    }


    public class ProcUpdateQRBankRefID : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateQRBankRefID(IDAL dal) => _dal = dal;
        public string GetName() => "Proc_UpdateQRBankRefID";

        public object Call(object obj)
        {
            var _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@RefID", _req.CommonInt),
                new SqlParameter("@TransactionID", _req.CommonStr),
                new SqlParameter("@BankRefID", _req.CommonStr2)
            };
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt != null && dt.Rows.Count > 0)
                {
                    _resp.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _resp.Msg = dt.Rows[0]["Msg"].ToString();
                    if (_resp.Statuscode == ErrorCodes.One)
                        _resp.CommonStr = dt.Rows[0]["_TransactionID"] is DBNull ? string.Empty : dt.Rows[0]["_TransactionID"].ToString();
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
            }
            return _resp;
        }

        public object Call() => throw new NotImplementedException();

    }

}
