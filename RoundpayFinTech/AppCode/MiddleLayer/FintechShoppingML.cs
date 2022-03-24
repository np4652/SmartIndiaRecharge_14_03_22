using Fintech.AppCode;
using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.DL;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.Configuration;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.DL.Shopping;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.App;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Shopping;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Validators;
using NewShoping = RoundpayFinTech.AppCode.Model.Shopping.WebShopping.ViewModel;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public class FintechShoppingML : IFintechShoppingML
    {
        #region Gloabl Variables
        private readonly IConfiguration Configuration;
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly ISession _session;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        private readonly IRequestInfo _rinfo;
        private readonly WebsiteInfo _WInfo;
        private readonly IResourceML _resourceML;
        private readonly LoginResponse _lr;
        #endregion


        public FintechShoppingML(IHttpContextAccessor accessor, IHostingEnvironment env, bool IsInSession = true)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            _rinfo = new RequestInfo(_accessor, _env);

            if (IsInSession)
            {
                _session = _accessor.HttpContext.Session;
                bool IsProd = _env.IsProduction();
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory());
                builder.AddJsonFile((IsProd ? "appsettings.json" : "appsettings.Development.json"));
                builder.AddEnvironmentVariables();
                Configuration = builder.Build();
                _WInfo = GetWebsiteInfo();
                //  _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
                _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.ECommLoginResponse);
            }
            _resourceML = new ResourceML(_accessor, _env);
        }

        public WebsiteInfo GetWebsiteInfo()
        {
            string domain = _rinfo.GetDomain(Configuration);
            var _wi = _session.GetObjectFromJson<WebsiteInfo>(SessionKeys.WInfo);
            bool IsCall = true;
            if (_wi != null)
            {
                if (_wi.WebsiteName == domain && _wi.WID > 0)
                {
                    IsCall = false;
                }
            }
            if (IsCall)
            {
                ProcGetWebsiteInfoEComm procGetWebsiteInfo = new ProcGetWebsiteInfoEComm(_dal);
                _wi = (WebsiteInfo)procGetWebsiteInfo.Call(domain);
                _session.SetObjectAsJson(SessionKeys.WInfo, _wi);
            }
            var cInfo = _rinfo.GetCurrentReqInfo();
            _wi.AbsoluteHost = cInfo.Scheme + "://" + cInfo.Host + (cInfo.Port > 0 ? ":" + cInfo.Port : "");
            return _wi;
        }
        private void WriteLocalSession(object value, bool isPersistent)
        {
            CookieOptions options = new CookieOptions();
            if (isPersistent)
                options.Expires = DateTime.Now.AddDays(1);
            else
                options.Expires = DateTime.Now.AddHours(1);
            _accessor.HttpContext.Response.Cookies.Append(SessionKeys.ECommUserDetail, JsonConvert.SerializeObject(value), options);
        }

        private ECommUserDetail ReadLocalSession()
        {
            var v = _accessor.HttpContext.Request.Cookies[SessionKeys.ECommUserDetail];
            return v == null ? null : JsonConvert.DeserializeObject<ECommUserDetail>(v);
        }

        public bool IsShoppingDomain()
        {
            bool res = false;
            var _wi = _session.GetObjectFromJson<WebsiteInfo>(SessionKeys.WInfo);
            if (_wi != null && !string.IsNullOrEmpty(_wi.ShoppingDomain))
            {
                var cInfo = _rinfo.GetCurrentReqInfo();
                if (cInfo.Host != "localhost")
                {
                    if (cInfo.HostValue == _wi.ShoppingDomain)
                    {
                        res = true;
                    }
                }
                else if (cInfo.Host == "localhost" && cInfo.Path.ToLower() == "/shopping")
                    res = true;
            }
            return res;
        }


        public ResponseStatus DoLogin(LoginDetail loginDetail)
        {
            loginDetail.RequestIP = _rinfo.GetRemoteIP();
            loginDetail.Browser = _rinfo.GetBrowserFullInfo();
            loginDetail.WID = _WInfo.WID;
            var responseStatus = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidLogin
            };
            if (loginDetail.WID < 0)
            {
                responseStatus.Msg = ErrorCodes.NotRecogRouteLogin;
                return responseStatus;
            }
            var _p = new ProcLogin(_dal);
            var _lr = (LoginResponse)_p.Call(loginDetail);
            _lr.LoginTypeID = loginDetail.LoginTypeID;
            _lr.LoginType = LoginType.GetLoginType(loginDetail.LoginTypeID);
            responseStatus.Msg = _lr.Msg;
            if (_lr.ResultCode < 1)
            {
                if (_lr.UserID > 0)
                {
                    LoginML loginML = new LoginML(_accessor, _env);
                    var resCheckInvalidAttempt = loginML.CheckInvalidAttempt(_lr.LoginTypeID, _lr.UserID, true, false, true, loginDetail.CommonStr);
                    if (resCheckInvalidAttempt.Statuscode == ErrorCodes.Minus1)
                    {
                        if ((resCheckInvalidAttempt.CommonStr ?? "").Trim() != "")
                        {
                            var activationlink = loginML.GenerateActvationLink(_lr.LoginTypeID, _lr.UserID);
                            var emailBody = new StringBuilder();
                            emailBody.AppendFormat(ErrorCodes.SuspeciousMsg, activationlink);
                            IEmailML emailManager = new EmailML(_dal);
                            int WID = 0;
                            if (_lr.WID <= 0)
                            {
                                WID = loginDetail.WID;
                            }
                            else
                            {
                                WID = _lr.WID;
                            }
                            emailManager.SendMail(resCheckInvalidAttempt.CommonStr, null, ErrorCodes.Suspecious, emailBody.ToString(), WID, _resourceML.GetLogoURL(WID).ToString());
                        }
                        responseStatus.Msg = resCheckInvalidAttempt.Msg;
                    }
                }
                return responseStatus;
            }
            _session.SetObjectAsJson(SessionKeys.ECommLoginResponse, _lr);
            if (!string.IsNullOrEmpty(_lr.OTP))
            {
                IUserML uml = new UserML(_accessor, _env);
                var alertData = uml.GetUserDeatilForAlert(_lr.UserID);
                alertData.FormatID = MessageFormat.OTP;
                if (alertData.Statuscode == ErrorCodes.One)
                {
                    IAlertML alertMl = new AlertML(_accessor, _env);
                    Parallel.Invoke(() => alertData.OTP = _lr.OTP,
                    () => alertMl.OTPSMS(alertData),
                    () => alertMl.OTPEmail(alertData),
                    () => alertMl.SocialAlert(alertData));
                }
                responseStatus.Statuscode = LoginResponseCode.OTP;
                responseStatus.Msg = "Enter OTP!";
                return responseStatus;
            }
            _session.SetString(SessionKeys.ECommAppSessionID, _lr.SessionID);
            CookieHelper cookie = new CookieHelper(_accessor);
            byte[] SessionID = Encoding.ASCII.GetBytes(_lr.SessionID);
            cookie.Set(SessionKeys.ECommAppSessionID, Base64UrlTextEncoder.Encode(SessionID), _lr.CookieExpire);
            responseStatus.Statuscode = LoginResponseCode.SUCCESS;
            responseStatus.Msg = "Login successfull!";
            return responseStatus;
        }

        public ResponseStatus RedirectLoginCheck(int WId, string SessionId, int UserId)
        {
            var responseStatus = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidLogin
            };
            if (WId < 0)
            {
                responseStatus.Msg = ErrorCodes.AuthError;
                return responseStatus;
            }
            var _ftSession = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse); // check this
            if (_ftSession != null && _ftSession.SessionID == SessionId && _ftSession.UserID == UserId && _WInfo != null && _WInfo.WID == WId)
            {
                var req = new LoginDetail
                {
                    RequestIP = _rinfo.GetRemoteIP(),
                    Browser = _rinfo.GetBrowserFullInfo(),
                    WID = WId,
                    SessionID = SessionId,
                    LoginID = UserId
                };
                var _p = new ProcVerifyECommLoginRedirect(_dal);
                var res = (LoginResponse)_p.Call(req);
                responseStatus.Msg = res.Msg;
                if (res.ResultCode == ErrorCodes.One)
                {
                    res.LoginTypeID = _ftSession.LoginTypeID;
                    res.LoginType = LoginType.GetLoginType(_ftSession.LoginTypeID);
                    _session.SetObjectAsJson(SessionKeys.ECommLoginResponse, res);
                    _session.SetString(SessionKeys.ECommAppSessionID, res.SessionID);
                    CookieHelper cookie = new CookieHelper(_accessor);
                    byte[] SessionID = Encoding.ASCII.GetBytes(res.SessionID);
                    cookie.Set(SessionKeys.ECommAppSessionID, Base64UrlTextEncoder.Encode(SessionID), res.CookieExpire);
                    responseStatus.Statuscode = ErrorCodes.One;
                }
            }
            return responseStatus;
        }

        public async Task<IResponseStatus> DoLogout(int ULT, int UserID, int SType)
        {
           
            LogoutReq logoutReq = new LogoutReq
            {
                LT =_lr!=null?_lr.LoginTypeID:0,
                LoginID = _lr != null ? _lr.UserID:0,
                ULT = ULT == 0 ? _lr.LoginTypeID : ULT,
                UserID = UserID == 0 ? _lr.UserID : UserID,
                SessID = _lr != null ? _lr.SessID:0,
                SessionType = SType == 0 ? SessionType.Single : SType,
                RequestMode = RequestMode.PANEL,
                IP = _rinfo.GetRemoteIP(),
                Browser = _rinfo.GetBrowserFullInfo()
            };
            var resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            IProcedureAsync procLogout = new ProcLogout(_dal);
            resp = (ResponseStatus)await procLogout.Call(logoutReq);
            return resp;
        }

        public UserBalnace GetUserBalnace(int UID = 0)
        {
            var _res = new UserBalnace();
            if (_lr != null || UID > 0)
            {
                var commonReq = new CommonReq
                {
                    LoginID = UID == 0 ? _lr.UserID : UID
                };
                IProcedure proc = new ProcGetUserBal(_dal);
                _res = (UserBalnace)proc.Call(commonReq);
            }
            return _res;
        }

        public UserInfoModel GetUserInfo()
        {
            var res = new UserInfoModel
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            if (_lr != null)
            {
                res.Statuscode = ErrorCodes.One;
                res.Msg = ErrorCodes.SUCCESS;
                res.Name = _lr.Name;
                res.MobileNo = _lr.MobileNo;
                res.OutletName = _lr.OutletName;

                var localSession = ReadLocalSession();
                if (localSession != null)
                {
                    string cartDet = string.Empty;
                    string wishList = string.Empty;
                    if (localSession.CartDetail != null)
                    {
                        StringBuilder sb = new StringBuilder(cartDet);
                        var counter = 0;
                        foreach (var item in localSession.CartDetail)
                        {
                            if (counter != 0)
                            {
                                sb.Append(",");
                            }
                            sb.Append(item.ProductDeatilID.ToString());
                            sb.Append("_");
                            sb.Append(item.Quantity.ToString());
                            counter++;
                        }
                        cartDet = sb.ToString();
                    }
                    if (localSession.Wishlist != null)
                    {
                        StringBuilder sb = new StringBuilder();
                        var counter = 0;
                        foreach (var item in localSession.Wishlist)
                        {
                            if (counter != 0)
                            {
                                sb.Append(",");
                            }
                            sb.Append(item.ToString());
                            counter++;
                        }
                        wishList = sb.ToString();
                    }

                    var req = new CommonReq
                    {
                        LoginID = _lr.UserID,
                        CommonStr = cartDet,
                        CommonStr2 = wishList
                    };
                    IProcedure proc = new ProcUpdateECommLocalSession(_dal);
                    var resDb = (IResponseStatus)proc.Call(req);
                    if (resDb.Status == ErrorCodes.One)
                    {
                        WriteLocalSession(null, false);
                    }
                }
            }
            return res;
        }

        public IEnumerable<ShoppingMenu> GetShoppingMenu()
        {
            IShoppingML mL = new ShoppingML(_accessor, _env);
            var Category = mL.GetShoppingMainCategoryNew().Where(lst => lst.ProductCount > 0);
            var subCategoryLvl1 = mL.GetCategory(0).Where(lst => lst.ProductCount > 0);
            var subCategoryLvl2 = mL.GetSubCategoryNew(0).Where(lst => lst.ProductCount > 0);
            var MenuLevel1 = new List<MenuLevel1>();
            foreach (var item in subCategoryLvl1)
            {
                MenuLevel1.Add(new MenuLevel1
                {
                    CategoryID = item.mainCategoryID,
                    SubCategoryID = item.SubCategoryId,
                    SubCategoryName = item.Name,
                    MenuLevel2 = subCategoryLvl2.Where(x => x.ParentId == item.SubCategoryId),
                    FilePath = item.image
                });
            }
            var Menu = new List<ShoppingMenu>();
            var menuType = GetShoppingSettings().DefaultMenuLevel;
            foreach (var item in Category)
            {
                Menu.Add(new ShoppingMenu
                {
                    MenuType = menuType,
                    MainCategoryID = item.MainCategoryID,
                    Name = item.Name,
                    MenuLevel1 = MenuLevel1.Where(x => x.CategoryID == item.MainCategoryID),
                    MainCategoryImage = item.MainCategoryImage
                });
            }
            return Menu;
        }

        public IEnumerable<ShoppingPincodeDetail> GetAreaByPincode(int Pincode, int UserId = 0, int LoginTypeId = 0)
        {
            var req = new CommonReq();
            if (UserId == 0 && LoginTypeId == 0)
            {
                req.LoginID = _lr.UserID;
                req.LoginTypeID = _lr.LoginTypeID;
            }
            else
            {
                req.LoginID = UserId;
                req.LoginTypeID = LoginTypeId;
            }
            req.CommonInt = Pincode;
            IProcedure proc = new ProcGetAreaByPincode(_dal);
            var res = (List<ShoppingPincodeDetail>)proc.Call(req);
            return res;
        }

        public ShoppingSettings GetShoppingSettings()
        {
            var res = new ShoppingSettings()
            {
                DefaultMenuLevel = 0
            };
            var req = new CommonReq();
            IProcedure proc = new ProcGetShoppingSettings(_dal);
            res = (ShoppingSettings)proc.Call(req);
            return res;
        }

        public IEnumerable<ShoppingCategory> GetShoppingCategory(int id = 0)
        {
            CommonReq request = new CommonReq()
            {
                LoginID = _lr != null ? _lr.UserID : 0,
                LoginTypeID = _lr != null ? _lr.LoginTypeID : 0,
                CommonInt = id
            };
            IProcedure proc = new ProcGetShoppingCategoryById(_dal);
            var response = (IEnumerable<ShoppingCategory>)proc.Call(request);
            return response;
        }

        public IEnumerable<FilterWithOptions> GetAvailableFilter(int PID, int PDetailId)
        {
            var req = new CommonReq
            {
                CommonInt = PID,
                CommonInt2 = PDetailId
            };
            IProcedure proc = new ProcGetAvailableFilter(_dal);
            var response = (FilterAndOptions)proc.Call(req);
            var res = new List<FilterWithOptions>();
            foreach (var item in response.Filter)
            {
                res.Add(new FilterWithOptions
                {
                    FilterID = item.ID,
                    FilterName = item.FilterName,
                    FilterOption = response.FilterOption.Where(x => x.FilterID == item.ID)
                });
            }
            return res;
        }

        public IEnumerable<Brand> GetBrand(int cid, int sid = 0)
        {
            var req = new CommonReq
            {
                LoginID = _lr != null ? _lr.UserID : 0,
                CommonInt = cid,
                CommonInt2 = sid
            };
            IProcedure proc = new ProcGetBrand(_dal);
            var response = (IEnumerable<Brand>)proc.Call(req);
            return response;
        }

        #region Shipping
        public ShippingAddress AddShippingAddress(SAddress param)
        {
            param.LoginID = _lr != null ? _lr.UserID : param.LoginID;
            IProcedure proc = new ProcAddShippingAddress(_dal);
            var res = (ShippingAddress)proc.Call(param);
            return res;
        }
        public IEnumerable<ShippingAddress> GetShippingAddresses(int UserID = 0)
        {
            IProcedure proc = new ProcGetShippingAddresses(_dal);
            var response = (IEnumerable<ShippingAddress>)proc.Call(UserID > 0 ? UserID : _lr.UserID);
            return response;
        }
        public IEnumerable<StateMaster> States()
        {
            IProcedure proc = new ProcGetState(_dal);
            var res = (List<StateMaster>)proc.Call();
            return res;
        }
        public IEnumerable<City> Cities(int StateID)
        {
            IProcedure proc = new ProcGetCity(_dal);
            var res = (List<City>)proc.Call(StateID);
            return res;
        }
        public ShoppingShipping GetShippingAddress(int ID)
        {
            if (_lr.LoginTypeID == LoginType.ApplicationUser)
            {
                var commonReq = new CommonReq
                {
                    LoginID = _lr.UserID,
                    LoginTypeID = _lr.LoginTypeID,
                    CommonInt = ID
                };
                IProcedure _proc = new ProcGetShippingDetail(_dal);
                return (ShoppingShipping)_proc.Call(commonReq);
            }
            return new ShoppingShipping();
        }
        #endregion

        public BannerList GetBanners()
        {
            var res = new BannerList();
            var imgList = GetFEImageList(0, 0);
            if (imgList.Statuscode == ErrorCodes.One)
            {
                res.Banners = imgList.ImgList.Where(e => e.ImgType == EFEImgType.Banner && e.IsActive == true).ToList();
                res.SideUpper = imgList.ImgList.Where(e => e.ImgType == EFEImgType.SideUpper && e.IsActive == true).FirstOrDefault();
                res.SideLower = imgList.ImgList.Where(e => e.ImgType == EFEImgType.SideLower && e.IsActive == true).FirstOrDefault();
            }
            return res;
        }

        public IEnumerable<ProductDetail> GetProductForIndex(ProductFilter p)
        {
            p.UserId = _lr != null ? _lr.UserID : p.UserId;
            p.LoginTypeId = _lr != null ? _lr.LoginTypeID : p.LoginTypeId;
            IProcedure proc = new ProcProductForIndex(_dal);
            var response = (IEnumerable<ProductDetail>)proc.Call(p);
            return response;
        }

        public IEnumerable<ProductDetail> GetProductTrending(ProductFilter p)
        {
            p.UserId = _lr != null ? _lr.UserID : p.UserId;
            p.LoginTypeId = _lr != null ? _lr.LoginTypeID : p.LoginTypeId;
            IProcedure proc = new ProcProductTrending(_dal);
            var response = (IEnumerable<ProductDetail>)proc.Call(p);
            return response;
        }

        public IEnumerable<ProductDetail> GetProductNewArrival(ProductFilter p)
        {
            p.UserId = _lr != null ? _lr.UserID : p.UserId;
            p.LoginTypeId = _lr != null ? _lr.LoginTypeID : p.LoginTypeId;
            IProcedure proc = new ProcProductNewArrival(_dal);
            var response = (IEnumerable<ProductDetail>)proc.Call(p);
            return response;
        }

        public IEnumerable<ProductDetail> GetProductSimilar(ProductFilter p)
        {
            p.UserId = _lr != null ? _lr.UserID : p.UserId;
            p.LoginTypeId = _lr != null ? _lr.LoginTypeID : p.LoginTypeId;
            IProcedure proc = new ProcProductSimilar(_dal);
            var response = (IEnumerable<ProductDetail>)proc.Call(p);
            return response;
        }

        public IEnumerable<ProductDetail> GetFilteredProduct(ProductFilter p)
        {
            p.UserId = _lr != null ? _lr.UserID : p.UserId;
            p.LoginTypeId = _lr != null ? _lr.LoginTypeID : p.LoginTypeId;
            string FIds = string.Empty, OIds = String.Empty;
            if (p.Filters != null && p.Filters.Count > 0)
            {
                foreach (string str in p.Filters)
                {
                    FIds = string.IsNullOrEmpty(FIds) ? str : FIds + "," + str;
                    //FIds = string.IsNullOrEmpty(FIds) ? str.Split('_')[0] : FIds + "," + str.Split('_')[0];
                    //OIds = string.IsNullOrEmpty(OIds) ? str.Split('_')[1] : OIds + "," + str.Split('_')[1];
                }
            }
            p.FilterIds = FIds;
            p.OptionIds = OIds;
            IProcedure proc = new ProcFilteredProduct(_dal);
            var response = (IEnumerable<ProductDetail>)proc.Call(p);
            return response;
        }

        public async Task<OnChangeFilter> OnPageChangeFilter(int ProductID, int ProductDeatilID, string Filters)
        {
            var req = new AddToCartRequest
            {
                LoginID = _lr != null ? _lr.UserID : 0,
                ProductDeatilID = ProductDeatilID,
                ProductID = ProductID,
                FilterIds = Filters
            };
            IProcedureAsync proc = new ProcOnChangeFilter(_dal);
            var res = (OnChangeFilter)await proc.Call(req);
            return res;
        }
        public async Task<ProductDetailForUser> ProDescription(int ProductDeatilID, int UserID = 0)
        {
            var req = new CommonReq
            {
                UserID = _lr == null ? UserID : _lr.UserID,
                CommonInt2 = ProductDeatilID,
            };
            IProcedureAsync proc = new ProcProDescription(_dal);
            var res = (ProductDetailForUser)await proc.Call(req);
            res.ProductDetailID = ProductDeatilID;
            res.FilterDetail = GetAvailableFilter(res.ProductID, ProductDeatilID);
            foreach (var item in res.FilterDetail)
            {
                foreach (var o in item.FilterOption)
                {
                    if (res.selectedOption.Any(x => x.FilterID == item.FilterID && x.FilterOptionID == o.FilterOptionID))
                    {
                        o.IsSelected = true;
                        break;
                    }
                }
            }
            var files = new List<string>();
            try
            {
                string path = DOCType.ProductImagePath.Replace("{0}", res.ProductID.ToString());
                if (Directory.Exists(path))
                {
                    DirectoryInfo d = new DirectoryInfo(path);
                    FileInfo[] Files = d.GetFiles(ProductDeatilID.ToString() + "_*-1x.png");
                    foreach (FileInfo file in Files)
                    {
                        files.Add(res.ProductID.ToString() + "/" + file.Name);
                    }
                }
                res.Files = files;
            }
            catch (Exception ex)
            {
                res.Error = ex.Message;
            }
            return res;
        }
        public FEImgList GetFEImageList(int id, int catId)
        {
            var req = new CommonReq
            {
                LoginTypeID = _lr == null ? 1 : _lr.LoginTypeID,
                LoginID = _lr == null ? 0 : _lr.UserID,
                CommonInt = id,
                CommonInt2 = catId
                //CommonStr = filename,
                //CommonInt3 = imgType,
                //CommonBool = true
            };
            IProcedure proc = new ProcGetFEImg(_dal);
            var response = (FEImgList)proc.Call(req);
            return response;
        }

        public IEnumerable<FilterWithOptions> GetRequeiredFilter(int s2id, int sid = 0)
        {
            var req = new CommonReq
            {
                LoginID = _lr != null ? _lr.UserID : 0,
                CommonInt = s2id,
                CommonInt2 = sid
            };
            IProcedure proc = new ProcGetFilterWithOption(_dal);
            var response = (IEnumerable<FilterWithOptions>)proc.Call(req);
            return response;
        }

        #region New Shopping Main Category
        public IEnumerable<NewShoping.Menu> GetShoppingMainCategoryNew(int id = 0)
        {
            CommonReq request = new CommonReq()
            {
                LoginID = _lr != null ? _lr.UserID : 0,
                LoginTypeID = _lr != null ? _lr.LoginTypeID : 0,
                CommonInt = id
            };
            IProcedure proc = new ProcGetShoppingCategoryByIdNew(_dal);
            var response = (IEnumerable<NewShoping.Menu>)proc.Call(request);
            return response;
        }
        public NewShoping.Menu GetShoppingMainCategoryByIDNew(int id)
        {
            var req = new CommonReq
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID,
                CommonInt = id
            };
            IProcedure proc = new ProcGetShoppingCategoryByIDNew(_dal);
            var response = (NewShoping.Menu)proc.Call(req);
            return response;
        }
        public ResponseStatus UpdateShoppingMainCategoryNew(CommonReq req)
        {
            req.LoginID = _lr.UserID;
            req.LoginTypeID = _lr.LoginTypeID;
            IProcedure proc = new ProcUpdateShoppingCategoryNew(_dal);
            ResponseStatus response = (ResponseStatus)proc.Call(req);
            return response;
        }
        #endregion

        #region New Shopping Category
        public ResponseStatus UpdateShoppingCategoryNew(CommonReq req)
        {
            req.LoginID = _lr.UserID;
            req.LoginTypeID = _lr.LoginTypeID;
            IProcedure proc = new ProcUpdateCategoryNew(_dal);
            ResponseStatus response = (ResponseStatus)proc.Call(req);
            return response;
        }
        public IEnumerable<NewShoping.CategoryList> GetCategory(int cid = 0, int id = 0)
        {
            var req = new CommonReq
            {
                LoginID = _lr != null ? _lr.UserID : 0,
                LoginTypeID = _lr != null ? _lr.LoginTypeID : 0,
                CommonInt = cid,
                CommonInt2 = id
            };
            IProcedure proc = new ProcGetCategoryByIdNew(_dal);
            var response = (IEnumerable<NewShoping.CategoryList>)proc.Call(req);
            return response;
        }
        public NewShoping.CategoryList GetShoppingCategoryByIDNew(int id)
        {
            var req = new CommonReq
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID,
                CommonInt = id
            };
            IProcedure proc = new ProcGetShoppingCategoryLvl1ByIDNew(_dal);
            var response = (NewShoping.CategoryList)proc.Call(req);
            return response;
        }
        #endregion

        #region New Shopping Sub Category
        public ResponseStatus UpdateSubCategoryNew(CommonReq req)
        {
            req.LoginID = _lr.UserID;
            req.LoginTypeID = _lr.LoginTypeID;
            IProcedure proc = new ProcUpdateSubCategoryNew(_dal);
            ResponseStatus response = (ResponseStatus)proc.Call(req);
            return response;
        }
        public IEnumerable<NewShoping.SubCategory> GetSubCategoryNew(int sid = 0, int id = 0)
        {
            var req = new CommonReq
            {
                LoginID = _lr != null ? _lr.UserID : 0,
                LoginTypeID = _lr != null ? _lr.LoginTypeID : 0,
                CommonInt = sid,
                CommonInt2 = id
            };
            IProcedure proc = new ProcGetSubCategoryByIdNew(_dal);
            var response = (IEnumerable<NewShoping.SubCategory>)proc.Call(req);
            return response;
        }
        public ShoppingSubCategoryLvl2 GetSubCategoryByIDNew(int id)
        {
            var req = new CommonReq
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID,
                CommonInt = id
            };
            IProcedure proc = new ProcGetShoppingSubCategoryLvl2ByID(_dal);
            var response = (ShoppingSubCategoryLvl2)proc.Call(req);
            return response;
        }


        #endregion


        #region Category

        public ResponseStatus UpdateShoppingCategory(CommonReq req)
        {
            req.LoginID = _lr.UserID;
            req.LoginTypeID = _lr.LoginTypeID;
            IProcedure proc = new ProcUpdateShoppingCategory(_dal);
            ResponseStatus response = (ResponseStatus)proc.Call(req);
            return response;
        }
        public ShoppingCategory GetShoppingCategoryByID(int id)
        {
            var req = new CommonReq
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID,
                CommonInt = id
            };
            IProcedure proc = new ProcGetShoppingCategoryByID(_dal);
            var response = (ShoppingCategory)proc.Call(req);
            return response;
        }

        #endregion

        #region SubCategory Level-1
        public ResponseStatus UpdateShoppingSubCategoryLvl1(CommonReq req)
        {
            req.LoginID = _lr.UserID;
            req.LoginTypeID = _lr.LoginTypeID;
            IProcedure proc = new ProcUpdateSubCategoryLvl1(_dal);
            ResponseStatus response = (ResponseStatus)proc.Call(req);
            return response;
        }
        public IEnumerable<ShoppingSubCategoryLvl1> GetSubCategoryLvl1(int cid = 0, int id = 0)
        {
            var req = new CommonReq
            {
                LoginID = _lr != null ? _lr.UserID : 0,
                LoginTypeID = _lr != null ? _lr.LoginTypeID : 0,
                CommonInt = cid,
                CommonInt2 = id
            };
            IProcedure proc = new ProcGetSubCategoryLvl1ById(_dal);
            var response = (IEnumerable<ShoppingSubCategoryLvl1>)proc.Call(req);
            return response;
        }
        public ShoppingSubCategoryLvl1 GetShoppingSubCategoryByID(int id)
        {
            var req = new CommonReq
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID,
                CommonInt = id
            };
            IProcedure proc = new ProcGetShoppingSubCategoryLvl1ByID(_dal);
            var response = (ShoppingSubCategoryLvl1)proc.Call(req);
            return response;
        }
        #endregion

        #region SubCategory Level-2        
        public ResponseStatus UpdateSubCategoryLvl2(CommonReq req)
        {
            req.LoginID = _lr.UserID;
            req.LoginTypeID = _lr.LoginTypeID;
            IProcedure proc = new ProcUpdateSubCategoryLvl2(_dal);
            ResponseStatus response = (ResponseStatus)proc.Call(req);
            return response;
        }
        public IEnumerable<ShoppingSubCategoryLvl2> GetSubCategoryLvl2(int sid = 0, int id = 0)
        {
            var req = new CommonReq
            {
                LoginID = _lr != null ? _lr.UserID : 0,
                LoginTypeID = _lr != null ? _lr.LoginTypeID : 0,
                CommonInt = sid,
                CommonInt2 = id
            };
            IProcedure proc = new ProcGetSubCategoryLvl2ById(_dal);
            var response = (IEnumerable<ShoppingSubCategoryLvl2>)proc.Call(req);
            return response;
        }
        public ShoppingSubCategoryLvl2 GetSubCategoryLvl2ByID(int id)
        {
            var req = new CommonReq
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID,
                CommonInt = id
            };
            IProcedure proc = new ProcGetShoppingSubCategoryLvl2ByID(_dal);
            var response = (ShoppingSubCategoryLvl2)proc.Call(req);
            return response;
        }
        #endregion

        #region SubCategory Level-3
        public ResponseStatus UpdateSubCategoryLvl3(CommonReq req)
        {
            req.LoginID = _lr.UserID;
            req.LoginTypeID = _lr.LoginTypeID;
            IProcedure proc = new ProcUpdateSubCategoryLvl3(_dal);
            ResponseStatus response = (ResponseStatus)proc.Call(req);
            return response;
        }
        public IEnumerable<ShoppingSubCategoryLvl3> GetSubCategoryLvl3(int sid)
        {
            var req = new CommonReq
            {
                LoginID = _lr.UserID,
                CommonInt = sid
            };
            IProcedure proc = new ProcGetSubCategoryLvl3(_dal);
            var response = (IEnumerable<ShoppingSubCategoryLvl3>)proc.Call(req);
            return response;
        }
        public ShoppingSubCategoryLvl3 GetSubCategoryLvl3ByID(int id)
        {
            var req = new CommonReq
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID,
                CommonInt = id
            };
            IProcedure proc = new ProcGetShoppingSubCategoryLvl3ByID(_dal);
            var response = (ShoppingSubCategoryLvl3)proc.Call(req);
            return response;
        }
        #endregion

        #region Filter
        public ResponseStatus UpdateFilter(CommonReq req)
        {
            req.LoginID = _lr.UserID;
            req.LoginTypeID = _lr.LoginTypeID;
            IProcedure proc = new ProcAddShoppingFilter(_dal);
            ResponseStatus response = (ResponseStatus)proc.Call(req);
            return response;
        }
        public Filter GetFilterByID(int id)
        {
            var req = new CommonReq
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID,
                CommonInt = id
            };
            IProcedure proc = new ProcGetFilterByID(_dal);
            var response = (Filter)proc.Call(req);
            return response;
        }
        public IEnumerable<Filter> GetFilter()
        {
            var req = new CommonReq
            {
                LoginID = _lr.UserID
            };
            IProcedure proc = new ProcGetFilter(_dal);
            var response = (IEnumerable<Filter>)proc.Call(req);
            return response;
        }
        public IEnumerable<Filter> GetFilterForMapping(int CID)
        {
            var req = new CommonReq
            {
                LoginID = _lr.UserID,
                CommonInt = CID
            };
            IProcedure proc = new ProcGetMappedFilter(_dal);
            var response = (IEnumerable<Filter>)proc.Call(req);
            return response;
        }
        public ResponseStatus UpdateMappedFilter(CommonReq req)
        {
            req.LoginID = _lr.UserID;
            req.LoginTypeID = _lr.LoginTypeID;
            IProcedure proc = new ProcUpdateMappedFilter(_dal);
            ResponseStatus response = (ResponseStatus)proc.Call(req);
            return response;
        }
        public IEnumerable<UOM> GetUom()
        {
            IProcedure proc = new ProcGetUOM(_dal);
            var response = (IEnumerable<UOM>)proc.Call();
            return response;
        }
        public IEnumerable<Colors> GetColors()
        {
            IProcedure proc = new ProcGetColors(_dal);
            var response = (IEnumerable<Colors>)proc.Call();
            return response;
        }
        public IEnumerable<FilterOption> GetFilterOption(int FilterID)
        {
            var req = new CommonReq
            {
                LoginID = _lr.UserID,
                CommonInt = FilterID
            };
            IProcedure proc = new ProcGetFilterOptions(_dal);
            var response = (IEnumerable<FilterOption>)proc.Call(req);
            return response;
        }
        public ResponseStatus UpdateFilterOption(CommonReq req)
        {
            req.LoginID = _lr.UserID;
            req.LoginTypeID = _lr.LoginTypeID;
            IProcedure proc = new ProcupdateFilterOption(_dal);
            ResponseStatus response = (ResponseStatus)proc.Call(req);
            return response;
        }
        public FilterOption GetFilterOptionByID(int id)
        {
            var req = new CommonReq
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID,
                CommonInt = id
            };
            IProcedure proc = new ProcGetFilterOptionByID(_dal);
            var response = (FilterOption)proc.Call(req);
            return response;
        }

        #endregion

        #region Brand

        public ResponseStatus SaveBrand(Brand req)
        {
            req.LoginID = _lr.UserID;
            req.LT = _lr.LoginTypeID;
            IProcedure proc = new ProcSaveBrand(_dal);
            ResponseStatus response = (ResponseStatus)proc.Call(req);
            return response;
        }
        public IEnumerable<Brand> GetBranddetail(int CategoryID)
        {
            var req = new CommonReq
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID,
                CommonInt = CategoryID
            };
            IProcedure proc = new ProcGetBranddetail(_dal);
            var res = (List<Brand>)proc.Call(req);
            return res;
        }
        public IEnumerable<Brand> GetBrandById(int BrandId = 0)
        {
            var req = new CommonReq
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID,
                CommonInt = BrandId
            };
            IProcedure proc = new ProcGetBrandById(_dal);
            var res = (List<Brand>)proc.Call(req);
            return res;
        }
        #endregion

        #region Vendor
        public ResponseStatus SaveVendor(Vendors req)
        {
            req.LoginID = _lr.UserID;
            req.LT = _lr.LoginTypeID;
            IProcedure proc = new ProcSaveVendor(_dal);
            ResponseStatus response = (ResponseStatus)proc.Call(req);
            return response;
        }
        public IEnumerable<Vendors> GetVendors()
        {
            IProcedure proc = new ProcGetVendors(_dal);
            var response = (IEnumerable<Vendors>)proc.Call(_lr.UserID);
            return response;
        }
        public IEnumerable<VendorMaster> GetVendorList()
        {
            CommonReq req = new CommonReq
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID
            };
            IProcedure proc = new ProcGetVendorList(_dal);
            var response = (IEnumerable<VendorMaster>)proc.Call(req);
            return response;
        }

        public ResponseStatus ChangeECommVendorStatus(string id, bool val)
        {
            ResponseStatus res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            if (string.IsNullOrEmpty(id))
            {
                return res;
            }
            var _id = id.Split("_");
            CommonReq req = new CommonReq
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID,
                CommonInt = 0,
                CommonInt2 = Convert.ToInt32(_id[1]),
                CommonBool = val
            };
            if (_id[0] == "isAct") { req.CommonInt = 1; }
            else if (_id[0] == "isB2B") { req.CommonInt = 2; }
            else if (_id[0] == "isOB2B") { req.CommonInt = 3; }
            IProcedure proc = new ProcChangeEComVendorStatus(_dal);
            var response = (ResponseStatus)proc.Call(req);
            return response;
        }
        #endregion



        #region Product
        public ResponseStatus AddMasterProduct(MasterProduct req)
        {
            req.LoginID = _lr.UserID;
            req.LoginTypeID = _lr.LoginTypeID;
            IProcedure proc = new ProcAddMasterProduct(_dal);
            ResponseStatus response = (ResponseStatus)proc.Call(req);
            return response;
        }

        public IResponseStatus UploadProductImage(IFormFile file, int ProductID, int ProductDetailID, string ImgName, int count)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var filename = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
            if (filename != "fileName.png")
            {
                if (_lr.LoginTypeID == LoginType.ApplicationUser)
                {
                    if (!file.ContentType.Any())
                    {
                        _res.Msg = "File not found!";
                        return _res;
                    }
                    if (file.Length < 1)
                    {
                        _res.Msg = "Empty file not allowed!";
                        return _res;
                    }
                    if (file.Length / 1024 > 1024)
                    {
                        _res.Msg = "File size exceeded! Not more than 1 MB is allowed";
                        return _res;
                    }
                    //       var filename = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    string ext = Path.GetExtension(filename);
                    byte[] filecontent = null;
                    using (var ms = new MemoryStream())
                    {
                        file.CopyTo(ms);
                        filecontent = ms.ToArray();
                    }
                    if (!Validate.O.IsFileAllowed(filecontent, ext))
                    {
                        _res.Msg = "Invalid File Format!";
                        return _res;
                    }
                    try
                    {
                        StringBuilder sb = new StringBuilder();
                        StringBuilder sb2 = new StringBuilder();
                        sb.Append(DOCType.ProductImagePath.Replace("{0}", ProductID.ToString()));
                        if (!Directory.Exists(sb.ToString()))
                        {
                            Directory.CreateDirectory(sb.ToString());
                        }
                        sb.Append(ProductDetailID.ToString());
                        sb.Append("_");
                        sb.Append(ImgName.Split('#')[1]);
                        sb.Append("_");
                        sb.Append(count.ToString());
                        sb.Append("-1x");
                        sb.Append(".png");
                        sb2.Append(DOCType.ProductImagePath.Replace("{0}", ProductID.ToString()));
                        sb2.Append(ProductDetailID.ToString());
                        sb2.Append("_");
                        sb2.Append(ImgName.Split('#')[1]);
                        sb2.Append("_");
                        sb2.Append(count.ToString());
                        sb2.Append(".png");
                        //CompressImage.Compress_Image(file.OpenReadStream(), sb.ToString(), 150, 138);
                        //CompressImage.Compress_Image(file.OpenReadStream(), sb2.ToString(), 500, 350);
                        CompressImage.CompressImageByPercentage(file, sb.ToString(), 100L);
                        CompressImage.CompressImageByPercentage(file, sb2.ToString(), 50L);
                        _res.Statuscode = ErrorCodes.One;
                        _res.Msg = "Image uploaded successfully";
                    }
                    catch (Exception ex)
                    {
                        _res.Msg = "Error in Image uploading. Try after sometime...";
                        ErrorLog errorLog = new ErrorLog
                        {
                            ClassName = GetType().Name,
                            FuncName = "UploadLogo",
                            Error = ex.Message,
                            LoginTypeID = _lr.LoginTypeID,
                            UserId = _lr.UserID
                        };
                        var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                    }
                }
            }
            else
            {
                _res.Statuscode = ErrorCodes.One;
                _res.Msg = ErrorCodes.SUCCESS;
            }
            return _res;
        }

        public IResponseStatus UploadIcon(IFormFile file, int id, string pathConcat = "", string fileNameConcat = "")
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var filename = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
            if (filename != "fileName.png")
            {
                if (_lr.LoginTypeID == LoginType.ApplicationUser)
                {
                    if (!file.ContentType.Any())
                    {
                        _res.Msg = "File not found!";
                        return _res;
                    }
                    if (file.Length < 1)
                    {
                        _res.Msg = "Empty file not allowed!";
                        return _res;
                    }
                    if (file.Length / 1024 > 50)
                    {
                        _res.Msg = "File size exceeded! Not more than 50 KB is allowed";
                        return _res;
                    }
                    string ext = Path.GetExtension(filename);
                    byte[] filecontent = null;
                    using (var ms = new MemoryStream())
                    {
                        file.CopyTo(ms);
                        filecontent = ms.ToArray();
                    }
                    if (!Validate.O.IsFileAllowed(filecontent, ext))
                    {
                        _res.Msg = "Invalid File Format!";
                        return _res;
                    }
                    try
                    {
                        StringBuilder sb = new StringBuilder();
                        if (pathConcat != "")
                        {
                            sb.Append(DOCType.IconImagePath.Replace("{0}", pathConcat));
                        }
                        else
                        {
                            sb.Append(DOCType.IconImagePath.Replace("/{0}/", pathConcat));
                        }
                        if (!Directory.Exists(sb.ToString()))
                        {
                            Directory.CreateDirectory(sb.ToString());
                        }
                        if (fileNameConcat != "")
                        {
                            sb.Append(fileNameConcat);
                            sb.Append("_");
                        }
                        sb.Append(id.ToString());
                        sb.Append(ext);
                        var imagePath = sb.ToString();
                        var uploadResponse = CompressImage.UploadPngImage(file, imagePath);
                        if (uploadResponse)
                        {
                            _res.Statuscode = ErrorCodes.One;
                            _res.Msg = "Image uploaded successfully";
                        }
                        //CompressImage.CompressImageByPercentage(file, sb.ToString(), 100L);
                    }
                    catch (Exception ex)
                    {
                        _res.Msg = "Error in Image uploading. Try after sometime...";
                        ErrorLog errorLog = new ErrorLog
                        {
                            ClassName = GetType().Name,
                            FuncName = "UploadLogo",
                            Error = ex.Message,
                            LoginTypeID = _lr.LoginTypeID,
                            UserId = _lr.UserID
                        };
                        var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                    }
                }
            }
            else
            {
                _res.Statuscode = ErrorCodes.One;
                _res.Msg = ErrorCodes.SUCCESS;
            }
            return _res;
        }



        public IResponseStatus UploadFEImage(IFormFile file, CommonReq req)
        {
            // imgType = 0 => banner, 1=> sideUpper, 2=> sidelower, 4=>catOfferImg
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var validateResponse = new ResponseStatus();
            if (file != null)
            {
                validateResponse = Validate.O.IsImageValid(file);
                if (validateResponse.Statuscode == ErrorCodes.One)
                {
                    var filename = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    if (filename != "fileName.png")
                    {
                        if (_lr.LoginTypeID == LoginType.ApplicationUser)
                        {
                            if (!file.ContentType.Any())
                            {
                                _res.Msg = "File not found!";
                                return _res;
                            }
                            if (file.Length < 1)
                            {
                                _res.Msg = "Empty file not allowed!";
                                return _res;
                            }
                            if (file.Length / 1024 > 1024)
                            {
                                _res.Msg = "File size exceeded! Not more than 1 MB is allowed";
                                return _res;
                            }
                            string ext = Path.GetExtension(filename);
                            byte[] filecontent = null;
                            using (var ms = new MemoryStream())
                            {
                                file.CopyTo(ms);
                                filecontent = ms.ToArray();
                            }
                            if (!Validate.O.IsFileAllowed(filecontent, ext))
                            {
                                _res.Msg = "Invalid File Format!";
                                return _res;
                            }
                            try
                            {
                                req.LoginID = _lr.UserID;
                                req.LoginTypeID = _lr.LoginTypeID;
                                req.CommonStr = filename ?? "";
                                IProcedure proc = new ProcUpdateFEImg(_dal);
                                var response = (ResponseStatus)proc.Call(req);
                                if (response.Statuscode == ErrorCodes.One)
                                {
                                    StringBuilder sb = new StringBuilder();
                                    StringBuilder sb2 = new StringBuilder();
                                    sb.Append(DOCType.ECommFEImagePath);
                                    if (!Directory.Exists(sb.ToString()))
                                    {
                                        Directory.CreateDirectory(sb.ToString());
                                    }
                                    sb.Append(filename);
                                    //sb.Append(req.CommonInt3.ToString()); //append imageTYpe
                                    //sb.Append("_");
                                    //sb.Append(response.Msg.Trim());
                                    //sb.Append(ext);
                                    var imagePath = sb.ToString();
                                    var uploadResponse = CompressImage.UploadPngImage(file, imagePath, ext);
                                    if (uploadResponse)
                                    {
                                        _res.Statuscode = ErrorCodes.One;
                                        _res.Msg = imagePath;
                                    }
                                }
                                else
                                {
                                    _res.Msg = response.Msg;
                                    return _res;
                                }
                            }
                            catch (Exception ex)
                            {
                                _res.Msg = "Error in Image uploading. Try after sometime...";
                                ErrorLog errorLog = new ErrorLog
                                {
                                    ClassName = GetType().Name,
                                    FuncName = "UploadLogo",
                                    Error = ex.Message,
                                    LoginTypeID = _lr.LoginTypeID,
                                    UserId = _lr.UserID
                                };
                                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                            }
                        }
                    }
                    else
                    {
                        _res.Statuscode = ErrorCodes.One;
                        _res.Msg = ErrorCodes.SUCCESS;
                    }
                }
            }
            return _res;
        }



        public IResponseStatus UpdateFEImg(int id, bool isActive, bool isDelete)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var req = new CommonReq
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID,
                CommonInt = id,
                CommonBool = isActive,
                CommonBool1 = isDelete
            };
            IProcedure proc = new ProcUpdateFEImgStatus(_dal);
            res = (ResponseStatus)proc.Call(req);
            return res;
        }

        public IEnumerable<MasterProduct> GetMasterProduct(MasterProduct req)
        {
            IProcedure proc = new ProcGetMasterProduct(_dal);
            var response = (IEnumerable<MasterProduct>)proc.Call(req);
            return response;
        }
        public MasterProduct GetMasterProductById(int PID)
        {
            IProcedure proc = new ProcGetMasterProductByID(_dal);
            var response = (MasterProduct)proc.Call(PID);
            return response;
        }


        public ResponseStatus AddProduct(ProductDetail product)
        {
            var response = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            if (product == null)
            {
                response.Msg = "Invalid Parameters";
                goto Finish;
            }
            if (product.ProductID <= 0)
            {
                response.Msg = "Invalid Product";
                goto Finish;
            }
            if (product.BrandID <= 0)
            {
                response.Msg = "Invalid Brand";
                goto Finish;
            }
            if (product.VendorID <= 0)
            {
                response.Msg = "Invalid Vendor";
                goto Finish;
            }
            if (!Validate.O.IsDecimal(product.MRP) || product.MRP < 1)
            {
                response.Msg = "Invalid MRP";
                goto Finish;
            }
            if (!Validate.O.IsDecimal(product.Discount) || product.Discount < 0)
            {
                response.Msg = "Invalid Discount";
                goto Finish;
            }
            if (!Validate.O.IsDecimal(product.Commission))
            {
                response.Msg = "Invalid Commission";
                goto Finish;
            }
            if ((!Validate.O.IsNumeric(product.Quantity.ToString()) || product.Quantity < 1) && product.ProductDetailID == 0)
            {
                response.Msg = "Invalid Quantity";
                goto Finish;
            }
            DataTable dt = new DataTable();
            dt.Columns.Add("FilterID", typeof(int));
            dt.Columns.Add("OptionID", typeof(int));
            if (product.FilterWithOption != null && product.FilterWithOption.Count > 0)
            {
                foreach (string str in product.FilterWithOption)
                {
                    DataRow dr = dt.NewRow();
                    dr["FilterID"] = str.Split('_')[0];
                    dr["OptionID"] = str.Split('_')[1];
                    dt.Rows.Add(dr);
                }
                //response.Msg = "Invalid Filter Details";
                //goto Finish;
            }
            product.LoginID = _lr.UserID;
            product.LoginTypeID = _lr.LoginTypeID;
            product.FilterDetail = dt;
            IProcedure proc = new ProcAddProductDetail(_dal);
            response = (ResponseStatus)proc.Call(product);
            goto Finish;
        Finish:
            return response;
        }
        public IEnumerable<ProductDetail> GetProductDetails(int ProductID)
        {
            var req = new CommonReq
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID,
                CommonInt = ProductID
            };

            IProcedure proc = new ProcGetProductDetail(_dal);
            var response = (IEnumerable<ProductDetail>)proc.Call(req);
            return response;
        }


        public IEnumerable<ProductDetail> GetTrendingProducts()
        {
            IProcedure proc = new ProcGetTrendingProducts(_dal);
            var response = (IEnumerable<ProductDetail>)proc.Call();
            return response;

        }
        public ProductWithMaster GetAllProducts(int CID, int SID1, int SID2)
        {
            var m = new MasterProduct
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID,
                CategoryID = CID,
                SubCategoryID1 = SID1,
                SubCategoryID2 = SID2
            };
            //m.LoginID = _lr.UserID;
            //m.LoginTypeID = _lr.LoginTypeID;
            IProcedure proc = new ProcGetMasterProduct(_dal);
            var master = (IEnumerable<MasterProduct>)proc.Call(m);
            //var req = new CommonReq
            //{
            //    LoginID = _lr.UserID,
            //    LoginTypeID = _lr.LoginTypeID,
            //    CommonInt = 1,
            //};
            //IProcedure procDetail = new ProcGetProductDetail(_dal);
            //var proDetail = (IEnumerable<ProductDetail>)procDetail.Call(req);
            var res = new ProductWithMaster
            {
                MasterProduct = master,
                //,ProductDetail = proDetail
                ProductDetail = new List<ProductDetail>()
            };
            return res;
        }
        public async Task<OnChangeFilter> OnChangeFilter(int ProductID, int ProductDeatilID, List<string> Filters)
        {
            var req = new AddToCartRequest
            {
                LoginID = _lr.UserID,
                ProductDeatilID = ProductDeatilID,
                ProductID = ProductID,
                FilterIds = string.Join(",", Filters)
            };
            IProcedureAsync proc = new ProcOnChangeFilter(_dal);
            var res = (OnChangeFilter)await proc.Call(req);
            var resposne = ProDescription(res.ProductDetailID);
            return res;
        }

        public async Task<ProductDetailForUser> OnChangeFilterForApp(int ProductID, int ProductDeatilID, List<string> Filters, int UserID = 0)
        {
            var req = new AddToCartRequest
            {
                LoginID = UserID,
                ProductDeatilID = ProductDeatilID,
                ProductID = ProductID,
                FilterIds = string.Join(",", Filters)
            };
            IProcedureAsync proc = new ProcOnChangeFilter(_dal);
            var res = (OnChangeFilter)await proc.Call(req);
            var resposne = ProDescription(res.ProductDetailID).Result;
            return resposne;
        }


        public async Task<AddProductModal> AddProductModal(int ProductDetailID)
        {
            var param = new CommonReq
            {
                LoginID = _lr.UserID,
                CommonInt = ProductDetailID
            };
            IProcedureAsync proc = new ProcGetProductDetailByID(_dal);
            var response = (AddProductModal)await proc.Call(param).ConfigureAwait(false);
            return response;
        }
        public ResponseStatus DeleteProductDetail(int ProductDetailID, bool IsDeleted)
        {
            var req = new CommonReq
            {
                LoginID = _lr.UserID,
                CommonInt = ProductDetailID,
                CommonBool = IsDeleted
            };
            IProcedure proc = new ProcDeleteProductDetail(_dal);
            var response = (ResponseStatus)proc.Call(req);
            return response;
        }
        public ResponseStatus StockUpdation(int ProductDetailID, int Quantity, string Remark)
        {
            var req = new CommonReq
            {
                LoginID = _lr.UserID,
                CommonInt = ProductDetailID,
                CommonInt2 = Quantity,
                CommonStr = Remark
            };
            IProcedure proc = new ProcStockUpdation(_dal);
            var response = (ResponseStatus)proc.Call(req);
            return response;
        }
        #endregion

        #region Commission

        public IEnumerable<MasterRole> ShoppingCommissionRoles()
        {
            var req = new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID
            };
            IProcedure proc = new ProcgetShoppingCommRole(_dal);
            var response = (List<MasterRole>)proc.Call(req);
            return response;
        }
        public ShoppingCommissionExtend GetShopppingSlabCommission(ShoppingCommissionReq req)
        {
            req.LT = _lr.LoginTypeID;
            req.LoginID = _lr.UserID;
            IProcedure proc = new ProcGetShopppingSlabComm(_dal);
            var response = (ShoppingCommissionExtend)proc.Call(req);
            return response;
        }

        public ResponseStatus UpdateShoppingComm(ShoppingCommissionReq req)
        {
            req.LoginID = _lr.UserID;
            req.LT = _lr.LoginTypeID;
            IProcedure proc = new ProcUpdateShoppingComm(_dal);
            ResponseStatus response = (ResponseStatus)proc.Call(req);
            return response;
        }

        public ResponseStatus GetUserCommission(int productDeatilId, int userId = 0)
        {
            CommonReq req = new CommonReq
            {
                LoginID = userId != 0 ? userId : _lr.UserID,
                LoginTypeID = userId == 0 ? 1 : _lr.LoginTypeID,
                CommonInt = productDeatilId,
                CommonInt2 = ApplicationSetting.ECommerceDistributionCommissionType
            };
            IProcedure proc = new ProcShoppingComm(_dal);
            ResponseStatus response = (ResponseStatus)proc.Call(req);
            return response;
        }
        #endregion

        #region Cart
        public async Task<ResponseStatus> AddToCart(int ProductDetailID, int Quantity, int UserID = 0)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            if (Quantity == null || Quantity <= 0)
            {
                res.Msg = "Please Choose Quantity";
                return res;
            }
            //if (ProductID == null || ProductID <= 0)
            //{
            //    res.Msg = "Invalid ProductID";
            //}
            if (ProductDetailID == null || ProductDetailID <= 0)
            {
                res.Msg = "Invalid ProductID";
                return res;
            }
            var req = new AddToCartRequest
            {
                LoginID = _lr != null ? _lr.UserID : UserID,
                Quantity = Quantity,
                ProductDeatilID = ProductDetailID
            };
            if (req.LoginID <= 0)
            {
                var eCommUser = ReadLocalSession();
                if (eCommUser == null)
                {
                    ECommUserDetail eCommUserDetail = new ECommUserDetail();
                    eCommUserDetail.CartDetail = new List<AddToCartRequest>();
                    eCommUserDetail.Wishlist = new List<int>();
                    eCommUserDetail.CartDetail.Add(req);
                    WriteLocalSession(eCommUserDetail, true);
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = "Product added to cart";
                }
                else
                {
                    bool SameProduct = false;
                    foreach (var item in eCommUser.CartDetail)
                    {
                        if (item.ProductDeatilID == ProductDetailID)
                        {
                            SameProduct = true;
                            break;
                        }
                    }
                    if (!SameProduct)
                    {
                        eCommUser.CartDetail.Add(req);
                        WriteLocalSession(eCommUser, true);
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
                IProcedureAsync proc = new ProcAddToCart(_dal);
                res = (ResponseStatus)await proc.Call(req);
            }
            return res;
        }
        public async Task<IEnumerable<CartDetail>> CartDetail(int UserID = 0)
        {
            var res = new List<CartDetail>();
            if (_lr != null || UserID > 0)
            {
                IProcedureAsync proc = new ProcCartDetail(_dal);
                res = (List<CartDetail>)await proc.Call(_lr != null ? _lr.UserID : UserID).ConfigureAwait(false);
            }
            else
            {
                var eCommUser = ReadLocalSession();
                if (eCommUser == null || eCommUser.CartDetail == null)
                {
                    return res;
                }
                else
                {
                    string cartDet = string.Empty;
                    StringBuilder sb = new StringBuilder(cartDet);
                    var counter = 0;
                    foreach (var item in eCommUser.CartDetail)
                    {
                        if (counter != 0)
                        {
                            sb.Append(",");
                        }
                        sb.Append(item.ProductDeatilID.ToString());
                        sb.Append("_");
                        sb.Append(item.Quantity.ToString());
                        counter++;
                    }
                    cartDet = sb.ToString();
                    var req = new CommonReq
                    {
                        CommonStr = cartDet
                    };
                    IProcedureAsync proc = new ProcCartDetailExternal(_dal);
                    res = (List<CartDetail>)await proc.Call(req).ConfigureAwait(false);
                }
            }
            return res;
        }
        public ItemInCart ItemInCart(int UserID = 0)
        {
            var res = new ItemInCart
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.FAILED,
                TQuantity = 0,
                TCost = 0
            };
            if (_lr != null || UserID > 0)
            {
                IProcedure proc = new ProcItemInCart(_dal);
                res = (ItemInCart)proc.Call(_lr == null ? UserID : _lr.UserID);
            }
            else
            {
                var eCommUser = ReadLocalSession();
                if (eCommUser == null || eCommUser.CartDetail == null)
                {
                    return res;
                }
                else
                {
                    res.TQuantity = eCommUser.CartDetail.Count;
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = ErrorCodes.SUCCESS;
                }
            }
            return res;
        }
        public async Task<ResponseStatus> RemoveItemFromCart(int ID, int UserID = 0, int ProductDetailId = 0, bool RemoveAll = false)
        {
            var req = new CommonReq
            {
                LoginID = _lr != null ? _lr.UserID : UserID,
                CommonInt = ID,
                CommonInt2 = ProductDetailId,
                CommonBool = RemoveAll
            };
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            if (req.LoginID <= 0)
            {
                var eCommUser = ReadLocalSession();
                if (eCommUser == null || eCommUser.CartDetail == null)
                {
                    res.Statuscode = ErrorCodes.Minus1;
                    res.Msg = "Cart is empty";
                }
                else
                {
                    if (RemoveAll)
                    {
                        eCommUser.CartDetail = new List<AddToCartRequest>();
                        WriteLocalSession(eCommUser, true);
                        res.Statuscode = ErrorCodes.One;
                        res.Msg = "Cart cleared";
                    }
                    else
                    {
                        bool IsRemoved = false;
                        foreach (var item in eCommUser.CartDetail)
                        {
                            if (item.ProductDeatilID == ProductDetailId)
                            {
                                eCommUser.CartDetail.Remove(item);
                                WriteLocalSession(eCommUser, true);
                                IsRemoved = true;
                                res.Statuscode = ErrorCodes.One;
                                res.Msg = "Product removed from Cart";
                                break;
                            }
                        }
                        if (!IsRemoved)
                        {
                            res.Statuscode = ErrorCodes.Minus1;
                            res.Msg = "Product not found";
                        }
                    }
                }
            }
            else
            {
                IProcedureAsync proc = new ProcRemoveItemFromCart(_dal);
                res = (ResponseStatus)await proc.Call(req);
            }
            return res;
        }
        public ProceedToPay ProceedToPay(int UserID = 0)
        {
            var res = new ProceedToPay
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Please login first"
            };
            if (_lr != null || UserID > 0)
            {
                IProcedure proc = new ProcProceedToPay(_dal);
                res = (ProceedToPay)proc.Call(UserID == 0 ? _lr.UserID : UserID);
            }
            return res;
        }
        public async Task<ResponseStatus> ChangeQuantity(int ItemID, int Quantity, int UserID = 0)
        {
            var req = new CommonReq
            {
                LoginID = _lr != null ? _lr.UserID : UserID,
                CommonInt = ItemID,
                CommonInt2 = Quantity
            };
            IProcedureAsync proc = new ProcChangeQuantity(_dal);
            var res = (ResponseStatus)await proc.Call(req);
            return res;
        }

        public async Task<ResponseStatus> ChangeQuantityByPdId(int PdId, int Quantity, int UserID = 0)
        {
            var req = new CommonReq
            {
                LoginID = _lr != null ? _lr.UserID : UserID,
                CommonInt = PdId,
                CommonInt2 = Quantity
            };
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.FAILED
            };
            if (req.LoginID > 0)
            {
                IProcedureAsync proc = new ProcChangeQuantityByPdId(_dal);
                res = (ResponseStatus)await proc.Call(req);
            }
            else
            {
                var eCommUser = ReadLocalSession();
                if (eCommUser != null && eCommUser.CartDetail != null)
                {
                    var selectedItem = eCommUser.CartDetail.Where(item => item.ProductDeatilID == PdId).ToList();
                    selectedItem.ForEach(i => i.Quantity = Quantity);
                    WriteLocalSession(eCommUser, true);
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = "Cart updated successfully";
                }
            }
            return res;
        }

        public async Task<ResponseStatus> PlaceOrder(PlaceOrder order)
        {
            order.UserID = _lr != null ? _lr.UserID : order.UserID;
            order.RequestMode = _lr != null ? 1 : order.RequestMode;
            IProcedureAsync proc = new ProcPlaceOrder(_dal);
            var response = (ResponseStatus)await proc.Call(order);
            return response;
        }
        public ResponseStatus AddToWishList(int ProductDetailID)
        {
            var req = new CommonReq
            {
                LoginID = _lr != null ? _lr.UserID : 0,
                CommonInt = ProductDetailID
            };

            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.FAILED
            };

            if (req.LoginID <= 0)
            {
                var eCommUser = ReadLocalSession();
                if (eCommUser == null)
                {
                    ECommUserDetail eCommUserDetail = new ECommUserDetail();
                    eCommUserDetail.CartDetail = new List<AddToCartRequest>();
                    eCommUserDetail.Wishlist = new List<int>();
                    eCommUserDetail.Wishlist.Add(ProductDetailID);
                    WriteLocalSession(eCommUserDetail, true);
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = "Product added to cart";
                }
                else
                {
                    if (eCommUser.Wishlist == null)
                    {
                        eCommUser.Wishlist = new List<int>();
                    }
                    bool SameProduct = false;
                    foreach (var item in eCommUser.Wishlist)
                    {
                        if (item == ProductDetailID)
                        {
                            SameProduct = true;
                            break;
                        }
                    }
                    if (!SameProduct)
                    {
                        eCommUser.Wishlist.Add(ProductDetailID);
                        WriteLocalSession(eCommUser, true);
                        res.Statuscode = ErrorCodes.One;
                        res.Msg = "Product added to wishlist";
                    }
                    else
                    {
                        res.Statuscode = ErrorCodes.Minus1;
                        res.Msg = "Product already in wishlist";
                    }
                }
            }
            else
            {
                IProcedure proc = new ProcAddToWishList(_dal);
                res = (ResponseStatus)proc.Call(req);
            }
            return res;
        }

        public ResponseStatus RemoveFromWishList(int ProductDetailID, int id = 0, bool RemoveAll = false)
        {
            var req = new CommonReq
            {
                LoginID = _lr != null ? _lr.UserID : 0,
                CommonInt = id,
                CommonInt2 = ProductDetailID,
                CommonBool = RemoveAll
            };

            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.FAILED
            };

            if (req.LoginID <= 0)
            {
                var eCommUser = ReadLocalSession();
                if (eCommUser == null || eCommUser.Wishlist == null)
                {
                    res.Statuscode = ErrorCodes.Minus1;
                    res.Msg = "Wishlist is empty";
                }
                else
                {
                    if (RemoveAll)
                    {
                        eCommUser.Wishlist = new List<int>();
                        WriteLocalSession(eCommUser, true);
                        res.Statuscode = ErrorCodes.One;
                        res.Msg = "Wishlist cleared successfully";
                    }
                    else
                    {
                        bool itemRemoved = false;
                        foreach (var item in eCommUser.Wishlist)
                        {
                            if (item == ProductDetailID)
                            {
                                eCommUser.Wishlist.Remove(item);
                                WriteLocalSession(eCommUser, true);
                                itemRemoved = true;
                                res.Statuscode = ErrorCodes.One;
                                res.Msg = "Item removed from Wishlist";
                                break;
                            }
                        }
                        if (!itemRemoved)
                        {
                            res.Statuscode = ErrorCodes.Minus1;
                            res.Msg = "Item not found";
                        }
                    }
                }
            }
            else
            {
                IProcedure proc = new ProcRemoveItemFromWishlist(_dal);
                res = (ResponseStatus)proc.Call(req);
            }
            return res;
        }

        public ItemInCart WishListCount(int UserID = 0)
        {
            var res = new ItemInCart
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.FAILED,
                TQuantity = 0
            };
            if (_lr != null || UserID > 0)
            {
                IProcedure proc = new ProcItemInWishlist(_dal);
                res = (ItemInCart)proc.Call(_lr == null ? UserID : _lr.UserID);
            }
            else
            {
                var eCommUser = ReadLocalSession();
                if (eCommUser == null || eCommUser.Wishlist == null)
                {
                    return res;
                }
                else
                {
                    res.TQuantity = eCommUser.Wishlist.Count;
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = ErrorCodes.SUCCESS;
                }
            }
            return res;
        }
        public async Task<IEnumerable<CartDetail>> WishlistDetail(int UserID = 0)
        {
            var res = new List<CartDetail>();
            if (_lr != null || UserID > 0)
            {
                IProcedureAsync proc = new ProcWishlistDetail(_dal);
                res = (List<CartDetail>)await proc.Call(_lr != null ? _lr.UserID : UserID).ConfigureAwait(false);
            }
            else
            {
                var eCommUser = ReadLocalSession();
                if (eCommUser == null || eCommUser.Wishlist == null)
                {
                    return res;
                }
                else
                {
                    string cartDet = string.Empty;
                    StringBuilder sb = new StringBuilder(cartDet);
                    var counter = 0;
                    foreach (var item in eCommUser.Wishlist)
                    {
                        if (counter != 0)
                        {
                            sb.Append(",");
                        }
                        sb.Append(item.ToString());
                        counter++;
                    }
                    cartDet = sb.ToString();
                    var req = new CommonReq
                    {
                        CommonStr = cartDet
                    };
                    IProcedureAsync proc = new ProcWishlistDetailExternal(_dal);
                    res = (List<CartDetail>)await proc.Call(req).ConfigureAwait(false);
                }
            }
            return res;
        }
        #endregion

        #region OrderDetail
        public IEnumerable<OrderList> GetOrderHistory(OrderModel req)
        {
            req.LoginID = _lr != null ? _lr.UserID : req.LoginID;
            IProcedure proc = new procGetOrderHistory(_dal);
            var response = (IEnumerable<OrderList>)proc.Call(req);
            return response;
        }
        public IEnumerable<OrderDetailList> getOrderDetails(int OrderID, int UserID = 0)
        {
            var req = new OrderModel
            {
                LoginID = _lr != null ? _lr.UserID : UserID,
                OrderId = OrderID
            };
            IProcedure proc = new getOrderDetailDL(_dal);
            var response = (IEnumerable<OrderDetailList>)proc.Call(req);
            return response;
        }
        public IEnumerable<OrderReport> getOrderReport(OrderModel req)
        {
            if (req.Criteria == Criteria.OutletMobile)
                req.MobileNo = req.CriteriaText;

            if (req.Criteria == Criteria.Name)
                req.CCName = req.CriteriaText;

            if (req.Criteria == Criteria.CMobileNo)
                req.CCMobile = req.CriteriaText;

            if (req.Criteria == Criteria.UserID)
            {
                var Prefix = Validate.O.Prefix(req.CriteriaText);
                if (Validate.O.IsNumeric(Prefix))
                    req.UserID = Validate.O.IsNumeric(req.CriteriaText) ? Convert.ToInt32(req.CriteriaText) : req.UserID;
                var uid = Validate.O.LoginID(req.CriteriaText);
                req.UserID = Validate.O.IsNumeric(uid) ? Convert.ToInt32(uid) : req.UserID;
            }

            IProcedure proc = new ProcOrderReport(_dal);
            req.LoginID = _lr.UserID;
            var response = (IEnumerable<OrderReport>)proc.Call(req);
            return response;
        }
        public ResponseStatus ChangeOrderStatus(CommonReq req)
        {
            if (_lr != null)
            {
                req.LoginID = _lr.UserID;
                req.LoginTypeID = _lr.LoginTypeID;
            }
            IProcedure proc = new ProcChangeOrderStatus(_dal);
            var response = (ResponseStatus)proc.Call(req);
            return response;
        }
        public IResponseStatus ChangePartialOrderStatus(ChangeOrderStatus req)
        {
            if (_lr != null)
            {
                req.LoginID = _lr.UserID;
                req.LT = _lr.LoginTypeID;
            }
            IProcedure proc = new ProcChangePartialOrderStatus(_dal);
            var response = (ResponseStatus)proc.Call(req);
            return response;
        }
        public IEnumerable<OrderDetailList> GetOrderDetailList(CommonReq req)
        {
            IProcedure proc = new ProcGetOrderDetailList(_dal);
            var res = (List<OrderDetailList>)proc.Call(req);
            return res;
        }
        public IEnumerable<AppOrderModel> AppOrderList(CommonReq req)
        {
            IProcedure proc = new ProcAppOrderList(_dal);
            var res = (List<AppOrderModel>)proc.Call(req);
            return res;
        }
        #endregion

        #region UserShopping
        public IEnumerable<CategoriesForIndex> GetCategoriesForUserIndex(ProductFilter p)
        {
            p.UserId = _lr != null ? _lr.UserID : p.UserId;
            p.LoginTypeId = _lr != null ? _lr.LoginTypeID : p.LoginTypeId;
            IProcedure proc = new ProcGetCategoryForUserIndex(_dal);
            var response = (IEnumerable<CategoriesForIndex>)proc.Call(p);
            return response;
        }
        #endregion

        //public async Task<OnChangeFilter> OnChangeFilterForApp(int ProductID, int ProductDeatilID, List<string> Filters)
        //{
        //    var res = new OnChangeFilter
        //    {
        //        Statuscode = ErrorCodes.Minus1,
        //        Msg = ErrorCodes.TempError
        //    };
        //    var req = new AddToCartRequest
        //    {
        //        LoginID = _lr.UserID,
        //        ProductDeatilID = ProductDeatilID,
        //        ProductID = ProductID,
        //        FilterIds = string.Join(",", Filters)
        //    };
        //    IProcedureAsync proc = new ProcOnChangeFilter(_dal);
        //    var result = (ResponseStatus)await proc.Call(req);
        //    res.Statuscode = result.Statuscode;
        //    res.Msg = result.Msg;
        //    res.ProductDetailID = result.CommonInt;
        //    res.Quantity = result.CommonInt2;
        //    if (res.ProductDetailID > 0)
        //    {
        //        List<string> files = new List<string>();
        //        try
        //        {
        //            string path = DOCType.ProductImagePath.Replace("{0}", ProductID.ToString());
        //            if (Directory.Exists(path))
        //            {
        //                DirectoryInfo d = new DirectoryInfo(path);
        //                FileInfo[] Files = d.GetFiles(res.ProductDetailID + "_*-1x.png");
        //                foreach (FileInfo file in Files)
        //                {
        //                    files.Add(ProductID.ToString() + "/" + file.Name);
        //                }
        //                res.Files = files;
        //            }
        //        }
        //        catch (Exception ex)
        //        {

        //        }
        //    }
        //    return res;
        //}
    }
}
