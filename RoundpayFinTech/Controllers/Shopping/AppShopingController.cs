using System;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Mvc;
using RoundpayFinTech.AppCode.Configuration;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.Shopping;
using RoundpayFinTech.AppCode.Model.Shopping.WebShopping;
using RoundpayFinTech.AppCode.Model.Shopping.WebShopping.ViewModel;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using RoundpayFinTech.AppCode.Model.App;

namespace RoundpayFinTech.Controllers
{
    public partial class AppController : IAppControllerShoping
    {
        [HttpPost]
        public async Task<IActionResult> GetShoppingMenu([FromBody] AppRequest appRequest)
        {
            var Response = new ShoppingMenuResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            //var appResp = appML.CheckApp(appRequest);
            //Response.IsAppValid = appResp.IsAppValid;
            //Response.IsVersionValid = appResp.IsVersionValid;
            try
            {
                if (ApplicationSetting.IsECommerceAllowed)
                {
                    IShoppingML ml = new ShoppingML(_accessor, _env, false);
                    Response.DefaultMenuLevel = ml.GetShoppingSettings().DefaultMenuLevel;
                    Response.GetShoppingMenu = ml.GetShoppingMenu();
                    Response.Statuscode = ErrorCodes.One;
                    Response.Msg = ErrorCodes.SUCCESS;
                }
            }
            catch (Exception ex)
            {

            }
            return Json(Response);
        }

        [HttpPost]
        public async Task<IActionResult> GetProducts([FromBody] ProductFilterRequest appRequest)
        {
            var Response = new ProductResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            var appResp = appML.CheckApp(appRequest);
            Response.IsAppValid = appResp.IsAppValid;
            Response.IsVersionValid = appResp.IsVersionValid;
            try
            {
                if (ApplicationSetting.IsECommerceAllowed)
                {
                    var req = new ProductFilter
                    {
                        UserId = appRequest.UserID,
                        LoginTypeId = appRequest.LoginTypeID,
                        CategoryID = appRequest.CategoryID,
                        SubCategoryID1 = appRequest.SubCategoryID1,
                        SubCategoryID2 = appRequest.SubCategoryID2,
                        Filters = appRequest.Filters,
                        BrandIDs = appRequest.BrandIDs
                    };
                    IEnumerable<ProductDetail> res = null;
                    IFintechShoppingML ml = new FintechShoppingML(_accessor, _env, false);
                    if ((req.Filters != null && req.Filters.Count > 0) || !string.IsNullOrEmpty(req.BrandIDs))
                    {
                        res = ml.GetFilteredProduct(req);
                    }
                    else
                    {
                        res = ml.GetProductForIndex(req);
                    }

                    var productList = res.Select(x => new ProductDetailForApp()
                    {
                        ProductID = x.ProductID,
                        ProductName = x.ProductName,
                        ProductDetailID = x.ProductDetailID,
                        ProductCode = x.ProductCode,
                        Batch = x.Batch,
                        VendorName = x.VendorName,
                        BrandName = x.BrandName,
                        MRP = x.MRP,
                        SellingPrice = x.SellingPrice,
                        Discount = x.Discount,
                        DiscountType = x.DiscountType,
                        ShippingCharges = x.ShippingCharges,
                        Quantity = x.Quantity,
                        ImgUrl = x.ImgUrl,
                        AdditionalTitle = x.AdditionalTitle
                    });

                    Response.Products = productList;
                    Response.Statuscode = ErrorCodes.One;
                    Response.Msg = ErrorCodes.SUCCESS;
                }
            }
            catch (Exception ex)
            {

            }
            return Json(Response);
        }

        [HttpPost]
        public async Task<IActionResult> GetTrendingProducts([FromBody] AppSessionReq appRequest)
        {
            var Response = new ProductResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            var appResp = appML.CheckApp(appRequest);
            Response.IsAppValid = appResp.IsAppValid;
            Response.IsVersionValid = appResp.IsVersionValid;
            try
            {
                if (ApplicationSetting.IsECommerceAllowed)
                {
                    var req = new ProductFilter
                    {
                        UserId = appRequest.UserID,
                        LoginTypeId = appRequest.LoginTypeID,
                        CategoryID = 0,
                        SubCategoryID1 = 0,
                        SubCategoryID2 = 0
                    };
                    IFintechShoppingML ml = new FintechShoppingML(_accessor, _env, false);
                    var res = ml.GetProductTrending(req);
                    var productList = res.Select(x => new ProductDetailForApp()
                    {
                        ProductID = x.ProductID,
                        ProductName = x.ProductName,
                        ProductDetailID = x.ProductDetailID,
                        ProductCode = x.ProductCode,
                        Batch = x.Batch,
                        VendorName = x.VendorName,
                        BrandName = x.BrandName,
                        MRP = x.MRP,
                        SellingPrice = x.SellingPrice,
                        Discount = x.Discount,
                        DiscountType = x.DiscountType,
                        ShippingCharges = x.ShippingCharges,
                        Quantity = x.Quantity,
                        ImgUrl = x.ImgUrl,
                        AdditionalTitle = x.AdditionalTitle
                    });

                    Response.Products = productList;
                    Response.Statuscode = ErrorCodes.One;
                    Response.Msg = ErrorCodes.SUCCESS;
                }
            }
            catch (Exception ex)
            {

            }
            return Json(Response);
        }

