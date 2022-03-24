using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Model;
using Fintech.AppCode.Interfaces;


namespace RoundpayFinTech.AppCode.Interfaces
{
    interface IAdvertisementML
    {
        IResponseStatus UpdateAdvertisement(FileUploadAdvertisementRequest advertisementrequest);
        IEnumerable<AdvertisementRequest> GetAdvertisement(AdvertisementRequest advertisementrequest);
        IEnumerable<AdvertisementPackage> GetAdvertisementPackage();
        ResponseStatus ApprovedAdvertisement(AdvertisementRequest UserData);
        IEnumerable<AdvertisementRequest> _GetAdvertisement();
    }
}
