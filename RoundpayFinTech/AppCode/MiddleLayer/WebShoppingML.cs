using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Fintech.AppCode.WebRequest;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.DL.Shopping.WebShopping;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.Shopping.WebShopping.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Data = RoundpayFinTech.AppCode.Model.Shopping.WebShopping.ViewModel.Data;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public class WebShoppingML : IWebShoppingML
    {
        #region Gloabl Variables
        protected readonly IHttpContextAccessor _accessor;
        protected readonly IHostingEnvironment _env;
        protected readonly IDAL _dal;
        private readonly IRequestInfo _rinfo;
        protected readonly IConnectionConfiguration _c;
        private readonly IConfiguration Configuration;
        private readonly IWebsiteML Webml;
        #endregion
        public WebShoppingML(IHttpContextAccessor accessor, IHostingEnvironment env, bool IsInSession = true)
        {
            _accessor = accessor;
            _env = env;
            _rinfo = new RequestInfo(_accessor, _env);
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            bool IsProd = _env.IsProduction();
            var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile((IsProd ? "appsettings.json" : "appsettings.Development.json"));
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            Webml = new WebsiteML(_accessor, _env);
        }
        public Data GetwebsiteInfo(string WebsiteId)
        {
            int id = Convert.ToInt32(WebsiteId);
            var data = new Data();
            try
            {

                List<ApiClassDealofWeek> dealofWeekList = new List<ApiClassDealofWeek>();
                List<ProductsList> productsLists = new List<ProductsList>();
                List<ProductsList> _productsLists = new List<ProductsList>();
                List<TopCategories> topCategoriesList = new List<TopCategories>();
                string DomainInfo = Webml.GetWebsiteInfo(id).AbsoluteHost;
                IProcedureAsync proc = new ProcGetWebsiteInfo(_dal);
                var result = (GetAllMenu)proc.Call(new CommonReq
                {
                    CommonStr = WebsiteId,
                    CommonStr2 = DomainInfo
                }).Result;
                List<Menu> menuList = new List<Menu>();
                WebSite ws = new WebSite();
                ws = result.websiteSettings.FirstOrDefault();
                id++;
                data.Id = id;
                int countMainCat = data.Id;
                foreach (var item in result.maincategoryMenus)
                {
                    List<CategoryList> categoryList = new List<CategoryList>();
                    IEnumerable<SubCategory> SubCategory = new List<SubCategory>();
                    var subCat = result.subCategoryMenus.Where(x => x.ParentId == 0 && x.mainCategoryID == item.MainCategoryID).ToList();
                    countMainCat++;
                    var mainMenu = new Model.Shopping.WebShopping.ViewModel.Menu
                    {
                        Id = countMainCat,
                        MainCategoryID = item.MainCategoryID,
                        Name = item.Name,
                        CategoryString = string.Join(",", subCat.Select(x => x.Name).ToList()),
                        MainCategoryImage = item.MainCategoryImage,
                        icon = item.icon,
                        Active = item.Active
                    };
                    foreach (var sub in subCat)
                    {
                        CategoryList cat = new CategoryList();
                        TopCategories TOPcat = new TopCategories();
                        countMainCat++;
                        cat.CategoryId = sub.SubCategoryId;
                        cat.Id = countMainCat;
                        cat.mainCategoryID = sub.mainCategoryID;
                        cat.Subcategory = sub.Subcategory;
                        // cat.SubCategoryId = sub.SubCategoryId;
                        cat.Name = sub.Name;
                        cat.IsActive = sub.IsActive;
                        cat.image = sub.image;
                        TOPcat.CategoryID = sub.SubCategoryId;
                        TOPcat.ParentId = sub.ParentId;
                        TOPcat.MainCategoryID = sub.mainCategoryID;
                        TOPcat.Name = sub.Name;
                        TOPcat.Image = sub.image;
                        List<SubCategory> subCategoryList = new List<SubCategory>();
                        List<SubCategoryMenus> subCategoryMenusList = new List<SubCategoryMenus>();
                        foreach (var sub2 in result.subCategoryMenus.Where(x => x.ParentId == sub.SubCategoryId))
                        {
                            countMainCat++;
                            subCategoryList.Add(new SubCategory
                            {
                                Id = countMainCat,
                                CategoryID = sub2.mainCategoryID,
                                ParentId = sub.SubCategoryId,
                                SubCategoryId = sub2.SubCategoryId,
                                Name = sub2.Name,
                                IsActive = sub2.IsActive,
                                subCategoryImage = sub2.image
                            });

                            subCategoryMenusList.Add(new SubCategoryMenus
                            {
                                Name = sub2.Name,
                                SubCategoryId = sub2.SubCategoryId,
                                SubCategoryImage = sub2.image


                            });
                        }
                        cat.Subcategory = subCategoryList;
                        TOPcat.subcategory = subCategoryMenusList;
                        categoryList.Add(cat);
                        topCategoriesList.Add(TOPcat);
                    }
                    mainMenu.CategoryList = categoryList;
                    menuList.Add(mainMenu);
                }
                data.menus = menuList;
                data.topcategories = topCategoriesList;
                data.dealofWeek = dealofWeekList;
                ResourceML rml = new ResourceML(_accessor, _env);
                ws.Logo = rml.GetLogoURL(Convert.ToInt32(WebsiteId)).ToString();
                data.websiteSettings = ws;
                data.BannerProductsFront = result.BannerProductsFront;
                data.BannerProductsRight = result.BannerProductsRight;
            }
            catch (Exception ex)
            {
                throw new NotImplementedException();
            }
            return data;
        }
        public List<MainCategoriesProduct> GetProductDetailByCategoryID(int id, int MainCategorieId, int CategoryID)
        {
            var data = new List<MainCategoriesProduct>();
            try
            {
                string DomainInfo = Webml.GetWebsiteInfo(id).AbsoluteHost;
                IProcedureAsync proc = new ProcGetProductDetailByCategoryID(_dal);
                data = (List<MainCategoriesProduct>)proc.Call(new CommonReq
                {
                    CommonInt = MainCategorieId,
                    CommonInt2 = CategoryID,
                    CommonStr = DomainInfo
                }).Result;
            }
            catch (Exception ex)
            {
                throw new NotImplementedException();
            }
            return data;
        }
        public async Task<NewArrivalOnSaleProducts> NewArrival_OnSaleProductAsync(string WebsiteId)
        {
            NewArrivalOnSaleProducts data = new NewArrivalOnSaleProducts();
            try
            {
                string DomainInfo = Webml.GetWebsiteInfo(Convert.ToInt32(WebsiteId)).AbsoluteHost;
                IProcedureAsync proc = new ProcGetNewArrivel(_dal);
                data = (NewArrivalOnSaleProducts)await proc.Call(new CommonReq
                {
                    CommonStr = WebsiteId,
                    CommonStr2 = DomainInfo
                }).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                throw new NotImplementedException();
            }
            return data;
        }
        public async Task<AllProductInfo> QuickViewApiAsync(int POSId)
        {
            AllProductInfo data = new AllProductInfo();
            try
            {
                string DomainInfo = Webml.GetWebsiteInfo(1).AbsoluteHost;
                IProcedureAsync proc = new ProcShoppingQuickViewApi(_dal);
                var result = (GetQuickViewProduct)await proc.Call(new CommonReq
                {
                    CommonInt = POSId,
                    CommonStr2 = DomainInfo
                }).ConfigureAwait(true);
                List<FilterWithOption> filterWithOption = (List<FilterWithOption>)result.FilterWithOption;
                var distinctfilter = filterWithOption.Distinct().Select(x => x.FilterName).ToList();
                List<FilterList> filterList = new List<FilterList>();
                foreach (var f in distinctfilter)
                {
                    filterList.Add(new FilterList
                    {
                        name = f,
                        option_Lists = filterWithOption.Where(x => x.FilterName == f)
                        .Select(x => new OptionList
                        {
                            Id = x.Id,
                            POSID = x.POSID,
                            OptionName = x.OptionName
                        }).ToList()
                    }); ;
                }
                List<AllProductInfo> allProductInfo = (List<AllProductInfo>)result.AllProductInfo;
                data = allProductInfo.FirstOrDefault();
                var files = new List<string>();

                data.filters = filterList;
            }
            catch (Exception ex)
            {
                throw new NotImplementedException();
            }
            return data;
        }
        public SubCategoryListResponse GetAllSubCategoryList(int id, int CategoryID)
        {
            SubCategoryListResponse data = new SubCategoryListResponse();
            List<SubCategoryProcResp> SubCategoryRepository = new List<SubCategoryProcResp>();
            try
            {
                string DomainInfo = Webml.GetWebsiteInfo(1).AbsoluteHost;
                IProcedureAsync proc = new ProcGetAllSubCategoryList(_dal);
                var result = (List<SubCategoryProcResp>)proc.Call(new CommonReq
                {
                    CommonInt = CategoryID,
                    CommonStr = DomainInfo
                }).Result;
                var v = result.FirstOrDefault();
                data.CategoryName = v.CategoryName;
                data.CategoryID = v.CategoryID;
                data.MainCategoryName = v.MainCategoryName;
                data.MainCategoryID = v.MainCategoryID;
                foreach (var item in result)
                {
                    var CategoryData = new SubCategoryProcResp
                    {
                        SubCategoryId = item.SubCategoryId,
                        SubcategoryName = item.SubcategoryName,
                        Image = item.Image,
                        CategoryName = item.CategoryName,
                        CategoryID = item.CategoryID,
                        MainCategoryName = item.MainCategoryName,
                        MainCategoryID = item.MainCategoryID

                    };
                    SubCategoryRepository.Add(CategoryData);
                }
                data.SubCategoryRepository = SubCategoryRepository;
            }
            catch (Exception ex)
            {
                throw new NotImplementedException();
            }
            return data;
        }
        public async Task<AllProductInfo> GetAllProductInfo(int POSId, string LoginId)
        {
            AllProductInfo data = new AllProductInfo();
            try
            {
                string DomainInfo = Webml.GetWebsiteInfo(1).AbsoluteHost;
                IProcedureAsync proc = new ProcShoppingQuickViewApi(_dal);
                var result = (GetQuickViewProduct)await proc.Call(new CommonReq
                {
                    CommonInt = POSId,
                    CommonStr = string.IsNullOrEmpty(LoginId) ? DomainInfo : LoginId,
                    CommonStr2 = DomainInfo
                }).ConfigureAwait(true);
                List<FilterWithOption> filterWithOption = (List<FilterWithOption>)result.FilterWithOption;
                var distinctfilter = filterWithOption.Distinct().Select(x => x.FilterName).ToList();
                List<FilterList> filterList = new List<FilterList>();
                List<KeyValyePairs> SpecificFeatures = new List<KeyValyePairs>();
                foreach (var f in distinctfilter)
                {
                    filterList.Add(new FilterList
                    {
                        name = f,
                        option_Lists = filterWithOption.Where(x => x.FilterName == f)
                        .Select(x => new OptionList
                        {
                            Id = x.Id,
                            POSID = x.POSID,
                            OptionName = x.OptionName
                        }).ToList()
                    });
                    SpecificFeatures.Add(new KeyValyePairs
                    {
                        Key = f,
                        Value = (from s in filterWithOption where s.FilterName == f select s.OptionName).FirstOrDefault()

                    });
                }
                List<AllProductInfo> allProductInfo = (List<AllProductInfo>)result.AllProductInfo;
                data = allProductInfo.FirstOrDefault();
                if (SpecificFeatures.Count > 0)
                {
                    data.SpecificFeatures = SpecificFeatures;
                }
                data.DefaultAddress = result.DefaultAddress;
                var files = new List<string>();
                var ProductID = data.ProductOptionSetId;
                string path = DOCType.ProductImagePath.Replace("{0}", ProductID.ToString());
                if (Directory.Exists(path))
                {
                    DirectoryInfo d = new DirectoryInfo(path);
                    FileInfo[] Files = d.GetFiles(POSId.ToString() + "_*-1x.png");
                    foreach (FileInfo file in Files)
                    {
                        files.Add(ProductID.ToString() + "/" + file.Name);
                    }
                }

                // data.sideImage = "http://85.10.235.153/Image/Products/"+files[0];

                data.filters = filterList;
            }
            catch (Exception ex)
            {
                throw new NotImplementedException();
            }
            return data;
        }
        public async Task<ProductOptionSetResponse> GetAllProductsetList(ProductSetInfo productSetInfo)
        {
            ProductOptionSetResponse data = new ProductOptionSetResponse();

            try
            {
                string filterOptionTypeIdList = "";
                if (productSetInfo.FilterOptionTypeIdList == null)
                {
                    productSetInfo.FilterOptionTypeIdList = new List<string>();
                }

                if (productSetInfo.FilterOptionTypeIdList != null)
                {
                    if (productSetInfo.FilterOptionTypeIdList.Count() > 1)
                    {
                        foreach (var item in productSetInfo.FilterOptionTypeIdList)
                        {
                            filterOptionTypeIdList += item + ",";
                        }
                        int count = filterOptionTypeIdList.LastIndexOf(',');
                        filterOptionTypeIdList = filterOptionTypeIdList.Substring(0, count);
                    }
                    else if (productSetInfo.FilterOptionTypeIdList.Count() == 1)
                    {
                        filterOptionTypeIdList = productSetInfo.FilterOptionTypeIdList[0];
                    }
                    else if (productSetInfo.FilterOptionTypeIdList.Count() == 0)
                    {
                        filterOptionTypeIdList = 0.ToString();
                    }
                }
                string DomainInfo = Webml.GetWebsiteInfo(1).AbsoluteHost;
                IProcedureAsync proc = new ProcGetAllProductsetList(_dal);
                var result = (List<ProductOptionSetInfoList>)await proc.Call(new CommonReq
                {
                    CommonStr = filterOptionTypeIdList,
                    CommonInt = productSetInfo.StartIndex,
                    CommonInt2 = productSetInfo.PageLimitIndex,
                    CommonStr5 = productSetInfo.WebsiteId,
                    CommonInt4 = productSetInfo.KeywordId,
                    CommonStr2 = productSetInfo.FilterType,
                    CommonInt3 = productSetInfo.FilterTypeId,
                    CommonStr3 = productSetInfo.OrderBy,
                    CommonStr4 = productSetInfo.OrderByType,
                    CommonStr1 = DomainInfo
                }).ConfigureAwait(true);

                data.PageLimitIndex = productSetInfo.PageLimitIndex;
                data.ProductSetList = result;
                data.CurrentIndex = productSetInfo.StartIndex;
                data.TotalRecords = result.Count;
            }
            catch (Exception ex)
            {
                throw new NotImplementedException();
            }
            return data;
        }
        public List<SimilarProducts> GetAllSimilarItems(int POSId)
        {
            var data = new List<SimilarProducts>();
            try
            {
                string DomainInfo = Webml.GetWebsiteInfo(1).AbsoluteHost;
                IProcedureAsync proc = new ProcGetAllSimilarItems(_dal);
                data = (List<SimilarProducts>)proc.Call(new CommonReq
                {
                    CommonInt = POSId,
                    CommonStr = DomainInfo
                }).Result;
            }
            catch (Exception ex)
            {
                throw new NotImplementedException();
            }
            return data;
        }
        public async Task<FilterListResponse> GetAllFilterList(FilterOptionRequest filterOptionRequest)
        {

            FilterListResponse data = new FilterListResponse();
            try
            {

                IProcedureAsync proc = new ProcGetAllFilterList(_dal);
                var result = (List<FilterOptionList>)await proc.Call(new CommonReq
                {
                    CommonStr = filterOptionRequest.FilterType,
                    CommonInt = filterOptionRequest.FilterTypeId,
                    CommonStr2 = filterOptionRequest.WebsiteId
                }).ConfigureAwait(true);

                var distinctfilter = result.Select(x => x.FilterName).Distinct().ToList();
                List<FilterList> filterList = new List<FilterList>();
                foreach (var f in distinctfilter)
                {
                    filterList.Add(new FilterList
                    {
                        name = f,
                        option_Lists = result.Where(x => x.FilterName == f)
                        .Select(x => new OptionList
                        {
                            Id = x.FilterOptionTypeId,
                            POSID = x.POSID,
                            OptionName = x.OptionName
                        }).ToList()
                    }); ;
                }
                data.filterLists = filterList;

                data.CategoryId = result.Count > 0 ? result.FirstOrDefault().CategoryId : 0;
            }
            catch (Exception ex)
            {
                throw new NotImplementedException();
            }
            return data;
        }
        public List<KeywordList> GetKeywordList(string WebsiteId, string SearchKeyword)
        {
            var data = new List<KeywordList>();
            try
            {
                string DomainInfo = Webml.GetWebsiteInfo(Convert.ToInt32(WebsiteId)).AbsoluteHost;
                IProcedureAsync proc = new ProcGetKeywordList(_dal);
                data = (List<KeywordList>)proc.Call(new CommonReq
                {
                    CommonStr2 = WebsiteId,
                    CommonStr1 = DomainInfo,
                    CommonStr = SearchKeyword
                }).Result;
            }
            catch (Exception ex)
            {
                throw new NotImplementedException();
            }
            return data;
        }
        public List<RecentViewModel> GetRecentViewItems(string WebsiteId, int CustomerId)
        {
            var data = new List<RecentViewModel>();
            try
            {
                IProcedureAsync proc = new ProcGetRecentViewItems(_dal);
                data = (List<RecentViewModel>)proc.Call(new CommonReq { CommonStr = WebsiteId, CommonInt2 = CustomerId }).Result;
            }
            catch (Exception ex)
            {
                throw new NotImplementedException();
            }
            return data;
        }
        public List<WishListResponse> GetAllWishListItems(string WebsiteId, int CustomerId)
        {
            var data = new List<WishListResponse>();
            try
            {
                string DomainInfo = Webml.GetWebsiteInfo(Convert.ToInt32(WebsiteId)).AbsoluteHost;
                IProcedureAsync proc = new ProcGetAllWishListItems(_dal);
                data = (List<WishListResponse>)proc.Call(new CommonReq
                {
                    CommonStr = WebsiteId,
                    CommonInt2 = CustomerId,
                    CommonStr1 = DomainInfo
                }).Result;
            }
            catch (Exception ex)
            {
                throw new NotImplementedException();
            }
            return data;
        }
        public async Task<FilterListResponse> GetSearchKeyword(KeywordSearch keywordSearch)
        {

            FilterListResponse data = new FilterListResponse();

            try
            {

                IProcedureAsync proc = new ProcGetAllFilterList(_dal);
                var result = (List<FilterOptionList>)await proc.Call(new CommonReq
                {
                    //CommonStr = filterOptionRequest.FilterType,
                    //CommonInt = filterOptionRequest.FilterTypeId,
                    //CommonStr2 = filterOptionRequest.WebsiteId
                }).ConfigureAwait(true);


            }
            catch (Exception ex)
            {
                throw new NotImplementedException();
            }
            return data;
        }

        public List<RecentViewModel> GetRecentViewItems(string BrowserId, string WebsiteId)
        {
            var data = new List<RecentViewModel>();
            try
            {
                string DomainInfo = Webml.GetWebsiteInfo(Convert.ToInt32(WebsiteId)).AbsoluteHost;
                IProcedureAsync proc = new ProcGetRecentViewItems(_dal);
                data = (List<RecentViewModel>)proc.Call(new CommonReq
                {
                    CommonStr = WebsiteId,
                    CommonStr1 = DomainInfo,
                    CommonStr2 = BrowserId
                }).Result;
            }
            catch (Exception ex)
            {
                throw new NotImplementedException();
            }
            return data;
        }
        public CheckPinCodeStatus CheckDelivery(string PinCode)
        {
            var _APICode = "DELHIVERY";
            var data = new CheckPinCodeStatus();
            try
            {
                var DVsetting = new DelhiveryAppSetting
                {
                    Token = Configuration["SHOPPING:" + _APICode + ":Token"],
                    BaseURL = Configuration["SHOPPING:" + _APICode + ":BaseURL"],
                };

                string DeliveryUrl = DVsetting.BaseURL + DVsetting.Token;
                DeliveryUrl += "&filter_codes=" + PinCode;
                string Response = AppWebRequest.O.CallUsingHttpWebRequest_GET(DeliveryUrl);
                RootObject rootObject = JsonConvert.DeserializeObject<RootObject>(Response);
                if (rootObject.delivery_codes.Count() > 0)
                {
                    bool Deliveryable = false;
                    foreach (var item in rootObject.delivery_codes)
                    {
                        if (item.postal_code.pickup == "Y")
                        {
                            Deliveryable = true;
                        }
                    }
                    if (Deliveryable)
                    {
                        data.City = rootObject.delivery_codes.FirstOrDefault().postal_code.district;
                        data.State = rootObject.delivery_codes.FirstOrDefault().postal_code.state_code;
                        return data;
                    }

                }
            }
            catch (Exception ex)
            {
                throw new NotImplementedException();
            }
            return data = null;
        }



        public ResponseStatus AddRecentViewProduct(int UserID, string BrowserId, int ProductDetailID)
        {
            var data = new ResponseStatus();
            try
            {
                IProcedureAsync proc = new ProcSaveRecentView(_dal);
                data = (ResponseStatus)proc.Call(new CommonReq
                {
                    CommonInt = UserID,
                    CommonStr = BrowserId,
                    CommonInt2 = ProductDetailID

                }).Result;
            }
            catch (Exception ex)
            {
                throw new NotImplementedException();
            }
            return data;
        }

        #region ShoppingHeaderMenu nafd footer

        public ShoppingHeaderMenus ShoppingHeaderMenus(string WebsiteId)
        {
            int id = 1;
            var data = new ShoppingHeaderMenus();
            try
            {

                string DomainInfo = Webml.GetWebsiteInfo(id).AbsoluteHost;
                IProcedureAsync proc = new ProcGetWebsiteInfo(_dal);
                var result = (GetAllMenu)proc.Call(new CommonReq
                {
                    CommonStr = WebsiteId,
                    CommonStr2 = DomainInfo
                }).Result;
                List<Menu> menuList = new List<Menu>();
                WebSite ws = new WebSite();
                ws = result.websiteSettings.FirstOrDefault();
                id++;
                int countMainCat = id++;
                foreach (var item in result.maincategoryMenus)
                {
                    List<CategoryList> categoryList = new List<CategoryList>();
                    IEnumerable<SubCategory> SubCategory = new List<SubCategory>();
                    var subCat = result.subCategoryMenus.Where(x => x.ParentId == 0 && x.mainCategoryID == item.MainCategoryID).ToList();
                    countMainCat++;
                    var mainMenu = new Model.Shopping.WebShopping.ViewModel.Menu
                    {
                        Id = countMainCat,
                        MainCategoryID = item.MainCategoryID,
                        Name = item.Name,
                        CategoryString = string.Join(",", subCat.Select(x => x.Name).ToList()),
                        MainCategoryImage = item.MainCategoryImage,
                        icon = item.icon,
                        Active = item.Active
                    };
                    foreach (var sub in subCat)
                    {
                        CategoryList cat = new CategoryList();
                        TopCategories TOPcat = new TopCategories();
                        countMainCat++;
                        cat.CategoryId = sub.SubCategoryId;
                        cat.Id = countMainCat;
                        cat.mainCategoryID = sub.mainCategoryID;
                        cat.Subcategory = sub.Subcategory;
                        // cat.SubCategoryId = sub.SubCategoryId;
                        cat.Name = sub.Name;
                        cat.IsActive = sub.IsActive;
                        cat.image = sub.image;
                        TOPcat.CategoryID = sub.SubCategoryId;
                        TOPcat.ParentId = sub.ParentId;
                        TOPcat.MainCategoryID = sub.mainCategoryID;
                        TOPcat.Name = sub.Name;
                        TOPcat.Image = sub.image;
                        List<SubCategory> subCategoryList = new List<SubCategory>();
                        List<SubCategoryMenus> subCategoryMenusList = new List<SubCategoryMenus>();
                        foreach (var sub2 in result.subCategoryMenus.Where(x => x.ParentId == sub.SubCategoryId))
                        {
                            countMainCat++;
                            subCategoryList.Add(new SubCategory
                            {
                                Id = countMainCat,
                                CategoryID = sub2.mainCategoryID,
                                ParentId = sub.SubCategoryId,
                                SubCategoryId = sub2.SubCategoryId,
                                Name = sub2.Name,
                                IsActive = sub2.IsActive,
                                subCategoryImage = sub2.image
                            });

                            subCategoryMenusList.Add(new SubCategoryMenus
                            {
                                Name = sub2.Name,
                                SubCategoryId = sub2.SubCategoryId
                            });
                        }
                        cat.Subcategory = subCategoryList;
                        TOPcat.subcategory = subCategoryMenusList;
                        categoryList.Add(cat);
                    }
                    mainMenu.CategoryList = categoryList;
                    menuList.Add(mainMenu);
                }
                data.menus = menuList;
                ResourceML rml = new ResourceML(_accessor, _env);
                ws.Logo = rml.GetLogoURL(Convert.ToInt32(WebsiteId)).ToString();
                data.Logo = ws.Logo;
            }
            catch (Exception ex)
            {
                throw new NotImplementedException();
            }
            return data;
        }


        public WebSite ShoppingWebsiteInfo(string WebsiteId)
        {
            int id = 1;
            var data = new WebSite();
            try
            {

                string DomainInfo = Webml.GetWebsiteInfo(id).AbsoluteHost;
                IProcedureAsync proc = new ProcGetWebsiteInfo(_dal);
                var result = (GetAllMenu)proc.Call(new CommonReq
                {
                    CommonStr = WebsiteId,
                    CommonStr2 = DomainInfo
                }).Result;
                data = result.websiteSettings.FirstOrDefault();
                ResourceML rml = new ResourceML(_accessor, _env);
                data.WebsiteName = DomainInfo;

                data.Logo = rml.GetLogoURL(Convert.ToInt32(WebsiteId)).ToString();
            }
            catch (Exception ex)
            {
                throw new NotImplementedException();
            }
            return data;
        }

        public GetKeywordResponse ShoppingGetKeywordDetails(string Keyword = "")
        {
            int id = 1;
            var data = new GetKeywordResponse();
            try
            {

                string DomainInfo = Webml.GetWebsiteInfo(id).AbsoluteHost;
                IProcedureAsync proc = new ProcShoppingGetKeywordDetails(_dal);
                data = (GetKeywordResponse)proc.Call(new CommonReq
                {
                    CommonStr = Keyword,
                    CommonStr2 = DomainInfo
                }).Result;
            }
            catch (Exception ex)
            {
                throw new NotImplementedException();
            }
            return data;
        }

        #endregion



    }
}
