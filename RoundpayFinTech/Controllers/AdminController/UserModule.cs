using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Fintech.AppCode.Configuration;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QRCoder;
using RoundpayFinTech.AppCode;
using RoundpayFinTech.AppCode.Configuration;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.Models;


namespace RoundpayFinTech.Controllers
{
    public partial class AdminController
    {
        #region UserManagement
        [HttpPost]
        [Route("PinCodeDetail")]
        public IActionResult PinCodeDetail(string Pincode)
        {
            //IUserML userML = new UserML(_accessor, _env);
            return Json(uow.userML.GetPinCodeDetail(Pincode));
        }

        [HttpGet]
        [Route("Admin/CreateUser")]
        [Route("CreateUser")]
        public IActionResult CreateUser()
        {
            return View("CreateUser/UserReg");
        }

        [HttpPost]
        [Route("Admin/ureg")]
        [Route("ureg")]
        public IActionResult CreateUser(string m)
        {
            m = string.IsNullOrEmpty(m) ? _lr.UserID.ToString() : m;
            //IUserML userML = new UserML(_accessor, _env);
            var res = uow.userML.GetReffDeatil(m);
            var DMRModels = uow.userML.GetDMRModelList();
            if (DMRModels.Any())
            {
                res.DMRModelSelect = new SelectList(DMRModels, "ID", "Name");
            }
            return PartialView("CreateUser/Partial/_UserReg", res);
        }

        [HttpPost]
        [Route("GetChangeSlab")]
        public IActionResult GetChangeSlab(int UserID)
        {
            IUserML userML = new UserML(_accessor, _env);
            var _slab = userML.fn_GetChangeSlab(UserID);
            _slab.IsDoubleFactor = _lr.IsDoubleFactor;
            return PartialView("Partial/_ChnageSlab", _slab);
        }

        [HttpPost]
        [Route("UpdateSlab")]
        public IActionResult UpdateSlab(int UserID, int SlabID, string pinPasswowrd)
        {
            IUserML userML = new UserML(_accessor, _env);
            return Json(userML.UpdateChangeSlab(UserID, SlabID, pinPasswowrd));
        }

        [HttpPost]
        [Route("GetChangeRole")]
        public IActionResult GetChangeRole()
        {
            return PartialView("Partial/_ChangeRole");
        }

        [HttpPost]
        [Route("UpdateRole")]
        public IActionResult UpdateRole(int UserID, int RoleID)
        {
            //IAdminML userML = new UserML(_accessor, _env);
            //return Json(userML.UpdateChangeRole(UserID, RoleID));
            return Json(uow.adminML.UpdateChangeRole(UserID, RoleID, _lr.UserID, _lr.LoginTypeID));
        }
        [Route("Admin/creg")]

        [HttpPost]
        [Route("creg")]
        public IActionResult AddUser([FromBody] UserCreate userCreate)
        {
            userCreate.RequestModeID = RequestMode.PANEL;
            IUserML _CreateUserML = new UserML(_accessor, _env);
            IResponseStatus _resp = _CreateUserML.CallCreateUser(userCreate);
            return Json(_resp);
        }


        [HttpPost]
        [Route("Admin/ShowPass/{id}")]
        [Route("ShowPass/{id}")]
        public IActionResult ShowPass(int ID)
        {
            string password = string.Empty;
            //IAdminML userML = new UserML(_accessor, _env);
            if (_lr.RoleID == Role.Admin)
                password = uow.adminML.ShowPassword(ID);
            return Ok(password);
        }

        [HttpGet]
        [Route("Admin/UserList")]
        [Route("UserList")]
        public IActionResult UserList()
        {
            //IUserML userML = new UserML(_accessor, _env);
            var Roles = uow.userML.GetRoleSlab().Roles.Where(w => w.ID != FixedRole.FOS);
            var userListModel = new UserListModel
            {
                IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser,
                selectListItems = new SelectList(Roles, "ID", "Role"),
                userBalnace = uow.userML.GetUserBalnace(0)
            };
            return View("UserList/UserList", userListModel);
        }

