using Fintech.AppCode.DB;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Net.Mail;
using System.Text;


namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public class EmailML : IEmailML
    {
        private readonly IDAL _dal;
        public EmailML(IDAL dal)
        {
            _dal = dal;
        }
        public EmailSetting GetSetting(int WID, int RoleId = 0)
        {
            EmailDL _emailDL = new EmailDL(_dal);
            DataTable dt = _emailDL.GetEmailSetting(WID, RoleId);
            EmailSetting setting = new EmailSetting();
            if (dt.Rows.Count > 0)
            {
                setting.HostName = dt.Rows[0]["HostName"].ToString();
                setting.Password = dt.Rows[0]["Password"].ToString();
                setting.Port = Convert.ToInt32(dt.Rows[0]["Port"]);
                setting.FromEmail = dt.Rows[0]["FromEmail"].ToString();
                setting.MailUserID = Convert.ToString(dt.Rows[0]["MailUserID"]);
                setting.IsActive = Convert.ToBoolean(dt.Rows[0]["IsActive"]);
                setting.IsSSL = dt.Rows[0]["IsSSL"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["IsSSL"]);
            }
            return setting;
        }

        private StringBuilder GetTemplate(String Logo)
        {
            StringBuilder HtmlTemplate = new StringBuilder(ErrorCodes.HTMLTEMPLATE);
            HtmlTemplate.Replace("{logo}", Logo);
            return HtmlTemplate;
        }
        public bool SendMailDefault(string ToEmail, List<string> bccList, string Subject, string Body, int WID, string Logo, bool IsHTML = true, string MailFooter = "", int RoleId = 0)
        {
            bool IsSent = false;
            EmailSetting setting = new EmailSetting();
            setting = GetSetting(WID, RoleId);
            if (setting.FromEmail != null && setting.Port != 0 && setting.Password != null && setting.HostName != null)
            {
                if (IsHTML)
                {
                    Body = GetTemplate(Logo).Replace("{BODY}", Body).Replace("{Footer}", MailFooter).ToString();
                }
                try
                {
                    MailMessage mailMessage = new MailMessage
                    {
                        From = new MailAddress(setting.FromEmail),
                        Subject = Subject,
                        Body = Body,
                        IsBodyHtml = IsHTML
                    };
                    ToEmail = string.IsNullOrEmpty(ToEmail) ? setting.FromEmail : ToEmail;
                    mailMessage.To.Add(ToEmail);
                    if (bccList != null)
                    {
                        foreach (string bcc in bccList)
                        {
                            if (bcc.Contains("@") && bcc.Contains(".") && bcc.Length <= 255)
                            {
                                mailMessage.Bcc.Add(bcc.ToLower());
                            }
                        }
                    }
                    SmtpClient smtpClient = new SmtpClient(setting.HostName, setting.Port)
                    {
                        Credentials = new NetworkCredential(string.IsNullOrEmpty(setting.MailUserID) ? setting.FromEmail : setting.MailUserID, setting.Password)
                    };
                    if (setting.IsSSL)
                    {
                        smtpClient.EnableSsl = setting.IsSSL;
                    }
                    try
                    {
                        smtpClient.Send(mailMessage);
                        IsSent = true;
                    }
                    catch (Exception ex)
                    {
                    }

                }
                catch (Exception ex)
                {

                }
            }

            try
            {

                SendEmail sendEmail = new SendEmail
                {
                    From = setting.FromEmail,
                    Body = Body,
                    Recipients = ToEmail + "," + (bccList != null ? (bccList.Count > 0 ? String.Join(",", bccList) : "") : ""),
                    Subject = Subject,
                    IsSent = IsSent,
                    WID = WID
                };
                EmailDL emailDL = new EmailDL(_dal);
                emailDL.SaveMail(sendEmail);
            }
            catch (Exception)
            {

            }
            return IsSent;
        }
        public bool SendMail(string ToEmail, List<string> bccList, string Subject, string Body, int WID, string Logo, bool IsHTML = true, string MailFooter = "")
        {
            bool IsSent = false;
            EmailSetting setting = new EmailSetting();
            if (WID > 0)
            {
                setting = GetSetting(WID);

                if (setting.FromEmail != null && setting.Port != 0 && setting.Password != null && setting.HostName != null)
                {
                    if (IsHTML)
                    {
                        Body = GetTemplate(Logo).Replace("{BODY}", Body).Replace("{Footer}", MailFooter).ToString();
                    }
                    try
                    {
                        MailMessage mailMessage = new MailMessage
                        {
                            From = new MailAddress(setting.FromEmail),
                            Subject = Subject,
                            Body = Body,
                            IsBodyHtml = IsHTML
                        };
                        ToEmail = string.IsNullOrEmpty(ToEmail) ? setting.FromEmail : ToEmail;
                        mailMessage.To.Add(ToEmail);
                        if (bccList != null)
                        {
                            foreach (string bcc in bccList)
                            {
                                if (bcc.Contains("@") && bcc.Contains(".") && bcc.Length <= 255)
                                {
                                    mailMessage.Bcc.Add(bcc.ToLower());
                                }
                            }
                        }
                        SmtpClient smtpClient = new SmtpClient(setting.HostName, setting.Port)
                        {
                            Credentials = new NetworkCredential(string.IsNullOrEmpty(setting.MailUserID) ? setting.FromEmail : setting.MailUserID, setting.Password)
                        };
                        if (setting.IsSSL)
                        {
                            smtpClient.EnableSsl = setting.IsSSL;
                        }
                        try
                        {
                            smtpClient.Send(mailMessage);
                            IsSent = true;
                        }
                        catch (Exception ex)
                        {
                        }

                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
            try
            {
                SendEmail sendEmail = new SendEmail
                {
                    From = setting.FromEmail,
                    Body = Body,
                    Recipients = ToEmail + "," + (bccList != null ? (bccList.Count > 0 ? String.Join(",", bccList) : "") : ""),
                    Subject = Subject,
                    IsSent = IsSent,
                    WID = WID
                };
                EmailDL emailDL = new EmailDL(_dal);
                emailDL.SaveMail(sendEmail);
            }
            catch (Exception)
            {

            }
            return IsSent;
        }
        public bool SendEMail(EmailSettingswithFormat setting, string ToEmail, List<string> bccList, string Subject, string Body, int WID, string Logo, bool IsHTML = true, string MailFooter = "")
        {
            bool IsSent = false;
            if (setting.FromEmail != null && setting.Port != 0 && setting.Password != null && setting.HostName != null)
            {
                if (IsHTML)
                {
                    Body = GetTemplate(Logo).Replace("{BODY}", Body).Replace("{Footer}", MailFooter).ToString();
                }
                try
                {
                    MailMessage mailMessage = new MailMessage
                    {
                        From = new MailAddress(setting.FromEmail),
                        Subject = Subject,
                        Body = Body,
                        IsBodyHtml = IsHTML
                    };
                    ToEmail = string.IsNullOrEmpty(ToEmail) ? setting.FromEmail : ToEmail;
                    mailMessage.To.Add(ToEmail);
                    if (bccList != null)
                    {
                        foreach (string bcc in bccList)
                        {
                            if (bcc.Contains("@") && bcc.Contains(".") && bcc.Length <= 255)
                            {
                                mailMessage.Bcc.Add(bcc.ToLower());
                            }
                        }
                    }
                    SmtpClient smtpClient = new SmtpClient(setting.HostName, setting.Port)
                    {
                        Credentials = new NetworkCredential(string.IsNullOrEmpty(setting.MailUserID) ? setting.FromEmail : setting.MailUserID, setting.Password)
                    };
                    if (setting.IsSSL)
                    {
                        smtpClient.EnableSsl = setting.IsSSL;
                    }
                    try
                    {
                        smtpClient.Send(mailMessage);
                        IsSent = true;
                    }
                    catch (Exception ex)
                    {

                    }
                }
                catch (Exception ex)
                {

                }
            }
            return IsSent;
        }
    }
}
