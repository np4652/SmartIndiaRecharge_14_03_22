using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RoundpayFinTech.AppCode.Model.App;
using RoundpayFinTech.AppCode.Model.ProcModel;

namespace RoundpayFinTech.AppCode.Model
{
    public class PromoCode:AppResponse
    {
        public int ID { get; set; }
        public List<OpTypeMaster> OpTypes { get; set; }
        public int OpTypeID { get; set; }
        public List<OperatorDetail> Operator { get; set; }
        public int OID { get; set; }
        public string Promocode { get; set; }
        public string Extension { get; set; }
        public decimal CashBack { get; set; }
        public bool IsFixed { get; set; }
        public string Description { get; set; }
        public int CashbackMaxCycle { get; set; }
        public int CashBackCycleType { get; set; }
        public int AfterTransNumber { get; set; }
        public bool IsGift { get; set; }
        public string ValidFrom { get; set; }
        public string ValidTill { get; set; }
        public int LoginID { get; set; }
        public int LT { get; set; }
        public IFormFile PromoCodeImg { get; set; }
        public string ModifyDate { get; set; }
        public List<PromoCode> lstPromoCode { get; set; }
        public string OpType { get; set; }
        public string OperatorName { get; set; }
        public string CycleType { get; set; }
        public PromoCode promoCode { get; set; }
        public string OfferDetail { get; set; }
    }
   
}
