using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Mvc;
using RoundpayFinTech.AppCode;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;

namespace RoundpayFinTech.Controllers
{
    public partial class AdminController
    {
        #region KYCDocSetting

        [HttpPost]
        [Route("Home/DocTypeMaster")]
        [Route("DocTypeMaster")]
        public IActionResult _DocTypeMaster(bool f)
        {
            IKYCML userML = new UserML(_accessor, _env);
            var list = userML.GetDocTypeMaster(f);
            ViewBag.IsOutlet = f;
            return PartialView("Partial/_DocTypeMasterDetail", list);
        }

        [HttpPost]
        [Route("Home/UpdateDocTypeMaster")]
        [Route("UpdateDocTypeMaster")]
        public IActionResult _UpdateDocTypeMaster([FromBody] DocTypeMaster tx)
        {
            IKYCML userML = new UserML(_accessor, _env);
            var _res = userML.UpdateDocTypeMaster(tx);
            return Json(_res);
        }

        #endregion

        #region KYCApproval
        [HttpGet]
        [Route("KYCUsers")]
        public IActionResult KYCUsers()
        {
            return View("UserList/KYCUsers");
        }
        [HttpPost]
        [Route("KYC-Users")]
        public async Task<IActionResult> _KYCUsers(UserRequest req)
        {
            IKYCML userML = new UserML(_accessor, _env);
            var res = await userML.GetKYCUsers(req);
            return PartialView("UserList/Partial/_KYCUsers", res);
        }
        [HttpPost]
        [Route("GetKYCStatus")]
        public IActionResult GetKYCStatus(int uid)
        {
            var operation = new UserML(_accessor, _env);
            var _res = operation.GetKYCStatus(uid);
            return Json(_res);
        }
        [HttpPost]
        [Route("Home/ApproveDocument")]
        [Route("ApproveDocument")]
        public IActionResult _DocumentDetails(int UserID, int outletid)
        {
            IKYCML userML = new UserML(_accessor, _env);
            var list = userML.GetDocumentsForApproval(UserID, outletid);
            return PartialView("UserList/Partial/_ApproveDocuments", list);
        }
        [HttpGet]
        [Route("DownloadKYC/{id}")]
        public IActionResult DownloadKYC(int id)
        {
            var userML = new UserML(_accessor, _env);
            IResponseStatus res = userML.DownloadKYC(id);
            if (res.Statuscode == ErrorCodes.One)
            {
                return RedirectToAction("Download", new { filePath = DOCType.DocFilePath + res.CommonStr, FileName = res.CommonStr2 });
            }
            return Json(res);
        }
        [Route("Download")]
        public async Task<IActionResult> Download(string filePath, string FileName)
        {
            try
            {
                if (!System.IO.File.Exists(filePath))
                {
                    return Content("filename not found");
                }
                var memory = new MemoryStream();
                using (var stream = new FileStream(filePath, FileMode.Open))
                {
                    await stream.CopyToAsync(memory);
                }
                memory.Position = 0;
                return File(memory, GetContentType(filePath), FileName + Path.GetExtension(filePath));
            }
            catch (Exception er)
            {
                return View();
            }
        }
        private string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = System.IO.Path.GetExtension(path).ToLowerInvariant();
            return types[ext];
        }
        private Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
            {
                {".txt", "text/plain"},
                {".pdf", "application/pdf"},
                {".doc", "application/vnd.ms-word"},
                {".docx", "application/vnd.ms-word"},
                {".xls", "application/vnd.ms-excel"},
                {".xlsx", "application/vnd.openxmlformatsofficedocument.spreadsheetml.sheet"},
                {".png", "image/png"},
                {".jpg", "image/jpeg"},
                {".jpeg", "image/jpeg"},
                {".gif", "image/gif"},
                {".csv", "text/csv"}
            };
        }
        [HttpPost]
        [Route("ApproveKYC")]
        public IActionResult _ApproveKYC(int d, int s, string r)
        {
            var operation = new UserML(_accessor, _env);
            var _res = operation.ApproveKYCDoc(d, s, r);
            return Json(_res);
        }

        #endregion

        #region KYCDetails
        [HttpPost]
        [Route("KYCDetails")]
        public IActionResult KYCDetails(int UserID)
        {
            IKYCML userML = new UserML(_accessor, _env);
            var list = userML.UsersKYCDetails(UserID);
            return PartialView("UserList/Partial/_UsersKYCDetails", list);
        }

        #endregion
    }
}