        [HttpPost]
        [Route("Admin/U_List")]
        [Route("U_List")]
        public IActionResult UserList([FromBody] CommonFilter f)
        {
            IUserML userML = new UserML(_accessor, _env);
            f.LoginRoleID = _lr.RoleID;
            f.LT = _lr.LoginTypeID;
            f.IsAdminDefined = _lr.IsAdminDefined;
            f.LoginID = _lr.UserID;
            var res = userML.GetList(f);
            if (res != null && res.userReports != null)
            {
                res.userReports = res.userReports.Where(w => w.RoleID != FixedRole.FOS).ToList();
            }

            var userListModel = new UserListModel
            {
                IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser,
                userList = res,
                PegeSetting = res.PegeSetting,
                userBalnace = userML.GetUserBalnace(0)
            };
            return PartialView("UserList/Partial/_UserList", userListModel);
        }

        [HttpPost]
        [Route("Admin/U_List_Child")]
        [Route("U_List_Child")]
        public IActionResult UserListChild(int ID, bool IsUp)
        {
            IUserML userML = new UserML(_accessor, _env);
            var res = userML.GetListChild(ID, IsUp);
            res.userReports = res.userReports.Where(w => w.RoleID != FixedRole.FOS).ToList();
            var userListModel = new UserListModel
            {
                IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser,
                userList = res,
                userBalnace = userML.GetUserBalnace(0)
            };
            return PartialView("UserList/Partial/_UserList", userListModel);
        }

        [HttpPost]
        [Route("Admin/ChildTotal")]
        [Route("ChildTotal")]
        public IActionResult ChildTotalBalance(int ID)
        {
            IUserML userML = new UserML(_accessor, _env);
            var mdl = new MyBalanceViewModel
            {
                userBalnace = userML.ShowTotalChildBalance(ID)
            };
            return PartialView("UserList/Partial/_Childbalance", mdl);
        }

        [HttpPost]
        [Route("Admin/ToggleStatus")]
        [Route("ToggleStatus")]
        public IActionResult ChangeStatus(int ID, int type, bool Is)
        {
            IUserML userML = new UserML(_accessor, _env);
            IResponseStatus _res = userML.ChangeOTPStatus(ID, type, Is);
            return Json(_res);
        }

        [HttpPost]
        [Route("Admin/updatevirtual")]
        [Route("updatevirtual")]
        public IActionResult UpdateVirtual(int ID)
        {
            IUserML userML = new UserML(_accessor, _env);
            IResponseStatus _res = userML.ChangeVirtualStatus(ID);
            return Json(_res);
        }

        [HttpPost]
        [Route("Admin/updateIsFlatComm")]
        [Route("updateIsFlatComm")]
        public IActionResult UpdateFlatComm(int ID)
        {
            IUserML userML = new UserML(_accessor, _env);
            IResponseStatus _res = userML.ChangeFlatCommStatus(ID);
            return Json(_res);
        }

