using Fintech.AppCode.DB;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcUpdateLogOnboardingReqResp
    {
        private readonly IDAL _dal;
        public ProcUpdateLogOnboardingReqResp(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var _req = (OnboardingLog)obj;
            SqlParameter[] param = {
            new SqlParameter("@APIID", _req.APIID),
            new SqlParameter("@Request", _req.Request??""),
            new SqlParameter("@Response", _req.Response??""),
            new SqlParameter("@Method", _req.Method??"")
        };
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                _dal.Execute(GetName(), param);                
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = _req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName()=> "insert into Log_OnboardAPIReqResp(_APIID , _Method , _Request , _Response , _EntryDate )values(@APIID , @Method , @Request , @Response , Getdate())";
    }
}
