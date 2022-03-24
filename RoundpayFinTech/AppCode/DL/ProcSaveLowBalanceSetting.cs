using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcSaveLowBalanceSetting : IProcedure
    {
        private readonly IDAL _dal;
        public ProcSaveLowBalanceSetting(IDAL dal) => _dal = dal;
        public string GetName() => "proc_SaveLowBalanceSetting";
        public object Call(object obj)
        {
            var req = (LowBalanceSetting)obj;
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            SqlParameter[] param = {
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@LT",req.LT),
                new SqlParameter("@UserID",req.LoginID),
                new SqlParameter("@AlertBalance",req.AlertBalance),
                new SqlParameter("@MobileNos",req.MobileNos??""),
                new SqlParameter("@Emails",req.Emails??""),
                new SqlParameter("@WhatsappNo",req.WhatsappNo??""),
                new SqlParameter("@TelegramNo",req.TelegramNo??""),
                new SqlParameter("@HangoutId",req.HangoutId??"")
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LT,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }
        public object Call() => throw new NotImplementedException();

    }
}
