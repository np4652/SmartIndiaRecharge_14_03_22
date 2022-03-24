using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model;


namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface ISettingsWithTemplateML
    {
        SMSSetting getSMSSettingsWithFormat(CommonReq req);
        EmailSettingswithFormat getEmailSettingsWithFormat(CommonReq req);
    }
}
