using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetMessageFormat : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetMessageFormat(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            int FormatID = (int)obj;
            var _res = new MessageTemplate
            {
                Statuscode=ErrorCodes.Minus1,
                Msg=ErrorCodes.TempError
            };
            SqlParameter[] param = {
                new SqlParameter("@FormatID",FormatID),
            };
            try
            {
                DataTable dt = _dal.Get(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _res.Statuscode = ErrorCodes.One;
                    _res.Msg = ErrorCodes.SUCCESS;
                    _res.Subject = Convert.ToString(dt.Rows[0]["_Subject"]);
                    _res.Template = Convert.ToString(dt.Rows[0]["_Template"]);
                    _res.EmailTemplate = Convert.ToString(dt.Rows[0]["_EmailTemplate"]);
                    _res.AlertTemplate = Convert.ToString(dt.Rows[0]["_AlertTemplate"]);
                    _res.FormatID = Convert.ToInt32(dt.Rows[0]["_FormatID"]);
                    _res.IsEnableSMS = dt.Rows[0]["_IsEnableSMS"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsEnableSMS"]);
                    _res.IsEnableNotificaion = dt.Rows[0]["_IsEnableNotification"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsEnableNotification"]);
                    _res.IsEnableEmail = dt.Rows[0]["_IsEnableEmail"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsEnableEmail"]);
                    _res.IsEnableWebNotification = dt.Rows[0]["_IsEnableWebNotification"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsEnableWebNotification"]);
                    _res.WebNotificationTemplate = Convert.ToString(dt.Rows[0]["_WebNotificationTemplate"]);

                    _res.IsEnableWhatsApp = dt.Rows[0]["_IsWhatsappAlert"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsWhatsappAlert"]);
                    _res.WhatsAppTemplate = Convert.ToString(dt.Rows[0]["_WhatsappTemplate"]);
                    _res.WhatsappAPIID = dt.Rows[0]["_WhatsappAPIID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_WhatsappAPIID"]);
                    _res.IsEnableHangout = dt.Rows[0]["_IsHangoutAlert"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsHangoutAlert"]);
                    _res.HangoutTemplate = Convert.ToString(dt.Rows[0]["_HangoutTemplate"]);
                    _res.HangoutAPIID = dt.Rows[0]["_HangoutAPIID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_HangoutAPIID"]);

                    _res.IsEnableTelegram = dt.Rows[0]["_IsTelegramAlert"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsTelegramAlert"]);
                    _res.TelegramTemplate = Convert.ToString(dt.Rows[0]["_TelegramTemplate"]);
                    _res.TelegramAPIID = dt.Rows[0]["_TelegramAPIID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_TelegramAPIID"]);
                    _res.TemplateID = Convert.ToString(dt.Rows[0]["_TemplateID"]);
                    _res.WhatsAppTemplateID = Convert.ToString(dt.Rows[0]["_WhatsAppTemplateID"]);
                    _res.WID = dt.Rows[0]["_WID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_WID"]);
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = 0,
                    UserId = 0
                });
            }
            return _res;
           
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "select * from tbl_MessageTemplate where _FormatID=@FormatID";
    }
}
