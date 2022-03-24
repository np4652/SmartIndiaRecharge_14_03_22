using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface IWebsiteML
    {
        ResponseStatus UpdateRefferalContent(int WID, string appPackage);
        string GetAppWebsiteContent(int WID);
        HomeDisplay Template(int ThemeID);
        IResponseStatus UpdateDisplayHtml(HomeDisplayRequest req);
        WebsiteInfo GetWebsiteInfo(int WID=0);
        IEnumerable<Theme> GetThemes();
        ResponseStatus ChangeTheme(int ThemeId);
        ResponseStatus ChangeSiteTemplate(int LoginId, int TemplateId, int WID);
        IEnumerable<WebsiteModel> GetWebsite();
        IResponseStatus UpdateWebsiteList(WebsiteModel Web);
        ResponseStatus UpdateAppPackageID(int WID, string appPackage);
        IResponseStatus UpdateIsWLAPIAllowed(WebsiteModel Web);
        ResponseStatus WLAllowTheme(int ThemeId, bool IsWLAllowed);
        GetApiDocument GetAPIDocument();
        Task<IResponseStatus> UpdateWebsiteContentAsync(CommonReq req);
        Task<SiteTemplateSection> GetWebsiteContentAsync(int wId);
        Task<List<websiteBanks>> GetWebsiteBankDetails();
    }
}
