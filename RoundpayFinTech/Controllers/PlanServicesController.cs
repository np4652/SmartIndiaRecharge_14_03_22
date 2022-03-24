using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.Controllers
{
    [Authorize]
    [ApiController]
    [Route("PlanServices/v1/")]
    public class PlanServicesController : Controller
    {
        

        public PlanServicesController()
        {

        }
        [HttpGet]
        [Route("BestOffer")]
        public IActionResult Roffer()
        {
            var userReq = HttpContext.Items["User"];
            return Ok(userReq);
        }
        [HttpGet]
        [Route("/RechargePlan")]
        public IActionResult RechargePlan()
        {

            return Ok();
        }
        [HttpGet]
        [Route("/DTHPlan")]
        public IActionResult DTHPlan()
        {

            return Ok();
        }
        [HttpGet]
        [Route("/DTHCustomerInfo")]
        public IActionResult DTHCustomerInfo()
        {

            return Ok();
        }
        [HttpGet]
        [Route("/DTHHeavyRefresh")]
        public IActionResult DTHHeavyRefresh()
        {

            return Ok();
        }
        [HttpGet]
        [Route("/MobileLookup")]
        public IActionResult MobileLookup()
        {

            return Ok();
        }
    }
}
