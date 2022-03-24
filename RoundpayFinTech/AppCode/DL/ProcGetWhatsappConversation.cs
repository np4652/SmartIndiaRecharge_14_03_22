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
    public class ProcGetWhatsappConversation : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetWhatsappConversation(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@ContactId",req.CommonInt3),
                  new SqlParameter("@APIID",req.CommonInt),
                  new SqlParameter("@SenderNoID",req.CommonInt2),
            };
            var getWhatsappConversations = new List<WhatsappConversation>();
            var resp = new GetWhatsappContactListModel
            {
                WhatsappConversations = getWhatsappConversations
            };
            try
            {
                DataSet ds = _dal.GetByProcedureAdapterDS(GetName(), param);
                DataTable dt = new DataTable();
                //DataTable dtCus = new DataTable();
                 dt = ds.Tables[0];
                //dtCus = ds.Tables[1];
                if (dt.Rows.Count > 0 && !dt.Columns.Contains("Msg"))
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        var WhatsappConversation = new WhatsappConversation
                        {
                            Id = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                            ContactId = row["_ContactID"] is DBNull ? "" : row["_ContactID"].ToString(),
                            Text = row["_Text"] is DBNull ? "" : row["_Text"].ToString(),
                            EntryDate = row["_EntryDate"] is DBNull ? "" : row["_EntryDate"].ToString(),
                            CCID = row["_CCID"] is DBNull ? 0 : Convert.ToInt32(row["_CCID"]),
                            SenderNo = row["_SenderNo"] is DBNull ? "" : row["_SenderNo"].ToString(),
                            CCName = row["_CCName"] is DBNull ? "" : row["_CCName"].ToString(),
                            MessageTime = row["MessageTime"] is DBNull ? "" : row["MessageTime"].ToString(),
                            SenderName = row["_SenderName"] is DBNull ? "" : row["_SenderName"].ToString(),
                            StatusString = row["_StatusString"] is DBNull ? "" : row["_StatusString"].ToString(),
                            Type = row["_Type"] is DBNull ? "" : row["_Type"].ToString(),
                            Data = row["_Data"] is DBNull ? "" : row["_Data"].ToString(),
                            UnreadMessages = row["UnreadMessages"] is DBNull ? 0 : Convert.ToInt32(row["UnreadMessages"]),
                            IsSeen = row["_IsSeen"] is DBNull ? false : Convert.ToBoolean(row["_IsSeen"]),
                            MessageDate = row["_MessageDate"] is DBNull ? "" : row["_MessageDate"].ToString(),
                            Cdate = row["_CDate"] is DBNull ? "" : row["_CDate"].ToString(),
                            APICODE = row["_Apicode"] is DBNull ? "" : row["_Apicode"].ToString(),
                            GroupID = row["_GroupID"] is DBNull ? "" : row["_GroupID"].ToString(),
                            WAMobileNo = row["_WAMobileNo"] is DBNull ? "" : row["_WAMobileNo"].ToString(),
                            QuoteMsgID = row["_QuotedMsgID"] is DBNull ? "" : row["_QuotedMsgID"].ToString(),
                            QuoteMsg = row["_QuotedMsg"] is DBNull ? "" : row["_QuotedMsg"].ToString(),
                            QuoteMobileno = row["_QuoteMobileno"] is DBNull ? "" : row["_QuoteMobileno"].ToString(),
                            RemChatTime = row["_RemChatTime"] is DBNull ? "" : row["_RemChatTime"].ToString(),
                            conversationId = row["_ConversationID"] is DBNull ? "" : row["_ConversationID"].ToString(),
                            ReplyJID = row["_ReplyJID"] is DBNull ? "" : row["_ReplyJID"].ToString()
                        };
                        getWhatsappConversations.Add(WhatsappConversation);
                    }
                }
             
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                });
            }
            return resp;
        }

        public object Call() => throw new NotImplementedException();


        public string GetName() => "Proc_GetWhatsappConversation";

    }
}
