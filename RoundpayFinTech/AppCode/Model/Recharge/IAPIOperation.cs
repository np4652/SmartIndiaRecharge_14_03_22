using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public interface IAPIOperation
    {
        ResponseStatus SaveRechargeAPI(RechargeAPIHit rechargeAPI);
        RechargeAPIHit GetRechargeAPI(int ID);
        List<RechargeAPIHit> GetRechargeAPI();
        ResponseStatus ToggleAPIStatus(int ID);
        List<RechargeAPIHit> GetAllAPIWithoutSMS();
        APISTATUSCHECK GetAPISTATUSCHECK(APISTATUSCHECK apistatuscheck);
        ResponseStatus UpdateAPISTATUSCHECK(APISTATUSCHECK apistatuscheck);
    }
}
