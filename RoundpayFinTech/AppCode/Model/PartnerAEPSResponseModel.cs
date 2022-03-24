using Microsoft.AspNetCore.Mvc.Rendering;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Model
{
    public class PartnerAEPSResponseModel
    {
        public int StatusCode { get; set; }
        public string Msg { get; set; }
        public int PartnerId { get; set; }
        public string LogoPath { get; set; }
        public string BannerPath { get; set; }
        public string BgPath { get; set; }
        public string CompanyName { get; set; }
        public string CompanyMobile { get; set; }
        public string CompanyEmail { get; set; }
        public string CompanyAddress { get; set; }
        public string SupportNumber { get; set; }
        public string SupportMail { get; set; }
        public SelectList BankList { get; set; }
        public List<BankMaster> BankDetails { get; set; }
    }
}