        [HttpPost]
        public async Task<IActionResult> GetNewArrivalProducts([FromBody] AppSessionReq appRequest)
        {
            var Response = new ProductResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            var appResp = appML.CheckApp(appRequest);
            Response.IsAppValid = appResp.IsAppValid;
            Response.IsVersionValid = appResp.IsVersionValid;
            try
            {
                if (ApplicationSetting.IsECommerceAllowed)
                {
                    var req = new ProductFilter
                    {
                        UserId = appRequest.UserID,
                        LoginTypeId = appRequest.LoginTypeID,
                        CategoryID = 0,
                        SubCategoryID1 = 0,
                        SubCategoryID2 = 0
                    };
                    IFintechShoppingML ml = new FintechShoppingML(_accessor, _env, false);
                    var res = ml.GetProductNewArrival(req);
                    var productList = res.Select(x => new ProductDetailForApp()
                    {
                        ProductID = x.ProductID,
                        ProductName = x.ProductName,
                        ProductDetailID = x.ProductDetailID,
                        ProductCode = x.ProductCode,
                        Batch = x.Batch,
                        VendorName = x.VendorName,
                        BrandName = x.BrandName,
                        MRP = x.MRP,
                        SellingPrice = x.SellingPrice,
                        Discount = x.Discount,
                        DiscountType = x.DiscountType,
                        ShippingCharges = x.ShippingCharges,
                        Quantity = x.Quantity,
                        ImgUrl = x.ImgUrl,
                        AdditionalTitle = x.AdditionalTitle
                    });

                    Response.Products = productList;
                    Response.Statuscode = ErrorCodes.One;
                    Response.Msg = ErrorCodes.SUCCESS;
                }
            }
            catch (Exception ex)
            {

            }
            return Json(Response);
        }

        [HttpPost]
        public async Task<IActionResult> GetFilteredProducts([FromBody] ExtendProductFilterRequest appRequest)
        {
            var Response = new ProductResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            var appResp = appML.CheckApp(appRequest);
            Response.IsAppValid = appResp.IsAppValid;
            Response.IsVersionValid = appResp.IsVersionValid;
            try
            {
                if (ApplicationSetting.IsECommerceAllowed)
                {
                    var req = new ProductFilter
                    {
                        UserId = appRequest.UserID,
                        LoginTypeId = appRequest.LoginTypeID,
                        CategoryID = appRequest.CategoryID,
                        SubCategoryID1 = appRequest.SubCategoryID1,
                        SubCategoryID2 = appRequest.SubCategoryID2,
                        FilterIds = appRequest.FilterIds,
                        OptionIds = appRequest.OptionIds
                    };
                    IShoppingML ml = new ShoppingML(_accessor, _env, false);
                    var res = ml.GetProductForIndex(req);
                    var productList = res.Select(x => new ProductDetailForApp()
                    {
                        ProductID = x.ProductID,
                        ProductName = x.ProductName,
                        ProductDetailID = x.ProductDetailID,
                        ProductCode = x.ProductCode,
                        Batch = x.Batch,
                        VendorName = x.VendorName,
                        BrandName = x.BrandName,
                        MRP = x.MRP,
                        SellingPrice = x.SellingPrice,
                        Discount = x.Discount,
                        DiscountType = x.DiscountType,
                        ShippingCharges = x.ShippingCharges,
                        Quantity = x.Quantity
                    });

                    Response.Products = productList;
                    Response.Statuscode = ErrorCodes.One;
                    Response.Msg = ErrorCodes.SUCCESS;
                }
            }
            catch (Exception ex)
            {

            }
            return Json(Response);
        }

