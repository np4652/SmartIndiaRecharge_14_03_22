using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.Model.Reports;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.App;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.Models;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;


namespace RoundpayFinTech.AppCode.Interfaces
{
    interface IPromoCodeML
    {
        IResponseStatus SavePromoCode(PromoCode PromocodeObj);
        List<PromoCode> GetPromoCodeDetail();
        PromoCode GetPromoCodeByID(int ID);
        IResponseStatus UploadPromoImage(int ID, IFormFile PromoImage);
        List<PromoCode> GetPromoCodeByOpTypeOID(PromoCode prc);
    }


}
