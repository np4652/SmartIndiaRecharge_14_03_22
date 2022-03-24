using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using RoundpayFinTech.AppCode;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Report;
using RoundpayFinTech.AppCode.Model.Reports.Filter;
using RoundpayFinTech.AppCode.ThirdParty.HotelAPI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.Controllers
{
    public partial class ReportController
    {
        [HttpPost]
        [Route("Home/Hotel-Report")]
        [Route("Hotel-Report")]
        public async Task<IActionResult> HotelReport([FromBody] RechargeReportFilter filter)
        {
            IUserML userML = new UserML(_lr);
            ReportML ml = new ReportML(_accessor, _env);
            var model = new HotelReport();
            model.BookingDetails = await ml.GetHotelReport(filter).ConfigureAwait(false);
            return PartialView("PartialView/_HotelReport", model);
        }
        [HttpPost]
        [Route("Home/Hotel-summary")]
        [Route("Hotel-summary")]
        public async Task<IActionResult> _HotelReportSummary(int s)
        {
            IRechargeReportML ml = new ReportML(_accessor, _env);
            RechargeReportSummary _transactionSummary = await ml.GetHotelSummary(s).ConfigureAwait(false);
            return Json(_transactionSummary);
        }
        #region HotelReport
        [HttpGet]
        [Route("Home/HotelReport")]
        [Route("HotelReport")]
        public async Task<IActionResult> HotelReport()
        {
            HotelReport hr = new HotelReport();
            return View(hr);
        }
        [HttpPost]
        [Route("LoaderBannerImageUpload")]
        public IActionResult LoaderBannerImageUpload(IFormFile file, int opType)
        {
            IBannerML bannerML = new ResourceML(_accessor, _env);
            var _res = bannerML.LoaderBannerUpload(file, _lr);
            return Json(_res);
        }
        [HttpPost]
        [Route("HotelSearchImageUpload")]
        public IActionResult HotelSearchImageUpload(IFormFile file, int opType)
        {
            IBannerML bannerML = new ResourceML(_accessor, _env);
            var _res = bannerML.SaveHotelImage(file, _lr);
            return Json(_res);
        }
        [HttpPost]
        [Route("HotelLoaderBannerMaster")]
        public IActionResult HotelLoaderBannerMaster()
        {

            IBannerML bannerML = new ResourceML(_accessor, _env);
            var resp = bannerML.GetHotelLoaderBanner(_lr.WID.ToString());
            return PartialView("PartialView/_HotelLoaderBanner", resp);
        }
        [Route("RemHotelLoaderBanner")]
        public IActionResult RemoveHotelLoaderBanner(string id)
        {
            IBannerML bannerML = new ResourceML(_accessor, _env);
            var _res = bannerML.RemHotelLoaderBanner(id, _lr.WID.ToString(), _lr);
            return Json(_res);
        }
        [HttpPost]
        [Route("HotelCancelBooking")]
        public IActionResult HotelCancelBooking(int bookingid, int TID, string RPID, string remark)
        {
            HotelML ml = new HotelML(_accessor, _env);
            return Json(ml.HotelCancelAPiReq(bookingid, TID, RPID, remark));
        }
        [HttpPost]
        [Route("Home/Hotel-Receipt")]
        [Route("Hotel-Receipt")]
        public async Task<IActionResult> HotelReceipt(int bookingid, bool IsInvoice, int TID)
        {
            ReportML ml = new ReportML(_accessor, _env);
            var hr = await ml.HotelReceipt(bookingid, TID).ConfigureAwait(false);
            if (IsInvoice)
            {
                return PartialView("PartialView/_HotelReceipt", hr);
            }
            else
            {
                return PartialView("PartialView/_HotelReceiptPrint", hr);
            }
            
        }
        [HttpGet]
        [Route("HotelReceipt")]
        public IActionResult HotelReceipt()
        {
            return View();
        }

        #endregion
    }
}
