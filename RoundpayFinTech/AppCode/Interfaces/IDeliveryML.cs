using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.App;
using RoundpayFinTech.AppCode.Model.Shopping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Interfaces
{
    interface IDeliveryML
    {
        #region DeliveryPersonnel
        DeliveryPersonnelList GetDeliveryPersonnelList(bool ActiveOnly = true);
        AUDeliverPersonnel GetDeliveryPersonnelById(int id);
        ResponseStatus AUDeliveryPersonnel(AUDeliverPersonnel req);
        ResponseStatus DeliveryPersonnelStatus(AUDeliverPersonnel req);
        ResponseStatus UpdateDeliveryPersonnelStatusLocation(int id, int status, string latitude, string longitude, int userid = 0, int OrderDetailId = 0);
        LoginDeliveryPersonnel LoginDeliveryPersonnel(LoginDeliveryPersonnelReq req);
        ResponseStatus UpdateDeliveryPersonnelToken(DPToken dPToken);
        LoginDeliveryPersonnelResp ValidateLoginDeliveryPersonnel(LoginDetail req);
        List<OrderDeliveryResp> GetOrderDeliveryList(int OrderDetailId, int UserId = 0);
        OrderDeliveryResp GetOrderDetailForDelivery(int LoginId, int OrderDetailId, int UserId = 0);
        DeliveryDashboard GetDeliveryDashboard(int LoginId, int LT);
        DPLocationList GetDPLocationHistory(int id);
        #endregion
    }
}
