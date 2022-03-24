using Fintech.AppCode;
using Fintech.AppCode.Configuration;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode;
using RoundpayFinTech.AppCode.Configuration;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.MoneyTransfer;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Recharge;
using RoundpayFinTech.AppCode.StaticModel.MoneyTransfer;
using RoundpayFinTech.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Validators;

namespace RoundpayFinTech.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public partial class SellerController : Controller
    {
        #region Globle Variables        
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly ISession _session;
        private readonly LoginResponse _lr;
        private readonly ILoginML loginML;
        #endregion
        public SellerController(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            _session = _accessor.HttpContext.Session;
            _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
            loginML = new LoginML(_accessor, _env);
        }
        public IActionResult Index()
        {
            if (loginML.IsInValidSession())
            {
                return RedirectToAction("Index", "Login");
            }
            if (_lr.RoleID == Role.Retailor_Seller && LoginType.ApplicationUser == _lr.LoginTypeID)
            {
                ISellerML s_ML = new SellerML(_accessor, _env);
                var package_Cl = new SellerDashboard()
                {
                    PackageCl = s_ML.GetPackage(),
                    IsMLM = _lr.IsMultiLevel
                };
                _session.SetObjectAsJson(SessionKeys.Service, package_Cl);
                return View(package_Cl);
            }
            return Ok();
        }
        [HttpGet]
        [Route("Seller/Service/{s}")]
        [Route("Service/{s}")]
        public IActionResult Recharge(int s)
        {
            if (s == OPTypes.CouponVoucher)
            {
                ISellerCouponModuleML ml = new SellerCouponModuleML(_accessor, _env);
                if (ml.CountImage())
                {
                    var a = ml.GetCouponvocherImage(s);


                    return View("CouponVoucherImageList", a);
                }
            }
            if (s == OPTypes.Hotel)
            {
                return View("~/Views/Hotel/SearchHotel.cshtml");
            }
            return View("Recharge", s);
        }
        [HttpPost]
        [Route("TopFive")]
        public async Task<IActionResult> TopFive(int Id)
        {
            IReportML RML = new ReportML(_accessor, _env);
            return PartialView("Partial/Recharge/_TopFive", await RML.GetTopRecentTransactions(Id, 5).ConfigureAwait(false));
        }
        [HttpPost]
        [Route("Seller/BPConfirm")]
        [Route("BPConfirm")]
        public IActionResult BillPaymentConfirm(bool IsQuickPay)
        {
            return PartialView("Partial/Recharge/_BillPaymentConfirm", IsQuickPay);
        }
        [HttpPost]
        [Route("Seller/rech")]
        [Route("rech")]
        public async Task<IActionResult> rech(int s)
        {
            IOperatorML OpML = new OperatorML(_accessor, _env);
            IOperatorAppML opml = new OperatorML(_accessor, _env, false);

            var servicesPM = new ServicesPM
            {
                Detail = OpML.GetOperatorsActive(s),
                OPType = s,
                Circles = await opml.CircleList().ConfigureAwait(false),
                IsDoubleFactor = _lr.IsDoubleFactor

            };

            servicesPM.ServiceID = OpML.GetServiceByOpTypeID(s);
            if (s != 55 && s != 56 && s != 57)
                return PartialView("Partial/Recharge/_Recharge", servicesPM);
            else
            {
                ILeadServiceML ml = new LeadServiceML(_accessor, _env);
                if (s == 55)
                {
                    servicesPM.loanTypes = ml.GetLoanType();
                    servicesPM.customerTypes = ml.GetCustomerType();
                }
                else if (s == 57)
                {
                    servicesPM.insuranceTypes = ml.GetInsuranceTypes();
                }
                else if (s == 56)
                {
                    IBankML bankML = new BankML(_accessor, _env);
                    servicesPM.bankMasters = bankML.GetCrediCardBanks();
                }
                return PartialView("Partial/Recharge/_LeadGeneration", servicesPM);
            }
        }

        [HttpPost]
        [Route("Seller/dorech")]
        [Route("dorech")]
        public async Task<IActionResult> DoRech([FromBody] RechargeRequest m)
        {
            ISellerML sellerML = new SellerML(_accessor, _env);
            //await Task.Delay(0);
            //return Json(new ResponseStatus
            //{
            //    Statuscode = 2,
            //    Msg = "Recharge Successful",
            //    CommonStr = "TRAKSAJ123123123",
            //    CommonStr2 = "LiveID",
            //    CommonStr3 = DateTime.Now.ToString("dd MMM yyyy")
            //});
            return Json(await sellerML.Recharge(m).ConfigureAwait(false));
        }

        [HttpPost]
        [Route("Seller/RechargeConfirmationPopUp")]
        [Route("RechargeConfirmationPopUp")]
        public IActionResult RechargeConfirmationPopUp([FromBody] PaymentConfirmationModel paymentConfirmationModel)
        {
            return PartialView("Partial/Recharge/_RechargeConfirmationPopup", paymentConfirmationModel);
        }

        [HttpGet]
        [Route("Seller/GetAccountOpeningBanner/{OpTypeID}")]
        [Route("GetAccountOpeningBanner/{OpTypeID}")]
        public IActionResult GetAccountOpeningBanner(int OpTypeID)
        {
            var res = new List<AccountOpData>();
            if (_lr.RoleID == Role.Retailor_Seller)
            {

                IOperatorML opML = new OperatorML(_accessor, _env);
                res = opML.GetAccountOpeningRedirectionDataByOpType(new CommonReq
                {
                    LoginID = _lr.UserID,
                    CommonInt = OpTypeID
                });

            }
            return View("AccountOpeningChart", res);
        }
        #region AEPS
        [HttpPost]
        [Route("Seller/aeps-interface")]
        [Route("aeps-interface")]
        public IActionResult AEPSInterface(int t, int o)
        {
            var model = new AepsInterfaceModel
            {
                OID = o,
                AEPSInterfaceType = t
            };
            IBankML bankML = new BankML(_accessor, _env);
            var bankList = bankML.AEPSBankMasters();
            if (bankList.Count > 0)
            {
                model.BankList = new SelectList(bankList, nameof(BankMaster.IIN), nameof(BankMaster.BankName));
            }
            return PartialView("Partial/AEPS/_AEPSInterface", model);
        }
        [HttpPost]
        [Route("Seller/aeps-interface-n")]
        [Route("aeps-interface-n")]
        public IActionResult AEPSInterfaceN(int t)
        {
            var model = new AepsInterfaceModel
            {
                AEPSInterfaceType = t
            };
            IBankML bankML = new BankML(_accessor, _env);
            var bankList = bankML.AEPSBankMasters();
            if (bankList.Count > 0)
            {
                model.BankList = new SelectList(bankList, nameof(BankMaster.IIN), nameof(BankMaster.BankName));
            }
            model.BankDetails = bankList;
            IOperatorML opml = new OperatorML(_accessor, _env);
            var Operators = opml.GetOperators(OPTypes.AEPS);
            if (Operators.Any())
            {
                model.Operators = Operators.Where(x => x.IsActive).OrderBy(o => o.Ind);
            }
            return PartialView("Partial/AEPS/_AEPS", model);
        }

        [HttpPost]
        [Route("Seller/Aeps-MiniStatement")]
        [Route("Aeps-MiniStatement")]
        public async Task<IActionResult> _MiniStatement(string PidData, string aadhar, string bank, string bankIIN, int t, string _lat, string _long)
        {

            PidData pidData = new PidData();
            if (!string.IsNullOrEmpty(PidData))
            {
                pidData = XMLHelper.O.DesrializeToObject(pidData, PidData, "PidData", true);
            }
            IDeviceML deviceMl = new DeviceML(_accessor, _env);
            return PartialView("Partial/AEPS/_MiniStatement", deviceMl.MiniStatement(pidData, aadhar, bank, bankIIN, t, _lr.UserID, _lr.OutletID, RequestMode.PANEL, 0, SPKeys.AepsMiniStatement, string.Empty, _lat, _long, PidData));
        }

        [HttpPost]
        [Route("Seller/Deposit-ot")]
        [Route("Deposit-ot")]
        public async Task<IActionResult> GenerateDepositOTP(int Amount, string account, string bankIIN, int t, string _lat, string _long)
        {
            IDeviceML deviceMl = new DeviceML(_accessor, _env);
            return PartialView("Partial/AEPS/_DepositOTP", await deviceMl.DepositGenerateOTP(new DepositRequest
            {
                UserID = _lr.UserID,
                OutletID = _lr.OutletID,
                InterfaceType = t,
                AccountNo = account,
                IIN = bankIIN,
                Amount = Amount,
                RMode = RequestMode.PANEL,
                IMEI = string.Empty,
                Lattitude = _lat,
                Longitude = _long
            }).ConfigureAwait(false));
        }

        [HttpPost]
        [Route("Seller/Deposit-Verify")]
        [Route("Deposit-Verify")]
        public async Task<IActionResult> VerifyDepositOTP(int Amount, string account, string bankIIN, int t, string ref1, string ref2, string ref3, string otp)
        {
            IDeviceML deviceMl = new DeviceML(_accessor, _env);
            var verifyOTPRes = await deviceMl.DepositVerifyOTP(new DepositRequest
            {
                UserID = _lr.UserID,
                OutletID = _lr.OutletID,
                InterfaceType = t,
                AccountNo = account,
                IIN = bankIIN,
                Amount = Amount,
                RMode = RequestMode.PANEL,
                Reff1 = ref1,
                Reff2 = ref2,
                Reff3 = ref3,
                OTP = otp
            }).ConfigureAwait(false);
            if (verifyOTPRes.Statuscode == ErrorCodes.One)
            {
                return PartialView("Partial/AEPS/_DepositConfirmation", verifyOTPRes);
            }
            return Json(verifyOTPRes);
        }

        [HttpPost]
        [Route("Seller/Deposit-Now")]
        [Route("Deposit-Now")]
        public async Task<IActionResult> DepositNow(int Amount, string account, string bankIIN, int t, string ref1, string ref2, string ref3, string otp)
        {
            IDeviceML deviceMl = new DeviceML(_accessor, _env);
            return Json(await deviceMl.DepositAccount(new DepositRequest
            {
                UserID = _lr.UserID,
                OutletID = _lr.OutletID,
                InterfaceType = t,
                AccountNo = account,
                IIN = bankIIN,
                Amount = Amount,
                RMode = RequestMode.PANEL,
                Reff1 = ref1,
                Reff2 = ref2,
                Reff3 = ref3,
                OTP = otp
            }).ConfigureAwait(false));
        }
        #endregion
        #region ElectricityBBPS
        [HttpPost("Seller/FetchBill")]
        [Route("FetchBill")]
        public async Task<IActionResult> FetchBill([FromBody] TransactionServiceReq obj)
        {
            BBPSML bbpsML = new BBPSML(_accessor, _env);
            if (Validators.Validate.O.IsGeoCodeInValid(obj.GEOCode))
            {
                obj.GEOCode = "";
            }
            var res = await bbpsML.FetchBillML(obj).ConfigureAwait(false);
            res.IsBBPSInStaging = ApplicationSetting.IsBBPSInStaging;
            return Json(res);
            //var res = new RoundpayFinTech.AppCode.ThirdParty.CyberPlate.BBPSResponse
            //{
            //    Amount = "100",
            //    billAdditionalInfo = new List<AppCode.ThirdParty.CyberPlate.BillAdditionalInfo> {
            //        new AppCode.ThirdParty.CyberPlate.BillAdditionalInfo{
            //            InfoName="Name1",
            //            InfoValue="Value1"
            //        },new AppCode.ThirdParty.CyberPlate.BillAdditionalInfo{
            //            InfoName="Name2",
            //            InfoValue="Value2"
            //        },
            //    },
            //    CustomerName="Test",
            //    BillDate="01 Jul 2021",
            //    BillNumber="123412341234"
            //};
            return Json(res);
        }
        #endregion
        [HttpPost]
        [Route("OpDetail")]
        public IActionResult OpDetail(int ID)
        {
            IOperatorML operatorML = new OperatorML(_accessor, _env);
            return Json(operatorML.GetOperator(ID));
        }
        [HttpPost]
        [Route("OpOptional")]
        public IActionResult OpOptional(int ID)
        {
            IOperatorML operatorML = new OperatorML(_accessor, _env);
            return PartialView("Partial/Recharge/_OptionalParam", operatorML.OperatorOptional(ID));
        }
        [HttpPost]
        [Route("UserDashBoard")]
        public IActionResult UserDashBoard()
        {
            ISellerML sellerML = new SellerML(_accessor, _env);
            return Json(sellerML.DashBoard());
        }
        #region DMR
        [HttpGet]
        [Route("DMR")]
        public IActionResult DMR()
        {
            return View();
        }
        [HttpPost]
        [Route("SenderGet")]
        public async Task<IActionResult> GetSender(string MobileNo)
        {
            var dmr = new SellerML(_accessor, _env);
            var res = await dmr.CheckSender(MobileNo).ConfigureAwait(false);
            if (res.Statuscode == ErrorCodes.Minus1 && res.CommonInt == ErrorCodes.One)
            {
                return PartialView("Partial/DMT/_SenderReg_OTP", res);
            }
            else if (res.Statuscode == ErrorCodes.One && res.CommonInt == ErrorCodes.One)
            {
                return PartialView("Partial/DMT/_SenderReg");
            }
            return Json(res);
        }

        [HttpPost]
        [Route("SenderKYC")]
        public IActionResult SenderKYC()
        {
            return PartialView("Partial/DMT/_SenderKYC");
        }

        [HttpPost]
        [Route("SenderDoKYC")]
        public async Task<IActionResult> SenderDoKYC(string m, string name, string aaharno, string panno, IFormFile fileAddhar1, IFormFile fileAddhar2, IFormFile filePic, IFormFile filePAN)
        {
            ISellerML sellerML = new SellerML(_accessor, _env);
            var senderKYC = new SenderRequest
            {
                NameOnKYC = name,
                AadharNo = aaharno,
                PANNo = panno,
                AadharFront = fileAddhar1,
                AadharBack = fileAddhar2,
                SenderPhoto = filePic,
                PAN = filePAN,
                MobileNo = m
            };
            return Json(await sellerML.DoKYCSender(senderKYC).ConfigureAwait(false));
        }

        [HttpPost]
        [Route("CreateSender")]
        public async Task<IActionResult> CreateSender(string Name, string Add, string Dob, string MobileNo, string PinCode, string OTP, string LastName)
        {
            var dmr = new SellerML(_accessor, _env);
            var request = new SenderRequest
            {
                Name = Name,
                Address = Add,
                Dob = Dob,
                MobileNo = MobileNo,
                Pincode = PinCode,
                OTP = OTP,
                LastName = LastName
            };
            var _res = await dmr.CreateSender(request).ConfigureAwait(false);
            if (_res.Statuscode == 1 && _res.CommonInt == 1)
            {
                return PartialView("Partial/DMT/_SenderReg_OTP", _res);
            }
            return Json(_res);
        }

        [HttpPost]
        [Route("VerifySender")]
        public async Task<IActionResult> VerifySender(string MobileNo, string OTP, string ReffID)
        {
            var dmr = new SellerML(_accessor, _env);
            var _res = await dmr.VerifySender(MobileNo, OTP, ReffID).ConfigureAwait(false);
            return Json(_res);
        }

        [HttpPost]
        [Route("SReOTP")]
        public async Task<IActionResult> ResendSenderOTP(string MobileNo, string SID)
        {
            var dmr = new SellerML(_accessor, _env);
            var _res = await dmr.SenderResendOTP(MobileNo, SID);
            return Json(_res);
        }
        [HttpPost]
        [Route("AddBene")]
        public IActionResult AddBene()
        {
            IBankML bankML = new BankML(_accessor, _env);
            var banks = bankML.DMRBanks();
            return PartialView("Partial/DMT/_AddBeni", banks);
        }
        [HttpPost]
        [Route("CreateBeni")]
        public async Task<IActionResult> CreateBeni([FromBody] AddBeni addBeni)
        {
            var dmr = new SellerML(_accessor, _env);
            var res = await dmr.AddBeni(addBeni);
            if (res.Statuscode == ErrorCodes.One && res.CommonInt == ErrorCodes.One && (res.ReffID ?? string.Empty) != string.Empty)
            {
                return PartialView("Partial/DMT/_SenderReg_OTP", res);
            }
            else
            {
                return Json(res);
            }
        }
        [HttpPost]
        [Route("GetBeni")]
        public async Task<IActionResult> GetBeni(string MobileNo, string SID)
        {
            var dmr = new SellerML(_accessor, _env);
            return PartialView("Partial/DMT/_BeneList", await dmr.GetBeneficiary(MobileNo, SID));
        }
        [HttpPost]
        [Route("DeleteBene")]
        public async Task<IActionResult> DeleteBene(string MobileNo, string BeneID, string SID, string OTP)
        {
            var dmr = new SellerML(_accessor, _env);
            var res = await dmr.DeleteBene(MobileNo, BeneID, SID, OTP);
            if (res.CommonBool)
            {
                return PartialView("Partial/DMT/_BeneficiaryRemove_OTP", res);
            }
            return Json(res);
        }
        [HttpPost]
        [Route("GenerateOTPForBene")]
        public async Task<IActionResult> GenerateOTPForBene(string MobileNo)
        {
            var dmr = new SellerML(_accessor, _env);
            var res = await dmr.GenerateOTP(MobileNo);
            return PartialView("Partial/DMT/_BeneficiaryValidate_OTP", res);
        }
        [HttpPost]
        [Route("ValidateOTPForBene")]
        public async Task<IActionResult> ValidateOTPForBene(string MobileNo, string BMobile, string AccountNo, string OTP)
        {
            var dmr = new SellerML(_accessor, _env);
            var res = await dmr.ValidateBeneficiaryOTP(MobileNo, BMobile, AccountNo, OTP);
            return Json(res);
        }
        [HttpPost]
        [Route("SendMoneyPopUp")]
        public IActionResult SendMoneyPopUp()
        {
            var dmr = new SellerML(_accessor, _env);
            return PartialView("Partial/DMT/_SendMoney", _lr.IsDoubleFactor);
        }
        [HttpPost]
        [Route("SendMoney")]
        public async Task<IActionResult> SendMoney([FromBody] ReqSendMoney reqSendMoney)
        {
            var dmr = new SellerML(_accessor, _env);
            return Json(await dmr.SendMoney(reqSendMoney));
        }

        [HttpPost]
        [Route("DMTVerification")]
        public async Task<IActionResult> DMTVerification(string MobileNo, string AccountNo, string IFSC, string Bank, int BankID, string BeneName)
        {
            var dmr = new SellerML(_accessor, _env);
            return Json(await dmr.DMTVerification(MobileNo, AccountNo, IFSC, Bank, BankID, BeneName));
        }
        [HttpPost]
        [Route("GetCharge")]
        public IActionResult GetCharge(decimal amount)
        {
            var dmr = new SellerML(_accessor, _env);
            return Json(dmr.GetCharge(amount));
        }
        #endregion
        #region DMTWithPipe
        [HttpPost]
        [Route("SenderGet-p")]
        public IActionResult GetSenderP(string MobileNo, int o)
        {
            var dmr = new SellerML(_accessor, _env);
            var res = dmr.GetSender(MobileNo, o);
            if (res.IsNotActive)
            {
                var senderResend = dmr.SenderResendOTP(MobileNo, res.ReferenceID, o);
                return PartialView("Partial/DMTP/_SenderReg_OTP_Resend", senderResend);
            }
            if (res.IsOTPGenerated)
            {
                return PartialView("Partial/DMTP/_SenderReg_OTP", res);
            }
            else if (res.IsSenderNotExists)
            {
                return PartialView("Partial/DMTP/_SenderReg");
            }
            return Json(res);
        }

        [HttpPost]
        [Route("SenderKYC-p")]
        public IActionResult SenderKYCP()
        {
            return PartialView("Partial/DMTP/_SenderKYC");
        }

        [HttpPost]
        [Route("SenderDoKYC-p")]
        public IActionResult SenderDoKYCP(int o, string m, string name, string aaharno, string panno, IFormFile fileAddhar1, IFormFile fileAddhar2, IFormFile filePic, IFormFile filePAN)
        {
            var sellerML = new SellerML(_accessor, _env);
            var senderKYC = new SenderRequest
            {
                NameOnKYC = name,
                AadharNo = aaharno,
                PANNo = panno,
                AadharFront = fileAddhar1,
                AadharBack = fileAddhar2,
                SenderPhoto = filePic,
                PAN = filePAN,
                MobileNo = m,
                OID = o
            };
            return Json(sellerML.SenderKYC(senderKYC));
        }
        [HttpPost]
        [Route("SenderEKYC-p")]
        public IActionResult SenderEKYCP()
        {
            return PartialView("Partial/DMTP/_SenderEKYC");
        }
        [HttpPost]
        [Route("SenderDoEKYC-p")]
        public IActionResult SenderDoEKYCP(int o, string m, string PidData, string name, string aaharno, string ReffID)
        {
            var sellerML = new SellerML(_accessor, _env);
            var senderKYC = new SenderRequest
            {
                NameOnKYC = name,
                AadharNo = aaharno,
                PidData = PidData,
                MobileNo = m,
                OID = o,
                ReffID = ReffID
            };
            return Json(sellerML.SenderEKYC(senderKYC));
        }

        [HttpPost]
        [Route("CreateSender-p")]
        public async Task<IActionResult> CreateSenderP(int o, string Name, string Add, string Dob, string MobileNo, int PinCode, string OTP, string LastName)
        {
            var dmr = new SellerML(_accessor, _env);
            var request = new SenderRequest
            {
                Name = Name,
                Address = Add,
                Dob = Dob,
                MobileNo = MobileNo,
                PinCode = PinCode,
                OTP = OTP,
                LastName = LastName,
                OID = o
            };
            var _res = dmr.CreateSenderP(request);
            if (_res.IsOTPGenerated)
            {
                return PartialView("Partial/DMTP/_SenderReg_OTP_Resend", _res);
            }
            return Json(_res);
        }

        [HttpPost]
        [Route("VerifySender-p")]
        public IActionResult VerifySenderP(int o, string MobileNo, string OTP, string ReffID)
        {
            var dmr = new SellerML(_accessor, _env);
            return Json(dmr.VerifySender(MobileNo, OTP, ReffID, o));
        }

        [HttpPost]
        [Route("SReOTP-p")]
        public IActionResult ResendSenderOTPP(int o, string MobileNo, string SID)
        {
            var dmr = new SellerML(_accessor, _env);
            return Json(dmr.SenderResendOTP(MobileNo, SID, o));
        }
        [HttpPost]
        [Route("AddBene-p")]
        public IActionResult AddBeneP()
        {
            IBankML bankML = new BankML(_accessor, _env);
            return PartialView("Partial/DMTP/_AddBeni", bankML.DMRBanks());
        }
        [HttpPost]
        [Route("CreateBeni-p")]
        public IActionResult CreateBeniP(AddBeni addBeni)
        {
            var dmr = new SellerML(_accessor, _env);
            var res = dmr.AddBeneFiciary(addBeni);
            if (res.IsOTPGenerated)
            {
                var mdlResp = new MSenderCreateResp
                {
                    Statuscode = res.Statuscode,
                    Msg = res.Msg,
                    ReferenceID = res.ReferenceID
                };
                return PartialView("Partial/DMTP/_BeneficiaryValidate_OTP", mdlResp);
            }
            else
            {
                return Json(res);
            }
        }
        [HttpPost]
        [Route("GetBeni-p")]
        public IActionResult GetBeniP(int o, string MobileNo, string SID)
        {
            var dmr = new SellerML(_accessor, _env);
            return PartialView("Partial/DMTP/_BeneList", dmr.GetBeneficiary(MobileNo, SID, o));
        }
        [HttpPost]
        [Route("DeleteBene-p")]
        public IActionResult DeleteBeneP(int o, string MobileNo, string BeneID, string SID, string OTP)
        {
            var dmr = new SellerML(_accessor, _env);
            var res = dmr.DeleteBene(o, MobileNo, BeneID, SID, OTP);
            if (res.IsOTPGenerated)
            {
                return PartialView("Partial/DMTP/_BeneficiaryRemove_OTP", res);
            }
            return Json(res);
        }
        [HttpPost]
        [Route("GenerateOTPForBene-p")]
        public IActionResult GenerateOTPForBeneP(int o, string MobileNo, string SID, string BeneID)
        {
            var dmr = new SellerML(_accessor, _env);
            var res = dmr.GenerateOTP(o, MobileNo, SID, BeneID);
            return PartialView("Partial/DMTP/_BeneficiaryValidate_OTP", res);
        }
        [HttpPost]
        [Route("ValidateOTPForBene-p")]
        public IActionResult ValidateOTPForBeneP(int o, string MobileNo, string BMobile, string AccountNo, string OTP, string SID, string BeneID)
        {
            var dmr = new SellerML(_accessor, _env);
            var res = dmr.ValidateBeneficiaryOTP(o, MobileNo, BMobile, AccountNo, OTP, SID, BeneID);
            return Json(res);
        }
        [HttpPost]
        [Route("SendMoneyPopUp-p")]
        public IActionResult SendMoneyPopUpP()
        {
            var dmr = new SellerML(_accessor, _env);
            return PartialView("Partial/DMTP/_SendMoney", _lr.IsDoubleFactor);
        }
        [HttpPost]
        [Route("SendMoney-p")]
        public IActionResult SendMoneyP([FromBody] ReqSendMoney reqSendMoney)
        {
            var dmr = new SellerML(_accessor, _env);
            return Json(dmr.AccountTransfer(reqSendMoney));
        }
        [HttpPost]
        [Route("DMTVerification-p")]
        public IActionResult DMTVerificationP(int o, string MobileNo, string AccountNo, string IFSC, string Bank, int BankID, string BeneName, string SID)
        {
            var dmr = new SellerML(_accessor, _env);
            return Json(dmr.VerifyAccount(o, MobileNo, AccountNo, IFSC, Bank, BankID, BeneName, SID));
        }
        [HttpPost]
        [Route("GetCharge-p")]
        public IActionResult GetChargeP(int o, int amount)
        {
            var dmr = new SellerML(_accessor, _env);
            return Json(dmr.GetCharges(o, amount));
        }
        #endregion
        #region UPIPayment
        [HttpPost]
        [Route("upi-pmt")]
        public IActionResult DoUPIPayment([FromBody] ReqSendMoney reqSendMoney)
        {
            var dmr = new SellerML(_accessor, _env);
            return Json(dmr.DoUPIPayment(reqSendMoney));
        }
        #endregion
        #region ROfferSegment
        [HttpPost]
        [Route("RechSimpPlans")]
        public IActionResult RechSimpPlans(int o, int c)
        {
            if (ApplicationSetting.IsPlanServiceUpdated)
            {
                var plansAPIML = new PlansAPIML(_accessor, _env);
                var data = plansAPIML.GetRNPRechargePlans(o, c);
                return PartialView("Partial/RNPNewPlans/RNPSimplePlan", data);
            }
            else
            {
                if (ApplicationSetting.PlanType == PlanType.MPLAN)
                {
                    IMplan plansAPIML = new PlansAPIML(_accessor, _env);
                    var resp = plansAPIML.GetSimplePlan(c, o);
                    return PartialView("Partial/MPLAN/SimplePlan", resp);
                }
                else if (ApplicationSetting.PlanType == PlanType.Roundpay)
                {
                    IRoundpayPlan plansAPIML = new PlansAPIML(_accessor, _env);
                    var resp = plansAPIML.GetSimplePlanRoundpay(c, o);
                    return PartialView("Partial/RoundpayPLAN/SimplePlan", resp);
                }
                else if (ApplicationSetting.PlanType == PlanType.PLANAPI)
                {
                    IPlanAPIPlan plansAPIML = new PlansAPIML(_accessor, _env);
                    var resp = plansAPIML.GetSimplePlanAPI(c, o);
                    return PartialView("Partial/PLANAPI/SimplePlan", resp);
                }
                else if (ApplicationSetting.PlanType == PlanType.MYPLAN)
                {
                    IMyPlanAPI plansAPIML = new PlansAPIML(_accessor, _env);
                    var resp = plansAPIML.GetSimpleMyPlanAPI(c, o);
                    return PartialView("Partial/MYPLANAPI/SimplePlanView", resp);
                }
            }
            return BadRequest(new { Msg = "No setting found" });
        }
        [HttpPost]
        [Route("roffer")]
        public IActionResult Roffer(int o, string a)
        {
            if (ApplicationSetting.IsRoffer)
            {
                if (ApplicationSetting.IsPlanServiceUpdated)
                {
                    var plansAPIML = new PlansAPIML(_accessor, _env);
                    var data = plansAPIML.GetRNPRoffer(a, o);
                    return PartialView("Partial/RNPNewPlans/RNPRofferPlan", data);
                }
                else
                {
                    if (ApplicationSetting.PlanType == PlanType.MPLAN)
                    {
                        IMplan plansAPIML = new PlansAPIML(_accessor, _env);
                        var resp = plansAPIML.GetRoffer(a, o);
                        return PartialView("Partial/MPLAN/Roffer", resp);
                    }
                    else if (ApplicationSetting.PlanType == PlanType.Roundpay)
                    {
                        IRoundpayPlan plansAPIML = new PlansAPIML(_accessor, _env);
                        var resp = plansAPIML.GetRofferRoundpay(a, o);
                        return PartialView("Partial/RoundpayPLAN/Roffer", resp);
                    }
                    else if (ApplicationSetting.PlanType == PlanType.PLANAPI)
                    {
                        IPlanAPIPlan plansAPIML = new PlansAPIML(_accessor, _env);
                        var resp = plansAPIML.GetRofferPLANAPI(a, o);
                        return PartialView("Partial/PLANAPI/Roffer", resp);
                    }
                    else if (ApplicationSetting.PlanType == PlanType.CYRUS)
                    {
                        ICyrusAPIPlan plansAPIML = new PlansAPIML(_accessor, _env);
                        var resp = plansAPIML.GetRofferCYRUS(a, o);
                        return PartialView("Partial/CYRUS/Roffer", resp);
                    }
                    else if (ApplicationSetting.PlanType == PlanType.VASTWEB)
                    {
                        IVastWebPlan plansAPIML = new PlansAPIML(_accessor, _env);
                        var resp = plansAPIML.GetRofferVastWeb(a, o);
                        return PartialView("Partial/VastWebPlan/Roffer", resp);
                    }
                    else if (ApplicationSetting.PlanType == PlanType.MYPLAN)
                    {
                        IMyPlanAPI plansAPIML = new PlansAPIML(_accessor, _env);
                        var resp = plansAPIML.GetRofferMyPlanApi(a, o);
                        return PartialView("Partial/MYPLANAPI/Roffer", resp);
                    }
                }
            }
            return BadRequest(new { Msg = "No setting found" });
        }
        [HttpPost]
        [Route("dthsimplan")]
        public IActionResult DTHSimpPlan(int o)
        {
            if (ApplicationSetting.IsPlanServiceUpdated)
            {
                var plansAPIML = new PlansAPIML(_accessor, _env);
                var data = plansAPIML.GetRNPDTHPlans(o);
                return PartialView("Partial/RNPNewPlans/RNPDTHSimplePlan", data);
            }
            else
            {
                if (ApplicationSetting.PlanType == PlanType.MPLAN)
                {
                    IMplan plansAPIML = new PlansAPIML(_accessor, _env);
                    var resp = plansAPIML.GetDTHSimplePlan(o);
                    return PartialView("Partial/MPLAN/DTHPlan", resp);
                }
                else if (ApplicationSetting.PlanType == PlanType.Roundpay)
                {
                    IRoundpayPlan plansAPIML = new PlansAPIML(_accessor, _env);
                    if (ApplicationSetting.IsDTHPlanWithChannelList)
                    {
                        var resp = plansAPIML.RPDTHSimplePlansOfPackages(o);
                        return PartialView("Partial/RoundpayPLAN/DTHPlanPackage", resp);
                    }
                    else
                    {
                        var resp = plansAPIML.GetDTHSimplePlanRoundpay(o);
                        return PartialView("Partial/RoundpayPLAN/DTHPlan", resp);
                    }
                }
                else if (ApplicationSetting.PlanType == PlanType.PLANAPI)
                {
                    IPlanAPIPlan plansAPIML = new PlansAPIML(_accessor, _env);
                    var resp = plansAPIML.GetDTHSimplePlanAPI(o);
                    return PartialView("Partial/PLANAPI/DTHPlan", resp);
                }
                else if (ApplicationSetting.PlanType == PlanType.MYPLAN)
                {
                    IMyPlanAPI plansAPIML = new PlansAPIML(_accessor, _env);
                    var resp = plansAPIML.GetDthSimpleMyPlanApi(o);
                    return PartialView("Partial/MYPLANAPI/DthPlanView", resp);
                }
                return BadRequest(new { Msg = "No setting found" });
            }
            return BadRequest(new { Msg = "No setting found" });
        }
        [HttpPost]
        [Route("dthcust")]
        public IActionResult DTHCustInfo(int o, string a)
        {
            if (ApplicationSetting.IsDTHInfo)
            {
                if (ApplicationSetting.IsPlanServiceUpdated)
                {
                    var plansAPIML = new PlansAPIML(_accessor, _env);
                    var data = plansAPIML.GetRNPDTHCustInfo(o, a);
                    return PartialView("Partial/RNPNewPlans/RNPDTHCustInfo", data);
                }
                else
                {
                    if (ApplicationSetting.PlanType == PlanType.MPLAN)
                    {
                        IMplan plansAPIML = new PlansAPIML(_accessor, _env);
                        var resp = plansAPIML.GetDTHCustInfo(a, o);
                        return PartialView("Partial/MPLAN/DTHCustInfo", resp);
                    }
                    else if (ApplicationSetting.PlanType == PlanType.Roundpay)
                    {
                        IRoundpayPlan plansAPIML = new PlansAPIML(_accessor, _env);
                        var resp = plansAPIML.GetDTHCustInfoRoundpay(a, o);
                        return PartialView("Partial/RoundpayPLAN/DTHCustInfo", resp);
                    }
                    else if (ApplicationSetting.PlanType == PlanType.PLANAPI)
                    {
                        IPlanAPIPlan plansAPIML = new PlansAPIML(_accessor, _env);
                        var resp = plansAPIML.GetDTHCustInfoPlanAPI(a, o);
                        return PartialView("Partial/PLANAPI/DTHCustInfo", resp);
                    }
                    else if (ApplicationSetting.PlanType == PlanType.VASTWEB)
                    {
                        IVastWebPlan plansAPIML = new PlansAPIML(_accessor, _env);
                        var resp = plansAPIML.GetDTHCustInfoVastWeb(a, o);
                        return PartialView("Partial/VastWebPlan/DTHCustInfo", resp);
                    }
                    else if (ApplicationSetting.PlanType == PlanType.MYPLAN)
                    {
                        IMyPlanAPI plansAPIML = new PlansAPIML(_accessor, _env);
                        var resp = plansAPIML.GetDTHCustInfoMyPlan(a, o);
                        return PartialView("Partial/MYPLANAPI/DthCustomerRecord", resp);
                    }
                }

            }
            return BadRequest(new { Msg = "No setting found" });
        }
        /// <summary>
        /// Mplan DTH Heavy Refresh
        /// </summary>
        /// <param name="o"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("dthheavyref")]
        public IActionResult DTHHeavyRefresh(int o, string a)
        {
            if (ApplicationSetting.IsHeavyRefresh)
            {
                if (ApplicationSetting.IsPlanServiceUpdated)
                {
                    var plansAPIML = new PlansAPIML(_accessor, _env);
                    var data = plansAPIML.GetRNPDTHHeavyRefresh(o, a);
                    return PartialView("Partial/RNPNewPlans/RNPDTHHeavyRefresh", data);
                }
                else
                {
                    if (ApplicationSetting.PlanType == PlanType.MPLAN)
                    {
                        IMplan plansAPIML = new PlansAPIML(_accessor, _env);
                        var resp = plansAPIML.GetDTHHeavyRefresh(a, o);
                        return PartialView("Partial/MPLAN/DTHHeavyRefresh", resp);
                    }
                    else if (ApplicationSetting.PlanType == PlanType.Roundpay)
                    {
                        IRoundpayPlan plansAPIML = new PlansAPIML(_accessor, _env);
                        var resp = plansAPIML.GetDTHRPHeavyRefresh(a, o);
                        return PartialView("Partial/RoundpayPLAN/DTHHeavyRefresh", resp);
                    }
                    else if (ApplicationSetting.PlanType == PlanType.MYPLAN)
                    {
                        IMyPlanAPI plansAPIML = new PlansAPIML(_accessor, _env);
                        var resp = plansAPIML.GetDthHaveyRefershMyPlan(a, o);
                        return PartialView("Partial/MYPLANAPI/MyPlanDTHHeavyRefresh", resp);
                    }
                }

            }
            return BadRequest(new { Msg = "No setting found" });
        }
        #endregion
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (_lr == null && (context.RouteData.Values["Action"].ToString() != "GIRedirect"))
            {
                context.Result = new RedirectResult("~/");
            }
            else if (loginML.IsInValidSession() && (context.RouteData.Values["Action"].ToString() != "Index" || context.RouteData.Values["Controller"].ToString() != "Seller" || (_lr.RoleID != Role.Retailor_Seller && _lr.LoginTypeID == LoginType.ApplicationUser)) && context.RouteData.Values["Action"].ToString() != "GIRedirect")
            {
                context.Result = new RedirectResult("~/");
            }
            else
            {
                base.OnActionExecuting(context);
            }
        }
        #region Public-E-Services
        [HttpGet("Public-E-Services.php")]
        public IActionResult PES()
        {
            IOperatorML operatorML = new OperatorML(_accessor, _env);
            return View(operatorML.GetOperators(20));
        }
        [HttpPost]
        [Route("ShowForm.php")]
        public IActionResult ShowForm(int OID)
        {
            IPublicService pes = new PublicServiceML(_accessor, _env);
            return PartialView("Partial/PES/_ShowForm", pes.ShowForm(OID));
        }
        [HttpPost]
        [Route("SavePESForm.php")]
        public IActionResult SavePESForm(IFormCollection formdata)
        {
            IPublicService pes = new PublicServiceML(_accessor, _env);
            return Json(pes.SavePESFormML(formdata));
        }
        #endregion
        #region OnboardingSteps
        [HttpPost]
        [Route("OnBoradUser")]
        public IActionResult OnboardUserRequest(int ID, string OTP, int OTPRefID, string pidData, bool isBio, int BioAuthtype)
        {
            ISellerML sellerML = new SellerML(_accessor, _env);
            var _res = sellerML.CheckOnboardUser(ID, OTP, OTPRefID, pidData, isBio, BioAuthtype);
            if (_res.IsRedirection)
            {
                LoginML loginML = new LoginML(_accessor, _env);
                var WebInfo = loginML.GetWebsiteInfo();
                _res.CommonStr = WebInfo.AbsoluteHost + "/ProfileUser";
                _res.Msg = _res.Msg + " To use this service goto update profile section";
            }
            return Json(_res);
        }
        [HttpPost]
        [Route("OpenBiometric")]
        public IActionResult OpenBiometric(int ID, int OTPRefID,int BioAuthtype)
        {
            var mdl = new OutletbBiometricModel
            {
                OID = ID,
                RefID = OTPRefID,
                BioAuthType= BioAuthtype
            };
            return PartialView("Partial/Recharge/_BioMetricAuthentication", mdl);
        }

        [HttpPost]
        [Route("OpenEKYC")]
        public IActionResult OpenEKYCForm(string a)
        {
            return PartialView("Partial/SellerEKYC", a);
        }
        [HttpPost]
        [Route("DoEKYC")]
        public IActionResult DoEKYC(string PidData, string aaharno)
        {
            IOnboardingML onboardingML = new OnboardingML(_accessor, _env);
            return Json(onboardingML.UpdateUIDTokenAgent(new OutletEKYCRequest
            {
                AadaarNo = aaharno,
                PidData = PidData,
                OutletID = _lr.OutletID,
                UserID = _lr.UserID
            }));
        }
        #endregion
        #region PSARegion
        [HttpPost]
        [Route("Seller/dopsa")]
        [Route("dopsa")]
        public async Task<IActionResult> DoPSA([FromBody] RechargeRequest m)
        {
            ISellerML sellerML = new SellerML(_accessor, _env);
            return Json(await sellerML.PSATransaction(m).ConfigureAwait(false));
        }
        [HttpPost]
        [Route("PSA-Credential")]
        public IActionResult PSACredential()
        {
            return PartialView("Partial/Recharge/_ViewPSACrediential");
        }
        [HttpPost]
        [Route("PurchargeToken")]
        public IActionResult PurchargeToken()
        {
            IOperatorML OpML = new OperatorML(_accessor, _env);
            var servicesPM = new ServicesPM
            {
                Detail = OpML.GetOperatorsActive(OPTypes.PSA)
            };
            return PartialView("Partial/Recharge/_PurchargeToken", servicesPM);
        }
        #endregion
        #region CallMe
        [HttpPost]
        [Route("callme")]
        public IActionResult CallMe(string uMob)
        {
            var seller = new SellerML(_accessor, _env);
            ResponseStatus _resp = seller.CallMeUserRequest(uMob);
            return Json(_resp);
        }
        #endregion
        [HttpPost]
        [Route("_cashback")]
        public IActionResult _cashback(int OID)
        {
            var ml = new SlabML(_accessor, _env);
            var _filter = new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID,
                CommonInt = OID
            };
            var resp = ml.GetIncentive(_filter);
            return PartialView("Partial/Recharge/_cashback", resp);
        }
        #region GIRedirection
        [HttpGet("Seller/GIRedirect")]
        [Route("/GIRedirect")]
        public IActionResult GIRedirect(string code, string prodid, string tranid, int outlet)
        {
            Dictionary<string, string> KeyVals = null;
            var giML = new GeneralInsuranceML(_accessor, _env);
            var appSetting = giML.GetGIAppSetting(code);
            string APIURL = appSetting._URL ?? string.Empty;
            if (code == APICode.APIWALE)
            {
                KeyVals = new Dictionary<string, string>
                {
                    { "Productid", prodid },
                    { "utm_source", "Roundpay" },
                    { "utm_medium", "Roundpay" },
                    { "partnerleadid", tranid },
                    { "partneragentid",  outlet.ToString()},
                    { "utm_term",  tranid}
                };
            }

            if (code == APICode.GIBL)
            {
                var UID = _lr.OutletID.ToString().PadLeft(8, 'R');
                var Name = _lr.Name.Split(" ");
                int lastIndex = Name.Length;
                KeyVals = new Dictionary<string, string>
                {
                    { "urc" ,UID},
                    { "umc", ApplicationSetting.GIBLUMC },
                    { "ak", tranid },
                    { "fname", lastIndex > 1 ? Name[0]:""},
                    { "lname",  lastIndex > 1 ? Name[lastIndex-1]:Name[0]},
                    { "email", _lr.EmailID },
                    { "phno", _lr.MobileNo },
                    { "pin", _lr.Pincode }
                };

                var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(KeyVals));
                var ReqSignature = HashEncryption.O.EncryptUsingPublicKeyPEM(data, DOCType.GIBLCertificatePath);

                KeyVals = new Dictionary<string, string>
                {
                    { "ret_data",ReqSignature}
                };
            }

            var model = new GIRedirectModel
            {
                dic = KeyVals,
                URL = APIURL
            };
            return View(model);
        }
        #endregion
        [HttpPost]
        [Route("/DTHChannelByPackage")]
        public IActionResult DTHChannelByPackage(int Pid)
        {
            IOperatorML ml = new OperatorML(_accessor, _env);
            var res = ml.DTHChannelByPackage(Pid);
            return PartialView("Partial/Recharge/_DTHChannelByPackage", res);
        }
        [HttpPost]
        [Route("/doDTH-subscription")]
        public async Task<IActionResult> DoDTHSub(DTHConnectionServiceModel dTHConnectionServiceModel)
        {
            //PID(int), CustomerNumber(string),Customer(string),Address(string),Pincode(string),SecurityKey(string if DoubleFactorEnable) 
            ISellerML sellerML = new SellerML(_accessor, _env);
            var res = await sellerML.DoDTHSubscription(dTHConnectionServiceModel).ConfigureAwait(false);
            return Json(res);
        }
        [HttpGet]
        [Route("AffilietedItems")]
        public IActionResult AffilitedItems()
        {
            return View("Affiliates/AffilietedItems");
        }
        [HttpPost]
        [Route("GetAffilietedItems")]
        public IActionResult _GetAffilietedItems(int VendorID)
        {
            IAffiliationML ml = new AffiliationML(_accessor, _env);
            var res = ml.GetAllAfItems(VendorID);
            res = res.Where(x => x.IsActive);
            return PartialView("Affiliates/Partial/_GetAffilietedItems", res);
        }
        [HttpPost]
        [Route("GetAfItemsForDisplay")]
        public IActionResult GetAfItemsForDisplay()
        {
            IAffiliationML ml = new AffiliationML(_accessor, _env);
            var res = ml.GetAfItemsForDisplay();
            return Json(res);
        }
        [HttpPost]
        [Route("/bind-PinCodeArea")]
        public IActionResult bindPinCodeArea(int PinCode)
        {

            var _req = new CommonReq
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID,
                CommonInt2 = PinCode
            };
            IUserML UML = new UserML(_accessor, _env);
            var _res = UML.GetPincodeArea(_req);
            return Json(_res);
        }
        [HttpPost]
        [Route("/DTHDiscription")]
        public IActionResult DTHDiscription(int PID)
        {
            IOperatorML ml = new OperatorML(_accessor, _env);
            var _res = ml.GetDTHDiscription(PID, 0);
            return PartialView("Partial/Recharge/_DTHDiscription", _res);
        }
        [HttpPost]
        [Route("/BamountCommDetails")]
        public IActionResult BamountCommDetails(int PID)
        {
            IOperatorML ml = new OperatorML(_accessor, _env);
            var _res = ml.GetDTHDiscription(PID, 0);
            return PartialView("Partial/Recharge/_BAmountComm", _res);
        }
        #region LeadService




        [HttpPost]
        [Route("/SaveLeadService")]
        public IActionResult SaveLeadService([FromBody] LeadService req)
        {
            IResponseStatus responseStatus = new ResponseStatus
            {
                Statuscode = 1,
                Msg = "Pending"
            };
            ILeadServiceML ml = new LeadServiceML(_accessor, _env);
            if (!Validate.O.IsMobile(req.Mobile))
            {
                responseStatus.Statuscode = -1;
                responseStatus.Msg = "Invalid Mobile No.";
                return Json(responseStatus);
            }
            if (req.Email != null && !Validate.O.IsEmail(req.Email))
            {
                responseStatus.Statuscode = -1;
                responseStatus.Msg = "Invalid Email Id.";
                return Json(responseStatus);
            }
            if (req.PAN != null && !Validate.O.IsPAN(req.PAN))
            {
                responseStatus.Statuscode = -1;
                responseStatus.Msg = "Invalid PAN";
                return Json(responseStatus);
            }
            return Json(ml.SaveLeadService(req));
        }
        #endregion
        #region WalletToWallet

        [HttpPost]
        [Route("Home/g-u-i")]
        [Route("g-u-i")]
        public IActionResult UserInfo(string MobileNo)
        {
            IUserML uml = new UserML(_accessor, _env);
            if (_lr.MobileNo.Equals(MobileNo))
                return Json(new { StatusCode = -1, Msg = "Wrong User!" });
            var req = new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID,
                CommonStr = MobileNo
            };
            var userInfo = uml.GetUserByMobile(req);
            if ((userInfo != null) && (userInfo.RoleID == Role.Retailor_Seller))
                return Json(userInfo);
            return Json(new { StatusCode = -1, Msg = "Wrong User!" });
        }
        [HttpPost]
        [Route("Home/w-2-w")]
        [Route("w-2-w")]
        public IActionResult _WToWPV()
        {
            return PartialView("Partial/_WalletToWallet");
        }

        [HttpPost]
        [Route("Home/wtwft")]
        [Route("wtwft")]
        public IActionResult WToWFT(CommonReq commonReq)
        {
            IUserML uml = new UserML(_accessor, _env);
            commonReq.LoginID = _lr.UserID;
            commonReq.LoginTypeID = _lr.LoginTypeID;
            var res = uml.WTWFT(commonReq);
            return Json(res);
        }


        #endregion
        [Route("RedirectToShopping")]
        public IActionResult RedirectToShopping()
        {
            ViewBag.WId = _lr.WID;
            ViewBag.SessKey = _lr.SessionID;
            ViewBag.UId = _lr.UserID;

            var request = _accessor.HttpContext.Request;
            var host = request.Host.Value;
            string url = string.Empty;
            if (!host.Contains("localhost"))
            {
                var _wi = _session.GetObjectFromJson<WebsiteInfo>(SessionKeys.WInfo);
                url = _wi.ShoppingDomain;
            }
            else
            {
                if (request.IsHttps)
                {
                    url = "https://";
                }
                else
                { url = "http://"; }
                url += host + "/ECommRedirectLogin";
            }
            ViewBag.Url = url;
            return View();
        }

        [HttpGet]
        [Route("/CUserSeller")]
        public IActionResult CUserSeller(int rid)
        {
            rid = _lr.UserID;
            ISettingML _settingML = new SettingML(_accessor, _env);
            var WebInfo = loginML.GetWebsiteInfo();
            UserML userML = new UserML(_accessor, _env);
            var Cprofile = userML.GetCompanyProfileApp(WebInfo.WID);
            var loginPageModel = new LoginPageModel
            {
                WID = WebInfo.WID,
                Host = WebInfo.AbsoluteHost,
                ThemeID = WebInfo.ThemeId,
                AppName = Cprofile.AppName,
                CustomerCareMobileNos = Cprofile.CustomerCareMobileNos,
                CustomerPhoneNos = Cprofile.CustomerPhoneNos,
                referralRoleMaster = new ReferralRoleMaster
                {
                    ReferralID = rid,
                    Roles = _settingML.GetRoleForReferral(rid)
                }
            };
            if (loginPageModel.ThemeID == 4)
            {
                IBannerML bannerML = new ResourceML(_accessor, _env);
                loginPageModel.BGServiceImgURLs = bannerML.SiteGetServices(loginPageModel.WID, loginPageModel.ThemeID);
            }
            return View("CreateUser/UserReg", loginPageModel);
        }
        [HttpPost]
        [Route("CUserSell")]
        public IActionResult CUserSell([FromBody] UserCreate UserCreate)
        {
            if (!ApplicationSetting.IsSingupPageOff)
            {
                IUserML _UserML = new UserML(_accessor, _env);
                IResponseStatus _resp = _UserML.CallSignup(UserCreate);
                return Json(_resp);
            }
            return Ok();
        }

       
        [Route("Team")]
        public IActionResult Team()
        {           
            return View();
        }
        [HttpPost]
        [Route("_Team")]
        public async Task<IActionResult> TeamList()
        {
            IUserML ml = new UserML(_accessor, _env);
            var res = await ml.GetTeam(_lr.UserID);
            return PartialView("Partial/_Team", res);
        }

        [Route("Level")]
        public IActionResult Level()
        {
            return View();
        }

        [HttpPost]
        [Route("_Level")]
        public async Task<IActionResult> LevelList()
        {
            IUserML ml = new UserML(_accessor, _env);
            var res = await ml.GetLevel(_lr.UserID);
            return PartialView("Partial/_Level", res);
        }
    }
}