        [HttpPost]
        public async Task<IActionResult> ProductDescription([FromBody] ProductDescriptionRequest appRequest)
        {
            var Response = new ProductDescriptionResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            if (ApplicationSetting.IsECommerceAllowed)
            {
                var appResp = appML.CheckApp(appRequest);
                Response.IsAppValid = appResp.IsAppValid;
                Response.IsVersionValid = appResp.IsVersionValid;
                try
                {
                    IFintechShoppingML ml = new FintechShoppingML(_accessor, _env, false);
                    var result = (ProductDetailForUser)await ml.ProDescription(appRequest.ProductDetailID, appRequest.UserID).ConfigureAwait(false);
                    Response.ProductID = result.ProductID;
                    Response.ProductDetailID = result.ProductDetailID;
                    Response.ProductName = result.ProductName;
                    Response.Quantity = result.Quantity;
                    Response.MRP = result.MRP;
                    Response.Discount = result.Discount;
                    Response.DiscountType = result.DiscountType;
                    Response.SellingPrice = result.SellingPrice;
                    Response.ShippingCharges = result.ShippingCharges;
                    Response.Specification = result.Specification;
                    Response.Discription = result.Discription;
                    Response.CommonDiscription = result.CommonDiscription;
                    Response.Files = result.Files;
                    Response.FilterDetail = result.FilterDetail;
                    Response.AdditionalTitle = result.AdditionalTitle;
                    Response.Statuscode = ErrorCodes.One;
                    Response.Msg = ErrorCodes.SUCCESS;
                }
                catch (Exception ex)
                {

                }
            }
            return Json(Response);
        }


