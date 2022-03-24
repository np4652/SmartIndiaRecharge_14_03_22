using Microsoft.AspNetCore.Mvc;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;

namespace RoundpayFinTech.Controllers
{
    public partial class AdminController
    {
        public IActionResult AdvertisementSetting()
        {
            return View();
        }
        [HttpGet]
        [Route("AdvertisementRequest")]
        [Route("Admin/AdvertisementRequest")]
        public IActionResult AdvertisementRequest(AdvertisementRequest advertisementrequest)
        {
            IAdvertisementML ret = new AdvertisementML(_accessor, _env);
            var res = ret.GetAdvertisement(advertisementrequest);

            return View(res);
        }


        [HttpPost]
        [Route("_AdvertisementRequest")]
        [Route("Admin/_AdvertisementRequest")]
        public IActionResult _AdvertisementRequest([FromBody] AdvertisementRequest advertisementrequest)
        {
            IAdvertisementML ret = new AdvertisementML(_accessor, _env);
            var res = ret.GetAdvertisement(advertisementrequest);
            return PartialView("Partial/_AdvertisementRequest", res);
        }

        [HttpPost]
        [Route("ApprovedAdvertisement")]
        public IActionResult ApprovedAdvertisement([FromBody] AdvertisementRequest UserData)
        {
            IAdvertisementML ret = new AdvertisementML(_accessor, _env);
            var res = ret.ApprovedAdvertisement(UserData);


            return Json(res);
        }

    }
}
