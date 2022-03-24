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
    public class ProcUpdateWhatsappTask : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUpdateWhatsappTask(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@ContactID",(req.CommonInt)),
                new SqlParameter("@Task",req.CommonInt2)
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? string.Empty : dt.Rows[0]["Msg"].ToString();
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
            return res;
        }

        public object Call() => throw new NotImplementedException();


        public string GetName() => "proc_UpdateWhatsappTask";
    }



    public class procSelectWhatsappSenderNoService : IProcedure
    {
        private readonly IDAL _dal;
        public procSelectWhatsappSenderNoService(IDAL dal) => _dal = dal;
        public object Call()
        {
            var getWhatsappContacts = new List<WhatsappServiceSenderNo>();
            var resp = new WhatsappService
            {
                WhatsappServiceSenderNoList = getWhatsappContacts
               
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName());
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        var GetWhatsappServiceSenderNo = new WhatsappServiceSenderNo
                        {
                            URL = row["_Url"] is DBNull ? "" : row["_Url"].ToString(),
                            SenderNo = row["_SenderNo"] is DBNull ? "" : row["_SenderNo"].ToString(),
                            PassedChatTime = row["_ConversationDiff"] is DBNull ? "" : row["_ConversationDiff"].ToString(),
                        };
                        getWhatsappContacts.Add(GetWhatsappServiceSenderNo);
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
                    LoginTypeID = 0,
                    UserId =0
                });
            }
            return getWhatsappContacts;
        }

        public object Call(object obj) => throw new NotImplementedException();


        public string GetName() => "proc_SelectWhatsappSenderNoService";
    }
}
