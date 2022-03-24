using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fintech.AppCode;
using Fintech.AppCode.Configuration;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.ThirdParty.HotelAPI;
using Newtonsoft.Json;
using Fintech.AppCode.HelperClass;
using Newtonsoft.Json.Linq;
using System.Dynamic;
using System.Xml;
using RoundpayFinTech.AppCode.Model;

namespace RoundpayFinTech.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class HotelController : Controller
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly ISession _session;
        private readonly LoginResponse _lr;
        private readonly ILoginML loginML;

        public HotelController(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            _session = _accessor.HttpContext.Session;
            _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
            loginML = new LoginML(_accessor, _env);
        }

        public IActionResult Index()
        {
            if (loginML.IsInValidSession())
            {
                return RedirectToAction("Index", "Login");
            }
            if ((_lr.RoleID == Role.Retailor_Seller || _lr.RoleID == Role.Customer) && LoginType.ApplicationUser == _lr.LoginTypeID)
                return View();
            return Ok();
        }
        [HttpGet]
        [Route("/Hotel/SearchHotel")]
        [Route("/SearchHotel")]
        public IActionResult SearchHotel()
        {

            if (loginML.IsInValidSession())
            {
                return RedirectToAction("Index", "Login");
            }
            if ((_lr.RoleID == Role.Retailor_Seller || _lr.RoleID == Role.Customer) && LoginType.ApplicationUser == _lr.LoginTypeID)
                return View();
            return Ok();
        }
        [HttpPost]
        public async Task<IActionResult> SearchHotel(string searchJson)
        {
            if (string.IsNullOrEmpty(searchJson)) return View();

            TempData.Clear();
            TempData["searchJson"] = searchJson;
            return RedirectToAction("HotelList");
        }

        [HttpPost]
        public IActionResult bindCity(string KeyWords)
        {
            IHotelDestination _hot = new HotelML(_accessor, _env);
            CommonReq _req = new CommonReq
            {
                CommonInt = 500,
                CommonStr = KeyWords,
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID
            };
            IEnumerable<HotelDestination> list = _hot.Hotels(_req);
            return Json(new { list });
        }

        [HttpGet]
        public IActionResult HotelList()
        {
            if (loginML.IsInValidSession())
            {
                return RedirectToAction("Index", "Login");
            }
            TekTvlSearchingResponse resp = new TekTvlSearchingResponse();
            if (TempData["searchJson"] == null) return RedirectToActionPermanent("SearchHotel");
            else if (TempData["searchJson"] != null)
            {
                var searchJson = TempData["searchJson"].ToString();
                TekTvlHotelSearchRequest objSr = JsonConvert.DeserializeObject<TekTvlHotelSearchRequest>(searchJson);
                IHotelDestination _hot = new HotelML(_accessor, _env);
                resp = _hot.HotelsearchDataUsingApi(objSr).Result;
                resp.HotelSearchResult.NoOfChild = objSr.RoomGuests.Sum(x => x.NoOfChild);
                resp.HotelSearchResult.NoOfPersons = objSr.RoomGuests.Sum(x => x.NoOfAdults);
                resp.searchJson = searchJson;
            }
            if ((_lr.RoleID == Role.Retailor_Seller || _lr.RoleID == Role.Customer) && LoginType.ApplicationUser == _lr.LoginTypeID)
            {
                if (resp.HotelSearchResult == null)
                {
                    return RedirectToAction("SearchHotel");
                }
                return View(resp);
            }
            return Ok();
        }

        [HttpPost]
        [Route("/Get-Hotel-Info")]
        public async Task<IActionResult> GetHotelInfo(TvkHotelInfoRequest request)
        {
            HotelML hotelML = new HotelML(_accessor, _env);
            return Json(await hotelML.GetHotelInfo(request).ConfigureAwait(false));
        }

        [HttpPost]
        [Route("/Get-Hotel-facility")]
        public async Task<IActionResult> GetHotelfacility(string hc)
        {
            HotelML hotelML = new HotelML(_accessor, _env);
            return Json(await hotelML.GetHotelFacility(hc).ConfigureAwait(false));
        }

        [Route("/Hotel-Single")]
        public IActionResult HotelSingle()
        {
            HotelML hotelML = new HotelML(_accessor, _env);
            TekTvlHotelSingleResponse res = new TekTvlHotelSingleResponse();
            if (TempData.ContainsKey("TekTvlSingleHotelInfoApiRequest"))
            {
                var req = JsonConvert.DeserializeObject<TekTvlSingleHotelInfoApiRequest>(TempData.Peek("TekTvlSingleHotelInfoApiRequest").ToString());
                res = hotelML.GetHotelSingleApiInfo(req).Result;
                if (res.HotelInfoResult.Error.ErrorCode != 0)
                {
                    return RedirectToAction("SearchHotel");
                }
                res.HotelInfoResult.HotelDetails.ResultIndex = req.ResultIndex;
                res.HotelInfoResult.HotelDetails.HotelCode = req.HotelCode;
                res.HotelInfoResult.HotelDetails.NoOfRooms = req.NoOfRooms;
                res.HotelInfoResult.HotelDetails.CheckInDate = req.CheckInDate;
                res.HotelInfoResult.HotelDetails.CheckOutDate = req.CheckOutDate;
                res.HotelInfoResult.HotelDetails.NoOfPersons = req.NoOfPersons;
                res.HotelInfoResult.HotelDetails.NoOfChild = req.NoOfChild;
                if (TempData.ContainsKey("searchJson"))
                {
                    res.searchJson = TempData.Peek("searchJson").ToString();
                }
            }
            return View(res);
        }
        [HttpPost]
        [Route("/Hotel-Single")]
        public IActionResult HotelSingle(TekTvlSingleHotelInfoApiRequest req, string searchJson)
        {
          //  if (req.CategoryId == null) return RedirectToActionPermanent("SearchHotel");
            HotelML hotelML = new HotelML(_accessor, _env);
            var res = hotelML.GetHotelSingleApiInfo(req).Result;
            if (res.HotelInfoResult.Error.ErrorCode != 0)
            {
                return RedirectToAction("SearchHotel");
            } 
            res.HotelInfoResult.HotelDetails.ResultIndex = req.ResultIndex;
            res.HotelInfoResult.HotelDetails.HotelCode = req.HotelCode;
            res.HotelInfoResult.HotelDetails.NoOfRooms = req.NoOfRooms;
            res.HotelInfoResult.HotelDetails.CheckInDate = req.CheckInDate;
            res.HotelInfoResult.HotelDetails.CheckOutDate = req.CheckOutDate;
            res.HotelInfoResult.HotelDetails.NoOfPersons = req.NoOfPersons;
            res.HotelInfoResult.HotelDetails.NoOfChild = req.NoOfChild;
            res.CategoryId = req.CategoryId;
            res.searchJson = searchJson;
            TempData["TekTvlSingleHotelInfoApiRequest"] = JsonConvert.SerializeObject(req);
            TempData["searchJson"] = searchJson;
            return View(res);
        }
        [HttpPost]
        [Route("/Get-Hotel-Room")]
        public IActionResult GetHotelRoom(TekTvlSingleHotelRoomReq req)
        {
            HotelML hotelML = new HotelML(_accessor, _env);
            return Json(hotelML.GetHotelSingleRoomApiReq(req).Result);
        }
        [HttpPost]
        [Route("/ProceedBooking")]
        public IActionResult ProceedBooking(TekTvlHotelProceedBookingReq req, [FromForm] string RoomIndexes)
        {
            HotelML hotelML = new HotelML(_accessor, _env);
           
            req.RoomIndexes = RoomIndexes;
        
            return View(req);
        }
        [HttpPost]
        [Route("/Book-Hotel-Room")]
        public IActionResult BookHotelRoom(string req,string SearchJsonData)
        {
            HotelML hotelML = new HotelML(_accessor, _env);
            var _req = JsonConvert.DeserializeObject<TekTvlHotelBookRoomReq>(req);
            var blockreq = JsonConvert.DeserializeObject<HotelRoomBlockReq>(req);
            var SearchData = JsonConvert.DeserializeObject<TekTvlHotelSearchRequest>(SearchJsonData);
            TekTvlHotelProceedBookingReq res =new TekTvlHotelProceedBookingReq();
            res.blockresponse  = hotelML.HotelBlockRoomApiReq(blockreq, _req, SearchData).Result;
            return PartialView("ProceedBooking", res);
        }
        [HttpGet]
        [Route("/Hotel/BookingDetails")]
        [Route("/BookingDetails")]
        public IActionResult GetBookingDetails()
        {
            return View();
        }
        [HttpPost]
        [Route("/BookingDetilasByNo")]
        public IActionResult GetBookingDetails(int bookingno)
        {
            HotelML hotelML = new HotelML(_accessor, _env);
            var req = new TekTvlHotelBookedRoomReqRes();
            {
                req.BookingId = bookingno;
            }
            var roomblock = hotelML.BookRoomDetails(req).Result;
            return View();
        }
        [HttpPost]
        [Route("/HotelDataSync")]
        public IActionResult HotelDataSync()
        {
            HotelML hotelML = new HotelML(_accessor, _env);
            var roomblock = hotelML.SyncHotelDestinationDataUsingApi().Result;
            return Json(roomblock);
        }
        [HttpPost]
        [Route("/HotelDataSyncByHotel")]
        public IActionResult HotelDataSyncByHotel()
        {
            HotelML hotelML = new HotelML(_accessor, _env);
            var roomblock = hotelML.SyncHotelDataByHotelCode().Result;
            return Json(roomblock);
        }
    }
}
