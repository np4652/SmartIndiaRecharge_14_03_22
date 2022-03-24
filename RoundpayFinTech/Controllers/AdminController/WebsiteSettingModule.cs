using Fintech.AppCode.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoundpayFinTech.AppCode;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.StaticModel;

namespace RoundpayFinTech.Controllers
{
    public partial class AdminController
    {
        #region WebsiteSetting

        [HttpGet]
        [Route("Admin/WebsiteSetting")]
        [Route("Website-Setting")]
        public IActionResult WebsiteSetting()
        {
            return View("WebsiteSetting/WebsiteSetting");
        }

        [HttpPost]
        [Route("_WebsiteSetting")]
        public IActionResult _WebsiteSetting()
        {
            IWebsiteML ml = new WebsiteML(_accessor, _env);
            var websiteSettingModel = new WebsiteSettingModel
            {
                websiteInfo = ml.GetWebsiteInfo()
            };
            if (websiteSettingModel.websiteInfo.ThemeId == 4)
            {
                IBannerML bannerML = new ResourceML(_accessor, _env);
                websiteSettingModel.BGServiceImgURLs = bannerML.SiteGetServices(websiteSettingModel.websiteInfo.WID, websiteSettingModel.websiteInfo.ThemeId);
            }
            return PartialView("WebsiteSetting/Partial/_WebsiteSetting", websiteSettingModel);
        }

        [HttpPost]
        [Route("Admin/upload-logo")]
        [Route("upload-logo")]
        public IActionResult UploadLogo(IFormFile file)
        {
            IBannerML _bannerML = new ResourceML(_accessor, _env);
            var _res = _bannerML.UploadImages(file, _lr, FolderType.Website, FolderType.Logo);
            return Json(_res);
        }

        [HttpPost]
        [Route("Admin/upload-whitelogo")]
        [Route("upload-whitelogo")]
        public IActionResult UploadWhiteLogo(IFormFile file)
        {
            IBannerML _bannerML = new ResourceML(_accessor, _env);
            var _res = _bannerML.UploadImages(file, _lr, FolderType.Website, FolderType.whiteLogo);
            return Json(_res);
        }
        [HttpPost]
        [Route("Admin/upload-b2clogo")]
        [Route("upload-b2clogo")]
        public IActionResult UploadB2CLogo(IFormFile file)
        {
            IBannerML _bannerML = new ResourceML(_accessor, _env);
            var _res = _bannerML.UploadImages(file, _lr, FolderType.Website, FolderType.b2cLogo);
            return Json(_res);
        }
        [HttpPost]
        [Route("Admin/upload-bgimage")]
        [Route("upload-bgimage")]
        public IActionResult UploadBgImage(IFormFile file)
        {
            IBannerML _bannerML = new ResourceML(_accessor, _env);
            var _res = _bannerML.UploadImages(file, _lr, FolderType.Website, FolderType.BgImage);
            return Json(_res);
        }

        [HttpPost]
        [Route("Admin/upload-serviceimage")]
        [Route("upload-serviceimage")]
        public IActionResult UploadServicesImage(IFormFile file)
        {
            IBannerML _bannerML = new ResourceML(_accessor, _env);
            var _res = _bannerML.UploadImages(file, _lr, FolderType.Website, FolderType.ServiceImage);
            return Json(_res);
        }
        [HttpPost]
        [Route("Admin/upload-serviceimages")]
        [Route("upload-serviceimages")]
        public IActionResult UploadServicesImages(IFormFile file, string n)
        {
            IBannerML _bannerML = new ResourceML(_accessor, _env);
            var _res = _bannerML.UploadImages(file, _lr, FolderType.Website, FolderType.ServiceImage, n);
            return Json(_res);
        }

        [HttpPost]
        [Route("Admin/removeServiceImg")]
        [Route("removeServiceImg")]
        public IActionResult RemoveImg(string n)
        {
            IBannerML _bannerML = new ResourceML(_accessor, _env);
            var _res = _bannerML.RemoveImage(_lr, FolderType.Website, FolderType.ServiceImage, n);
            return Json(_res);
        }
        [HttpPost]
        [Route("upload-sign")]
        public IActionResult Uploadsing(IFormFile file)
        {
            IBannerML _bannerML = new ResourceML(_accessor, _env);
            var _res = _bannerML.UploadImages(file, _lr, FolderType.Website, FolderType.Sign);
            return Json(_res);
        }

        [HttpPost]
        [Route("upload-CertificateFooterImage")]
        public IActionResult CertificateFooterImage(IFormFile file)
        {
            IBannerML _bannerML = new ResourceML(_accessor, _env);
            var _res = _bannerML.UploadImages(file, _lr, FolderType.Website, FolderType.CertificateFooterImage);
            return Json(_res);
        }

        [HttpGet]
        [Route("Admin/GetCompanyProfile")]
        [Route("CompanyProfile")]
        public IActionResult GetCompanyProfile()
        {
            IUserWebsite _userWebsite = new UserML(_accessor, _env);
            var model = _userWebsite.GetCompanyProfileUser(_lr.UserID);
            return View("WebsiteSetting/GetCompanyProfile", model);
        }

        [HttpPost]
        [Route("Admin/UpdateCompanyProfile")]
        [Route("UpdateCompanyProfile")]
        public IActionResult UpdateCompanyProfile([FromBody] CompanyProfileDetailReq req)
        {

            //var WebInfo = loginML.GetWebsiteInfo();
            req.WID = _lr.WID;
            IUserWebsite _userWebsite = new UserML(_accessor, _env);
            var _res = _userWebsite.CompanyProfileCU(req);
            return Json(_res);
        }

        #endregion
    }
}
