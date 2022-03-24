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
    public class ProcSaveWhatsappSendMessage : IProcedure
    {
        private readonly IDAL _dal;
        public ProcSaveWhatsappSendMessage(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var res = new WhatsappConversation
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            var req = (WhatsappConversation)obj;
            if (req.Text != null)
            {
                req.Text = req.Text.Replace("\n", "<br/>");
            }
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@MobileNo",(req.ContactId)),
                new SqlParameter("@SenderName",req.SenderName),
                new SqlParameter("@Type",req.Type),
                new SqlParameter("@StatusString",req.StatusString),
                new SqlParameter("@Text",req.Text),
                new SqlParameter("@Data",req.Data),
                new SqlParameter("@CCId",req.CCID),
                new SqlParameter("@CCName",req.CCName),
                new SqlParameter("@ForwardMsgID",req.Id),
                new SqlParameter("@ApiCode",req.APICODE),
                new SqlParameter("@SenderNo",req.SenderNo),
                new SqlParameter("@QuotedconvertationID",req.conversationId)
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? string.Empty : dt.Rows[0]["Msg"].ToString();
                    if (res.Statuscode == 1)
                    {
                        res.APIURL = dt.Rows[0]["APIURL"] is DBNull ? string.Empty : dt.Rows[0]["APIURL"].ToString();
                        res.MessageID = Convert.ToInt32(dt.Rows[0]["MessageId"]);
                        res.Type = req.Type;
                        res.Data = req.Data;
                        res.FileName = req.FileName;
                        res.SenderNo = dt.Rows[0]["_SenderNo"] is DBNull ? string.Empty : dt.Rows[0]["_SenderNo"].ToString();
                        if (req.Text != null)
                        {
                            req.Text = req.Text.Replace("<br/>", "\n");
                        }
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
                    UserId = req.LoginTypeID
                });
            }
            return res ?? new WhatsappConversation();
        }

        public object Call() => throw new NotImplementedException();


        public string GetName() => "Proc_SaveWhatsappSendMessage";
    }
}
