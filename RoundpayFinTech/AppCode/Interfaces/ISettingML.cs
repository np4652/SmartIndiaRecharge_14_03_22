using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface ISettingML
    {
        IResponseStatus SaveSystemSetting(SystemSetting setting);
        SystemSetting GetSettings();
        SystemSetting GetSettingsForApp();
        bool UpdateSignupSlab(int SlabID);
        IResponseStatus UpdateAddMoneyCharge(int OID, decimal Charge, bool Is);
        ResponseStatus UpdateReferralSetting(bool r, bool u);
        ReferralSetting GetReferralSetting();
        IEnumerable<RoleMaster> GetRoleForReferral(int _userID);
    }
}
