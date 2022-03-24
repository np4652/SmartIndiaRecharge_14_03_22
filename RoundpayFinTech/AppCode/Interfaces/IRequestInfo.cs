using Fintech.AppCode.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.Models;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fintech.AppCode.Interfaces
{
    public interface IRequestInfo
    {
        string GetRemoteIP();
        string GetLocalIP();
        string GetBrowser();
        string GetBrowserVersion();
        string GetUserAgent();
        string GetBrowserFullInfo();
        string GetDomain(IConfiguration Configuration);
        CurrentRequestInfo GetCurrentReqInfo();
        string GetUserAgentMD5();

    }
    public interface IResourceML
    {
        ResponseStatus UploadBankBanner(IFormFile file, int OID);
        IResponseStatus UploadEmployeeGift(IFormFile file, string filename);
        ResponseStatus UploadOperatorIcon(IFormFile file, int OID);
        ResponseStatus UploadWebNotificationImage(IFormFile file, LoginResponse _lr, string ImageName);
        ResponseStatus UploadProfile(IFormFile file, int UserID, int LoginTypeID);
        IResponseStatus UploadProductImage(IFormFile file, LoginResponse _lr, int ProductID, int ProductDetailID, string ImgName, int count);

        StringBuilder GetLogoURL(int WID);
        void CreateWebsiteDirectory(int WID, string _FolderType);
        IResponseStatus UpLoadApk(IFormFile file, string FileName, bool IsTest, LoginResponse _lr);
        CertificateModel downloadPdf(int LT, int UserID, int WID);
        IResponseStatus uploadAfItem(IFormFile file, int VendorID, LoginResponse _lr);
        ResponseStatus UploadChannelImage(IFormFile file, LoginResponse _lr, int ChannelID);
        IEnumerable<Video> Getvideolink(LoginResponse _lr);
        IResponseStatus SaveVideo(LoginResponse _lr, int ID, string URL, string Title);
        IResponseStatus RemoveVideo(LoginResponse _lr, int ID);
        IEnumerable<Video> GetvideolinkApp(CommonReq req);
        ResponseStatus UploadCouponVoucher(IFormFile file, int OID, string Filename);
        IResponseStatus RemoveCouponVoucher(string OID, string FileName, LoginResponse _lr);
        IrctcCertificateModel downloadPdfirctc(int LT, int UserID, int WID);
        IrctcCertificateModel GetOutletData(int LT, int UserID);
        IResponseStatus UploadAdvertisementImage(IFormFile file, int LT, int UserID, string imageName);
    }
    public interface IBannerML
    {
        IEnumerable<BannerImage> GetSavedRefferalImage(string WID);
        string GetSavedAppImage(int WID);
        IResponseStatus RefferalImageRemove(string FileName, string WID, LoginResponse _lr);
        IResponseStatus SavedRefferalImage(IFormFile file, string WID, int OpType, LoginResponse _lr);
        IResponseStatus SaveHotelImage(IFormFile file,  LoginResponse _lr);
        IResponseStatus LoaderBannerUpload(IFormFile file,  LoginResponse _lr);

        IEnumerable<BannerImage> GetOperatorBanner(string WID, int Name);
        IResponseStatus UploadDTHLeadOperator(IFormFile file, string WID, int OpType, LoginResponse _lr);
        IResponseStatus RemoveOperatorBanners(string FileName, int Name, string WID, LoginResponse _lr);
        ResponseStatus UploadEmployeeImage(IFormFile file, LoginResponse _lr, string Mobile, string ImageType);
        IResponseStatus uploadTinyMCEImage(IFormFile file, int WID);
        IEnumerable<BannerImage> GetBanners(string WID);
        IResponseStatus UploadBanners(IFormFile file, string WID,string url, LoginResponse _lr);
        IResponseStatus UploadB2CBanners(IFormFile file, string WID, int OpType, LoginResponse _lr);
        IEnumerable<BannerImage> GetB2CBanners(string WID, int OpType);
        IResponseStatus RemoveB2CBanners(string FileName, int opType, string WID, LoginResponse _lr);
        IResponseStatus RemoveBanners(string FileName, string WID, LoginResponse _lr);
        IResponseStatus UploadImages(IFormFile file, LoginResponse _lr, string FolderType, string ImageType, string ImgName = "");
        IResponseStatus SiteUploadBanners(IFormFile file, string WID, LoginResponse _lr);
        IResponseStatus SiteRemoveBanners(string FileName, string WID, LoginResponse _lr);
        IResponseStatus RemHotelLoaderBanner(string FileName, string WID, LoginResponse _lr);
        IEnumerable<BannerImage> SiteGetBanners(string WID);
        IEnumerable<BannerImage> GetHotelLoaderBanner(string WID);
        IResponseStatus UploadPopup(IFormFile file, string WID, LoginResponse _lr, string folderType, string ImageType);
        IResponseStatus RemovePopup(string FileName, string WID, LoginResponse _lr);
        IResponseStatus UpLoadQR(IFormFile file, string BankID, LoginResponse _lr);
        IResponseStatus UploadReceipt(IFormFile file, int ID, LoginResponse _lr);
        IResponseStatus UploadPartnerImages(IFormFile file, string WID, LoginResponse _lr, string ImageType);
        IResponseStatus UploadPESDocument(List<IFormFile> files, int WID, LoginResponse _lr, int TID);
        IEnumerable<BannerImage> GetPopup(string WID);
        string GetAfterLoginPoup(string WID);
        IEnumerable<BannerImage> SiteGetServices(int WID, int ThemeID);
        IResponseStatus RemoveImage(LoginResponse _lr, string folderType, string ImageType, string ImgName);
    }
}
