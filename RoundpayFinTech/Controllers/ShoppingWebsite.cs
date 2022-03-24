using Fintech.AppCode;
using Fintech.AppCode.Configuration;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model.ShopingWebSite;
using RoundpayFinTech.AppCode.Model.Shopping;
using RoundpayFinTech.AppCode.Model.Shopping.WebShopping.ViewModel;
using RoundpayFinTech.AppCode.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ShoppingWebsite : Controller
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly ISession _session;
        private readonly LoginResponse _lr;
        private readonly ILoginML loginML;
        private readonly IWebShoppingML _WebShoppingML;
        private readonly WebSite _ws;


        private readonly RoundpayFinTech.AppCode.Model.WebsiteInfo _WInfo;
        private readonly string WebsiteID;
        public ShoppingWebsite(IHttpContextAccessor accessor, IHostingEnvironment env)
        {

            _accessor = accessor;
            _env = env;
            _session = _accessor.HttpContext.Session;
            _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
            loginML = new LoginML(_accessor, _env);
            _WebShoppingML = new WebShoppingML(_accessor, _env);
            _WInfo = loginML.GetWebsiteInfo();

            WebsiteID = Convert.ToString(_WInfo != null ? _WInfo.WID : 1);
            _ws = _WebShoppingML.ShoppingWebsiteInfo(WebsiteID);
        }
        [HttpGet]
        [Route("Index")]
        public IActionResult Index()
        {
            NewArrivalOnSaleProducts resnosp = new NewArrivalOnSaleProducts();
            resnosp = _WebShoppingML.NewArrival_OnSaleProductAsync("1").Result;
            Data Data = _WebShoppingML.GetwebsiteInfo("1");
            var sws = new ShoppingWebSiteLists()
            {
                BestSellerList = resnosp.BestSellerList,
                NewArrivals = resnosp.NewArrivals,
                OnSale = resnosp.OnSale,
                BannerProductsFront = Data.BannerProductsFront,
                BannerProductsRight = Data.BannerProductsRight,
                topcategories = Data.topcategories,
                menus = Data.menus
            };
            return View(sws);
        }

        #region Profile Info
        [HttpPost]
        [Route("ProfileInfo")]
        public async Task<IActionResult> ProfileInfo()
        {
            return PartialView("Partial/_ProfileInfo", _lr);
        }
        #endregion

        #region CategoryProduct ProductList
        [HttpPost]
        [Route("CategoryProduct")]
        public IActionResult CategoryProduct(string wsid, int mcatid, int catid)
        {
            var res = _WebShoppingML.GetProductDetailByCategoryID(1, mcatid, catid);
            return PartialView("Partial/_Index", res);
        }
        [HttpGet]
        [Route("ProductList/{Type}/{CatID}")]
        public IActionResult ProductList(string Type = "C", int CatID = 0, string WebSiteID = "1")
        {
            //OrderBy New For Newest Firts,Price and in  OrderByType A nd d
            var ProductSetInfo = new ProductSetInfo()
            {
                FilterType = Type,
                FilterTypeId = CatID,
                KeywordId = 0,
                OrderBy = "New",
                OrderByType = "d",
                PageLimitIndex = 40,
                StartIndex = 0,
                WebsiteId = WebSiteID,
                FilterOptionTypeIdList = new List<string>()
            };
            ViewData["FilterType"] = Type;
            //var res = swml.GetAllProductsetList(ProductSetInfo).Result;
            var res = _WebShoppingML.GetAllProductsetList(ProductSetInfo).Result.ProductSetList;
            return View(res);
        }
        [HttpGet]
        [Route("Dashboard/{LP}")]
        public IActionResult SwitchDashboard(string LP = "I")
        {
            if (loginML.IsInValidSession())
            {
                return RedirectToAction("Index", "Login");
            }
            ILoginML _ml = new LoginML(_accessor, _env);
            _ml.SetLaunchPreference(LP, _lr);
            if (_lr.LaunchPreferences == LaunchPreferences.DynamicWebSiteIndex)
            {
                return RedirectToAction("Index", "ShoppingWebsite");
            }
            else
            {
                return RedirectToAction("Index", "Seller");
            }
        }
        [HttpPost]
        [Route("ProductSetList")]
        public IActionResult ProductList(ProductSetInfo productsetinfo)
        {
            ViewData["FilterType"] = productsetinfo.FilterType;
            productsetinfo.WebsiteId = "1";
            var res = _WebShoppingML.GetAllProductsetList(productsetinfo).Result.ProductSetList;
            return PartialView("Partial/_ProductList", res);
        }
        #endregion

        #region QuickView FilterList SimilarProducts RecentView
        [HttpPost]
        [Route("QuickView")]
        public IActionResult QuickView(string wsid = "1", int PosID = 0)
        {
            var res = _WebShoppingML.QuickViewApiAsync(PosID).Result;
            ViewData["PosID"] = PosID;
            return PartialView("Partial/_QuickView", res);
        }
        [HttpPost]
        [Route("FilterList")]
        public IActionResult GetFilterList(FilterOptionRequest FilterOptionRequest)
        {
            var res = _WebShoppingML.GetAllFilterList(FilterOptionRequest).Result;
            return PartialView("Partial/_FilterList", res);
        }


        [HttpPost]
        [Route("SimilarProducts")]
        public IActionResult GetProductSimilarList(int POSId, string WebSiteId = "1")
        {
            var res = _WebShoppingML.GetAllSimilarItems(POSId);
            return PartialView("Partial/_SimilarProducts", res);
        }
        [HttpPost]
        [Route("RecentView")]
        public IActionResult RecentView(string POSId, string WebSiteId = "1")
        {
            IShoppingML sml = new ShoppingML(_accessor, _env);
            List<RecentViewModel> res = new List<RecentViewModel>();
            res = sml.RecentViewDetails(_lr != null ? _lr.UserID : 0, WebSiteId).Result?.ToList();
            return PartialView("Partial/_RecentViewProducts", res);
        }
        #endregion

        #region GetProductDetails
        [HttpGet]
        [Route("GetProductDetails/{POSId}")]
        public async Task<IActionResult> ProductDetails(int POSId)
        {
            // getAllProductInfo
            var res = _WebShoppingML.GetAllProductInfo(POSId, "0").Result;
            var req = new RecentViewRequest
            {
                LoginID = _lr != null ? _lr.UserID : 0,
                ProductDeatilID = POSId
            };
            if (req.LoginID <= 0)
            {
                var eCommUser = ReadLocalSession(SessionKeys.ECommRecentView);
                if (eCommUser == null)
                {
                    ECommUserDetail eCommRecentView = new ECommUserDetail();
                    eCommRecentView.RecentView = new List<RecentViewRequest>();
                    eCommRecentView.RecentView.Add(req);
                    WriteLocalSession(eCommRecentView, true, SessionKeys.ECommRecentView);
                }
                else
                {
                    bool SameProduct = false;
                    foreach (var item in eCommUser.RecentView)
                    {
                        if (item.ProductDeatilID == POSId)
                        {
                            SameProduct = true;
                            break;
                        }
                    }
                    if (!SameProduct)
                    {
                        eCommUser.RecentView.Add(req);
                        WriteLocalSession(eCommUser, true, SessionKeys.ECommRecentView);

                    }
                }
            }
            else
            {
                var eCommUser = ReadLocalSession(SessionKeys.ECommRecentView);
                if (eCommUser != null)
                {
                    foreach (var item in eCommUser.RecentView)
                    {
                        var rview = _WebShoppingML.AddRecentViewProduct(_lr.UserID, "", item.ProductDeatilID);
                    }
                }
                var review = _WebShoppingML.AddRecentViewProduct(_lr.UserID, "", POSId);
            }


            ViewData["POSId"] = POSId;
            return View(res);
        }
        #endregion

        #region Add To Cart removecart CartDetails ProceedCheckOut shoppingcart Logout
        [HttpPost]
        [Route("AddToCartProd")]
        public IActionResult AddToCart(string posid, int posdetid, int quantity)
        {
            IShoppingML ML = new ShoppingML(_accessor, _env);
            var res = new RoundpayFinTech.AppCode.Model.ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            var req = new AddToCartRequest
            {
                LoginID = _lr != null ? _lr.UserID : 0,
                Quantity = quantity,
                ProductDeatilID = posdetid
            };
            if (req.LoginID <= 0)
            {
                var eCommUser = ReadLocalSession(SessionKeys.ECommUserDetail);
                if (eCommUser == null)
                {
                    ECommUserDetail eCommUserDetail = new ECommUserDetail();
                    eCommUserDetail.CartDetail = new List<AddToCartRequest>();
                    eCommUserDetail.Wishlist = new List<int>();
                    eCommUserDetail.CartDetail.Add(req);
                    WriteLocalSession(eCommUserDetail, true, SessionKeys.ECommUserDetail);
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = "Product added to cart";
                }
                else
                {
                    bool SameProduct = false;
                    foreach (var item in eCommUser.CartDetail)
                    {
                        if (item.ProductDeatilID == posdetid)
                        {
                            SameProduct = true;
                            break;
                        }
                    }
                    if (!SameProduct)
                    {
                        eCommUser.CartDetail.Add(req);
                        WriteLocalSession(eCommUser, true, SessionKeys.ECommUserDetail);
                        res.Statuscode = ErrorCodes.One;
                        res.Msg = "Product added to cart";
                    }
                    else
                    {
                        res.Statuscode = ErrorCodes.Minus1;
                        res.Msg = "Product already in cart";
                    }
                }
            }
            else
            {
                var eCommUser = ReadLocalSession(SessionKeys.ECommUserDetail);
                if (eCommUser != null)
                {
                    foreach (var item in eCommUser.CartDetail)
                    {
                        res = ML.AddToCart(item.ProductDeatilID, item.Quantity, _lr.UserID).Result;
                    }
                }
                res = ML.AddToCart(posdetid, quantity, _lr.UserID).Result;
            }
            return Json(res);
        }
        [HttpPost]
        [Route("removecart")]
        public async Task<IActionResult> RemoveItemFromCart(int ID = 0, int ProductDetailID = 0, bool RemoveAll = false)
        {
            IShoppingML ml = new ShoppingML(_accessor, _env);
            var resp = await ml.RemoveItemFromCart(ID, _lr != null ? _lr.UserID : 0, ProductDetailID, RemoveAll).ConfigureAwait(false);
            return Json(resp);
        }
        [HttpPost]
        [Route("CartDetails")]
        public async Task<IActionResult> CartDetails()
        {
            IShoppingML ml = new ShoppingML(_accessor, _env);
            ShoppingWebsiteCartDetails swcd = new ShoppingWebsiteCartDetails();
            var cartdetail = await ml.CartDetail().ConfigureAwait(false);
            if (cartdetail != null)
            {
                swcd.ItemInCart = ml.ItemInCart();
                swcd.CartDetail = cartdetail?.ToList();
            }
            return PartialView("Partial/_CartDetail", swcd);
        }
        [HttpGet]
        [Route("shoppingcart")]
        public async Task<IActionResult> ShoppingCart()
        {
            return View();
        }
        [HttpPost]
        [Route("shoppingcart")]
        public async Task<IActionResult> ShoppingCart(int id = 0)
        {
            IShoppingML ml = new ShoppingML(_accessor, _env);
            ShoppingWebsiteCartDetails swcd = new ShoppingWebsiteCartDetails();
            var cartdetail = await ml.CartDetail().ConfigureAwait(false);
            if (cartdetail != null)
            {
                swcd.ItemInCart = ml.ItemInCart();
                swcd.CartDetail = cartdetail?.ToList();
            }
            return PartialView("Partial/_ShoppingCart", swcd);
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            CookieHelper cookie = new CookieHelper(_accessor);
            cookie.Remove(SessionKeys.AppSessionID);
            return RedirectToAction("Index", "Login");
        }
        #endregion

        #region Wish List
        [HttpGet]
        [Route("MyProfile/{type}")]
        public async Task<IActionResult> MyProfile(string type)
        {
            if (loginML.IsInValidSession())
            {
                return RedirectToAction("Index", "Login");
            }
            var res = new RoundpayFinTech.AppCode.Model.ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = type,
                CommonStr = _lr.Name
            };
            return View(res);
        }
        [HttpPost]
        [Route("WishList")]
        public async Task<IActionResult> GetWishList()
        {
            return PartialView("Partial/_MyWishList", _WebShoppingML.GetAllWishListItems(WebsiteID, _lr.UserID));
        }
        [HttpPost]
        [Route("addwishlist")]
        public IActionResult AddWishListProduct(int ProductDetailID = 0)
        {
            IShoppingML ml = new ShoppingML(_accessor, _env);
            var res = new RoundpayFinTech.AppCode.Model.ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            if (_lr == null)
            {
                res.Statuscode = -2;
                res.Msg = "/Login";
                return Json(res);
            }
            res = ml.AddToWishList(ProductDetailID, _lr == null ? 0 : _lr.UserID);
            return Json(res);
        }

        [HttpPost]
        [Route("removewishlist")]
        public IActionResult RemoveWishListItem(int ProductDetailID = 0, int ID = 0, bool RemoveAll = false)
        {
            IShoppingML ml = new ShoppingML(_accessor, _env);
            var res = new RoundpayFinTech.AppCode.Model.ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            if (_lr == null)
            {
                res.Statuscode = -2;
                res.Msg = "/Login";
                return Json(res);
            }
            res = ml.RemoveFromWishList(ProductDetailID, _lr == null ? 0 : _lr.UserID, ID, RemoveAll);
            return Json(res);
        }
        [HttpPost]
        [Route("WishListToCart")]
        public IActionResult WishListToCart(int ID, int posdetid = 0)
        {
            IShoppingML ml = new ShoppingML(_accessor, _env);
            var res = new RoundpayFinTech.AppCode.Model.ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            if (_lr == null)
            {
                res.Statuscode = -2;
                res.Msg = "/Login";
                return Json(res);
            }
            res = ml.AddToCart(posdetid, 1, _lr == null ? 0 : _lr.UserID).Result;
            ml.RemoveFromWishList(posdetid, _lr == null ? 0 : _lr.UserID, ID, false);
            return Json(res);
        }
        #endregion

        #region Add To  Buy Now
        [HttpPost]
        [Route("buynow")]
        public IActionResult BuyNow(int ProductDetailID = 0, int quantity = 1)
        {
            IShoppingML ml = new ShoppingML(_accessor, _env);
            var res = new RoundpayFinTech.AppCode.Model.ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            if (_lr == null)
            {
                res.Statuscode = -2;
                res.Msg = "/Login";
                return Json(res);
            }
            res = ml.AddToCart(ProductDetailID, quantity, _lr.UserID).Result;
            return Json(res);
        }
        [HttpPost]
        [Route("proceedcheckout")]
        public IActionResult ProceedCheckOut()
        {
            IShoppingML ml = new ShoppingML(_accessor, _env);
            var res = new ShoppingWebsiteCartDetails
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            if (_lr == null)
            {
                res.Statuscode = -2;
                res.Msg = "/Login";
                return Json(res);
            }
            var data = ml.ProceedToPay();
            data.CartDetails = ml.CartDetail().Result;
            return PartialView("Partial/_CheckOut", data);
        }
        [HttpPost]
        [Route("ChangeItemQunatity")]
        public IActionResult ChangeQuantity(int ProductDetailID, int Quantity, int Type)
        {
            var res = new RoundpayFinTech.AppCode.Model.ResponseStatus()
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            if (Quantity < 1)
            {
                res.Statuscode = ErrorCodes.Minus1;
                res.Msg = "Invalid Quantity";
                return Json(res);
            }
            if (Type != -1 && Type != 1)
            {
                res.Statuscode = ErrorCodes.Minus1;
                res.Msg = "Invalid Opreation";
                return Json(res);
            }
            Quantity = Type == -1 ? Quantity - 1 : Quantity + 1;
            IShoppingML ml = new ShoppingML(_accessor, _env);
            res = ml.ChangeQuantityByPdId(ProductDetailID, Quantity).Result;
            return Json(res);
        }
        [HttpPost]
        [Route("/placecartorder")]
        public async Task<IActionResult> PlaceOrderCart(PlaceOrder order)
        {
            IShoppingML ml = new ShoppingML(_accessor, _env);
            var res = await ml.PlaceOrder(order).ConfigureAwait(false);
            return Json(res);
        }
        [HttpPost]
        [Route("/ordercompleted")]
        public async Task<IActionResult> OrderCompleted(PlaceOrder order)
        {
            return PartialView("Partial/_OrderCompleted");
        }


        #endregion

        #region Methods
        private void WriteLocalSession(object value, bool isPersistent, string key)
        {
            CookieHelper cookie = new CookieHelper(_accessor);
            cookie.Set(key, JsonConvert.SerializeObject(value), isPersistent ? DateTime.Now.AddDays(30) : DateTime.Now.AddHours(1));
        }
        private ECommUserDetail ReadLocalSession(string key)
        {
            CookieHelper cookie = new CookieHelper(_accessor);
            var v = cookie.Get(key);
            return v == null ? null : JsonConvert.DeserializeObject<ECommUserDetail>(v);
        }
        #endregion

        #region Check Delhivery Status ,Search KeyWord List

        [Route("checkcelivery")]
        public IActionResult CheckDelivery(string PinCode)
        {
            var data = new CheckPinCodeStatus();
            var res = new RoundpayFinTech.AppCode.Model.ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            data = _WebShoppingML.CheckDelivery(PinCode);
            return Json(data);
        }

        [HttpPost]
        public IActionResult GetSearchKeyWordList(string KeyWords)
        {
            IEnumerable<KeywordList> list = _WebShoppingML.GetKeywordList("1", KeyWords);
            return Json(new { list });
        }

        #endregion

        #region FooterPages
        [HttpGet]
        [Route("PrivacyPolicy")]
        public IActionResult PrivacyPolicy()
        {
            return View("PrivacyPolicy", _ws);
        }

        [HttpGet]
        [Route("Refund")]
        public IActionResult Refund()
        {
            return View("Refund", _ws);
        }
        [HttpGet]
        [Route("Term")]
        public IActionResult Term()
        {
            return View("Term", _ws);
        }
        #endregion

        #region HeaderMenu,Footer Details
        [HttpPost]
        [Route("HeaderMenu")]
        public async Task<IActionResult> HeaderMenu()
        {
            var res = _WebShoppingML.ShoppingHeaderMenus(WebsiteID);
            return PartialView("Partial/_HeaderMenu", res);
        }
        [HttpPost]
        [Route("ShoppingFooter")]
        public async Task<IActionResult> ShoppingFooter()
        {
            return PartialView("Partial/_ShoppingFooter", _ws);
        }
        #endregion

        #region OrderDetails
        [HttpPost]
        [Route("OrderReport")]
        public async Task<IActionResult> OrderReport()
        {
            IShoppingML ml = new ShoppingML(_accessor, _env);
            var req = new OrderModel()
            {
                TopRows = 50,
                RequestedMode = 3,
                FromDate = string.Format("{0:dd MMM yyyy}", DateTime.Now.AddDays(-30)),
                ToDate = string.Format("{0:dd MMM yyyy}", DateTime.Now)
            };
            return PartialView("Partial/_MyOrders", ml.getOrderReport(req));
        }
        [HttpPost]
        [Route("ChangePartialOrder")]
        public IActionResult ChangePartialOrderStatus(ChangeOrderStatus req)
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var res = mL.ChangePartialOrderStatus(req);
            return Json(res);
        }
        #endregion

        #region MySearch
        [HttpPost]
        [Route("MySearch")]
        public IActionResult MySearch(string Words)
        {
            var res = _WebShoppingML.ShoppingGetKeywordDetails(Words);
            return Json(res);
        }
        #endregion

        #region Shipping Address
        [HttpPost]
        [Route("ShoppingWebsite/_ShippingAddress/{id}")]
        public IActionResult _ShippingAddress(int id = 0)
        {
            IShoppingML ML = new ShoppingML(_accessor, _env);
            var res = new ShippingAddressModal();
            res.States = ML.States();
            res.Cities = ML.Cities(0);
            res.Shipping = new ShoppingShipping();
            if (id > 0)
            {
                res.Shipping = ML.GetShippingAddressByID(id);
            }
            return PartialView("Partial/_ShippingAddress", res);
        }

        [HttpPost]
        [Route("ShoppingWebsite/_DeleteShippingAddress/{id}")]
        public IActionResult _DeleteShippingAddress(int id)
        {
            IShoppingML ML = new ShoppingML(_accessor, _env);
            var res = ML.DeleteShippingAddress(id);
            return Json(res);
        }

        [HttpPost]
        [Route("ShoppingWebsite/SaveShippingAddress")]
        public IActionResult SaveShippingAddress(SAddress param)
        {
            IShoppingML ML = new ShoppingML(_accessor, _env);
            var res = ML.AddShippingAddress(param);
            return Json(res);
        }
        #endregion
    }
}
