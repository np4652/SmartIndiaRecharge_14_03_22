using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcUpdateAPIOpCodeCircle : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateAPIOpCodeCircle(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (APIOpCodeReq)obj;
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@OID", _req.aPIOpCode.OID),
                new SqlParameter("@APIID", _req.aPIOpCode.APIID),
                new SqlParameter("@OpCode", (_req.aPIOpCode.OpCode??string.Empty).Replace(" ","")),
                new SqlParameter("@IP", _req.CommonStr??string.Empty),
                new SqlParameter("@Browser", _req.CommonStr2??string.Empty),
                new SqlParameter("@CIrcleID", _req.aPIOpCode.CircleID)
            };

            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
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
                    LoginTypeID = _req.LoginTypeID,
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

        public string GetName() => "proc_UpdateAPIOpCodeCircle";
    }
}
