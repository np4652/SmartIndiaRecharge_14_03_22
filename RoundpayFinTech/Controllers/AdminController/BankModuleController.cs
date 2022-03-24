using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using GoogleAuthenticatorService.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using OfficeOpenXml;
using RoundpayFinTech.AppCode;
using RoundpayFinTech.AppCode.HelperClass;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.Models;

namespace RoundpayFinTech.Controllers
{
    public partial class AdminController
    {
        #region BankMasterRegion

        [HttpGet]
        [Route("Home/BankMasterAdmin")]
        [Route("BankMasterAdmin")]
        public IActionResult BankMasterAdmin()
        {
            return View();
        }

        [HttpPost]
        [Route("Home/Bank-Master-Admin")]
        [Route("Bank-Master-Admin")]
        public IActionResult _BankMasterAdmin(int TopRows, string KeyWords)
        {
            IBankML opml = new BankML(_accessor, _env);
            CommonReq _req = new CommonReq
            {
                CommonInt = TopRows,
                CommonStr = KeyWords,
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID
            };
            IEnumerable<BankMaster> list = opml.GetBankMasterAdmin(_req);
            return PartialView("Partial/_BankMasterAdmin", list);
        }

        [HttpPost]
        [Route("Admin/BankStatusUpdate")]
        [Route("BankStatusUpdate")]
        public IActionResult BankStatusUpdate(int ID, int StatusColumn)
        {
            IBankML userML = new BankML(_accessor, _env);
            IResponseStatus _res = userML.UpdatebankSetting(ID, StatusColumn);
            return Json(_res);
        }


        [HttpPost]
        [Route("Home/UpdateBankMasterAdmin")]
        [Route("UpdateBankMasterAdmin")]
        public IActionResult AcountLimit(int ID, int AccountLimit, string IFSC, string Code, int IIN)
        {
            IResponseStatus _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidParam
            };
            if (ID == 0)
                return Json(_resp);
            BankMaster BankMaster = new BankMaster
            {
                ID = ID,
                AccountLimit = AccountLimit,
                IFSC = IFSC,
                Code = Code,
                IIN = IIN
            };
            IBankML opml = new BankML(_accessor, _env);
            _resp = opml.UpdateBank(BankMaster);
            return Json(_resp);
        }

