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
    public class ProcToggleStatus : IProcedure
    {
        private readonly IDAL _dal;
        public ProcToggleStatus(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            var _req = (ProcToggleStatusRequest)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@UserID", _req.UserID),
                new SqlParameter("@StatusColumn", _req.StatusColumn),
                new SqlParameter("@LTID", _req.LTID),
                new SqlParameter("@IP",_req.IP),
                new SqlParameter("@Browser",_req.Browser),
                new SqlParameter("@Is",_req.Is)
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
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = _req.LTID,
                    UserId = _req.LoginID
                });
            }
            return _resp;
        }
        public object Call()
        {
            throw new NotImplementedException();
        }
        public string GetName()
        {
            return "proc_ToggleStatus";
        }
    }
}