        [HttpPost]
        [Route("Admin/DoubleFactor")]
        [Route("DoubleFactor")]
        public IActionResult ChangeDoubleFactor(bool Is, string otp)
        {
            IResponseStatus _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            IUserML userML = new UserML(_accessor, _env);
            if (!Is)
            {
                bool IsResend = (otp ?? "").ToLower().Equals(nameof(otp));
                var _otp = _session.GetString(SessionKeys.CommonOTP);
                if ((otp ?? "").Trim().Length > 0 && !IsResend)
                {
                    if (!_otp.Equals(otp))
                    {
                        _res.Msg = nameof(ErrorCodes.Invalid_OTP).Replace("_", " ");
                        return Json(_res);
                    }
                }
                else
                {
                    var o = (_otp ?? "").Length == 6 ? _otp : HashEncryption.O.CreatePasswordNumeric(6);

                    IUserML uml = new UserML(_accessor, _env);
                    var alertData = uml.GetUserDeatilForAlert(_lr.UserID);
                    if (alertData.Statuscode == ErrorCodes.One)
                    {
                        IAlertML alertMl = new AlertML(_accessor, _env);
                        alertData.OTP = o;
                        _res = alertMl.OTPSMS(alertData);
                        alertMl.OTPEmail(alertData);
                    }
                    if (_res.Statuscode == ErrorCodes.One)
                    {
                        if (!IsResend)
                        {
                            _session.SetString(SessionKeys.CommonOTP, o);
                            return PartialView("UserEdit/_DoubleFactor", _res);
                        }
                        _res.Msg = "OTP has been resend successfully!";
                        return Json(_res);
                    }
                }
            }
            _res = userML.ChangeOTPStatus(_lr.UserID, 3, Is);
            if (_res.Statuscode == ErrorCodes.One)
            {
                _lr.IsDoubleFactor = Is;
                _session.SetObjectAsJson(SessionKeys.LoginResponse, _lr);
            }
            return Json(_res);
        }

        #region UserWiseQRByAdmin
        [HttpGet]
        public IActionResult UserUPIQRCodeByAdmin(int a, int uid, string uName)
        {

            IUPIPaymentML paymentML = new PaymentGatewayML(_accessor, _env);
            var res = paymentML.GetUPIQR(_lr.LoginTypeID, uid, a);
            if (res.Statuscode == ErrorCodes.One && !string.IsNullOrEmpty(res.CommonStr4))
            {
                var qRCodeGenerator = new QRCodeGenerator();
                QRCodeData QCD = qRCodeGenerator.CreateQrCode(res.CommonStr4, QRCodeGenerator.ECCLevel.Q);
                IUserWebsite _userWebsite = new UserML(_accessor, _env);
                var model = _userWebsite.GetCompanyProfileUser(1);
                QRCode qRCode = new QRCode(QCD);
                Bitmap b = new Bitmap(1050, 2100);
                Graphics g = Graphics.FromImage(b);
                g.DrawRectangle(new Pen(Color.Black, 3), 5, 5, b.Width - 12, b.Height - 12);
                Image imgLogo = (Image)(new Bitmap(Image.FromFile(DOCType.LogoSuffix.Replace("{0}", "1")), new Size(450, 175)));
                Image imgUPI = Image.FromFile(@"Image/QRImg/bhim_upi.jpg");
                Image imgAPP = Image.FromFile(@"Image/QRImg/app_logo.jpg");
                g.DrawImage(imgLogo, new Point(((b.Width - imgLogo.Width) / 2), 40));
                var str = "All in ONE BHIM UPI QR";
                var msW = g.MeasureString(str, new Font("Arial", 36));
                g.DrawString(str, new Font("Arial", 36), Brushes.Red, new Point(((b.Width - Convert.ToInt16(msW.Width)) / 2), (imgLogo.Height + 75)));
                str = (uName ?? "User QR Code");
                msW = g.MeasureString(str, new Font("Arial", 36));
                g.DrawString(str, new Font("Arial", 36), Brushes.Black, new Point(((b.Width - Convert.ToInt16(msW.Width)) / 2), 325));
                g.DrawImage(qRCode.GetGraphic(19), new Point(((b.Width - qRCode.GetGraphic(19).Width) / 2), 450));
                str = "Scan & Pay with any BHIM UPI app";
                msW = g.MeasureString(str, new Font("Arial", 36));
                g.DrawString(str, new Font("Arial", 36), Brushes.Black, new Point(((b.Width - Convert.ToInt16(msW.Width)) / 2), 1500));
                g.DrawImage(imgUPI, new Point(125, 1600));
                g.DrawImage(imgAPP, new Point(60, 1800));
                str = ("Customer Helpline : " + (model.CustomerCareMobileNos ?? string.Empty));
                msW = g.MeasureString(str, new Font("Arial", 32));
                g.DrawString(str, new Font("Arial", 32), Brushes.Black, new Point(((b.Width - Convert.ToInt16(msW.Width)) / 2), 2025));
                return File(ConverterHelper.BitmapToBytesCode(b), "image/png");
            }
            else
            {
                string msg = "";// res.Statuscode == ErrorCodes.One ? "QR Data Not Found" : res.Msg;
                Bitmap b = new Bitmap(500, 500);
                Graphics g = Graphics.FromImage(b);
                g.DrawString(msg, new Font("Arial", 36), Brushes.Red, new Point(10, 10));
                return File(ConverterHelper.BitmapToBytesCode(b), "image/png");
            }
        }

