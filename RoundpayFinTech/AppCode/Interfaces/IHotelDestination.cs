using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.ThirdParty.HotelAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface IHotelDestination
    {
        List<HotelDestination> Hotels(CommonReq _req);
        TekTvlError HotelApiReqRes(HotelApiReqRes _req);
        Task<TekTvlErrorModel> SyncHotelDestinationDataUsingApi();
        Task<TekTvlSearchingResponse> HotelsearchDataUsingApi(TekTvlHotelSearchRequest obj);
        Task<TekTvlErrorModel> SyncHotelMasterDataUsingApi(string cityId);
    }
}
