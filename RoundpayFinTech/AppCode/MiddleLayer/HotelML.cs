using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using RoundpayFinTech.AppCode.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.ThirdParty.HotelAPI;
using RoundpayFinTech.AppCode.Model;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public class HotelML : IHotelDestination
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly ISession _session;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        
        private readonly IRequestInfo _info;
        public HotelML(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            _info = new RequestInfo(_accessor, _env);
            bool IsProd = _env.IsProduction();


        }
        public List<HotelDestination> Hotels(CommonReq UserID)
        {
            var bMList = new List<HotelDestination>();
            IProcedure _proc = new ProcGetHotelDestination(_dal);
            bMList = (List<HotelDestination>)_proc.Call(UserID);
            return bMList;
        }
        public TekTvlError HotelApiReqRes(HotelApiReqRes ReqRes)
        {
            var horeqres = new TekTvlError();
            IProcedure _proc = new ProcSaveHotelReqResp(_dal);
            horeqres = (TekTvlError)_proc.Call(ReqRes);
            return horeqres;
        }
        public async Task<TekTvlErrorModel> SyncHotelDestinationDataUsingApi()
        {
            List<TekTvlDestinations> lstcitycode = new List<TekTvlDestinations>();
            var res = new TekTvlErrorModel()
            {
                ErrorCode = "-1",
                ErrorMessage = ErrorCodes.TempError
            };
            string _IP = _info.GetRemoteIP();
            ITrkTravalHotelMLAPI _api = new HotelApiML(_accessor, _env);
            lstcitycode = await _api.SyncHotelDestinationApiDAta(_IP);
            if (lstcitycode != null)
            {
                foreach (var item in lstcitycode)
                {
                    if (item.DestinationId > 0)
                    {
                        try
                        {
                            await _api.SyncHotelMasterApiDAta(_IP, Convert.ToString(item.DestinationId));
                            break;
                        }
                        catch (Exception ex)
                        { }
                    }
                }
                res.ErrorCode = "1";
                res.ErrorMessage = "Success";
            }
            return res;
        }


        public async Task<TekTvlErrorModel> SyncHotelDataByHotelCode()
        {
            string _IP = _info.GetRemoteIP();
            List<HotelImages> lstcitycode = new List<HotelImages>();
            var res = new TekTvlErrorModel()
            {
                ErrorCode = "-1",
                ErrorMessage = ErrorCodes.TempError
            };
            IProcedure _proc = new ProcGetHotelCode(_dal);
            lstcitycode = (List<HotelImages>)_proc.Call();
            ITrkTravalHotelMLAPI _api = new HotelApiML(_accessor, _env);
            if (lstcitycode != null)
            {
                foreach (var item in lstcitycode)
                {
                    if (!string.IsNullOrEmpty(item.CityId) && !string.IsNullOrEmpty(item.TBOHotelCode))
                    {
                        try
                        {
                            await _api.SyncHotelMasterApiDAtaByHotelCode(_IP, item.CityId, item.TBOHotelCode);
                        }
                        catch (Exception ex)
                        { }
                    }
                }
                res.ErrorCode = "1";
                res.ErrorMessage = "Success";
            }
            return res;
        }
        public async Task<TekTvlErrorModel> SyncHotelMasterDataUsingApi(string cityId)
        {
            string _IP = _info.GetRemoteIP();
            ITrkTravalHotelMLAPI _api = new HotelApiML(_accessor, _env);
            return await _api.SyncHotelMasterApiDAta(_IP, cityId);
        }

        public async Task<TekTvlSearchingResponse> HotelsearchDataUsingApi(TekTvlHotelSearchRequest obj)
        {
            obj.EndUserIp = _info.GetRemoteIP();
            ITrkTravalHotelMLAPI _api = new HotelApiML(_accessor, _env);

            return await _api.Hotelsearch(obj);

        }

        public async Task<TekTvlHotelSingleResponse> GetHotelSingleApiInfo(TekTvlSingleHotelInfoApiRequest req)
        {
            req.EndUserIp = _info.GetRemoteIP();
            HotelApiML hotelApi = new HotelApiML(_accessor, _env);
            return await hotelApi.GetHotelSingleApiInfo(req).ConfigureAwait(false);
        }

        public async Task<TekTvlSingleHotelRoomRes> GetHotelSingleRoomApiReq(TekTvlSingleHotelRoomReq req)
        {
            req.EndUserIp = _info.GetRemoteIP();
            HotelApiML hotelApi = new HotelApiML(_accessor, _env);
            return await hotelApi.GetHotelSingleRoomApiRes(req).ConfigureAwait(false);
        }

        public async Task<IEnumerable<TBOBasicPropertyInfo>> GetHotelInfo(TvkHotelInfoRequest req)
        {
            IProcedureAsync _proc = new ProcGetHotelInfo(_dal);
            return (List<TBOBasicPropertyInfo>)await _proc.Call(req).ConfigureAwait(false);
        }

        public async Task<IEnumerable<string>> GetHotelFacility(string hc)
        {
            IProcedure _proc = new ProcGetHotelFacility(_dal);
            return (List<string>)_proc.Call(hc);
        }

        public async Task<HotelRoomBookRepsonse> ProceedHotelBookApiReq(TekTvlHotelBookRoomReq req)
        {
            req.EndUserIp = _info.GetRemoteIP();
            HotelApiML hotelApi = new HotelApiML(_accessor, _env);
            var res=new HotelRoomBookRepsonse();
        res=await hotelApi.ProceedHotelBookingApiReq(req).ConfigureAwait(false);
            return res;
        }


        public async Task<blockresponse> HotelBlockRoomApiReq(HotelRoomBlockReq _req ,TekTvlHotelBookRoomReq Bookreq, TekTvlHotelSearchRequest tth)
        {
           _req.EndUserIp = _info.GetRemoteIP();
            Bookreq.EndUserIp = _req.EndUserIp;
            HotelApiML hotelApi = new HotelApiML(_accessor, _env);
           var res = await hotelApi.HotelBlockRoomApiReq(_req, Bookreq, tth).ConfigureAwait(false);
            return res;
        }
        public async Task<string> BookRoomDetails(TekTvlHotelBookedRoomReqRes _req)
        {
            _req.EndUserIp = _info.GetRemoteIP();
            HotelApiML hotelApi = new HotelApiML(_accessor, _env);
            var res = await hotelApi.BookRoomDetails(_req).ConfigureAwait(false);
            return res;
        }

        public async Task<ResponseStatus> HotelCancelAPiReq(int BookingID,int TID,string RPID, string Remark)
        {
            CancelBookingRequest creq = new CancelBookingRequest();
            creq.BookingId = BookingID;
            creq.Remark = Remark;
            creq.EndUserIp = _info.GetRemoteIP();
            HotelApiML hotelApi = new HotelApiML(_accessor, _env);
            return  await hotelApi.HotelBookingCancelAPI(creq, TID, RPID).ConfigureAwait(false);
        }


    }
}
