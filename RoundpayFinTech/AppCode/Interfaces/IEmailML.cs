using RoundpayFinTech.AppCode.Model;
using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface IEmailML
    {
        bool SendMail(string ToEmail, List<string> bccList, string Subject, string Body, int WID, string Logo, bool IsHTML = true, string mailFooter = "");
        bool SendEMail(EmailSettingswithFormat setting, string ToEmail, List<string> bccList, string Subject, string Body, int WID, string Logo, bool IsHTML = true, string mailFooter = "");
        bool SendMailDefault(string ToEmail, List<string> bccList, string Subject, string Body, int WID, string Logo, bool IsHTML = true, string MailFooter = "", int RoleId = 0);
    }
}
