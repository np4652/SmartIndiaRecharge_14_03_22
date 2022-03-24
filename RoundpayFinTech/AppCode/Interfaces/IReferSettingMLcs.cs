using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Interfaces
{
    interface IReferSettingML
    {
        IEnumerable<Master_Topup_Commission> GetMasterTopupCommission();
        IEnumerable<Master_Role> GetMasterRole();
        IResponseStatus UpdateMaster_Topup_Commission(Master_Topup_Commission para);
        IResponseStatus UpdateMaster_Role(Master_Role para);
        IEnumerable<ReferralCommission> ReferralCommissions();
        IResponseStatus ReferralCommissionsUpdate(ReferralCommission data);
    }
}
