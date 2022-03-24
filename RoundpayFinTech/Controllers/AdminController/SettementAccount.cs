using Fintech.AppCode.Configuration;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OfficeOpenXml;
using RoundpayFinTech.AppCode;
using RoundpayFinTech.AppCode.HelperClass;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Report;
using RoundpayFinTech.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.Controllers
{
    public partial class AdminController
    {
        [HttpPost]
        [Route("Settlementaccount")]
        public IActionResult Settlementaccount()
        {
            if (_lr.LoginTypeID == LoginType.ApplicationUser)
            {
                if (_lr.RoleID.In(Role.Admin, Role.Retailor_Seller, Role.APIUser))
                {
                    IBankML bankML = new BankML(_accessor, _env);
                    var banks = bankML.BankMasters();
                    SattlementAccountModels res = new SattlementAccountModels();
                    if (banks.Any())
                    {
                        var banaklist = banks.Select(x => new
                        {
                            ID = x.ID + "~" + x.IFSC,
                            BankName = x.BankName
                        });
                        res.Bankselect = new SelectList(banaklist, "ID", "BankName");
                    }

                    return PartialView("Partial/_Settlementaccount", res);
                }
            }
            return BadRequest("Invalid Request");
        }
        [HttpPost]
        [Route("SettlementaccountList/{UserID}")]
        public IActionResult SettlementaccountList(int UserID)
        {
            if (_lr.LoginTypeID == LoginType.ApplicationUser)
            {
                if (_lr.RoleID.In(Role.Admin, Role.Retailor_Seller, Role.APIUser))
                {
                    if ((UserID != _lr.UserID && _lr.RoleID == Role.Admin) || (_lr.UserID == UserID && _lr.RoleID != Role.Admin))
                    {
                        ISettlementaccountML ml = new SettlementaccountML(_accessor, _env, false);
                        return PartialView("Partial/_SettlementaccountList", ml.GetSettlementaccountList(UserID, _lr.UserID));
                    }
                }
            }
            return BadRequest("Invalid Request");
        }
        [HttpPost]
        [Route("SettlementaccountEdit")]
        public IActionResult SettlementaccountEdit([FromBody] SattlementAccountModels req)
        {
            if (_lr.LoginTypeID == LoginType.ApplicationUser)
            {
                if (_lr.RoleID.In(Role.Admin, Role.Retailor_Seller, Role.APIUser))
                {
                    ISettlementaccountML ml = new SettlementaccountML(_accessor, _env);
                    req.EntryBy = _lr.UserID;
                    return Json(ml.UpdateSettlementaccount(req));
                }
            }
            return BadRequest("{}");
        }
        [HttpPost]
        [Route("Toggle-Settlement-IsDefault/{id}")]
        public IActionResult Sattlementaccountdefault(int id)
        {
            if (_lr.LoginTypeID == LoginType.ApplicationUser)
            {
                if (_lr.RoleID.In(Role.Retailor_Seller, Role.APIUser))
                {
                    ISettlementaccountML ml = new SettlementaccountML(_accessor, _env);
                    return Json(ml.SetDefaultSettlementaccount(new SattlementAccountModels
                    {
                        LoginID = _lr.UserID,
                        UserID = _lr.UserID,
                        ID = id
                    }));
                }
            }
            return BadRequest("{}");
        }
        [HttpPost]
        [Route("Toggle-Settlement-Delete/{id}")]
        public IActionResult SattlementaccountDelete(int id)
        {
            if (_lr.LoginTypeID == LoginType.ApplicationUser)
            {
                if (_lr.RoleID.In(Role.Retailor_Seller, Role.APIUser))
                {
                    SattlementAccountModels res = new SattlementAccountModels();
                    ISettlementaccountML ml = new SettlementaccountML(_accessor, _env);
                    return Json(ml.SetDeleteSettlementaccount(new Fintech.AppCode.Model.CommonReq
                    {
                        LoginTypeID = _lr.LoginTypeID,
                        LoginID = _lr.UserID,
                        UserID = _lr.UserID,
                        CommonInt = id
                    }));
                }
            }
            return BadRequest("{}");
        }
        [HttpPost]
        [Route("Update_SattlementAccountbyadmin")]
        public async Task<IActionResult> _RAUserpartialUpdate(char Status, int RequestID, string Name)
        {
            if (_lr.LoginTypeID == LoginType.ApplicationUser)
            {
                if (_lr.RoleID.In(Role.Admin))
                {
                    IResponseStatus _res = new ResponseStatus
                    {
                        Statuscode = ErrorCodes.Minus1,
                        Msg = ErrorCodes.InvalidParam

                    };
                    GetEditUser RequestData = new GetEditUser
                    {
                        RequestID = RequestID,
                        Name = Name,

                    };
                    if (Status == 'A' || Status == 'a')
                    {
                        RequestData.RequestStatus = "Approved";
                    }
                    if (Status == 'R' || Status == 'r')
                    {
                        RequestData.RequestStatus = "Rejected";
                    }

                    ISettlementaccountML ml = new SettlementaccountML(_accessor, _env);
                    _res = await ml.AcceptOrRejectBankupdateRequest(RequestData);
                    return Json(_res);
                }
            }
            return BadRequest("{}");
        }

        [HttpPost]
        [Route("verify-account-user")]
        public IActionResult VerifySettlementAccountOfUser(int id)
        {
            var ress = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser)
            {
                if (_lr.RoleID.In(Role.Retailor_Seller, Role.APIUser))
                {
                    ISettlementaccountML settlementaccountML = new SettlementaccountML(_accessor, _env);
                    var accRes = settlementaccountML.GeSettlementAccountByID(new Fintech.AppCode.Model.CommonReq
                    {
                        CommonInt = id,
                        LoginID = _lr.UserID
                    });
                    if (accRes.ID > 0)
                    {
                        IMoneyTransferML moneyTransferML = new MoneyTransferML(_accessor, _env);
                        var res = moneyTransferML.VerifyAccount(new AppCode.Model.MoneyTransfer.MBeneVerifyRequest
                        {
                            UserID = _lr.UserID,
                            OutletID = _lr.OutletID,
                            AccountNo = accRes.AccountNumber,
                            BankID = accRes.BankID,
                            IFSC = accRes.IFSC,
                            RequestMode = RequestMode.PANEL,
                            SenderMobile = _lr.MobileNo,
                            AccountTableID = accRes.ID ?? 0,
                            TransMode = "IMPS",
                            Bank = accRes.BankName,
                            ReferenceID = _lr.MobileNo
                        });
                        ress.Statuscode = res.Statuscode;
                        ress.Msg = res.Msg;
                    }
                }
            }
            return Json(ress);
        }

        [HttpPost]
        [Route("update-utr")]
        public IActionResult UpdateUTRByUser(int id, string utr)
        {
            IResponseStatus ress = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser)
            {
                if (_lr.RoleID.In(Role.Retailor_Seller, Role.APIUser))
                {
                    ISettlementaccountML settlementaccountML = new SettlementaccountML(_accessor, _env);
                    ress = settlementaccountML.UpdateUTRByUser(id, utr, _lr.UserID);
                }
            }
            return Json(ress);
        }
    }
}
