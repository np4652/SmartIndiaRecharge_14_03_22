using RoundpayFinTech.AppCode.ThirdParty.HotelAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface ITrkTravalHotelMLAPI
    {
        Task<List<TekTvlDestinations>> SyncHotelDestinationApiDAta( string ip);
        Task<TekTvlSearchingResponse> Hotelsearch(TekTvlHotelSearchRequest req);
        Task<TekTvlErrorModel> SyncHotelMasterApiDAta(string ip, string cityId);
        Task<TekTvlErrorModel> SyncHotelMasterApiDAtaByHotelCode(string ip, string cityId,string HoelCode);
    }
}