        [HttpPost]
        [Route("Admin/UserVADetailByAdmin")]
        [Route("UserVADetailByAdmin")]
        public IActionResult UserVADetailByAdmin(int uid)
        {

            if (_lr.RoleID == Role.Admin)
            {
                IUPIPaymentML paymentML = new PaymentGatewayML(_accessor, _env);
                var res = paymentML.GetUPIQRBankDetail(_lr.LoginTypeID, uid);
                return Json(res);
            }
            return Ok();
        }
        #endregion
        [HttpGet]
        [Route("Home/EditUser/{id}")]
        [Route("EditUser/{id}")]
        public IActionResult EditUser(int id)
        {
            IUserML userML = new UserML(_accessor, _env);
            var userData = userML.GetEditUser(id);
            var DMRModels = userML.GetDMRModelList();
            if (DMRModels.Any())
            {
                userData.DMRModelSelect = new SelectList(DMRModels, "ID", "Name");
            }
            IBankML bankML = new BankML(_accessor, _env);
            var banks = bankML.BankMasters();
            if (banks.Any())
            {
                userData.Bankselect = new SelectList(banks, "BankName", "BankName");
            }
            userData.IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.EditUser);
            userData.IsLoginWebsite = _lr.IsWebsite;
            return View("UserEdit/EditUser", userData);
        }

        [HttpPost]
        [Route("Admin/UpdateFlatComm")]
        [Route("UpdateFlatComm")]
        public IActionResult UpdateFlatComm(decimal comm, int UserID)
        {
            IUserML userML = new UserML(_accessor, _env);
            return Json(userML.UpdateFlatComm(comm, UserID));
        }

