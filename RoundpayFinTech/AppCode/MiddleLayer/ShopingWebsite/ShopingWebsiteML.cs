using Fintech.AppCode.StaticModel;
using Fintech.AppCode.WebRequest;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.Shopping.WebShopping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RoundpayFinTech.AppCode.Model.ShopingWebSite;
using RoundpayFinTech.AppCode.Model.Shopping.WebShopping.ViewModel;
using RoundpayFinTech.AppCode.DL;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.DB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Fintech.AppCode.Configuration;

namespace RoundpayFinTech.AppCode.MiddleLayer.ShopingWebsite
{

    public class ShopingWebsiteML : IShopingWebsiteML
    {
        protected readonly IDAL _dal;
        protected readonly IHttpContextAccessor _accessor;
        protected readonly IHostingEnvironment _env;
        protected readonly IConnectionConfiguration _c;
        public ShopingWebsiteML(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());

        }
        public ResponseStatus SwitchDashboard(CommonReq _req)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.FAILED
            };
            IProcedure proc = new ProcUpdateUserLaunchPreferences(_dal);
            res = (ResponseStatus)proc.Call(_req);
            return res;
        }



    }
}
