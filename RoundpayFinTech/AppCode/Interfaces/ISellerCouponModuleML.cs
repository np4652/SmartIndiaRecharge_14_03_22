using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Interfaces
{
    interface ISellerCouponModuleML
    {
        IEnumerable<CoupanMaster> GetCouponVoucherList(int OPID);
     IResponseStatus SaveCouponVocher(CouponData data);
        
        IEnumerable<ImageCount_OID> GetCouponvocherImage(int optype);
        bool CountImage();
    }
}
