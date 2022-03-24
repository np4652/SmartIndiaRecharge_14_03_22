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
    public partial class AdminController
    {
        [HttpGet]
        [Route("Home/CoupanMaster")]
        [Route("CoupanMaster")]
        public IActionResult CoupanMaster()
        {
            return View();
        }

        [HttpPost]
        [Route("Home/Coupan-Master")]
        [Route("Coupan-Master")]
        public IActionResult _CoupanMaster()
        {
            IUserML userML = new UserML(_lr);
            var coupanvoucher = new CoupanMaster
            {
                IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser 
                
            };
            if ((coupanvoucher.IsAdmin || !_lr.IsAdminDefined))
            {
                ICoupanVoucherML coupanVoucherML = new CoupanVoucherML(_accessor, _env);
              
                var res = new CoupanReq();
                res.CoupanDetail= coupanVoucherML.GetCouponMaster();
               
                return PartialView("Partial/_CouponMasterList", res);
            }
            return Ok();
        }


        [HttpPost]
        [Route("Home/CoupanEdit/{id}")]
        [Route("CoupanEdit/{id}")]
        public IActionResult _CouponEdit(int ID)
        {
            IUserML userML = new UserML(_lr);
            var coupanvoucher = new CoupanMaster
            {
                IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser

            };
           
            if ((coupanvoucher.IsAdmin || !_lr.IsAdminDefined) && !userML.IsEndUser())
            {
                ICoupanVoucherML coupanVoucherML = new CoupanVoucherML(_accessor, _env);
                coupanvoucher = coupanVoucherML.GetCouponMaster(ID);
                 var OpML = new OperatorML(_accessor, _env);
                coupanvoucher.OpDetail = OpML.GetOperators(OPTypes.CouponVoucher);
                coupanvoucher.VocImage = coupanVoucherML.GetCouponvocherImage(coupanvoucher.OID);

                return PartialView("Partial/_CouponMasterEdit", coupanvoucher);
            }
            return Ok();
        }


        [HttpPost]
        [Route("Home/Coupon-Edit")]
        [Route("Coupon-Edit")]
        public IActionResult CouponUpdate([FromBody] CoupanMaster coupanMaster)
        {
            IUserML userML = new UserML(_lr);
            ViewBag.IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.AddEditSLAB);
            if ((ViewBag.IsAdmin || !_lr.IsAdminDefined) && !userML.IsEndUser())
            {
                ICoupanVoucherML coupanVoucherML = new CoupanVoucherML(_accessor, _env);
                var resp = coupanVoucherML.UpdateCouponMaster(coupanMaster);
                return Json(resp);
            }
            return Ok();
        }
        [HttpPost]
        
        [Route("CoupanVoucher/{id}")]
        public IActionResult CoupanVoucher(int id)
        {
            ViewBag.VoucherID = id;
     
            var res = new APIML(_accessor, _env).GetVoucherApi();
            return PartialView("Partial/_CouponVoucher", res);
        }
        
        [HttpPost]
        [Route("CouponVoucherList/{id}")]
        public IActionResult CoupanVoucherList(int id)
        {
            ViewBag.VoucherID = id;
            IUserML userML = new UserML(_lr);
            var coupanvoucher = new CoupanMaster
            {
                IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser

            };
            if ((coupanvoucher.IsAdmin || !_lr.IsAdminDefined))
            {
                ICoupanVoucherML coupanVoucherML = new CoupanVoucherML(_accessor, _env);
                CoupanVoucherReq res = new CoupanVoucherReq();
                res.CoupanVoucher = coupanVoucherML.GetCouponVoucher(id);
                ViewBag.TotalACoupn = res.CoupanVoucher.Where(x => x.IsSale == false).Count();
                ViewBag.TotalAmount = res.CoupanVoucher.Where(x => x.IsSale == false).ToList().Sum(a => a.Amount);
                return PartialView("Partial/_CouponVoucherList", res);
            }
            return Ok();
            
        }

        [HttpPost]
        [Route("Home/CouponVoucher-Edit")]
        [Route("CouponVoucher-Edit")]
        public IActionResult CouponVoucherUpdate([FromBody] CoupanVoucher couponvouchert)
        {
            IUserML userML = new UserML(_lr);
            ViewBag.IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.AddEditSLAB);
            if ((ViewBag.IsAdmin || !_lr.IsAdminDefined) && !userML.IsEndUser())
            {
                ICoupanVoucherML coupanVoucherML = new CoupanVoucherML(_accessor, _env);
                var resp = coupanVoucherML.UpdateCouponVoucher(couponvouchert);
                return Json(resp);
            }
            return Ok();
        }


        [Route("CoupanSetting/{id}/{name}")]
        public IActionResult CoupanSetting(int id,string name)
        {
            ViewBag.VoucherID = id;
            ViewBag.VoucherType = name;
            CouponsettingReq resp = new CouponsettingReq();
              ICoupanVoucherML coupanVoucherML = new CoupanVoucherML(_accessor, _env);
               resp.denominationsoucher = coupanVoucherML.GetDenominationVoucher();
               resp.couponsetting      = coupanVoucherML.GetCouponSetting(id);
            return PartialView("Partial/_CouponSetting",resp);
        }


        [HttpPost]
        [Route("Home/CoupanSetting-Edit")]
        [Route("CoupanSetting-Edit")]
        public IActionResult CoupanSettingEdit([FromBody] DenominationVoucher para)
        {
            IUserML userML = new UserML(_lr);
            ViewBag.IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.AddEditSLAB);
            if ((ViewBag.IsAdmin || !_lr.IsAdminDefined) && !userML.IsEndUser())
            {
                ICoupanVoucherML coupanVoucherML = new CoupanVoucherML(_accessor, _env);
                var resp = coupanVoucherML.UpdateCouponSetting(para);
                return Json(resp);
            }
            return Ok();
        }
        [HttpPost]
        [Route("Admin/Toggle-CouponMaster-Status")]
        [Route("Toggle-CouponMaster-Status")]
        public IActionResult ChangeCouponStatus1(int ID)
        {
            ICoupanVoucherML coupanVoucherML = new CoupanVoucherML(_accessor, _env);
            var resp = coupanVoucherML.ChangeCouponStatus(ID);
            return Json(resp);
        }

        [HttpPost]
        
        [Route("CouponUpload/{id}")]
        public IActionResult _CoupanUpload(int id)
        {
            ViewBag.VoucherID = id;
            var res = new APIML(_accessor, _env).GetVoucherApi();
            return PartialView("Partial/_CouponUploadExl", res);
           
            
        }


        [HttpPost]
        
        [Route("CouponVocherUpload")]
        public async Task<IActionResult> UploadOutletUserListExcelAsync(Microsoft.AspNetCore.Http.IFormFile file, int APIID,int VocherID)
        {
            ICoupanVoucherML coupanVoucherML = new CoupanVoucherML(_accessor, _env);
            IResponseStatus res = new ResponseStatus();
            if (file == null || file.Length <= 0)
            {
                res.Statuscode = -1;
                res.Msg = "No file found.";
                return Json(res);
            }
            if (APIID < 1)
            {
                res.Statuscode = -1;
                res.Msg = "API Id not found.";
                return Json(res);
            }
            if (VocherID < 1)
            {
                res.Statuscode = -1;
                res.Msg = "Vocher Id not found.";
                return Json(res);
            }
            if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                res.Statuscode = -1;
                res.Msg = "Uploaded file is not valid.Please upload .xlsx file only.";
                return Json(res);
            }
            var list = new List<CoupanVoucherEXl>();
            try
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                        var cIndex = new CoupanVoucherIndex
                        {
                            CouponCode = worksheet.GetColumnByName("CouponCode"),
                            Amount = worksheet.GetColumnByName("Amount")
                            
                           
                        };
                        var rowCount = worksheet.Dimension.Rows;
                        for (int row = 2; row <= rowCount; row++)
                        {
                            var olt = new CoupanVoucherEXl
                            {
                                CouponCode = Convert.ToString(worksheet.Cells[row, cIndex.CouponCode].Value),
                                Amount = Convert.ToInt32(worksheet.Cells[row, cIndex.Amount].Value),
                                
                               
                            };
                            list.Add(olt);
                        }
                    }
                }

                var ReqData = new CoupanVoucherEXlReq
                {
                    Couponvocher = list,
                    APIID = APIID,
                    VocherID= VocherID
                };
                res = await coupanVoucherML.UploadCouponExcelUPloda(ReqData).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                res.Statuscode = ErrorCodes.Minus1;
                res.Msg = ErrorCodes.AnError;
            }
            return Json(res);
        }

        [HttpGet]
        [Route("Admin/CouponVocher-Tamplate")]
        [Route("CouponVocher-Tamplate")]
        public IActionResult _CouponVocherTamplate()
        {
            List<CoupanVoucherEXl> _report = new List<CoupanVoucherEXl>();
            DataTable dataTable = ConverterHelper.O.ToDataTable(_report);
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("CouponVocherList");
                worksheet.Cells["A1"].LoadFromDataTable(dataTable, PrintHeaders: true);
                worksheet.Row(1).Height = 20;
                worksheet.Row(1).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Row(1).Style.Font.Bold = true;
                for (var col = 1; col < dataTable.Columns.Count + 1; col++)
                {
                    worksheet.Column(col).AutoFit();
                }
                var exportToExcel = new ExportToExcel
                {
                    Contents = package.GetAsByteArray(),
                    FileName = "CouponVocherTamplate.xlsx"
                };
                return File(exportToExcel.Contents, DOCType.XlsxContentType, exportToExcel.FileName);
            }
        }


        [HttpGet]
        [Route("Home/CouponVocher-Export")]
        [Route("CouponVocher-Export/{id}")]
      
        public async Task<IActionResult> _CouponVocherImport(int id)


        { 
            var IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser ;
            ICoupanVoucherML coupanVoucherML = new CoupanVoucherML(_accessor, _env);

            CoupanVoucherReq res = new CoupanVoucherReq();
            res.CoupanVoucher = coupanVoucherML.GetCouponVoucher(id);
            DataTable dataTable = ConverterHelper.O.ToDataTable(res.CoupanVoucher.ToList());
            var removableCol = new List<string> { "VoucherID","ID", "APIID", "EntryBy" ,"EntryDate" ,  "ModifyBy", "ModifyDate",  "LoginID", "LoginTypeID" ,"UserID",  "CommonInt" ,"CommonInt2"  ,"CommonStr" ,"CommonStr2",   "CommonInt3", "CommonDecimal",   "CommonInt4" ,"CommonStr3",  "CommonStr4", "CommonBool" , "CommonBool1", "CommonBool2", "CommonChar" };
            
            foreach (string str in removableCol)
            {
                dataTable.Columns.Remove(str);
            }
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("VocherList1");
                worksheet.Cells["A1"].LoadFromDataTable(dataTable, PrintHeaders: true);
                worksheet.Row(1).Height = 20;
                worksheet.Row(1).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Row(1).Style.Font.Bold = true;
                for (var col = 1; col < dataTable.Columns.Count + 1; col++)
                {
                    worksheet.Column(col).AutoFit();
                }
                var exportToExcel = new ExportToExcel
                {
                    Contents = package.GetAsByteArray(),
                    FileName = "CouponVocherList.xlsx"
                };
                return File(exportToExcel.Contents, DOCType.XlsxContentType, exportToExcel.FileName);
            }
        }

        [HttpPost]
        [Route("upload-CouponVocher")]
        public IActionResult UploadCouponVocher(IFormFile file, int OID,string filename)
        {
            IResourceML mL = new ResourceML(_accessor, _env);
            var _res = mL.UploadCouponVoucher(file, OID,filename);
            return Json(_res);
        }

        [HttpPost]
        
        [Route("CouponVoucherImageList/{id}")]
        public IActionResult _CouponImageList(int ID)
        {
            IUserML userML = new UserML(_lr);
            var coupanvoucher = new CoupanMaster
            {
                IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser

            };

            if ((coupanvoucher.IsAdmin || !_lr.IsAdminDefined) && !userML.IsEndUser())
            {
                ICoupanVoucherML coupanVoucherML = new CoupanVoucherML(_accessor, _env);
                coupanvoucher = coupanVoucherML.GetCouponMaster(ID);
                var OpML = new OperatorML(_accessor, _env);
                coupanvoucher.OpDetail = OpML.GetOperators(OPTypes.CouponVoucher);
                coupanvoucher.VocImage = coupanVoucherML.GetCouponvocherImage(coupanvoucher.OID);

                return PartialView("Partial/_CouponVoucherImageList", coupanvoucher);
            }
            return Ok();
        }

        [HttpPost]
      
        [Route("removeVocherImage")]
        public IActionResult RemoveImg(string OID,string filename)
        {
            IResourceML _IResourceML = new ResourceML(_accessor, _env);
            var _res = _IResourceML.RemoveCouponVoucher(OID, filename, _lr);
            return Json(_res);
        }


        [HttpPost]
       
        [Route("CouponVoucher-Del")]
        public IActionResult CouponVoucherDel([FromBody] CoupanVoucher couponvouchert)
        {
            IUserML userML = new UserML(_lr);
            ViewBag.IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.AddEditSLAB);
            if ((ViewBag.IsAdmin || !_lr.IsAdminDefined) && !userML.IsEndUser())
            {
                ICoupanVoucherML coupanVoucherML = new CoupanVoucherML(_accessor, _env);
                var resp = coupanVoucherML.DelCouponVoucher(couponvouchert);
                return Json(resp);
            }
            return Ok();
        }


    }
}
