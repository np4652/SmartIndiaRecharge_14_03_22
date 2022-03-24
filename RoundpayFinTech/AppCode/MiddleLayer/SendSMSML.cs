using Fintech.AppCode;
using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Fintech.AppCode.WebRequest;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.Configuration;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data;
using System.IO;
using System.Net;
using System.Text;

namespace RoundpayFinTech.AppCode
{
    public class SendSMSML : ISendSMSML
    {
        private readonly IConfiguration Configuration;
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly ISession _session;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        private readonly IRequestInfo _rinfo;
        private readonly WebsiteInfo _WInfo;
        private readonly LoginResponse _lr;
        public SendSMSML(IDAL dal)
        {
            _dal = dal;
        }
        public SendSMSML(IHttpContextAccessor accessor, IHostingEnvironment env, bool IsInSession = true)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            _rinfo = new RequestInfo(_accessor, _env);
            if (IsInSession)
            {
                _session = _accessor.HttpContext.Session;
                bool IsProd = _env.IsProduction();
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory());
                builder.AddJsonFile((IsProd ? "appsettings.json" : "appsettings.Development.json"));
                builder.AddEnvironmentVariables();
                Configuration = builder.Build();
                _WInfo = new LoginML(_accessor, _env).GetWebsiteInfo();
                _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
            }

        }
        private object[] AddKeyAndValue(string Key, string Value)
        {
            return new object[] { Key, Value };
        }
        public void SendUserForget(string LoginID, string Password, string Pin, string MobileNo, string EmailID, int WID,string Logo)
        {
            DataTable Tp_ReplaceKeywords = new DataTable();
            Tp_ReplaceKeywords.Columns.Add("Keyword", typeof(string));
            Tp_ReplaceKeywords.Columns.Add("ReplaceValue", typeof(string));
            Tp_ReplaceKeywords.Rows.Add(AddKeyAndValue(MessageTemplateKeywords.LoginID, LoginID));
            Tp_ReplaceKeywords.Rows.Add(AddKeyAndValue(MessageTemplateKeywords.Password, Password));
            Tp_ReplaceKeywords.Rows.Add(AddKeyAndValue(MessageTemplateKeywords.PinPassword, Pin));
            SendSMS(Tp_ReplaceKeywords, MobileNo, EmailID, WID, true, "forget password", MessageFormat.ForgetPass,Logo);
        }
        public void SendUserReg(string LoginID, string Password, string MobileNo, string EmailID, int WID,string Logo)
        {
            try
            {
                DataTable Tp_ReplaceKeywords = new DataTable();
                Tp_ReplaceKeywords.Columns.Add("Keyword", typeof(string));
                Tp_ReplaceKeywords.Columns.Add("ReplaceValue", typeof(string));
                Tp_ReplaceKeywords.Rows.Add(AddKeyAndValue(MessageTemplateKeywords.LoginID, LoginID));
                Tp_ReplaceKeywords.Rows.Add(AddKeyAndValue(MessageTemplateKeywords.Password, Password));
                Tp_ReplaceKeywords.Rows.Add(AddKeyAndValue(MessageTemplateKeywords.PinPassword, string.Empty));
                SendSMS(Tp_ReplaceKeywords, MobileNo, EmailID, WID, true, "User Registration", MessageFormat.Registration, Logo);
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "SendUserReg",
                    Error = ex.Message,
                    LoginTypeID = 0,
                    UserId = WID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }

        }
        public void SendSMS(DataTable Tp_ReplaceKeywords, string MobileNo, string EmailID, int WID, bool WithMail, string MailSub, int FormatType,string Logo)
        {
            if (Tp_ReplaceKeywords == null) {
                Tp_ReplaceKeywords = new DataTable();
                Tp_ReplaceKeywords.Columns.Add("Keyword", typeof(string));
                Tp_ReplaceKeywords.Columns.Add("ReplaceValue", typeof(string));
                Tp_ReplaceKeywords.Rows.Add(AddKeyAndValue(MessageTemplateKeywords.LoginID, "0"));
            }
            var procSendSMS = new SMSSendREQ
            {
                FormatType = FormatType,
                MobileNo = MobileNo,
                Tp_ReplaceKeywords = Tp_ReplaceKeywords,
                WID = WID
            };
            SMSSendResp smsResponse = (SMSSendResp)SendSMS(procSendSMS);
            if (WithMail)
            {
                if (!string.IsNullOrEmpty(smsResponse.SMS) && !string.IsNullOrEmpty(EmailID))
                {
                    IEmailML emailManager = new EmailML(_dal);
                    emailManager.SendMail(EmailID, null, MailSub, smsResponse.SMS, WID,Logo);
                }
            }
        }
        public object SendSMS(SMSSendREQ _req)
        {
            IProcedure _p = new ProcSendSMS(_dal);
            object _o = _p.Call(_req);
            SMSSendResp _resp = (SMSSendResp)_o;
            _resp.IsSend = false;
            if (_resp.ResultCode > 0)
            {
                string ApiResp = "";
                try
                {
                    if (_resp.APIMethod == "GET")
                    {
                        ApiResp = AppWebRequest.O.CallUsingWebClient_GET(_resp.SmsURL, 0);
                        _resp.IsSend = true;
                    }
                    else if (_resp.APIMethod == "POST")
                    {
                        //To be implemented
                        _resp.IsSend = true;
                    }
                }
                catch (Exception ex)
                {
                    ApiResp = "Exception Occured! " + ex.Message;
                }
                _resp.ApiResp = ApiResp;
                SMSUpdateREQ updateRequest = null;
                updateRequest = new SMSUpdateREQ
                {
                    Response = _resp.ApiResp,
                    ResponseID = "error",
                    Status = SMSResponseTYPE.SEND,
                    SMSID = _resp.SMSID
                };

                UpdateSMSResponse(updateRequest);
            }
            return _resp;
        }
        public ResponseStatus SendSMSBulk(SMSSendBulk _resp)
        {
            ResponseStatus res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            DataTable dt = new DataTable();
            dt.Columns.Add("_MobileNo");
            dt.Columns.Add("_Request");
            dt.Columns.Add("_Respones");

            foreach (var item in _resp.MobileNo.Split(','))
            {
                DataRow dr = dt.NewRow();
                dr["_MobileNo"] = item;
                string req = _resp.SmsURL.Replace("{TO}", item).Replace("{MESSAGE}", _resp.SMS);
                dr["_Request"] = req;
                dr["_Respones"] = "";
                try
                {
                    if (_resp.APIMethod == "GET" && _resp.IsLapu == false)
                    {
                        dr["_Respones"] = AppWebRequest.O.CallUsingWebClient_GET(req);
                        _resp.IsSend = true;
                    }
                    else if (_resp.APIMethod == "POST")
                    {
                        //To be implemented
                        _resp.IsSend = true;
                    }
                }
                catch (Exception ex)
                {
                    dr["_Respones"] = ex.Message;
                }
                dt.Rows.Add(dr);
                var bulk = new SMSREqSendBulk
                {
                    ApiID = _resp.APIID,
                    IsLapu = _resp.IsLapu,
                    GeneralSMS = _resp.SMS,
                    Tp_ReplaceKeywords = dt,
                    WID = _resp.WID
                };
                IProcedure _p = new ProcSendSMSBulk(_dal);
                res = (ResponseStatus)_p.Call(bulk);
            }
            return res;
        }
        public void UpdateSMSResponse(SMSUpdateREQ _req)
        {
            IProcedure _p = new ProcSendSMSUpdate(_dal);
            object _o = _p.Call(_req);
        }
       
        public NotificationServiceReq GetNotificationSendDetail(CommonReq commonReq)
        {
            IProcedure proc = new ProcGetNotificationSendDetail(_dal);
            return (NotificationServiceReq)proc.Call(commonReq);
        }
        public string SendNotification(string mssg, string img, string title, string url)
        {
            string Respones = "";
            try
            {
                int id = new Random().Next(100000, 999999);
                System.Net.WebRequest tRequest = System.Net.WebRequest.Create("https://fcm.googleapis.com/fcm/send");
                tRequest.Method = "post";
                //serverKey - Key from Firebase cloud messaging server  
                tRequest.Headers.Add(string.Format("Authorization: key={0}", ApplicationSetting.FCMKey));
                //Sender Id - From firebase project setting  
                tRequest.Headers.Add(string.Format("Sender: id={0}", ApplicationSetting.FCMSenderID));
                tRequest.ContentType = "application/json";
                var payload = new
                {
                    to = "/topics/global",
                    collapse_key = "type_a",
                    data = new
                    {
                        Message = mssg,
                        Image = img,
                        Key = id,
                        Title = title,
                        Url = url,
                        Type = "Browsable_Notification"
                    }
                };

                string postbody = JsonConvert.SerializeObject(payload).ToString();
                byte[] byteArray = Encoding.UTF8.GetBytes(postbody);
                tRequest.ContentLength = byteArray.Length;
                using (Stream dataStream = tRequest.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    using (WebResponse tResponse = tRequest.GetResponse())
                    {
                        using (Stream dataStreamResponse = tResponse.GetResponseStream())
                        {
                            if (dataStreamResponse != null) using (StreamReader tReader = new StreamReader(dataStreamResponse))
                                {
                                    Respones = tReader.ReadToEnd();
                                }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog {
                    ClassName = GetType().Name,
                    FuncName= "SendNotification",
                    Error=ex.Message                    
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return Respones;
        }
        public string SendNotification(string to,string mssg, string img, string title, string url)
        {
            string Respones = "";
            try
            {
                int id = new Random().Next(100000, 999999);
                System.Net.WebRequest tRequest = System.Net.WebRequest.Create("https://fcm.googleapis.com/fcm/send");
                tRequest.Method = "post";
                //serverKey - Key from Firebase cloud messaging server  
                tRequest.Headers.Add(string.Format("Authorization: key={0}", ApplicationSetting.FCMKey));
                //Sender Id - From firebase project setting  
                tRequest.Headers.Add(string.Format("Sender: id={0}", ApplicationSetting.FCMSenderID));
                tRequest.ContentType = "application/json";
                var payload = new
                {
                    to,
                    collapse_key = "type_a",
                    data = new
                    {
                        Message = mssg,
                        Image = img,
                        Key = id,
                        Title = title,
                        Url = url,
                        Type = "Personal_Notification"
                    }
                };
                string postbody = JsonConvert.SerializeObject(payload).ToString();
                byte[] byteArray = Encoding.UTF8.GetBytes(postbody);
                tRequest.ContentLength = byteArray.Length;
                using (Stream dataStream = tRequest.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    using (WebResponse tResponse = tRequest.GetResponse())
                    {
                        using (Stream dataStreamResponse = tResponse.GetResponseStream())
                        {
                            if (dataStreamResponse != null) using (StreamReader tReader = new StreamReader(dataStreamResponse))
                            {
                                Respones = tReader.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "SendNotification_"+to,
                    Error = ex.Message
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return Respones;
        }
        public string SendNotification(string to, string orderkey)
        {
            string Respones = string.Empty;
            try
            {
                int id = new Random().Next(100000, 999999);
                System.Net.WebRequest tRequest = System.Net.WebRequest.Create("https://fcm.googleapis.com/fcm/send");
                tRequest.Method = "post";
                //serverKey - Key from Firebase cloud messaging server  
                tRequest.Headers.Add(string.Format("Authorization: key={0}", ApplicationSetting.FCMKey));
                //Sender Id - From firebase project setting  
                tRequest.Headers.Add(string.Format("Sender: id={0}", ApplicationSetting.FCMSenderID));
                tRequest.ContentType = "application/json";
                var payload = new
                {
                    to,
                    collapse_key = "type_a",
                    data = new
                    {
                        orderkey,
                        Type = "order_key"
                    }
                };
                string postbody = JsonConvert.SerializeObject(payload);
                byte[] byteArray = Encoding.UTF8.GetBytes(postbody);
                tRequest.ContentLength = byteArray.Length;
                using (Stream dataStream = tRequest.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    using (WebResponse tResponse = tRequest.GetResponse())
                    {
                        using (Stream dataStreamResponse = tResponse.GetResponseStream())
                        {
                            if (dataStreamResponse != null) using (StreamReader tReader = new StreamReader(dataStreamResponse))
                                {
                                    Respones = tReader.ReadToEnd();
                                }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "SendNotification_Order_k" + to,
                    Error = ex.Message
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return Respones;
        }
        public bool SendNotificationSmsAndEMAILToUser(NotificationServiceReq notificationServiceReq)
        {
            var IsSent = false;
            try
            {
                if (notificationServiceReq != null)
                {
                    if (notificationServiceReq.notification.Message != null && (notificationServiceReq.notification.Message ?? "") != "")
                    {
                        if (notificationServiceReq.messsageTemplate.IsEnableSMS)
                        {                           
                            SendSMS(null, notificationServiceReq.Mobile, notificationServiceReq.EmailID, notificationServiceReq.WID, false, notificationServiceReq.Title, -1,"");
                        }
                        if (notificationServiceReq.messsageTemplate.IsEnableNotificaion)
                        {
                            SendNotification(notificationServiceReq.FCMID, notificationServiceReq.notification.Message, notificationServiceReq.notification.ImageUrl, notificationServiceReq.notification.Title, notificationServiceReq.notification.Url);
                        }
                        IsSent = true;
                    }
                }
            }
            catch (Exception ex)
            {

            }
            
            return IsSent;
        }
        public MessageTemplate GetRechargeMessage(int RechargeStatus, ReplacementModelForSMS replacementModelForSMS)
        {
            var FormatID = GetRechargeFormatID(RechargeStatus);

            var userML = new UserML(_accessor, _env);
            var messsageTemplate = userML.GetMessageFormat(FormatID);
            if ((messsageTemplate.Template ?? "") != "") {
                messsageTemplate.Msg = ReplaceTemplateWithValue(messsageTemplate.Template,replacementModelForSMS);
            }
            return messsageTemplate;
        }
        public Notification GetNotificationForRecharge(int RechargeStatus, string Msg)
        {
            var title = "Recharge "+ RechargeRespType._PENDING;
            title = RechargeStatus == RechargeRespType.SUCCESS ? "Recharge "+RechargeRespType._SUCCESS : title;
            title = RechargeStatus == RechargeRespType.FAILED ? "Recharge " + RechargeRespType._FAILED : title;
            title = RechargeStatus == RechargeRespType.REFUND ? "Recharge " + RechargeRespType._REFUND : title;
            var image = Path.Combine(DOCType.ImgNotificationSuffix,Images.PENDING);
            image = RechargeStatus == RechargeRespType.SUCCESS ? Images.SUCCESS : image;
            image = RechargeStatus == RechargeRespType.FAILED ? Images.FAILED : image;
            image = RechargeStatus == RechargeRespType.REFUND ? Images.FAILED : image;
            return new Notification {
                Title=title,
                Message=Msg,
                ImageUrl=image,
                Url=""
            };
        }
        public MessageTemplate GetFundTransferMessage(int FormatID, ReplacementModelForSMS replacementModelForSMS)
        {
            var userML = new UserML(_accessor, _env);
            var messsageTemplate = userML.GetMessageFormat(FormatID);
            if ((messsageTemplate.Template ?? "") != "")
            {
                messsageTemplate.Msg = ReplaceTemplateWithValue(messsageTemplate.Template, replacementModelForSMS);
            }
            return messsageTemplate;
        }
        public int GetRechargeFormatID(int RechargeStatus)
        {
            var FormatID = MessageFormat.RechargeAccept;
            FormatID = RechargeStatus == RechargeRespType.SUCCESS ? MessageFormat.RechargeSuccess : FormatID;
            FormatID = RechargeStatus == RechargeRespType.FAILED ? MessageFormat.RechargeFailed : FormatID;
            FormatID = RechargeStatus == RechargeRespType.REFUND ? MessageFormat.RechargeRefund : FormatID;
            return FormatID;
        }        
        public string ReplaceTemplateWithValue(string template, ReplacementModelForSMS replacementModelForSMS)
        {
            StringBuilder sb = new StringBuilder(template);
            sb.Replace(ReplacementSMS.Mobile, replacementModelForSMS.Mobile);
            sb.Replace(ReplacementSMS.Amount, replacementModelForSMS.Amount);
            sb.Replace(ReplacementSMS.OperatorName, replacementModelForSMS.OperatorName);
            sb.Replace(ReplacementSMS.FromMobileNo, replacementModelForSMS.FromMobileNo);
            sb.Replace(ReplacementSMS.ToMobileNo, replacementModelForSMS.ToMobileNo);
            sb.Replace(ReplacementSMS.BalanceAmount, replacementModelForSMS.BalanceAmount);
            sb.Replace(ReplacementSMS.TransactionID, replacementModelForSMS.TransactionID);
            sb.Replace(ReplacementSMS.Company, replacementModelForSMS.Company);
            sb.Replace(ReplacementSMS.CompanyDomain, replacementModelForSMS.CompanyDomain);
            sb.Replace(ReplacementSMS.Password, replacementModelForSMS.Password);
            sb.Replace(ReplacementSMS.PinPassword, replacementModelForSMS.PinPassword);
            sb.Replace(ReplacementSMS.OTP, replacementModelForSMS.OTP);
            sb.Replace(ReplacementSMS.FromUserName, replacementModelForSMS.FromUserName);
            sb.Replace(ReplacementSMS.ToUserName, replacementModelForSMS.ToUserName);
            sb.Replace(ReplacementSMS.UserName, replacementModelForSMS.UserName);
            sb.Replace(ReplacementSMS.CompanyMobile, replacementModelForSMS.CompanyMobile);
            sb.Replace(ReplacementSMS.CompanyEmail, replacementModelForSMS.CompanyEmail);
            sb.Replace(ReplacementSMS.LoginID, replacementModelForSMS.LoginID);
            sb.Replace(ReplacementSMS.LiveID, replacementModelForSMS.LiveID);
            return sb.ToString();
        }
        public string CallSendSMSAPI(SendSMSRequest _req)
        {
            string ApiResp = "";
            try
            {
                if (_req.APIMethod == "GET")
                {
                    ApiResp = AppWebRequest.O.CallUsingWebClient_GET(_req.SmsURL, 0);
                    _req.IsSend = true;
                }
                if (_req.APIMethod == "POST")
                {

                }
            }
            catch (Exception ex)
            {
                ApiResp = "Exception Occured! " + ex.Message;
            }
            return ApiResp;
        }
        public string SendDeliveryNotification(string to, string mssg, string img, string title, string url, string orderId, string type)
        {
            //type = new_delivery, pickup_complete, delivery_complete
            string Respones = "";
            try
            {
                int id = new Random().Next(100000, 999999);
                System.Net.WebRequest tRequest = System.Net.WebRequest.Create("https://fcm.googleapis.com/fcm/send");
                tRequest.Method = "post";
                //serverKey - Key from Firebase cloud messaging server  
                tRequest.Headers.Add(string.Format("Authorization: key={0}", ApplicationSetting.DeliveryFCMKey));
                //Sender Id - From firebase project setting  
                tRequest.Headers.Add(string.Format("Sender: id={0}", ApplicationSetting.DeliveryFCMSenderID));
                tRequest.ContentType = "application/json";
                var payload = new
                {
                    to,
                    collapse_key = "type_a",
                    data = new
                    {
                        Message = mssg,
                        Image = img,
                        Key = id,
                        Title = title,
                        Url = url,
                        OrderId = orderId,
                        Type = type
                    }
                };
                string postbody = JsonConvert.SerializeObject(payload).ToString();
                byte[] byteArray = Encoding.UTF8.GetBytes(postbody);
                tRequest.ContentLength = byteArray.Length;
                using (Stream dataStream = tRequest.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    using (WebResponse tResponse = tRequest.GetResponse())
                    {
                        using (Stream dataStreamResponse = tResponse.GetResponseStream())
                        {
                            if (dataStreamResponse != null) using (StreamReader tReader = new StreamReader(dataStreamResponse))
                                {
                                    Respones = tReader.ReadToEnd();
                                }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "SendNotification_" + to,
                    Error = ex.Message
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return Respones;
        }
    }
}


