using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OfficeOpenXml;
using RoundpayFinTech.AppCode.Configuration;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.Report;
using RoundpayFinTech.AppCode.Model.Shopping;
using RoundpayFinTech.AppCode.Model.Shopping.WebShopping.ViewModel;
using RoundpayFinTech.AppCode.StaticModel;
using Validators;

namespace RoundpayFinTech.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class AdminShopping : BaseController
    {
        public AdminShopping(IHttpContextAccessor accessor, IHostingEnvironment env) : base(accessor, env)
        {

        }

        public IActionResult Index()
        {
            if (_lr.RoleID == Role.Admin && LoginType.ApplicationUser == _lr.LoginTypeID && ApplicationSetting.IsECommerceAllowed)
                return View();
            return Ok();
        }

        [HttpGet]
        [Route("/FEImage")]
        public IActionResult FEImage()
        {
            if (_lr.RoleID == Role.Admin && LoginType.ApplicationUser == _lr.LoginTypeID && ApplicationSetting.IsECommerceAllowed)
            {
                IShoppingML ml = new ShoppingML(_accessor, _env);
                return View();
            }
            return Ok();
        }

        [HttpPost]
        [Route("/GetFEImg")]
        public IActionResult _FEImg()
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var list = mL.GetFEImageList(0, 0);
            return PartialView("Partial/_FEImg", list);
        }

        [HttpPost]
        [Route("/_FEImgUpdate")]
        public IActionResult _FEImgUpadte(int id = 0)
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var res = new FEImage();
            if (id > 0)
            {
                var data = mL.GetFEImageList(id, 0);
                res = data.ImgList[0];
            }
            res.Categories = mL.GetShoppingMainCategoryNew();
            return PartialView("Partial/_FEImgUpdate", res);
        }

        [HttpPost]
        [Route("/AddFEImg")]
        public IActionResult AddFEImg(IFormFile file, string detail)
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var req = JsonConvert.DeserializeObject<CommonReq>(detail);
            var res = mL.UploadFEImage(file, req);
            return Json(res);
        }

        [HttpPost]
        [Route("/UpdateFEImg")]
        public IActionResult UpdateFEImg(int Id, bool IsActive, bool IsDelete = false)
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var res = mL.UpdateFEImg(Id, IsActive, IsDelete);
            return Json(res);
        }

        #region Category

        [HttpGet]
        [Route("/shopping/category")]
        public IActionResult Category()
        {
            if (_lr.RoleID == Role.Admin && LoginType.ApplicationUser == _lr.LoginTypeID && ApplicationSetting.IsECommerceAllowed)
                return View();
            return Ok();
        }

        [HttpPost]
        [Route("/_ShoppingCategory")]
        public IActionResult _Category()
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var res = mL.GetShoppingCategory();
            return PartialView("Partial/_Category", res);
        }

        [HttpPost]
        [Route("/AddCategory")]
        public IActionResult AddCategory(int id)
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var res = mL.GetShoppingCategoryByID(id);
            return PartialView("Partial/_AddCategory", res);
        }


        //[HttpPost]
        //[Route("/updateCategory")]
        //public IActionResult updateCategory(CommonReq req)//CommonReq
        //{
        //    IShoppingML mL = new ShoppingML(_accessor, _env);
        //    var res = mL.UpdateShoppingCategory(req);
        //    return Json(res);
        //}
        [HttpPost]
        [Route("/updateCategory")]
        public IActionResult updateCategory(List<IFormFile> file, string ImgName, string detail)//CommonReq
        {
            var res = new ResponseStatus();
            try
            {
                var categoryData = JsonConvert.DeserializeObject<CommonReq>(detail);
                IShoppingML mL = new ShoppingML(_accessor, _env);
                var validateResponse = new ResponseStatus();
                if (file != null)
                {
                    foreach (var f in file)
                    {
                        validateResponse = Validate.O.IsImageValid(f);
                        if (validateResponse.Statuscode == ErrorCodes.Minus1)
                        {
                            break;
                        }
                    }
                    if (validateResponse.Statuscode == ErrorCodes.One)
                    {
                        //var req = new CommonReq
                        //{
                        //    LoginTypeID = _lr.LoginTypeID,
                        //    LoginID = _lr.UserID,
                        //    CommonInt = 0,
                        //    CommonInt2 = catId,
                        //    CommonStr = filename,
                        //    CommonInt3 = imgType,
                        //    CommonBool = true
                        //};
                        var _res = mL.UploadIcon(file[0], categoryData.CommonInt == 0 ? int.Parse(res.Msg) : categoryData.CommonInt, "", "Shopping");
                        // res = mL.UpdateShoppingCategory(categoryData);
                        //if (res.Statuscode == ErrorCodes.One)
                        //{
                        //    for (int i = 0; i < file.Count; i++)
                        //    {

                        //        if (_res.Statuscode == ErrorCodes.Minus1)
                        //        {
                        //            res.Statuscode = _res.Statuscode;
                        //            res.Msg = _res.Msg;
                        //            break;
                        //        }
                        //    }
                        //}
                    }
                    else
                    {
                        // res = mL.UpdateShoppingCategory(categoryData);
                    }
                }
                else
                {
                    res.Statuscode = ErrorCodes.Minus1;
                    res.Msg = "No Image Found";
                }
            }
            catch (Exception ex)
            {
                res.Statuscode = ErrorCodes.Minus1;
                res.Msg = ex.Message;
            }

            return Json(res);
        }

        [HttpPost]
        [Route("/getIconImage")]
        public IActionResult getIconImage(int id, string iconType, string pathConcat = "")
        {
            List<string> files = new List<string>();
            try
            {
                string path = string.Empty;
                if (string.IsNullOrEmpty(path))
                {
                    path = DOCType.IconImagePath.Replace("{0}", "Shopping");
                }
                else
                {
                    path = DOCType.IconImagePath.Replace("{0}", pathConcat);
                }

                if (Directory.Exists(path))
                {
                    DirectoryInfo d = new DirectoryInfo(path);
                    string fileName = string.Empty;
                    if (!string.IsNullOrEmpty(iconType))
                    {
                        fileName = iconType + "_" + id.ToString();
                    }
                    else
                    {
                        fileName = id.ToString();
                    }
                    fileName = fileName + ".png";
                    FileInfo[] Files = d.GetFiles(fileName);
                    if (Files.Length > 0)
                    {
                        foreach (FileInfo file in Files)
                        {
                            files.Add(file.Name);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return Json(files);
        }
        #endregion

        #region SubCategory Level-1

        [HttpPost]
        [Route("/AddSubCategoryLvl1")]
        public IActionResult AddSubCategoryLvl1(int id, int cid)
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var res = mL.GetShoppingSubCategoryByID(id);
            res.CategoryID = res.CategoryID == 0 ? cid : res.CategoryID;
            return PartialView("Partial/_AddSubCategoryLvl1", res);
        }

        [HttpPost]
        [Route("/updatesubCategoryLvl1")]
        public IActionResult updatesubCategoryLvl1(List<IFormFile> file, string ImgName, string detail)
        {
            var res = new ResponseStatus();
            try
            {
                var categoryData = JsonConvert.DeserializeObject<CommonReq>(detail);
                IShoppingML mL = new ShoppingML(_accessor, _env);
                var validateResponse = new ResponseStatus();
                if (file != null)
                {
                    foreach (var f in file)
                    {
                        validateResponse = Validate.O.IsImageValid(f);
                        if (validateResponse.Statuscode == ErrorCodes.Minus1)
                        {
                            break;
                        }
                    }
                    if (validateResponse.Statuscode == ErrorCodes.One)
                    {
                        var _res = mL.UploadIcon(file[0], categoryData.CommonInt == 0 ? int.Parse(res.Msg) : categoryData.CommonInt, "Shopping", "S1");
                        categoryData.CommonStr2 = "S1_" + categoryData.CommonInt + ".png";
                        res = mL.UpdateShoppingSubCategoryLvl1(categoryData);


                        //if (res.Statuscode == ErrorCodes.One)
                        //{
                        //    for (int i = 0; i < file.Count; i++)
                        //    {
                        //        var _res = mL.UploadIcon(file[i], categoryData.CommonInt == 0 ? int.Parse(res.Msg) : categoryData.CommonInt, "Shopping", "S1");
                        //        if (_res.Statuscode == ErrorCodes.Minus1)
                        //        {
                        //            res.Statuscode = _res.Statuscode;
                        //            res.Msg = _res.Msg;
                        //            break;
                        //        }
                        //    }
                        //}
                    }
                    else
                    {
                        //res = validateResponse;
                        res = mL.UpdateShoppingSubCategoryLvl1(categoryData);
                    }
                }
                else
                {
                    res.Statuscode = ErrorCodes.Minus1;
                    res.Msg = "No Image Found";
                }
            }
            catch (Exception ex)
            {
                res.Statuscode = ErrorCodes.Minus1;
                res.Msg = ex.Message;
            }

            return Json(res);
            //IShoppingML mL = new ShoppingML(_accessor, _env);
            //var res = mL.UpdateShoppingSubCategoryLvl1(req);
            //return Json(res);
        }

        [HttpPost]
        [Route("/_SubCategoryLvl1")]
        public IActionResult _SubCategoryLvl1(int cid)
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var list = mL.GetSubCategoryLvl1(cid);
            var response = new subCategoryModelLvl1
            {
                CategoryID = cid,
                Level1 = list
            };
            return PartialView("Partial/_SubCategoryLvl1", response);
        }

        [HttpPost]
        [Route("/List-SubCategoryLvl1")]
        public IActionResult ListSubCategoryLvl1(int cid)
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var list = mL.GetSubCategoryLvl1(cid);
            return Json(list);

        }
        #endregion

        #region SubCategory Level-2

        [HttpPost]
        [Route("/updatesubCategoryLvl2")]
        public IActionResult updatesubCategoryLvl2(List<IFormFile> file, string ImgName, string detail)
        {
            var res = new ResponseStatus();
            try
            {
                var validateResponse = new ResponseStatus();
                var categoryData = JsonConvert.DeserializeObject<CommonReq>(detail);
                IShoppingML mL = new ShoppingML(_accessor, _env);
                if (file != null)
                {
                    foreach (var f in file)
                    {
                        validateResponse = Validate.O.IsImageValid(f);
                        if (validateResponse.Statuscode == ErrorCodes.Minus1)
                        {
                            break;
                        }
                    }
                    if (validateResponse.Statuscode == ErrorCodes.One)
                    {
                        res = mL.UpdateSubCategoryLvl2(categoryData);
                        if (res.Statuscode == ErrorCodes.One)
                        {
                            for (int i = 0; i < file.Count; i++)
                            {
                                var _res = mL.UploadIcon(file[i], categoryData.CommonInt2 == 0 ? int.Parse(res.Msg) : categoryData.CommonInt2, "Shopping", "S2");
                                if (_res.Statuscode == ErrorCodes.Minus1)
                                {
                                    res.Statuscode = _res.Statuscode;
                                    res.Msg = _res.Msg;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        //res = validateResponse;
                        res = mL.UpdateSubCategoryLvl2(categoryData);
                    }
                }
                else
                {
                    res.Statuscode = ErrorCodes.Minus1;
                    res.Msg = "No Image Found";
                }
            }
            catch (Exception ex)
            {
                res.Statuscode = ErrorCodes.Minus1;
                res.Msg = ex.Message;
            }

            return Json(res);
        }

        [HttpPost]
        [Route("/AddSubCategoryLvl2")]
        public IActionResult AddSubCategoryLvl2(int id, int sid)
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var res = mL.GetSubCategoryLvl2ByID(id);
            res.SubCategoryID = id == 0 ? sid : res.SubCategoryID;
            return PartialView("Partial/_AddSubCategoryLvl2", res);
        }

        [HttpPost]
        [Route("/_SubCategoryLvl2")]
        public IActionResult _SubCategoryLvl2(int sid)
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var list = mL.GetSubCategoryLvl2(sid);
            var response = new subCategoryModelLvl2
            {
                SubCategoryID = sid,
                subCategoryList = list
            };
            return PartialView("Partial/_SubCategoryLvl2", response);
        }


        [HttpPost]
        [Route("/List-SubCategoryLvl2")]
        public IActionResult ListSubCategoryLvl2(int sid)
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var list = mL.GetSubCategoryLvl2(sid);
            return Json(list);
        }

        #endregion

        #region SubCategory Level-3

        [HttpPost]
        [Route("/updatesubCategoryLvl3")]
        public IActionResult updatesubCategoryLvl3(CommonReq req)
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var res = mL.UpdateSubCategoryLvl3(req);
            return Json(res);
        }

        [HttpPost]
        [Route("/AddSubCategoryLvl3")]
        public IActionResult AddSubCategoryLvl3(int id, int sid)
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var res = mL.GetSubCategoryLvl3ByID(id);
            res.SubCategoryIDLvl2 = res.SubCategoryIDLvl2 == 0 ? sid : res.SubCategoryIDLvl2;
            return PartialView("Partial/_AddSubCategoryLvl3", res);
        }

        [HttpPost]
        [Route("/_SubCategoryLvl3")]
        public IActionResult _SubCategoryLvl3(int sid)
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var list = mL.GetSubCategoryLvl3(sid);
            var response = new subCategoryModelLvl3
            {
                SubCategoryID = sid,
                subCategoryList = list
            };
            return PartialView("Partial/_SubCategoryLvl3", response);
        }
        #endregion

        #region Filter
        [HttpGet]
        [Route("/Filter")]
        public IActionResult Filter()
        {
            if (_lr.RoleID == Role.Admin && LoginType.ApplicationUser == _lr.LoginTypeID && ApplicationSetting.IsECommerceAllowed)
            {
                IShoppingML ml = new ShoppingML(_accessor, _env);
                var res = ml.GetShoppingMainCategoryNew();
                return View(res);
            }
            return Ok();
        }

        [HttpPost]
        [Route("/GetFilter")]
        public IActionResult _Filter()
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var list = mL.GetFilter();
            return PartialView("Partial/_Filter", list);
        }

        [HttpPost]
        [Route("/updateFilter")]
        public IActionResult updateFilter(CommonReq req)
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var res = mL.UpdateFilter(req);
            return Json(res);
        }

        [Route("/GetFilterByID")]
        public IActionResult AddSubCategoryLvl3(int FID)
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var res = mL.GetFilterByID(FID);
            return PartialView("Partial/_AddFilter", res);
        }

        [HttpPost]
        [Route("/GetMappedFilter")]
        public IActionResult _MappedFilter(int CID)
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var list = mL.GetFilterForMapping(CID);
            var res = new FilterMapping
            {
                filter = list,
                SubCategoryId = CID
            };
            return PartialView("Partial/_MappedFilter", res);
        }


        [HttpPost]
        [Route("/updateFilterMapping")]
        public IActionResult updateMappedFilter(CommonReq req)
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var res = mL.UpdateMappedFilter(req);
            return Json(res);
        }

        [HttpPost]
        [Route("/FilterOption")]
        public IActionResult FilterOption(int FilterID)
        {
            return PartialView("FilterOption", FilterID);
        }

        [HttpPost]
        [Route("/GetFilterOption")]
        public IActionResult _FilterOption(int FilterID)
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var list = mL.GetFilterOption(FilterID);
            return PartialView("Partial/_FilterOption", list);
        }

        [HttpPost]
        [Route("/AddFilterOption")]
        public IActionResult AddFilterOption(int OptionID, int FilterID)
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var res = mL.GetFilterOptionByID(OptionID);
            res.FilterID = res.FilterID == 0 ? FilterID : FilterID;
            if (res.FilterID == ShoppingFilters.Size)
            {
                res.Uoms = mL.GetUom();
            }
            if (res.FilterID == ShoppingFilters.Color)
            {
                res.Colors = mL.GetColors();
            }
            return PartialView("Partial/_AddFilterOption", res);
        }

        [HttpPost]
        [Route("/UpdateFilterOption")]
        public IActionResult UpdateFilterOption(CommonReq req)
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var res = mL.UpdateFilterOption(req);
            return Json(res);
        }
        #endregion

        #region Product

        [HttpGet]
        [Route("/AddProduct")]
        [Route("/AddProduct/{ProductDetailID}")]
        public IActionResult AddProduct(int ProductDetailID)
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var model = new ProductModel
            {
                Role = _lr.RoleID,
                MainCategory = mL.GetShoppingMainCategoryNew(),
                Vendors = mL.GetVendors(),
                Detail = new AddProductModal()
                {
                    ProductDetail = new ProductDetail()
                }
            };

            if (ProductDetailID > 0)
            {
                model.Detail = mL.AddProductModal(ProductDetailID).Result;
                if (model.Detail.ProductDetail != null)
                {
                    model.CategoryList = mL.GetCategory(model.Detail.ProductDetail.CategoryID);
                    model.SubCategory = mL.GetSubCategoryNew(model.Detail.ProductDetail.SubCategoryID1);
                    model.MasterProduct = mL.GetMasterProduct(new MasterProduct
                    {
                        CategoryID = model.Detail.ProductDetail.CategoryID,
                        SubCategoryID1 = model.Detail.ProductDetail.SubCategoryID1,
                        SubCategoryID2 = model.Detail.ProductDetail.SubCategoryID2
                    });
                    model.Brands = mL.GetBrand(model.Detail.ProductDetail.CategoryID);
                }
                else
                {
                    model.Detail.ProductDetail.ProductDetailID = 0;
                }
            }

            if (LoginType.ApplicationUser == _lr.LoginTypeID && ApplicationSetting.IsECommerceAllowed)
                return View(model);
            return Ok();

        }

        //[HttpPost]
        //[Route("/Add-MasterProduct")]
        //public IActionResult AddMasterProduct()
        //{
        //    IShoppingML mL = new ShoppingML(_accessor, _env);
        //    var model = new ProductModel
        //    {
        //        Category = mL.GetShoppingCategory()
        //    };
        //    return PartialView("Partial/_ProductMaster", model);
        //}

        [HttpPost]
        [Route("/Add-MasterProduct")]
        public IActionResult AddMasterProduct(int PID)
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var detail = mL.GetMasterProductById(PID);
            var model = new ProductModel
            {
                MainCategory = mL.GetShoppingMainCategoryNew(),
                MasterProductDetail = detail
            };
            if (PID > 0)
            {
                model.CategoryList = mL.GetCategory(model.MasterProductDetail.CategoryID);
                model.SubCategory = mL.GetSubCategoryNew(model.MasterProductDetail.SubCategoryID1);
            }
            return PartialView("Partial/_ProductMaster", model);
        }

        [HttpPost]
        [Route("/updateMasterProduct")]
        public IActionResult AddMasterProduct(MasterProduct req)
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var res = mL.AddMasterProduct(req);
            return Json(res);
        }

        [HttpPost]
        [Route("/List-MasterProduct")]
        public IActionResult ListMasterProduct(MasterProduct req)
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var list = mL.GetMasterProduct(req);
            return Json(list);
        }

        [HttpPost]
        [Route("/List-Brand")]
        public IActionResult ListBrand(int cid)
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var list = mL.GetBrand(cid);
            return Json(list);
        }

        [HttpPost]

        [Route("/List-Filter-Option")]
        [ResponseCache(VaryByHeader = "/List-Filter-Option", Duration = 120)]
        public IActionResult ListFilterOption(int CID, int sid, int sid2, string filters, bool IsListForm = false)
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var list = mL.GetRequeiredFilter(CID, sid, sid2, filters);
            var modal = new FilterForEndUser
            {
                FilterWithOptions = list,
                Brands = mL.GetBrand(CID)
            };
            if (IsListForm)
                return PartialView("Partial/_FilterForEndUser", modal);
            else
                return PartialView("Partial/_FilterWithOption", modal.FilterWithOptions);
        }

        [HttpPost]
        [Route("upload-ProductImage")]
        public IActionResult uploadProductImage(List<IFormFile> file, string ImgName, string productDetail)
        {
            var res = new ResponseStatus();
            try
            {
                var validateResponse = new ResponseStatus();
                if (file != null)
                {
                    if (file.Count > 3)
                    {
                        foreach (var f in file)
                        {
                            validateResponse = Validate.O.IsImageValid(f);
                            if (validateResponse.Statuscode == ErrorCodes.Minus1)
                            {
                                break;
                            }
                        }
                        if (validateResponse.Statuscode == ErrorCodes.One)
                        {
                            var product = JsonConvert.DeserializeObject<ProductDetail>(productDetail);
                            IShoppingML mL = new ShoppingML(_accessor, _env);
                            res = mL.AddProduct(product);
                            if (res.Statuscode == ErrorCodes.One)
                            {
                                if (product.ProductDetailID == 0 || product.ProductDetailID > 0)
                                {
                                    StringBuilder Images = new StringBuilder();

                                    for (int i = 0; i < file.Count; i++)
                                    {
                                        var _res = mL.UploadProductImage(file[i], product.ProductID, product.ProductDetailID > 0 ? product.ProductDetailID : res.CommonInt, ImgName, i);
                                        if (_res.Statuscode == ErrorCodes.Minus1)
                                        {
                                            res.Statuscode = _res.Statuscode;
                                            res.Msg = _res.Msg;
                                            break;
                                        }
                                        Images.Append(product.ProductDetailID.ToString() + "_" + ImgName.Split('#')[1] + "_" + i.ToString() + "-1x.png,");
                                    }
                                    // product.Images = Images.ToString();
                                }
                            }

                        }
                        else
                        {
                            res = validateResponse;
                        }
                    }
                    else
                    {
                        res.Statuscode = ErrorCodes.Minus1;
                        res.Msg = "Minimum 3 Image";
                    }
                }
                else
                {
                    res.Statuscode = ErrorCodes.Minus1;
                    res.Msg = "No Image Found";
                }
            }
            catch (Exception ex)
            {
                res.Statuscode = ErrorCodes.Minus1;
                res.Msg = ex.Message;
            }
            return Json(res);
        }
        [HttpPost]
        [Route("/deleteProductImage")]
        public IActionResult DeleteProductImage(string ImagePath)
        {
            var Res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1
            };
            try
            {
                ImagePath = ImagePath.Substring(1, ImagePath.Length - 1);
                if (System.IO.File.Exists(ImagePath))
                {
                    System.IO.File.Delete(ImagePath);
                    Res.Statuscode = ErrorCodes.One;
                }
                var s = ImagePath.Split('.');
                if (s != null && s.Length > 1)
                {
                    var ImagePath2 = s[0] + "-1x." + s[1];
                    if (!string.IsNullOrEmpty(ImagePath2) && System.IO.File.Exists(ImagePath2))
                    {
                        System.IO.File.Delete(ImagePath2);
                        Res.Statuscode = ErrorCodes.One;
                    }
                }
            }
            catch (Exception ex)
            {
                Res.Msg = ex.Message;
            }
            return Json(Res);
        }

        [HttpPost]
        [Route("/DeletedProductDetail")]
        public IActionResult DeletedProductDetail(int ProductDetailID, bool IsDeleted)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "You are not permited for this user"
            };
            if (_lr.RoleID == Role.Admin)
            {
                IShoppingML ml = new ShoppingML(_accessor, _env);
                res = ml.DeleteProductDetail(ProductDetailID, IsDeleted);
            }
            return Json(res);
        }

        [HttpGet]
        [Route("/AllProduct")]
        public IActionResult AllProduct()
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var Category = mL.GetShoppingMainCategoryNew();
            if (LoginType.ApplicationUser == _lr.LoginTypeID && ApplicationSetting.IsECommerceAllowed)
                return View(Category);
            return Ok();
        }

        [HttpPost]
        [Route("/_AllProduct")]
        public IActionResult _AllProduct(int CID, int SID1, int SID2)
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var res = mL.GetAllProducts(CID, SID1, SID2);
            return PartialView("Partial/_AllProduct", res);
        }

        [HttpPost]
        [Route("/_ProductDetail")]
        public IActionResult _ProductDetail(int ProductID)
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var res = mL.GetProductDetails(ProductID);
            return PartialView("Partial/_ProductDetails", res);
        }

        [HttpPost]
        [Route("/ReadProductImages")]
        public IActionResult ReadProductImages(int PID, int PdetailId)
        {
            List<string> files = new List<string>();
            try
            {
                string path = DOCType.ProductImagePath.Replace("{0}", PID.ToString());
                if (Directory.Exists(path))
                {
                    DirectoryInfo d = new DirectoryInfo(path);
                    FileInfo[] Files = d.GetFiles(PdetailId.ToString() + "_*");
                    foreach (FileInfo file in Files)
                    {
                        files.Add(file.Name);
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return Json(files);
        }

        [HttpPost]
        [Route("/_ProductForIndex")]
        public IActionResult _ProductForIndex(ProductFilter p)
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var res = mL.GetProductForIndex(p);
            return PartialView("Partial/_ProductForIndex", res ?? new List<ProductDetail>());
        }

        [HttpPost]
        [Route("/_ProductTrending")]
        public IActionResult _ProductTrending(ProductFilter p)
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var res = mL.GetProductTrending(p);
            return PartialView("Partial/_ProductTrending", res);
        }

        [HttpPost]
        [Route("/_ProductNewArrival")]
        public IActionResult _ProductNewArrival(ProductFilter p)
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var res = mL.GetProductNewArrival(p);
            return PartialView("Partial/_ProductNewArrival", res);
        }

        [HttpPost]
        [Route("/FilteredProduct")]
        public IActionResult FilteredProduct(ProductFilter p)
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var res = mL.GetFilteredProduct(p);
            return PartialView("Partial/_ProductFiltered", res);
        }

        #endregion

        #region Commission 
        [HttpPost]
        [Route("/_ShoppingCommission")]
        public IActionResult _ShoppingCommission(bool IsAdminDefined, int slabid)
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var subCategoryLvl2 = mL.GetSubCategoryNew();
            var res = new ShoppingCommissionModel
            {
                IsAdminDefined = IsAdminDefined,
                SlabID = slabid,
                ShoppingCategories = subCategoryLvl2,
                Roles = mL.ShoppingCommissionRoles()
            };
            if (!IsAdminDefined)
                return PartialView("Partial/_ShoppingCommission", res);
            else
                return PartialView("Partial/_ShoppingCommissionLvl", res);
        }

        [HttpPost]
        [Route("/_ShoppingCommissionDetail")]
        public IActionResult _ShoppingCommissionDetail(ShoppingCommissionReq req)
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var res = mL.GetShopppingSlabCommission(req);
            if (!req.IsAdminDefined)
                return Json(res);
            else
                return PartialView("Partial/_ShoppingCommissionGrid", res);
        }

        [HttpPost]
        [Route("/_UpdateShoppingComm")]
        public IActionResult UpdateShoppingComm(ShoppingCommissionReq req)
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var res = mL.UpdateShoppingComm(req);
            return Json(res);
        }

        [HttpPost]
        [Route("/GetUserCommission")]
        public IActionResult GetUserCommission(int pdId)
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var res = mL.GetUserCommission(pdId);
            return Json(res);
        }

        #endregion

        #region Brand
        [HttpPost]
        [Route("/Brand")]
        public IActionResult Brand(int Id = 0)
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            BrandEdit model = new BrandEdit();
            if (Id > 0)
            {
                var res = (List<Brand>)mL.GetBrandById(Id);
                if (res.Count == 1)
                {
                    model.BrandId = res[0].BrandId;
                    model.BrandName = res[0].BrandName;
                    model.CategoryID = res[0].CategoryID;
                    model.CategoryName = res[0].CategoryName;
                    model.IsActive = res[0].IsActive;
                }
            }
            model.Categories = mL.GetShoppingMainCategoryNew();
            return PartialView("Partial/Brand", model);
        }

        [HttpPost]
        [Route("/SaveBrand")]
        public IActionResult SaveBrand(Brand br)
        {
            IShoppingML ml = new ShoppingML(_accessor, _env);
            var res = ml.SaveBrand(br);
            return Json(res);
        }

        [HttpPost]
        [Route("GetBrands")]
        public IActionResult GetBrands(int CategoryID)
        {
            IShoppingML ml = new ShoppingML(_accessor, _env);
            var res = ml.GetBranddetail(CategoryID);
            return PartialView("Partial/_BrandDetail", res);
        }
        #endregion

        #region Vendor
        [HttpGet]
        [Route("/ShoppingVendor")]
        public IActionResult VendorList()
        {
            if (_lr.RoleID == Role.Admin && LoginType.ApplicationUser == _lr.LoginTypeID && ApplicationSetting.IsECommerceAllowed)
            {
                return View();
            }
            return Ok();
        }

        [HttpPost]
        [Route("/_ShoppingVendor")]
        public IActionResult _VendorList()
        {
            if (_lr.RoleID == Role.Admin && LoginType.ApplicationUser == _lr.LoginTypeID && ApplicationSetting.IsECommerceAllowed)
            {
                IShoppingML mL = new ShoppingML(_accessor, _env);
                var res = mL.GetVendorList();
                return PartialView("Partial/_VendorList", res);
            }
            return Ok();
        }

        [HttpPost]
        [Route("/_ShoppingVendorStatus")]
        public IActionResult _VendorList(string id, bool val)
        {
            if (_lr.RoleID == Role.Admin && LoginType.ApplicationUser == _lr.LoginTypeID && ApplicationSetting.IsECommerceAllowed)
            {
                IShoppingML mL = new ShoppingML(_accessor, _env);
                var res = mL.ChangeECommVendorStatus(id, val);
                return Json(res);
            }
            return Ok();
        }

        [HttpPost]
        [Route("/Vendor")]
        public IActionResult _Vendor()
        {
            return PartialView("Partial/_Vendor");
        }

        [HttpPost]
        [Route("/SaveVendor")]
        public IActionResult SaveVendor(Vendors br)
        {
            IShoppingML ml = new ShoppingML(_accessor, _env);
            var res = ml.SaveVendor(br);
            return Json(res);
        }
        #endregion

        #region Shop
        [HttpGet]
        [Route("/Shop")]
        public IActionResult Shop()
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            //var model = mL.GetShoppingCategory();
            var model = mL.GetShoppingMenu();
            if (ApplicationSetting.IsECommerceAllowed)
                return View(model);
            return Ok();
        }

        [HttpPost]
        [Route("/ShopMenu")]
        public IActionResult ShopMenu()
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var model = mL.GetShoppingMenu();
            return Json(model);
        }

        [HttpPost]
        [Route("/ProductDetailForUser")]
        public async Task<IActionResult> ProductDetailForUser(int PID, int PdetailId)
        {
            IShoppingML ml = new ShoppingML(_accessor, _env);
            var loginid = _lr.UserID;
            var res = await ml.ProDescription(PdetailId, loginid);
            return PartialView("Partial/_ProductDetailForUser", res);
        }

        [HttpPost]
        [Route("/GetFilters")]
        public IActionResult GetFilters(int PID, int PdetailId)
        {
            IShoppingML ml = new ShoppingML(_accessor, _env);
            var FilterDetail = ml.GetAvailableFilter(PID, PdetailId);
            return Json(FilterDetail);
        }
        #endregion

        #region Cart

        //[HttpPost]
        //[Route("/AddToCart")]
        //public async Task<IActionResult> AddToCart(int ProductID, int Quantity, List<string> Filters)
        //{
        //    IShoppingML ml = new ShoppingML(_accessor, _env);
        //    var res = await ml.AddToCart(ProductID, Quantity, Filters);
        //    return Json(res);
        //}

        [HttpPost]
        [Route("/AddToCart")]
        public async Task<IActionResult> AddToCart(int ProductID, int ProductDetailID, int Quantity)
        {
            IShoppingML ml = new ShoppingML(_accessor, _env);
            var res = await ml.AddToCart(ProductDetailID, Quantity);
            return Json(res);
        }

        [HttpPost]
        [Route("/CartDetail")]
        public async Task<IActionResult> CartDetail()
        {
            IShoppingML ml = new ShoppingML(_accessor, _env);
            var res = await ml.CartDetail().ConfigureAwait(false);
            return PartialView("Partial/_CartDetail", res);
        }

        [HttpPost]
        [Route("/RemoveItemFromCart")]
        public async Task<IActionResult> RemoveItemFromCart(int ID)
        {
            IShoppingML ml = new ShoppingML(_accessor, _env);
            var res = await ml.RemoveItemFromCart(ID);
            return Json(res);
        }

        [HttpPost]
        [Route("/ChangeQuantity")]
        public async Task<IActionResult> ChangeQuantity(int ItemID, int Quantity)
        {
            IShoppingML ml = new ShoppingML(_accessor, _env);
            var res = await ml.ChangeQuantity(ItemID, Quantity);
            return Json(res);
        }

        [HttpPost]
        [Route("/ItemCountinCart")]
        public IActionResult ItemCountinCart()
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var data = mL.ItemInCart();
            return Json(data);
        }

        [HttpPost]
        [Route("/ProceedToPay")]
        public IActionResult ProceedToPay()
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var data = mL.ProceedToPay();
            data.CartDetails = mL.CartDetail().Result;
            return PartialView("Partial/_ProceedToPay", data);
        }

        [HttpPost]
        [Route("/OnChangeFilter")]
        public async Task<IActionResult> OnChangeFilter(int ProductID, int ProductDetailID, List<string> Filters)
        {
            IShoppingML ml = new ShoppingML(_accessor, _env);
            var res = await ml.OnChangeFilter(ProductID, ProductDetailID, Filters);

            var response = await ml.ProDescription(res.ProductDetailID);
            return PartialView("Partial/_ProductDetailForUser", response);
            //return Json(res);
        }
        #endregion

        #region Order

        [HttpPost]
        [Route("/PlaceOrder")]
        public async Task<IActionResult> PlaceOrder(PlaceOrder order)
        {
            IShoppingML ml = new ShoppingML(_accessor, _env);
            var res = await ml.PlaceOrder(order).ConfigureAwait(false);
            return Json(res);
        }

        [HttpGet]
        [Route("shopping/OrderList")]
        public IActionResult Orders()
        {
            return View();
        }

        [HttpPost]
        [Route("/_Orders")]
        public IActionResult _Orders(OrderModel model)
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var res = new OrderHistory
            {
                Orders = mL.GetOrderHistory(model),
                Role = _lr.RoleID
            };
            return PartialView("Partial/_Orders", res);
        }

        [HttpPost]
        [Route("/_OrderDetailList")]
        public IActionResult _OrderDetailList(int OrderId)
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var res = new OrderDetail
            {
                OrderList = mL.getOrderDetails(OrderId),
                Role = _lr.RoleID
            };
            return PartialView("Partial/_OrderDetailList", res);
        }

        [HttpPost]
        [Route("/ChangeOrderStatus")]
        public IActionResult ChangeOrderStatus(CommonReq req)
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var res = mL.ChangeOrderStatus(req);
            return Json(res);
        }

        [HttpPost]
        [Route("/ChangePartialOrderStatus")]
        public IActionResult ChangePartialOrderStatus(ChangeOrderStatus req)
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var res = mL.ChangePartialOrderStatus(req);
            return Json(res);
        }

        [HttpGet]
        [Route("shopping/OrderReport")]
        public IActionResult OrderReport()
        {
            IShoppingML ml = new ShoppingML(_accessor, _env);
            var res = new OrderReport
            {
                Category = ml.GetShoppingMainCategoryNew(),
                Vendors = ml.GetVendors(),
                Role = _lr.RoleID
            };
            return View(res);
        }

        [HttpPost]
        [Route("/_OrderReport")]
        public IActionResult _OrderReport(OrderModel req)
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var res = new OrderReportModel
            {
                Role = _lr.RoleID,
                OrderReport = mL.getOrderReport(req)
            };
            return PartialView("Partial/_OrderReport", res);
        }
        [HttpGet]
        [Route("_OrderReport")]
        public IActionResult _OrderReportExport(OrderModel req)
        {
            ViewBag.RoleID = _lr.RoleID;
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var listItem = new List<OrderReport>();
            listItem = (List<OrderReport>)mL.getOrderReport(req);
            var dataTable = ConverterHelper.O.ToDataTable(listItem);
            dataTable.Columns.Remove("Statuscode");
            dataTable.Columns.Remove("Msg");
            dataTable.Columns.Remove("ProductDetailID");
            dataTable.Columns.Remove("ProductID");
            dataTable.Columns.Remove("ProductImage");
            dataTable.Columns.Remove("OrderStatusID");
            dataTable.Columns.Remove("Role");
            dataTable.Columns.Remove("Category");
            dataTable.Columns.Remove("OrderDetailID");
            dataTable.Columns.Remove("UserId");
            dataTable.Columns.Remove("UserName");
            dataTable.Columns.Remove("Address");
            dataTable.Columns.Remove("DebitAmount");
            dataTable.Columns.Remove("SDeduction");
            dataTable.Columns.Remove("Vendors");
            //dataTable.Columns.Remove("IsVendorPaid");
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("OrderReport1");
                worksheet.Cells["A1"].LoadFromDataTable(dataTable, PrintHeaders: true);
                int rowindex = 2;
                foreach (DataRow row in dataTable.Rows)
                {
                    if (rowindex == 2)
                    {
                        worksheet.Cells[1, dataTable.Columns["OrderID"].Ordinal + 1].Value = "OrderNo";
                        worksheet.Cells[1, dataTable.Columns["RequestAmount"].Ordinal + 1].Value = "Amount";
                        worksheet.Cells[1, dataTable.Columns["UserCommAmount"].Ordinal + 1].Value = "Comm";
                        worksheet.Cells[1, dataTable.Columns["PDeduction"].Ordinal + 1].Value = "Debit Amount";
                    }
                    rowindex++;
                }
                for (var col = 1; col < dataTable.Columns.Count + 1; col++)
                {
                    worksheet.Column(col).AutoFit();
                }
                var exportToExcel = new ExportToExcel
                {
                    Contents = package.GetAsByteArray(),
                    FileName = "OrderReport.xlsx"
                };
                return File(exportToExcel.Contents, DOCType.XlsxContentType, exportToExcel.FileName);
            }
        }
        [HttpGet]
        [Route("shopping/OrderReportVendor")]
        public IActionResult OrderReportVendor()
        {
            IShoppingML ml = new ShoppingML(_accessor, _env);
            var res = new OrderReport
            {
                Category = ml.GetShoppingMainCategoryNew(),
                Vendors = ml.GetVendors(),
                Role = _lr.RoleID
            };
            return View(res);
        }

        [HttpPost]
        [Route("/_OrderReportVendor")]
        public IActionResult _OrderReportVendor(OrderModel req)
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var res = new OrderReportModel
            {
                Role = _lr.RoleID,
                OrderReport = mL.getOrderReport(req)
            };
            return PartialView("Partial/_OrderReportVendor", res);
        }

        #endregion

        [HttpPost]
        [Route("/getImageOptionWise")]
        public IActionResult getImageOptionWise(int ProductID, int ProductDetailID)
        {
            List<string> files = new List<string>();
            try
            {
                string path = DOCType.ProductImagePath.Replace("{0}", ProductID.ToString());
                if (Directory.Exists(path))
                {
                    DirectoryInfo d = new DirectoryInfo(path);
                    FileInfo[] Files = d.GetFiles(ProductDetailID + "_*-1x.png");
                    foreach (FileInfo file in Files)
                    {
                        files.Add(ProductID.ToString() + "/" + file.Name);
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return Json(files);
        }

        [HttpPost]
        [Route("/_ShippingAddress")]
        public IActionResult _ShippingAddress()
        {
            IShoppingML ML = new ShoppingML(_accessor, _env);
            var res = new ShippingAddressModal
            {
                States = ML.States(),
                Cities = ML.Cities(0)
            };
            return PartialView("Partial/_ShippingAddress", res);
        }

        [HttpPost]
        [Route("/SaveShippingAddress")]
        public IActionResult SaveShippingAddress(SAddress param)
        {
            IShoppingML ML = new ShoppingML(_accessor, _env);
            var res = ML.AddShippingAddress(param);
            return Json(res);
        }

        [HttpGet]
        [Route("ShippingDetailReceipt")]
        public IActionResult ShippingDetailReceipt()
        {
            return View();
        }

        [HttpPost]
        [Route("Home/ShippingDetail-Receipt")]
        [Route("ShippingDetail-Receipt")]
        public IActionResult _ShippingDetailReceipt(int ID, bool IsPrint)
        {
            IShoppingML operation = new ShoppingML(_accessor, _env);
            ShoppingShipping Shipping = new ShoppingShipping();
            if (ID > 0)
            {
                Shipping = operation.GetShippingAddress(ID);
            }
            if (!IsPrint)
            {
                return PartialView("Partial/_ShoppingShipping", Shipping);
            }
            else
            {
                return PartialView("Partial/_ShoppingShippingPrint", Shipping);
            }
        }

        [HttpPost]
        [Route("/AddToWishList")]
        public IActionResult AddToWishList(int ProductDetailID)
        {
            IShoppingML ml = new ShoppingML(_accessor, _env);
            var res = ml.AddToWishList(ProductDetailID, _lr.UserID);
            return Json(res);
        }

        [HttpPost]
        [Route("/StockUpdation")]
        public IActionResult StockUpdation(int ProductDetailID, int Quantity, string Remark)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "You are not permited for this user"
            };
            if (_lr.RoleID != Role.Retailor_Seller)
            {
                IShoppingML ml = new ShoppingML(_accessor, _env);
                res = ml.StockUpdation(ProductDetailID, Quantity, Remark);
            }
            return Json(res);
        }

        #region DeliveryPersonnel

        [HttpGet]
        [Route("/shopping/DeliveryPersonnel")]
        public IActionResult DeliveryPersonnel()
        {
            if (_lr.RoleID == Role.Admin && LoginType.ApplicationUser == _lr.LoginTypeID && ApplicationSetting.IsECommerceAllowed && ApplicationSetting.IsECommDeliveryAllowed)
                return View();
            return Ok();
        }

        [HttpPost]
        [Route("/_DeliveryPersonnel")]
        public IActionResult _DeliveryPersonnel()
        {
            IDeliveryML mL = new DeliveryML(_accessor, _env);
            var res = mL.GetDeliveryPersonnelList(false);
            return PartialView("Partial/_DeliveryPersonnel", res);
        }

        [HttpPost]
        [Route("/PersonnelById")]
        public IActionResult GetDeliveryPersonnelById(int id)
        {
            IDeliveryML mL = new DeliveryML(_accessor, _env);
            var res = mL.GetDeliveryPersonnelById(id);
            return PartialView("Partial/_AUDeliveryPersonnel", res);
        }

        [HttpPost]
        [Route("/GetDPLocationHistory")]
        public IActionResult GetDPLocationHistory(int id)
        {
            IDeliveryML mL = new DeliveryML(_accessor, _env);
            var res = mL.GetDPLocationHistory(id);
            return PartialView("Partial/_DPLocationHistory", res);
        }

        [HttpPost]
        [Route("/AUDeliveryPersonnel")]
        public IActionResult AUDeliveryPersonnel(AUDeliverPersonnel req)//CommonReq
        {
            IDeliveryML mL = new DeliveryML(_accessor, _env);
            var res = mL.AUDeliveryPersonnel(req);
            return Json(res);
        }

        [HttpPost]
        [Route("/DeliveryPersonnelStatus")]
        public IActionResult DeliveryPersonnelStatus(AUDeliverPersonnel req)//CommonReq
        {
            IDeliveryML mL = new DeliveryML(_accessor, _env);
            var res = mL.DeliveryPersonnelStatus(req);
            return Json(res);
        }

        [HttpGet]
        [Route("/OrderDelivery")]
        public IActionResult OrderDelivery()
        {
            if (_lr.RoleID == Role.Admin && LoginType.ApplicationUser == _lr.LoginTypeID && ApplicationSetting.IsECommerceAllowed && ApplicationSetting.IsECommDeliveryAllowed)
                return View();
            return Ok();
        }

        [HttpPost]
        [Route("/_OrderDelivery")]
        public IActionResult _OrderDelivery(int id)
        {
            IDeliveryML mL = new DeliveryML(_accessor, _env);
            var res = mL.GetOrderDeliveryList(0, 0);
            return PartialView("Partial/_OrderDelivery", res);
        }
        #endregion

        #region Product Main Category by Noman
        [HttpGet]
        [Route("/shopping/Maincategory")]
        public IActionResult MainCategory()
        {
            if (_lr.RoleID == Role.Admin && LoginType.ApplicationUser == _lr.LoginTypeID && ApplicationSetting.IsECommerceAllowed)
                return View();
            return Ok();
        }

        [HttpPost]
        [Route("/_MainCategory")]
        public IActionResult _MainCategory()
        {
            try
            {
                IShoppingML mL = new ShoppingML(_accessor, _env);
                var res = mL.GetShoppingMainCategoryNew();
                return PartialView("Partial/_MainCategory", res);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
            }
        }



        [HttpPost]
        [Route("/AddMainCategory")]
        public IActionResult AddMainCategory(int id)
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var res = mL.GetShoppingMainCategoryByIDNew(id);
            return PartialView("Partial/_AddMainCategory", res);
        }
        [HttpPost]
        [Route("/updateMainCategory")]
        public IActionResult updateMainCategory(List<IFormFile> file, string ImgName, string detail)//CommonReq
        {
            var res = new ResponseStatus();
            try
            {
                var categoryData = JsonConvert.DeserializeObject<CommonReq>(detail);
                IShoppingML mL = new ShoppingML(_accessor, _env);
                var validateResponse = new ResponseStatus();
                if (file != null)
                {
                    foreach (var f in file)
                    {
                        validateResponse = Validate.O.IsImageValid(f);
                        if (validateResponse.Statuscode == ErrorCodes.Minus1)
                        {
                            break;
                        }
                    }
                    if (validateResponse.Statuscode == ErrorCodes.One)
                    {

                        List<string> files = new List<string>();
                        categoryData.CommonStr3 = string.Join(",", files);
                        res = mL.UpdateShoppingMainCategoryNew(categoryData);
                        for (int i = 0; i < file.Count; i++)
                        {
                            files.Add("MainCategory_" + i + 1 + ".png");
                            var _res = mL.UploadIcon(file[i], categoryData.CommonInt == 0 ? int.Parse(res.Msg) : categoryData.CommonInt, "Shopping");
                            if (_res.Statuscode == ErrorCodes.Minus1)
                            {
                                res.Statuscode = _res.Statuscode;
                                res.Msg = _res.Msg;
                                break;
                            }
                        }


                    }
                    else
                    {
                        res = mL.UpdateShoppingMainCategoryNew(categoryData);
                    }
                }
                else
                {
                    res.Statuscode = ErrorCodes.Minus1;
                    res.Msg = "No Image Found";
                }
            }
            catch (Exception ex)
            {
                res.Statuscode = ErrorCodes.Minus1;
                res.Msg = ex.Message;
            }

            return Json(res);
        }
        #endregion

        #region Product Category by Noman

        [HttpPost]
        [Route("/AddCategoryNew")]
        public IActionResult AddCategoryNew(int id, int cid)
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var res = mL.GetShoppingCategoryByIDNew(id);
            res.mainCategoryID = res.mainCategoryID == 0 ? cid : res.mainCategoryID;
            return PartialView("Partial/_AddCategoryLvl1", res);
        }

        [HttpPost]
        [Route("/updateCategoryNew")]
        public IActionResult updateCategoryNew(List<IFormFile> file, string ImgName, string detail)
        {
            var res = new ResponseStatus();
            try
            {
                var categoryData = JsonConvert.DeserializeObject<CommonReq>(detail);
                IShoppingML mL = new ShoppingML(_accessor, _env);
                var validateResponse = new ResponseStatus();
                if (file != null)
                {
                    foreach (var f in file)
                    {
                        validateResponse = Validate.O.IsImageValid(f);
                        if (validateResponse.Statuscode == ErrorCodes.Minus1)
                        {
                            break;
                        }
                    }
                    if (validateResponse.Statuscode == ErrorCodes.One)
                    {
                        List<string> files = new List<string>();
                        categoryData.CommonStr3 = string.Join(",", files);
                        res = mL.UpdateShoppingCategoryNew(categoryData);

                        for (int i = 0; i < file.Count; i++)
                        {
                            files.Add("Category_" + i + 1 + ".png");
                            var _res = mL.UploadIcon(file[i], categoryData.CommonInt == 0 ? int.Parse(string.IsNullOrEmpty(res.Msg) ? "0" : res.Msg) : categoryData.CommonInt, "Shopping", "S1");
                            if (_res.Statuscode == ErrorCodes.Minus1)
                            {
                                res.Statuscode = _res.Statuscode;
                                res.Msg = _res.Msg;
                                break;
                            }
                        }
                    }
                    else
                    {
                        //res = validateResponse;
                        res = mL.UpdateShoppingCategoryNew(categoryData);
                    }
                }
                else
                {
                    res.Statuscode = ErrorCodes.Minus1;
                    res.Msg = "No Image Found";
                }
            }
            catch (Exception ex)
            {
                res.Statuscode = ErrorCodes.Minus1;
                res.Msg = ex.Message;
            }

            return Json(res);
            //IShoppingML mL = new ShoppingML(_accessor, _env);
            //var res = mL.UpdateShoppingSubCategoryLvl1(req);
            //return Json(res);
        }

        [HttpPost]
        [Route("/_Category")]
        public IActionResult _Category(int cid)
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var list = mL.GetCategory(cid);
            var response = new CategoryModelLvl1
            {
                CategoryID = cid,
                Level1 = list
            };
            return PartialView("Partial/_CategoryLvl1New", response);
        }

        [HttpPost]
        [Route("/List-Category")]
        public IActionResult ListCategory(int cid)
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var list = mL.GetCategory(cid);
            return Json(list);

        }

        #endregion

        #region Product Sub Category by Noman

        [HttpPost]
        [Route("/updatesubCategoryNew")]
        public IActionResult updatesubCategoryNew(List<IFormFile> file, string ImgName, string detail)
        {
            var res = new ResponseStatus();
            try
            {
                var validateResponse = new ResponseStatus();
                var categoryData = JsonConvert.DeserializeObject<CommonReq>(detail);
                IShoppingML mL = new ShoppingML(_accessor, _env);
                if (file != null)
                {
                    foreach (var f in file)
                    {
                        validateResponse = Validate.O.IsImageValid(f);
                        if (validateResponse.Statuscode == ErrorCodes.Minus1)
                        {
                            break;
                        }
                    }
                    if (validateResponse.Statuscode == ErrorCodes.One)
                    {
                        List<string> files = new List<string>();
                        categoryData.CommonStr3 = string.Join(",", files);
                        res = mL.UpdateSubCategoryNew(categoryData);
                        if (res.Statuscode == ErrorCodes.One)
                        {

                            for (int i = 0; i < file.Count; i++)
                            {

                                var _res = mL.UploadIcon(file[i], categoryData.CommonInt == 0 ? int.Parse(string.IsNullOrEmpty(res.Msg) ? "0" : res.Msg) : categoryData.CommonInt, "Shopping", "S1");
                                if (_res.Statuscode == ErrorCodes.Minus1)
                                {
                                    res.Statuscode = _res.Statuscode;
                                    res.Msg = _res.Msg;
                                    break;
                                }
                            }

                        }
                    }
                    else
                    {
                        //res = validateResponse;
                        res = mL.UpdateSubCategoryNew(categoryData);
                    }
                }
                else
                {
                    res.Statuscode = ErrorCodes.Minus1;
                    res.Msg = "No Image Found";
                }
            }
            catch (Exception ex)
            {
                res.Statuscode = ErrorCodes.Minus1;
                res.Msg = ex.Message;
            }

            return Json(res);
        }

        [HttpPost]
        [Route("/AddSubCategoryNew")]
        public IActionResult AddSubCategoryNew(int id, int sid)
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var res = mL.GetShoppingCategoryByIDNew(id);
            // res.ParentId = id == 0 ? sid : res.ParentId;
            res.ParentId = sid;
            return PartialView("Partial/_AddSubCategoryNew", res);
        }

        [HttpPost]
        [Route("/_SubCategoryNew")]
        public IActionResult _SubCategoryNew(int pid)
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var list = mL.GetSubCategoryNew(pid);
            var response = new SubCategoryModelLvl2New
            {
                CategoryID = pid,
                subCategoryList = list
            };
            return PartialView("Partial/_SubCategoryNew", response);
        }
        [HttpPost]
        [Route("/List-SubCategoryNew")]
        public IActionResult ListSubCategoryNew(int sid)
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var list = mL.GetSubCategoryNew(sid);
            return Json(list);
        }

        #endregion
    }
}
