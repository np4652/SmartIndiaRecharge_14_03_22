using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data;
using System.Data.SqlClient;
using Newtonsoft.Json;
using System.Text;
using Fintech.AppCode.HelperClass;
using RoundpayFinTech.AppCode.StaticModel;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcSaveWhatsappReceiceMessage : IProcedure
    {
        private readonly IDAL _dal;
        public ProcSaveWhatsappReceiceMessage(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (WhatsappReceiveMsgResp)obj;
            var res = new WhatsappReceiveMsgResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                string mobInText = string.Empty;
                if (!string.IsNullOrEmpty(req.text))
                {
                    req.text = req.text.Replace("\r\n", " <br/> ");
                    mobInText = string.Join(",", ConverterHelper.O.seprateMobileNos(req.text));
                }
                if (string.IsNullOrEmpty(req.waId))
                {
                    return res;
                }
                SqlParameter[] param = {
                new SqlParameter("@MobileNo",(req.waId)),
                new SqlParameter("@SenderName",req.senderName),
                new SqlParameter("@Type",req.type),
                new SqlParameter("@StatusString",WhatsappStatusString.Received),
                new SqlParameter("@Text",req.text),
                new SqlParameter("@Data",req.data),
                new SqlParameter("@SenderNoID",req.SenderNoID),
                new SqlParameter("@conversationId",req.conversationId),
                new SqlParameter("@QuotedMsgID",req.QuotedMsgID),
                new SqlParameter("@ReceiveMsgMobileNo",req.ReceiveMsgMobileNo),
                new SqlParameter("@MobInText",mobInText)
            };
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    DataRow dr = dt.Rows[0];
                    res.Statuscode = dr["_StatusCode"] is DBNull ? 0 : Convert.ToInt32(dr["_StatusCode"]);
                    res.Msg = dr["_Msg"] is DBNull ? string.Empty : dr["_Msg"].ToString();
                    if (res.Statuscode == 1)
                    {
                        res.senderName = dr["_SenderName"] is DBNull ? string.Empty : dr["_SenderName"].ToString();
                        res.text = dr["_ReplyText"] is DBNull ? string.Empty : dr["_ReplyText"].ToString();
                        res.waId = dr["_waId"] is DBNull ? string.Empty : dr["_waId"].ToString();
                        res.senderName = dr["_senderName"] is DBNull ? string.Empty : dr["_senderName"].ToString();
                        res.CCID = dr["_CCID"] is DBNull ? 0 : Convert.ToInt32(dr["_CCID"]);
                        res.CCName = dr["_CCName"] is DBNull ? string.Empty : dr["_CCName"].ToString();
                        res.TransactionId = dr["_TransactionId"] is DBNull ? string.Empty : dr["_TransactionId"].ToString();
                        res.TID = dr["_TID"] is DBNull ? string.Empty : dr["_TID"].ToString();
                        res.FormatType = dr["_FormatType"] is DBNull ? string.Empty : dr["_FormatType"].ToString();
                        res.UserId = dr["_UserId"] is DBNull ? 0 : Convert.ToInt32(dr["_UserId"]);
                        res.TransactionAPIId = dr["_APIId"] is DBNull ? 0 : Convert.ToInt32(dr["_APIId"]);
                        res.GroupID = dr["_GroupID"] is DBNull ? "" : dr["_GroupID"].ToString();
                        res.SenderNO = dr["_SenderNo"] is DBNull ? "" : dr["_SenderNo"].ToString();
                        res.conversationId = dr["_ConvertationID"] is DBNull ? "" : dr["_ConvertationID"].ToString();
                        res.QuoteMsg = dr["_QuotedMessage"] is DBNull ? "" : dr["_QuotedMessage"].ToString();
                        res.ReplyJID = dr["_ReplyJID"] is DBNull ? "" : dr["_ReplyJID"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                string s = req == null ? "" : JsonConvert.SerializeObject(req);
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message + " ~  " + s,
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginTypeID
                });
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "Proc_SaveWhatsappReceiceMessage";
    }
}
