using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Fintech.AppCode;
using Fintech.AppCode.Configuration;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.Model.Reports;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using OfficeOpenXml;
using QRCoder;
using RoundpayFinTech.AppCode;
using RoundpayFinTech.AppCode.Configuration;
using RoundpayFinTech.AppCode.HelperClass;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.MiddleLayer.DepartmentMiddleLayer;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.DepartmentModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Report;
using RoundpayFinTech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.ThirdParty.WhatsappAPI;
using RoundpayFinTech.Models;
using Validators;

namespace RoundpayFinTech.Controllers
{
    public partial class AdminController
    {

        #region WhatsappSenderNo
        [HttpPost]
        [Route("Home/Save_Senderno")]
        [Route("Save_Senderno")]
        public IActionResult SaveSenderNo([FromBody] WhatsappAPIDetail wtnodetail)
        {
            IResponseStatus _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Incomplete request!"
            };
            if (_lr.LoginTypeID.In(LoginType.ApplicationUser, LoginType.CustomerCare))
            {
                if (wtnodetail != null)
                {
                    wtnodetail.LoginID = _lr.UserID;
                    wtnodetail.LT = _lr.LoginTypeID;
                    IWhatsappSenderNoML aPIML = new APIML(_accessor, _env);
                    _res = aPIML.SaveWtSenderNo(wtnodetail);
                }
            }
            return Json(_res);
        }
        [HttpPost]
        [Route("Home/WtSendernoStatus")]
        [Route("WtSendernoStatus")]
        public IActionResult ISWtSendernoStatus(int wtsenderid, int sapiid, bool IsActive)
        {
            IResponseStatus _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Incomplete request!"
            };
            if (_lr.LoginTypeID.In(LoginType.ApplicationUser, LoginType.CustomerCare))
            {
                var wtnodetail = new WhatsappAPIDetail
                {
                    LoginID = _lr.UserID,
                    LT = _lr.LoginTypeID,
                    APIID = sapiid,
                    IsActive = IsActive,
                    ID = wtsenderid
                };
                if (wtnodetail != null)
                {
                    IWhatsappSenderNoML aPIML = new APIML(_accessor, _env);
                    _res = aPIML.SaveWtSenderNo(wtnodetail);
                }
            }
            return Json(_res);
        }

        [HttpPost]
        [Route("Home/WtSenderno_Delete")]
        [Route("WtSenderno_Delete")]
        public IActionResult WtSendernoDelete(int wtsenderid)
        {

            IResponseStatus _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Incomplete request!"
            };
            if (_lr.LoginTypeID.In(LoginType.ApplicationUser, LoginType.CustomerCare))
            {
                if (wtsenderid != null)
                {
                    IWhatsappSenderNoML aPIML = new APIML(_accessor, _env);
                    _res = aPIML.DeleteWtSenderNo(wtsenderid);

                }
            }
            return Json(_res);
        }
        [HttpPost]
        [Route("Home/WhatsAppSenderNo/{id}")]
        [Route("WhatsAppSenderNo/{id}")]
        public IActionResult WhatsappSenderNoList(int id)
        {
            IWhatsappSenderNoML ML = new APIML(_accessor, _env);
            return PartialView("Partial/_WhatsappAPI", ML.GetWhatsappSenderNoList(id));
        }

        [HttpPost]
        [Route("Home/SaveWhatsappGroup")]
        [Route("SaveWhatsappGroup")]
        public async Task<IActionResult> SaveWhatsappGroup(string SenderNo,int id)
        {
            if (_lr.LoginTypeID.In(LoginType.ApplicationUser, LoginType.CustomerCare))
            {
                CommonReq req = new CommonReq();
                if (req != null)
                {
                    req.UserID = _lr.UserID;
                    req.LoginTypeID = _lr.LoginTypeID;
                    req.CommonInt = id;
                    req.CommonStr = SenderNo.Trim();
                    IWhatsappML aPIML = new WhatsappML(_accessor, _env);
                   var res=await aPIML.SaveWhatsappGroup(req);
                    return Json(res);
                }
            }
            return Json(1);
        }
        #endregion 
    }
}
