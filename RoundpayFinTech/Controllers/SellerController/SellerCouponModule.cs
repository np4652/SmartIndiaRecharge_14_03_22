using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using RoundpayFinTech.AppCode;
using RoundpayFinTech.AppCode.HelperClass;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Report;
using RoundpayFinTech.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.Controllers
{
    public partial class SellerController
    {
        [HttpPost]
        [Route("SellerCouponVoucher/{OPID}")]
        public IActionResult _CouponVoucher(int OPID)
        {
         CoupanReq res = new CoupanReq();
         ISellerCouponModuleML ml = new SellerCouponModuleML(_accessor, _env);
         res.CoupanDetail = ml.GetCouponVoucherList(OPID);
            return PartialView("Partial/Recharge/_CouponVoucher", res);
        }
        [HttpPost]
      
        [Route("SaveCouponVoucher")]
        public IActionResult SaveCouponVoucher([FromBody] CouponData data)
        {
            IUserML userML = new UserML(_lr);
          
            if (_lr.RoleID == Role.Retailor_Seller && LoginType.ApplicationUser == _lr.LoginTypeID)
            {
                ISellerCouponModuleML ml = new SellerCouponModuleML(_accessor, _env);
               
                return Json(ml.SaveCouponVocher(data));
            }
            return Ok();
        }

        [HttpPost]
        [Route("SloadCouponVoucher/{OPID}")]
        public IActionResult __CouponVoucher(int OPID)
        {
            CoupanReq res = new CoupanReq();
            ISellerCouponModuleML ml = new SellerCouponModuleML(_accessor, _env);
            res.CoupanDetail = ml.GetCouponVoucherList(OPID);

            return PartialView("Partial/Recharge/_CouponVoucherImage", res);
        }
    }
}
