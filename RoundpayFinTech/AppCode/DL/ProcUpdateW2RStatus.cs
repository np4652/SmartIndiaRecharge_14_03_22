using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcUpdateW2RStatus : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateW2RStatus(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            RefundRequestData _req = (RefundRequestData)obj;
            SqlParameter[] param = {
             new SqlParameter("@LoginID", _req.LoginID),
             new SqlParameter("@TID", _req.TID),
             new SqlParameter("@RefundStatus", _req.RefundStatus),
             new SqlParameter("@Remark", _req.Remark ?? ""),
             new SqlParameter("@IP", _req.RequestIP ?? ""),
             new SqlParameter("@Browser", _req.Browser ?? "")
            };
            var _res = new ResponseStatus {
                Statuscode=ErrorCodes.Minus1,
                Msg=ErrorCodes.TempError
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _res.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _res.Msg = dt.Rows[0]["Msg"].ToString();
                }
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

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "proc_UpdateW2RStatus";
        }
    }
}