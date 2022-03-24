using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcUpdateWhatsappBotDic : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateWhatsappBotDic(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var req = (WhatsappBotDic)obj;
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };

            SqlParameter[] param = {
                new SqlParameter("@LT", req.LT),
                new SqlParameter("@LoginId",req.LoginID),
                new SqlParameter("@KeyId", req.KeyId),
                new SqlParameter("@Key", req.Key??string.Empty),
                new SqlParameter("@FormatType", req.FormatType??string.Empty),
                new SqlParameter("@ReplyType", req.ReplyType??string.Empty),
                new SqlParameter("@ReplyText1", req.ReplyText1??string.Empty),
                new SqlParameter("@ReplyText2",req.ReplyText2??string.Empty),
                new SqlParameter("@ReplyText3",req.ReplyText3??string.Empty),
                new SqlParameter("@IsActive",req.IsActive),
                new SqlParameter("@Action",req.Action??string.Empty),
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt16(dt.Rows[0]["StatusCode"]);
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
        public string GetName() => "proc_UpdateWhatsappBotDic";
    }
}
