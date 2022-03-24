using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{

    public class ProcSelectWhatsappMsgByConvID : IProcedure
    {
        private readonly IDAL _dal;
        public ProcSelectWhatsappMsgByConvID(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var res = new WhatsappConversation
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            var req = (WhatsappConversation)obj;
            SqlParameter[] param = {
                              new SqlParameter("@ConversationID",req.conversationId)
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                res.Statuscode = 1;
                res.Msg = "Success";
                res.Text = req.Text;
                res.conversationId = req.conversationId;
                res.LoginTypeID =1;
                res.Id = 0;
                res.Type = "text";
                res.ContactId = dt.Rows[0]["_WAID_GroupID"] is DBNull ? string.Empty : dt.Rows[0]["_WAID_GroupID"].ToString();
                res.SenderName = dt.Rows[0]["_SenderName"] is DBNull ? string.Empty : dt.Rows[0]["_SenderName"].ToString();
                res.APICODE = dt.Rows[0]["_ApiCode"] is DBNull ? string.Empty : dt.Rows[0]["_ApiCode"].ToString();
                res.SenderNo = dt.Rows[0]["_SenderNo"] is DBNull ? string.Empty : dt.Rows[0]["_SenderNo"].ToString();
                res.CCID = dt.Rows[0]["_CustomreCareID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_CustomreCareID"]);
                res.CCName = dt.Rows[0]["_CustomreCareName"] is DBNull ? string.Empty : dt.Rows[0]["_CustomreCareName"].ToString();
                    //QuotedReply params
                res.QuoteMsg = dt.Rows[0]["_Text"] is DBNull ? string.Empty : dt.Rows[0]["_Text"].ToString();
                res.ReplyJID = dt.Rows[0]["_WAMobileNo"] is DBNull ? string.Empty : dt.Rows[0]["_WAMobileNo"].ToString();
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = 1
                });
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();


        public string GetName() => "prco_SelectWhatsappMsgByConvID";
    }
}
