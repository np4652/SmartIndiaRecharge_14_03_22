using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcDeleteWebNotification : IProcedure
    {
        private readonly IDAL _dal;
        public ProcDeleteWebNotification(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (Fintech.AppCode.Model.CommonReq)obj;
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), 
                    new SqlParameter[] {
                        new SqlParameter("@IDs", req.CommonStr),
                        new SqlParameter("@Action", req.CommonInt),
                });
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    res.Msg = Convert.ToString(dt.Rows[0]["Msg"]);
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                res.Statuscode = ErrorCodes.Minus1;
                res.Msg = ErrorCodes.UNSUCCESS;
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "Proc_DeleteWebNotification";
    }
}
