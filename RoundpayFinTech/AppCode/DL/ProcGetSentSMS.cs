using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetSentSMS : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetSentSMS(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            var res = new List<SentSmsResponse>();
            SentSMSRequest _req = (SentSMSRequest)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@Top", _req.Top),
                new SqlParameter("@MobileNo", _req.MobileNo.ToString() == "" ? null : _req.MobileNo.ToString()),
                new SqlParameter("@Message", _req.Message.ToString() == "" ? null : _req.Message.ToString())
            };
            
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        SentSmsResponse item = new SentSmsResponse
                        {
                            EntryDate = dt.Rows[i]["_EntryDate"].ToString(),
                            IsRead = Convert.ToBoolean(dt.Rows[i]["_IsRead"].ToString()),
                            Message = dt.Rows[i]["_Message"].ToString(),
                            MobileNo = dt.Rows[i]["_MobileNo"].ToString(),
                            Name = dt.Rows[i]["_Name"].ToString(),
                            Response = dt.Rows[i]["_Response"].ToString(),
                            TransactionId = dt.Rows[i]["_TransactionId"].ToString(),
                            ServiceType = dt.Rows[i]["_ServiceType"].ToString(),
                            ReqURL = (dt.Rows[i]["_Req"] is DBNull) || (_req.LoginTypeID == LoginType.CustomerCare) ? "No URL" : dt.Rows[i]["_Req"].ToString()  ,
                            Status= dt.Rows[i]["_Status"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[i]["_Status"])
                        };
                        res.Add(item);
                    }
                }
            }
            catch (Exception er)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = er.Message,
                    LoginTypeID = _req.LoginTypeID,
                    UserId = _req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);



            }
            return res;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "proc_GetSentSms";
        }
    }
}