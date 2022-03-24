using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
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
    public class ProcMatchUserIDPassword : IProcedure
    {
        private readonly IDAL _dal;
        public ProcMatchUserIDPassword(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            CommonReq _req = (CommonReq)obj;
            SqlParameter[] param = {
                 new SqlParameter("@UserID", _req.LoginID),
                 new SqlParameter("@MerchantID", _req.str),
                 new SqlParameter("@Password",HashEncryption.O.Encrypt(_req.CommonStr)),
                 new SqlParameter("@MobileNo", _req.CommonStr2??""),
                 new SqlParameter("@RequestID", _req.CommonStr3??"")
        };
            IResponseStatus _res = new ResponseStatus
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
                    if (_res.Statuscode == ErrorCodes.One)
                    {
                        _res.Msg = ErrorCodes.SUCCESS;
                        _res.CommonInt = Convert.ToInt32(dt.Rows[0]["_UserID"]);
                        _res.CommonInt2 = Convert.ToInt32(dt.Rows[0]["_WID"]);
                        _res.CommonStr = dt.Rows[0]["_MobileNo"].ToString();
                        _res.CommonStr2 = dt.Rows[0]["_EmailID"].ToString();
                        _res.CommonStr3 = dt.Rows[0]["_TransactionID"].ToString();
                        _res.CommonStr4 = dt.Rows[0]["_OTP"].ToString();

                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = _req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "proc_checkUserValid";
        }

    }
   
}