        [HttpPost]
        [Route("Admin/_partialUpdate")]
        [Route("_partialUpdate")]
        public IActionResult _partialUpdate(UserCreate param)
        {
            IUserML userML = new UserML(_accessor, _env);
            return Json(userML.PartialUpdate(param));
        }
        [HttpPost]
        [Route("Home/EditUser")]
        [Route("EditUser")]
        public IActionResult UpdateUser([FromBody] GetEditUser UserData)
        {
            IUserML userML = new UserML(_accessor, _env);
            return Json(userML.UpdateUser(UserData));
        }
        [HttpGet]
        [Route("Admin/FOSList")]
        [Route("FOSList")]
        public IActionResult FOSList()
        {
            IUserML userML = new UserML(_accessor, _env);
            var Roles = userML.GetRoleSlab().Roles;
            var userListModel = new UserListModel
            {
                IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser,
                selectListItems = new SelectList(Roles, "ID", "Role"),
                userBalnace = userML.GetUserBalnace(0),
                SelfMobileNo = _lr.MobileNo
            };
            return View("UserList/FOSList", userListModel);
        }
        [HttpPost]
        [Route("Admin/FOS_List")]
        [Route("FOS_List")]
        public IActionResult _FOSList([FromBody] CommonFilter f)
        {
            IUserML userML = new UserML(_accessor, _env);
            f.IsFOSListAdmin = true;
            var res = userML.GetFOSList(f);
            res.userReports = res.userReports.Where(w => w.RoleID == FixedRole.FOS).ToList();
            var userListModel = new UserListModel
            {
                IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser,
                userList = res,
                userBalnace = userML.GetUserBalnace(0)
            };
            return PartialView("UserList/Partial/_FOSList", userListModel);
        }
        [HttpGet]
        [Route("Home/EditFOS/{id}")]
        [Route("EditFOS/{id}")]
        public IActionResult EditFOS(int id)
        {
            IUserML userML = new UserML(_accessor, _env);
            var userData = userML.GetEditUser(id);
            userData.IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.EditUser);
            userData.IsLoginWebsite = _lr.IsWebsite;
            return View("UserEdit/EditFOS", userData);
        }
        [HttpPost]
        [Route("Home/EditFOS")]
        [Route("EditFOS")]
        public IActionResult EditFOS([FromBody] GetEditUser UserData)
        {
            IUserML userML = new UserML(_accessor, _env);
            return Json(userML.UpdateFOS(UserData));
        }
        [HttpPost]
        [Route("Admin/chng-fos-colsts")]
        [Route("chng-fos-colsts")]
        public IActionResult ChangeFOSColSts(int ID)
        {
            IUserML userML = new UserML(_accessor, _env);
            IResponseStatus _res = userML.ChangeFOSColSts(ID);
            return Json(_res);
        }
        #endregion
        #region UserRelated
        [HttpPost]
        [Route("Home/UserInfo")]
        [Route("UserInfo")]
        public IActionResult UserInfo(string MobileNo)
        {
            IUserML uml = new UserML(_accessor, _env);
            var userInfo = uml.GetUser(MobileNo);
            return Json(new { userInfo.Name, userInfo.OutletName, ID = userInfo.UserID, userInfo.RoleID, userInfo.IsDenominationSwtichBlock, userInfo.blockDetails });
        }
        [HttpPost]
        [Route("Home/ch-denomswitch-sts")]
        [Route("ch-denomswitch-sts")]
        public IActionResult ChangeDenomSwitchStatus(int u, int s)
        {
            ISwitchingML sml = new SwitchingML(_accessor, _env);
            var res = sml.BlockUsersForSwitching(u, s);
            return Json(res);
        }

        [HttpGet]
        [Route("ProfileUser")]
        public IActionResult ProfileUser()
        {
            IUserML userML = new UserML(_accessor, _env);
            var userData = userML.GetEditUser(_lr.UserID);
            IBankML bankML = new BankML(_accessor, _env);
            var banks = bankML.BankMasters();
            var CompanyType = userML.GetCompanyTypeMaster(new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID
            });
            if (banks.Any())
            {
                userData.Bankselect = new SelectList(banks, "BankName", "BankName");
            }
            if (CompanyType.Any())
            {
                userData.CompanyTypeSelect = new SelectList(CompanyType, "ID", "CompanyName");
            }

            return View("UserEdit/UpdateProfile", userData);
        }
        [HttpPost]
        [Route("/upload-Profile")]
        public IActionResult UploadProfile(IFormFile file)
        {
            IResourceML ml = new ResourceML(_accessor, _env);
            var res = ml.UploadProfile(file, _lr.UserID, LoginType.ApplicationUser);
            return Json(res);
        }
        [HttpPost]
        [Route("/get-ekyc-detail")]
        public IActionResult GetEKYCDetail()
        {
            IEKYCML userML = new EKYCML(_accessor, _env);
            var details = userML.GetEKYCDetailOfUser(new CommonReq
            {
                LoginID = _lr.UserID
            });
            if (!string.IsNullOrEmpty(details.GSTAuthorizedSignatory))
            {
                if (details.GSTAuthorizedSignatory.Split(',').Length > 0)
                {
                    details.Directors = new SelectList(details.GSTAuthorizedSignatory.Split(',').Select(x => new { value = x.Trim() }), "value", "value");
                }
            }
            IBankML bankML = new BankML(_accessor, _env);
            var banks = bankML.DMRBanks();
            if (banks != null)
            {
                details.BankList = new SelectList(banks, "ID", "BankName");
            }
            UserML userML1 = new UserML(_accessor, _env, false);
            var CompanyType = userML1.GetCompanyTypeMaster(new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID
            });
            if (CompanyType.Any())
            {
                details.CompanyTypeSelect = new SelectList(CompanyType, "ID", "CompanyName");
            }
            return PartialView("Partial/_EKYCUserInterface", details);
        }
        [HttpPost]
        [Route("validate-gstin")]
        public IActionResult ValidateGSTINForEKYC(string GSTIN, bool IsConcent, bool IsSkip, int CompanyType)
        {
            if (IsConcent == false && IsSkip == false)
            {
                return Json(new ResponseStatus { Statuscode = ErrorCodes.Minus1, Msg = "Please select consent" });
            }
            IEKYCML ml = new EKYCML(_accessor, _env);
            var res = ml.ValidateGST(new EKYCRequestModel
            {
                UserID = _lr.UserID,
                VerificationAccount = GSTIN,
                IsSkip = IsSkip,
                CompanyTypeID= CompanyType//In app also
            });
            return Json(res);
        }

        [HttpPost]
        [Route("validate-aadhar")]
        public IActionResult ValidateAadharForEKYC(string Aadhar, bool IsConcent, string Director)
        {
            if (IsConcent == false)
            {
                return Json(new ResponseStatus { Statuscode = ErrorCodes.Minus1, Msg = "Please select consent" });
            }
            IEKYCML ml = new EKYCML(_accessor, _env);
            var res = ml.GenerateAadharOTP(new EKYCRequestModel
            {
                UserID = _lr.UserID,
                VerificationAccount = Aadhar,
                DirectorName = Director
            });
            return PartialView("Partial/_EKYCAadharOTP", res);
        }

        [HttpPost]
        [Route("validate-aadhar-otp")]
        public IActionResult ValidateAadharOTPForEKYC(string OTP, int ReffID)
        {
            IEKYCML ml = new EKYCML(_accessor, _env);
            var res = ml.ValidateAadharOTP(new EKYCRequestModel
            {
                UserID = _lr.UserID,
                OTP = OTP,
                ReferenceID = ReffID
            });
            return Json(res);
        }
        [HttpPost]
        [Route("validate-pan")]
        public IActionResult ValidatePAN(string PAN, bool IsConcent, string Director)
        {
            if (IsConcent == false)
            {
                return Json(new ResponseStatus { Statuscode = ErrorCodes.Minus1, Msg = "Please select consent" });
            }
            IEKYCML ml = new EKYCML(_accessor, _env);
            var res = ml.GetPanDetail(new EKYCRequestModel
            {
                UserID = _lr.UserID,
                VerificationAccount = PAN,
                DirectorName = Director
            });
            return Json(res);
        }
        [HttpPost]
        [Route("validate-bank-account")]
        public IActionResult ValidateBankAccount(int BankID, string AccounNo, string IFSC, bool IsConcent)
        {
            if (IsConcent == false)
            {
                return Json(new ResponseStatus { Statuscode = ErrorCodes.Minus1, Msg = "Please select consent" });
            }
            IEKYCML ml = new EKYCML(_accessor, _env);
            var res = ml.GetBankAccountDetail(new EKYCRequestModel
            {
                UserID = _lr.UserID,
                VerificationAccount = AccounNo,
                IFSC = IFSC,
                BankID = BankID,
                RequestMode = RequestMode.PANEL
            });
            return Json(res);
        }
        [HttpPost]
        [Route("editekyc-step")]
        public IActionResult EditEKYCStep(int StepID)
        {
            IEKYCML ml = new EKYCML(_accessor, _env);
            var res = ml.EditEKYCStep(new EKYCRequestModel
            {
                UserID = _lr.UserID,
                EditStepID = StepID
            });
            return Json(res);
        }
        [HttpPost]
        [Route("/_LowBalance-Setting")]
        public IActionResult _LowBalanceSetting()
        {
            IUserML ml = new UserML(_accessor, _env);
            var _res = ml.GetSetting();
            return PartialView("UserEdit/_LowBalanceSetting", _res);
        }

        [HttpPost]
        [Route("/_Save-LowBalance-Setting")]
        public IActionResult _SaveLowBalanceSetting(LowBalanceSetting setting)
        {
            IUserML ml = new UserML(_accessor, _env);
            var _res = ml.SaveLowBalanceSetting(setting);
            return Json(_res);
        }

        [HttpPost]
        [Route("UpdateProfile")]
        public IActionResult UpdateProfile([FromBody] GetEditUser UserData)
        {
            if (!string.IsNullOrEmpty(UserData.EmailID))
            {
                UserData.EmailID = UserData.EmailID.ToLower();
                UserData.CustomLoginID = _lr.CustomLoginID;
                UserData.UserID = _lr.UserID;
            }
            IUserML userML = new UserML(_accessor, _env);
            var res = userML.UpdateUser(UserData);
            if (res.CommonInt == ErrorCodes.One)
                return PartialView("OTP/_OTP");
            return Json(res);

        }
        [HttpPost]
        [Route("GetUpperRole")]
        public IActionResult GetUpperRole(int ID)
        {
            IUserML userML = new UserML(_accessor, _env);
            return PartialView("UserList/Partial/_UserRole", userML.GetUpperRole(ID));
        }
        #region UpdateProfileAfterKYCApproved
        [HttpPost]
        [Route("/_UpdateBankRequest")]
        public IActionResult _UpdateBankRequest()
        {
            IUserML userML = new UserML(_accessor, _env);
            var userData = userML.GetEditUser(_lr.UserID);
            IBankML bankML = new BankML(_accessor, _env);
            var banks = bankML.BankMasters();
            if (banks.Any())
            {
                userData.Bankselect = new SelectList(banks, "BankName", "BankName");
            }
            return PartialView("UserEdit/_UpdateBankRequest", userData);
        }
        [HttpPost]
        [Route("/_SubmitUpdatebankRequest")]
        public IActionResult _SubmitUpdatebankRequest(GetEditUser UserData)
        {
            IUserML userML = new UserML(_accessor, _env);
            IResponseStatus _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvaildSession
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID != Role.Admin)
            {
                UserData.UserID = _lr.UserID;
                UserData.LoginID = _lr.UserID;
                UserData.LT = _lr.LoginTypeID;
                UserData.MobileNo = _lr.MobileNo;
            }
            _res = userML.UserBankRequest(UserData);
            return Json(_res);
        }
        #endregion
        #endregion
        #region AgreementApproval
        [HttpGet]
        [Route("AgreementApproval")]
        public IActionResult AgreementApproval()
        {
            if(ApplicationSetting.IsAgreementApproval)
                return View("AgreementApproval");
            return Ok("Requested path is not exists.");
        }

        [HttpPost]
        [Route("Admin/aggr-appr")]
        [Route("aggr-appr")]
        public IActionResult _AgreementApproval([FromBody] AAUserReq aAUserReq)
        {
            IUserML userML = new UserML(_accessor, _env);
            var res = userML.GetUserAggrement(aAUserReq);
            return PartialView("Partial/_AgreementApproval", res);
        }
        [HttpPost]
        [Route("Admin/update-aggr-appr")]
        [Route("update-aggr-appr")]
        public IActionResult _AgreementApproval(UpdateUserReq updateUserReq)
        {
            IUserML userML = new UserML(_accessor, _env);
            var res = userML.UpdateUAA(updateUserReq);
            return Json(res);
        }
        #endregion


        [HttpPost]
        [Route("Admin/ToggleCandebitStatus")]
        [Route("ToggleCandebitStatus")]
        public IActionResult ToggleCandebitStatus(int ID, bool Is)
        {
            IUserML userML = new UserML(_accessor, _env);
            IResponseStatus _res = userML.ToggleCandebitStatus(ID, Is);
            return Json(_res);
        }

        [HttpPost]
        [Route("Admin/ToggleCandebitDownLineStatus")]
        [Route("ToggleCandebitDownLineStatus")]
        public IActionResult ToggleCandebitDownLineStatus(int ID, bool Is)
        {
            IUserML userML = new UserML(_accessor, _env);
            IResponseStatus _res = userML.ToggleCandebitDownLineStatus(ID, Is);
            return Json(_res);
        }

        [HttpGet]
        [Route("DebitFundRequest")]
        public IActionResult DebitFundRequest()
        {

            return View();
        }
        [HttpPost]
        [Route("DebitFundRequestList")]
        public IActionResult DebitFundRequestList([FromBody] DebitFundrequest data)
        {
            IUserML userML = new UserML(_accessor, _env);

            var res = userML.DebitFundRequest(data);
            return PartialView("Partial/_DebitFundRequestList", res);
        }

        [HttpPost]
        [Route("DebitRquesttStatus")]
        public IActionResult _DebitFundRequestList(int ID, int Status, bool IsMarkPaid, string Remark)
        {
            IUserML userML = new UserML(_accessor, _env);
            IResponseStatus _res = userML.DebitRquesttStatus(ID, Status, IsMarkPaid, Remark);
            return Json(_res);
        }


        [HttpGet]
        [Route("ExportDebitFundRequest")]
        public async Task<IActionResult> ExportDebitFundRequest(string Status, int Criteria, string CriteriaText, string CommonInt2, string Todate, string FromDate, int TopRows)
        {
            var IsAdmin = _lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser;
            ICoupanVoucherML coupanVoucherML = new CoupanVoucherML(_accessor, _env);

            UserML userML = new UserML(_accessor, _env);
            var data = new DebitFundrequest() { Status = Status, Criteria = Criteria, CriteriaText = CriteriaText ?? "", FromDate = FromDate, TopRows = TopRows };
            data.CriteriaText = CriteriaText; data.ToDate = Todate;
            var res = userML.DebitFundRequest(data);
            List<DebitFundrequestExl> req = new List<DebitFundrequestExl>();

            foreach (var itm in res)
            {

                itm.Status = itm.Status == "1" ? "Requested" : itm.Status == "2" ? "Accepted" : "Rejected";

                req.Add(new DebitFundrequestExl { User = itm.FromName, From_Name = itm.ToName, Amount = itm.Amount, Status = itm.Status, RequetedDate = itm.RequetedDate, Remark = itm.Remark, IsAccountStatmentEntry = itm.IsAccountStatmentEntry });
            }
            DataTable dataTable = ConverterHelper.O.ToDataTable(req);
            //var removableCol = new List<string> { "ID", "WalletID", "UserID", "LT","LoginID", "EmailID", "EmployeeRole", "Name", "IsDesc", "IsFosListAdmin", "btnID", "Criteria", "CriteriaText", "MapStatus", "FosID", "FromDate", "ToDate" , "RequestMode", "IsAdminDefined","TopRows","SortByID","LoginRoleID","RoleID" };

            //foreach (string str in removableCol)
            //{
            //    dataTable.Columns.Remove(str);
            //}
            using (var package = new OfficeOpenXml.ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Debit Request");
                worksheet.Cells["A1"].LoadFromDataTable(dataTable, PrintHeaders: true);
                worksheet.Row(1).Height = 20;
                worksheet.Row(1).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Row(1).Style.Font.Bold = true;
                for (var col = 1; col < dataTable.Columns.Count + 1; col++)
                {
                    worksheet.Column(col).AutoFit();
                }
                var exportToExcel = new AppCode.Model.Report.ExportToExcel
                {
                    Contents = package.GetAsByteArray(),
                    FileName = "DebitFundRequest.xlsx"
                };
                return File(exportToExcel.Contents, DOCType.XlsxContentType, exportToExcel.FileName);
            }
        }




    }
}
