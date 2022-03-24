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
    public class ProcBonafideAccountSetting : IProcedure
    {
        private readonly IDAL _dal;
        public ProcBonafideAccountSetting(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            var _req = (BonafideAccount)obj;
            SqlParameter[] param = {
                   new SqlParameter("@LoginID", _req.LoginID),
                   new SqlParameter("@LT", _req.LTID),
                   new SqlParameter("@AccountID",_req.ID),
                   new SqlParameter("@IsDelete",_req.IsDelete),

        };
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _resp.Statuscode = Convert.ToInt16(dt.Rows[0][0]);
                    _resp.Msg = dt.Rows[0]["Msg"].ToString();
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = _req.LTID,
                    UserId = _req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _resp;
        }
        public object Call()
        {
            throw new NotImplementedException();
        }
        public string GetName()
        {
            return "proc_BonafideAccountSetting";
        }
    }
}