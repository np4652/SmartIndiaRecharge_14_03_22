using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcUpdateMessageTemplatecs : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateMessageTemplatecs(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (MessageTemplateParam)obj;
            SqlParameter[] param = {
                new SqlParameter("@FormatID", _req.FormatID),
                new SqlParameter("@TemplateType", _req.TemplateType),
                new SqlParameter("@Subject", _req.Subject??string.Empty),
                new SqlParameter("@Template", _req.Template??string.Empty),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@LTID", _req.LoginTypeID),
                new SqlParameter("@IsEnableSMS", _req.IsEnableSMS),
                new SqlParameter("@IsEnableNotification", _req.IsEnableNotification),
                new SqlParameter("@IsEnableWebNotification", _req.IsEnableWebNotification),
                new SqlParameter("@IsEnableEmail", _req.IsEnableEmail),
                new SqlParameter("@IsEnableSocialAlert", _req.IsEnableSocialAlert),
                new SqlParameter("@IsHangoutAlert", _req.IsEnableHangout),
                new SqlParameter("@IsWhatsappAlert", _req.IsEnableWhatsApp),
                new SqlParameter("@IsTelegramAlert", _req.IsEnableTelegram),
                new SqlParameter("@APIID", _req.APIID),
                 new SqlParameter("@TemplateID", _req.TemplateID??string.Empty),
                 new SqlParameter("@WhatsAppTemplateID", _req.WhatsAppTemplateID??string.Empty),
                new SqlParameter("@WID", _req.WID)
            };
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _resp.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
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
                    LoginTypeID = _req.LoginTypeID,
                    UserId = _req.LoginID
                });
            }
            return _resp;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_UpdateMessageTemplate";
    }
}