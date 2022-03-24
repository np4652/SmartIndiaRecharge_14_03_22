using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ShopingWebSite;
using RoundpayFinTech.AppCode.Model.Shopping.WebShopping;
using RoundpayFinTech.AppCode.Model.Shopping.WebShopping.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface IShopingWebsiteML
    {
        ResponseStatus SwitchDashboard(CommonReq _req);


    }
}
