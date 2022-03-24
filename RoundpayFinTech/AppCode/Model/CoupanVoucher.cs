using Fintech.AppCode.Model;
using Microsoft.AspNetCore.Http;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model
{
    public class CoupanMaster : DenominationVoucher
    {
        public bool IsAdmin { get; set; }

        public int ID { get; set; } = 0;
        public int OID { get; set; } = 0;
        public string VoucherType { get; set; }
        public string Remark { get; set; }
        public string LastModifyDate { get; set; }
        public string OpName { get; set; }

        public int Min { get; set; } = 0;
        public int Max { get; set; } = 0;
        public bool IsActive { get; set; } = true;



        public IEnumerable<VoucherImage> VocImage { get; set; }
        public IEnumerable<OperatorDetail> OpDetail { get; set; }

    }

    public class CoupanReq : CommonReq
    {
        public CoupanMaster coupanMaster { get; set; }
        public IEnumerable<CoupanMaster> CoupanDetail { get; set; }
        public IEnumerable<OperatorDetail> OpDetail { get; set; }

    }

    public class CoupanVoucher : CommonReq
    {
        public int ID { get; set; }
        public int VoucherID { get; set; }
        public string APIID { get; set; }
        public string ApiName { get; set; }
        public string CouponCode { get; set; }
        public int Amount { get; set; }
        public string EntryBy { get; set; }
        public string EntryDate { get; set; }
        public string ModifyBy { get; set; }
        public string ModifyDate { get; set; }
        public bool IsSale { get; set; }



    }

    public class CoupanVoucherReq
    {
        public IEnumerable<CoupanVoucher> CoupanVoucher { get; set; }


    }

    public class DenominationVoucher : CommonReq
    {
        public int DenominationID { get; set; }
        public int DenminationAmount { get; set; }
        public int VoucherID { get; set; }
        public int MapDenominationID { get; set; }
        public bool IsActive { get; set; }
        public string DenominationIDs { get; set; }


    }
    public class CouponsettingReq
    {
        public IEnumerable<DenominationVoucher> denominationsoucher { get; set; }
        public IEnumerable<DenominationVoucher> couponsetting { get; set; }


    }

    public class CoupanVoucherEXl
    {


        public string CouponCode { get; set; }
        public int Amount { get; set; }

    }
    public class CoupanVoucherIndex
    {

        public int VoucherID { get; set; }
        public int APIID { get; set; }
        public int CouponCode { get; set; }
        public int Amount { get; set; }




    }

    public class CoupanVoucherEXlReq : CommonReq
    {

        public List<CoupanVoucherEXl> Couponvocher { get; set; }
        public int VocherID { get; set; }
        public int APIID { get; set; }

    }

    public class VoucherImage : AppResource
    {


    }


    public class CouponData : LoginReq
    {

        public int OID { get; set; }
        public string To { get; set; }
        public string From { get; set; }
        public string Message { get; set; }
        public decimal Amount { get; set; }
        public int Quantity { get; set; }



    }

    public class ImageCount_OID
    {
        public int OID { get; set; }
        public string Oname { get; set; }
        public int ImageCount { get; set; }
        public IEnumerable<CoupanMaster> CouponDetail { get; set; }

    }

    public class Master_Topup_Commission : CommonReq
    {

        public int ID { get; set; }
        public string TopUpName { get; set; }
        public string IsGreaterThan { get; set; }
        public string Comm { get; set; }
        public bool IsActive { get; set; }

        public string CommOnReg { get; set; }
        public string Ip { get; set; }
        public string Browser { get; set; }


    }
    public class MasterTopupCommissionViewModel
    {
        public List<Master_Topup_Commission> Master_Topup_Commission { get; set; }
        public ReferralCommission ReferralCommission { get; set; }
    }

    public class Master_Role : CommonReq
    {
        public int ID { get; set; }
        public string Role { get; set; }
        public string InPriority { get; set; }
        public string Priority { get; set; }
        public string EntryBy { get; set; }
        public string EntryDate { get; set; }
        public string ModifyBy { get; set; }
        public string ModifyDate { get; set; }
        public bool IsActive { get; set; }
        public string Remark { get; set; }
        public string IsEditable { get; set; }
        public string InFundRequestFrom { get; set; }
        public string InFundRequestTo { get; set; }
        public string Ind { get; set; }
        public string Prefix { get; set; }
        public string IsEndUser { get; set; }
        public string IsAPIUserCreator { get; set; }
        public string IsB2BSignup { get; set; }
        public string IsFlatComByAdmin { get; set; }
        public string RegCharge { get; set; }
        public string SignupAmount { get; set; }
        public string IsBankVerification { get; set; }
        public string Ip { get; set; }
        public string Browser { get; set; }



    }

    public class AdvertisementRequest: AdvertisementPackage
    {
        public int Id { get; set; }
        public int LT { get; set; }
        public int PackageId { get; set; }
        
        public int UserID { get; set; }
        public int Status { get; set; }
        public string Remark { get; set; }
        public string ContentText { get; set; }
        public string ContentImage { get; set; }
        public string EntryDate { get; set; }
        public string ModifyDate { get; set; }
        public int EntryBy { get; set; }
        public int ModifyBy { get; set; }
        public DateTime? ToDate { get; set; }
        public DateTime? FromDate { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Type { get; set; }
        public string CurrentStatus { get; set; }
        public string  Name { get; set; }
        public string MobileNo { get; set; }
        public string ReturnUrl { get; set; }


    }

    public class FileUploadAdvertisementRequest: AdvertisementRequest
    {
        public IFormFile File { get; set; }

    }

    public class AdvertisementPackage
    {
         public int ID { get; set; }
        public string PackageName { get; set; }
        public bool IsActive { get; set; }
        public string PackageCost { get; set; }
        public int PackageValidity { get; set; }


    }

}
