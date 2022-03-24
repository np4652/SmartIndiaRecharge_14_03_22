using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Interfaces
{
    interface ICoupanVoucherML
    { 
        IEnumerable<CoupanMaster> GetCouponMaster();
        CoupanMaster GetCouponMaster(int Id);
        IResponseStatus UpdateCouponMaster(CoupanMaster couponMaster);
        IResponseStatus UpdateCouponVoucher(CoupanVoucher couponvoucher);
        IEnumerable<CoupanVoucher> GetCouponVoucher(int Id);
        IEnumerable<DenominationVoucher> GetDenominationVoucher();
        IResponseStatus UpdateCouponSetting(DenominationVoucher para);
        IResponseStatus ChangeCouponStatus(int ID);
        IEnumerable<DenominationVoucher> GetCouponSetting(int ID);
        Task<IResponseStatus> UploadCouponExcelUPloda(CoupanVoucherEXlReq reqData);
        IEnumerable<VoucherImage> GetCouponvocherImage(int OID);
        IResponseStatus DelCouponVoucher(CoupanVoucher couponvoucher);
    }
}