        [HttpPost]
        public async Task<IActionResult> OnChnageFilter([FromBody] OnFilterChnageRequest appRequest)
        {
            var Response = new ProductDescriptionResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            if (ApplicationSetting.IsECommerceAllowed)
            {
                var appResp = appML.CheckApp(appRequest);
                Response.IsAppValid = appResp.IsAppValid;
                Response.IsVersionValid = appResp.IsVersionValid;
                try
                {
                    IShoppingML ml = new ShoppingML(_accessor, _env, false);
                    var result = (ProductDetailForUser)await ml.OnChangeFilterForApp(appRequest.ProductID, appRequest.ProductDetailID, appRequest.Filters).ConfigureAwait(false);
                    Response.ProductID = result.ProductID;
                    Response.ProductDetailID = result.ProductDetailID;
                    Response.ProductName = result.ProductName;
                    Response.Quantity = result.Quantity;
                    Response.MRP = result.MRP;
                    Response.Discount = result.Discount;
                    Response.DiscountType = result.DiscountType;
                    Response.SellingPrice = result.SellingPrice;
                    Response.ShippingCharges = result.ShippingCharges;
                    Response.Specification = result.Specification;
                    Response.Discription = result.Discription;
                    Response.CommonDiscription = result.CommonDiscription;
                    Response.Files = result.Files;
                    Response.FilterDetail = result.FilterDetail;
                    Response.Statuscode = ErrorCodes.One;
                    Response.Msg = ErrorCodes.SUCCESS;
                }
                catch (Exception ex)
                {

                }
            }
            return Json(Response);
        }


        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartReq appRequest)
        {
            var Response = new AddToCartResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            if (ApplicationSetting.IsECommerceAllowed)
            {
                var appResp = new AppResponse();
                if (!appRequest.IsB2C)
                    appResp = appML.CheckAppSession(appRequest);
                else
                    appResp = appML.CheckWebAppSession(appRequest);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired)
                    {
                        IShoppingML ml = new ShoppingML(_accessor, _env);
                        var res = (ResponseStatus)await ml.AddToCart(appRequest.ProductDetailID, appRequest.Quantity, appRequest.UserID).ConfigureAwait(false);
                        Response.Statuscode = res.Statuscode;
                        Response.Msg = res.Msg;
                        Response.TotalItem = res.CommonInt;
                    }
                }
            }
            return Json(Response);
        }

        [HttpPost]
        public async Task<IActionResult> CartDetail([FromBody] AppSessionReq appRequest)
        {
            var Response = new CartDetailResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            if (ApplicationSetting.IsECommerceAllowed)
            {
                var appResp = appML.CheckAppSession(appRequest);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired)
                    {
                        IShoppingML ml = new ShoppingML(_accessor, _env);
                        var res = await ml.CartDetail(appRequest.UserID).ConfigureAwait(false);
                        var CartDetails = res.Select(x => new CartDetailForApp()
                        {
                            ID = x.ID,
                            ProductID = x.ProductID,
                            ProductName = x.ProductName,
                            ProductCode = x.ProductCode,
                            ProductDetailID = x.ProductDetailID,
                            MRP = x.MRP,
                            SellingPrice = x.SellingPrice,
                            ImgUrl = x.ImgUrl,
                            Discount = x.Discount,
                            DiscountType = x.DiscountType,
                            Quantity = x.Quantity,
                            Batch = x.Batch
                        });
                        Response.CartDetail = CartDetails;
                        Response.Statuscode = ErrorCodes.One;
                        Response.Msg = ErrorCodes.SUCCESS;
                    }
                }
            }
            return Json(Response);
        }

        [HttpPost]
        public async Task<IActionResult> ItemInCart([FromBody] AppSessionReq appRequest)
        {
            var Response = new ItemInCartResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            if (ApplicationSetting.IsECommerceAllowed)
            {
                var appResp = appML.CheckAppSession(appRequest);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired)
                    {
                        IShoppingML ml = new ShoppingML(_accessor, _env);
                        var res = await Task.FromResult(ml.ItemInCart(appRequest.UserID)).ConfigureAwait(false);
                        Response.TQuantity = res.TQuantity;
                        Response.TCost = res.TCost;
                        Response.Statuscode = ErrorCodes.One;
                        Response.Msg = ErrorCodes.SUCCESS;
                    }
                }
            }
            return Json(Response);
        }

        [HttpPost]
        public async Task<IActionResult> ProceedToPay([FromBody] ProceedToPayReq appRequest)
        {
            var Response = new ProceedToPayResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            if (ApplicationSetting.IsECommerceAllowed)
            {
                var appResp = new AppResponse();
                if (!appRequest.IsB2C)
                    appResp = appML.CheckAppSession(appRequest);
                else
                    appResp = appML.CheckWebAppSession(appRequest);

                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired)
                    {
                        IShoppingML ml = new ShoppingML(_accessor, _env);
                        var res = await Task.FromResult(ml.ProceedToPay(appRequest.UserID)).ConfigureAwait(false);
                        Response.Quantity = res.Quantity;
                        Response.PrimaryDeductionPer = res.PrimaryDeductionPer;
                        Response.TotalCost = res.TotalSellingPrice;
                        Response.TotalMRP = res.TotalMRP;
                        Response.TotalDiscount = res.TotalDiscount;
                        Response.PDeduction = res.PDeduction;
                        Response.SDeduction = res.SDeduction;
                        Response.ShippingCharge = res.ShippingCharge;
                        Response.PWallet = res.PWallet;
                        Response.SWallet = res.SWallet;
                        Response.AddressList = res.Addresses != null ? res.Addresses.ToList() : null;
                        if (res.Addresses != null && res.Addresses.Any(x => x.IsDefault))
                        {
                            Response.Address = res.Addresses.Where(x => x.IsDefault).FirstOrDefault();
                        }
                        else
                        {
                            Response.Address = (res.Addresses != null) ? res.Addresses.FirstOrDefault() : null;
                        }
                        var cart = await ml.CartDetail(appRequest.UserID).ConfigureAwait(false);
                        var CartDetails = cart.Select(x => new CartDetailForApp()
                        {
                            ID = x.ID,
                            ProductID = x.ProductID,
                            ProductName = x.ProductName,
                            ProductCode = x.ProductCode,
                            ProductDetailID = x.ProductDetailID,
                            MRP = x.MRP,
                            SellingPrice = x.SellingPrice,
                            ImgUrl = x.ImgUrl,
                            Discount = x.Discount,
                            DiscountType = x.DiscountType,
                            Quantity = x.Quantity,
                            Batch = x.Batch
                        });
                        Response.CartDetails = CartDetails;
                        Response.Statuscode = ErrorCodes.One;
                        Response.Msg = ErrorCodes.SUCCESS;
                    }
                }
            }
            return Json(Response);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart([FromBody] RemoveFromCartReq appRequest)
        {
            var Response = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            if (ApplicationSetting.IsECommerceAllowed)
            {
                var appResp = new AppResponse();
                if (!appRequest.IsB2C)
                    appResp = appML.CheckAppSession(appRequest);
                else
                    appResp = appML.CheckWebAppSession(appRequest);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired)
                    {
                        IShoppingML ml = new ShoppingML(_accessor, _env);
                        var result = (ResponseStatus)await ml.RemoveItemFromCart(appRequest.ID, appRequest.UserID).ConfigureAwait(false);
                        Response.Statuscode = result.Statuscode;
                        Response.Msg = result.Msg;
                    }
                }
            }
            return Json(Response);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeQuantity([FromBody] ChangeQuantityReq appRequest)
        {
            var Response = new ChangeQuantityResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            if (ApplicationSetting.IsECommerceAllowed)
            {
                var appResp = new AppResponse();
                if (!appRequest.IsB2C)
                    appResp = appML.CheckAppSession(appRequest);
                else
                    appResp = appML.CheckWebAppSession(appRequest);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired)
                    {
                        IShoppingML ml = new ShoppingML(_accessor, _env);
                        var result = await ml.ChangeQuantity(appRequest.ItemID, appRequest.Quantity, appRequest.UserID).ConfigureAwait(false);
                        Response.Statuscode = result.Statuscode;
                        Response.Msg = result.Msg;
                        Response.Cost = result.CommonStr;
                    }
                }
            }
            return Json(Response);
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderReq appRequest)
        {
            var Response = new PlaceOrderResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            if (ApplicationSetting.IsECommerceAllowed)
            {
                var appResp = new AppResponse();
                if (!appRequest.IsB2C)
                    appResp = appML.CheckAppSession(appRequest);
                else
                    appResp = appML.CheckWebAppSession(appRequest);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired)
                    {
                        IShoppingML ml = new ShoppingML(_accessor, _env);
                        var OrderDetail = new PlaceOrder
                        {
                            UserID = appRequest.UserID,
                            AddressID = appRequest.AddressID
                        };
                        var result = await ml.PlaceOrder(OrderDetail).ConfigureAwait(false);
                        Response.Statuscode = result.Statuscode;
                        Response.Msg = result.Msg;
                        if (!string.IsNullOrEmpty(result.CommonStr) && result.CommonStr != "")
                        {
                            Response.OrderId = result.CommonStr;
                        }
                    }
                }
            }
            return Json(Response);
        }

        [HttpPost]
        public async Task<IActionResult> Order([FromBody] OrderDetailReq appRequest)
        {
            var Response = new AppOrderResp
            {
                StatusCode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            if (ApplicationSetting.IsECommerceAllowed)
            {
                var appResp = new AppResponse();
                if (!appRequest.IsB2C)
                    appResp = appML.CheckAppSession(appRequest);
                else
                    appResp = appML.CheckWebAppSession(appRequest);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired)
                    {
                        IShoppingML ml = new ShoppingML(_accessor, _env);
                        var OrderDetail = new OrderModel();

                        var req = new CommonReq
                        {
                            LoginID = appRequest.UserID,
                            CommonInt = appRequest.OrderId
                        };
                        Response.Order = ml.AppOrderList(req);

                        if (Response.Order.Count() > 0 && Response.Order.FirstOrDefault().StatusCode == ErrorCodes.One)
                        {
                            Response.StatusCode = ErrorCodes.One;
                            Response.Msg = ErrorCodes.SUCCESS;
                        }
                        else
                        {
                            Response.StatusCode = 0;
                            Response.Msg = ErrorCodes.NODATA;
                        }
                        await Task.Delay(0).ConfigureAwait(false);
                    }
                }
            }
            return Json(Response);
        }
        [HttpPost]
        public async Task<IActionResult> OrderDetailList([FromBody] AppSessionReq appRequest)
        {
            var Response = new OrderDeatilResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            if (ApplicationSetting.IsECommerceAllowed)
            {
                var appResp = appML.CheckAppSession(appRequest);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired)
                    {
                        IShoppingML ml = new ShoppingML(_accessor, _env);
                        var OrderDetail = new OrderModel();

                        var req = new CommonReq
                        {
                            LoginID = appRequest.UserID,
                            CommonStr = OrderDetail.OrderDetailMode,
                        };
                        Response.OrderDetail = ml.GetOrderDetailList(req);
                        Response.Statuscode = ErrorCodes.One;
                        Response.Msg = ErrorCodes.SUCCESS;
                        await Task.Delay(0).ConfigureAwait(false);
                    }
                }
            }
            return Json(Response);
        }
        [HttpPost]
        public async Task<IActionResult> OrderDetail([FromBody] OrderDetailReq appRequest)
        {
            var Response = new OrderDeatilResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            if (ApplicationSetting.IsECommerceAllowed)
            {
                var appResp = appML.CheckAppSession(appRequest);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired)
                    {
                        IShoppingML ml = new ShoppingML(_accessor, _env);
                        Response.OrderDetail = ml.getOrderDetails(appRequest.OrderId, appRequest.UserID);
                        Response.Statuscode = ErrorCodes.One;
                        Response.Msg = ErrorCodes.SUCCESS;
                        await Task.Delay(0).ConfigureAwait(false);
                    }
                }
            }
            return Json(Response);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeOrderStatus([FromBody] ChangeOrderStatusReq appRequest)
        {
            var Response = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            if (ApplicationSetting.IsECommerceAllowed)
            {
                var appResp = appML.CheckAppSession(appRequest);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired)
                    {
                        var Req = new CommonReq
                        {
                            LoginID = appRequest.UserID,
                            LoginTypeID = LoginType.ApplicationUser,
                            CommonInt = appRequest.OrderId,
                            CommonInt2 = appRequest.Status
                        };
                        IShoppingML ml = new ShoppingML(_accessor, _env);
                        var result = (ResponseStatus)await Task.Run(() => ml.ChangeOrderStatus(Req)).ConfigureAwait(false);
                        Response.Statuscode = result.Statuscode;
                        Response.Msg = result.Msg;
                    }
                }
            }
            return Json(Response);
        }

        [HttpPost]
        public async Task<IActionResult> GetShippingAddresses([FromBody] GetShippingAddressesReq appRequest)
        {
            var Response = new ShippingAddressResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            if (ApplicationSetting.IsECommerceAllowed)
            {
                var appResp = new AppResponse();
                if (!appRequest.IsB2C)
                    appResp = appML.CheckAppSession(appRequest);
                else
                    appResp = appML.CheckWebAppSession(appRequest);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired)
                    {
                        IShoppingML ml = new ShoppingML(_accessor, _env);
                        Response.Addresses = ml.GetShippingAddresses(appRequest.UserID);
                        Response.Statuscode = ErrorCodes.One;
                        Response.Msg = ErrorCodes.SUCCESS;
                        await Task.Delay(0).ConfigureAwait(false);
                    }
                }
            }
            return Json(Response);
        }

        [HttpPost]
        public async Task<IActionResult> AddShippingAddress([FromBody] AddAddressRequest appRequest)
        {
            var Response = new AddShippingAddressRes
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            if (ApplicationSetting.IsECommerceAllowed)
            {
                var appResp = new AppResponse();
                if (!appRequest.IsB2C)
                    appResp = appML.CheckAppSession(appRequest);
                else
                    appResp = appML.CheckWebAppSession(appRequest);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired)
                    {
                        IShoppingML ml = new ShoppingML(_accessor, _env);
                        var req = new SAddress
                        {
                            ID = appRequest.ID,
                            Address = appRequest.Address,
                            LoginID = appRequest.UserID,
                            Area = appRequest.Area,
                            CityID = appRequest.CityID,
                            StateID = appRequest.StateID,
                            PIN = appRequest.PIN,
                            Landmark = appRequest.Landmark,
                            MobileNo = appRequest.MobileNo,
                            CustomerName = appRequest.CustomerName,
                            Title = appRequest.Title,
                            IsDefault = appRequest.IsDefault,
                            IsDeleted = appRequest.IsDeleted,
                            Latitude = appRequest.Latitude,
                            Longitude = appRequest.Longitude
                        };
                        var res = (ShippingAddress)await Task.FromResult(ml.AddShippingAddress(req));
                        Response.Id = res.ID;
                        Response.Statuscode = res.Statuscode;
                        Response.Msg = res.Msg;
                    }
                }
            }
            return Json(Response);
        }

        [HttpPost]
        public async Task<IActionResult> AllAvailableFilter([FromBody] AllAvailableFilterReq appRequest)
        {
            var Response = new AllAvailableFilterRes
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            if (ApplicationSetting.IsECommerceAllowed)
            {
                var appResp = appML.CheckAppSession(appRequest);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired)
                    {
                        IShoppingML ml = new ShoppingML(_accessor, _env);
                        Response.Filters = ml.GetRequeiredFilter(appRequest.S2ID, appRequest.S2ID != 0 ? 0 : appRequest.SID,appRequest.CID,"");
                        Response.Brands = ml.GetBrand(appRequest.CID);
                        Response.Statuscode = ErrorCodes.One;
                        Response.Msg = ErrorCodes.SUCCESS;
                        await Task.Delay(0).ConfigureAwait(false);
                    }
                }
            }
            return Json(Response);
        }

        [HttpPost]
        public async Task<IActionResult> GetStates([FromBody] GetStatesReq appRequest)
        {
            var Response = new GetStatesResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            var appResp = new AppResponse();
            if (!appRequest.IsB2C)
                appResp = appML.CheckAppSession(appRequest);
            else
                appResp = appML.CheckWebAppSession(appRequest);
            Response.IsAppValid = appResp.IsAppValid;
            Response.IsVersionValid = appResp.IsVersionValid;
            try
            {
                IShoppingML ml = new ShoppingML(_accessor, _env, false);
                Response.States = ml.States();
                Response.Statuscode = ErrorCodes.One;
                Response.Msg = ErrorCodes.SUCCESS;
            }
            catch (Exception ex)
            {

            }
            return Json(Response);
        }

        [HttpPost]
        public async Task<IActionResult> GetCities([FromBody] CityReq appRequest)
        {
            var Response = new GetCityResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            var appResp = new AppResponse();
            if (!appRequest.IsB2C)
                appResp = appML.CheckApp(appRequest);
            else
                appResp = appML.CheckWebApp(appRequest);
            Response.IsAppValid = appResp.IsAppValid;
            Response.IsVersionValid = appResp.IsVersionValid;
            try
            {
                IShoppingML ml = new ShoppingML(_accessor, _env, false);
                Response.Cities = ml.Cities(appRequest.StateID);
                Response.Statuscode = ErrorCodes.One;
                Response.Msg = ErrorCodes.SUCCESS;
            }
            catch (Exception ex)
            {

            }
            return Json(Response);
        }

        public async Task<IActionResult> GetAreabyPincodeRequest([FromBody] GetAreabyPincodeRequest appRequest)
        {
            var Response = new GetAreabyPincodeResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            try
            {
                var appResp = new AppResponse();
                if (!appRequest.IsB2C)
                    appResp = appML.CheckApp(appRequest);
                else
                    appResp = appML.CheckWebApp(appRequest);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    Response.IsVersionValid = true;
                    if (!appResp.IsPasswordExpired)
                    {
                        Response.IsPasswordExpired = false;
                        IShoppingML ml = new ShoppingML(_accessor, _env, false);
                        Response.Data = ml.GetAreaByPincode(appRequest.Pincode, appRequest.UserID, appRequest.LoginTypeID);
                        Response.Statuscode = ErrorCodes.One;
                        Response.Msg = ErrorCodes.SUCCESS;
                    }
                    else
                    {
                        Response.Msg = appResp.IsPasswordExpired ? "Password Expired" : "Permission denied";
                    }
                }
            }
            catch (Exception ex)
            {
                Response.Msg = ErrorCodes.AnError;
            }
            return Json(Response);
        }

        [HttpPost]
        public async Task<IActionResult> GetVendorLocation([FromBody] VendorLocationReq appRequest)
        {
            var Response = new VendorLocationResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            if (ApplicationSetting.IsECommerceAllowed)
            {
                var appResp = appML.CheckAppSession(appRequest);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired)
                    {
                        IShoppingML ml = new ShoppingML(_accessor, _env);
                        var res = ml.GetECommVendorLocation(appRequest.VendorId, appRequest.UserID);
                        Response.Statuscode = res.Status;
                        Response.Msg = res.Msg;
                        if (res.Status == ErrorCodes.One)
                        {
                            Response.VendorId = appRequest.VendorId;
                            Response.Latitude = res.CommonStr;
                            Response.Longitude = res.CommonStr2;
                        }
                        await Task.Delay(0).ConfigureAwait(false);
                    }
                }
            }
            return Json(Response);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateVendorLocation([FromBody] VendorLocationReq appRequest)
        {
            var Response = new VendorLocationResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            if (ApplicationSetting.IsECommerceAllowed)
            {
                var appResp = appML.CheckDeliveryAppSession(appRequest);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired)
                    {
                        IShoppingML ml = new ShoppingML(_accessor, _env);
                        var res = ml.UpdateECommVendorLocation(appRequest.VendorId, appRequest.Latitude, appRequest.Longitude, appRequest.UserID);
                        Response.Statuscode = res.Status;
                        Response.Msg = res.Msg;
                        await Task.Delay(0).ConfigureAwait(false);
                    }
                }
            }
            return Json(Response);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateDPStatusLocation([FromBody] DPLocationReq appRequest)
        {
            var Response = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            if (ApplicationSetting.IsECommerceAllowed && ApplicationSetting.IsECommDeliveryAllowed)
            {
                var appResp = appML.CheckDeliveryAppSession(appRequest);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired)
                    {
                        IDeliveryML ml = new DeliveryML(_accessor, _env);
                        var res = ml.UpdateDeliveryPersonnelStatusLocation(appRequest.UserID, appRequest.Status, appRequest.Latitude, appRequest.Longitude, appRequest.UserID, appRequest.OrderDetailId);
                        Response.Statuscode = res.Status;
                        Response.Msg = res.Msg;
                        await Task.Delay(0).ConfigureAwait(false);
                    }
                }
            }
            return Json(Response);
        }

        [HttpPost]
        public async Task<IActionResult> LoginDeliveryPersonnel([FromBody] LoginDeliveryPersonnelReq appRequest)
        {
            var Response = new LoginDeliveryPersonnelResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            if (ApplicationSetting.IsECommerceAllowed && ApplicationSetting.IsECommDeliveryAllowed)
            {
                var checkRequest = new AppRequest
                {
                    APPID = appRequest.APPID,
                    IMEI = appRequest.IMEI,
                    RegKey = appRequest.RegKey,
                    SerialNo = appRequest.SerialNo,
                    Version = appRequest.Version
                };
                var checkResp = appML.CheckApp(appRequest);
                Response.IsAppValid = checkResp.IsAppValid;
                Response.IsVersionValid = checkResp.IsVersionValid;
                if (checkResp.Statuscode == ErrorCodes.One)
                {
                    IDeliveryML ml = new DeliveryML(_accessor, _env);
                    var res = ml.LoginDeliveryPersonnel(appRequest);
                    Response.Statuscode = res.Status;
                    Response.Msg = res.Msg;
                    if(res.Status == ErrorCodes.One)
                    {
                        Response.UserID = res.ID;
                        Response.SessionID = res.SessionKey;
                        Response.SessID = res.SessId;
                        Response.CookieExpire = res.CookieExpire;
                        Response.ID = res.ID;
                        Response.Name = res.Name;
                        Response.Mobile = res.Mobile;
                        Response.VehicleNumber = res.VehicleNumber;
                    }
                    await Task.Delay(0).ConfigureAwait(false);
                }
                else
                {
                    Response.Msg = checkResp.Msg;
                }
            }
            return Json(Response);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateDPToken([FromBody] DPToken appRequest)
        {
            var Response = new AppResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            if (ApplicationSetting.IsECommerceAllowed && ApplicationSetting.IsECommDeliveryAllowed)
            {
                var appResp = appML.CheckDeliveryAppSession(appRequest);
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired)
                    {
                        IDeliveryML ml = new DeliveryML(_accessor, _env);
                        var res = ml.UpdateDeliveryPersonnelToken(appRequest);
                        Response.Statuscode = res.Status;
                        Response.Msg = res.Msg;
                        await Task.Delay(0).ConfigureAwait(false);
                    }
                }
            }
            return Json(Response);
        }

        [HttpPost]
        public async Task<IActionResult> GetOrderDetailForDelivery([FromBody] OrderDeliveryReq appRequest)
        {
            var Response = new AppOrderDeliveryResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            if (ApplicationSetting.IsECommerceAllowed && ApplicationSetting.IsECommDeliveryAllowed)
            {
                var appResp = appML.CheckDeliveryAppSession(appRequest);
                Response.IsAppValid = appResp.IsAppValid;
                Response.IsVersionValid = appResp.IsVersionValid;
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired)
                    {
                        IDeliveryML ml = new DeliveryML(_accessor, _env);
                        var res = ml.GetOrderDetailForDelivery(appRequest.UserID, appRequest.OrderDetailId);
                        Response.Statuscode = res.Status;
                        Response.Msg = res.Msg;
                        if(res.Status == ErrorCodes.One)
                        {
                            Response.Id = res.Id;
                            Response.OrderId = res.OrderId;
                            Response.OrderDetailId = res.OrderDetailId;
                            Response.ProductName = res.ProductName;
                            Response.Quantity = res.Quantity;
                            Response.TotalAmount = res.TotalAmount;
                            Response.CustomerAddressId = res.CustomerAddressId;
                            Response.CustomerName = res.CustomerName;
                            Response.CustomerAddress = res.CustomerAddress;
                            Response.CustomerPinCode = res.CustomerPinCode;
                            Response.CustomerMobile = res.CustomerMobile;
                            Response.CustomerArea = res.CustomerArea;
                            Response.CustomerLandmark = res.CustomerLandmark;
                            Response.CustomerLat = res.CustomerLat;
                            Response.CustomerLong = res.CustomerLong;
                            Response.VendorOutlet = res.VendorOutlet;
                            Response.VendorMobile = res.VendorMobile;
                            Response.VendorAddress = res.VendorAddress;
                            Response.VendorLat = res.VendorLat;
                            Response.VendorLong = res.VendorLong;
                            Response.DPId = res.DPId;
                            Response.DPLat = res.DPLat;
                            Response.DPLong = res.DPLong;
                            Response.IsPicked = res.IsPicked;
                            Response.IsDelivered = res.IsDelivered;
                        }
                        await Task.Delay(0).ConfigureAwait(false);
                    }
                }
            }
            return Json(Response);
        }

        [HttpPost]
        public async Task<IActionResult> GetDeliveryDashboard([FromBody] AppSessionReq appRequest)
        {
            var Response = new AppDeliveryDashBoardResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            if (ApplicationSetting.IsECommerceAllowed && ApplicationSetting.IsECommDeliveryAllowed)
            {
                var appResp = appML.CheckDeliveryAppSession(appRequest);
                Response.IsAppValid = appResp.IsAppValid;
                Response.IsVersionValid = appResp.IsVersionValid;
                if (appResp.Statuscode == ErrorCodes.One)
                {
                    if (!appResp.IsPasswordExpired)
                    {
                        appResp.IsPasswordExpired = false;
                        IDeliveryML ml = new DeliveryML(_accessor, _env);
                        var res = ml.GetDeliveryDashboard(appRequest.UserID, appRequest.LoginTypeID);
                        Response.Statuscode = res.Status;
                        Response.Msg = res.Msg;
                        if (res.Status == ErrorCodes.One)
                        {
                            Response.UserId = res.UserId;
                            Response.Name = res.Name;
                            Response.Mobile = res.Mobile;
                            Response.IsAssigned = res.IsAssigned;
                            Response.IsAvailable = res.IsAvailable;
                            Response.OrderList = res.OrderList;
                        }
                        await Task.Delay(0).ConfigureAwait(false);
                    }
                }
            }
            return Json(Response);
        }

        #region New Shopping APIs
        #region  Get Product Info App
        [HttpGet]
        [Route("app/productInfoApp")]
        public async Task<IActionResult> ProductInfoApp(string WebsiteId, int POSId, string LoginId = "")
        {
            var Response = new Response();
            IWebShoppingML ML = new WebShoppingML(_accessor, _env);
            try
            {
                int id = 1;
                if (ApplicationSetting.IsECommerceAllowed)
                {
                    Response.Id = id;
                    Response.Status = Convert.ToBoolean(ErrorCodes.One);
                    Response.Message = ErrorCodes.SUCCESS;
                    Response.Data = ML.GetAllProductInfo(POSId, LoginId).Result;
                }
            }
            catch (Exception ex)
            {

            }
            return Json(Response);
        }
        #endregion
        #endregion





    }
}
