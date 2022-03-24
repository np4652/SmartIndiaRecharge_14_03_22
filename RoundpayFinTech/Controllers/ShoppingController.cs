using Fintech.AppCode.Configuration;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RoundpayFinTech.AppCode.Configuration;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.Shopping;
using System;
using System.Linq;
using System.Threading.Tasks;
using Validators;

namespace RoundpayFinTech.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ShoppingController : Controller
    {
        #region GLobal Variable
        protected readonly IHttpContextAccessor _accessor;
        protected readonly IHostingEnvironment _env;
        protected readonly ISession _session;
        protected readonly LoginResponse _lr;
        #endregion

        public ShoppingController(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            _session = _accessor.HttpContext.Session;
            _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.ECommLoginResponse);
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.RouteData.Values["Action"].ToString() == "Index")
            {
                IFintechShoppingML mL = new FintechShoppingML(_accessor, _env);
                var res = mL.IsShoppingDomain();
                if (!res)
                {
                    context.Result = new RedirectResult("/Error404");
                }
            }
        }

        public IActionResult Index()
        {
            if (ApplicationSetting.IsECommerceAllowed)
            {
                return View();
            }
            return Ok();
        }

        [Route("Error404")]
        public IActionResult PageNotFound()
        {
            //return PageNotFound();
            return View();
        }

        [Route("/EcommLogin")]
        public IActionResult EcommLogin()
        {
            if (ApplicationSetting.IsECommerceAllowed)
                return PartialView("Partial/_Login");
            return Ok();
        }

        [HttpPost]
        [Route("/ECLogin")]
        public IActionResult LoginCheck([FromBody] LoginDetail loginDetail)
        {
            IResponseStatus responseStatus = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidLogin
            };
            if (loginDetail == null)
            {
                return Json(responseStatus);
            }
            else if (!Validate.O.IsMobile(loginDetail.LoginMobile))
            {
                loginDetail.Prefix = Validate.O.Prefix(loginDetail.LoginMobile);
                if (Validate.O.IsNumeric(loginDetail.Prefix))
                    return Json(responseStatus);
                string loginID = Validate.O.LoginID(loginDetail.LoginMobile);
                if (!Validate.O.IsNumeric(loginID))
                {
                    return Json(responseStatus);
                }
                loginDetail.LoginID = Convert.ToInt32(loginID);
                loginDetail.LoginMobile = "";
            }
            loginDetail.RequestMode = RequestMode.PANEL;
            loginDetail.Password = HashEncryption.O.Encrypt(loginDetail.Password);
            IFintechShoppingML mL = new FintechShoppingML(_accessor, _env);
            responseStatus = mL.DoLogin(loginDetail);
            return Json(new { responseStatus.Statuscode, responseStatus.Msg, Path = responseStatus.CommonStr });
        }

        [HttpPost]
        [Route("/ECommRedirectLogin")]
        public IActionResult RedirectLogin(int WId, string SId, int UId)
        {
            IResponseStatus responseStatus = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildRoute
            };
            if (ApplicationSetting.IsECommerceAllowed && WId > 0 && UId > 0 && !string.IsNullOrEmpty(SId))
            {
                IFintechShoppingML mL = new FintechShoppingML(_accessor, _env);
                responseStatus = mL.RedirectLoginCheck(WId, SId, UId);
                if (responseStatus.Statuscode == ErrorCodes.One)
                {
                    return View("Index");
                }
            }
            return Ok();
        }

        [HttpPost]
        [Route("ECLogout")]
        public async Task<IActionResult> Logout(int ULT, int UserID, int SType)
        {
            IResponseStatus responseStatus = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            IFintechShoppingML ml = new FintechShoppingML(_accessor, _env);
            responseStatus = await ml.DoLogout(ULT, UserID, SType);
            if (ClearCurrentSession())
            {
                return Json(responseStatus);
            }
            else
            {
                return Json(responseStatus);
            }
            return Json(responseStatus);
        }

        [HttpPost]
        [Route("ECBal")]
        public IActionResult _MyBalance()
        {
            if (_lr.RoleID != Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser) 
            {
                IFintechShoppingML ml = new FintechShoppingML(_accessor, _env);
                var res = ml.GetUserBalnace();
                return Json(res);
            }
            return Ok();
        }

        [HttpPost]
        [Route("/GetUserInfo")]
        public IActionResult GetUserInfo()
        {
            IFintechShoppingML mL = new FintechShoppingML(_accessor, _env);
            var res = mL.GetUserInfo();
            return Json(res);
        }

        [HttpPost]
        [Route("/ECommMenu")]
        public IActionResult ECommMenu()
        {
            IFintechShoppingML mL = new FintechShoppingML(_accessor, _env);
            var model = mL.GetShoppingMenu();
            return PartialView("Partial/_ECommMenu", model);
        }

        [HttpPost]
        [Route("/BindCatForIndex")]
        public IActionResult BindCatForIndex()
        {
            IFintechShoppingML mL = new FintechShoppingML(_accessor, _env);
            ProductFilter filter = new ProductFilter
            {
                CategoryID = 0,
                SubCategoryID1 = 0,
                SubCategoryID2 = 0
            };
            var res = mL.GetCategoriesForUserIndex(filter);
            return PartialView("Partial/_IndexCategorySingle", res);
        }

        [HttpPost]
        [Route("/GetECommBanners")]
        public IActionResult GetECommBanners()
        {
            IFintechShoppingML mL = new FintechShoppingML(_accessor, _env);
            var res = mL.GetBanners();
            return PartialView("Partial/_IndexBanners", res);
        }

        [HttpPost]
        [Route("/ProductGridForIndex")]
        public IActionResult ProductGridForIndex(ProductFilter p)
        {
            IFintechShoppingML mL = new FintechShoppingML(_accessor, _env);
            ViewBag.MainId = p.CategoryID;
            ViewBag.SubId = p.SubCategoryID1;
            var res = mL.GetProductForIndex(p);
            return PartialView("Partial/_IndexProductGrid", res);
        }

        [HttpPost]
        [Route("/ECommTrending")]
        public IActionResult ProductGridTrending(ProductFilter p)
        {
            IFintechShoppingML mL = new FintechShoppingML(_accessor, _env);
            var res = mL.GetProductTrending(p);
            return PartialView("Partial/_TrendingGrid", res);
        }

        [HttpPost]
        [Route("/ECommNewArrival")]
        public IActionResult ProductGridNewArrival(ProductFilter p)
        {
            IFintechShoppingML mL = new FintechShoppingML(_accessor, _env);
            var res = mL.GetProductNewArrival(p);
            return PartialView("Partial/_NewArrivalGrid", res);
        }

        [HttpPost]
        [Route("/ECommSimilar")]
        public IActionResult ProductGridSimilar(ProductFilter p)
        {
            IFintechShoppingML mL = new FintechShoppingML(_accessor, _env);
            var res = mL.GetProductSimilar(p);
            return PartialView("Partial/_SimilarGrid", res);
        }

        [Route("/FilteredProductList/{id}")]
        public IActionResult FilteredProductList(int id)
        {
            if (ApplicationSetting.IsECommerceAllowed)
            {
                ViewBag.Id = id;
                return View();
            }
            return Ok();
        }

        [HttpPost]
        [Route("/GetFilteredProduct")]
        public IActionResult FilteredProduct(ProductFilter p)
        {
            IFintechShoppingML mL = new FintechShoppingML(_accessor, _env);
            var res = mL.GetFilteredProduct(p);
            return PartialView("Partial/_FilteredProduct", res);
        }

        [HttpPost]
        [Route("/GetFilterList")]
        public IActionResult GetFilters(int sid, bool IsListForm = false)
        {
            IFintechShoppingML mL = new FintechShoppingML(_accessor, _env);
            var list = mL.GetRequeiredFilter(sid);
            var modal = new FilterForEndUser
            {
                FilterWithOptions = list,
                Brands = mL.GetBrand(0, sid)
            };
            if (IsListForm)
                return PartialView("Partial/_FilterForEndUser", modal);
            else
                return PartialView("Partial/_FilterWithOption", modal.FilterWithOptions);
        }

        [Route("Shop/Product/{PdetailId}")]
        public async Task<IActionResult> ProductDetailForUser(int PdetailId)
        {
            if (ApplicationSetting.IsECommerceAllowed)
            {
                IFintechShoppingML ml = new FintechShoppingML(_accessor, _env);
                var res = await ml.ProDescription(PdetailId);
                return View("ProductDetailForUser", res);
            }
            return Ok();
        }

        [HttpPost]
        [Route("OnFilterChange")]
        public async Task<IActionResult> OnFilterChange(ProductFilter p)
        {
            IFintechShoppingML ml = new FintechShoppingML(_accessor, _env);
            var res = await ml.OnPageChangeFilter(p.ProductId, p.ProductDetailId, p.FilterIds);
            return Json(res);
        }

        [HttpPost]
        [Route("Quickview/{PdetailId}")]
        public async Task<IActionResult> QuickViewProduct(int PDetailId)
        {
            IFintechShoppingML ml = new FintechShoppingML(_accessor, _env);
            var res = await ml.ProDescription(PDetailId);
            return PartialView("Partial/_Quickview", res);
        }

        [HttpPost]
        [Route("/ECommWishList")]
        public IActionResult AddToWishList(int ProductDetailID)
        {
            IFintechShoppingML ml = new FintechShoppingML(_accessor, _env);
            var res = ml.AddToWishList(ProductDetailID);
            return Json(res);
        }

        [HttpPost]
        [Route("/WishListCount")]
        public IActionResult WishListCount()
        {
            IFintechShoppingML ml = new FintechShoppingML(_accessor, _env);
            var res = ml.WishListCount();
            return Json(res);
        }

        [HttpPost]
        [Route("/GetECommWishlist")]
        public async Task<IActionResult> WishlistDetail()
        {
            IFintechShoppingML ml = new FintechShoppingML(_accessor, _env);
            var res = await ml.WishlistDetail().ConfigureAwait(false);
            return PartialView("Partial/_WishlistDetail", res);
        }

        [HttpPost]
        [Route("/ECommRemoveItemFromWishlist")]
        public IActionResult RemoveItemFromWishlist(int PdId, int ID)
        {
            IFintechShoppingML ml = new FintechShoppingML(_accessor, _env);
            var res = ml.RemoveFromWishList(PdId, ID);
            return Json(res);
        }

        [HttpPost]
        [Route("/ECommClearWishlist")]
        public IActionResult ECommClearWishlist()
        {
            IFintechShoppingML ml = new FintechShoppingML(_accessor, _env);
            var res = ml.RemoveFromWishList(0, 0, true);
            return Json(res);
        }

        [HttpPost]
        [Route("/ECommAddToCart")]
        public async Task<IActionResult> AddToCart(int ProductDetailID, int Quantity)
        {
            IFintechShoppingML ml = new FintechShoppingML(_accessor, _env);
            var res = await ml.AddToCart(ProductDetailID, Quantity);
            return Json(res);
        }

        //[HttpPost]
        [Route("/ECommCart")]
        public async Task<IActionResult> CartDetail()
        {
            IFintechShoppingML ml = new FintechShoppingML(_accessor, _env);
            var res = await ml.CartDetail().ConfigureAwait(false);
            return View(res);
        }

        [HttpPost]
        [Route("/ECommRemoveItemFromCart")]
        public async Task<IActionResult> RemoveItemFromCart(int ID, int PdId)
        {
            IFintechShoppingML ml = new FintechShoppingML(_accessor, _env);
            var res = await ml.RemoveItemFromCart(ID, 0, PdId);
            return Json(res);
        }

        [HttpPost]
        [Route("/ECommClearCart")]
        public async Task<IActionResult> ECommClearCart()
        {
            IFintechShoppingML ml = new FintechShoppingML(_accessor, _env);
            var res = await ml.RemoveItemFromCart(0, 0, 0, true);
            return Json(res);
        }

        [HttpPost]
        [Route("/QuantityChange")]
        public async Task<IActionResult> ChangeQuantity(int PdId, int Quantity)
        {
            IFintechShoppingML ml = new FintechShoppingML(_accessor, _env);
            var res = await ml.ChangeQuantityByPdId(PdId, Quantity);
            return Json(res);
        }

        [HttpPost]
        [Route("/CartCount")]
        public IActionResult ItemCountinCart()
        {
            IFintechShoppingML mL = new FintechShoppingML(_accessor, _env);
            var data = mL.ItemInCart();
            return Json(data);
        }

        [HttpPost]
        [Route("/ECProceedToPay")]
        public IActionResult ProceedToPay()
        {
            if (ApplicationSetting.IsECommerceAllowed)
            {
                IFintechShoppingML mL = new FintechShoppingML(_accessor, _env);
                var data = mL.ProceedToPay();
                if (data.Statuscode == ErrorCodes.One)
                {
                    data.CartDetails = mL.CartDetail().Result;
                    if (data.CartDetails.Any())
                    {
                        data.Statuscode = ErrorCodes.One;
                        return PartialView("Partial/_ProceedToPay", data);
                    }
                    else
                    {
                        data.Statuscode = ErrorCodes.Two;
                        data.Msg = "No items in cart";
                    }
                }
                return Json(data);
            }
            else
            {
                return Ok();
            }
        }

        #region ShippingAddress
        [HttpPost]
        [Route("/AddShippingAddress")]
        public IActionResult ShippingAddress()
        {
            IFintechShoppingML ML = new FintechShoppingML(_accessor, _env);
            var res = new ShippingAddressModal
            {
                States = ML.States(),
                Cities = ML.Cities(0)
            };
            return PartialView("Partial/_ShippingAddress", res);
        }

        [HttpPost]
        [Route("/ECSaveShippingAddress")]
        public IActionResult SaveShippingAddress(SAddress param)
        {
            IFintechShoppingML ML = new FintechShoppingML(_accessor, _env);
            var res = ML.AddShippingAddress(param);
            return Json(res);
        }
        #endregion

        [HttpGet]
        [Route("ECommOrderReport")]
        public IActionResult OrderReport()
        {
            if (!ApplicationSetting.IsECommerceAllowed)
            {
                return Ok();
            }
            if (_lr == null)
            {
                return RedirectToAction("Index");
                //return Ok();
            }
            IFintechShoppingML ml = new FintechShoppingML(_accessor, _env);
            var res = new OrderReport
            {
                Category = ml.GetShoppingMainCategoryNew(),
                Role = _lr.RoleID
            };
            return View(res);
        }

        [HttpPost]
        [Route("/DetailedOrderReport")]
        public IActionResult DetailedOrderReport(OrderModel req)
        {
            IFintechShoppingML mL = new FintechShoppingML(_accessor, _env);
            var res = new OrderReportModel
            {
                Role = _lr.RoleID,
                OrderReport = mL.getOrderReport(req)
            };
            return PartialView("Partial/_OrderReport", res);
        }

        [HttpPost]
        [Route("ShippingReceipt")]
        public IActionResult ShippingDetailReceipt(int ID, bool IsPrint)
        {
            IFintechShoppingML operation = new FintechShoppingML(_accessor, _env);
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

        private bool ClearCurrentSession()
        {
            try
            {
                HttpContext.Session.Clear();
                CookieHelper cookie = new CookieHelper(_accessor);
                cookie.Remove(SessionKeys.ECommAppSessionID);
                return true;
            }
            catch (Exception)
            {
            }
            return false;
        }

    

    }
}
