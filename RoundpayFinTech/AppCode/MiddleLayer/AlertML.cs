using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using RoundpayFinTech.AppCode.Configuration;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public class AlertML : BaseML, IAlertML
    {
        private readonly IResourceML _resourceML;
        private readonly ISendSMSML _sendSMSML;
        public AlertML(IHttpContextAccessor accessor, IHostingEnvironment env, bool IsInSession = true) : base(accessor, env, IsInSession)
        {
            _resourceML = new ResourceML(_accessor, _env);
            _sendSMSML = new SendSMSML(_dal);
        }


        #region Bulk
        public IResponseStatus SendSMS(int APIID, AlertReplacementModel model)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.SUCCESS
            };
            if (string.IsNullOrEmpty(model.MobileNos))
            {
                _res.Msg = "Invaild MobileMo";
                return _res;
            }
            var MobileList = !string.IsNullOrEmpty(model.MobileNos) ? model.MobileNos.Split(',').ToList() : new List<string>();
            if (MobileList == null || MobileList.Count < 1)
            {
                _res.Msg = "Invaild MobileMo";
                return _res;
            }
            if (string.IsNullOrEmpty(model.Msg))
            {
                _res.Msg = "Fill message";
                return _res;
            }
            if (APIID < 1)
            {
                _res.Msg = "Invaild API";
                return _res;
            }

            string sendRes = string.Empty;
            ISMSAPIML ML = new APIML(_accessor, _env);
            var detail = ML.GetSMSAPIDetailByID(APIID);
            var _l = MobileList.LastOrDefault();
            if (string.IsNullOrEmpty(_l))
            {
                MobileList.RemoveAt(MobileList.Count - 1);
            }
            if (detail != null)
            {
                model.Msg = GetFormatedMessage(model.Msg, model);
                var smsSetting = new SMSSetting
                {
                    APIID = detail.ID,
                    SMSID = detail.ID,
                    APIMethod = detail.APIMethod,
                    IsLapu = false,
                    URL = detail.URL,
                    MobileNos = string.Join(",", MobileList),
                    WID = _lr != null ? _lr.WID : 0,
                    SMS = model.Msg
                };
                var SMSURL = new StringBuilder(smsSetting.URL);
                if (!detail.IsMultipleAllowed)
                {
                    foreach (var item in MobileList)
                    {

                        SMSURL.Clear();
                        SMSURL.Append(smsSetting.URL);
                        SMSURL.Replace("{SENDERID}", smsSetting.SenderID ?? "");
                        SMSURL.Replace("{TO}", item);
                        SMSURL.Replace("{MESSAGE}", smsSetting.SMS);

                        var p = new SendSMSRequest()
                        {
                            APIMethod = smsSetting.APIMethod,
                            SmsURL = SMSURL.ToString(),
                            IsLapu = false
                        };
                        if (detail.APIType == 2)
                        {
                            sendRes = "Only save message";
                        }
                        else
                        {
                            sendRes = _sendSMSML.CallSendSMSAPI(p);
                        }

                        var _Response = new SMSResponse
                        {
                            ReqURL = SMSURL.ToString(),
                            Response = Convert.ToString(sendRes),
                            ResponseID = "",
                            Status = SMSResponseTYPE.SEND,
                            SMSID = smsSetting.SMSID,
                            MobileNo = item,
                            TransactionID = "",
                            SMS = smsSetting.SMS,
                            WID = smsSetting.WID
                        };
                        SaveSMSResponse(_Response);
                    }
                }
                else
                {
                    SMSURL.Replace("{SENDERID}", smsSetting.SenderID ?? "");
                    SMSURL.Replace("{TO}", smsSetting.MobileNos);
                    SMSURL.Replace("{MESSAGE}", smsSetting.SMS);
                    var p = new SendSMSRequest()
                    {
                        APIMethod = smsSetting.APIMethod,
                        SmsURL = SMSURL.ToString(),
                        IsLapu = false
                    };
                    sendRes = _sendSMSML.CallSendSMSAPI(p);
                    var _Response = new SMSResponse
                    {
                        ReqURL = SMSURL.ToString(),
                        Response = Convert.ToString(sendRes),
                        ResponseID = "",
                        Status = SMSResponseTYPE.SEND,
                        SMSID = smsSetting.SMSID,
                        MobileNo = smsSetting.MobileNos,
                        TransactionID = "",
                        SMS = smsSetting.SMS,
                        WID = smsSetting.WID
                    };
                    SaveSMSResponse(_Response);
                }
            }
            _res.Statuscode = ErrorCodes.One;
            return _res;
        }
        public IResponseStatus SendEmail(AlertReplacementModel model)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Mail " + ErrorCodes.NotSent
            };
            if (string.IsNullOrEmpty(model.Msg))
            {
                res.Msg = "Fill message";
                return res;
            }
            if (model.bccList.Count < 1)
            {
                res.Msg = "Invaild EmailDs";
                return res;
            }
            string _l = model.bccList.LastOrDefault();
            if (string.IsNullOrEmpty(_l))
            {
                model.bccList.RemoveAt(model.bccList.Count - 1);
            }
            IEmailML emailManager = new EmailML(_dal);
            if (model.bccList != null)
            {
                List<string> bccList = new List<string>();
                while (model.bccList.Any())
                {
                    model.Msg = GetFormatedMessage(model.Msg, model);
                    bccList = model.bccList.Take(49).ToList();
                    model.bccList = model.bccList.Skip(49).ToList();
                    string Footer = "<p><h4 style='color:#000000;font-family:verdana,sans-serif;margin-bottom:1.5px'><em>{CompanyName}</em></h4><span>{CompanyAddress}</span></p>";
                    Footer = Footer.Replace("{CompanyName}", model.Company).Replace("{CompanyAddress}", model.CompanyAddress);
                    int _WID = _lr != null ? _lr.WID : 0;
                    if (emailManager.SendMail(model.SupportEmail, bccList, model.Subject, model.Msg, _WID, _resourceML.GetLogoURL(_WID).ToString(), true, Footer))
                    //if (emailManager.SendMail(bccList.FirstOrDefault(), bccList, model.Subject, model.Msg, _lr.WID, _resourceML.GetLogoURL(_lr.WID).ToString(), true, Footer))
                    {
                        res.Statuscode = ErrorCodes.One;
                        res.Msg = "Mail " + ErrorCodes.Sent;
                    }
                }
            }
            return res;
        }
        public async Task BulkWebNotification(AlertReplacementModel param)
        {
            string _Notification = GetFormatedMessage(param.Msg, param);
            var notification = new WebNotification
            {
                LoginID = param.LoginID,
                UserID = param.UserID,
                Title = param.NotificationTitle,
                Notification = string.IsNullOrEmpty(_Notification) ? "" : _Notification,
                FormatID = param.FormatID,
                Operator = param.Operator,
                Img = param.URL,
                RoleId = param.Roles,
            };
            SaveWebNotification(notification).ConfigureAwait(false);
        }
        public IResponseStatus SendSocialAlert(AlertReplacementModel model, List<string> APIIDs)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Mess " + ErrorCodes.NotSent
            };

            if (model.SocialAlertType == 0)
            {
                res.Msg = "Please select social alert type";
                return res;
            }

            if (string.IsNullOrEmpty(model.Msg))
            {
                res.Msg = "Fill message";
                return res;
            }

            if (APIIDs == null || APIIDs.Count < 1)
            {
                res.Msg = "Select API";
                return res;
            }

            var SocialIDList = model.SocialIDs.Split(",").Where(x => !string.IsNullOrEmpty(x.Trim())).ToArray();
            if (SocialIDList == null || SocialIDList.Count() < 1)
            {
                res.Msg = "Please select Social Ids";
                return res;
            }
            var arrAPI = APIIDs.ToArray();



            model.Msg = GetFormatedMessage(model.Msg, model);
            DataTable dt = new DataTable();
            dt.Columns.Add("SendTo");
            dt.Columns.Add("Message");
            dt.Columns.Add("SocialAlertType");
            dt.Columns.Add("SMSAPIID");
            int _SMSAPIID = 0;
            for (int i = 0; i < SocialIDList.Count(); i++)
            {
                int selectedAPI = SocialIDList.Count() / 500;
                int kk = arrAPI.Count();
                if ((selectedAPI + 1) <= arrAPI.Count())
                {
                    DataRow dr = dt.NewRow();
                    dr["SendTo"] = SocialIDList[i];
                    dr["Message"] = model.Msg;
                    dr["SocialAlertType"] = model.SocialAlertType;
                    dr["SMSAPIID"] = arrAPI[selectedAPI];
                    dt.Rows.Add(dr);
                    _SMSAPIID = Convert.ToInt32(arrAPI[selectedAPI]);
                }
            }
            var modal = new SocialMessage
            {
                LT = _lr != null ? _lr.LoginTypeID : 0,
                LoginID = _lr != null ? _lr.UserID : 0,
                Data = dt,
                IsBulk = true,
                SMSAPIID = _SMSAPIID
            };
            //,
            //    SCANNO = model.CommonStr2 ?? string.Empty,
            //CountryCode = model.CommonStr3 ?? string.Empty
            var response = SendSocialNotification(modal);
            res.Statuscode = response.Statuscode;
            res.Msg = response.Msg;
            return res;
        }

        #endregion

        #region FundTransfer
        public IResponseStatus FundTransferSMS(AlertReplacementModel param)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS
            };
            string sendRes = string.Empty;
            string SMS = string.Empty;
            var Sqlparam = new CommonReq()
            {
                LoginID = param.LoginID,
                CommonInt = MessageFormat.FundTransfer
            };
            IProcedure _proc = new ProcGetSMSSettingByFormat(_dal);
            var smsSetting = (SMSSetting)_proc.Call(Sqlparam);
            StringBuilder sbUrl = new StringBuilder(smsSetting.URL);
            if (smsSetting.IsEnableSMS)
            {

                bool IsNoTemplate = true;
                if (string.IsNullOrEmpty(smsSetting.Template))
                {
                    sendRes = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if (smsSetting.SMSID == 0)
                    {
                        sendRes = "No API Found";
                    }
                    SMS = GetFormatedMessage(smsSetting.Template, param);
                    if (smsSetting.SMSID > 0 && !string.IsNullOrEmpty(smsSetting.URL))
                    {
                        sbUrl.Replace("{SENDERID}", smsSetting.SenderID);
                        sbUrl.Replace("{TO}", param.LoginMobileNo);
                        sbUrl.Replace("{MESSAGE}", SMS);
                        var p = new SendSMSRequest()
                        {
                            APIMethod = smsSetting.APIMethod,
                            SmsURL = sbUrl.ToString()
                        };
                        sendRes = _sendSMSML.CallSendSMSAPI(p);
                    }
                }
                var _Response = new SMSResponse
                {
                    ReqURL = sbUrl.ToString(),
                    Response = Convert.ToString(sendRes),
                    ResponseID = "",
                    Status = SMSResponseTYPE.SEND,
                    SMSID = smsSetting.SMSID,
                    MobileNo = param.LoginMobileNo,
                    TransactionID = param.TransactionID,
                    SMS = SMS,
                    WID = param.WID
                };
                SaveSMSResponse(_Response);
            }
            return _res;
        }
        public IResponseStatus FundTransferEmail(AlertReplacementModel param)
        {
            bool IsSent = false;
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS
            };
            string EmailBody = string.Empty;
            var Sqlparam = new CommonReq()
            {
                LoginID = param.LoginID,
                CommonInt = MessageFormat.FundTransfer
            };
            IProcedure _proc = new ProcGetEmailSettingByFormat(_dal);
            var mailSetting = (EmailSettingswithFormat)_proc.Call(Sqlparam);
            if (mailSetting.IsEnableEmail)
            {
                bool IsNoTemplate = true;
                if (string.IsNullOrEmpty(mailSetting.EmailTemplate))
                {
                    EmailBody = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if (string.IsNullOrEmpty(mailSetting.FromEmail))
                    {
                        EmailBody = "No Email Found";
                    }
                    EmailBody = GetFormatedMessage(mailSetting.EmailTemplate, param);
                    if (param.WID > 0)
                    {
                        if (!string.IsNullOrEmpty(mailSetting.FromEmail))
                        {
                            IEmailML emailManager = new EmailML(_dal);
                            string logo = _resourceML.GetLogoURL(param.WID).ToString();
                            string Footer = "<p><h4 style='color:#000000;font-family:verdana,sans-serif;margin-bottom:1.5px'><em>{CompanyName}</em></h4><span>{CompanyAddress}</span></p>";
                            Footer = Footer.Replace("{CompanyName}", param.Company).Replace("{CompanyAddress}", param.CompanyAddress);
                            IsSent = emailManager.SendEMail(mailSetting, param.LoginEmailID, null, mailSetting.Subject, EmailBody, param.WID, logo, true, Footer);
                        }
                    }
                }
                SendEmail sendEmail = new SendEmail
                {
                    From = mailSetting.FromEmail,
                    Body = EmailBody,
                    Recipients = param.LoginEmailID + "," + (param.bccList != null ? (param.bccList.Count > 0 ? String.Join(",", param.bccList) : "") : ""),
                    Subject = mailSetting.Subject,
                    IsSent = IsSent,
                    WID = param.WID
                };
                EmailDL emailDL = new EmailDL(_dal);
                emailDL.SaveMail(sendEmail);
            }
            return _res;
        }
        public IResponseStatus FundTransferNotification(AlertReplacementModel param)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS
            };
            string _Notification = string.Empty;
            string _response = string.Empty;
            var Sqlparam = new CommonReq()
            {
                CommonInt = MessageFormat.FundTransfer
            };
            IProcedure _proc = new ProcGetNotificationFormat(_dal);
            var NotificationDetail = (ResponseStatus)_proc.Call(Sqlparam);
            if (NotificationDetail.CommonBool)
            {
                string Title = nameof(MessageFormat.FundTransfer);
                StringBuilder logo = new StringBuilder();
                logo.AppendFormat("{0}.png", MessageFormat.FundTransfer);
                bool IsNoTemplate = true;
                if (string.IsNullOrEmpty(NotificationDetail.CommonStr))
                {
                    _Notification = "No Template Found";
                    IsNoTemplate = false;
                }

                if (IsNoTemplate)
                {
                    if ((param.LoginFCMID ?? string.Empty).Length < 5)
                    {
                        _Notification = "No FCMID Found";
                        IsNoTemplate = false;
                    }
                    _Notification = GetFormatedMessage(NotificationDetail.CommonStr, param);
                    if (param.WID > 0 && IsNoTemplate)
                    {
                        SendSMSML sendSmsMl = new SendSMSML(_accessor, _env);
                        _response = sendSmsMl.SendNotification(param.LoginFCMID, _Notification, logo.ToString(), Title, param.URL);
                    }
                }
                var _Response = new Notification
                {
                    LoginID = param.LoginID,
                    Response = _response,
                    Title = Title,
                    ImageUrl = logo.ToString(),
                    Url = param.URL,
                    Message = _Notification,
                    WID = param.WID,
                    UserID = param.LoginID,
                    FCMID = param.LoginFCMID
                };
                SaveNotificationResponse(_Response);
            }
            return _res;
        }
        #endregion

        #region FundReceive
        public IResponseStatus FundReceiveSMS(AlertReplacementModel param)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS
            };
            string sendRes = string.Empty;
            string SMS = string.Empty;
            var Sqlparam = new CommonReq()
            {
                LoginID = param.LoginID,
                CommonInt = MessageFormat.FundReceive
            };
            IProcedure _proc = new ProcGetSMSSettingByFormat(_dal);
            var smsSetting = (SMSSetting)_proc.Call(Sqlparam);
            if (smsSetting.IsEnableSMS)
            {
                StringBuilder sbUrl = new StringBuilder(smsSetting.URL);
                bool IsNoTemplate = true;
                if (string.IsNullOrEmpty(smsSetting.Template))
                {
                    sendRes = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if (smsSetting.SMSID == 0)
                    {
                        sendRes = "No API Found";
                    }
                    SMS = GetFormatedMessage(smsSetting.Template, param);
                    if (smsSetting.SMSID > 0 && !string.IsNullOrEmpty(smsSetting.URL))
                    {
                        sbUrl.Replace("{SENDERID}", smsSetting.SenderID);
                        sbUrl.Replace("{TO}", param.UserMobileNo);
                        sbUrl.Replace("{MESSAGE}", SMS);
                        var p = new SendSMSRequest()
                        {
                            APIMethod = smsSetting.APIMethod,
                            SmsURL = sbUrl.ToString()
                        };
                        sendRes = _sendSMSML.CallSendSMSAPI(p);
                    }
                }
                var _Response = new SMSResponse
                {
                    ReqURL = sbUrl.ToString(),
                    Response = Convert.ToString(sendRes),
                    ResponseID = "",
                    Status = SMSResponseTYPE.SEND,
                    SMSID = smsSetting.SMSID,
                    MobileNo = param.UserMobileNo,
                    TransactionID = param.TransactionID,
                    SMS = SMS,
                    WID = param.WID
                };
                SaveSMSResponse(_Response);
            }
            return _res;
        }
        public IResponseStatus FundReceiveEmail(AlertReplacementModel param)
        {
            bool IsSent = false;
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS
            };
            string EmailBody = string.Empty;
            var Sqlparam = new CommonReq()
            {
                LoginID = param.LoginID,
                CommonInt = MessageFormat.FundReceive
            };
            IProcedure _proc = new ProcGetEmailSettingByFormat(_dal);
            var mailSetting = (EmailSettingswithFormat)_proc.Call(Sqlparam);
            if (mailSetting.IsEnableEmail)
            {
                bool IsNoTemplate = true;
                if (string.IsNullOrEmpty(mailSetting.EmailTemplate))
                {
                    EmailBody = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if (string.IsNullOrEmpty(mailSetting.FromEmail))
                    {
                        EmailBody = "No Email Found";
                    }
                    EmailBody = GetFormatedMessage(mailSetting.EmailTemplate, param);
                    if (param.WID > 0)
                    {
                        if (!string.IsNullOrEmpty(mailSetting.FromEmail))
                        {
                            IEmailML emailManager = new EmailML(_dal);
                            string logo = _resourceML.GetLogoURL(param.WID).ToString();
                            string Footer = "<p><h4 style='color:#000000;font-family:verdana,sans-serif;margin-bottom:1.5px'><em>{CompanyName}</em></h4><span>{CompanyAddress}</span></p>";
                            Footer = Footer.Replace("{CompanyName}", param.Company).Replace("{CompanyAddress}", param.CompanyAddress);
                            IsSent = emailManager.SendEMail(mailSetting, param.UserEmailID, null, mailSetting.Subject, EmailBody, param.WID, logo, true, Footer);
                        }
                    }
                }
                SendEmail sendEmail = new SendEmail
                {
                    From = mailSetting.FromEmail,
                    Body = EmailBody,
                    Recipients = param.UserEmailID + "," + (param.bccList != null ? (param.bccList.Count > 0 ? String.Join(",", param.bccList) : "") : ""),
                    Subject = mailSetting.Subject,
                    IsSent = IsSent,
                    WID = param.WID
                };
                EmailDL emailDL = new EmailDL(_dal);
                emailDL.SaveMail(sendEmail);
            }
            return _res;
        }
        public IResponseStatus FundReceiveNotification(AlertReplacementModel param)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS
            };
            string _Notification = string.Empty;
            string _response = string.Empty;
            var Sqlparam = new CommonReq()
            {
                CommonInt = MessageFormat.FundReceive
            };
            IProcedure _proc = new ProcGetNotificationFormat(_dal);
            var NotificationDetail = (ResponseStatus)_proc.Call(Sqlparam);
            if (NotificationDetail.CommonBool)
            {
                string Title = nameof(MessageFormat.FundReceive);
                StringBuilder logo = new StringBuilder();
                logo.AppendFormat("{0}.png", MessageFormat.FundReceive);
                bool IsNoTemplate = true;
                if (string.IsNullOrEmpty(NotificationDetail.CommonStr))
                {
                    _Notification = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if ((param.UserFCMID ?? string.Empty).Length < 5)
                    {
                        _Notification = "No FCMID Found";
                        IsNoTemplate = false;
                    }
                    _Notification = GetFormatedMessage(NotificationDetail.CommonStr, param);
                    if (param.WID > 0 && IsNoTemplate)
                    {
                        SendSMSML sendSmsMl = new SendSMSML(_accessor, _env);
                        _response = sendSmsMl.SendNotification(param.UserFCMID, _Notification, logo.ToString(), Title, param.URL);
                    }
                }
                var _Response = new Notification
                {
                    LoginID = param.LoginID,
                    Response = _response,
                    Title = Title,
                    ImageUrl = logo.ToString(),
                    Url = param.URL,
                    Message = _Notification,
                    WID = param.WID,
                    UserID = param.UserID,
                    FCMID = param.UserFCMID
                };
                SaveNotificationResponse(_Response);
            }
            return _res;
        }
        #endregion

        #region Payout

        public IResponseStatus PayoutSMS(AlertReplacementModel param)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS
            };
            string sendRes = string.Empty;
            string SMS = string.Empty;
            var Sqlparam = new CommonReq
            {
                LoginID = param.LoginID,
                CommonInt = 28
            };
            IProcedure _proc = new ProcGetSMSSettingByFormat(_dal);
            var smsSetting = (SMSSetting)_proc.Call(Sqlparam);
            smsSetting.IsEnableSMS = true;
            StringBuilder sbUrl = new StringBuilder(smsSetting.URL);
            if (smsSetting.IsEnableSMS)
            {
                bool IsNoTemplate = true;

                if (string.IsNullOrEmpty(smsSetting.Template))
                {
                    sendRes = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if (smsSetting.SMSID == 0)
                    {
                        sendRes = "No API Found";
                    }
                    SMS = GetFormatedMessage(smsSetting.Template, param);
                    if (smsSetting.SMSID > 0 && !string.IsNullOrEmpty(smsSetting.URL))
                    {
                        sbUrl.Replace("{SENDERID}", smsSetting.SenderID);
                        sbUrl.Replace("{TO}", param.UserMobileNo);
                        sbUrl.Replace("{MESSAGE}", SMS);
                        var p = new SendSMSRequest
                        {
                            APIMethod = smsSetting.APIMethod,
                            SmsURL = sbUrl.ToString()
                        };
                        sendRes = _sendSMSML.CallSendSMSAPI(p);
                    }
                }
                var _Response = new SMSResponse
                {
                    ReqURL = sbUrl.ToString(),
                    Response = Convert.ToString(sendRes),
                    ResponseID = "",
                    Status = SMSResponseTYPE.SEND,
                    SMSID = smsSetting.SMSID,
                    MobileNo = param.UserMobileNo,
                    TransactionID = "",
                    SMS = SMS,
                    WID = param.WID
                };
                SaveSMSResponse(_Response);
            }
            return _res;
        }
        #endregion

        #region FundCredit
        public IResponseStatus FundCreditSMS(AlertReplacementModel param)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS
            };
            string sendRes = string.Empty;
            string SMS = string.Empty;
            var Sqlparam = new CommonReq()
            {
                LoginID = param.LoginID,
                CommonInt = MessageFormat.FundCredit
            };
            IProcedure _proc = new ProcGetSMSSettingByFormat(_dal);
            var smsSetting = (SMSSetting)_proc.Call(Sqlparam);
            StringBuilder sbUrl = new StringBuilder(smsSetting.URL);
            if (smsSetting.IsEnableSMS)
            {
                bool IsNoTemplate = true;
                if (string.IsNullOrEmpty(smsSetting.Template))
                {
                    sendRes = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if (smsSetting.SMSID == 0)
                    {
                        sendRes = "No API Found";
                    }
                    SMS = GetFormatedMessage(smsSetting.Template, param);
                    if (smsSetting.SMSID > 0 && !string.IsNullOrEmpty(smsSetting.URL))
                    {
                        sbUrl.Replace("{SENDERID}", smsSetting.SenderID);
                        sbUrl.Replace("{TO}", param.LoginMobileNo);
                        sbUrl.Replace("{MESSAGE}", SMS);
                        var p = new SendSMSRequest()
                        {
                            APIMethod = smsSetting.APIMethod,
                            SmsURL = sbUrl.ToString()
                        };
                        sendRes = _sendSMSML.CallSendSMSAPI(p);
                    }
                }
                var _Response = new SMSResponse
                {
                    ReqURL = sbUrl.ToString(),
                    Response = Convert.ToString(sendRes),
                    ResponseID = "",
                    Status = SMSResponseTYPE.SEND,
                    SMSID = smsSetting.SMSID,
                    MobileNo = param.LoginMobileNo,
                    TransactionID = param.TransactionID,
                    SMS = SMS,
                    WID = param.WID
                };
                SaveSMSResponse(_Response);
            }

            return _res;
        }
        public IResponseStatus FundCreditEmail(AlertReplacementModel param)
        {
            bool IsSent = false;
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS
            };
            string EmailBody = string.Empty;
            var Sqlparam = new CommonReq()
            {
                LoginID = param.LoginID,
                CommonInt = MessageFormat.FundCredit
            };
            IProcedure _proc = new ProcGetEmailSettingByFormat(_dal);
            var mailSetting = (EmailSettingswithFormat)_proc.Call(Sqlparam);
            if (mailSetting.IsEnableEmail)
            {
                bool IsNoTemplate = true;
                if (string.IsNullOrEmpty(mailSetting.EmailTemplate))
                {
                    EmailBody = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if (string.IsNullOrEmpty(mailSetting.FromEmail))
                    {
                        EmailBody = "No Email Found";
                    }
                    EmailBody = GetFormatedMessage(mailSetting.EmailTemplate, param);
                    if (param.WID > 0)
                    {
                        if (!string.IsNullOrEmpty(mailSetting.FromEmail))
                        {
                            IEmailML emailManager = new EmailML(_dal);
                            string logo = _resourceML.GetLogoURL(param.WID).ToString();
                            string Footer = "<p><h4 style='color:#000000;font-family:verdana,sans-serif;margin-bottom:1.5px'><em>{CompanyName}</em></h4><span>{CompanyAddress}</span></p>";
                            Footer = Footer.Replace("{CompanyName}", param.Company).Replace("{CompanyAddress}", param.CompanyAddress);
                            IsSent = emailManager.SendEMail(mailSetting, param.LoginEmailID, null, mailSetting.Subject, EmailBody, param.WID, logo, true, Footer);
                        }
                    }
                }
                SendEmail sendEmail = new SendEmail
                {
                    From = mailSetting.FromEmail,
                    Body = EmailBody,
                    Recipients = param.LoginEmailID + "," + (param.bccList != null ? (param.bccList.Count > 0 ? String.Join(",", param.bccList) : "") : ""),
                    Subject = mailSetting.Subject,
                    IsSent = IsSent,
                    WID = param.WID
                };
                EmailDL emailDL = new EmailDL(_dal);
                emailDL.SaveMail(sendEmail);
            }
            return _res;
        }

        public IResponseStatus FundCreditNotification(AlertReplacementModel param)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS
            };
            string _Notification = string.Empty;
            string _response = string.Empty;
            var Sqlparam = new CommonReq()
            {
                CommonInt = MessageFormat.FundCredit
            };
            IProcedure _proc = new ProcGetNotificationFormat(_dal);
            var NotificationDetail = (ResponseStatus)_proc.Call(Sqlparam);
            if (NotificationDetail.CommonBool)
            {
                string Title = nameof(MessageFormat.FundCredit);
                StringBuilder logo = new StringBuilder();
                logo.AppendFormat("{0}.png", MessageFormat.FundCredit);
                bool IsNoTemplate = true;
                if (string.IsNullOrEmpty(NotificationDetail.CommonStr))
                {
                    _Notification = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if ((param.LoginFCMID ?? string.Empty).Length < 5)
                    {
                        _Notification = "No FCMID Found";
                        IsNoTemplate = false;
                    }

                    _Notification = GetFormatedMessage(NotificationDetail.CommonStr, param);
                    if (param.WID > 0 && IsNoTemplate)
                    {
                        SendSMSML sendSmsMl = new SendSMSML(_accessor, _env);
                        _response = sendSmsMl.SendNotification(param.LoginFCMID, _Notification, logo.ToString(), Title, param.URL);
                    }
                }
                var _Response = new Notification
                {
                    LoginID = param.LoginID,
                    Response = _response,
                    Title = Title,
                    ImageUrl = logo.ToString(),
                    Url = param.URL,
                    Message = _Notification,
                    WID = param.WID,
                    UserID = param.LoginID,
                    FCMID = param.LoginFCMID
                };
                SaveNotificationResponse(_Response);
            }
            return _res;
        }
        #endregion

        #region FundDebit
        public IResponseStatus FundDebitSMS(AlertReplacementModel param)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS
            };
            string sendRes = string.Empty;
            string SMS = string.Empty;
            var Sqlparam = new CommonReq()
            {
                LoginID = param.LoginID,
                CommonInt = MessageFormat.FundDebit
            };
            IProcedure _proc = new ProcGetSMSSettingByFormat(_dal);
            var smsSetting = (SMSSetting)_proc.Call(Sqlparam);
            if (smsSetting.IsEnableSMS)
            {
                bool IsNoTemplate = true;
                StringBuilder sbUrl = new StringBuilder(smsSetting.URL);
                if (string.IsNullOrEmpty(smsSetting.Template))
                {
                    sendRes = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if (smsSetting.SMSID == 0)
                    {
                        sendRes = "No API Found";
                    }

                    SMS = GetFormatedMessage(smsSetting.Template, param);
                    if (smsSetting.SMSID > 0 && !string.IsNullOrEmpty(smsSetting.URL))
                    {
                        sbUrl.Replace("{SENDERID}", smsSetting.SenderID);
                        sbUrl.Replace("{TO}", param.UserMobileNo);
                        sbUrl.Replace("{MESSAGE}", SMS);
                        var p = new SendSMSRequest()
                        {
                            APIMethod = smsSetting.APIMethod,
                            SmsURL = sbUrl.ToString()
                        };
                        sendRes = _sendSMSML.CallSendSMSAPI(p);
                    }
                }
                var _Response = new SMSResponse
                {
                    ReqURL = sbUrl.ToString(),
                    Response = Convert.ToString(sendRes),
                    ResponseID = "",
                    Status = SMSResponseTYPE.SEND,
                    SMSID = smsSetting.SMSID,
                    MobileNo = param.UserMobileNo,
                    TransactionID = param.TransactionID,
                    SMS = SMS,
                    WID = param.WID
                };
                SaveSMSResponse(_Response);
            }

            return _res;
        }
        public IResponseStatus FundDebitEmail(AlertReplacementModel param)
        {
            bool IsSent = false;
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS
            };
            string EmailBody = string.Empty;
            var Sqlparam = new CommonReq()
            {
                LoginID = param.LoginID,
                CommonInt = MessageFormat.FundDebit
            };
            IProcedure _proc = new ProcGetEmailSettingByFormat(_dal);
            var mailSetting = (EmailSettingswithFormat)_proc.Call(Sqlparam);
            if (mailSetting.IsEnableEmail)
            {
                bool IsNoTemplate = true;
                if (string.IsNullOrEmpty(mailSetting.EmailTemplate))
                {
                    EmailBody = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if (string.IsNullOrEmpty(mailSetting.FromEmail))
                    {
                        EmailBody = "No Email Found";
                    }

                    EmailBody = GetFormatedMessage(mailSetting.EmailTemplate, param);
                    if (param.WID > 0)
                    {
                        if (!string.IsNullOrEmpty(mailSetting.FromEmail))
                        {
                            IEmailML emailManager = new EmailML(_dal);
                            string logo = _resourceML.GetLogoURL(param.WID).ToString();
                            string Footer = "<p><h4 style='color:#000000;font-family:verdana,sans-serif;margin-bottom:1.5px'><em>{CompanyName}</em></h4><span>{CompanyAddress}</span></p>";
                            Footer = Footer.Replace("{CompanyName}", param.Company).Replace("{CompanyAddress}", param.CompanyAddress);
                            IsSent = emailManager.SendEMail(mailSetting, param.UserEmailID, null, mailSetting.Subject, EmailBody, param.WID, logo, true, Footer);
                        }
                    }
                }
                SendEmail sendEmail = new SendEmail
                {
                    From = mailSetting.FromEmail,
                    Body = EmailBody,
                    Recipients = param.UserEmailID + "," + (param.bccList != null ? (param.bccList.Count > 0 ? String.Join(",", param.bccList) : "") : ""),
                    Subject = mailSetting.Subject,
                    IsSent = IsSent,
                    WID = param.WID
                };
                EmailDL emailDL = new EmailDL(_dal);
                emailDL.SaveMail(sendEmail);
            }
            return _res;
        }
        public IResponseStatus FundDebitNotification(AlertReplacementModel param)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS
            };
            string _Notification = string.Empty;
            string _response = string.Empty;
            var Sqlparam = new CommonReq()
            {
                CommonInt = MessageFormat.FundDebit
            };
            IProcedure _proc = new ProcGetNotificationFormat(_dal);
            var NotificationDetail = (ResponseStatus)_proc.Call(Sqlparam);
            if (NotificationDetail.CommonBool)
            {
                string Title = nameof(MessageFormat.FundDebit);
                StringBuilder logo = new StringBuilder();
                logo.AppendFormat("{0}.png", MessageFormat.FundDebit);
                bool IsNoTemplate = true;
                if (string.IsNullOrEmpty(NotificationDetail.CommonStr))
                {
                    _Notification = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if ((param.UserFCMID ?? string.Empty).Length < 5)
                    {
                        _Notification = "No FCMID Found";
                        IsNoTemplate = false;
                    }
                    _Notification = GetFormatedMessage(NotificationDetail.CommonStr, param);
                    if (param.WID > 0 && IsNoTemplate)
                    {
                        SendSMSML sendSmsMl = new SendSMSML(_accessor, _env);
                        _response = sendSmsMl.SendNotification(param.UserFCMID, _Notification, logo.ToString(), Title, param.URL);
                    }
                }
                var _Response = new Notification
                {
                    LoginID = param.LoginID,
                    Response = _response,
                    Title = Title,
                    ImageUrl = logo.ToString(),
                    Url = param.UserFCMID,
                    Message = _Notification,
                    WID = param.WID,
                    UserID = param.UserID,
                    FCMID = param.UserFCMID
                };
                SaveNotificationResponse(_Response);
            }

            return _res;
        }
        #endregion

        #region OTP
        public IResponseStatus OTPSMS(AlertReplacementModel param)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS
            };
            string sendRes = string.Empty;
            string SMS = string.Empty;
            var Sqlparam = new CommonReq()
            {
                LoginID = param.LoginID,
                CommonInt = MessageFormat.OTP
            };
            IProcedure _proc = new ProcGetSMSSettingByFormat(_dal);
            var smsSetting = (SMSSetting)_proc.Call(Sqlparam);

            if (smsSetting.IsEnableSMS)
            {
                StringBuilder sbUrl = new StringBuilder(smsSetting.URL);
                bool IsNoTemplate = true;
                if (string.IsNullOrEmpty(smsSetting.Template))
                {
                    sendRes = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if (smsSetting.SMSID == 0)
                    {
                        sendRes = "No API Found";
                    }
                    SMS = GetFormatedMessage(smsSetting.Template, param);
                    if (smsSetting.SMSID > 0 && !string.IsNullOrEmpty(smsSetting.URL))
                    {
                        sbUrl.Replace("{SENDERID}", smsSetting.SenderID);
                        sbUrl.Replace("{TO}", param.UserMobileNo);
                        sbUrl.Replace("{MESSAGE}", SMS);
                        var p = new SendSMSRequest
                        {
                            APIMethod = smsSetting.APIMethod,
                            SmsURL = sbUrl.ToString()
                        };
                        sendRes = _sendSMSML.CallSendSMSAPI(p);
                    }
                }
                var _Response = new SMSResponse
                {
                    ReqURL = sbUrl.ToString(),
                    Response = Convert.ToString(sendRes),
                    ResponseID = "",
                    Status = SMSResponseTYPE.SEND,
                    SMSID = smsSetting.SMSID,
                    MobileNo = param.UserMobileNo,
                    TransactionID = "",
                    SMS = SMS,
                    WID = param.WID
                };
                SaveSMSResponse(_Response);
            }
            return _res;
        }
        public IResponseStatus OTPEmail(AlertReplacementModel param)
        {
            bool IsSent = false;
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS
            };
            string EmailBody = string.Empty;
            var Sqlparam = new CommonReq()
            {
                LoginID = param.LoginID,
                CommonInt = MessageFormat.OTP
            };
            IProcedure _proc = new ProcGetEmailSettingByFormat(_dal);
            var mailSetting = (EmailSettingswithFormat)_proc.Call(Sqlparam);
            if (mailSetting.IsEnableEmail)
            {
                bool IsNoTemplate = true;
                if (string.IsNullOrEmpty(mailSetting.EmailTemplate))
                {
                    EmailBody = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if (string.IsNullOrEmpty(mailSetting.FromEmail))
                    {
                        EmailBody = "No Email Found";
                    }
                    EmailBody = GetFormatedMessage(mailSetting.EmailTemplate, param);
                    if (param.WID > 0)
                    {
                        if (!string.IsNullOrEmpty(mailSetting.FromEmail))
                        {
                            IEmailML emailManager = new EmailML(_dal);
                            string logo = _resourceML.GetLogoURL(param.WID).ToString();
                            string Footer = "<p><h4 style='color:#000000;font-family:verdana,sans-serif;margin-bottom:1.5px'><em>{CompanyName}</em></h4><span>{CompanyAddress}</span></p>";
                            Footer = Footer.Replace("{CompanyName}", param.Company).Replace("{CompanyAddress}", param.CompanyAddress);
                            IsSent = emailManager.SendEMail(mailSetting, param.UserEmailID, null, mailSetting.Subject, EmailBody, param.WID, logo, true, Footer);
                        }
                    }
                }
                SendEmail sendEmail = new SendEmail
                {
                    From = mailSetting.FromEmail,
                    Body = EmailBody,
                    Recipients = param.UserEmailID,
                    Subject = mailSetting.Subject,
                    IsSent = IsSent,
                    WID = param.WID
                };
                EmailDL emailDL = new EmailDL(_dal);
                emailDL.SaveMail(sendEmail);
            }
            return _res;
        }
        #endregion

        #region ForgetPassword
        public IResponseStatus ForgetPasswordSMS(AlertReplacementModel param)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS
            };
            string sendRes = string.Empty;
            string SMS = string.Empty;
            var Sqlparam = new CommonReq
            {
                LoginID = param.LoginID,
                CommonInt = MessageFormat.ForgetPass,
                CommonInt2 = param.WID
            };
            IProcedure _proc = new ProcGetSMSSettingByFormat(_dal);
            var smsSetting = (SMSSetting)_proc.Call(Sqlparam);
            StringBuilder sbUrl = new StringBuilder(smsSetting.URL);
            if (smsSetting.IsEnableSMS)
            {
                bool IsNoTemplate = true;
                if (string.IsNullOrEmpty(smsSetting.Template))
                {
                    sendRes = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if (smsSetting.SMSID == 0)
                    {
                        sendRes = "No API Found";
                    }
                    SMS = GetFormatedMessage(smsSetting.Template, param);
                    if (smsSetting.SMSID > 0 && !string.IsNullOrEmpty(smsSetting.URL))
                    {

                        sbUrl.Replace("{SENDERID}", smsSetting.SenderID);
                        sbUrl.Replace("{TO}", param.LoginMobileNo);
                        sbUrl.Replace("{MESSAGE}", SMS);
                        var p = new SendSMSRequest()
                        {
                            APIMethod = smsSetting.APIMethod,
                            SmsURL = sbUrl.ToString()
                        };
                        sendRes = _sendSMSML.CallSendSMSAPI(p);
                    }
                }
                var _Response = new SMSResponse
                {
                    ReqURL = sbUrl.ToString(),
                    Response = Convert.ToString(sendRes),
                    ResponseID = "",
                    Status = SMSResponseTYPE.SEND,
                    SMSID = smsSetting.SMSID,
                    MobileNo = param.LoginMobileNo,
                    TransactionID = "",
                    SMS = SMS,
                    WID = param.WID
                };
                SaveSMSResponse(_Response);
            }
            return _res;
        }
        public bool ForgetPasswordEmail(AlertReplacementModel param)
        {
            bool IsSent = false;
            string EmailBody = string.Empty;
            var Sqlparam = new CommonReq
            {
                LoginID = param.LoginID,
                CommonInt = MessageFormat.ForgetPass,
                CommonInt2 = param.WID,
                CommonBool = param.IsSendFailed
            };
            IProcedure _proc = new ProcGetEmailSettingByFormat(_dal);
            var mailSetting = (EmailSettingswithFormat)_proc.Call(Sqlparam);
            if (mailSetting.IsEnableEmail)
            {
                bool IsNoTemplate = true;
                if (string.IsNullOrEmpty(mailSetting.EmailTemplate))
                {
                    EmailBody = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if (string.IsNullOrEmpty(mailSetting.FromEmail))
                    {
                        EmailBody = "No Email Found";
                    }
                    EmailBody = GetFormatedMessage(mailSetting.EmailTemplate, param);
                    if (param.WID > 0)
                    {
                        if (!string.IsNullOrEmpty(mailSetting.FromEmail))
                        {
                            IEmailML emailManager = new EmailML(_dal);
                            string logo = _resourceML.GetLogoURL(param.WID).ToString();
                            string Footer = "<p><h4 style='color:#000000;font-family:verdana,sans-serif;margin-bottom:1.5px'><em>{CompanyName}</em></h4><span>{CompanyAddress}</span></p>";
                            Footer = Footer.Replace("{CompanyName}", param.Company).Replace("{CompanyAddress}", param.CompanyAddress);
                            IsSent = emailManager.SendEMail(mailSetting, param.LoginEmailID, null, mailSetting.Subject, EmailBody, param.WID, logo, true, Footer);
                        }
                    }
                }
                SendEmail sendEmail = new SendEmail
                {
                    From = mailSetting.FromEmail,
                    Body = EmailBody,
                    Recipients = param.LoginEmailID,
                    Subject = mailSetting.Subject,
                    IsSent = IsSent,
                    WID = param.WID
                };
                EmailDL emailDL = new EmailDL(_dal);
                emailDL.SaveMail(sendEmail);
            }
            return IsSent;
        }
        #endregion

        #region Registration
        public IResponseStatus RegistrationSMS(AlertReplacementModel param)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS
            };
            string sendRes = string.Empty;
            string SMS = string.Empty;
            var Sqlparam = new CommonReq()
            {
                LoginID = param.LoginID,
                CommonInt = MessageFormat.Registration
            };
            IProcedure _proc = new ProcGetSMSSettingByFormat(_dal);
            var smsSetting = (SMSSetting)_proc.Call(Sqlparam);
            if (smsSetting.IsEnableSMS)
            {
                bool IsNoTemplate = true;
                StringBuilder sbUrl = new StringBuilder(smsSetting.URL);
                if (string.IsNullOrEmpty(smsSetting.Template))
                {
                    sendRes = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if (smsSetting.SMSID == 0)
                    {
                        sendRes = "No API Found";
                    }
                    SMS = GetFormatedMessage(smsSetting.Template, param);
                    if (smsSetting.SMSID > 0 && !string.IsNullOrEmpty(smsSetting.URL))
                    {
                        sbUrl.Replace("{SENDERID}", smsSetting.SenderID);
                        sbUrl.Replace("{TO}", param.UserMobileNo);
                        sbUrl.Replace("{MESSAGE}", SMS);
                        var p = new SendSMSRequest
                        {
                            APIMethod = smsSetting.APIMethod,
                            SmsURL = sbUrl.ToString()
                        };
                        sendRes = _sendSMSML.CallSendSMSAPI(p);
                    }
                }
                var _Response = new SMSResponse
                {
                    ReqURL = sbUrl.ToString(),
                    Response = Convert.ToString(sendRes),
                    ResponseID = "",
                    Status = SMSResponseTYPE.SEND,
                    SMSID = smsSetting.SMSID,
                    MobileNo = param.UserMobileNo,
                    TransactionID = "",
                    SMS = SMS,
                    WID = param.WID
                };
                SaveSMSResponse(_Response);
            }
            return _res;
        }
        public IResponseStatus RegistrationEmail(AlertReplacementModel param)
        {
            bool IsSent = false;
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS
            };
            string EmailBody = string.Empty;
            var Sqlparam = new CommonReq()
            {
                LoginID = param.LoginID,
                CommonInt = MessageFormat.Registration
            };
            IProcedure _proc = new ProcGetEmailSettingByFormat(_dal);
            var mailSetting = (EmailSettingswithFormat)_proc.Call(Sqlparam);
            if (mailSetting.IsEnableEmail)
            {
                bool IsNoTemplate = true;
                if (string.IsNullOrEmpty(mailSetting.EmailTemplate))
                {
                    EmailBody = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if (string.IsNullOrEmpty(mailSetting.FromEmail))
                    {
                        EmailBody = "No Email Found";
                    }
                    EmailBody = GetFormatedMessage(mailSetting.EmailTemplate, param);
                    if (param.WID > 0)
                    {
                        if (!string.IsNullOrEmpty(mailSetting.FromEmail))
                        {
                            IEmailML emailManager = new EmailML(_dal);
                            string logo = _resourceML.GetLogoURL(param.WID).ToString();
                            string Footer = "<p><h4 style='color:#000000;font-family:verdana,sans-serif;margin-bottom:1.5px'><em>{CompanyName}</em></h4><span>{CompanyAddress}</span></p>";
                            Footer = Footer.Replace("{CompanyName}", param.Company).Replace("{CompanyAddress}", param.CompanyAddress);
                            IsSent = emailManager.SendEMail(mailSetting, param.UserEmailID, null, mailSetting.Subject, EmailBody, param.WID, logo, true, Footer);
                        }
                    }
                }
                SendEmail sendEmail = new SendEmail
                {
                    From = mailSetting.FromEmail,
                    Body = EmailBody,
                    Recipients = param.UserEmailID + "," + (param.bccList != null ? (param.bccList.Count > 0 ? String.Join(",", param.bccList) : "") : ""),
                    Subject = mailSetting.Subject,
                    IsSent = IsSent,
                    WID = param.WID
                };
                EmailDL emailDL = new EmailDL(_dal);
                emailDL.SaveMail(sendEmail);
            }
            return _res;
        }
        #endregion

        #region OperatorUP
        public IResponseStatus OperatorUPSMS(AlertReplacementModel param, bool IsUp)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS
            };
            string sendRes = string.Empty;
            string SMS = string.Empty;
            var Sqlparam = new CommonReq()
            {
                LoginID = param.LoginID,
                CommonInt = IsUp == true ? MessageFormat.OperatorUPMessage : MessageFormat.OperatorDownMessage
            };
            IProcedure _proc = new ProcGetSMSSettingByFormat(_dal);
            var smsSetting = (SMSSetting)_proc.Call(Sqlparam);
            if (smsSetting.IsEnableSMS)
            {
                bool IsNoTemplate = true;
                if (string.IsNullOrEmpty(smsSetting.Template))
                {
                    sendRes = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if (smsSetting.SMSID == 0)
                    {
                        sendRes = "No API Found";
                    }
                    SMS = GetFormatedMessage(smsSetting.Template, param);
                    if (smsSetting.SMSID > 0 && !string.IsNullOrEmpty(smsSetting.URL))
                    {
                        StringBuilder sbUrl = new StringBuilder(smsSetting.URL);
                        sbUrl.Replace("{SENDERID}", smsSetting.SenderID);
                        sbUrl.Replace("{TO}", param.UserMobileNo);
                        sbUrl.Replace("{MESSAGE}", SMS);
                        var p = new SendSMSRequest()
                        {
                            APIMethod = smsSetting.APIMethod,
                            SmsURL = sbUrl.ToString()
                        };
                        sendRes = _sendSMSML.CallSendSMSAPI(p);
                    }
                }
                var _Response = new SMSResponse
                {
                    ReqURL = smsSetting.URL,
                    Response = Convert.ToString(sendRes),
                    ResponseID = "",
                    Status = SMSResponseTYPE.SEND,
                    SMSID = smsSetting.SMSID,
                    MobileNo = param.UserMobileNo,
                    TransactionID = "",
                    SMS = SMS,
                    WID = param.WID
                };
                SaveSMSResponse(_Response);
            }
            return _res;
        }
        public IResponseStatus OperatorUpEmail(AlertReplacementModel param, bool IsUp)
        {
            bool IsSent = false;
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS
            };
            string EmailBody = string.Empty;
            string Subject = string.Empty;
            var Sqlparam = new CommonReq()
            {
                LoginID = param.LoginID,
                CommonInt = IsUp == true ? MessageFormat.OperatorUPMessage : MessageFormat.OperatorDownMessage
            };
            IProcedure _proc = new ProcGetEmailSettingByFormat(_dal);
            var mailSetting = (EmailSettingswithFormat)_proc.Call(Sqlparam);
            if (mailSetting.IsEnableEmail)
            {
                bool IsNoTemplate = true;
                if (string.IsNullOrEmpty(mailSetting.EmailTemplate))
                {
                    EmailBody = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if (string.IsNullOrEmpty(mailSetting.FromEmail))
                    {
                        EmailBody = "No Email Found";
                    }

                    EmailBody = GetFormatedMessage(mailSetting.EmailTemplate, param);
                    Subject = mailSetting.Subject.Replace("{Operator}", param.Operator).Replace("{OperatorName}", param.Operator);
                    if (param.WID > 0)
                    {
                        if (!string.IsNullOrEmpty(mailSetting.FromEmail))
                        {
                            IEmailML emailManager = new EmailML(_dal);
                            string logo = _resourceML.GetLogoURL(param.WID).ToString();
                            string Footer = "<p><h4 style='color:#000000;font-family:verdana,sans-serif;margin-bottom:1.5px'><em>{CompanyName}</em></h4><span>{CompanyAddress}</span></p>";
                            Footer = Footer.Replace("{CompanyName}", param.Company).Replace("{CompanyAddress}", param.CompanyAddress);
                            IsSent = emailManager.SendEMail(mailSetting, param.UserEmailID, param.bccList, Subject, EmailBody, param.WID, logo, true, Footer);
                        }
                    }
                }
                SendEmail sendEmail = new SendEmail
                {
                    From = mailSetting.FromEmail,
                    Body = EmailBody,
                    Recipients = (param.bccList != null ? (param.bccList.Count > 0 ? String.Join(",", param.bccList) : "") : "") + ",",
                    Subject = Subject,
                    IsSent = IsSent,
                    WID = param.WID
                };
                EmailDL emailDL = new EmailDL(_dal);
                emailDL.SaveMail(sendEmail);
            }
            return _res;
        }
        public IResponseStatus OperatorUpNotification(AlertReplacementModel param, bool IsUp, bool IsBulk)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS
            };
            string _Notification = string.Empty;
            string _response = string.Empty;
            var Sqlparam = new CommonReq()
            {
                CommonInt = IsUp == true ? MessageFormat.OperatorUPMessage : MessageFormat.OperatorDownMessage
            };
            IProcedure _proc = new ProcGetNotificationFormat(_dal);
            var NotificationDetail = (ResponseStatus)_proc.Call(Sqlparam);
            if (NotificationDetail.CommonBool)
            {
                string Title = IsUp == true ? nameof(MessageFormat.OperatorUPMessage) : nameof(MessageFormat.OperatorDownMessage);
                StringBuilder logo = new StringBuilder();
                logo.AppendFormat("{0}.png", IsUp ? MessageFormat.OperatorUPMessage : MessageFormat.OperatorDownMessage);
                bool IsNoTemplate = true;
                if (string.IsNullOrEmpty(NotificationDetail.CommonStr))
                {
                    _Notification = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if (!IsBulk)
                    {
                        if ((param.UserFCMID ?? string.Empty).Length < 5)
                        {
                            _Notification = "No FCMID Found";
                            IsNoTemplate = false;
                        }
                    }
                    else
                    {
                        param.UserFCMID = "-1";
                    }
                    _Notification = GetFormatedMessage(NotificationDetail.CommonStr, param);

                    if (param.WID > 0 && IsNoTemplate)
                    {
                        SendSMSML sendSmsMl = new SendSMSML(_accessor, _env);
                        if (IsBulk)
                        {
                            _response = sendSmsMl.SendNotification(_Notification, logo.ToString(), Title, param.URL);
                        }
                        else
                        {
                            _response = sendSmsMl.SendNotification(param.UserFCMID, _Notification, logo.ToString(), Title, param.URL);
                        }
                    }
                }
                var _Response = new Notification
                {
                    LoginID = param.LoginID,
                    Response = _response,
                    Title = Title,
                    ImageUrl = logo.ToString(),
                    Url = param.URL,
                    Message = _Notification,
                    WID = param.WID,
                    UserID = param.UserID,
                    FCMID = param.UserFCMID
                };
                SaveNotificationResponse(_Response);
            }
            return _res;
        }
        #endregion

        #region FundOrder
        public IResponseStatus FundOrderSMS(AlertReplacementModel param)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS
            };
            string sendRes = string.Empty;
            string SMS = string.Empty;
            var Sqlparam = new CommonReq()
            {
                LoginID = param.LoginID,
                CommonInt = MessageFormat.FundOrderAlert
            };
            IProcedure _proc = new ProcGetSMSSettingByFormat(_dal);
            var smsSetting = (SMSSetting)_proc.Call(Sqlparam);
            if (smsSetting.IsEnableSMS)
            {
                bool IsNoTemplate = true;
                StringBuilder sbUrl = new StringBuilder(smsSetting.URL);
                if (string.IsNullOrEmpty(smsSetting.Template))
                {
                    sendRes = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if (smsSetting.SMSID == 0)
                    {
                        sendRes = "No API Found";
                    }
                    SMS = GetFormatedMessage(smsSetting.Template, param);
                    if (smsSetting.SMSID > 0 && !string.IsNullOrEmpty(smsSetting.URL))
                    {
                        sbUrl.Replace("{SENDERID}", smsSetting.SenderID);
                        sbUrl.Replace("{TO}", param.UserMobileNo);
                        sbUrl.Replace("{MESSAGE}", SMS);
                        var p = new SendSMSRequest()
                        {
                            APIMethod = smsSetting.APIMethod,
                            SmsURL = sbUrl.ToString()
                        };
                        sendRes = _sendSMSML.CallSendSMSAPI(p);
                    }
                }
                var _Response = new SMSResponse
                {
                    ReqURL = sbUrl.ToString(),
                    Response = Convert.ToString(sendRes),
                    ResponseID = "",
                    Status = SMSResponseTYPE.SEND,
                    SMSID = smsSetting.SMSID,
                    MobileNo = param.UserMobileNo,
                    TransactionID = "",
                    SMS = SMS,
                    WID = param.WID
                };
                SaveSMSResponse(_Response);
            }
            return _res;
        }
        public IResponseStatus FundOrderEmail(AlertReplacementModel param)
        {
            bool IsSent = false;
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS
            };
            string EmailBody = string.Empty;
            var Sqlparam = new CommonReq()
            {
                LoginID = param.LoginID,
                CommonInt = MessageFormat.FundOrderAlert
            };
            IProcedure _proc = new ProcGetEmailSettingByFormat(_dal);
            var mailSetting = (EmailSettingswithFormat)_proc.Call(Sqlparam);
            if (mailSetting.IsEnableEmail)
            {
                bool IsNoTemplate = true;
                if (string.IsNullOrEmpty(mailSetting.EmailTemplate))
                {
                    EmailBody = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if (string.IsNullOrEmpty(mailSetting.FromEmail))
                    {
                        EmailBody = "No Email Found";
                    }
                    EmailBody = GetFormatedMessage(mailSetting.EmailTemplate, param);
                    if (param.WID > 0)
                    {
                        if (!string.IsNullOrEmpty(mailSetting.FromEmail))
                        {
                            IEmailML emailManager = new EmailML(_dal);
                            string logo = _resourceML.GetLogoURL(param.WID).ToString();
                            string Footer = "<p><h4 style='color:#000000;font-family:verdana,sans-serif;margin-bottom:1.5px'><em>{CompanyName}</em></h4><span>{CompanyAddress}</span></p>";
                            Footer = Footer.Replace("{CompanyName}", param.Company).Replace("{CompanyAddress}", param.CompanyAddress);
                            IsSent = emailManager.SendEMail(mailSetting, param.UserEmailID, null, mailSetting.Subject, EmailBody, param.WID, logo, true, Footer);
                        }
                    }
                }
                SendEmail sendEmail = new SendEmail
                {
                    From = mailSetting.FromEmail,
                    Body = EmailBody,
                    Recipients = param.UserEmailID,
                    Subject = mailSetting.Subject,
                    IsSent = IsSent,
                    WID = param.WID
                };
                EmailDL emailDL = new EmailDL(_dal);
                emailDL.SaveMail(sendEmail);
            }
            return _res;
        }
        public IResponseStatus FundOrderNotification(AlertReplacementModel param)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS
            };
            string _Notification = string.Empty;
            string _response = string.Empty;
            string Title = nameof(MessageFormat.FundOrderAlert);
            StringBuilder logo = new StringBuilder();
            logo.AppendFormat("{0}.png", MessageFormat.FundOrderAlert);
            var Sqlparam = new CommonReq()
            {
                CommonInt = MessageFormat.FundOrderAlert
            };
            IProcedure _proc = new ProcGetNotificationFormat(_dal);
            var NotificationDetail = (ResponseStatus)_proc.Call(Sqlparam);
            if (NotificationDetail.CommonBool)
            {
                bool IsNoTemplate = true;
                if (string.IsNullOrEmpty(NotificationDetail.CommonStr))
                {
                    _Notification = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if ((param.UserFCMID ?? string.Empty).Length < 5)
                    {
                        _Notification = "No FCMID Found";
                        IsNoTemplate = false;
                    }
                    _Notification = GetFormatedMessage(NotificationDetail.CommonStr, param);
                    if (param.WID > 0 && IsNoTemplate)
                    {
                        SendSMSML sendSmsMl = new SendSMSML(_accessor, _env);
                        _response = sendSmsMl.SendNotification(param.UserFCMID, _Notification, logo.ToString(), Title, param.URL);
                    }
                }
                var _Response = new Notification
                {
                    LoginID = param.LoginID,
                    Response = _response,
                    Title = Title,
                    ImageUrl = logo.ToString(),
                    Url = param.URL,
                    Message = _Notification,
                    WID = param.WID,
                    UserID = param.UserID,
                    FCMID = param.UserFCMID
                };
                SaveNotificationResponse(_Response);
            }
            return _res;
        }
        #endregion

        #region KYCApprovedAndReject
        public IResponseStatus KYCApprovalSMS(AlertReplacementModel param, bool IsApproved)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS
            };
            string sendRes = string.Empty;
            string SMS = string.Empty;
            var Sqlparam = new CommonReq()
            {
                LoginID = param.LoginID,
                CommonInt = IsApproved ? MessageFormat.KYCApproved : MessageFormat.KYCReject
            };
            IProcedure _proc = new ProcGetSMSSettingByFormat(_dal);
            var smsSetting = (SMSSetting)_proc.Call(Sqlparam);
            if (smsSetting.IsEnableSMS)
            {
                bool IsNoTemplate = true;
                StringBuilder sbUrl = new StringBuilder(smsSetting.URL);
                if (string.IsNullOrEmpty(smsSetting.Template))
                {
                    sendRes = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if (smsSetting.SMSID == 0)
                    {
                        sendRes = "No API Found";
                    }
                    SMS = GetFormatedMessage(smsSetting.Template, param);
                    if (smsSetting.SMSID > 0 && !string.IsNullOrEmpty(smsSetting.URL))
                    {
                        sbUrl.Replace("{SENDERID}", smsSetting.SenderID);
                        sbUrl.Replace("{TO}", param.UserMobileNo);
                        sbUrl.Replace("{MESSAGE}", SMS);
                        var p = new SendSMSRequest()
                        {
                            APIMethod = smsSetting.APIMethod,
                            SmsURL = sbUrl.ToString()
                        };
                        sendRes = _sendSMSML.CallSendSMSAPI(p);
                    }
                }
                var _Response = new SMSResponse
                {
                    ReqURL = sbUrl.ToString(),
                    Response = Convert.ToString(sendRes),
                    ResponseID = "",
                    Status = SMSResponseTYPE.SEND,
                    SMSID = smsSetting.SMSID,
                    MobileNo = param.UserMobileNo,
                    TransactionID = "",
                    SMS = SMS,
                    WID = param.WID
                };
                SaveSMSResponse(_Response);
            }
            return _res;
        }
        public IResponseStatus KYCApprovalEmail(AlertReplacementModel param, bool IsApproved)
        {
            bool IsSent = false;
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS
            };
            string EmailBody = string.Empty;
            var Sqlparam = new CommonReq()
            {
                LoginID = param.LoginID,
                CommonInt = IsApproved ? MessageFormat.KYCApproved : MessageFormat.KYCReject
            };
            IProcedure _proc = new ProcGetEmailSettingByFormat(_dal);
            var mailSetting = (EmailSettingswithFormat)_proc.Call(Sqlparam);
            if (mailSetting.IsEnableEmail)
            {
                bool IsNoTemplate = true;
                if (string.IsNullOrEmpty(mailSetting.EmailTemplate))
                {
                    EmailBody = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if (string.IsNullOrEmpty(mailSetting.FromEmail))
                    {
                        EmailBody = "No Email Found";
                    }
                    EmailBody = GetFormatedMessage(mailSetting.EmailTemplate, param);

                    if (param.WID > 0)
                    {
                        if (!string.IsNullOrEmpty(mailSetting.FromEmail))
                        {
                            IEmailML emailManager = new EmailML(_dal);
                            string logo = _resourceML.GetLogoURL(param.WID).ToString();
                            string Footer = "<p><h4 style='color:#000000;font-family:verdana,sans-serif;margin-bottom:1.5px'><em>{CompanyName}</em></h4><span>{CompanyAddress}</span></p>";
                            Footer = Footer.Replace("{CompanyName}", param.Company).Replace("{CompanyAddress}", param.CompanyAddress);
                            IsSent = emailManager.SendEMail(mailSetting, param.UserEmailID, param.bccList, mailSetting.Subject, EmailBody, param.WID, logo, true, Footer);
                        }
                    }
                }
                SendEmail sendEmail = new SendEmail
                {
                    From = mailSetting.FromEmail,
                    Body = EmailBody,
                    Recipients = param.UserEmailID + "," + (param.bccList != null ? (param.bccList.Count > 0 ? String.Join(",", param.bccList) : "") : ""),
                    Subject = mailSetting.Subject,
                    IsSent = IsSent,
                    WID = param.WID
                };
                EmailDL emailDL = new EmailDL(_dal);
                emailDL.SaveMail(sendEmail);
            }
            return _res;
        }
        public IResponseStatus KYCApprovalNotification(AlertReplacementModel param, bool IsApproved)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS
            };
            string _Notification = string.Empty;
            string _response = string.Empty;
            string Title = IsApproved ? nameof(MessageFormat.KYCApproved) : nameof(MessageFormat.KYCReject);
            StringBuilder logo = new StringBuilder();
            logo.AppendFormat("{0}.png", IsApproved ? MessageFormat.KYCApproved : MessageFormat.KYCReject);
            var Sqlparam = new CommonReq()
            {
                CommonInt = IsApproved ? MessageFormat.KYCApproved : MessageFormat.KYCReject
            };
            IProcedure _proc = new ProcGetNotificationFormat(_dal);
            var NotificationDetail = (ResponseStatus)_proc.Call(Sqlparam);
            if (NotificationDetail.CommonBool)
            {
                bool IsNoTemplate = true;
                if (string.IsNullOrEmpty(NotificationDetail.CommonStr))
                {
                    _Notification = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if ((param.UserFCMID ?? string.Empty).Length < 5)
                    {
                        _Notification = "No FCMID Found";
                        IsNoTemplate = false;
                    }
                    _Notification = GetFormatedMessage(NotificationDetail.CommonStr, param);
                    if (param.WID > 0 && IsNoTemplate)
                    {
                        SendSMSML sendSmsMl = new SendSMSML(_accessor, _env);
                        _response = sendSmsMl.SendNotification(param.UserFCMID, _Notification, logo.ToString(), Title, param.URL);
                    }
                }
                var _Response = new Notification
                {
                    LoginID = param.LoginID,
                    Response = _response,
                    Title = Title,
                    ImageUrl = logo.ToString(),
                    Url = param.UserFCMID,
                    Message = _Notification,
                    WID = param.WID
                };
                SaveNotificationResponse(_Response);
            }
            return _res;
        }
        #endregion

        #region RecharegeSuccessAndFailed
        public IResponseStatus RecharegeSuccessSMS(AlertReplacementModel param, bool IsSuccess)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS
            };
            string sendRes = string.Empty;
            string SMS = string.Empty;
            var Sqlparam = new CommonReq()
            {
                LoginID = param.LoginID,
                CommonInt = IsSuccess ? MessageFormat.RechargeSuccess : MessageFormat.RechargeFailed
            };
            IProcedure _proc = new ProcGetSMSSettingByFormat(_dal);
            var smsSetting = (SMSSetting)_proc.Call(Sqlparam);
            if (smsSetting.IsEnableSMS)
            {
                bool IsNoTemplate = true;
                StringBuilder sbUrl = new StringBuilder(smsSetting.URL);
                if (string.IsNullOrEmpty(smsSetting.Template))
                {
                    sendRes = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if (smsSetting.SMSID == 0)
                    {
                        sendRes = "No API Found";
                    }
                    SMS = GetFormatedMessage(smsSetting.Template, param);
                    if (smsSetting.SMSID > 0 && !string.IsNullOrEmpty(smsSetting.URL))
                    {
                        sbUrl.Replace("{SENDERID}", smsSetting.SenderID);
                        sbUrl.Replace("{TO}", param.UserMobileNo);
                        sbUrl.Replace("{MESSAGE}", SMS);
                        var p = new SendSMSRequest()
                        {
                            APIMethod = smsSetting.APIMethod,
                            SmsURL = sbUrl.ToString()
                        };
                        sendRes = _sendSMSML.CallSendSMSAPI(p);
                    }
                }
                var _Response = new SMSResponse
                {
                    ReqURL = sbUrl.ToString(),
                    Response = Convert.ToString(sendRes),
                    ResponseID = "",
                    Status = SMSResponseTYPE.SEND,
                    SMSID = smsSetting.SMSID,
                    MobileNo = param.UserMobileNo,
                    TransactionID = param.TransactionID,
                    SMS = SMS,
                    WID = param.WID
                };
                SaveSMSResponse(_Response);
            }
            return _res;
        }
        public IResponseStatus RecharegeSuccessEmail(AlertReplacementModel param, bool IsSuccess)
        {
            bool IsSent = false;
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS
            };
            string EmailBody = string.Empty;
            var Sqlparam = new CommonReq()
            {
                LoginID = param.LoginID,
                CommonInt = IsSuccess ? MessageFormat.RechargeSuccess : MessageFormat.RechargeFailed
            };
            IProcedure _proc = new ProcGetEmailSettingByFormat(_dal);
            var mailSetting = (EmailSettingswithFormat)_proc.Call(Sqlparam);
            if (mailSetting.IsEnableEmail)
            {
                bool IsNoTemplate = true;
                if (string.IsNullOrEmpty(mailSetting.EmailTemplate))
                {
                    EmailBody = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if (string.IsNullOrEmpty(mailSetting.FromEmail))
                    {
                        EmailBody = "No Email Found";
                    }
                    EmailBody = GetFormatedMessage(mailSetting.EmailTemplate, param);
                    if (param.WID > 0)
                    {
                        if (!string.IsNullOrEmpty(mailSetting.FromEmail))
                        {
                            IEmailML emailManager = new EmailML(_dal);
                            string logo = _resourceML.GetLogoURL(param.WID).ToString();
                            string Footer = "<p><h4 style='color:#000000;font-family:verdana,sans-serif;margin-bottom:1.5px'><em>{CompanyName}</em></h4><span>{CompanyAddress}</span></p>";
                            Footer = Footer.Replace("{CompanyName}", param.Company).Replace("{CompanyAddress}", param.CompanyAddress);
                            IsSent = emailManager.SendEMail(mailSetting, param.UserEmailID, null, mailSetting.Subject, EmailBody, param.WID, logo, true, Footer);
                        }
                    }
                }
                SendEmail sendEmail = new SendEmail
                {
                    From = mailSetting.FromEmail,
                    Body = EmailBody,
                    Recipients = param.UserEmailID,
                    Subject = mailSetting.Subject,
                    IsSent = IsSent,
                    WID = param.WID
                };
                EmailDL emailDL = new EmailDL(_dal);
                emailDL.SaveMail(sendEmail);
            }
            return _res;
        }
        public IResponseStatus RecharegeSuccessNotification(AlertReplacementModel param, bool IsSuccess)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS
            };
            string _Notification = string.Empty;
            string _response = string.Empty;
            string Title = IsSuccess ? nameof(MessageFormat.RechargeSuccess) : nameof(MessageFormat.RechargeFailed);
            StringBuilder logo = new StringBuilder();
            logo.AppendFormat("{0}.png", IsSuccess ? MessageFormat.RechargeSuccess : MessageFormat.RechargeFailed);
            var Sqlparam = new CommonReq()
            {
                CommonInt = IsSuccess ? MessageFormat.RechargeSuccess : MessageFormat.RechargeFailed
            };
            IProcedure _proc = new ProcGetNotificationFormat(_dal);
            var NotificationDetail = (ResponseStatus)_proc.Call(Sqlparam);
            if (NotificationDetail.CommonBool)
            {
                bool IsNoTemplate = true;
                if (string.IsNullOrEmpty(NotificationDetail.CommonStr))
                {
                    _Notification = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if ((param.UserFCMID ?? string.Empty).Length < 5)
                    {
                        _Notification = "No FCMID Found";
                        IsNoTemplate = false;
                    }
                    _Notification = GetFormatedMessage(NotificationDetail.CommonStr, param);
                    if (param.WID > 0 && IsNoTemplate)
                    {
                        SendSMSML sendSmsMl = new SendSMSML(_accessor, _env);
                        _response = sendSmsMl.SendNotification(param.UserFCMID, _Notification, logo.ToString(), Title, param.URL);
                    }
                }
                var _Response = new Notification
                {
                    LoginID = param.LoginID,
                    Response = _response,
                    Title = Title,
                    ImageUrl = logo.ToString(),
                    Url = param.URL,
                    Message = _Notification,
                    WID = param.WID,
                    UserID = param.UserID,
                    FCMID = param.UserFCMID
                };
                SaveNotificationResponse(_Response);
            }
            return _res;
        }

        #endregion

        #region RechargeRefund
        public IResponseStatus RechargeRefundSMS(AlertReplacementModel param, bool IsRejected)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS
            };
            string sendRes = string.Empty;
            string SMS = string.Empty;
            var Sqlparam = new CommonReq()
            {
                LoginID = param.LoginID,
                CommonInt = IsRejected ? MessageFormat.RechargeRefundReject : MessageFormat.RechargeRefund
            };
            IProcedure _proc = new ProcGetSMSSettingByFormat(_dal);
            var smsSetting = (SMSSetting)_proc.Call(Sqlparam);
            if (smsSetting.IsEnableSMS)
            {
                bool IsNoTemplate = true;
                StringBuilder sbUrl = new StringBuilder(smsSetting.URL);
                if (string.IsNullOrEmpty(smsSetting.Template))
                {
                    sendRes = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if (smsSetting.SMSID == 0)
                    {
                        sendRes = "No API Found";
                    }
                    SMS = GetFormatedMessage(smsSetting.Template, param);
                    if (smsSetting.SMSID > 0 && !string.IsNullOrEmpty(smsSetting.URL))
                    {

                        sbUrl.Replace("{SENDERID}", smsSetting.SenderID);
                        sbUrl.Replace("{TO}", param.UserMobileNo);
                        sbUrl.Replace("{MESSAGE}", SMS);
                        var p = new SendSMSRequest()
                        {
                            APIMethod = smsSetting.APIMethod,
                            SmsURL = sbUrl.ToString()
                        };
                        sendRes = _sendSMSML.CallSendSMSAPI(p);
                    }
                }
                var _Response = new SMSResponse
                {
                    ReqURL = sbUrl.ToString(),
                    Response = Convert.ToString(sendRes),
                    ResponseID = "",
                    Status = SMSResponseTYPE.SEND,
                    SMSID = smsSetting.SMSID,
                    MobileNo = param.UserMobileNo,
                    TransactionID = "",
                    SMS = SMS,
                    WID = param.WID
                };
                SaveSMSResponse(_Response);
            }
            return _res;
        }
        public IResponseStatus RechargeRefundEmail(AlertReplacementModel param, bool IsRejected)
        {
            bool IsSent = false;
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS
            };
            string EmailBody = string.Empty;
            var Sqlparam = new CommonReq()
            {
                LoginID = param.LoginID,
                CommonInt = IsRejected ? MessageFormat.RechargeRefundReject : MessageFormat.RechargeRefund
            };
            IProcedure _proc = new ProcGetEmailSettingByFormat(_dal);
            var mailSetting = (EmailSettingswithFormat)_proc.Call(Sqlparam);
            if (mailSetting.IsEnableEmail)
            {
                bool IsNoTemplate = true;
                if (string.IsNullOrEmpty(mailSetting.EmailTemplate))
                {
                    EmailBody = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if (string.IsNullOrEmpty(mailSetting.FromEmail))
                    {
                        EmailBody = "No Email Found";
                    }
                    EmailBody = GetFormatedMessage(mailSetting.EmailTemplate, param);
                    if (param.WID > 0)
                    {
                        if (!string.IsNullOrEmpty(mailSetting.FromEmail))
                        {
                            IEmailML emailManager = new EmailML(_dal);
                            string logo = _resourceML.GetLogoURL(param.WID).ToString();
                            string Footer = "<p><h4 style='color:#000000;font-family:verdana,sans-serif;margin-bottom:1.5px'><em>{CompanyName}</em></h4><span>{CompanyAddress}</span></p>";
                            Footer = Footer.Replace("{CompanyName}", param.Company).Replace("{CompanyAddress}", param.CompanyAddress);
                            IsSent = emailManager.SendEMail(mailSetting, param.UserEmailID, null, mailSetting.Subject, EmailBody, param.WID, logo, true, Footer);
                        }
                    }
                }
                SendEmail sendEmail = new SendEmail
                {
                    From = mailSetting.FromEmail,
                    Body = EmailBody,
                    Recipients = param.UserEmailID,
                    Subject = mailSetting.Subject,
                    IsSent = IsSent,
                    WID = param.WID
                };
                EmailDL emailDL = new EmailDL(_dal);
                emailDL.SaveMail(sendEmail);
            }
            return _res;
        }
        public IResponseStatus RechargeRefundNotification(AlertReplacementModel param, bool IsRejected)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS
            };
            string _Notification = string.Empty;
            string _response = string.Empty;
            string logo = Convert.ToString(IsRejected ? MessageFormat.RechargeRefundReject : MessageFormat.RechargeRefund) + ".png";
            var Sqlparam = new CommonReq()
            {
                CommonInt = IsRejected ? MessageFormat.RechargeRefundReject : MessageFormat.RechargeRefund
            };
            IProcedure _proc = new ProcGetNotificationFormat(_dal);
            var NotificationDetail = (ResponseStatus)_proc.Call(Sqlparam);
            if (NotificationDetail.CommonBool)
            {
                bool IsNoTemplate = true;
                if (string.IsNullOrEmpty(NotificationDetail.CommonStr))
                {
                    _Notification = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if ((param.UserFCMID ?? string.Empty).Length < 5)
                    {
                        _Notification = "No FCMID Found";
                        IsNoTemplate = false;
                    }
                    _Notification = GetFormatedMessage(NotificationDetail.CommonStr, param);
                    if (param.WID > 0 && IsNoTemplate)
                    {
                        SendSMSML sendSmsMl = new SendSMSML(_accessor, _env);
                        _response = sendSmsMl.SendNotification(param.UserFCMID, _Notification, logo, nameof(MessageFormat.RechargeRefund), param.URL);
                    }
                }
                var _Response = new Notification
                {
                    LoginID = param.LoginID,
                    Response = _response,
                    Title = nameof(MessageFormat.RechargeRefund),
                    ImageUrl = logo,
                    Url = param.URL,
                    Message = _Notification,
                    WID = param.WID,
                    UserID = param.UserID,
                    FCMID = param.UserFCMID
                };
                SaveNotificationResponse(_Response);
            }
            return _res;
        }
        #endregion

        #region LowBalanceAlert
        public async Task LowBalanceSMS(AlertReplacementModel param, SMSSetting smsSetting)
        {
            await Task.Delay(0).ConfigureAwait(false);
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS
            };
            string sendRes = string.Empty;
            string SMS = string.Empty;
            var SMSparam = new CommonReq()
            {
                LoginID = param.LoginID,
                CommonInt2 = param.WID,
                CommonInt = MessageFormat.LowBalanceFormat
            };
            if (smsSetting == null || (smsSetting != null && smsSetting.WID != param.WID))
            {
                IProcedure _proc = new ProcGetSMSSettingByFormat(_dal);
                smsSetting = (SMSSetting)_proc.Call(SMSparam);
            }
            if (smsSetting.IsEnableSMS)
            {
                bool IsNoTemplate = true;
                StringBuilder sbUrl = new StringBuilder(smsSetting.URL);
                if (string.IsNullOrEmpty(smsSetting.Template))
                {
                    sendRes = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if (smsSetting.SMSID == 0)
                    {
                        sendRes = "No API Found";
                    }
                    SMS = GetFormatedMessage(smsSetting.Template, param);
                    if (smsSetting.SMSID > 0 && !string.IsNullOrEmpty(smsSetting.URL))
                    {
                        sbUrl.Replace("{SENDERID}", smsSetting.SenderID);
                        sbUrl.Replace("{TO}", param.UserMobileNo);
                        sbUrl.Replace("{MESSAGE}", SMS);
                        var p = new SendSMSRequest()
                        {
                            APIMethod = smsSetting.APIMethod,
                            SmsURL = sbUrl.ToString()
                        };
                        sendRes = _sendSMSML.CallSendSMSAPI(p);
                    }
                }
                var _Response = new SMSResponse
                {
                    ReqURL = sbUrl.ToString(),
                    Response = Convert.ToString(sendRes),
                    ResponseID = "",
                    Status = SMSResponseTYPE.SEND,
                    SMSID = smsSetting.SMSID,
                    MobileNo = param.UserMobileNo,
                    TransactionID = "",
                    SMS = SMS,
                    WID = param.WID
                };
                SaveSMSResponse(_Response);
            }
            //return _res;
        }
        public async Task LowBalanceEmail(AlertReplacementModel param, EmailSettingswithFormat mailSetting)
        {
            await Task.Delay(0).ConfigureAwait(false);
            bool IsSent = false;
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS
            };
            string EmailBody = string.Empty;
            var SMSparam = new CommonReq()
            {
                LoginID = param.LoginID,
                CommonInt = MessageFormat.LowBalanceFormat,
                CommonInt2 = param.WID
            };
            IProcedure _proc = new ProcGetEmailSettingByFormat(_dal);
            if (mailSetting == null || (mailSetting != null && mailSetting.WID != param.WID))
                mailSetting = (EmailSettingswithFormat)_proc.Call(SMSparam);
            if (mailSetting.IsEnableEmail)
            {
                bool IsNoTemplate = true;
                if (string.IsNullOrEmpty(mailSetting.EmailTemplate))
                {
                    EmailBody = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if (string.IsNullOrEmpty(mailSetting.FromEmail))
                    {
                        EmailBody = "No Email Found";
                    }
                    EmailBody = GetFormatedMessage(mailSetting.EmailTemplate, param);
                    if (param.WID > 0)
                    {
                        if (!string.IsNullOrEmpty(mailSetting.FromEmail))
                        {
                            IEmailML emailManager = new EmailML(_dal);
                            string logo = _resourceML.GetLogoURL(param.WID).ToString();
                            string Footer = "<p><h4 style='color:#000000;font-family:verdana,sans-serif;margin-bottom:1.5px'><em>{CompanyName}</em></h4><span>{CompanyAddress}</span></p>";
                            Footer = Footer.Replace("{CompanyName}", param.Company).Replace("{CompanyAddress}", param.CompanyAddress);
                            IsSent = emailManager.SendEMail(mailSetting, param.UserEmailID, null, mailSetting.Subject, EmailBody, param.WID, logo, true, Footer);
                        }
                    }
                }
                SendEmail sendEmail = new SendEmail
                {
                    From = mailSetting.FromEmail,
                    Body = EmailBody,
                    Recipients = param.UserEmailID,
                    Subject = mailSetting.Subject,
                    IsSent = IsSent,
                    WID = param.WID
                };
                EmailDL emailDL = new EmailDL(_dal);
                emailDL.SaveMail(sendEmail);
            }
            //return _res;
        }
        public async Task LowBalanceNotification(AlertReplacementModel param)
        {
            await Task.Delay(0).ConfigureAwait(false);
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS
            };
            string _Notification = string.Empty;
            string _response = string.Empty;
            string Title = nameof(MessageFormat.LowBalanceFormat);
            StringBuilder logo = new StringBuilder();
            logo.AppendFormat("{0}.png", MessageFormat.LowBalanceFormat);
            var Sqlparam = new CommonReq()
            {
                CommonInt = MessageFormat.LowBalanceFormat
            };
            IProcedure _proc = new ProcGetNotificationFormat(_dal);
            var NotificationDetail = (ResponseStatus)_proc.Call(Sqlparam);
            if (NotificationDetail.CommonBool)
            {
                bool IsNoTemplate = true;
                if (string.IsNullOrEmpty(NotificationDetail.CommonStr))
                {
                    _Notification = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if ((param.UserFCMID ?? string.Empty).Length < 5)
                    {
                        _Notification = "No FCMID Found";
                        IsNoTemplate = false;
                    }
                    _Notification = GetFormatedMessage(NotificationDetail.CommonStr, param);
                    if (param.WID > 0 && IsNoTemplate)
                    {
                        SendSMSML sendSmsMl = new SendSMSML(_accessor, _env);
                        _response = sendSmsMl.SendNotification(param.UserFCMID, _Notification, logo.ToString(), Title, param.URL);
                    }
                }
                var _Response = new Notification
                {
                    LoginID = param.LoginID,
                    Response = _response,
                    Title = Title,
                    ImageUrl = logo.ToString(),
                    Url = param.UserFCMID,
                    Message = _Notification,
                    WID = param.WID,
                    UserID = param.UserID,
                    FCMID = param.UserFCMID
                };
                SaveNotificationResponse(_Response);
            }
            //return _res;
        }
        #endregion

        #region RecharegeAccept
        public IResponseStatus RecharegeAcceptSMS(AlertReplacementModel param)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS
            };
            string sendRes = string.Empty;
            string SMS = string.Empty;
            var Sqlparam = new CommonReq()
            {
                LoginID = param.LoginID,
                CommonInt = MessageFormat.RechargeAccept
            };
            IProcedure _proc = new ProcGetSMSSettingByFormat(_dal);
            var smsSetting = (SMSSetting)_proc.Call(Sqlparam);
            if (smsSetting.IsEnableSMS)
            {
                bool IsNoTemplate = true;
                StringBuilder sbUrl = new StringBuilder(smsSetting.URL);
                if (string.IsNullOrEmpty(smsSetting.Template))
                {
                    sendRes = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if (smsSetting.SMSID == 0)
                    {
                        sendRes = "No API Found";
                    }
                    SMS = GetFormatedMessage(smsSetting.Template, param);
                    if (smsSetting.SMSID > 0 && !string.IsNullOrEmpty(smsSetting.URL))
                    {
                        sbUrl.Replace("{SENDERID}", smsSetting.SenderID);
                        sbUrl.Replace("{TO}", param.UserMobileNo);
                        sbUrl.Replace("{MESSAGE}", SMS);
                        var p = new SendSMSRequest()
                        {
                            APIMethod = smsSetting.APIMethod,
                            SmsURL = sbUrl.ToString()
                        };
                        sendRes = _sendSMSML.CallSendSMSAPI(p);
                    }
                }
                var _Response = new SMSResponse
                {
                    ReqURL = sbUrl.ToString(),
                    Response = Convert.ToString(sendRes),
                    ResponseID = "",
                    Status = SMSResponseTYPE.SEND,
                    SMSID = smsSetting.SMSID,
                    MobileNo = param.UserMobileNo,
                    TransactionID = "",
                    SMS = SMS,
                    WID = param.WID
                };
                SaveSMSResponse(_Response);
            }
            return _res;
        }
        public IResponseStatus RecharegeAcceptEmail(AlertReplacementModel param)
        {
            bool IsSent = false;
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS
            };
            string EmailBody = string.Empty;
            var Sqlparam = new CommonReq()
            {
                LoginID = param.LoginID,
                CommonInt = MessageFormat.RechargeAccept
            };
            IProcedure _proc = new ProcGetEmailSettingByFormat(_dal);
            var mailSetting = (EmailSettingswithFormat)_proc.Call(Sqlparam);
            if (mailSetting.IsEnableEmail)
            {
                bool IsNoTemplate = true;
                if (string.IsNullOrEmpty(mailSetting.EmailTemplate))
                {
                    EmailBody = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if (string.IsNullOrEmpty(mailSetting.FromEmail))
                    {
                        EmailBody = "No Email Found";
                    }
                    EmailBody = GetFormatedMessage(mailSetting.EmailTemplate, param);
                    if (param.WID > 0)
                    {
                        if (!string.IsNullOrEmpty(mailSetting.FromEmail))
                        {
                            IEmailML emailManager = new EmailML(_dal);
                            string logo = _resourceML.GetLogoURL(param.WID).ToString();
                            string Footer = "<p><h4 style='color:#000000;font-family:verdana,sans-serif;margin-bottom:1.5px'><em>{CompanyName}</em></h4><span>{CompanyAddress}</span></p>";
                            Footer = Footer.Replace("{CompanyName}", param.Company).Replace("{CompanyAddress}", param.CompanyAddress);
                            IsSent = emailManager.SendEMail(mailSetting, param.UserEmailID, null, mailSetting.Subject, EmailBody, param.WID, logo, true, Footer);
                        }
                    }
                }
                SendEmail sendEmail = new SendEmail
                {
                    From = mailSetting.FromEmail,
                    Body = EmailBody,
                    Recipients = param.UserEmailID,
                    Subject = mailSetting.Subject,
                    IsSent = IsSent,
                    WID = param.WID
                };
                EmailDL emailDL = new EmailDL(_dal);
                emailDL.SaveMail(sendEmail);
            }
            return _res;
        }
        public IResponseStatus RecharegeAcceptNotification(AlertReplacementModel param)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS
            };
            string _Notification = string.Empty;
            string _response = string.Empty;
            string Title = nameof(MessageFormat.RechargeAccept);
            StringBuilder logo = new StringBuilder();
            logo.AppendFormat("{0}.png", MessageFormat.RechargeAccept);
            var Sqlparam = new CommonReq()
            {
                CommonInt = MessageFormat.RechargeAccept
            };
            IProcedure _proc = new ProcGetNotificationFormat(_dal);
            var NotificationDetail = (ResponseStatus)_proc.Call(Sqlparam);
            if (NotificationDetail.CommonBool)
            {
                bool IsNoTemplate = true;
                if (string.IsNullOrEmpty(NotificationDetail.CommonStr))
                {
                    _Notification = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if ((param.UserFCMID ?? string.Empty).Length < 5)
                    {
                        _Notification = "No FCMID Found";
                        IsNoTemplate = false;
                    }
                    _Notification = GetFormatedMessage(NotificationDetail.CommonStr, param);
                    if (param.WID > 0 && IsNoTemplate)
                    {
                        SendSMSML sendSmsMl = new SendSMSML(_accessor, _env);
                        _response = sendSmsMl.SendNotification(param.UserFCMID, _Notification, logo.ToString(), Title, param.URL);
                    }
                }
                var _Response = new Notification
                {
                    LoginID = param.LoginID,
                    Response = _response,
                    Title = Title,
                    ImageUrl = logo.ToString(),
                    Url = param.URL,
                    Message = _Notification,
                    WID = param.WID,
                    UserID = param.UserID,
                    FCMID = param.UserFCMID
                };
                SaveNotificationResponse(_Response);
            }
            return _res;
        }
        #endregion

        #region UserPartialApproval
        public async Task UserPartialApprovalSMS(AlertReplacementModel param)
        {
            await Task.Delay(0).ConfigureAwait(false);
            string sendRes = string.Empty;
            string SMS = string.Empty;
            var Sqlparam = new CommonReq()
            {
                LoginID = param.LoginID,
                CommonInt = MessageFormat.UserPartialApproval,
                CommonInt2 = param.WID,
            };
            IProcedure _proc = new ProcGetSMSSettingByFormat(_dal);
            var smsSetting = (SMSSetting)_proc.Call(Sqlparam);
            if (smsSetting.IsEnableSMS)
            {
                bool IsNoTemplate = true;
                StringBuilder sbUrl = new StringBuilder(smsSetting.URL);
                if (string.IsNullOrEmpty(smsSetting.Template))
                {
                    sendRes = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if (smsSetting.SMSID == 0)
                    {
                        sendRes = "No API Found";
                    }
                    SMS = GetFormatedMessage(smsSetting.Template, param);
                    if (smsSetting.SMSID > 0 && !string.IsNullOrEmpty(smsSetting.URL))
                    {

                        sbUrl.Replace("{SENDERID}", smsSetting.SenderID);
                        sbUrl.Replace("{TO}", param.UserMobileNo);
                        sbUrl.Replace("{MESSAGE}", SMS);
                        var p = new SendSMSRequest()
                        {
                            APIMethod = smsSetting.APIMethod,
                            SmsURL = sbUrl.ToString()
                        };
                        sendRes = _sendSMSML.CallSendSMSAPI(p);
                    }
                }
                var _Response = new SMSResponse
                {
                    ReqURL = sbUrl.ToString(),
                    Response = Convert.ToString(sendRes),
                    ResponseID = "",
                    Status = SMSResponseTYPE.SEND,
                    SMSID = smsSetting.SMSID,
                    MobileNo = param.UserMobileNo,
                    TransactionID = "",
                    SMS = SMS,
                    WID = param.WID
                };
                SaveSMSResponse(_Response);
            }
        }
        public async Task UserPartialApprovalEmail(AlertReplacementModel param)
        {
            await Task.Delay(0).ConfigureAwait(false);
            bool IsSent = false;
            string EmailBody = string.Empty;
            var Sqlparam = new CommonReq()
            {
                LoginID = param.LoginID,
                CommonInt = MessageFormat.UserPartialApproval
            };
            IProcedure _proc = new ProcGetEmailSettingByFormat(_dal);
            var mailSetting = (EmailSettingswithFormat)_proc.Call(Sqlparam);
            if (mailSetting.IsEnableEmail)
            {
                bool IsNoTemplate = true;
                if (string.IsNullOrEmpty(mailSetting.EmailTemplate))
                {
                    EmailBody = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if (string.IsNullOrEmpty(mailSetting.FromEmail))
                    {
                        EmailBody = "No Email Found";
                    }
                    EmailBody = GetFormatedMessage(mailSetting.EmailTemplate, param);
                    if (param.WID > 0)
                    {
                        if (!string.IsNullOrEmpty(mailSetting.FromEmail))
                        {
                            IEmailML emailManager = new EmailML(_dal);
                            string logo = _resourceML.GetLogoURL(param.WID).ToString();
                            string Footer = "<p><h4 style='color:#000000;font-family:verdana,sans-serif;margin-bottom:1.5px'><em>{CompanyName}</em></h4><span>{CompanyAddress}</span></p>";
                            Footer = Footer.Replace("{CompanyName}", param.Company).Replace("{CompanyAddress}", param.CompanyAddress);
                            IsSent = emailManager.SendEMail(mailSetting, param.UserEmailID, null, mailSetting.Subject, EmailBody, param.WID, logo, true, Footer);
                        }
                    }
                }
                SendEmail sendEmail = new SendEmail
                {
                    From = mailSetting.FromEmail,
                    Body = EmailBody,
                    Recipients = param.UserEmailID,
                    Subject = mailSetting.Subject,
                    IsSent = IsSent,
                    WID = param.WID
                };
                EmailDL emailDL = new EmailDL(_dal);
                emailDL.SaveMail(sendEmail);
            }
        }
        #endregion

        #region CallNotPicked
        public IResponseStatus CallNotPickedSMS(AlertReplacementModel param)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS
            };
            string sendRes = string.Empty;
            string SMS = string.Empty;
            var Sqlparam = new CommonReq()
            {
                LoginID = param.LoginID,
                CommonInt = MessageFormat.CallNotPicked
            };
            IProcedure _proc = new ProcGetSMSSettingByFormat(_dal);
            var smsSetting = (SMSSetting)_proc.Call(Sqlparam);

            if (smsSetting.IsEnableSMS)
            {
                try
                {
                    bool IsNoTemplate = true;
                    StringBuilder sbUrl = new StringBuilder(smsSetting.URL);
                    if (string.IsNullOrEmpty(smsSetting.Template))
                    {
                        sendRes = "No Template Found";
                        IsNoTemplate = false;
                    }
                    if (IsNoTemplate)
                    {
                        if (smsSetting.SMSID == 0)
                        {
                            sendRes = "No API Found";
                        }
                        SMS = GetFormatedMessage(smsSetting.Template, param);
                        if (smsSetting.SMSID > 0 && !string.IsNullOrEmpty(smsSetting.URL))
                        {
                            sbUrl.Replace("{SENDERID}", smsSetting.SenderID);
                            sbUrl.Replace("{TO}", param.UserMobileNo);
                            sbUrl.Replace("{MESSAGE}", SMS);
                            var p = new SendSMSRequest()
                            {
                                APIMethod = smsSetting.APIMethod,
                                SmsURL = sbUrl.ToString()
                            };
                            sendRes = _sendSMSML.CallSendSMSAPI(p);
                        }
                    }
                    var _Response = new SMSResponse
                    {
                        ReqURL = sbUrl.ToString(),
                        Response = Convert.ToString(sendRes),
                        ResponseID = "",
                        Status = SMSResponseTYPE.SEND,
                        SMSID = smsSetting.SMSID,
                        MobileNo = param.UserMobileNo,
                        TransactionID = "",
                        SMS = SMS,
                        WID = param.WID
                    };
                    SaveSMSResponse(_Response);
                }
                catch (Exception ex)
                {
                    var errorLog = new ErrorLog
                    {
                        ClassName = GetType().Name,
                        FuncName = "Call",
                        Error = ex.Message,
                        LoginTypeID = 1,
                        UserId = param.LoginID
                    };
                    var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                }
            }
            return _res;
        }
        public IResponseStatus CallNotPickedEmail(AlertReplacementModel param)
        {
            bool IsSent = false;
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS
            };
            string EmailBody = string.Empty;
            var Sqlparam = new CommonReq()
            {
                LoginID = param.LoginID,
                CommonInt = MessageFormat.CallNotPicked
            };
            IProcedure _proc = new ProcGetEmailSettingByFormat(_dal);
            var mailSetting = (EmailSettingswithFormat)_proc.Call(Sqlparam);
            if (mailSetting.IsEnableEmail)
            {
                bool IsNoTemplate = true;
                if (string.IsNullOrEmpty(mailSetting.EmailTemplate))
                {
                    EmailBody = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if (string.IsNullOrEmpty(mailSetting.FromEmail))
                    {
                        EmailBody = "No Email Found";
                    }
                    EmailBody = GetFormatedMessage(mailSetting.EmailTemplate, param);
                    if (param.WID > 0)
                    {
                        if (!string.IsNullOrEmpty(mailSetting.FromEmail))
                        {
                            IEmailML emailManager = new EmailML(_dal);
                            string logo = _resourceML.GetLogoURL(param.WID).ToString();
                            string Footer = "<p><h4 style='color:#000000;font-family:verdana,sans-serif;margin-bottom:1.5px'><em>{CompanyName}</em></h4><span>{CompanyAddress}</span></p>";
                            Footer = Footer.Replace("{CompanyName}", param.Company).Replace("{CompanyAddress}", param.CompanyAddress);
                            IsSent = emailManager.SendEMail(mailSetting, param.UserEmailID, null, mailSetting.Subject, EmailBody, param.WID, logo, true, Footer);
                        }
                    }
                }
                SendEmail sendEmail = new SendEmail
                {
                    From = mailSetting.FromEmail,
                    Body = EmailBody,
                    Recipients = param.UserEmailID,
                    Subject = mailSetting.Subject,
                    IsSent = IsSent,
                    WID = param.WID
                };
                EmailDL emailDL = new EmailDL(_dal);
                emailDL.SaveMail(sendEmail);
            }
            return _res;
        }
        public IResponseStatus CallNotPickedNotification(AlertReplacementModel param)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS
            };
            string _Notification = string.Empty;
            string _response = string.Empty;
            string Title = nameof(MessageFormat.CallNotPicked);
            StringBuilder logo = new StringBuilder();
            logo.AppendFormat("{0}.png", MessageFormat.CallNotPicked);
            var Sqlparam = new CommonReq()
            {
                CommonInt = MessageFormat.CallNotPicked
            };
            IProcedure _proc = new ProcGetNotificationFormat(_dal);
            var NotificationDetail = (ResponseStatus)_proc.Call(Sqlparam);
            if (NotificationDetail.CommonBool)
            {
                bool IsNoTemplate = true;
                if (string.IsNullOrEmpty(NotificationDetail.CommonStr))
                {
                    _Notification = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if ((param.UserFCMID ?? string.Empty).Length < 5)
                    {
                        _Notification = "No FCMID Found";
                        IsNoTemplate = false;
                    }
                    _Notification = GetFormatedMessage(NotificationDetail.CommonStr, param);
                    if (param.WID > 0 && IsNoTemplate)
                    {
                        SendSMSML sendSmsMl = new SendSMSML(_accessor, _env);
                        _response = sendSmsMl.SendNotification(param.UserFCMID, _Notification, logo.ToString(), Title, param.URL);
                    }
                }
                var _Response = new Notification
                {
                    LoginID = param.LoginID,
                    Response = _response,
                    Title = Title,
                    ImageUrl = logo.ToString(),
                    Url = param.URL,
                    Message = _Notification,
                    WID = param.WID,
                    UserID = param.UserID,
                    FCMID = param.UserFCMID
                };
                SaveNotificationResponse(_Response);
            }
            return _res;
        }
        #endregion

        #region MarginRevised
        public async Task MarginRevisedSMS(AlertReplacementModel param)
        {
            await Task.Delay(0).ConfigureAwait(false);
            string sendRes = string.Empty;
            string SMS = string.Empty;
            var Sqlparam = new CommonReq()
            {
                LoginID = param.LoginID,
                CommonInt = MessageFormat.MarginRevised
            };
            IProcedure _proc = new ProcGetSMSSettingByFormat(_dal);
            var smsSetting = (SMSSetting)_proc.Call(Sqlparam);
            if (smsSetting.IsEnableSMS)
            {
                bool IsNoTemplate = true;
                if (string.IsNullOrEmpty(smsSetting.Template))
                {
                    sendRes = "No Template Found";
                    IsNoTemplate = false;
                }
                var UserMobileNos = !string.IsNullOrEmpty(param.UserMobileNo) ? param.UserMobileNo.Split(',') : new string[0];
                foreach (var mobile in UserMobileNos)
                {
                    param.UserMobileNo = mobile;
                    StringBuilder sbUrl = new StringBuilder(smsSetting.URL);
                    if (IsNoTemplate)
                    {
                        if (smsSetting.SMSID == 0)
                        {
                            sendRes = "No API Found";
                        }
                        SMS = GetFormatedMessage(smsSetting.Template, param);
                        if (smsSetting.SMSID > 0 && !string.IsNullOrEmpty(smsSetting.URL))
                        {
                            sbUrl.Replace("{SENDERID}", smsSetting.SenderID);
                            sbUrl.Replace("{TO}", param.UserMobileNo);
                            sbUrl.Replace("{MESSAGE}", SMS);
                            var p = new SendSMSRequest()
                            {
                                APIMethod = smsSetting.APIMethod,
                                SmsURL = sbUrl.ToString()
                            };
                            sendRes = _sendSMSML.CallSendSMSAPI(p);
                        }
                    }
                    var _Response = new SMSResponse
                    {
                        ReqURL = sbUrl.ToString(),
                        Response = Convert.ToString(sendRes),
                        ResponseID = "",
                        Status = SMSResponseTYPE.SEND,
                        SMSID = smsSetting.SMSID,
                        MobileNo = param.UserMobileNo,
                        TransactionID = "",
                        SMS = SMS,
                        WID = param.WID
                    };
                    SaveSMSResponse(_Response);
                }

            }
            //return _res;
        }
        public async Task MarginRevisedEmail(AlertReplacementModel param)
        {
            bool IsSent = false;
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS
            };
            string EmailBody = string.Empty;
            string Subject = string.Empty;
            var Sqlparam = new CommonReq()
            {
                LoginID = param.LoginID,
                CommonInt = MessageFormat.MarginRevised
            };
            IProcedure _proc = new ProcGetEmailSettingByFormat(_dal);
            var mailSetting = (EmailSettingswithFormat)_proc.Call(Sqlparam);
            if (mailSetting.IsEnableEmail)
            {
                bool IsNoTemplate = true;
                if (string.IsNullOrEmpty(mailSetting.EmailTemplate))
                {
                    EmailBody = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if (string.IsNullOrEmpty(mailSetting.FromEmail))
                    {
                        EmailBody = "No Email Found";
                    }
                    EmailBody = GetFormatedMessage(mailSetting.EmailTemplate, param);
                    Subject = mailSetting.Subject.Replace("{OperatorName}", param.Operator);
                    if (param.WID > 0)
                    {
                        List<string> newList = new List<string>();
                        while (param.bccList.Any())
                        {
                            newList = param.bccList.Take(255).ToList();
                            param.bccList = param.bccList.Skip(255).ToList();
                            if (!string.IsNullOrEmpty(mailSetting.FromEmail))
                            {
                                IEmailML emailManager = new EmailML(_dal);
                                IResourceML rml = new ResourceML(_accessor, _env);
                                string logo = _resourceML.GetLogoURL(param.WID).ToString();
                                string Footer = "<p><h4 style='color:#000000;font-family:verdana,sans-serif;margin-bottom:1.5px'><em>{CompanyName}</em></h4><span>{CompanyAddress}</span></p>";
                                Footer = Footer.Replace("{CompanyName}", param.Company).Replace("{CompanyAddress}", param.CompanyAddress);
                                IsSent = emailManager.SendEMail(mailSetting, newList.First(), newList, Subject, EmailBody, param.WID, logo, true, Footer);

                                await Task.Delay(0).ConfigureAwait(false);
                            }

                            SendEmail sendEmail = new SendEmail
                            {
                                From = mailSetting.FromEmail,
                                Body = EmailBody,
                                Recipients = param.UserEmailID + "," + (param.bccList != null ? (param.bccList.Count > 0 ? String.Join(",", param.bccList) : "") : ""),
                                Subject = Subject,
                                IsSent = IsSent,
                                WID = param.WID
                            };
                            EmailDL emailDL = new EmailDL(_dal);
                            emailDL.SaveMail(sendEmail);
                        }
                    }
                }
            }
        }
        public async Task MarginRevisedNotification(AlertReplacementModel param, bool IsBulk)
        {
            await Task.Delay(0).ConfigureAwait(false);
            string _Notification = string.Empty;
            string _response = string.Empty;
            var Sqlparam = new CommonReq()
            {
                CommonInt = MessageFormat.MarginRevised
            };
            IProcedure _proc = new ProcGetNotificationFormat(_dal);
            var NotificationDetail = (ResponseStatus)_proc.Call(Sqlparam);
            if (NotificationDetail.CommonBool)
            {
                string Title = nameof(MessageFormat.MarginRevised);
                StringBuilder logo = new StringBuilder();
                logo.AppendFormat("{0}.png", MessageFormat.MarginRevised);
                bool IsNoTemplate = true;
                if (string.IsNullOrEmpty(NotificationDetail.CommonStr))
                {
                    _Notification = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if (!IsBulk)
                    {
                        if ((param.UserFCMID ?? string.Empty).Length < 5)
                        {
                            _Notification = "No FCMID Found";
                            IsNoTemplate = false;
                        }
                    }
                    else
                    {
                        param.UserFCMID = "-1";
                    }
                    _Notification = GetFormatedMessage(NotificationDetail.CommonStr, param);
                    if (param.WID > 0 && IsNoTemplate)
                    {
                        SendSMSML sendSmsMl = new SendSMSML(_accessor, _env);
                        if (IsBulk)
                        {
                            _response = sendSmsMl.SendNotification(_Notification, logo.ToString(), Title, param.URL);
                        }
                        else
                        {
                            _response = sendSmsMl.SendNotification(param.UserFCMID, _Notification, logo.ToString(), Title, param.URL);
                        }
                    }
                }
                var _Response = new Notification
                {
                    LoginID = param.LoginID,
                    Response = _response,
                    Title = Title,
                    ImageUrl = logo.ToString(),
                    Url = param.URL,
                    Message = _Notification,
                    WID = param.WID,
                    UserID = param.UserID,
                    FCMID = param.UserFCMID
                };
                SaveNotificationResponse(_Response);
            }
        }
        #endregion

        public async Task WebNotification(AlertReplacementModel param)
        {
            try
            {
                if (RoundpayFinTech.AppCode.Configuration.ApplicationSetting.IsWebNotification)
                {
                    var Sqlparam = new CommonReq()
                    {
                        CommonInt = param.FormatID
                    };
                    IProcedure _proc = new ProcGetWebNotificationFormat(_dal);
                    var NotificationDetail = (ResponseStatus)_proc.Call(Sqlparam);
                    if (NotificationDetail.CommonBool)
                    {
                        StringBuilder logo = new StringBuilder();
                        logo.AppendFormat("{0}.png", param.FormatID);
                        string _Notification = GetFormatedMessage(NotificationDetail.CommonStr, param);
                        if (string.IsNullOrEmpty(param.UserIds))
                        {
                            var notification = new WebNotification
                            {
                                LoginID = param.LoginID,
                                UserID = param.UserID,
                                Title = param.NotificationTitle,
                                Notification = string.IsNullOrEmpty(_Notification) ? "" : _Notification,
                                FormatID = param.FormatID,
                                Operator = param.Operator
                            };
                            await SaveWebNotification(notification).ConfigureAwait(false);
                        }
                        else
                        {
                            var UserIds = param.UserIds.Split(',');
                            foreach (var UserId in UserIds)
                            {
                                if (!string.IsNullOrEmpty(UserId) && Convert.ToInt32(UserId) > 0)
                                {
                                    var notification = new WebNotification
                                    {
                                        LoginID = param.LoginID,
                                        UserID = Convert.ToInt32(UserId),
                                        Title = param.NotificationTitle,
                                        Notification = string.IsNullOrEmpty(_Notification) ? "" : _Notification
                                    };
                                    await SaveWebNotification(notification).ConfigureAwait(false);
                                }
                            }
                        }
                    }
                }
            }

            catch (Exception ex)
            {

            }
        }
        public async Task SocialAlert(AlertReplacementModel param)
        {
            try
            {


                IProcedure _p = new ProcGetMasterSocialType(_dal);
                var resp = (ResponseStatus)_p.Call();
                var ActivatedType = resp.CommonStr.Split(",");
                string sendTo = string.Empty;
                for (int i = 0; i < ActivatedType.Count(); i++)
                {
                    switch (int.Parse(ActivatedType[i]))// switch (i + 1)
                    {
                        case 1:
                            sendTo = param.WhatsappNo;
                            break;
                        case 2:
                            sendTo = param.TelegramNo;
                            break;
                        case 3:
                            sendTo = param.HangoutNo;
                            break;
                    }
                    param.SocialAlertType = int.Parse(ActivatedType[i]);
                    var Sqlparam = new CommonReq()
                    {
                        LoginID = param.LoginID,
                        CommonInt = param.FormatID,
                        CommonInt2 = param.SocialAlertType
                    };
                    IProcedure _proc = new ProcGetSocialAlertFormat(_dal);
                    var NotificationDetail = (ResponseStatus)_proc.Call(Sqlparam);
                    if (NotificationDetail.CommonBool)// check if SocialAlert Enable
                    {
                        string formatedMessage = GetFormatedMessage(!string.IsNullOrEmpty(param.Message) ? param.Message : NotificationDetail.CommonStr, param);
                        if (ApplicationSetting.IsWhatsappAgent && !string.IsNullOrEmpty(param.WhatsappConversationID))
                        {
                            IWhatsappML whatsappML = new WhatsappML(_accessor, _env);
                            whatsappML.SendWhatsappSessionMessageAllAPI(new WhatsappConversation { conversationId = param.WhatsappConversationID, Text = formatedMessage });
                        }
                        else
                        {
                            var modal = new SocialMessage
                            {
                                LoginID = param.LoginID,
                                UserName = param.LoginUserName,
                                SMSAPIID = NotificationDetail.CommonInt,
                                Message = formatedMessage,
                                SendTo = ApplicationSetting.IsWhatsappAgent && !string.IsNullOrEmpty(param.WhatsappConversationID) ? string.Empty : sendTo,
                                SenderName = param.SenderName,
                                IsBulk = false,
                                SocialAlertType = param.SocialAlertType,
                                SCANNO = NotificationDetail.CommonStr2 ?? string.Empty,
                                CountryCode = NotificationDetail.CommonStr3 ?? string.Empty,
                                ConversationId = ApplicationSetting.IsWhatsappAgent ? param.WhatsappConversationID : string.Empty,
                                CCID = param.CCID,
                                CCName = param.CCName
                            };
                            await Task.Delay(0).ConfigureAwait(false);
                            var res = SendSocialNotification(modal);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "SocialALert",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = _lr != null ? _lr.UserID : param.LoginID,
                });
            }
        }

        public IResponseStatus UserSubscription(AlertReplacementModel param)
        {
            bool IsSent = false;
            bool response = false;
            bool response1 = false;
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.FAILED
            };
            string EmailBody = string.Empty;
            var Sqlparam = new CommonReq
            {
                LoginID = param.LoginID,
                CommonInt = MessageFormat.UserSubscription,
                CommonInt2 = param.WID
            };
            IProcedure _proc = new ProcGetEmailSettingByFormat(_dal);
            var mailSetting = (EmailSettingswithFormat)_proc.Call(Sqlparam);
            if (mailSetting.IsEnableEmail)
            {
                bool IsNoTemplate = true;
                if (string.IsNullOrEmpty(mailSetting.EmailTemplate))
                {
                    EmailBody = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if (string.IsNullOrEmpty(mailSetting.FromEmail))
                    {
                        EmailBody = "No Email Found";
                    }
                    EmailBody = GetFormatedMessage(mailSetting.EmailTemplate, param);
                    if (param.WID > 0)
                    {
                        if (!string.IsNullOrEmpty(mailSetting.FromEmail))
                        {
                            IEmailML emailManager = new EmailML(_dal);
                            string logo = _resourceML.GetLogoURL(param.WID).ToString();
                            IsSent = emailManager.SendEMail(mailSetting, mailSetting.SaleEmail, null, mailSetting.Subject, EmailBody, param.WID, logo);
                        }
                    }
                }
                try
                {
                    SendEmail sendEmail = new SendEmail
                    {
                        From = mailSetting.FromEmail,
                        Body = EmailBody,
                        Recipients = mailSetting.SaleEmail,
                        Subject = mailSetting.Subject,
                        IsSent = IsSent,
                        UserName = param.UserName,
                        UserMobileNo = param.UserMobileNo,
                        EmailID = param.EmailID,
                        Message = param.Message,
                        RequestMode = param.RequestMode,
                        RequestIP = param.RequestIP,
                        RequestPage = param.RequestPage,
                        WID = param.WID
                    };

                    EmailDL emailDL = new EmailDL(_dal);
                    response = emailDL.SaveMail(sendEmail);
                    response1 = emailDL.SaveContactMail(sendEmail);



                }
                catch (Exception ex)
                {

                }
            }
            var SqlparamThank = new CommonReq
            {
                LoginID = param.LoginID,
                CommonInt = MessageFormat.ThankYou,
                CommonInt2 = param.WID
            };
            mailSetting = (EmailSettingswithFormat)_proc.Call(SqlparamThank);
            if (mailSetting.IsEnableEmail)
            {
                bool IsNoTemplate = true;
                if (string.IsNullOrEmpty(mailSetting.EmailTemplate))
                {
                    EmailBody = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if (string.IsNullOrEmpty(mailSetting.FromEmail))
                    {
                        EmailBody = "No Email Found";
                    }
                    StringBuilder sb = new StringBuilder(mailSetting.EmailTemplate);
                    EmailBody = Convert.ToString(sb);
                    if (param.WID > 0)
                    {
                        if (!string.IsNullOrEmpty(mailSetting.FromEmail))
                        {
                            string Footer = "<p><h4 style='color:#000000;font-family:verdana,sans-serif;margin-bottom:1.5px'><em>{CompanyName}</em></h4><span>{CompanyAddress}</span></p>";
                            Footer = Footer.Replace("{CompanyName}", param.Company).Replace("{CompanyAddress}", param.CompanyAddress);

                            IEmailML emailManager = new EmailML(_dal);
                            IsSent = emailManager.SendEMail(mailSetting, param.EmailID, null, mailSetting.Subject, Convert.ToString(sb), param.WID, "", true, Footer);
                            EmailBody = sb.ToString();
                        }
                    }
                }
                try
                {
                    SendEmail sendEmail = new SendEmail
                    {
                        From = mailSetting.FromEmail,
                        Body = EmailBody,
                        Recipients = param.EmailID,
                        Subject = mailSetting.Subject,
                        IsSent = IsSent,
                        WID = param.WID
                    };

                    EmailDL emailDL = new EmailDL(_dal);
                    emailDL.SaveMail(sendEmail);
                }
                catch (Exception ex)
                {
                    var errorLog = new ErrorLog
                    {
                        ClassName = GetType().Name,
                        FuncName = "UserSubscription",
                        Error = ex.Message,
                        LoginTypeID = 1,
                        UserId = _lr != null ? _lr.UserID : param.LoginID,
                    };
                    var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                }
                if (IsSent == true && response1 == true)
                {
                    _res.Statuscode = ErrorCodes.One;
                    _res.Msg = "Your request has been submitted. Our Sales Person will contact you soon.";
                }

            }
            return _res;
        }

        private void SaveSMSResponse(SMSResponse Response)
        {
            IProcedure _p = new ProcSaveSMSResponse(_dal);
            object _o = _p.Call(Response);
        }

        private void SaveNotificationResponse(Notification Response)
        {
            IProcedure _p = new ProcSendNotification(_dal);
            object _o = _p.Call(Response);
        }

        private async Task SaveWebNotification(WebNotification notification)
        {
            IProcedureAsync _p = new ProcSaveWebNotification(_dal);
            var _o = (WebNotification)await _p.Call(notification).ConfigureAwait(false);
        }

        private ResponseStatus SendSocialNotification(SocialMessage modal)
        {
            var response = new ResponseStatus();
            var SMSResponse = new SMSResponse();
            try
            {
                var req = new CommonReq
                {
                    LoginID = _lr != null ? _lr.UserID : modal.LoginID,
                    LoginTypeID = LoginType.ApplicationUser,
                    CommonInt = modal.SMSAPIID
                };
                IProcedure p = new ProcGetSMSAPI(_dal);
                var apiDetail = (SMSAPIDetail)p.Call(req);
                if (apiDetail != null && apiDetail.APIType != 2)
                {
                    modal.APICode = apiDetail.APICode;
                    if (!modal.IsBulk)
                    {
                        string sendRes = string.Empty;
                        var SMSURL = new StringBuilder(apiDetail.URL);
                        if (!string.IsNullOrEmpty(modal.SendTo))
                        {
                            string SenderID = "";
                            SMSURL.Replace("{SENDERID}", SenderID ?? "");
                            SMSURL.Replace("{TO}", modal.SendTo);
                            SMSURL.Replace("{MESSAGE}", modal.Message);
                            SMSURL.Replace("{SCANNO}", modal.SCANNO ?? string.Empty);
                            SMSURL.Replace("{COUNTRY}", modal.CountryCode ?? string.Empty);

                            var _p = new SendSMSRequest()
                            {
                                APIMethod = apiDetail.APIMethod,
                                SmsURL = SMSURL.ToString(),
                                IsLapu = false
                            };
                            sendRes = _sendSMSML.CallSendSMSAPI(_p);
                        }
                        SMSResponse = new SMSResponse
                        {
                            ReqURL = SMSURL.ToString(),
                            Response = !string.IsNullOrEmpty(sendRes) ? sendRes : APINotFoundResponse(modal.SocialAlertType),
                            ResponseID = "",
                            Status = SMSResponseTYPE.SEND,
                            SMSID = modal.SMSAPIID,
                            MobileNo = !string.IsNullOrEmpty(modal.SendTo) ? modal.SendTo : NumberNotFoundResponse(modal.SocialAlertType),
                            TransactionID = "",
                            SMS = modal.Message,
                            WID = _lr != null ? _lr.WID : 0,
                            SocialAlertType = modal.SocialAlertType
                        };
                        //SaveSMSResponse(SMSResponse);
                    }
                    else
                    {
                        if (modal.Data != null && modal.Data.Rows.Count > 0)
                        {
                            foreach (DataRow dr in modal.Data.Rows)
                            {
                                string SenderID = "";
                                var SMSURL = new StringBuilder(apiDetail.URL);
                                SMSURL.Replace("{SENDERID}", SenderID ?? "");
                                SMSURL.Replace("{TO}", Convert.ToString(dr["SendTo"]));
                                SMSURL.Replace("{MESSAGE}", Convert.ToString(dr["Message"]));
                                var _p = new SendSMSRequest()
                                {
                                    APIMethod = apiDetail.APIMethod,
                                    SmsURL = SMSURL.ToString(),
                                    IsLapu = false
                                };
                                var sendRes = _sendSMSML.CallSendSMSAPI(_p);
                                SMSResponse = new SMSResponse
                                {
                                    ReqURL = SMSURL.ToString(),
                                    Response = Convert.ToString(sendRes),
                                    ResponseID = "",
                                    Status = SMSResponseTYPE.SEND,
                                    SMSID = modal.SMSAPIID,
                                    MobileNo = Convert.ToString(dr["SendTo"]),
                                    TransactionID = "",
                                    SMS = Convert.ToString(dr["Message"]),
                                    WID = _lr != null ? _lr.WID : 0,
                                };
                                //SaveSMSResponse(SMSResponse);
                            }
                        }
                    }
                    SaveSMSResponse(SMSResponse);
                    // save convertation for whatsapp
                    if (modal.SocialAlertType == 1)
                    {
                        SaveWhatsappConvertation(modal);
                    }
                }
                else
                {
                    IProcedure proc = new ProcSendSocialMessage(_dal);
                    response = (ResponseStatus)proc.Call(modal);
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "SendSocialNotification",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = _lr != null ? _lr.UserID : modal.LoginID,
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return response;
        }

        private async Task SaveWhatsappConvertation(SocialMessage modal)
        {
            try
            {
                IWhatsappML whatsappML = new WhatsappML(_accessor, _env, false);
                var wc = new WhatsappConversation()
                {
                    ContactId = modal.SendTo,
                    SenderName = modal.SenderName,
                    Text = modal.Message?.Replace("%0a", "<br/>"),
                    LoginTypeID = 1,
                    CCID = modal.LoginID,
                    CCName = modal.UserName,
                    Type = "text",
                    Id = 0,
                    APICODE = modal.APICode,
                    SenderNo = modal.SCANNO,
                    conversationId = modal.ConversationId,
                    StatusString = "Sent",
                };
                whatsappML.SaveWhatsappConvertation(wc);
                await Task.Delay(0);
            }
            catch (Exception ex)
            {

            }
        }

        private string GetFormatedMessage(string Template, AlertReplacementModel Replacements)
        {
            StringBuilder sb = new StringBuilder(Template);

            sb.Replace("{FromUserName}", Replacements.LoginUserName);
            sb.Replace("{FromUserMobile}", Replacements.LoginMobileNo);
            sb.Replace("{FromUserID}", Replacements.LoginPrefix + Replacements.LoginID.ToString());
            sb.Replace("{ToUserMobile}", Replacements.UserMobileNo);
            sb.Replace("{ToUserID}", Replacements.UserPrefix + Replacements.UserID.ToString());
            sb.Replace("{ToUserName}", Replacements.UserName);
            sb.Replace("{UserName}", Replacements.UserName);
            sb.Replace("{Mobile}", Replacements.UserMobileNo);
            sb.Replace("{UserMobile}", Replacements.UserMobileNo);
            sb.Replace("{Amount}", Convert.ToString(Replacements.Amount));
            sb.Replace("{BalanceAmount}", Convert.ToString(Replacements.BalanceAmount));
            sb.Replace("{UserBalanceAmount}", Convert.ToString(Replacements.UserCurrentBalance));
            sb.Replace("{LoginBalanceAmount}", Convert.ToString(Replacements.LoginCurrentBalance));
            sb.Replace("{Operator}", Replacements.Operator);
            sb.Replace("{OperatorName}", Replacements.Operator);
            sb.Replace("{Company}", Replacements.Company);
            sb.Replace("{CompanyName}", Replacements.Company);
            sb.Replace("{CompanyDomain}", Replacements.CompanyDomain);
            sb.Replace("{CompanyAddress}", Replacements.CompanyAddress);
            sb.Replace("{BrandName}", Replacements.BrandName);
            sb.Replace("{OutletName}", Replacements.OutletName);
            sb.Replace("{SupportNumber}", Replacements.SupportNumber);
            sb.Replace("{SupportEmail}", Replacements.SupportEmail);
            sb.Replace("{AccountNumber}", Replacements.AccountNo);
            sb.Replace("{AccountsContactNo}", Replacements.AccountsContactNo);
            sb.Replace("{AccountEmail}", Replacements.AccountEmail);
            sb.Replace("{OTP}", Replacements.OTP);
            sb.Replace("{LoginID}", !String.IsNullOrEmpty(Replacements.CommonStr) ? Replacements.CommonStr : Replacements.LoginPrefix + Replacements.LoginID.ToString());
            sb.Replace("{Password}", Replacements.Password);
            sb.Replace("{PinPassword}", Replacements.PinPassword);
            sb.Replace("{AccountNo}", Replacements.AccountNo);
            sb.Replace("{LiveID}", Replacements.LiveID);
            sb.Replace("{TID}", Convert.ToString(Replacements.TID));
            sb.Replace("{TransactionID}", Replacements.TransactionID);
            sb.Replace("{BankRequestStatus}", Replacements.RequestStatus);
            sb.Replace("{OutletID}", Replacements.OutletID);
            sb.Replace("{OutletMobile}", Replacements.OutletMobile);
            sb.Replace("{RejectReason}", Replacements.KycRejectReason);
            sb.Replace(MessageTemplateKeywords.Message, Replacements.Message);
            sb.Replace(MessageTemplateKeywords.UserEmail, Replacements.EmailID);
            sb.Replace(MessageTemplateKeywords.SenderName, Replacements.SenderName);
            sb.Replace(MessageTemplateKeywords.TransMode, Replacements.TransMode);
            sb.Replace(MessageTemplateKeywords.UTRorRRN, Replacements.UTRorRRN);
            sb.Replace(MessageTemplateKeywords.IFSC, Replacements.IFSC);
            sb.Replace("{DATETIME}", Replacements.DATETIME);
            sb.Replace("{Duration}", Replacements.Duration);
            sb.Replace("{CouponCode}", Replacements.CouponCode);
            sb.Replace("{CouponQty}", Convert.ToString(Replacements.CouponQty));
            sb.Replace("{CouponValidty}", Convert.ToString(Replacements.CouponValdity));

            //sb.Replace(MessageTemplateKeywords.AccountNumber, Replacements.AccountNumber);
            return Convert.ToString(sb);
        }

        #region BirthdayWishAlert
        public IResponseStatus BirthdayWishSMS(AlertReplacementModel param, SMSSetting smsSetting)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS
            };
            string sendRes = string.Empty;
            string SMS = string.Empty;
            var SMSparam = new CommonReq()
            {
                LoginID = param.LoginID,
                CommonInt2 = param.WID,
                CommonInt = MessageFormat.BirthdayWish
            };
            if (smsSetting == null || (smsSetting != null && smsSetting.WID != param.WID))
            {
                IProcedure _proc = new ProcGetSMSSettingByFormat(_dal);
                smsSetting = (SMSSetting)_proc.Call(SMSparam);
            }
            if (smsSetting.IsEnableSMS)
            {
                bool IsNoTemplate = true;
                if (string.IsNullOrEmpty(smsSetting.Template))
                {
                    sendRes = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if (smsSetting.SMSID == 0)
                    {
                        sendRes = "No API Found";
                    }
                    SMS = GetFormatedMessage(smsSetting.Template, param);
                    if (smsSetting.SMSID > 0 && !string.IsNullOrEmpty(smsSetting.URL))
                    {
                        StringBuilder sbUrl = new StringBuilder(smsSetting.URL);
                        sbUrl.Replace("{SENDERID}", smsSetting.SenderID);
                        sbUrl.Replace("{TO}", param.UserMobileNo);
                        sbUrl.Replace("{MESSAGE}", SMS);
                        var p = new SendSMSRequest()
                        {
                            APIMethod = smsSetting.APIMethod,
                            SmsURL = sbUrl.ToString()
                        };
                        sendRes = _sendSMSML.CallSendSMSAPI(p);
                    }
                }
                var _Response = new SMSResponse
                {
                    Response = Convert.ToString(sendRes),
                    ResponseID = "",
                    Status = SMSResponseTYPE.SEND,
                    SMSID = smsSetting.SMSID,
                    MobileNo = param.UserMobileNo,
                    TransactionID = "",
                    SMS = SMS,
                    WID = param.WID
                };
                SaveSMSResponse(_Response);
            }
            return _res;
        }
        public IResponseStatus BirthdayWishEmail(AlertReplacementModel param, EmailSettingswithFormat mailSetting)
        {
            bool IsSent = false;
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS
            };
            string EmailBody = string.Empty;
            var SMSparam = new CommonReq()
            {
                LoginID = param.LoginID,
                CommonInt = MessageFormat.BirthdayWish,
                CommonInt2 = param.WID
            };
            IProcedure _proc = new ProcGetEmailSettingByFormat(_dal);
            if (mailSetting == null || (mailSetting != null && mailSetting.WID != param.WID))
                mailSetting = (EmailSettingswithFormat)_proc.Call(SMSparam);
            if (mailSetting.IsEnableEmail)
            {
                bool IsNoTemplate = true;
                if (string.IsNullOrEmpty(mailSetting.EmailTemplate))
                {
                    EmailBody = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if (string.IsNullOrEmpty(mailSetting.FromEmail))
                    {
                        EmailBody = "No Email Found";
                    }
                    EmailBody = GetFormatedMessage(mailSetting.EmailTemplate, param);
                    if (param.WID > 0)
                    {
                        if (!string.IsNullOrEmpty(mailSetting.FromEmail))
                        {
                            IEmailML emailManager = new EmailML(_dal);
                            string logo = _resourceML.GetLogoURL(param.WID).ToString();
                            string Footer = "<p><h4 style='color:#000000;font-family:verdana,sans-serif;margin-bottom:1.5px'><em>{CompanyName}</em></h4><span>{CompanyAddress}</span></p>";
                            Footer = Footer.Replace("{CompanyName}", param.Company).Replace("{CompanyAddress}", param.CompanyAddress);
                            IsSent = emailManager.SendEMail(mailSetting, param.UserEmailID, null, mailSetting.Subject, EmailBody, param.WID, logo, true, Footer);
                        }
                    }
                }
                SendEmail sendEmail = new SendEmail
                {
                    From = mailSetting.FromEmail,
                    Body = EmailBody,
                    Recipients = param.UserEmailID,
                    Subject = mailSetting.Subject,
                    IsSent = IsSent,
                    WID = param.WID
                };
                EmailDL emailDL = new EmailDL(_dal);
                emailDL.SaveMail(sendEmail);
            }
            return _res;
        }
        public IResponseStatus BirthdayWishNotification(AlertReplacementModel param)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS
            };
            string _Notification = string.Empty;
            string _response = string.Empty;
            string Title = nameof(MessageFormat.BirthdayWish);
            StringBuilder logo = new StringBuilder();
            logo.AppendFormat("{0}.png", MessageFormat.BirthdayWish);
            var Sqlparam = new CommonReq()
            {
                CommonInt = MessageFormat.BirthdayWish
            };
            IProcedure _proc = new ProcGetNotificationFormat(_dal);
            var NotificationDetail = (ResponseStatus)_proc.Call(Sqlparam);
            if (NotificationDetail.CommonBool)
            {
                bool IsNoTemplate = true;
                if (string.IsNullOrEmpty(NotificationDetail.CommonStr))
                {
                    _Notification = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if ((param.UserFCMID ?? string.Empty).Length < 5)
                    {
                        _Notification = "No FCMID Found";
                        IsNoTemplate = false;
                    }
                    _Notification = GetFormatedMessage(NotificationDetail.CommonStr, param);
                    if (param.WID > 0 && IsNoTemplate)
                    {
                        SendSMSML sendSmsMl = new SendSMSML(_accessor, _env);
                        _response = sendSmsMl.SendNotification(param.UserFCMID, _Notification, logo.ToString(), Title, param.URL);
                    }
                }
                var _Response = new Notification
                {
                    LoginID = param.LoginID,
                    Response = _response,
                    Title = Title,
                    ImageUrl = logo.ToString(),
                    Url = param.UserFCMID,
                    Message = _Notification,
                    WID = param.WID,
                    UserID = param.UserID,
                    FCMID = param.UserFCMID
                };
                SaveNotificationResponse(_Response);
            }
            return _res;
        }
        #endregion

        #region BBPSAlert
        public IResponseStatus BBPSSuccessSMS(AlertReplacementModel param)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS
            };
            string sendRes = string.Empty;
            string SMS = string.Empty;
            var Sqlparam = new CommonReq()
            {
                LoginID = param.LoginID,
                CommonInt = MessageFormat.BBPSSuccess
            };
            IProcedure _proc = new ProcGetSMSSettingByFormat(_dal);
            var smsSetting = (SMSSetting)_proc.Call(Sqlparam);
            if (smsSetting.IsEnableSMS)
            {
                bool IsNoTemplate = true;
                StringBuilder sbUrl = new StringBuilder(smsSetting.URL);
                if (string.IsNullOrEmpty(smsSetting.Template))
                {
                    sendRes = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if (smsSetting.SMSID == 0)
                    {
                        sendRes = "No API Found";
                    }
                    SMS = GetFormatedMessage(smsSetting.Template, param);
                    if (smsSetting.SMSID > 0 && !string.IsNullOrEmpty(smsSetting.URL))
                    {
                        sbUrl.Replace("{SENDERID}", smsSetting.SenderID);
                        sbUrl.Replace("{TO}", param.UserMobileNo);
                        sbUrl.Replace("{MESSAGE}", SMS);
                        var p = new SendSMSRequest()
                        {
                            APIMethod = smsSetting.APIMethod,
                            SmsURL = sbUrl.ToString()
                        };
                        sendRes = _sendSMSML.CallSendSMSAPI(p);
                    }
                }
                var _Response = new SMSResponse
                {
                    ReqURL = sbUrl.ToString(),
                    Response = Convert.ToString(sendRes),
                    ResponseID = "",
                    Status = SMSResponseTYPE.SEND,
                    SMSID = smsSetting.SMSID,
                    MobileNo = param.UserMobileNo,
                    TransactionID = "",
                    SMS = SMS,
                    WID = param.WID
                };
                SaveSMSResponse(_Response);
            }
            return _res;
        }
        public IResponseStatus BBPSComplainRegistrationAlert(AlertReplacementModel param)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS
            };
            string sendRes = string.Empty;
            string SMS = string.Empty;
            var Sqlparam = new CommonReq()
            {
                LoginID = param.LoginID,
                CommonInt = MessageFormat.BBPSComplainRegistration
            };
            IProcedure _proc = new ProcGetSMSSettingByFormat(_dal);
            var smsSetting = (SMSSetting)_proc.Call(Sqlparam);
            if (smsSetting.IsEnableSMS)
            {
                bool IsNoTemplate = true;
                StringBuilder sbUrl = new StringBuilder(smsSetting.URL);
                if (string.IsNullOrEmpty(smsSetting.Template))
                {
                    sendRes = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if (smsSetting.SMSID == 0)
                    {
                        sendRes = "No API Found";
                    }
                    SMS = GetFormatedMessage(smsSetting.Template, param);
                    if (smsSetting.SMSID > 0 && !string.IsNullOrEmpty(smsSetting.URL))
                    {
                        sbUrl.Replace("{SENDERID}", smsSetting.SenderID);
                        sbUrl.Replace("{TO}", param.UserMobileNo);
                        sbUrl.Replace("{MESSAGE}", SMS);
                        var p = new SendSMSRequest()
                        {
                            APIMethod = smsSetting.APIMethod,
                            SmsURL = sbUrl.ToString()
                        };
                        sendRes = _sendSMSML.CallSendSMSAPI(p);
                    }
                }
                var _Response = new SMSResponse
                {
                    ReqURL = sbUrl.ToString(),
                    Response = Convert.ToString(sendRes),
                    ResponseID = "",
                    Status = SMSResponseTYPE.SEND,
                    SMSID = smsSetting.SMSID,
                    MobileNo = param.UserMobileNo,
                    TransactionID = "",
                    SMS = SMS,
                    WID = param.WID
                };
                SaveSMSResponse(_Response);
            }
            return _res;
        }

        #endregion
        public IResponseStatus SendBulkSocialAlert(AlertReplacementModel model, List<string> APIIDs)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Mess " + ErrorCodes.NotSent
            };

            if (model.SocialAlertType == 0)
            {
                res.Msg = "Please select social alert type";
                return res;
            }

            if (string.IsNullOrEmpty(model.Msg))
            {
                res.Msg = "Fill message";
                return res;
            }

            if (APIIDs == null || APIIDs.Count < 1)
            {
                res.Msg = "Select API";
                return res;
            }

            var SocialIDList = model.SocialIDs.Split(",").Where(x => !string.IsNullOrEmpty(x.Trim())).ToArray();
            if (SocialIDList == null || SocialIDList.Count() < 1)
            {
                res.Msg = "Please select Social Ids";
                return res;
            }
            var arrAPI = APIIDs.ToArray();



            model.Msg = GetFormatedMessage(model.Msg, model);
            DataTable dt = new DataTable();
            dt.Columns.Add("SendTo");
            dt.Columns.Add("Message");
            dt.Columns.Add("SocialAlertType");
            dt.Columns.Add("SMSAPIID");
            int _SMSAPIID = 0;
            for (int i = 0; i < SocialIDList.Count(); i++)
            {
                int selectedAPI = 0;

                DataRow dr = dt.NewRow();
                dr["SendTo"] = SocialIDList[i];
                dr["Message"] = model.Msg;
                dr["SocialAlertType"] = model.SocialAlertType;
                dr["SMSAPIID"] = arrAPI[selectedAPI];
                dt.Rows.Add(dr);
                _SMSAPIID = Convert.ToInt32(arrAPI[selectedAPI]);


            }
            var modal = new SocialMessage
            {
                LT = _lr != null ? _lr.LoginTypeID : 0,
                LoginID = _lr != null ? _lr.UserID : 0,
                Data = dt,
                IsBulk = true,
                SMSAPIID = _SMSAPIID
            };
            var response = SendBulkSocialNotification(modal);
            res.Statuscode = response.Statuscode;
            res.Msg = response.Msg;
            return res;
        }
        private ResponseStatus SendBulkSocialNotification(SocialMessage modal)
        {
            var response = new ResponseStatus();
            try
            {
                var req = new CommonReq
                {
                    LoginID = _lr != null ? _lr.UserID : modal.LoginID,
                    LoginTypeID = LoginType.ApplicationUser,
                    CommonInt = modal.SMSAPIID
                };
                IProcedure p = new ProcGetSMSAPI(_dal);
                var apiDetail = (SMSAPIDetail)p.Call(req);
                if (apiDetail != null && apiDetail.APIType != 2)
                {
                    if (!modal.IsBulk)
                    {
                        string sendRes = string.Empty;
                        var SMSURL = new StringBuilder(apiDetail.URL);
                        if (!string.IsNullOrEmpty(modal.SendTo))
                        {
                            string SenderID = "";
                            SMSURL.Replace("{SENDERID}", SenderID ?? "");
                            SMSURL.Replace("{TO}", modal.SendTo);
                            SMSURL.Replace("{MESSAGE}", modal.Message);

                            var _p = new SendSMSRequest()
                            {
                                APIMethod = apiDetail.APIMethod,
                                SmsURL = SMSURL.ToString(),
                                IsLapu = false
                            };
                            sendRes = _sendSMSML.CallSendSMSAPI(_p);
                        }
                        var _Response = new SMSResponse
                        {
                            ReqURL = SMSURL.ToString(),
                            Response = !string.IsNullOrEmpty(sendRes) ? sendRes : "API Not hit because whatsapp number not found",
                            ResponseID = "",
                            Status = SMSResponseTYPE.SEND,
                            SMSID = modal.SMSAPIID,
                            MobileNo = !string.IsNullOrEmpty(modal.SendTo) ? modal.SendTo : "No Whatsapp Number Found",
                            TransactionID = "",
                            SMS = modal.Message,
                            WID = _lr != null ? _lr.WID : 0,
                            SocialAlertType = modal.SocialAlertType,
                        };
                        SaveSMSResponse(_Response);
                    }
                    else
                    {
                        if (modal.Data != null && modal.Data.Rows.Count > 0)
                        {
                            foreach (DataRow dr in modal.Data.Rows)
                            {
                                string SenderID = "";
                                var SMSURL = new StringBuilder(apiDetail.URL);
                                SMSURL.Replace("{SENDERID}", SenderID ?? "");
                                SMSURL.Replace("{TO}", Convert.ToString(dr["SendTo"]));
                                SMSURL.Replace("{MESSAGE}", Convert.ToString(dr["Message"]));
                                var _p = new SendSMSRequest()
                                {
                                    APIMethod = apiDetail.APIMethod,
                                    SmsURL = SMSURL.ToString(),
                                    IsLapu = false
                                };
                                var sendRes = _sendSMSML.CallSendSMSAPI(_p);
                                var _Response = new SMSResponse
                                {
                                    ReqURL = SMSURL.ToString(),
                                    Response = Convert.ToString(sendRes),
                                    ResponseID = "",
                                    Status = SMSResponseTYPE.SEND,
                                    SMSID = modal.SMSAPIID,
                                    MobileNo = Convert.ToString(dr["SendTo"]),
                                    TransactionID = "",
                                    SMS = Convert.ToString(dr["Message"]),
                                    WID = _lr != null ? _lr.WID : 0,
                                    SocialAlertType = modal.SocialAlertType,
                                };
                                SaveSMSResponse(_Response);
                            }
                        }
                    }
                }
                else
                {
                    IProcedure proc = new ProcSendSocialBulkMessage(_dal);
                    response = (ResponseStatus)proc.Call(modal);
                }
            }
            catch (Exception ex)
            {

            }
            return response;
        }

        private string APINotFoundResponse(int SocialType = 0)
        {
            string result = "API Not Found.";
            switch (SocialType)
            {
                case 1:
                    result = "No Whatsapp API Found";
                    break;
                case 2:
                    result = "No Hangout API Found";
                    break;
                case 3:
                    result = "No Telegram API Found";
                    break;
            }
            return result;
        }

        private string NumberNotFoundResponse(int SocialType = 0)
        {
            string result = "No number found.";
            switch (SocialType)
            {
                case 1:
                    result = "No whatsapp number found";
                    break;
                case 2:
                    result = "No hangout number found";
                    break;
                case 3:
                    result = "No telegram number found";
                    break;
            }
            return result;
        }

        public IResponseStatus CouponVocherEmail(AlertReplacementModel param, bool IsSuccess)
        {
            bool IsSent = false;
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS
            };
            string EmailBody = string.Empty;
            var Sqlparam = new CommonReq()
            {
                LoginID = param.LoginID,
                CommonInt = param.FormatID
            };
            IProcedure _proc = new ProcGetEmailSettingByFormat(_dal);
            var mailSetting = (EmailSettingswithFormat)_proc.Call(Sqlparam);
            if (mailSetting.IsEnableEmail)
            {
                bool IsNoTemplate = true;
                if (string.IsNullOrEmpty(mailSetting.EmailTemplate))
                {
                    EmailBody = "No Template Found";
                    IsNoTemplate = false;
                }
                if (IsNoTemplate)
                {
                    if (string.IsNullOrEmpty(mailSetting.FromEmail))
                    {
                        EmailBody = "No Email Found";
                    }
                    EmailBody = GetFormatedMessage(mailSetting.EmailTemplate, param);
                    if (param.WID > 0)
                    {
                        if (!string.IsNullOrEmpty(mailSetting.FromEmail))
                        {
                            IEmailML emailManager = new EmailML(_dal);
                            string logo = _resourceML.GetLogoURL(param.WID).ToString();
                            string Footer = "<p><h4 style='color:#000000;font-family:verdana,sans-serif;margin-bottom:1.5px'><em>{CompanyName}</em></h4><span>{CompanyAddress}</span></p>";
                            Footer = Footer.Replace("{CompanyName}", param.Company).Replace("{CompanyAddress}", param.CompanyAddress);
                            IsSent = emailManager.SendEMail(mailSetting, param.UserEmailID, null, mailSetting.Subject, EmailBody, param.WID, logo, true, Footer);
                        }
                    }
                }
                SendEmail sendEmail = new SendEmail
                {
                    From = mailSetting.FromEmail,
                    Body = EmailBody,
                    Recipients = param.UserEmailID,
                    Subject = mailSetting.Subject,
                    IsSent = IsSent,
                    WID = param.WID
                };
                EmailDL emailDL = new EmailDL(_dal);
                emailDL.SaveMail(sendEmail);
            }
            return _res;
        }

        public IResponseStatus ResendSMS(string SendTo, string msg, AlertReplacementModel param = null)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS
            };
            string sendRes = string.Empty;
            string SMS = string.Empty;
            var Sqlparam = new CommonReq()
            {
                LoginID = _lr.LoginTypeID,
                CommonInt = _lr.WID
            };
            IProcedure _proc = new procGetApiIDforResend(_dal);
            var smsSetting = (SMSAPIDetail)_proc.Call(Sqlparam);
            StringBuilder sbUrl = new StringBuilder(smsSetting.URL);
            if (smsSetting.ID == 0)
            {
                _res.Msg = "No API Found";
                return _res;
            }
            // SMS = GetFormatedMessage(smsSetting.Template, param);
            if (smsSetting.ID > 0 && !string.IsNullOrEmpty(smsSetting.URL))
            {
                sbUrl.Replace("{SENDERID}", "");
                sbUrl.Replace("{TO}", SendTo);
                sbUrl.Replace("{MESSAGE}", msg);
                var p = new SendSMSRequest
                {
                    APIMethod = smsSetting.APIMethod,
                    SmsURL = sbUrl.ToString()
                };
                sendRes = _sendSMSML.CallSendSMSAPI(p);
            }

            var _Response = new SMSResponse
            {
                ReqURL = sbUrl.ToString(),
                Response = Convert.ToString(sendRes),
                ResponseID = "",
                Status = SMSResponseTYPE.RESEND,
                SMSID = smsSetting.ID,
                MobileNo = SendTo,
                TransactionID = "",
                SMS = msg,
                WID = _lr.WID
            };
            SaveSMSResponse(_Response);

            return _res;
        }
    }
}
