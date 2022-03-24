using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.Configuration;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.StaticModel;

namespace RoundpayFinTech.Controllers
{
    public partial class AdminController
    {
        [HttpGet]
        [Route("Theme")]
        public IActionResult Theme()
        {
            //if (!ApplicationSetting.IsWhitelabel)
            //    return Ok();
            IWebsiteML ml = new WebsiteML(_accessor, _env);
            var res = new TemplatesAndThemes
            {
                Themes = ml.GetThemes(),
                SiteTemplates = GetSiteTemplates(),
                isOnlyForRP = _accessor.HttpContext.Request.Host.ToString().Contains("Roundpay.net")
            };
            return View("Theme/Theme", res);
        }

        [HttpPost]
        [Route("ChangeTheme")]
        public IActionResult ChangeTheme(int ThemeId)
        {
            IWebsiteML ml = new WebsiteML(_accessor, _env);
            var response = ml.ChangeTheme(ThemeId);
            return Json(response);
        }

        [HttpPost]
        [Route("WLAllowTheme")]
        public IActionResult WLAllowTheme(int ThemeId, bool IsWLAllowed)
        {
            IWebsiteML ml = new WebsiteML(_accessor, _env);
            var response = ml.WLAllowTheme(ThemeId, IsWLAllowed);
            return Json(response);
        }

        public IActionResult ChangeColorSet(int ColorSetId, int ThemeId = 0)
        {
            var res = ColorSets.UpdateThemColor(ThemeId, ColorSetId, _lr.WID);
            var response = new ResponseStatus
            {
                Statuscode = res ? ErrorCodes.One : ErrorCodes.Minus1,
                Msg = res ? "Coler set applied successfully" : ErrorCodes.TempError
            };
            return Json(response);
        }

        [HttpPost]
        public IActionResult ChangeSiteTemplate(int TemplateId)
        {
            IWebsiteML ml = new WebsiteML(_accessor, _env);
            var response = ml.ChangeSiteTemplate(_lr.UserID, TemplateId, _lr.WID);
            return Json(response);
        }

        [HttpPost]
        public IActionResult WebsitecontentEditor(int contentId)
        {
            IWebsiteML ml = new WebsiteML(_accessor, _env);
            //var response = ml.ChangeSiteTemplate(_lr.UserID, TemplateId, _lr.WID);
            return PartialView("Theme/WebsitecontentEditor");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateWebsiteContentAsync(string section, string content)
        {
            IWebsiteML ml = new WebsiteML(_accessor, _env);
            var response = await ml.UpdateWebsiteContentAsync(new CommonReq { LoginID = _lr.UserID, CommonInt = _lr.WID, CommonStr = section, CommonStr2 = content }).ConfigureAwait(true);
            return Json(response);
        }

        [HttpPost]
        public async Task<IActionResult> GetWebsiteContectAsync()
        {
            IWebsiteML ml = new WebsiteML(_accessor, _env);
            var response = await ml.GetWebsiteContentAsync(_lr.WID).ConfigureAwait(true);
            return Json(response);
        }

        private IEnumerable<SiteTemplate> GetSiteTemplates()
        {
            var res = new List<SiteTemplate>();
            try
            {
                var path = DOCType.SiteconfigJsonFilePath;
                if (System.IO.File.Exists(path))
                {
                    var jsonData = System.IO.File.ReadAllText(path);
                    res = JsonConvert.DeserializeObject<List<SiteTemplate>>(jsonData);
                }
            }
            catch (Exception ex)
            {

            }
            return res ?? new List<SiteTemplate>();
        }
    }
}
