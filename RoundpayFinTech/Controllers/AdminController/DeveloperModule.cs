using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer.DepartmentMiddleLayer;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.DepartmentModel;
using RoundpayFinTech.Models;

namespace RoundpayFinTech.Controllers
{
    public partial class AdminController
    {
        #region DeveloperSetting

        [HttpGet]
        [Route("Home/9c34176d6b9710399c3cf3128acc6b24")]
        [Route("9c34176d6b9710399c3cf3128acc6b24")]
        public IActionResult MenuOps()
        {
            return View();
        }

        [HttpPost]
        [Route("Home/menu-op")]
        [Route("menu-op")]
        public async Task<IActionResult> _9c34176d6b9710399c3cf3128acc6b24(string txt)
        {
            IMenuOpsML ml = new DepartmentML(_accessor, _env);
            var lst = await ml.GetMenuOperations(txt);
            return PartialView("Partial/_MenuOps", lst);
        }
        [HttpGet]
        [Route("Home/9c34176d6b9710399c3cf3128acc6b24_")]
        [Route("9c34176d6b9710399c3cf3128acc6b24_")]
        public IActionResult MapMsgTamplateToKey()
        {
            return View();
        }

        [HttpPost]
        [Route("Home/Template-Keyword-Map")]
        [Route("Template-Keyword-Map")]
        public async Task<IActionResult> _9c34176d6b9710399c3cf3128acc6b24_(string txt)
        {
            IMenuOpsML ml = new DepartmentML(_accessor, _env);
            var lst = await ml.GetMapMsgTamplateToKey(txt);
            return PartialView("Partial/_TamplateKeywordsMapping", lst);
        }

        //UpdateMenuOperations
        [HttpPost]
        [Route("Home/umenu-op")]
        [Route("umenu-op")]
        public async Task<IActionResult> _U9c34176d6b9710399c3cf3128acc6b24(string txt, int mid, int oid, bool ia)
        {
            var mo = new MenuOperation
            {
                DevKey = txt,
                MenuID = mid,
                OperationID = oid,
                IsActive = ia
            };
            IMenuOpsML ml = new DepartmentML(_accessor, _env);
            var _res = await ml.UpdateMenuOperations(mo);
            return Json(_res);
        }

        #region ThemesAndColorSets
        [HttpGet]
        [Route("Home/ThemeWithColor/9c34176d6b9710399c3cf3128acc6b24")]
        [Route("ThemeWithColor/9c34176d6b9710399c3cf3128acc6b24")]
        public IActionResult ThemeWithColor()
        {
            return View();
        }

        [HttpPost]
        [Route("Home/ThemeWithColor")]
        [Route("ThemeWithColor")]
        public async Task<IActionResult> ThemeWithColor(string txt)
        {
            IMenuOpsML ml = new DepartmentML(_accessor, _env);
            var lst = await ml.GetMenuOperations(txt);
            if (lst != null && lst.Count() > 0)
            {
                var res = new List<ThemeWithColor>();
                var path = DOCType.ThemeWithColorJsonFilePath;
                if (System.IO.File.Exists(path))
                {
                    var jsonData = System.IO.File.ReadAllText(path);
                    res = JsonConvert.DeserializeObject<List<ThemeWithColor>>(jsonData);
                    if (res == null) res = new List<ThemeWithColor>();
                }

                return PartialView("Partial/_ThemeWithColor", res);
            }

            return Ok();
        }

        [HttpPost]
        [Route("Home/theme-colorsets")]
        [Route("theme-colorsets")]
        public IActionResult ThemesAndColorSets(string json)
        {
            IResponseStatus res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = "Changes Saved Successfully!"
            };

            try
            {
                var path = DOCType.ThemeWithColorJsonFilePath;
                System.IO.File.WriteAllText(path, string.IsNullOrEmpty(json) ? "[]" : json);
                res.Statuscode = ErrorCodes.One;
                res.Msg = "Changes Saved Successfully!";
            }
            catch (Exception ex)
            {
                res.Statuscode = ErrorCodes.Minus1;
                res.Msg = "Sorry! unable to save changes.";
            }

            return Json(res);
        }
        #endregion

        #endregion
    }
}