        public async Task<IActionResult> GetAadharIIN(string apiType)
        {
            IResponseStatus response = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidParam
            };
            if (string.IsNullOrEmpty(apiType))
            {
                response.Msg = "Please select API type";
                goto Finish;
            }
            using (var client = new HttpClient())
            {
                string apiURL = string.Empty;
                if (apiType.Equals("Aadharpay", StringComparison.OrdinalIgnoreCase))
                    apiURL = "https://fingpayap.tapits.in/fpaepsservice/api/bankdata/bank/aadharpay";
                if (apiType.Equals("AEPS", StringComparison.OrdinalIgnoreCase))
                    apiURL = "https://fingpayap.tapits.in/fpaepsservice/api/bankdata/bank/details";
                if (string.IsNullOrEmpty(apiURL))
                {
                    response.Msg = "Empty URL";
                    goto Finish;
                }
                var httpResponse = await client.GetAsync(apiURL).ConfigureAwait(true);
                if (!httpResponse.IsSuccessStatusCode)
                {
                    response.Msg = httpResponse.ReasonPhrase;
                    goto Finish;
                }
                var content = httpResponse.Content.ReadAsStringAsync().Result;
                if (string.IsNullOrEmpty(content))
                {
                    response.Msg = "Recieved empty result from API";
                    goto Finish;
                }
                var result = JsonConvert.DeserializeObject<BankIINResponse>(content);
                if (result == null || result.data.Count < 1)
                {
                    response.Msg = "Deserialization Failed";
                    goto Finish;
                }
                var bankWithIIN = result.data.Select(x => x.bankName + "_" + x.iinno).ToList();
                var finalString = string.Join(",", bankWithIIN);
                IBankML mL = new BankML(_accessor, _env);
                response = mL.UpdateBankIIN(new CommonReq { CommonStr = finalString, CommonStr2 = apiType });
            }
        Finish:
            return Json(response);
        }

        [HttpPost]
        [Route("Home/remove-bank")]
        [Route("remove-bank")]
        public IActionResult RemoveBank(int ID)
        {
            BankML operation = new BankML(_accessor, _env);
            IResponseStatus _res = operation.Delete(ID); ;
            return Json(_res);
        }

        [HttpPost]
        [Route("Home/add-bank")]
        [Route("add-bank")]
        public IActionResult AddBank([FromBody] Bankreq bank)
        {
            IBankML operation = new BankML(_accessor, _env);
            return Json(operation.SaveBank(bank));
        }

        [HttpGet]
        [Route("Home/bank-master")]
        [Route("bank-master")]
        public IActionResult BankMaster()
        {
            return View("BankMaster/BankMaster");
        }

        [HttpPost]
        [Route("Home/bm-cu")]
        [Route("bm-cu")]
        public IActionResult _BankMasterCU(int ID)
        {
            IBankML operation = new BankML(_accessor, _env);
            Bank bankMaster = new Bank();
            if (ID > 0)
            {
                bankMaster = operation.GetBank(ID);
            }
            bankMaster.BankMasters = operation.BankMasters();
            bankMaster.Mode = operation.GetAllPaymentMode().ToList();
            return PartialView("BankMaster/Partial/_BankMasterCU", bankMaster);
        }

        [HttpPost]
        [Route("Admin/bm-lst")]
        [Route("bm-lst")]
        public IActionResult _BankMaster()
        {
            IBankML bankML = new BankML(_accessor, _env);
            //return View("BankMaster/BankMaster", bankML.BankMasters());
            return PartialView("BankMaster/Partial/_BankMaster", bankML.Banks(_lr.UserID));

        }

        [HttpPost]
        [Route("upload-QRCode")]
        public IActionResult UploadQRCode(IFormFile file, string FileName, int PreStatusofQR)
        {
            IBannerML bannerML = new ResourceML(_accessor, _env);
            if (PreStatusofQR == 0)
            {
                FileName = FileName + "_" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss").Replace(":", "").Replace("-", "").Replace(" ", "");
                var _res = bannerML.UpLoadQR(file, FileName, _lr);
                return Json(_res);
            }
            return Ok();
        }
        #endregion

        #region MoveToBank
        [HttpPost]
        [Route("Home/B-T")]
        [Route("B-T")]
        public IActionResult _BankTransfer()
        {
            return PartialView("Partial/_BankTransfer");
        }
        [HttpPost]
        [Route("Home/BT")]
        [Route("BT")]
        public IActionResult BankTransfer([FromBody] WalletRequest req)
        {
            IResponseStatus _res = new ResponseStatus()
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidParam
            };
            if (req != null)
            {
                IFundProcessML userML = new UserML(_accessor, _env);
                _res = userML.BankTransfer(req);
            }
            return Json(_res);
        }

        #endregion

        #region Upload UTR Excel or Bank reconciliation
        [Route("/bankreconciliation")]
        public IActionResult Bankreconciliation()
        {
            return View();
        }

        [HttpPost, Route("_UTRExcelList")]
        public async Task<IActionResult> UTRExcelAsync()
        {
            IAdminML ml = new UserML(_accessor, _env);
            var response = await ml.GetUTRExcelAsync();
            return PartialView("Partial/_UTRExcelList", response);
        }

        [HttpPost, Route("UTRExcel-OTP")]
        public IActionResult UTRExcel_OTP()
        {
            IResponseStatus response = new ResponseStatus()
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            if (_lr.IsGoogle2FAEnable)
            {
                response = new ResponseStatus
                {
                    Statuscode = ErrorCodes.One,
                    Msg = "Please Enter Google PIN"
                };
            }
            else
            {
                IAdminML ml = new UserML(_accessor, _env);
                response = ml.SendUTROTP(_lr.UserID);
            }

            return Json(response);
        }

        [HttpPost, Route("upload-UTRExcel")]
        public async Task<IActionResult> UploadUTRExcelAsync(IFormFile file, string OTP)
        {
            IResponseStatus res = new ResponseStatus();
            if (_lr.IsGoogle2FAEnable)
            {
                TwoFactorAuthenticator Authenticator = new TwoFactorAuthenticator();
                if (!Authenticator.ValidateTwoFactorPIN(_lr.AccountSecretKey, OTP))
                {
                    res = new ResponseStatus
                    {
                        Statuscode = -1,
                        Msg = "Invalid Google PIN"
                    };
                    return Json(res);
                }
            }
            else
            {
                var sessionOTP = _session.GetString(SessionKeys.CommonOTP);
                if (string.IsNullOrEmpty(OTP) || OTP != sessionOTP)
                {
                    res = new ResponseStatus
                    {
                        Statuscode = -1,
                        Msg = "Invalid OTP"
                    };
                    return Json(res);
                }
            }

            IAdminML ml = new UserML(_accessor, _env);

            if (file == null || file.Length <= 0)
            {
                res.Statuscode = -1;
                res.Msg = "No file found.";
                return Json(res);
            }
            if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                res.Statuscode = -1;
                res.Msg = "Uploaded file is not valid.Please upload .xlsx file only.";
                return Json(res);
            }
            List<UTRExcel> list = new List<UTRExcel>();
            try
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                        DataTable dt = worksheet.ToDataTable();
                        DataColumnCollection columns = dt.Columns;
                        string userIdentity = string.Empty;
                        string utr = string.Empty;
                        string amount = "Amount";
                        string virtualAccount = string.Empty;
                        string customerCode = string.Empty;
                        string type = string.Empty;
                        string status = "SUCCESS";
                        if (columns.Contains("merchantTranId"))
                        {
                            userIdentity = "merchantTranId";
                            utr = "bankRRN";
                            virtualAccount = "merchantTranId";
                        }
                        if (columns.Contains("Card Number"))
                        {
                            userIdentity = "Card Number";
                            type = "IsAxis";
                            virtualAccount = "Card Number";
                        }
                        if (columns.Contains("Unique Reference No"))
                            utr = "Unique Reference No";
                        if (columns.Contains("Company Code"))
                            customerCode = "Company Code";
                        foreach (DataRow dr in dt.Rows)
                        {
                            if (columns.Contains("status"))
                                status = Convert.ToString(dr["status"]);
                            if (status == "SUCCESS")
                            {
                                var _virtualAccount = !string.IsNullOrEmpty(virtualAccount) ? Convert.ToString(dr[virtualAccount]) : string.Empty;
                                if (_virtualAccount.Contains("TIDQR") && !_virtualAccount.StartsWith("TIDQR"))
                                {
                                    _virtualAccount = _virtualAccount.Substring(0, _virtualAccount.IndexOf("TIDQR"));
                                    type = "IsQRICICI";
                                }
                                var record = new UTRExcel
                                {
                                    UserIdentity = !string.IsNullOrEmpty(userIdentity) ? Convert.ToString(dr[userIdentity]) : string.Empty,
                                    UTR = !string.IsNullOrEmpty(utr) ? Convert.ToString(dr[utr]) : string.Empty,
                                    Amount = !string.IsNullOrEmpty(amount) ? Convert.ToDecimal(dr[amount]) : 0,
                                    VirtualAccount = _virtualAccount.Replace("TID", ""),
                                    CustomerAccountNumber = !string.IsNullOrEmpty(virtualAccount) ? Convert.ToString(dr[virtualAccount]) : string.Empty,
                                    CustomerCode = !string.IsNullOrEmpty(customerCode) ? Convert.ToString(dr[customerCode]) : string.Empty,
                                    Type = _virtualAccount.Contains("TID") ? string.Empty : type,
                                    ProcName = _virtualAccount.Contains("TID") ? "proc_UpdatePaytmTransaction" : "proc_ICICIRequest"
                                };
                                list.Add(record);
                            }
                        }
                    }
                }
                res = await ml.UploadUTRExcelAsync(list).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                res.Statuscode = ErrorCodes.Minus1;
                res.Msg = ErrorCodes.AnError;
            }
            return Json(res);
        }

        [Route("Download-UTRDetailExcel")]
        public async Task<IActionResult> DownloadUTRDetailExcel(int FileId)
        {
            IAdminML ml = new UserML(_accessor, _env);
            var response = await ml.GetUTRDetailExcelAsync(FileId);
            DataTable dataTable = ConverterHelper.O.ToDataTable(response.ToList());
            var removableCol = new string[] { "FileId", "Id" };
            var Contents = EportExcel.o.GetFile(dataTable, removableCol);
            return File(Contents, DOCType.XlsxContentType, "UTRDetail.xlsx");
        }
        #endregion

        [HttpGet]
        [Route("UtrStatement")]
        public IActionResult UtrStatement()
        {
           


            return View();
        }

        [HttpPost]
        [Route("UtrStatementInsert")]
        public IActionResult _UtrStatement()
        {
            IBankML bankML = new BankML(_accessor, _env);
            var banks = bankML.BankMaster(0);
            SattlementAccountModels res = new SattlementAccountModels();
          
            try
            {

                if (banks.Any())
                {
                    var banaklist = banks.Select(x => new
                    {
                        ID = x.ID,
                        BankName = x.BankName+"~"+x.AccountNo
                    });
                    res.Bankselect = new SelectList(banaklist, "ID", "BankName");
                }
                var path = DOCType.UTRStatementJsonFilePath;
               
                if (!System.IO.File.Exists(path))
                {
                    using (System.IO.File.Create(path)) ;
                   
                }
               
                    var jsonData = System.IO.File.ReadAllText(path);
                    ViewBag.jsonData = jsonData==""?"[]": jsonData;
                 // res = JsonConvert.DeserializeObject<List<UtrStatementSetting>>(jsonData);
                
            }
            catch (Exception ex)
            {
                
            }
            return PartialView("Partial/_UtrStatementInsert", res);
        }

        [HttpPost, Route("/UploadUtrStatement/{accountNo}/{bankID}")]
        public async Task<IActionResult> UploadUtrStatement(string AccountNo, int BankID, [FromBody] List<UtrStatementUpload> data)
        {
            IBankML ml = new BankML(_accessor, _env);
            var response = await ml.UploadUTRListAsync(AccountNo, BankID, data);
            return Json(response);
        }


        [HttpPost]
        [Route("UtrStatementSetting")]
        public IActionResult _UtrStatementSetting()
        {
            IBankML bankML = new BankML(_accessor, _env);
            var banks = bankML.BankMasters();
            SattlementAccountModels res = new SattlementAccountModels();
           
            try
            {

                if (banks.Any())
                {
                    var banaklist = banks.Select(x => new
                    {
                        ID = x.ID,
                        BankName = x.BankName
                    });
                    res.Bankselect = new SelectList(banaklist, "ID", "BankName");
                }
                
            }
            catch (Exception ex)
            {

            }

           return PartialView("Partial/_UtrStatementSetting",res);
        }
        [HttpPost]
        [Route("UtrFilterList/{ID}")]
        public IActionResult UtrFilterList(string ID)
        {
           
            var res = new List<UtrStatementSetting>();
            try
            {
                var path = DOCType.UTRStatementJsonFilePath;
                if (System.IO.File.Exists(path))
                {
                   var jsonData = System.IO.File.ReadAllText(path);
                    ViewBag.jsonData = jsonData; 
                   res = JsonConvert.DeserializeObject<List<UtrStatementSetting>>(jsonData).Where(x=>x.bankID==ID).ToList();

                }
            }
            catch (Exception ex)
            {

            }

            return Json(res);
        }

        [HttpPost]
        [Route("UpdateInsertUtrFilter")]
        public IActionResult UpdateInsertUtrFilter([FromBody] UtrStatementSetting data)
        {
            bool res = false;
            if ( string.IsNullOrEmpty(data.startWith)|| string.IsNullOrEmpty(data.endWith) || string.IsNullOrEmpty(data.startWith)  || string.IsNullOrEmpty(data.identifier) || data.bankID=="0" || data.transactionType=="0")
            {
                ResponseStatus response1 = new ResponseStatus
                {
                    Statuscode = res ? ErrorCodes.One : ErrorCodes.Minus1,
                    Msg =  ErrorCodes.InvalidParam
                };

                return Json(response1);

            }
            else
            {

                IBankML bankML = new BankML(_accessor, _env);
                 res = bankML.UpdateInsertUtrFilter(data);
            }
            var response = new ResponseStatus
            {
                Statuscode = res ? ErrorCodes.One : ErrorCodes.Minus1,
                Msg = res ? "Updated" : ErrorCodes.TempError
            };
            return Json(response);
        }

        [HttpPost, Route("_UTRStatementList")]
        public async Task<IActionResult> UTRStatementList()
        {
            IAdminML ml = new UserML(_accessor, _env);
            var response = await ml.GetUTRStatementAsync();
            return PartialView("Partial/_UTRStatementList", response);
        }


        [Route("Download-UTRStatement")]
        public async Task<IActionResult> DownloadUTRStatement(int FileId)
        {
            IAdminML ml = new UserML(_accessor, _env);
            var response = await ml.DownloadUTRStatementAsync(FileId);
            DataTable dataTable = ConverterHelper.O.ToDataTable(response.ToList());
            var removableCol = new string[] { "FileId", "Id" };
            var Contents = EportExcel.o.GetFile(dataTable, removableCol);
            return File(Contents, DOCType.XlsxContentType, "UTRDetail.xlsx");
        }

        [HttpPost, Route("UtrStatementReconcile/{FiledID}")]
        public async Task<IActionResult> UtrStatementReconcile(string FiledID)
        {
            IBankML ml = new BankML(_accessor, _env);
            var response = await ml.UtrStatementReconcile(FiledID);
            return Json(response); ;
        }

        [HttpPost, Route("UtrStatementDelete/{FiledID}")]
        public async Task<IActionResult> UtrStatementDelete(string FiledID)
        {
            IBankML ml = new BankML(_accessor, _env);
            var response = await ml.UtrStatementDelete(FiledID);
            return Json(response); ;
        }
        [HttpPost]
        [Route("_BankShowtoggle/{ID}")]
        public IActionResult BankShowtoggle(int ID)
        {
            IBankML operation = new BankML(_accessor, _env);
            var res = operation.BankShowMl(ID);
            return Json(res);
        }
        [HttpPost]
        [Route("InsertPartyDetail/{BankID}")]
        public IActionResult InsertPartyDetail(int BankID)
        {
            IBankML bankML = new BankML(_accessor, _env);
            int UserID = _lr.UserID;
            var res = bankML.AddParty(BankID, UserID);
            return Json(res);
        }

    }
}
