
using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Fintech.AppCode.WebRequest;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.ThirdParty.RBL;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace RoundpayFinTech.AppCode.ThirdParty.HotelAPI
{
    public class HotelApiML : ITrkTravalHotelMLAPI
    {
        //Live URLS
        private const string AuthunticateReq = "https://api.travelboutiqueonline.com/SharedAPI/SharedData.svc/rest/Authenticate";
        private const string GetDestinationCityList = "https://api.travelboutiqueonline.com/SharedAPI/StaticData.svc/rest/GetDestinationSearchStaticData";
        private const string TopDestinationCityList = "https://api.travelboutiqueonline.com/SharedAPI/SharedData.svc/rest/TopDestinationList";
        private const string GetHotelSearchList = "https://api.travelboutiqueonline.com/HotelAPI_V10/HotelService.svc/rest/GetHotelResult/";
        private const string GetHotelMasterList = "https://api.travelboutiqueonline.com/SharedAPI/StaticData.svc/rest/GetHotelStaticData";
        private const string GetHotelSingleInfo = "https://api.travelboutiqueonline.com/HotelAPI_V10/HotelService.svc/rest/GetHotelInfo";
        private const string GetHotelSingleRoom = "https://api.travelboutiqueonline.com/HotelAPI_V10/HotelService.svc/rest/GetHotelRoom";
        private const string HotelBookReq = "https://booking.travelboutiqueonline.com/HotelAPI_V10/HotelService.svc/rest/Book";
        private const string HotelBlockRoomReq = "https://api.travelboutiqueonline.com/HotelAPI_V10/HotelService.svc/rest/BlockRoom";
        private const string HotelBookedRoom = "https://booking.travelboutiqueonline.com/HotelAPI_V10/HotelService.svc/GetBookingDetail";
        private const string HotelBookingCancel = "http://api.tektravels.com/BookingEngineService_Hotel/hotelservice.svc/rest/SendChangeRequest";
        //local
        //private const string AuthunticateReq = "http://api.tektravels.com/SharedServices/SharedData.svc/rest/Authenticate";
        //private const string GetDestinationCityList = " http://api.tektravels.com/SharedServices/StaticData.svc/rest/GetDestinationSearchStaticData";
        //private const string TopDestinationCityList = "http://api.tektravels.com/SharedServices/SharedData.svc/rest/TopDestinationList";
        //private const string GetHotelSearchList = "http://api.tektravels.com/BookingEngineService_Hotel/hotelservice.svc/rest/GetHotelResult/";
        //private const string GetHotelMasterList = "http://api.tektravels.com/SharedServices/StaticData.svc/rest/GetHotelStaticData";
        //private const string GetHotelSingleInfo = "http://api.tektravels.com/BookingEngineService_Hotel/hotelservice.svc/rest/GetHotelInfo";
        //private const string GetHotelSingleRoom = "http://api.tektravels.com/BookingEngineService_Hotel/hotelservice.svc/rest/GetHotelRoom";
        //private const string HotelBookReq = "http://api.tektravels.com/BookingEngineService_Hotel/hotelservice.svc/rest/Book";
        //private const string HotelBlockRoomReq = "http://api.tektravels.com/BookingEngineService_Hotel/hotelservice.svc/rest/BlockRoom";
        //private const string HotelBookedRoom = "http://api.tektravels.com/BookingEngineService_Hotel/HotelService.svc/rest/GetBookingDetail";
        //private const string HotelBookingCancel = "http://api.tektravels.com/BookingEngineService_Hotel/hotelservice.svc/rest/SendChangeRequest";
        //For Testing Purpose
        //-----------------------------------------------------------------
        public const string TClientId = "ApiIntegrationNew";
        public const string TEndUserIp = "192.168.11.120";
        //-----------------------------------------------------------------

        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration Configuration;
        private readonly IConnectionConfiguration _c;
        private readonly TekTvlAppsetting appSetting;
        private readonly IDAL _dal;
        private static string TrkTvlSession;
        private static DateTime TrkTvlExpiry;
        private readonly IRequestInfo _info;
        private readonly LoginResponse _lr;
        private readonly ISession _session;
        //AppWebRequest _req = new AppWebRequest();
        public HotelApiML(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile((_env.IsProduction() ? "appsettings.json" : "appsettings.Development.json"));
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            _info = new RequestInfo(_accessor, _env);
            _session = _accessor.HttpContext.Session;
            _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
            appSetting = AppSetting(_info.GetRemoteIP());
        }
        private TekTvlAppsetting AppSetting(string ip)
        {
            var _setting = new TekTvlAppsetting();
            try
            {
                _setting = new TekTvlAppsetting
                {
                    ClientId = Configuration["SERVICESETTING:TEKTVL:HOTEL:ClientId"],
                    UserName = Configuration["SERVICESETTING:TEKTVL:HOTEL:UserName"],
                    Password = Configuration["SERVICESETTING:TEKTVL:HOTEL:Password"],
                    EndUserIp = ip

                };
            }
            catch (Exception)
            {

                throw;
            }
            return _setting;
        }

        private async Task<HotelModelResponce> HotelApiTokenAuth(string EndUserIP)
        {
            string _responce = await AppWebRequest.O.PostJsonDataUsingHWRTLS(AuthunticateReq, appSetting);
            HotelModelResponce model = new HotelModelResponce();

            // Static Hard Coded For Trsting Purpose Start
            //TrkTvlSession = "24e4438f-a730-4cde-a7ec-cc466817b325";
            //TrkTvlExpiry = Convert.ToDateTime("15-12-2021 10:35:30");
            //model.TokenId = "24e4438f-a730-4cde-a7ec-cc466817b325";
            //if (!string.IsNullOrEmpty(TrkTvlSession))
            //{
            //    return model;
            //}
            // Static Hard Coded For Trsting Purpose End
            model.Error = new TekTvlErrorModel { ErrorMessage = "Server not responding.", ErrorCode = "-1" };
            if (!string.IsNullOrEmpty(_responce))
            {
                HotelApiLog(AuthunticateReq, (JsonConvert.SerializeObject(appSetting, Newtonsoft.Json.Formatting.Indented)), _responce, "HotelApiML", "HotelBlockRoomApiReq", EndUserIP);
                var HotelResp = JsonConvert.DeserializeObject<HotelModelResponce>(_responce);
                if (HotelResp.Status == "1")
                {
                    TrkTvlSession = HotelResp.TokenId;
                    TrkTvlExpiry = string.IsNullOrEmpty(TrkTvlSession) ? DateTime.Now.AddMinutes(-10) : DateTime.Now.AddHours(24);
                    model.TokenId = HotelResp.TokenId;
                }
                else
                {
                    model.Error = HotelResp.Error;
                }
            }
            else
            {
                model.Error.ErrorMessage = "Server not responding.";
                model.Error.ErrorCode = "-1";
            }

            return model;
        }
        public async Task<TekTvlErrorModel> SyncHotelMasterApiDAta(string ip, string cityId)
        {
            HotelApiML _HotelApiML = new HotelApiML(_accessor, _env);
            HotelModelResponce model = new HotelModelResponce();
            bool IsCallAPI = string.IsNullOrEmpty(TrkTvlSession) ? true : TrkTvlExpiry == null ? true : TrkTvlExpiry < DateTime.Now ? true : false;
            if (IsCallAPI)
            {
                model = await HotelApiTokenAuth(ip);
            }

            if (!string.IsNullOrEmpty(TrkTvlSession) && TrkTvlExpiry > DateTime.Now)
            {
                TekTvlHotelMasterApiRequest _hotelMasterSetting = new TekTvlHotelMasterApiRequest();
                _hotelMasterSetting.TokenId = TrkTvlSession;
                _hotelMasterSetting.EndUserIp = ip;
                _hotelMasterSetting.CityId = cityId;
                _hotelMasterSetting.ClientId = TClientId;
                _hotelMasterSetting.IsCompactData = true;
                string _resHotelList = await AppWebRequest.O.PostJsonDataUsingHWRTLS(GetHotelMasterList, _hotelMasterSetting);
                HotelApiLog(GetHotelMasterList, (JsonConvert.SerializeObject(_hotelMasterSetting, Newtonsoft.Json.Formatting.Indented)),
               _resHotelList, "HotelApiML", "SyncHotelDestinationApiDAta", ip);
                var HotelResp = JsonConvert.DeserializeObject<TekTvlHotelMasterApiResponse>(_resHotelList);
                if (HotelResp.Status == "1")
                {
                    var res = DeserializeTvkHotelXmlObject(HotelResp.HotelData);
                    var xmlTBOBasicPropertyInfo = "";
                    if (res != null && res.ArrayOfBasicPropertyInfo != null && res.ArrayOfBasicPropertyInfo.BasicPropertyInfo != null)
                    {
                        xmlTBOBasicPropertyInfo =
                                  new XDocument(
                                    new XElement("root",
                                      res.ArrayOfBasicPropertyInfo.BasicPropertyInfo.Select(x =>
                                            new XElement("hotel",
                                                new XElement("_BrandCode", x.BrandCode),
                                                new XElement("_TBOHotelCode", x.TBOHotelCode),
                                                new XElement("_HotelName", x.HotelName),
                                                new XElement("_LocationCategoryCode", x.LocationCategoryCode),
                                                new XElement("_IsHalal", x.IsHalal),
                                                new XElement("_HotelCategoryId", x.HotelCategoryId),
                                                new XElement("_HotelCategoryName", x.HotelCategoryName),
                                                new XElement("_Latitude", x.Position != null ? x.Position.Latitude : ""),
                                                new XElement("_Longitude", x.Position != null ? x.Position.Longitude : ""),
                                                new XElement("_Provider", x.Award != null ? x.Award.Provider : ""),
                                                new XElement("_Rating", x.Award != null ? x.Award.Rating : ""),
                                                new XElement("_ReviewURL", x.Award != null ? x.Award.ReviewURL : ""),
                                                new XElement("_CheckInTime", x.Policy != null ? x.Policy.CheckInTime : ""),
                                                new XElement("_CheckOutTime", x.Policy != null ? x.Policy.CheckOutTime : ""),
                                                new XElement("_Title", x.VendorMessages.VendorMessage != null ? x.VendorMessages.VendorMessage.Title : ""),
                                                new XElement("_InfoType", x.VendorMessages.VendorMessage != null ? x.VendorMessages.VendorMessage.InfoType : ""),
                                                new XElement("_SubTitle", x.VendorMessages.VendorMessage != null ? x.VendorMessages.VendorMessage.SubSection.SubTitle : ""),
                                                new XElement("_TextFormat", x.VendorMessages.VendorMessage != null ? x.VendorMessages.VendorMessage.SubSection.Paragraph.Text.TextFormat : ""),
                                                new XElement("_HotelText", x.VendorMessages.VendorMessage != null ? x.VendorMessages.VendorMessage.SubSection.Paragraph.Text.Text : ""),
                                                new XElement("_AddressLine1", x.Address.AddressLine != null && x.Address.AddressLine.Count() > 0 ? x.Address.AddressLine.ElementAt(0) : ""),
                                                new XElement("_AddressLine2", x.Address.AddressLine != null && x.Address.AddressLine.Count() > 1 ? x.Address.AddressLine.ElementAt(1) : ""),
                                                new XElement("_CityName", x.Address != null ? x.Address.CityName : ""),
                                                new XElement("_PostalCode", x.Address != null ? x.Address.PostalCode : ""),
                                                new XElement("_StateProv", x.Address != null ? x.Address.StateProv : ""),
                                                new XElement("_CountryCode", x.Address != null ? x.Address.CountryName.Code : ""),
                                                new XElement("_CountryName", x.Address != null ? x.Address.CountryName.Text : "")
                                            )
                                        )
                                    )
                                  ).ToString();

                        //Initialize TBOHotelCode to each property of Attribute class
                        res.ArrayOfBasicPropertyInfo.BasicPropertyInfo.ForEach(x => { if (x.Attributes.Attribute != null) { x.Attributes.Attribute.ForEach(y => y.TBOHotelCode = x.TBOHotelCode); } });
                       
                    }
                    #region Attribute
                    List<TBOAttribute> lstAttr = new List<TBOAttribute>();
                    if (res != null && res.ArrayOfBasicPropertyInfo != null && res.ArrayOfBasicPropertyInfo.BasicPropertyInfo != null)
                    {
                        foreach (var item in res.ArrayOfBasicPropertyInfo.BasicPropertyInfo)
                        {
                            if (item.Attributes.Attribute != null) lstAttr.AddRange(item.Attributes.Attribute);
                        }
                    }

                    string xmlTBOAttribute = "";
                    if (lstAttr != null && lstAttr.Count() > 0)
                    {
                        xmlTBOAttribute = new XDocument(new XElement("root",
                                                   lstAttr.Select(x => new XElement("attribute",
                                                                           new XElement("_TBOHotelCode", x.TBOHotelCode),
                                                                           new XElement("_AttributeName", x.AttributeName),
                                                                           new XElement("_AttributeType", x.AttributeType)
                                                                       ))
                                               )).ToString();
                    }

                    #endregion
               
                    var req = new TvkHotelSyncProcRequest
                    {
                        cityId = cityId,
                        xmlTBOBasicPropertyInfo = xmlTBOBasicPropertyInfo,
                        xmlTBOAttribute = xmlTBOAttribute
                    };

                    IProcedure _proc = new ProcSyncApiHotelData(_dal);
                    model.Error = (TekTvlErrorModel)_proc.Call(req);
                }
            }
            return model.Error;
        }


        public async Task<TekTvlErrorModel> SyncHotelMasterApiDAtaByHotelCode(string ip, string cityId, string Hotelid)
        {
            HotelApiML _HotelApiML = new HotelApiML(_accessor, _env);
            HotelModelResponce model = new HotelModelResponce();
            bool IsCallAPI = string.IsNullOrEmpty(TrkTvlSession) ? true : TrkTvlExpiry == null ? true : TrkTvlExpiry < DateTime.Now ? true : false;
            if (IsCallAPI)
            {
                model = await HotelApiTokenAuth(ip);
            }
            if (!string.IsNullOrEmpty(TrkTvlSession) && TrkTvlExpiry > DateTime.Now)
            {
                TekTvlHotelMasterApiRequest _hotelMasterSetting = new TekTvlHotelMasterApiRequest();
                _hotelMasterSetting.TokenId = TrkTvlSession;
                _hotelMasterSetting.EndUserIp = ip;
                _hotelMasterSetting.CityId = cityId;
                _hotelMasterSetting.ClientId = TClientId;
                 _hotelMasterSetting.HotelId = Hotelid;
                string _resHotelList = await AppWebRequest.O.PostJsonDataUsingHWRTLS(GetHotelMasterList, _hotelMasterSetting);
                HotelApiLog(GetHotelMasterList, (JsonConvert.SerializeObject(_hotelMasterSetting, Newtonsoft.Json.Formatting.Indented)),
               _resHotelList, "HotelApiML", "SyncHotelDestinationApiDAta", ip);
                var HotelResp = JsonConvert.DeserializeObject<TekTvlHotelMasterApiResponse>(_resHotelList);
                if (HotelResp.Status == "1")
                {
                    var res = DeserializeTvkHotelXmlObjectByNode(HotelResp.HotelData);
                    if (res != null)
                    {
                        List<HotelImages> lstimg = res.HotelImages;
                        string xmlTBOImage = "";
                        if (lstimg != null && lstimg.Count() > 0)
                        {
                            xmlTBOImage = new XDocument(new XElement("root",
                                                       lstimg.Select(x => new XElement("images",
                                                                         new XElement("_ImageType", x.Type),
                                                                               new XElement("_TBOHotelCode", x.TBOHotelCode),
                                                                               new XElement("_CityId", x.CityId),
                                                                               new XElement("_ImageURL", x.ImageURL)
                                                                           ))
                                                   )).ToString();
                        }

                        List<HotelFacilities> lstfaci = res.HotelFacility;
                        string xmlTBOfacility = "";
                        if (lstfaci != null && lstfaci.Count() > 0)
                        {
                            xmlTBOfacility = new XDocument(new XElement("root",
                                                       lstfaci.Select(x => new XElement("facility",
                                                                               new XElement("_TBOHotelCode", x.TBOHotelCode),
                                                                               new XElement("_CityId", x.CityId),
                                                                               new XElement("_Title", x.Title),
                                                                               new XElement("_Text", x.Fcility)
                                                                           ))
                                                   )).ToString();
                        }
                        var req = new TvkHotelSyncProcRequestByHotelCode
                        {
                            cityId = cityId,
                            HotelID = Hotelid,
                            xmlTBOImage = xmlTBOImage,
                            xmlTBOFacility= xmlTBOfacility
                        };
                        IProcedure _proc = new ProcSyncApiHotelDataByHotelCode(_dal);
                        model.Error = (TekTvlErrorModel)_proc.Call(req);
                    }
                }
            }
            return model.Error;
        }
        public async Task<List<TekTvlDestinations>> SyncHotelDestinationApiDAta(string ip)
        {
            List<TekTvlDestinations> lstcitycode = new List<TekTvlDestinations>();
            HotelApiML _HotelApiML = new HotelApiML(_accessor, _env);
            HotelModelResponce Auth = new HotelModelResponce();
            bool IsCallAPI = string.IsNullOrEmpty(TrkTvlSession) ? true : TrkTvlExpiry == null ? true : TrkTvlExpiry < DateTime.Now ? true : false;
            if (IsCallAPI)
            {

                Auth = await HotelApiTokenAuth(ip);
            }
            if (!string.IsNullOrEmpty(TrkTvlSession) && TrkTvlExpiry > DateTime.Now)
            {
                var _cityListSetting = new HotelModelResponce()
                {
                    TokenId = TrkTvlSession,
                    EndUserIp = ip,
                    SearchType = "1",
                    CountryCode = "IN"
                };
                var _TopCity = new TekTvlTopCities
                { };
                string _resCityList = await AppWebRequest.O.PostJsonDataUsingHWRTLS(GetDestinationCityList, _cityListSetting).ConfigureAwait(false);
                //HotelApiLog(GetDestinationCityList, (JsonConvert.SerializeObject(_cityListSetting, Newtonsoft.Json.Formatting.Indented)),
                //    _resCityList, "HotelApiML", "SyncHotelDestinationApiDAta", ip);
                var CityResp = JsonConvert.DeserializeObject<DestinationSearchStaticDataResult>(_resCityList);
                if (CityResp.Status == "1")
                {
                    IProcedure _proc = new ProcSyncApiData(_dal);
                    lstcitycode = (List<TekTvlDestinations>)_proc.Call(CityResp);
                }
            }
            return lstcitycode;
        }


        public async Task<TekTvlSearchingResponse> Hotelsearch(TekTvlHotelSearchRequest req)
        {
            HotelApiML _HotelApiML = new HotelApiML(_accessor, _env);
            TekTvlSearchingResponse resp = new TekTvlSearchingResponse();
            bool IsCallAPI = string.IsNullOrEmpty(TrkTvlSession) ? true : TrkTvlExpiry == null ? true : TrkTvlExpiry < DateTime.Now ? true : false;
            if (IsCallAPI)
            {
                HotelModelResponce Auth = new HotelModelResponce();
                Auth = await HotelApiTokenAuth(req.EndUserIp);
            }
            if (!string.IsNullOrEmpty(TrkTvlSession) && TrkTvlExpiry > DateTime.Now)
            {
                // string tst = @"{""HotelSearchResult"":{""ResponseStatus"":1,""Error"":{""ErrorCode"":0,""ErrorMessage"":""""},""TraceId"":""c211e04c-3141-4e3d-956b-4cac18ca6000"",""CityId"":""130443"",""Remarks"":"""",""CheckInDate"":""2021-06-23"",""CheckOutDate"":""2021-06-24"",""PreferredCurrency"":""INR"",""NoOfRooms"":2,""RoomGuests"":[{ ""NoOfAdults"":1,""NoOfChild"":0,""ChildAge"":[]},{ ""NoOfAdults"":1,""NoOfChild"":0,""ChildAge"":[]}],""HotelResults"":[{ ""ResultIndex"":5,""HotelCode"":""1356729"",""HotelName"":""Hilton Garden Inn New Delhi"",""HotelCategory"":"""",""StarRating"":4,""HotelDescription"":"""",""HotelPromotion"":"""",""HotelPolicy"":"""",""IsTBOMapped"":true,""Price"":{ ""CurrencyCode"":""INR"",""RoomPrice"":9688.13,""Tax"":0.00,""ExtraGuestCharge"":0.00,""ChildCharge"":0.00,""OtherCharges"":10.00,""Discount"":0.00,""PublishedPrice"":9708.13,""PublishedPriceRoundedOff"":9708,""OfferedPrice"":9698.13,""OfferedPriceRoundedOff"":9698,""AgentCommission"":10.00,""AgentMarkUp"":0.00,""ServiceTax"":2.14,""TCS"":0.00,""TDS"":0.00,""ServiceCharge"":0,""TotalGSTAmount"":2.14,""GST"":{ ""CGSTAmount"":0.00,""CGSTRate"":0,""CessAmount"":0.00,""CessRate"":0,""IGSTAmount"":2.14,""IGSTRate"":18,""SGSTAmount"":0.00,""SGSTRate"":0,""TaxableAmount"":10.00} },""SupplierHotelCodes"":[{ ""CategoryId"":""25###1356729"",""CategoryIndex"":1}],""HotelPicture"":"""",""HotelAddress"":"""",""HotelContactNo"":"""",""HotelMap"":null,""Latitude"":"""",""Longitude"":"""",""HotelLocation"":null,""SupplierPrice"":null,""RoomDetails"":[]},{ ""ResultIndex"":3,""HotelCode"":""11960"",""HotelName"":""Test Property"",""HotelCategory"":"""",""StarRating"":5,""HotelDescription"":""Test Description "",""HotelPromotion"":"""",""HotelPolicy"":"""",""Price"":{ ""CurrencyCode"":""INR"",""RoomPrice"":14182.98,""Tax"":0.00,""ExtraGuestCharge"":0.00,""ChildCharge"":0.00,""OtherCharges"":10.00,""Discount"":0.00,""PublishedPrice"":14292.96,""PublishedPriceRoundedOff"":14293,""OfferedPrice"":14192.98,""OfferedPriceRoundedOff"":14193,""AgentCommission"":99.98,""AgentMarkUp"":0.00,""ServiceTax"":2.14,""TCS"":0.00,""TDS"":0.00,""ServiceCharge"":0,""TotalGSTAmount"":2.14,""GST"":{ ""CGSTAmount"":0.00,""CGSTRate"":0,""CessAmount"":0.00,""CessRate"":0,""IGSTAmount"":2.14,""IGSTRate"":18,""SGSTAmount"":0.00,""SGSTRate"":0,""TaxableAmount"":10.00} },""HotelPicture"":""ENGeeb57131-4453-4eac-a5d9-74eccdc95dce.jpg"",""HotelAddress"":""city center, city center, Delhi, 211222"",""HotelContactNo"":"""",""HotelMap"":null,""Latitude"":""0"",""Longitude"":""0"",""HotelLocation"":null,""SupplierPrice"":null,""RoomDetails"":[]},{ ""ResultIndex"":1,""HotelCode"":""11960"",""HotelName"":""Test Property"",""HotelCategory"":"""",""StarRating"":5,""HotelDescription"":""Test Description "",""HotelPromotion"":"""",""HotelPolicy"":"""",""Price"":{ ""CurrencyCode"":""INR"",""RoomPrice"":14282.96,""Tax"":0.00,""ExtraGuestCharge"":0.00,""ChildCharge"":0.00,""OtherCharges"":0.00,""Discount"":0.00,""PublishedPrice"":14282.96,""PublishedPriceRoundedOff"":14283,""OfferedPrice"":14282.96,""OfferedPriceRoundedOff"":14283,""AgentCommission"":0.00,""AgentMarkUp"":0.00,""ServiceTax"":0.00,""TCS"":0.00,""TDS"":0.00,""ServiceCharge"":0,""TotalGSTAmount"":0.00,""GST"":{ ""CGSTAmount"":0.00,""CGSTRate"":0,""CessAmount"":0.00,""CessRate"":0,""IGSTAmount"":0.00,""IGSTRate"":18,""SGSTAmount"":0.00,""SGSTRate"":0,""TaxableAmount"":0.00} },""HotelPicture"":"""",""HotelAddress"":""city center, city center, Delhi, 211222"",""HotelContactNo"":"""",""HotelMap"":null,""Latitude"":""0"",""Longitude"":""0"",""HotelLocation"":null,""SupplierPrice"":null,""RoomDetails"":[]},{ ""ResultIndex"":4,""HotelCode"":""2879"",""HotelName"":""TEst Des"",""HotelCategory"":"""",""StarRating"":4,""HotelDescription"":""fd "",""HotelPromotion"":"""",""HotelPolicy"":"""",""Price"":{ ""CurrencyCode"":""INR"",""RoomPrice"":72743.12,""Tax"":7141.48,""ExtraGuestCharge"":0.00,""ChildCharge"":0.00,""OtherCharges"":10.00,""Discount"":0.00,""PublishedPrice"":79994.57,""PublishedPriceRoundedOff"":79995,""OfferedPrice"":79894.59,""OfferedPriceRoundedOff"":79895,""AgentCommission"":99.98,""AgentMarkUp"":0.00,""ServiceTax"":2.14,""TCS"":0.00,""TDS"":0.00,""ServiceCharge"":0,""TotalGSTAmount"":2.14,""GST"":{ ""CGSTAmount"":0.00,""CGSTRate"":0,""CessAmount"":0.00,""CessRate"":0,""IGSTAmount"":2.14,""IGSTRate"":18,""SGSTAmount"":0.00,""SGSTRate"":0,""TaxableAmount"":10.00} },""HotelPicture"":""ENGa56b0353-5934-482c-8064-4e5aeb20b8e0.jpg"",""HotelAddress"":""erer, erewr, Delhi, 324324324"",""HotelContactNo"":"""",""HotelMap"":null,""Latitude"":""0"",""Longitude"":""0"",""HotelLocation"":null,""SupplierPrice"":null,""RoomDetails"":[]},{ ""ResultIndex"":2,""HotelCode"":""2879"",""HotelName"":""TEst Des"",""HotelCategory"":"""",""StarRating"":4,""HotelDescription"":""fd "",""HotelPromotion"":"""",""HotelPolicy"":"""",""Price"":{ ""CurrencyCode"":""INR"",""RoomPrice"":72843.10,""Tax"":7141.48,""ExtraGuestCharge"":0.00,""ChildCharge"":0.00,""OtherCharges"":0.00,""Discount"":0.00,""PublishedPrice"":79984.58,""PublishedPriceRoundedOff"":79985,""OfferedPrice"":79984.58,""OfferedPriceRoundedOff"":79985,""AgentCommission"":0.00,""AgentMarkUp"":0.00,""ServiceTax"":0.00,""TCS"":0.00,""TDS"":0.00,""ServiceCharge"":0,""TotalGSTAmount"":0.00,""GST"":{ ""CGSTAmount"":0.00,""CGSTRate"":0,""CessAmount"":0.00,""CessRate"":0,""IGSTAmount"":0.00,""IGSTRate"":18,""SGSTAmount"":0.00,""SGSTRate"":0,""TaxableAmount"":0.00} },""HotelPicture"":"""",""HotelAddress"":""erer, erewr, Delhi, 324324324"",""HotelContactNo"":"""",""HotelMap"":null,""Latitude"":""0"",""Longitude"":""0"",""HotelLocation"":null,""SupplierPrice"":null,""RoomDetails"":[]}]}}";
                var _cityListSetting = new TekTvlHotelSearchRequest()
                {
                    CheckInDate = req.CheckInDate,
                    CheckOutDate = req.CheckOutDate,
                    NoOfNights = req.NoOfNights,
                    CountryCode = req.CountryCode,
                    CityId = req.CityId,
                    IsTBOMapped = true,
                    ResultCount = req.ResultCount,
                    PreferredCurrency = req.PreferredCurrency, //"INR"
                    GuestNationality = req.GuestNationality,
                    NoOfRooms = req.NoOfRooms,
                    PreferredHotel = (req.PreferredHotel == null ? "" : req.PreferredHotel),
                    MaxRating = (req.MaxRating == 0 ? (int)TekTvlHotelValues.HotelRetating.MaxRating : req.MaxRating),
                    MinRating = (req.MinRating == 0 ? (int)TekTvlHotelValues.HotelRetating.MinRating : req.MinRating),
                    ReviewScore = req.ReviewScore,
                    IsNearBySearchAllowed = req.IsNearBySearchAllowed,
                    EndUserIp = req.EndUserIp,
                    TokenId = TrkTvlSession,
                    RoomGuests = req.RoomGuests
                };
                string _resCityList = await AppWebRequest.O.PostJsonDataUsingHWRTLS(GetHotelSearchList, _cityListSetting);
                HotelApiLog(GetHotelSearchList, (JsonConvert.SerializeObject(_cityListSetting, Newtonsoft.Json.Formatting.Indented)), _resCityList, "HotelApiML", "Hotelsearch", req.EndUserIp);
                var HotelResp = new TekTvlSearchingResponse();
                //var _d = JsonConvert.DeserializeObject<TekTvlSearchingResponse>(_resCityList);
                HotelResp = JsonConvert.DeserializeObject<TekTvlSearchingResponse>(_resCityList);
                //HotelResp = JsonConvert.DeserializeObject<TekTvlSearchingResponse>(tst);
                resp = HotelResp;
                if (HotelResp.HotelSearchResult.ResponseStatus == "1")
                {
                    resp = (TekTvlSearchingResponse)HotelResp;
                }
            }


            return resp;
        }


        public async Task<TekTvlHotelSingleResponse> GetHotelSingleApiInfo(TekTvlSingleHotelInfoApiRequest req)
        {
            HotelApiML _HotelApiML = new HotelApiML(_accessor, _env);

            //validates Hotel Tokend Start
            bool IsCallAPI = string.IsNullOrEmpty(TrkTvlSession) ? true : TrkTvlExpiry == null ? true : TrkTvlExpiry < DateTime.Now ? true : false;
            if (IsCallAPI)
            {
                HotelModelResponce Auth = new HotelModelResponce();
                Auth = await HotelApiTokenAuth(req.EndUserIp);
            }
            //validates Hotel Tokend Start End
            if (!string.IsNullOrEmpty(TrkTvlSession) && TrkTvlExpiry > DateTime.Now)
            {
                var _hotelSingleSetting = new TekTvlSingleHotelInfoApiRequest()
                {
                    TokenId = TrkTvlSession,
                    EndUserIp = req.EndUserIp,
                    HotelCode = req.HotelCode,
                    ResultIndex = req.ResultIndex,
                    TraceId = req.TraceId,
                    CategoryId = req.CategoryId
                };
                string _resHotelList = await AppWebRequest.O.PostJsonDataUsingHWRTLS(GetHotelSingleInfo, _hotelSingleSetting);
                //save req res
                HotelApiLog(GetHotelSingleInfo, (JsonConvert.SerializeObject(_hotelSingleSetting, Newtonsoft.Json.Formatting.Indented)), _resHotelList, "HotelApiML", "GetHotelSingleApiInfo", req.EndUserIp);
                //save req res Close
                var res = JsonConvert.DeserializeObject<TekTvlHotelSingleResponse>(_resHotelList);
                return res;
            }
            return new TekTvlHotelSingleResponse();
        }

        public async Task<TekTvlSingleHotelRoomRes> GetHotelSingleRoomApiRes(TekTvlSingleHotelRoomReq req)
        {
            HotelApiML _HotelApiML = new HotelApiML(_accessor, _env);
            //validates Hotel Tokend Start
            bool IsCallAPI = string.IsNullOrEmpty(TrkTvlSession) ? true : TrkTvlExpiry == null ? true : TrkTvlExpiry < DateTime.Now ? true : false;
            if (IsCallAPI)
            {
                HotelModelResponce Auth = new HotelModelResponce();
                Auth = await HotelApiTokenAuth(req.EndUserIp);

            }
            //validates Hotel Tokend Start End

            if (!string.IsNullOrEmpty(TrkTvlSession) && TrkTvlExpiry > DateTime.Now)
            {
                var _hotelSingleSetting = new TekTvlSingleHotelRoomReq()
                {
                    TokenId = TrkTvlSession,
                    EndUserIp = req.EndUserIp,
                    HotelCode = req.HotelCode,
                    ResultIndex = req.ResultIndex,
                    TraceId = req.TraceId
                };
                var res = new TekTvlSingleHotelRoomRes();
                string _resHotelList = await AppWebRequest.O.PostJsonDataUsingHWRTLS(GetHotelSingleRoom, _hotelSingleSetting);
                HotelApiLog(GetHotelSingleRoom, (JsonConvert.SerializeObject(_hotelSingleSetting, Newtonsoft.Json.Formatting.Indented)), _resHotelList, "HotelApiML", "GetHotelSingleRoomApiRes", req.EndUserIp);
                return JsonConvert.DeserializeObject<TekTvlSingleHotelRoomRes>(_resHotelList);
            }

            return new TekTvlSingleHotelRoomRes();
        }

        public async Task<HotelRoomBookRepsonse> ProceedHotelBookingApiReq(TekTvlHotelBookRoomReq req)
        {
            var res = new HotelRoomBookRepsonse();
            HotelApiML _HotelApiML = new HotelApiML(_accessor, _env);

            //validates Hotel Tokend Start
            bool IsCallAPI = string.IsNullOrEmpty(TrkTvlSession) ? true : TrkTvlExpiry == null ? true : TrkTvlExpiry < DateTime.Now ? true : false;
            if (IsCallAPI)
            {
                HotelModelResponce Auth = new HotelModelResponce();
                Auth = await HotelApiTokenAuth(req.EndUserIp);
            }
            //validates Hotel Tokend Start End

            if (!string.IsNullOrEmpty(TrkTvlSession) && TrkTvlExpiry > DateTime.Now)
            {
                string catID = null;
                foreach (var item in req.HotelRoomsDetails)
                {
                    if (item.SmokingPreference.ToString() == "NoPreference")
                    {
                        item.SmokingPreference = 0;
                    }
                    else if (item.SmokingPreference.ToString() == "Smoking")
                    {
                        item.SmokingPreference = 1;
                    }
                    else if (item.SmokingPreference.ToString() == "NonSmoking")
                    {
                        item.SmokingPreference = 2;
                    }
                    else if (item.SmokingPreference.ToString() == "Either")
                    {
                        item.SmokingPreference = 3;
                    }
                    else
                    {
                        item.SmokingPreference = 0;
                    }
                    catID = item.CategoryId;
                    foreach (var items in item.HotelPassenger)
                    {
                        if (items.Age > 12)
                        {
                            items.PaxType = 1;
                            if (items.LeadPassenger == true)
                            {
                                items.LeadPassenger = true;
                            }
                            else
                            {
                                items.LeadPassenger = false;
                            }
                        }
                        else if (items.Age <= 12 && items.Age >= 1)
                        {
                            items.PaxType = 2;
                        }
                        else
                        {
                            items.PaxType = 0;
                        }
                        if (items.PassportIssueDate == null)
                        {
                            DateTime dt = Convert.ToDateTime("0001-01-01");
                            items.PassportIssueDate = dt;
                            items.PassportExpDate = dt;
                        }


                    }
                }

                var _hotelBookSetting = new TekTvlHotelBookRoomReq()
                {
                    ResultIndex = req.ResultIndex,
                    HotelCode = req.HotelCode,
                    HotelName = req.HotelName,
                    GuestNationality = req.GuestNationality,
                    NoOfRooms = req.NoOfRooms,
                    ClientReferenceNo = "0",
                    IsVoucherBooking = "true",
                    IsPackageFare = true,
                    CategoryId = catID,
                    HotelRoomsDetails = req.HotelRoomsDetails,
                    EndUserIp = req.EndUserIp,
                    TokenId = TrkTvlSession,
                    TraceId = req.TraceId,
                };
                string _resHotelList = await AppWebRequest.O.PostJsonDataUsingHWRTLS(HotelBookReq, _hotelBookSetting);
                HotelApiLog(HotelBookReq, (JsonConvert.SerializeObject(_hotelBookSetting, Newtonsoft.Json.Formatting.Indented)), _resHotelList, "HotelApiML", "ProceedHotelBookingApiReq", req.EndUserIp);
                res = JsonConvert.DeserializeObject<HotelRoomBookRepsonse>(_resHotelList);
                //return res;
            }
            return res;
        }



        public async Task<blockresponse> HotelBlockRoomApiReq(HotelRoomBlockReq _req, TekTvlHotelBookRoomReq Bookreq, TekTvlHotelSearchRequest tth)
        {
            HotelApiML _HotelApiML = new HotelApiML(_accessor, _env);
            var blockrespose = new blockresponse()
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            bool IsCallAPI = string.IsNullOrEmpty(TrkTvlSession) ? true : TrkTvlExpiry == null ? true : TrkTvlExpiry < DateTime.Now ? true : false;
            if (IsCallAPI)
            {
                HotelModelResponce Auth = new HotelModelResponce();
                Auth = await HotelApiTokenAuth(_req.EndUserIp);
                if (Auth.Error.ErrorCode == "-1")
                {
                    blockrespose.Msg = Auth.Error.ErrorMessage;
                    return blockrespose;
                }
            }
            if (!string.IsNullOrEmpty(TrkTvlSession) && TrkTvlExpiry > DateTime.Now)
            {
                foreach (var item in _req.HotelRoomsDetails)
                {
                    if (item.SmokingPreference != null)
                    {
                        item.SmokingPreference = item.SmokingPreference.ToString() == "NoPreference" ? 0 : item.SmokingPreference.ToString() == "Smoking" ? 1 : item.SmokingPreference.ToString()
                            == "NonSmoking" ? 2 : item.SmokingPreference.ToString() == "Either" ? 3 : 0;
                    }
                    if (!string.IsNullOrEmpty(item.CategoryId))
                    {
                        _req.CategoryId = item.CategoryId;
                    }
                }
                _req.TokenId = TrkTvlSession;
                _req.IsVoucherBooking = true;
                _req.ClientReferenceNo = "0";
                string _resHotelList = await AppWebRequest.O.PostJsonDataUsingHWRTLS(HotelBlockRoomReq, _req);
                HotelApiLog(HotelBlockRoomReq, (JsonConvert.SerializeObject(_req, Newtonsoft.Json.Formatting.Indented)), _resHotelList, "HotelApiML", "HotelBlockRoomApiReq", _req.EndUserIp);
                var response = JsonConvert.DeserializeObject<BlockRoomResponse>(_resHotelList);
                if (response != null)
                {
                    if (response.BlockRoomResult.Error.ErrorCode == 0)
                    {
                        var hbreq = new HotelBook()
                        {
                            LT = _lr.LoginTypeID,
                            UserID = _lr.UserID,
                            TokenID = _req.TokenId,
                            TraceID = _req.TraceId,
                            RequestModeID = RequestMode.PANEL,
                            BookingId = 0,
                            EndUserIP = _req.EndUserIp,
                            AvailabilityType = response.BlockRoomResult.AvailabilityType,
                            HotelCode = _req.HotelCode,
                            HotelName = _req.HotelName,
                            NoOfRooms = _req.NoOfRooms,
                            OPID = HotelOPID.HotelOPIDDomestic,
                            ConfirmationNo = string.Empty,
                            HotelBookingStatus = RechargeRespType.PENDING,
                            IsPriceChanged = false,
                            CheckInDate = Convert.ToDateTime(tth.CheckInDate),
                            CheckOutDate = Convert.ToDateTime(tth.CheckOutDate),
                            DestinationID = tth.CityId
                        };
                        foreach (var item in tth.RoomGuests)
                        {
                            hbreq.Adults = item.NoOfAdults;
                            hbreq.Childs = item.NoOfChild;
                        }
                        if (response.BlockRoomResult.HotelRoomsDetails != null)
                        {
                            foreach (var item in response.BlockRoomResult.HotelRoomsDetails)
                            {

                                hbreq.TotalPrice = item.Price.PublishedPriceRoundedOff;
                            }
                        }
                        if (hbreq.TotalPrice < 1)
                        {
                            blockrespose.Msg = "Invaid Booking Amount!";
                            return blockrespose;
                        }
                        foreach (var item in Bookreq.HotelRoomsDetails)
                        {

                            foreach (var psnger in item.HotelPassenger)
                            {
                                if (psnger.LeadPassenger == true)
                                {
                                    if (psnger.Phoneno != null)
                                    {
                                        hbreq.AccountNo = psnger.Phoneno.ToString();
                                        break;
                                    }
                                }
                            }
                            var finalList = item.HotelPassenger.Select(x => new
                            {
                                _IsLead = x.LeadPassenger,
                                _IsChild = x.Age > 12 ? 0 : 1,
                                _Title = x.Title,
                                _Name = x.FirstName + " " + x.Middlename + " " + x.LastName,
                                _MobileNo = x.Phoneno.ToString()
                            ,
                                _EmailID = x.Email.ToString(),
                                _Age = x.Age,
                                _PanNo = x.PAN,
                                _PassportNo = x.PassportNo.ToString(),
                                _PassportIssueDate = x.PassportIssueDate,
                                _PassportExpDate = x.PassportExpDate
                            }).ToList();
                            hbreq.DTGuestDetails = finalList.ToDataTable();
                        }
                        if (string.IsNullOrEmpty(hbreq.AccountNo))
                        {
                            blockrespose.Msg = "Invaid Account No!";
                            return blockrespose;
                        }
                        //Hotel Book Transaction 
                        IProcedure _proc = new ProcHotelBooking(_dal);
                        var hbres = (TekTvlError)_proc.Call(hbreq);
                        blockrespose.IsPriceChange = response.BlockRoomResult.IsPriceChanged;
                        blockrespose.IsPolicychanges = response.BlockRoomResult.IsCancellationPolicyChanged;
                        blockrespose.Blockstatus = "Success";
                        blockrespose.AvailabilityType = response.BlockRoomResult.AvailabilityType;
                        //Hotel Final Booking
                        if (response.BlockRoomResult.AvailabilityType == "Confirm")
                        {
                            if (response.BlockRoomResult.IsCancellationPolicyChanged == false && response.BlockRoomResult.IsPriceChanged == false)
                            {
                                var res = await ProceedHotelBookingApiReq(Bookreq).ConfigureAwait(false);
                                if (res.BookResult.Error.ErrorCode == 0 && res.BookResult.HotelBookingStatus == "Confirmed")
                                {
                                    var UpdateBookingStatus = new HotelBook()
                                    {
                                        TID = hbres.TID,
                                        UserID = _lr.UserID,
                                        HotelBookingStatus = RechargeRespType.SUCCESS,
                                        ConfirmationNo = res.BookResult.ConfirmationNo,
                                        BookingId = res.BookResult.BookingId
                                    };
                                    IProcedure _procs = new ProcUpdateHotelTransactionStatus(_dal);

                                    return (blockresponse)_procs.Call(UpdateBookingStatus);
                                }
                                else
                                {
                                    blockrespose.Msg = response.BlockRoomResult.Error.ErrorMessage;
                                    blockrespose.IsError = true;
                                    return blockrespose;
                                }
                            }
                            else
                            {

                            }
                        }
                        else if (blockrespose.AvailabilityType == "Available")
                        {

                        }
                    }
                    else
                    {
                        blockrespose.Msg = response.BlockRoomResult.Error.ErrorMessage;
                        return blockrespose;
                    }
                }
            }
            //return res;
            return blockrespose;
        }





        public async Task<string> BookRoomDetails(TekTvlHotelBookedRoomReqRes _req)
        {
            HotelApiML _HotelApiML = new HotelApiML(_accessor, _env);
            bool IsCallAPI = false;
            if (string.IsNullOrEmpty(TrkTvlSession))
            {
                IsCallAPI = true;
            }
            else if (TrkTvlExpiry == null)
            {
                IsCallAPI = true;
            }
            else if (TrkTvlExpiry < DateTime.Now)
            {
                IsCallAPI = true;
            }
            HotelModelResponce model = new HotelModelResponce();
            if (IsCallAPI)
            {
                string _responce = await AppWebRequest.O.PostJsonDataUsingHWRTLS(AuthunticateReq, appSetting);
                if (!string.IsNullOrEmpty(_responce))
                {
                    HotelApiLog(AuthunticateReq, (JsonConvert.SerializeObject(appSetting, Newtonsoft.Json.Formatting.Indented)), _responce, "HotelApiML", "BookRoomDetails", _req.EndUserIp);
                    var HotelResp = JsonConvert.DeserializeObject<HotelModelResponce>(_responce);
                    if (HotelResp.Status == "1")
                    {
                        TrkTvlSession = HotelResp.TokenId;
                        TrkTvlExpiry = string.IsNullOrEmpty(TrkTvlSession) ? DateTime.Now.AddMinutes(-10) : DateTime.Now.AddHours(24);
                        model.TokenId = HotelResp.TokenId;
                    }
                    else
                    {
                        model.Error = HotelResp.Error;
                    }
                }
                else
                {
                    model.Error.ErrorMessage = "Server not responding.";
                    model.Error.ErrorCode = "-1";
                }
            }
            if (!string.IsNullOrEmpty(TrkTvlSession) && TrkTvlExpiry > DateTime.Now)
            {
                _req.TokenId = TrkTvlSession;

                string _resHotelList = await AppWebRequest.O.PostJsonDataUsingHWRTLS(HotelBookedRoom, _req);
                HotelApiLog(HotelBookedRoom, (JsonConvert.SerializeObject(_req, Newtonsoft.Json.Formatting.Indented)), _resHotelList, "HotelApiML", "HotelBlockRoomApiReq", _req.EndUserIp);
                var blockrespose = new blockresponse();
                var response = JsonConvert.DeserializeObject<BlockRoomResponse>(_resHotelList);
                return _resHotelList;
            }
            //return res;
            return null;
        }
        private bool HotelApiLog(string Requrl, string request, string response, string ClassName, string methodname, string EndUserIP)
        {
            IHotelDestination _hot = new HotelML(_accessor, _env);
            var reqresdata = new HotelApiReqRes()
            {
                ReqUrl = Requrl,
                Request = request,
                Response = response,
                ClassName = ClassName,
                Method = methodname,
                EndUserIP = EndUserIP,
                UserID = _lr != null ? _lr.UserID : 0
            };
            var bookreqres = _hot.HotelApiReqRes(reqresdata);
            return true;
        }
        private TvkHotelResponseRoot DeserializeTvkHotelXmlObject(string xmlStr)
        {
            TvkHotelResponseRoot res = new TvkHotelResponseRoot();
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlStr);
                var jsonStr = JsonConvert.SerializeXmlNode(doc);
                jsonStr = jsonStr.Replace("\"Attribute\":{", "\"Attribute\":[{").Replace("}}}]}}", "}]}}]}}").Replace("}}},{\"@BrandCode", "}]}},{\"@BrandCode");
                //jsonStr = jsonStr.Replace("\"VendorMessage\":{", "\"VendorMessage\":[{").Replace("}},\"Position\"", "}]},\"Position\"");
                //jsonStr = jsonStr.Replace("\"SubSection\":{", "\"SubSection\":[{").Replace("}},\"Title\"", "}]},\"Title\"");
                res = JsonConvert.DeserializeObject<TvkHotelResponseRoot>(jsonStr);
            }
            catch(Exception ex)
            { }
            return res;
        }
        private HotelImagesData DeserializeTvkHotelXmlObjectByNode(string xmlStr)
        {
            HotelImagesData res = new HotelImagesData();
            var lstimages = new List<HotelImages>();
            var lstfacility = new List<HotelFacilities>();
            var TBOHotelCode = "";
            var HotelCityCode = "";
            try
            {
                XmlDocument xml = new XmlDocument();

                //BasicPropertyInfo info
                xml.LoadXml(xmlStr);
                XmlNodeList xnList = xml.SelectNodes("/ArrayOfBasicPropertyInfo/BasicPropertyInfo");
                if (xnList.Count<1)
                {
                    return res;
                }
                foreach (XmlNode xn in xnList)
                {
                     TBOHotelCode = xn.Attributes["TBOHotelCode"].Value;
                     HotelCityCode = xn.Attributes["HotelCityCode"].Value;
                }
                //BasicPropertyInfo info Ends

                var __xml = XDocument.Parse(xmlStr);
                var topNode = __xml.Descendants("BasicPropertyInfo").FirstOrDefault();
                if (topNode == null)
                {
                    return res;
                }
                var decent = topNode.Descendants().Where(x => Convert.ToString(x.NodeType) == "Element").ToList();
                decent = decent.Where(x => x.Name.LocalName == "VendorMessages").ToList();
                var vendorMessages = decent.FirstOrDefault().Descendants().Where(x => x.Name.LocalName == "VendorMessage").ToList();
                var subSection = vendorMessages.Where(x => x.FirstAttribute.Value == "Hotel Pictures").ToList();
                var Facilities = vendorMessages.Where(x => x.FirstAttribute.Value == "Facilities").ToList();
                for (int i = 0; i < subSection.Count; i++)
                {
                    var xx = XDocument.Parse(subSection[0].ToString()).Descendants();
                    var paragrapgh = xx.Where(x => x.Name.LocalName == "Paragraph");
                    foreach (var p in paragrapgh)
                    {
                        if (!string.IsNullOrEmpty(p.Value))
                        {
                            lstimages.Add(new HotelImages
                            {
                                Type = (string)p.Attribute("Type"),
                                ImageURL = p.Value,
                                CityId = HotelCityCode,
                                TBOHotelCode = TBOHotelCode
                            });
                        }
                    }
                }
                res.HotelImages = lstimages;
                for (int i = 0; i < Facilities.Count; i++)
                {
                    var xx = XDocument.Parse(Facilities[0].ToString()).Descendants();
                    var paragrapgh = xx.Where(x => x.Name.LocalName == "Paragraph");
                    foreach (var p in paragrapgh)
                    {
                        if (!string.IsNullOrEmpty(p.Value))
                        {
                            lstfacility.Add(new HotelFacilities
                            {
                                Title = "Facilities",
                                Fcility = p.Value?.Trim().Replace("undefined", ""),
                                CityId = HotelCityCode,
                                TBOHotelCode = TBOHotelCode
                            });
                        }
                    }
                }
                res.HotelFacility = lstfacility;
            }
            catch (Exception ex)
            { }
            return res;
        }
        public List<string> ReadXmlFileByNode()
        {
            List<string> urlList = new List<string>();
            string xmlString = System.IO.File.ReadAllText("DATA/XML/HotelSingle.xml");
          
            XmlDocument xml = new XmlDocument();
            var __xml = XDocument.Parse(xmlString);
            var topNode = __xml.Descendants("BasicPropertyInfo").FirstOrDefault();
            var decent = topNode.Descendants().Where(x => Convert.ToString(x.NodeType) == "Element").ToList();
            decent = decent.Where(x => x.Name.LocalName == "VendorMessages").ToList();
            var vendorMessages = decent.FirstOrDefault().Descendants().Where(x => x.Name.LocalName == "VendorMessage").ToList();
            var subSection = vendorMessages.Where(x => x.FirstAttribute.Value == "Hotel Pictures").ToList();
            for (int i = 0; i < subSection.Count; i++)
            {
                var xx = XDocument.Parse(subSection[0].ToString()).Descendants();
                var paragrapgh = xx.Where(x => x.Name.LocalName == "Paragraph");
                foreach (var p in paragrapgh)
                {
                    foreach (Match item in Regex.Matches(p.Value, @"(http|ftp|https):\/\/([\w\-_]+(?:(?:\.[\w\-_]+)+))([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?(.jpg|.png|.jpeg)"))
                    {
                        urlList.Add(item.Value);
                    }
                }
            }
            return urlList;
        }

        public async Task<ResponseStatus> HotelBookingCancelAPI(CancelBookingRequest cbr, int TID, string RPID)
        {
            HotelApiML _HotelApiML = new HotelApiML(_accessor, _env);
            var ResponseStatus = new ResponseStatus()
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            bool IsCallAPI = string.IsNullOrEmpty(TrkTvlSession) ? true : TrkTvlExpiry == null ? true : TrkTvlExpiry < DateTime.Now ? true : false;
            if (IsCallAPI)
            {
                HotelModelResponce Auth = new HotelModelResponce();
                Auth = await HotelApiTokenAuth(cbr.EndUserIp);
                if (Auth.Error.ErrorCode == "-1")
                {
                    ResponseStatus.Msg = Auth.Error.ErrorMessage;
                    return ResponseStatus;
                }
            }
            if (!string.IsNullOrEmpty(TrkTvlSession) && TrkTvlExpiry > DateTime.Now)
            {
                cbr.TokenId = TrkTvlSession;
                string _HotelCancelResponse = await AppWebRequest.O.PostJsonDataUsingHWRTLS(HotelBookingCancel, cbr);
                HotelApiLog(HotelBlockRoomReq, (JsonConvert.SerializeObject(cbr, Newtonsoft.Json.Formatting.Indented)), _HotelCancelResponse, "HotelApiML", "HotelBookingCancelAPI", cbr.EndUserIp);
                if (!string.IsNullOrEmpty(_HotelCancelResponse))
                {
                    var response = JsonConvert.DeserializeObject<CancelBookingResponse>(_HotelCancelResponse);
                    if (response != null)
                    {
                        if (response.HotelChangeRequestResult.Error.ErrorCode == 0)
                        {
                            if (response.HotelChangeRequestResult.ChangeRequestStatus == HotelBookingCancelStatus.Processed)
                            {
                                RefundRequest refundreq = new RefundRequest();
                                refundreq.TID = TID;
                                refundreq.RPID = RPID;
                                ReportML ml = new ReportML(_accessor, _env);
                                var StstusResponse = UpdateHotelStatus(TID, cbr.BookingId, response.HotelChangeRequestResult.ChangeRequestId, response.HotelChangeRequestResult.ChangeRequestStatus);
                                var DisputeResp = ml.MarkDispute(refundreq);
                                ResponseStatus.Statuscode = 1;
                                ResponseStatus.Msg = "Hotel Cancel Successfull.";
                            }
                            if (response.HotelChangeRequestResult.ChangeRequestStatus == HotelBookingCancelStatus.Rejected)
                            {
                                ResponseStatus.Msg = "Hotel Booking Cacnel Is Rejected.";
                            }
                        }
                        else
                        {
                            ResponseStatus.Msg = response.HotelChangeRequestResult.Error.ErrorMessage;
                        }
                    }
                }
            }
            //return res;
            return ResponseStatus;
        }
        public ResponseStatus UpdateHotelStatus(int TID, int BookingID, int CancelRequestId, int CancelRequestStatus)
        {
            var _req = new CommonReq()
            {
                LoginID = _lr.UserID,
                CommonInt = TID,
                CommonInt2 = BookingID,
                CommonInt3 = CancelRequestId,
                CommonInt4 = CancelRequestStatus
            };
            IProcedure proc = new ProcUpdateHotelCancelStatus(_dal);
            return (ResponseStatus)proc.Call(_req);
        }
    }
}
