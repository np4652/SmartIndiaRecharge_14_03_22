using RoundpayFinTech.Models;
using System.Collections.Generic;
namespace RoundpayFinTech.AppCode.Model
{
    public class WebsiteInfo
    {
        public string AppPackageID { get; set; }
        public int ReffralID { get; set; }
        public int WID { get; set; }
        public int ThemeId { get; set; }
        public int SiteId { get; set; }
        public int WUserID { get; set; }
        public string WebsiteName { get; set; }
        public string RedirectDomain { get; set; }
        public List<PageMaster> PageMasters { get; set; }
        public bool IsMultipleMobileAllowed { get; set; }
        public string AbsoluteHost { get; set; }
        public string MainDomain { get; set; }
        public string ShoppingDomain { get; set; }
    }
    public class WebsiteSettingModel
    {
        public IEnumerable<BannerImage> BGServiceImgURLs { get; set; }
        public WebsiteInfo websiteInfo { get; set; }
    }
    public class PageMaster
    {
        public int ID { get; set; }
        public string PageName { get; set; }
    }
    public class GetApiDocument
    {
        public string Name { get; set; }
        public string MobileNo { get; set; }
        public string EmailTechnical { get; set; }
        public string MobileTechnical { get; set; }
        public string EmployeeID { get; set; }
        public string UserMobile { get; set; }
        public int UserID { get; set; }
        public string SubDomain { get; set; }
        public string StatusCheckDoamin { get; set; }

    }
}
