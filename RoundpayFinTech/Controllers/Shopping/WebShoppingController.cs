using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Mvc;
using RoundpayFinTech.AppCode.Configuration;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model.App;
using RoundpayFinTech.AppCode.Model.Shopping.WebShopping;
using System;
using System.Threading.Tasks;
using RoundpayFinTech.AppCode.Model.Shopping.WebShopping.ViewModel;

namespace RoundpayFinTech.Controllers
{
    public partial class AppController
    {
        [HttpGet]
        public async Task<IActionResult> GetMenu([FromBody] AppRequest appRequest)
        {
            var Response = new ShoppingMenuResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
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

        #region Get Main CategoriesList
        [HttpGet]
        [Route("app/getMainCategoriesList")]
        public async Task<IActionResult> GetMainCategoriesList(string WebsiteId, int MainCategoryId, int CategoryId)
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
                    Response.Data = ML.GetProductDetailByCategoryID(id, MainCategoryId, CategoryId);
                }
            }
            catch (Exception ex)
            {

            }
            return Json(Response);
        }
        #endregion

        #region Get Api


        [HttpGet]
        [Route("app/websiteInfo")]
        public async Task<IActionResult> GetwebsiteInfo(string WebsiteId = "1", string HostName = "", int LoginId = 0)
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
                    Response.Data = ML.GetwebsiteInfo(WebsiteId);
                }
            }
            catch (Exception ex)
            {

            }
            return Json(Response);
        }
        #endregion 

        #region Get New Arrival
        [HttpGet]
        [Route("app/getnewArrivalOnSaleApi")]
        public async Task<IActionResult> newArrival_OnSaleApi(string WebsiteId)
        {
            var Response = new Response<NewArrivalOnSaleProducts>();
            IWebShoppingML ML = new WebShoppingML(_accessor, _env);
            try
            {
                int id = 1;
                if (ApplicationSetting.IsECommerceAllowed)
                {
                    Response.Id = id;
                    Response.Status = Convert.ToBoolean(Convert.ToBoolean(ErrorCodes.One));
                    Response.Message = ErrorCodes.SUCCESS;
                    Response.Data = await ML.NewArrival_OnSaleProductAsync(WebsiteId);
                }
            }
            catch (Exception ex)
            {

            }
            return Json(Response);
        }
        #endregion Get New Arrival

        #region Get quickViewApi
        [HttpGet]
        [Route("app/quickViewApi")]
        public async Task<IActionResult> quickViewApi(string WebsiteId, int POSId)
        {
            var Response = new Response();
            IWebShoppingML ML = new WebShoppingML(_accessor, _env);
            try
            {
                int id = 1;
                if (ApplicationSetting.IsECommerceAllowed)
                {
                    Response.Id = id;
                    Response.Status = true;
                    Response.Message = ErrorCodes.SUCCESS;
                    Response.Data = ML.QuickViewApiAsync(POSId).Result;
                }
            }
            catch (Exception ex)
            {

            }
            return Json(Response);
        }
        #endregion quickViewApi

        #region Get All Sub Category List
        [HttpGet]
        [Route("app/GetAllSubCategoryList")]
        public async Task<IActionResult> getAllSubCategoryList(string WebsiteId, int CategoryId)
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
                    Response.Data = ML.GetAllSubCategoryList(id, CategoryId);
                }
            }
            catch (Exception ex)
            {

            }
            return Json(Response);
        }

        #endregion

        #region Get Get All Product Info
        [HttpGet]
        [Route("app/getAllProductInfo")]
        public async Task<IActionResult> GetAllProductInfo(string WebsiteId, int POSId, string LoginId)
        {
            var Response = new Response();
            IWebShoppingML ML = new WebShoppingML(_accessor, _env);
            IShoppingML ML1 = new ShoppingML(_accessor, _env);
            try
            {
                int id = 1;
                if (ApplicationSetting.IsECommerceAllowed)
                {
                    Response.Id = id;
                    Response.Status = Convert.ToBoolean(ErrorCodes.One);
                    Response.Message = ErrorCodes.SUCCESS;
                    Response.Data = ML.GetAllProductInfo(POSId, LoginId).Result;
                    //var stat = ML1.ProDescription(POSId, Convert.ToInt32(LoginId));
                }
            }
            catch (Exception ex)
            {

            }
            return Json(Response);
        }
        #endregion

        #region Get All Product set List
        [HttpPost]
        [Route("app/getAllProductsetList")]
        public async Task<IActionResult> getAllProductsetList([FromBody] ProductSetInfo productSetInfo)
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
                    Response.Data = ML.GetAllProductsetList(productSetInfo).Result;
                }
            }
            catch (Exception ex)
            {

            }
            return Json(Response);
        }
        #endregion

        #region Get All Similar Items
        [HttpGet]
        [Route("app/getAllSimilarItems")]
        public async Task<IActionResult> getAllSimilarItems(string WebsiteId, int POSId, string LoginId = "")
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
                    Response.Data = ML.GetAllSimilarItems(POSId);
                }
            }
            catch (Exception ex)
            {

            }
            return Json(Response);
        }
        #endregion

        #region Get All Filter List
        [HttpPost]
        [Route("app/getAllFilterList")]
        public async Task<IActionResult> getAllFilterList([FromBody] FilterOptionRequest filterOptionRequest)
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
                    Response.Data = ML.GetAllFilterList(filterOptionRequest).Result;
                }
            }
            catch (Exception ex)
            {

            }
            return Json(Response);
        }
        #endregion

        #region Get Keyword List
        [HttpGet]
        [Route("app/getKeywordList")]
        public async Task<IActionResult> getKeywordList(string SearchKeyword, string WebsiteId)
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
                    Response.Data = ML.GetKeywordList(WebsiteId, SearchKeyword);
                }
            }
            catch (Exception ex)
            {

            }
            return Json(ML.GetKeywordList(WebsiteId, SearchKeyword));
        }
        #endregion

        #region Insert Wish List
        [HttpPost]
        [Route("app/InsertWishList")]
        public async Task<IActionResult> InsertWishList([FromBody] WishList wishList)
        {
            var Response = new Response();
            IShoppingML ml = new ShoppingML(_accessor, _env);
            try
            {
                int id = 1, WebsiteId = 1;
                string SearchKeyword = "";
                if (ApplicationSetting.IsECommerceAllowed)
                {
                    var res= ml.AddToWishList(wishList.POSId,wishList.LoginId);
                    Response.Id = id;
                    Response.Status = Convert.ToBoolean( res.Statuscode);
                    Response.Message = res.Msg;
                    Response.Data = null;
                }
            }
            catch (Exception ex)
            {

            }
            return Json(Response);
        }
        #endregion

        #region Delete Wish List
        [HttpPost]
        [Route("app/DeleteWishList")]
        public async Task<IActionResult> DeleteWishList([FromBody] WishList wishList)
        {
            var Response = new Response();
            IShoppingML ml = new ShoppingML(_accessor, _env);
            try
            {
                int id = 1, WebsiteId = 1;
                string SearchKeyword = "";
                if (ApplicationSetting.IsECommerceAllowed)
                {
                    var res = ml.RemoveFromWishList(wishList.POSId,wishList.LoginId);
                    Response.Id = id;
                    Response.Status = Convert.ToBoolean(res.Statuscode);
                    Response.Message = res.Msg;
                    Response.Data = null;
                }
            }
            catch (Exception ex)
            {

            }
            return Json(Response);
        }
        #endregion

        #region get Wish List
        [HttpGet]
        [Route("app/getAllWishListItems")]
        public async Task<IActionResult> getAllWishListItems(string  WebsiteId, int CustomerId)
        {
            var Response = new Response();
            IWebShoppingML ML = new WebShoppingML(_accessor, _env);
            try
            {
                int id = 1;
                string SearchKeyword = "";
                if (ApplicationSetting.IsECommerceAllowed)
                {
                  
                    Response.Id = id;
                    Response.Status = Convert.ToBoolean(ErrorCodes.One);
                    Response.Message = ErrorCodes.SUCCESS;
                    Response.Data = ML.GetAllWishListItems(WebsiteId, CustomerId);
                }
            }
            catch (Exception ex)
            {

            }
            return Json(Response);
        }
        #endregion
     
        #region Search Keyword Api
        [HttpGet]
        [Route("app/SearchKeywordApi")]
        public async Task<IActionResult> SearchKeywordApi([FromBody] KeywordSearch keywordSearch)
        {
            var Response = new Response();
            IWebShoppingML ML = new WebShoppingML(_accessor, _env);
            try
            {
                int id = 1;
                string SearchKeyword = "";
                if (ApplicationSetting.IsECommerceAllowed)
                {

                    Response.Id = id;
                    Response.Status = Convert.ToBoolean(ErrorCodes.One);
                    Response.Message = ErrorCodes.SUCCESS;
                    Response.Data = ML.GetSearchKeyword(keywordSearch);
                }
            }
            catch (Exception ex)
            {

            }
            return Json(Response);
        }
        #endregion

        #region Get Recent View Items
        [HttpGet]
        [Route("app/getRecentViewItems")]
        public async Task<IActionResult> GetRecentViewItems(string BrowserId, string WebSiteId)
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
                    Response.Data = ML.GetRecentViewItems(BrowserId, WebSiteId);
                }
            }
            catch (Exception ex)
            {

            }
            return Json(Response);
        }
        #endregion

        #region Check Delivery Api
        [HttpGet]
        [Route("app/checkDeliveryApi")]
        public async Task<IActionResult> CheckDeliveryApi(string PinCode)
        {
            var Response = new Response();
            IWebShoppingML ML = new WebShoppingML(_accessor, _env);
            try
            {
                int id = 1;
                if (string.IsNullOrEmpty(PinCode) || PinCode.Length!=6)
                {
                    Response.Id = id; ; Response.Status = false; Response.Message = "Pincode should be valid!"; Response.Data = null;
                }
                else { 
                if (ApplicationSetting.IsECommerceAllowed)
                {

                    Response.Id = id;
                    Response.Status = Convert.ToBoolean(ErrorCodes.One);
                    Response.Message = ErrorCodes.SUCCESS;
                    Response.Data = ML.CheckDelivery(PinCode);
                }
                }
            }
            catch (Exception ex)
            {

            }
            return Json(Response);
        }
        #endregion

    }
}
