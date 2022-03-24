using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Mvc;
using RoundpayFinTech.AppCode.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class DynamicWebsite : Controller
    {
        public IActionResult Index()
        {
            if (ApplicationSetting.DynamicWebsiteType == DynamicWebsiteType.Shopping)
            {
                return RedirectToAction("Index", "ShoppingWebsite");

            }
            else if (ApplicationSetting.DynamicWebsiteType == DynamicWebsiteType.Travel)
            {
                return RedirectToAction("Index", "TravelWebiste");
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

            //Dynamic controller will descide where to redirect(TravelWebsite or shoppingwebsite)
            //Other wise self used as a corporate website controller
            return View();
        }
    }
}
