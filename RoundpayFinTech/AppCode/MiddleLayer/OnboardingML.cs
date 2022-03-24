using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Fintech.AppCode.WebRequest;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OfficeOpenXml.ConditionalFormatting;
using RoundpayFinTech.AppCode.Configuration;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer.Dmt_Api;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.StaticModel.MoneyTransfer;
using RoundpayFinTech.AppCode.ThirdParty.Eko;
using RoundpayFinTech.AppCode.ThirdParty.Fingpay;
using RoundpayFinTech.AppCode.ThirdParty.Mahagram;
using RoundpayFinTech.AppCode.ThirdParty.PanMitra;
using RoundpayFinTech.AppCode.ThirdParty.Roundpay;
using RoundpayFinTech.AppCode.ThirdParty.YesBank;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public class OnboardingML : IOnboardingML, IPSATransaction, IAPISetting
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration Configuration;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        public OnboardingML(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile((_env.IsProduction() ? "appsettings.json" : "appsettings.Development.json"));
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }
        public string DecodeJWT(string _APICode, string EncodeData)
        {
            if (_APICode == APICode.SPRINT)
            {
                SprintBBPSML sML = new SprintBBPSML(_accessor, _env, _dal);
                return sML.DecryptToken(EncodeData);
            }
            return string.Empty;
        }

        public void CallUpdateFromCallBack(OutletAPIStatusUpdateReq apiResp)
        {
            bool IsSave = false;
            if (apiResp._APICode == APICode.SPRINT)
            {
                SprintBBPSML sML = new SprintBBPSML(_accessor, _env, _dal);
                IsSave = sML.IsParnerIDIsValid(apiResp.MyPartnerIDInAPI);
            }
            if (IsSave)
            {
                IProcedure _proc = new ProcOutletAPIWiseCU(_dal);
                _proc.Call(apiResp);
            }

        }
        private APIAppSetting GetApiAppSetting(string _APICode)
        {
            var res = new APIAppSetting();
            if (!string.IsNullOrEmpty(_APICode) && !string.IsNullOrEmpty(_APICode))
            {
                string SERVICESETTING = "SERVICESETTING:" + _APICode;
                string OnboardingSetting = SERVICESETTING + ":OnboardingSetting";
                try
                {
                    res.onboardingSetting = new OnboardingSetting
                    {
                        BaseURL = Configuration[OnboardingSetting + ":BaseURL"],
                        Token = Configuration[OnboardingSetting + ":Token"]
                    };
                }

                catch (Exception ex)
                {
                    var errorLog = new ErrorLog
                    {
                        ClassName = GetType().Name,
                        FuncName = "GetApiAppSetting",
                        Error = "Exception:APICode=" + (_APICode ?? string.Empty) + "[" + ex.Message,
                    };
                    var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                }
            }
            return res;
        }
        private APIAppSetting GetApiPSAAppSetting(string _APICode)
        {
            var res = new APIAppSetting();
            if (!string.IsNullOrEmpty(_APICode) && !string.IsNullOrEmpty(_APICode))
            {
                string SERVICESETTING = "SERVICESETTING:" + _APICode;
                var OnboardingSetting = SERVICESETTING + ":PSA";
                try
                {
                    res.onboardingSetting = new OnboardingSetting
                    {
                        BaseURL = Configuration[OnboardingSetting + ":BaseURL"],
                        Token = Configuration[OnboardingSetting + ":Token"]
                    };
                }

                catch (Exception ex)
                {
                    var errorLog = new ErrorLog
                    {
                        ClassName = GetType().Name,
                        FuncName = "GetApiAppSetting",
                        Error = "Exception:APICode=" + (_APICode ?? string.Empty) + ",SCode=PSA [" + ex.Message,
                    };
                    var _ = new ProcPageErrorLog(_dal).Call(errorLog);

                }
            }
            return res;
        }
        public FingpayAPISetting GetFingpay()
        {
            var res = new FingpayAPISetting();
            string SERVICESETTING = "SERVICESETTING:FINGPAY";
            string OnboardingSetting = SERVICESETTING + ":AEP";
            try
            {                                                                                                                                                                                                                                                                       
                res.MERCHANTName = Configuration[OnboardingSetting + ":MERCHANTName"];
                res.MerchantPin = Configuration[OnboardingSetting + ":MerchantPin"];
                res.superMerchantId = Configuration[OnboardingSetting + ":superMerchantId"];
                res.secretKey = Configuration[OnboardingSetting + ":secretKey"];
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GetApiAppSetting",
                    Error = "Exception:APICode=FINGPAY,SCode=AEP [" + ex.Message,
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);

            }
            return res;
        }
        public SprintJsonSettings GetSprintSetting()
        {
            SprintBBPSML sprintBBPSML = new SprintBBPSML(_accessor, _env, _dal);
            return sprintBBPSML.GetSetting();
        }
        public ResponseStatus DoTwowayAuthentication(ValidateAPIOutletResp _ValidateAPIOutletResp)
        {
            var outletModelResp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            //TwoFactorAuthentication
            if (_ValidateAPIOutletResp.APICode == APICode.FINGPAY)
            {
                var fingPayML = new FingpayML(_dal);
                var fingPaySetting = GetFingpay();
                var apiResp = fingPayML.TwoFactorAuthentication(_ValidateAPIOutletResp, fingPaySetting);
                outletModelResp.Statuscode = apiResp.Statuscode;
                outletModelResp.Msg = apiResp.Msg;
                if (outletModelResp.Statuscode == ErrorCodes.One)
                {
                    UpdateAPITwoway(_ValidateAPIOutletResp.OutletID, _ValidateAPIOutletResp.APIOutletID, _ValidateAPIOutletResp.APIID);
                }
            }
            return outletModelResp;
        }
        public GenerateOTPModel CallGenerateOTP(ValidateAPIOutletResp _ValidateAPIOutletResp)
        {
            var res = new GenerateOTPModel
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
           
            var apiResp = new OutletAPIStatusUpdate();
            if (_ValidateAPIOutletResp.APICode == APICode.INSTANTPAY)
            {
                var instPay = new InstantPayUserOnboarding(_accessor, _env, _ValidateAPIOutletResp.APICode, _ValidateAPIOutletResp.APIID, _dal);
                apiResp = instPay.SignupInitiate(_ValidateAPIOutletResp);
            }
            else if (_ValidateAPIOutletResp.APICode == APICode.FINGPAY)
            {
                var fingPayML = new FingpayML(_dal);
                var fingPaySetting = GetFingpay();
                apiResp = fingPayML.SendOTP(_ValidateAPIOutletResp, fingPaySetting);
            }
            if (apiResp.Statuscode != 0)
            {
                res.Statuscode = apiResp.Statuscode;
                res.Msg = apiResp.Msg;
                if (apiResp.IsEKYCAlreadyVerified)
                {
                    UpdateEKYCStatus(_ValidateAPIOutletResp.OutletID, _ValidateAPIOutletResp.APIOutletID, _ValidateAPIOutletResp.APIID, UserStatus.ACTIVE);
                }
                else if (apiResp.Statuscode == ErrorCodes.One)
                {
                    IProcedure procedure = new ProcAadharOTPReference(_dal);
                    var refResp = (ResponseStatus)procedure.Call(new CommonReq
                    {
                        CommonStr = _ValidateAPIOutletResp.AADHAR,
                        CommonInt = _ValidateAPIOutletResp.APIID,
                        CommonStr2 = apiResp.APIReferenceID,
                        CommonStr3 = _ValidateAPIOutletResp.OutletName,
                        CommonStr4 = apiResp.APIHash
                    });
                    res.OTPRefID = refResp.CommonInt;
                    res.IsOTPRequired = apiResp.IsOTPRequired;
                }
            }

            return res;
        }
        public GenerateOTPModel CallValidateOTP(ValidateAPIOutletResp _ValidateAPIOutletResp)
        {
            var res = new GenerateOTPModel
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            ProcAadharOTPReference procAadharOTPReference = new ProcAadharOTPReference(_dal);
            var otpRefResp = procAadharOTPReference.GetAadharRefrence(_ValidateAPIOutletResp.OTPRefID);
            if (string.IsNullOrEmpty(otpRefResp.CommonStr))
            {
                res.Statuscode = ErrorCodes.Minus1;
                res.Msg = "Invalid OTP Refrence";
                return res;
            }
            _ValidateAPIOutletResp.APIReferenceID = otpRefResp.CommonStr;
            _ValidateAPIOutletResp.APIHash = otpRefResp.CommonStr4;
            var IsCallOnboarding = false;
            var apiResp = new OutletAPIStatusUpdate();
            if (_ValidateAPIOutletResp.APICode == APICode.INSTANTPAY)
            {
                //Validate OTP and insert in outlet api wise
                var instPay = new InstantPayUserOnboarding(_accessor, _env, _ValidateAPIOutletResp.APICode, _ValidateAPIOutletResp.APIID, _dal);
                apiResp = instPay.SignupValidate(_ValidateAPIOutletResp);
                IsCallOnboarding = apiResp.IsOnboardedOnAPI;
            }
            else if (_ValidateAPIOutletResp.APICode == APICode.FINGPAY)
            {
                var fingPayML = new FingpayML(_dal);
                var fingPaySetting = GetFingpay();
                apiResp = fingPayML.ValidateOTP(_ValidateAPIOutletResp, fingPaySetting);
            }
            if (apiResp.Statuscode != 0)
            {

                res.Statuscode = apiResp.Statuscode;
                res.Msg = apiResp.Msg;
                if (apiResp.Statuscode == ErrorCodes.One)
                {
                    res.OTPRefID = _ValidateAPIOutletResp.OTPRefID;
                    res.IsOTPRequired = apiResp.IsOTPRequired;
                    res.IsBioMetricRequired = apiResp.IsBioMetricRequired;
                    if (IsCallOnboarding == true)
                    {
                        //save apidata for onboarding
                        var procReq = new OutletAPIStatusUpdateReq();
                        procReq.UserID = _ValidateAPIOutletResp.UserID;
                        procReq.OutletID = _ValidateAPIOutletResp.OutletID;
                        procReq.APIID = _ValidateAPIOutletResp.APIID;
                        procReq.APIOutletID = apiResp.APIOutletID;
                        procReq.APIOutletStatus = apiResp.APIOutletStatus;
                        procReq.KYCStatus = apiResp.APIOutletStatus;
                        procReq.IsVerifyStatusUpdate = true;
                        procReq.IsDocVerifyStatusUpdate = true;
                        procReq.AEPSID = apiResp.AEPSID;
                        procReq.AEPSStatus = apiResp.AEPSStatus;
                        procReq.IsAEPSUpdate = true;
                        procReq.IsAEPSUpdateStatus = true;
                        //procReq.BBPSID = apiResp.BBPSID;
                        //procReq.BBPSStatus = apiResp.BBPSStatus;
                        //procReq.IsBBPSUpdate = true;
                        //procReq.IsBBPSUpdateStatus = true;
                        procReq.DMTID = apiResp.DMTID;
                        procReq.DMTStatus = apiResp.DMTStatus;
                        procReq.IsDMTUpdate = true;
                        procReq.IsDMTUpdateStatus = true;
                       

                        res.Statuscode = ErrorCodes.One;
                        res.Msg = ErrorCodes.OutletRegistered;
                        res.ErrorCode = ErrorCodes.Transaction_Successful;
                        IProcedure _proc = new ProcOutletAPIWiseCU(_dal);
                        _proc.Call(procReq);
                    }
                }
            }
            return res;
        }
        public ResponseStatus CallValidateBiometric(ValidateAPIOutletResp _ValidateAPIOutletResp)
        {
            var outletModelResp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            ProcAadharOTPReference procAadharOTPReference = new ProcAadharOTPReference(_dal);
            var otpRefResp = procAadharOTPReference.GetAadharRefrence(_ValidateAPIOutletResp.OTPRefID);
            if (string.IsNullOrEmpty(otpRefResp.CommonStr))
            {
                outletModelResp.Statuscode = ErrorCodes.Minus1;
                outletModelResp.Msg = "Invalid Biometric Refrence";
                return outletModelResp;
            }
            _ValidateAPIOutletResp.APIReferenceID = otpRefResp.CommonStr;
            _ValidateAPIOutletResp.APIHash = otpRefResp.CommonStr4;
            var apiResp = new OutletAPIStatusUpdate();
            if (_ValidateAPIOutletResp.APICode == APICode.FINGPAY)
            {
                var fingPayML = new FingpayML(_dal);
                var fingPaySetting = GetFingpay();
                apiResp = fingPayML.BioMetricEKYCRequest(_ValidateAPIOutletResp, fingPaySetting);
                outletModelResp.Statuscode = apiResp.Statuscode;
                outletModelResp.Msg = apiResp.Msg;
                if (apiResp.Statuscode == ErrorCodes.One)
                {
                    UpdateEKYCStatus(_ValidateAPIOutletResp.OutletID, _ValidateAPIOutletResp.APIOutletID, _ValidateAPIOutletResp.APIID, UserStatus.ACTIVE);
                }
            }
            return outletModelResp;
        }
        private void UpdateEKYCStatus(int OutletID, string APIOutletID, int APIID, int EKYCStatus)
        {
            IProcedure proc = new ProcUpdateAPIEKYCStatus(_dal);
            proc.Call(new CommonReq
            {
                UserID = OutletID,
                CommonStr = APIOutletID,
                CommonInt = EKYCStatus,
                CommonInt2 = APIID
            });
        }
        //ProcUpdateAPITwoWay
        private void UpdateAPITwoway(int OutletID, string APIOutletID, int APIID)
        {
            IProcedure proc = new ProcUpdateAPITwoWay(_dal);
            proc.Call(new CommonReq
            {
                UserID = OutletID,
                CommonStr = APIOutletID,
                CommonInt2 = APIID
            });
        }

        public IResponseStatus CallOnboarding(OutletServicePlusReqModel MDL)
        {
            var outletModelResp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var IsSaveOnboarding = false;
            var procReq = new OutletAPIStatusUpdateReq();
            if (MDL != null)
            {
                if (MDL._ValidateAPIOutletResp != null)
                {
                    var outletAPIData = MDL._ValidateAPIOutletResp;

                    var appSetting = GetApiAppSetting(outletAPIData.APICode);
                    outletAPIData.Name = ConverterHelper.O.ReplaceSpaceWithSingle(outletAPIData.Name);
                    string FirstName = string.Empty, MiddleName = string.Empty, LastName = string.Empty;
                    var NameBreakUp = outletAPIData.Name.Split(' ');
                    FirstName = NameBreakUp[0];
                    if (NameBreakUp.Length == 4) {
                        MiddleName = NameBreakUp[1]+" "+ NameBreakUp[2];
                        LastName = NameBreakUp[3];
                    }
                    else if (NameBreakUp.Length == 3)
                    {
                        MiddleName = NameBreakUp[1];
                        LastName = NameBreakUp[2];
                    }
                    else
                    {
                        LastName = NameBreakUp.Length == 2 ? NameBreakUp[1] : FirstName;
                    }
                    if (MDL._ValidateAPIOutletResp.APICode == APICode.ROUNDPAY)
                    {
                        var onboardingKyc = GetKYCURL(outletAPIData.UserID, outletAPIData.OutletID);
                        if (!File.Exists(onboardingKyc.ShopPhoto ?? string.Empty) || !File.Exists(onboardingKyc.Adhaar ?? string.Empty) || !File.Exists(onboardingKyc.Pan ?? string.Empty))
                        {
                            outletModelResp.Msg = ErrorCodes.Error212SystemNotSpecified;
                            outletModelResp.ErrorCode = ErrorCodes.Document_Not_Completed;
                            return outletModelResp;
                        }

                        var RNDPAYReq = new RoundpayApiRequestModel
                        {
                            Name = FirstName + (!string.IsNullOrEmpty(MiddleName) ? " " + MiddleName : string.Empty),
                            LastName = LastName,
                            DOB = outletAPIData.DOB,
                            Pincode = outletAPIData.Pincode,
                            Address = outletAPIData.Address,
                            Landmark = outletAPIData.Landmark,
                            Area = outletAPIData.Landmark,
                            Phone1 = outletAPIData.MobileNo,
                            Phone2 = outletAPIData.AlternateMobile,
                            Emailid = outletAPIData.EmailID,
                            Pan = outletAPIData.PAN,
                            PANLink = HttpUtility.UrlEncode(ConverterHelper.O.ConvertImagebase64(onboardingKyc.Pan)),
                            Aadhaar = outletAPIData.AADHAR,
                            AadharLink = HttpUtility.UrlEncode(ConverterHelper.O.ConvertImagebase64(onboardingKyc.Adhaar)),
                            ShopType = outletAPIData.ShopType,
                            ShopLink = HttpUtility.UrlEncode(ConverterHelper.O.ConvertImagebase64(onboardingKyc.ShopPhoto)),
                            Qualification = outletAPIData.Qualification,
                            Poupulation = outletAPIData.Poupulation,
                            Latitude = outletAPIData.Latlong.Contains(",") ? outletAPIData.Latlong.Split(',')[0] : outletAPIData.Latlong,
                            Longitude = outletAPIData.Latlong.Contains(",") ? outletAPIData.Latlong.Split(',')[1] : outletAPIData.Latlong,
                            AreaType = outletAPIData.LocationType,
                            StateId = outletAPIData.StateID,
                            DistrictId = outletAPIData.DistrictID,
                            BaseUrl = appSetting.onboardingSetting.BaseURL,
                            Token = appSetting.onboardingSetting.Token,
                            APIID = outletAPIData.APIID
                        };
                        if (RNDPAYReq.PANLink.Length == 0)
                        {
                            outletModelResp.Msg = ErrorCodes.Error212System.Replace("{DOC}", nameof(DOCType.PAN));
                            outletModelResp.ErrorCode = ErrorCodes.Document_Not_Completed;
                            return outletModelResp;
                        }
                        if (RNDPAYReq.AadharLink.Length == 0)
                        {
                            outletModelResp.Msg = ErrorCodes.Error212System.Replace("{DOC}", nameof(DOCType.AADHAR));
                            outletModelResp.ErrorCode = ErrorCodes.Document_Not_Completed;
                            return outletModelResp;
                        }
                        if (RNDPAYReq.ShopLink.Length == 0)
                        {
                            outletModelResp.Msg = ErrorCodes.Error212System.Replace("{DOC}", nameof(DOCType.ShopImage));
                            outletModelResp.ErrorCode = ErrorCodes.Document_Not_Completed;
                            return outletModelResp;
                        }
                        IRoundpayApiML roundpayApiML = new RoundpayApiML(_accessor, _env);
                        var apiResp = roundpayApiML.OutRegistration(RNDPAYReq);
                        if (apiResp != null)
                        {
                            if (apiResp.Statuscode == ErrorCodes.One)
                            {
                                procReq.UserID = outletAPIData.UserID;
                                procReq.OutletID = outletAPIData.OutletID;
                                procReq.APIID = outletAPIData.APIID;
                                procReq.APIOutletID = apiResp.APIOutletID;
                                procReq.APIOutletStatus = apiResp.APIOutletStatus;
                                procReq.KYCStatus = apiResp.KYCStatus;
                                procReq.BBPSID = apiResp.BBPSID;
                                procReq.BBPSStatus = apiResp.BBPSStatus;
                                procReq.AEPSID = apiResp.AEPSID;
                                procReq.AEPSStatus = apiResp.AEPSStatus;
                                procReq.DMTID = apiResp.DMTID;
                                procReq.DMTStatus = apiResp.DMTStatus;
                                procReq.PSARequestID = apiResp.PSARequestID;
                                procReq.PSAID = apiResp.PSAID;
                                procReq.PSAStatus = apiResp.PSAStatus;
                                IsSaveOnboarding = true;
                                procReq.IsVerifyStatusUpdate = true;
                                procReq.IsDocVerifyStatusUpdate = true;
                                procReq.IsBBPSUpdate = true;
                                procReq.IsBBPSUpdateStatus = true;
                                procReq.IsAEPSUpdate = true;
                                procReq.IsAEPSUpdateStatus = true;
                            }
                            else
                            {
                                outletModelResp.Msg = apiResp.Msg;
                            }
                        }
                    }
                    else if (MDL._ValidateAPIOutletResp.APICode == APICode.MAHAGRAM)
                    {
                        #region Mahagram-AEPS,DMT
                        if (MDL._ValidateAPIOutletResp.SCode.In(ServiceCode.AEPS, ServiceCode.MoneyTransfer, ServiceCode.MiniBank))
                        {
                            var onboardingKyc = GetKYCURL(outletAPIData.UserID, outletAPIData.OutletID);
                            if (!File.Exists(onboardingKyc.ShopPhoto ?? string.Empty) || !File.Exists(onboardingKyc.Adhaar ?? string.Empty) || !File.Exists(onboardingKyc.Pan ?? string.Empty) || !File.Exists(onboardingKyc.PassportPhoto ?? string.Empty))
                            {
                                outletModelResp.Msg = ErrorCodes.Error212SystemNotSpecified;
                                outletModelResp.ErrorCode = ErrorCodes.Document_Not_Completed;
                                return outletModelResp;
                            }
                            var MGReq = new MGOnboardingReq
                            {
                                bc_f_name = FirstName,
                                bc_m_name = MiddleName,
                                bc_l_name = LastName,
                                phone1 = outletAPIData.MobileNo,
                                phone2 = outletAPIData.AlternateMobile,
                                emailid = outletAPIData.EmailID,
                                bc_dob = Convert.ToDateTime(outletAPIData.DOB).ToString("dd-MM-yyyy"),
                                bc_pincode = outletAPIData.Pincode,
                                bc_address = outletAPIData.Address ?? string.Empty,
                                bc_landmark = outletAPIData.Landmark ?? string.Empty,
                                bc_block = outletAPIData.Landmark ?? string.Empty,
                                bc_mohhalla = outletAPIData.Landmark ?? string.Empty,
                                bc_city = outletAPIData.City ?? string.Empty,
                                bc_loc = outletAPIData.Landmark,
                                bc_district = outletAPIData.DistrictID.ToString(),
                                bc_state = outletAPIData.StateID.ToString(),
                                bc_pan = outletAPIData.PAN,
                                shopname = outletAPIData.OutletName,
                                kyc1 = HttpUtility.UrlEncode(ConverterHelper.O.ConvertImagebase64(onboardingKyc.Pan)),
                                kyc2 = HttpUtility.UrlEncode(ConverterHelper.O.ConvertImagebase64(onboardingKyc.Adhaar)),
                                kyc3 = HttpUtility.UrlEncode(ConverterHelper.O.ConvertImagebase64(onboardingKyc.ShopPhoto)),
                                kyc4 = HttpUtility.UrlEncode(ConverterHelper.O.ConvertImagebase64(onboardingKyc.PassportPhoto)),
                                shopType = outletAPIData.ShopType,
                                qualification = outletAPIData.Qualification,
                                population = outletAPIData.Poupulation,
                                locationType = outletAPIData.LocationType,
                                saltkey = string.Empty,
                                secretkey = string.Empty
                            };
                            if ((appSetting.onboardingSetting.Token ?? string.Empty).Contains("&"))
                            {
                                var setSplit = appSetting.onboardingSetting.Token.Split('&');
                                if (setSplit.Length == 3)
                                {
                                    MGReq.saltkey = setSplit[0].Contains("=") ? setSplit[0].Split("=")[1] : string.Empty;
                                    MGReq.secretkey = setSplit[1].Contains("=") ? setSplit[1].Split("=")[1] : string.Empty;
                                }
                            }
                            if (MGReq.kyc1.Length == 0)
                            {
                                outletModelResp.Msg = ErrorCodes.Error212System.Replace("{DOC}", nameof(DOCType.PAN));
                                outletModelResp.ErrorCode = ErrorCodes.Document_Not_Completed;
                                return outletModelResp;
                            }
                            if (MGReq.kyc2.Length == 0)
                            {
                                outletModelResp.Msg = ErrorCodes.Error212System.Replace("{DOC}", nameof(DOCType.AADHAR));
                                outletModelResp.ErrorCode = ErrorCodes.Document_Not_Completed;
                                return outletModelResp;
                            }
                            if (MGReq.kyc3.Length == 0)
                            {
                                outletModelResp.Msg = ErrorCodes.Error212System.Replace("{DOC}", nameof(DOCType.ShopImage));
                                outletModelResp.ErrorCode = ErrorCodes.Document_Not_Completed;
                                return outletModelResp;
                            }
                            if (MGReq.kyc4.Length == 0)
                            {
                                outletModelResp.Msg = ErrorCodes.Error212System.Replace("{DOC}", nameof(DOCType.PHOTO));
                                outletModelResp.ErrorCode = ErrorCodes.Document_Not_Completed;
                                return outletModelResp;
                            }
                            IMahagramAPIML MGAPIML = new MahagramAPIML(_accessor, _env, appSetting.onboardingSetting.BaseURL);
                            var apiResp = MGAPIML.APIBCRegistration(MGReq, outletAPIData.APIID);
                            if (apiResp != null)
                            {
                                if (apiResp.Statuscode == ErrorCodes.One)
                                {
                                    procReq.UserID = outletAPIData.UserID;
                                    procReq.OutletID = outletAPIData.OutletID;
                                    procReq.APIID = outletAPIData.APIID;
                                    procReq.APIOutletID = apiResp.APIOutletID;
                                    procReq.APIOutletStatus = UserStatus.ACTIVE;
                                    procReq.KYCStatus = apiResp.KYCStatus;
                                    procReq.BBPSID = apiResp.BBPSID;
                                    procReq.BBPSStatus = apiResp.BBPSStatus;
                                    procReq.AEPSID = apiResp.AEPSID;
                                    procReq.AEPSStatus = apiResp.AEPSStatus;
                                    procReq.DMTID = apiResp.DMTID;
                                    procReq.DMTStatus = apiResp.DMTStatus;
                                    IsSaveOnboarding = true;
                                    procReq.IsVerifyStatusUpdate = true;
                                    procReq.IsDocVerifyStatusUpdate = true;
                                    procReq.IsBBPSUpdate = true;
                                    procReq.IsBBPSUpdateStatus = true;
                                    procReq.IsAEPSUpdate = true;
                                    procReq.IsAEPSUpdateStatus = true;
                                    procReq.IsDMTUpdate = true;
                                    procReq.IsDMTUpdateStatus = true;
                                }
                                else
                                {
                                    outletModelResp.Msg = apiResp.Msg;
                                }
                            }
                        }
                        #endregion
                        #region Mahagram-PSA
                        if (MDL._ValidateAPIOutletResp.SCode.In(ServiceCode.PSAService))
                        {
                            if (MDL._ValidateAPIOutletResp.PANStatus == UserStatus.NOTREGISTRED)
                            {
                                var MGReq = new MGPSARequest
                                {
                                    psaname = outletAPIData.OutletName,
                                    contactperson = outletAPIData.Name,
                                    location = outletAPIData.City ?? string.Empty,
                                    dob = Convert.ToDateTime(outletAPIData.DOB).ToString("dd MMM yyyy"),
                                    pincode = outletAPIData.Pincode,
                                    state = outletAPIData.StateName,
                                    phone1 = outletAPIData.MobileNo,
                                    phone2 = outletAPIData.AlternateMobile,
                                    emailid = outletAPIData.EmailID,
                                    pan = outletAPIData.PAN,
                                    adhaar = outletAPIData.AADHAR,
                                    udf1 = outletAPIData.UserID.ToString(),
                                    udf2 = outletAPIData.TransactionID,
                                    udf3 = "User Registration",
                                    udf4 = "PSA",
                                    udf5 = string.Empty
                                };
                                appSetting = GetApiPSAAppSetting(outletAPIData.APICode);
                                if ((appSetting.onboardingSetting.Token ?? string.Empty).Contains("&"))
                                {
                                    var setSplit = appSetting.onboardingSetting.Token.Split('&');
                                    if (setSplit.Length == 2)
                                    {
                                        MGReq.securityKey = setSplit[0].Contains("=") ? setSplit[0].Split("=")[1] : string.Empty;
                                        MGReq.createdby = setSplit[1].Contains("=") ? setSplit[1].Split("=")[1] : string.Empty;
                                    }
                                }
                                if (!string.IsNullOrEmpty(MGReq.securityKey) && !string.IsNullOrEmpty(MGReq.createdby))
                                {
                                    IMahagramAPIML MGAPIML = new MahagramAPIML(_accessor, _env, appSetting.onboardingSetting.BaseURL);
                                    var apiResp = MGAPIML.UTIRegistration(MGReq, outletAPIData.APIID);
                                    if (apiResp != null)
                                    {
                                        if (apiResp.Statuscode == ErrorCodes.One)
                                        {
                                            procReq.UserID = outletAPIData.UserID;
                                            procReq.OutletID = outletAPIData.OutletID;
                                            procReq.APIID = outletAPIData.APIID;
                                            procReq.APIOutletID = string.IsNullOrEmpty(outletAPIData.APIOutletID) ? apiResp.PSAID : outletAPIData.APIOutletID;
                                            procReq.APIOutletStatus = UserStatus.ACTIVE;
                                            procReq.KYCStatus = outletAPIData.APIOutletDocVerifyStatus;
                                            procReq.PSAID = apiResp.PSAID;
                                            procReq.PSARequestID = apiResp.PSARequestID;
                                            procReq.PSAStatus = apiResp.PSAStatus;
                                            IsSaveOnboarding = true;
                                            procReq.IsVerifyStatusUpdate = true;
                                            procReq.IsDocVerifyStatusUpdate = true;
                                            procReq.IsPANRequestIDUpdate = true;
                                            procReq.IsPANUpdate = true;
                                            procReq.IsPANUpdateStatus = true;
                                        }
                                        else
                                        {
                                            outletModelResp.Msg = apiResp.Msg;
                                        }
                                    }
                                }
                                else
                                {
                                    outletModelResp.Msg = nameof(ErrorCodes.SystemErrorDown);
                                    outletModelResp.ErrorCode = ErrorCodes.Unknown_Error;
                                }
                            }
                            else
                            {
                                var MGReq = new MGPSARequest
                                {
                                    psaname = outletAPIData.Name,
                                    contactperson = outletAPIData.OutletName,
                                    location = outletAPIData.Address ?? string.Empty,
                                    dob = Convert.ToDateTime(outletAPIData.DOB).ToString("dd MMM yyyy"),
                                    pincode = outletAPIData.Pincode,
                                    state = outletAPIData.StateName,
                                    phone1 = outletAPIData.MobileNo,
                                    phone2 = outletAPIData.AlternateMobile,
                                    emailid = outletAPIData.EmailID,
                                    pan = outletAPIData.PAN,
                                    adhaar = outletAPIData.AADHAR,
                                    psaid = outletAPIData.PANID
                                };
                                appSetting = GetApiPSAAppSetting(outletAPIData.APICode);
                                if ((appSetting.onboardingSetting.Token ?? string.Empty).Contains("&"))
                                {
                                    var setSplit = appSetting.onboardingSetting.Token.Split('&');
                                    if (setSplit.Length == 2)
                                    {
                                        MGReq.securityKey = setSplit[0].Contains("=") ? setSplit[0].Split("=")[1] : string.Empty;
                                        MGReq.createdby = setSplit[1].Contains("=") ? setSplit[1].Split("=")[1] : string.Empty;
                                    }
                                }
                                if (!string.IsNullOrEmpty(MGReq.securityKey) && !string.IsNullOrEmpty(MGReq.createdby))
                                {
                                    IMahagramAPIML MGAPIML = new MahagramAPIML(_accessor, _env, appSetting.onboardingSetting.BaseURL);
                                    var apiResp = MGAPIML.UTIRegistrationUpdate(MGReq, outletAPIData.APIID);
                                    if (apiResp != null)
                                    {
                                        if (apiResp.Statuscode == ErrorCodes.One)
                                        {
                                            procReq.UserID = outletAPIData.UserID;
                                            procReq.OutletID = outletAPIData.OutletID;
                                            procReq.APIID = outletAPIData.APIID;
                                            procReq.APIOutletID = string.IsNullOrEmpty(outletAPIData.APIOutletID) ? apiResp.PSAID : outletAPIData.APIOutletID;
                                            procReq.APIOutletStatus = outletAPIData.APIOutletVerifyStatus == UserStatus.NOTREGISTRED ? apiResp.PSAStatus : outletAPIData.APIOutletVerifyStatus;
                                            procReq.KYCStatus = outletAPIData.APIOutletDocVerifyStatus;
                                            procReq.PSAID = apiResp.PSAID;
                                            procReq.PSARequestID = apiResp.PSARequestID;
                                            procReq.PSAStatus = apiResp.PSAStatus;
                                            IsSaveOnboarding = true;
                                            procReq.IsVerifyStatusUpdate = true;
                                            procReq.IsDocVerifyStatusUpdate = true;
                                            procReq.IsPANRequestIDUpdate = true;
                                            procReq.IsPANUpdateStatus = true;
                                        }
                                        else
                                        {
                                            outletModelResp.Msg = apiResp.Msg;
                                        }
                                    }
                                }
                                else
                                {
                                    outletModelResp.Msg = nameof(ErrorCodes.SystemErrorDown);
                                    outletModelResp.ErrorCode = ErrorCodes.Unknown_Error;
                                }
                            }
                        }
                        #endregion
                    }
                    else if (MDL._ValidateAPIOutletResp.APICode == APICode.YESBANK)
                    {
                        var YBReq = new bcagentregistrationforekycreq
                        {
                            bcagentname = FirstName,
                            middlename = MiddleName,
                            lastname = LastName,
                            companyname = outletAPIData.OutletName,
                            address = outletAPIData.Address,
                            area = outletAPIData.Landmark,
                            cityname = outletAPIData.City,
                            distrcit = outletAPIData.City,
                            statename = outletAPIData.State,
                            pincode = Convert.ToInt32(outletAPIData.Pincode),
                            mobilenumber = outletAPIData.MobileNo,
                            localaddress = outletAPIData.Address,
                            localarea = outletAPIData.Landmark,
                            localcity = outletAPIData.Landmark,
                            localdistrcit = outletAPIData.City,
                            localstate = outletAPIData.State,
                            localpincode = Convert.ToInt32(outletAPIData.Pincode),
                            alternatenumber = outletAPIData.AlternateMobile,
                            emailid = outletAPIData.EmailID,
                            dob = Convert.ToDateTime(outletAPIData.DOB).ToString("dd/MM/yyyy"),
                            idproof = "16",
                            idproofnumber = outletAPIData.AADHAR,
                            shopaddress = outletAPIData.Address,
                            shoparea = outletAPIData.Landmark,
                            shopcity = outletAPIData.City,
                            shopdistrcit = outletAPIData.City,
                            shopstate = outletAPIData.State,
                            shoppincode = Convert.ToInt32(outletAPIData.Pincode),
                            ifsccode = " ",
                            nooftransactionperday = 25,
                            transferamountperday = 125000,
                            bcagenttype = "1",
                            PanCard = outletAPIData.PAN,
                            bcagentid = outletAPIData.OutletID.ToString(),
                            vid = " ",
                            uidtoken = outletAPIData.UIDToken
                        };
                        var yesBankML = new YesBankML(_accessor, _env, _dal);
                        var apiResp = yesBankML.AgentEKYC(YBReq, outletAPIData.APIID);
                        if (apiResp != null)
                        {
                            if (apiResp.Statuscode == ErrorCodes.One)
                            {
                                procReq.UserID = outletAPIData.UserID;
                                procReq.OutletID = outletAPIData.OutletID;
                                procReq.APIID = outletAPIData.APIID;
                                procReq.APIOutletID = apiResp.APIOutletID;
                                procReq.APIOutletStatus = apiResp.APIOutletStatus;
                                procReq.KYCStatus = apiResp.APIOutletStatus;
                                IsSaveOnboarding = true;
                                procReq.IsVerifyStatusUpdate = true;
                                procReq.IsDocVerifyStatusUpdate = true;
                                procReq.DMTID = apiResp.DMTID;
                                procReq.DMTStatus = apiResp.DMTStatus;
                                procReq.IsDMTUpdate = true;
                                procReq.IsDMTUpdateStatus = true;
                            }
                            else
                            {
                                outletModelResp.Msg = apiResp.Msg;
                            }
                        }
                    }
                    else if (MDL._ValidateAPIOutletResp.APICode == APICode.FINGPAY)
                    {
                        var onboardingKyc = GetKYCURL(outletAPIData.UserID, outletAPIData.OutletID);
                        if (!File.Exists(onboardingKyc.ShopPhoto ?? string.Empty) || !File.Exists(onboardingKyc.Adhaar ?? string.Empty) || !File.Exists(onboardingKyc.Pan ?? string.Empty) || !File.Exists(onboardingKyc.PassportPhoto ?? string.Empty))
                        {
                            outletModelResp.Msg = ErrorCodes.Error212SystemNotSpecified;
                            outletModelResp.ErrorCode = ErrorCodes.Document_Not_Completed;
                            return outletModelResp;
                        }
                        var FPSetting = GetFingpay();
                        var FPReq = new FingpayReq
                        {
                            username = FPSetting.MERCHANTName,
                            password = FPSetting.MerchantPin,
                            supermerchantId = Convert.ToInt32(FPSetting.superMerchantId ?? "0"),
                            latitude = Convert.ToDouble((outletAPIData.Latlong ?? string.Empty).Split(',')[0].PadRight(10, '0')),
                            longitude = Convert.ToDouble((outletAPIData.Latlong ?? string.Empty).Split(',')[1].PadRight(10, '0')),
                            timestamp = DateTime.Now.ToString("dd/MMM/yyyy HH:mm:ss", CultureInfo.InvariantCulture),
                            merchants = new List<FingPMerchantModel>
                            {
                                new FingPMerchantModel
                                {
                                    merchantLoginId="FP"+outletAPIData.OutletID,
                                    merchantLoginPin="1234",
                                    merchantName=outletAPIData.Name.ToLower().UppercaseWords(),
                                    merchantPhoneNumber=outletAPIData.MobileNo,
                                    companyLegalName=outletAPIData.OutletName.ToLower().UppercaseWords(),
                                    companyMarketingName=outletAPIData.OutletName.ToLower().UppercaseWords(),
                                    merchantBranch=outletAPIData.BranchName,
                                    tan=string.Empty,
                                    merchantCityName=outletAPIData.City,
                                    merchantDistrictName=outletAPIData.City,
                                    merchantPinCode=outletAPIData.Pincode,
                                    merchantAddress=new FingPMerchantAddress{
                                        merchantAddress=outletAPIData.Address,
                                        merchantState=outletAPIData.StateID.ToString()
                                    },
                                    kyc=new FingPKYC{
                                        userPan=outletAPIData.PAN,
                                        companyOrShopPan=outletAPIData.PAN,
                                        aadhaarNumber=outletAPIData.AADHAR,
                                        gstInNumber=outletAPIData.GSTIN
                                    },
                                    emailId=outletAPIData.EmailID,
                                    settlement=new FingPSettlement{
                                        bankAccountName=outletAPIData.AccountName,
                                        bankBranchName=outletAPIData.BranchName,
                                        bankIfscCode=outletAPIData.IFSC,
                                        companyBankAccountNumber=outletAPIData.AccountNumber,
                                        companyBankName=outletAPIData.BankName
                                    },
                                    cancellationCheckImages=string.Empty,//HttpUtility.UrlEncode(ConverterHelper.O.ConvertImagebase64(onboardingKyc.CancelledCheque)),
                                    ekycDocuments=string.Empty,//HttpUtility.UrlEncode(ConverterHelper.O.ConvertImagebase64(onboardingKyc.Adhaar)),
                                    shopAndPanImage=string.Empty//HttpUtility.UrlEncode(ConverterHelper.O.ConvertImagebase64(onboardingKyc.Pan))
                                }
                            }
                        };
                        var fingPayML = new FingpayML(_dal);
                        var apiResp = fingPayML.MerchantOnboarding(FPReq, outletAPIData.APIID);
                        if (apiResp != null)
                        {
                            if (apiResp.Statuscode == ErrorCodes.One)
                            {
                                procReq.UserID = outletAPIData.UserID;
                                procReq.OutletID = outletAPIData.OutletID;
                                procReq.APIID = outletAPIData.APIID;
                                procReq.APIOutletID = apiResp.APIOutletID;
                                procReq.APIOutletStatus = apiResp.APIOutletStatus;
                                procReq.KYCStatus = apiResp.APIOutletStatus;
                                IsSaveOnboarding = true;
                                procReq.IsVerifyStatusUpdate = true;
                                procReq.IsDocVerifyStatusUpdate = true;
                                procReq.AEPSID = apiResp.AEPSID;
                                procReq.AEPSStatus = apiResp.AEPSStatus;
                                procReq.IsAEPSUpdate = true;
                                procReq.IsAEPSUpdateStatus = true;
                            }
                            else
                            {
                                outletModelResp.Msg = apiResp.Msg;
                            }
                        }
                    }
                    else if (MDL._ValidateAPIOutletResp.APICode.In(APICode.RPFINTECH, APICode.TPFINTECH))
                    {
                        var Lattitude = outletAPIData.Latlong.Contains(",") ? outletAPIData.Latlong.Split(",")[0] : string.Empty;
                        var Longitude = outletAPIData.Latlong.Contains(",") ? outletAPIData.Latlong.Split(",")[1] : string.Empty;
                        if (Lattitude.Contains("."))
                        {
                            Lattitude = Lattitude.Split('.')[1].Length > 4 ? Lattitude.Split('.')[0] + "." + Lattitude.Split('.')[1].Substring(0, 4) : (Lattitude.Split('.')[1].Length < 4 ? Lattitude.Split('.')[0] + "." + Lattitude.Split('.')[1].PadRight(4, '0') : Lattitude);
                            Lattitude = Lattitude.Split('.')[0].PadLeft(2, '0') + "." + Lattitude.Split('.')[1];
                        }

                        if (Longitude.Contains("."))
                        {
                            Longitude = Longitude.Split('.')[1].Length > 4 ? Longitude.Split('.')[0] + "." + Longitude.Split('.')[1].Substring(0, 4) : (Longitude.Split('.')[1].Length < 4 ? Longitude.Split('.')[0] + "." + Longitude.Split('.')[1].PadRight(4, '0') : Longitude);
                            Longitude = Longitude.Split('.')[0].PadLeft(2, '0') + "." + Longitude.Split('.')[1];
                        }
                        var onboardingKyc = GetKYCURL(outletAPIData.UserID, outletAPIData.OutletID);
                        var fintechRequest = new FintechAPIRequestModel
                        {
                            data = new OutletRequest
                            {
                                FirstName = FirstName,
                                MiddleName = MiddleName,
                                LastName = LastName,
                                MobileNo = outletAPIData.MobileNo,
                                AlternateMobile = outletAPIData.AlternateMobile,
                                Company = outletAPIData.OutletName,
                                EmailID = outletAPIData.EmailID,
                                Pincode = outletAPIData.Pincode,
                                Address = outletAPIData.Address,
                                PAN = outletAPIData.PAN,
                                AADHAR = outletAPIData.AADHAR,
                                StateID = outletAPIData.StateID,
                                CityID = outletAPIData.CityID,
                                DOB = Convert.ToDateTime(outletAPIData.DOB).ToString("dd MMM yyyy", CultureInfo.InvariantCulture),
                                ShopType = outletAPIData.ShopType,
                                Qualification = outletAPIData.Qualification,
                                Poupulation = outletAPIData.Poupulation,
                                LocationType = outletAPIData.LocationType,
                                Landmark = outletAPIData.Landmark,
                                Lattitude = Lattitude,
                                Longitude = Longitude,
                                BankName = outletAPIData.BankName,
                                AccountNumber = outletAPIData.AccountNumber,
                                AccountHolder = outletAPIData.AccountName,
                                BranchName = outletAPIData.BranchName,
                                IFSC = outletAPIData.IFSC
                            },
                            kycDoc = new OutletKYCDoc
                            {
                                PAN = string.IsNullOrEmpty(onboardingKyc.Pan) ? string.Empty : ConverterHelper.O.ConvertImagebase64(onboardingKyc.Pan),
                                AADHAR = string.IsNullOrEmpty(onboardingKyc.Adhaar) ? string.Empty : ConverterHelper.O.ConvertImagebase64(onboardingKyc.Adhaar),
                                PHOTO = string.IsNullOrEmpty(onboardingKyc.PassportPhoto) ? string.Empty : ConverterHelper.O.ConvertImagebase64(onboardingKyc.PassportPhoto),
                                ShopImage = string.IsNullOrEmpty(onboardingKyc.ShopPhoto) ? string.Empty : ConverterHelper.O.ConvertImagebase64(onboardingKyc.ShopPhoto),
                                CancelledCheque = string.IsNullOrEmpty(onboardingKyc.CancelledCheque) ? string.Empty : ConverterHelper.O.ConvertImagebase64(onboardingKyc.CancelledCheque),

                            }
                        };
                        var fintechAPIML = new FintechAPIML(_accessor, _env, MDL._ValidateAPIOutletResp.APICode, outletAPIData.APIID, _dal);
                        OutletAPIStatusUpdate apiResp = null;
                        if (string.IsNullOrEmpty(outletAPIData.APIOutletID))
                        {
                            apiResp = fintechAPIML.SaveOutletOfAPIUser(fintechRequest);
                        }
                        else
                        {
                            fintechRequest.data.OutletID = Convert.ToInt32(outletAPIData.APIOutletID);
                            apiResp = fintechAPIML.UpdateOutletOfAPIUser(fintechRequest);
                        }
                        if (apiResp != null)
                        {
                            if (apiResp.Statuscode == ErrorCodes.One)
                            {
                                procReq.UserID = outletAPIData.UserID;
                                procReq.OutletID = outletAPIData.OutletID;
                                procReq.APIID = outletAPIData.APIID;
                                procReq.APIOutletID = apiResp.APIOutletID;
                                procReq.APIOutletStatus = apiResp.APIOutletStatus;
                                procReq.KYCStatus = apiResp.APIOutletStatus;
                                IsSaveOnboarding = true;
                                procReq.IsVerifyStatusUpdate = true;
                                procReq.IsDocVerifyStatusUpdate = true;
                                procReq.AEPSID = apiResp.AEPSID;
                                procReq.AEPSStatus = apiResp.AEPSStatus;
                                procReq.IsAEPSUpdate = true;
                                procReq.IsAEPSUpdateStatus = true;
                                procReq.BBPSID = apiResp.BBPSID;
                                procReq.BBPSStatus = apiResp.BBPSStatus;
                                procReq.IsBBPSUpdate = true;
                                procReq.IsBBPSUpdateStatus = true;
                                procReq.DMTID = apiResp.DMTID;
                                procReq.DMTStatus = apiResp.DMTStatus;
                                procReq.IsDMTUpdate = true;
                                procReq.IsDMTUpdateStatus = true;
                                procReq.RailID = apiResp.RailID;
                                procReq.RailStatus = apiResp.RailStatus;
                                procReq.IsRailIDUpdate = true;
                                procReq.IsRailUpdateStatus = true;
                            }
                            else
                            {
                                outletModelResp.Msg = apiResp.Msg;
                            }
                        }
                    }
                    else if (MDL._ValidateAPIOutletResp.APICode.In(APICode.EKO2, APICode.EKOPAYOUT))
                    {
                        var eko2Request = new EKO2OnboardRequest
                        {
                            pan_number = outletAPIData.PAN,
                            mobile = outletAPIData.MobileNo,
                            first_name = FirstName + (!string.IsNullOrEmpty(MiddleName) ? " " + MiddleName : string.Empty),
                            last_name = LastName,
                            email = outletAPIData.EmailID,
                            dob = Convert.ToDateTime(outletAPIData.DOB).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                            shop_name = outletAPIData.OutletName,
                            line = outletAPIData.Landmark ?? string.Empty,
                            city = outletAPIData.City ?? string.Empty,
                            state = outletAPIData.StateName ?? string.Empty,
                            pincode = outletAPIData.Pincode,
                            district = outletAPIData.City,
                            area = outletAPIData.Landmark
                        };
                        var eko2ML = new EKO2ML(_accessor, _env, _dal, outletAPIData.APIID, MDL._ValidateAPIOutletResp.APICode);
                        var apiResp = eko2ML.AgentOnboarding(eko2Request);
                        if (apiResp != null)
                        {
                            if (apiResp.Statuscode == ErrorCodes.One)
                            {
                                procReq.UserID = outletAPIData.UserID;
                                procReq.OutletID = outletAPIData.OutletID;
                                procReq.APIID = outletAPIData.APIID;
                                procReq.APIOutletID = apiResp.APIOutletID;
                                procReq.APIOutletStatus = apiResp.APIOutletStatus;
                                procReq.KYCStatus = apiResp.APIOutletStatus;
                                IsSaveOnboarding = true;
                                procReq.IsVerifyStatusUpdate = true;
                                procReq.IsDocVerifyStatusUpdate = true;
                                procReq.AEPSID = apiResp.AEPSID;
                                procReq.AEPSStatus = apiResp.AEPSStatus;
                                procReq.IsAEPSUpdate = true;
                                procReq.IsAEPSUpdateStatus = false;
                                procReq.BBPSID = apiResp.BBPSID;
                                procReq.BBPSStatus = apiResp.BBPSStatus;
                                procReq.IsBBPSUpdate = true;
                                procReq.IsBBPSUpdateStatus = true;
                                procReq.DMTID = apiResp.DMTID;
                                procReq.DMTStatus = apiResp.DMTStatus;
                                procReq.IsDMTUpdate = true;
                                procReq.IsDMTUpdateStatus = true;
                            }
                            else
                            {
                                outletModelResp.Msg = apiResp.Msg;
                            }
                        }
                    }
                    else if (MDL._ValidateAPIOutletResp.APICode == APICode.PANMITRA)
                    {
                        var panMitraRequest = new PANMitraOnboardRequest
                        {
                            vle_id = ApplicationSetting.VLEIDPrefix + outletAPIData.OutletID,
                            vle_name = outletAPIData.Name,
                            vle_mob = outletAPIData.MobileNo,
                            vle_email = outletAPIData.EmailID,
                            vle_shop = outletAPIData.OutletName,
                            vle_loc = outletAPIData.Landmark,
                            vle_state = outletAPIData.StateID.ToString(),
                            vle_pin = outletAPIData.Pincode,
                            vle_uid = outletAPIData.AADHAR,
                            vle_pan = outletAPIData.PAN
                        };
                        var panmitrML = new PANMitraML(_accessor, _env, _dal);
                        OutletAPIStatusUpdate apiResp = panmitrML.VLEIDCreate(panMitraRequest, outletAPIData.APIID, outletAPIData.OutletID);

                        if (apiResp != null)
                        {
                            if (apiResp.Statuscode == ErrorCodes.One)
                            {
                                procReq.UserID = outletAPIData.UserID;
                                procReq.OutletID = outletAPIData.OutletID;
                                procReq.APIID = outletAPIData.APIID;
                                procReq.APIOutletID = apiResp.APIOutletID;
                                procReq.APIOutletStatus = apiResp.APIOutletStatus;
                                procReq.KYCStatus = apiResp.APIOutletStatus;
                                IsSaveOnboarding = true;
                                procReq.IsVerifyStatusUpdate = true;
                                procReq.IsDocVerifyStatusUpdate = true;
                                procReq.AEPSID = apiResp.AEPSID;
                                procReq.AEPSStatus = apiResp.AEPSStatus;
                                procReq.BBPSID = apiResp.BBPSID;
                                procReq.BBPSStatus = apiResp.BBPSStatus;
                                procReq.DMTID = apiResp.DMTID;
                                procReq.DMTStatus = apiResp.DMTStatus;
                                procReq.PSAID = apiResp.PSAID;
                                procReq.PSAStatus = apiResp.PSAStatus;
                                procReq.PSARequestID = outletAPIData.OutletID;
                                procReq.IsPANUpdate = true;
                                procReq.IsPANUpdateStatus = true;
                                procReq.IsPANRequestIDUpdate = true;
                            }
                            else
                            {
                                outletModelResp.Msg = apiResp.Msg;
                            }
                        }
                    }
                    else if (MDL._ValidateAPIOutletResp.APICode.Equals(APICode.CASHPOINTINDIA))
                    {
                        var cpiRequest = new CashPointIndiaOnboardRequest
                        {
                            vle_id = ApplicationSetting.VLEIDPrefix + outletAPIData.OutletID,
                            vle_name = outletAPIData.Name,
                            vle_mob = outletAPIData.MobileNo,
                            vle_email = outletAPIData.EmailID,
                            vle_shop = outletAPIData.OutletName,
                            vle_loc = outletAPIData.Landmark,
                            vle_state = outletAPIData.StateID.ToString(),
                            vle_pin = outletAPIData.Pincode,
                            vle_uid = outletAPIData.AADHAR,
                            vle_pan = outletAPIData.PAN
                        };
                        var cpiML = new CashPointIndiaML(_accessor, _env, _dal);
                        OutletAPIStatusUpdate apiResp = cpiML.VLEIDCreate(cpiRequest, outletAPIData.APIID, outletAPIData.OutletID);

                        if (apiResp != null)
                        {
                            if (apiResp.Statuscode == ErrorCodes.One)
                            {
                                procReq.UserID = outletAPIData.UserID;
                                procReq.OutletID = outletAPIData.OutletID;
                                procReq.APIID = outletAPIData.APIID;
                                procReq.APIOutletID = apiResp.APIOutletID;
                                procReq.APIOutletStatus = apiResp.APIOutletStatus;
                                procReq.KYCStatus = apiResp.APIOutletStatus;
                                IsSaveOnboarding = true;
                                procReq.IsVerifyStatusUpdate = true;
                                procReq.IsDocVerifyStatusUpdate = true;
                                procReq.AEPSID = apiResp.AEPSID;
                                procReq.AEPSStatus = apiResp.AEPSStatus;
                                procReq.BBPSID = apiResp.BBPSID;
                                procReq.BBPSStatus = apiResp.BBPSStatus;
                                procReq.DMTID = apiResp.DMTID;
                                procReq.DMTStatus = apiResp.DMTStatus;
                                procReq.PSAID = apiResp.PSAID;
                                procReq.PSAStatus = apiResp.PSAStatus;
                                procReq.PSARequestID = outletAPIData.OutletID;
                                procReq.IsPANUpdate = true;
                                procReq.IsPANUpdateStatus = true;
                                procReq.IsPANRequestIDUpdate = true;
                            }
                            else
                            {
                                outletModelResp.Msg = apiResp.Msg;
                            }
                        }
                    }
                }
            }
            if (IsSaveOnboarding)
            {
                outletModelResp.Statuscode = ErrorCodes.One;
                outletModelResp.Msg = ErrorCodes.OutletRegistered;
                outletModelResp.ErrorCode = ErrorCodes.Transaction_Successful;
                IProcedure _proc = new ProcOutletAPIWiseCU(_dal);
                _proc.Call(procReq);
            }
            return outletModelResp;
        }
        public ResponseStatus CallManualPSA(ValidateAPIOutletResp MDL)
        {
            var outletModelResp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var IsSaveOnboarding = false;
            var procReq = new OutletAPIStatusUpdateReq();
            if (MDL.APIID > 0 && string.IsNullOrEmpty(MDL.APICode) && MDL.OutletID > 0)
            {
                IProcedure proc = new ProcGetSeqForPSAID(_dal);
                var procResp = (ResponseStatus)proc.Call(new CommonReq
                {
                    LoginID = MDL.OutletID,
                    CommonInt = MDL.APIID
                });
                outletModelResp.Statuscode = procResp.Statuscode;
                if (procResp.Statuscode == ErrorCodes.One)
                {
                    IsSaveOnboarding = true;
                    procReq.UserID = MDL.UserID;
                    procReq.OutletID = MDL.OutletID;
                    procReq.APIID = MDL.APIID;
                    procReq.APIOutletID = MDL.OutletID.ToString();
                    procReq.APIOutletStatus = UserStatus.ACTIVE;
                    procReq.KYCStatus = UserStatus.ACTIVE;
                    procReq.IsVerifyStatusUpdate = true;
                    procReq.IsDocVerifyStatusUpdate = true;
                    procReq.PSAID = procResp.Msg;
                    procReq.PSAStatus = UserStatus.APPLIED;
                    procReq.IsPANUpdate = true;
                    procReq.IsPANUpdateStatus = true;
                }
                else
                {
                    outletModelResp.Msg = procResp.Msg;
                }
            }

            if (IsSaveOnboarding)
            {
                outletModelResp.Statuscode = ErrorCodes.One;
                outletModelResp.Msg = ErrorCodes.OutletRegistered;
                outletModelResp.ErrorCode = ErrorCodes.Transaction_Successful;
                outletModelResp.ResponseStatusForAPI = new OnboardAPIResponseStatus
                {
                    ServiceOutletID = procReq.PSAID,
                    ServiceStatus = procReq.PSAStatus
                };
                IProcedure _proc = new ProcOutletAPIWiseCU(_dal);
                _proc.Call(procReq);
            }
            return outletModelResp;
        }
        public OnboardingStatusCheckModel CallOnboardingStatusCheck(OutletServicePlusReqModel MDL, bool IsAPIOutletIDExists)
        {
            var outletStsChkResp = new OnboardingStatusCheckModel
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var IsSaveOnboarding = false;
            var procReq = new OutletAPIStatusUpdateReq();
            if (MDL != null)
            {
                if (MDL._ValidateAPIOutletResp != null)
                {
                    var outletAPIData = MDL._ValidateAPIOutletResp;
                    var appSetting = GetApiAppSetting(outletAPIData.APICode);

                    if (MDL._ValidateAPIOutletResp.APICode == APICode.ROUNDPAY)
                    {
                        var RNDPAYReq = new RoundpayApiRequestModel
                        {
                            Phone1 = outletAPIData.MobileNo,
                            BaseUrl = appSetting.onboardingSetting.BaseURL,
                            Token = appSetting.onboardingSetting.Token,
                            APIID = outletAPIData.APIID
                        };
                        IRoundpayApiML roundpayApiML = new RoundpayApiML(_accessor, _env);
                        var apiResp = roundpayApiML.CheckOnboardStatus(RNDPAYReq);
                        if (apiResp != null)
                        {
                            if (apiResp.Statuscode == ErrorCodes.One)
                            {
                                procReq.UserID = outletAPIData.UserID;
                                procReq.OutletID = outletAPIData.OutletID;
                                procReq.APIID = outletAPIData.APIID;
                                procReq.APIOutletID = apiResp.APIOutletID;
                                procReq.APIOutletStatus = apiResp.APIOutletStatus;
                                procReq.KYCStatus = apiResp.KYCStatus;
                                procReq.BBPSID = apiResp.BBPSID;
                                procReq.BBPSStatus = apiResp.BBPSStatus;
                                procReq.AEPSID = apiResp.AEPSID;
                                procReq.AEPSStatus = apiResp.AEPSStatus;
                                procReq.DMTID = apiResp.DMTID;
                                procReq.DMTStatus = apiResp.DMTStatus;
                                procReq.PSARequestID = apiResp.PSARequestID;
                                procReq.PSAID = apiResp.PSAID;
                                procReq.PSAStatus = apiResp.PSAStatus;
                                IsSaveOnboarding = true;
                                procReq.IsVerifyStatusUpdate = true;
                                procReq.IsDocVerifyStatusUpdate = true;
                                procReq.IsBBPSUpdate = true;
                                procReq.IsBBPSUpdateStatus = true;
                                procReq.IsAEPSUpdate = true;
                                procReq.IsAEPSUpdateStatus = true;
                                procReq.IsPANUpdateStatus = true;
                                procReq.IsPANRequestIDUpdate = procReq.PSARequestID > 0;
                                procReq.IsPANUpdate = !string.IsNullOrEmpty(procReq.PSAID);
                            }
                        }
                    }
                    else if (MDL._ValidateAPIOutletResp.APICode == APICode.MAHAGRAM)
                    {
                        #region Mahagram-AEPS,DMT
                        if (MDL._ValidateAPIOutletResp.SCode.In(ServiceCode.AEPS, ServiceCode.MiniBank, ServiceCode.MoneyTransfer))
                        {
                            var MGReq = new MGBCStatusRequest
                            {
                                bc_id = MDL._ValidateAPIOutletResp.SCode.In(ServiceCode.AEPS, ServiceCode.MiniBank) ? MDL._ValidateAPIOutletResp.AEPSID : MDL._ValidateAPIOutletResp.DMTID
                            };
                            if ((appSetting.onboardingSetting.Token ?? string.Empty).Contains("&"))
                            {
                                var setSplit = appSetting.onboardingSetting.Token.Split('&');
                                if (setSplit.Length == 3)
                                {
                                    MGReq.saltkey = setSplit[0].Contains("=") ? setSplit[0].Split("=")[1] : string.Empty;
                                    MGReq.secretkey = setSplit[1].Contains("=") ? setSplit[1].Split("=")[1] : string.Empty;
                                    MGReq.cpid = setSplit[2].Contains("=") ? setSplit[2].Split("=")[1] : string.Empty;
                                }
                            }
                            IMahagramAPIML MGAPIML = new MahagramAPIML(_accessor, _env, appSetting.onboardingSetting.BaseURL);
                            if ((outletAPIData.AEPSID ?? string.Empty).Length > 0 && (outletAPIData.DMTID ?? string.Empty).Length > 0)
                            {
                                MGReq.bc_id = outletAPIData.AEPSID;
                                var apiResp = MGAPIML.APIBCStatus(MGReq, outletAPIData.APIID);
                                if (apiResp != null)
                                {
                                    if (apiResp.Statuscode == ErrorCodes.One)
                                    {
                                        procReq.UserID = outletAPIData.UserID;
                                        procReq.OutletID = outletAPIData.OutletID;
                                        procReq.APIID = outletAPIData.APIID;
                                        procReq.APIOutletID = apiResp.APIOutletID;

                                        procReq.APIOutletStatus = apiResp.APIOutletStatus;
                                        procReq.KYCStatus = apiResp.KYCStatus;
                                        procReq.IsVerifyStatusUpdate = true;
                                        procReq.IsDocVerifyStatusUpdate = true;

                                        procReq.AEPSID = apiResp.AEPSID;
                                        procReq.AEPSStatus = apiResp.AEPSStatus;
                                        procReq.DMTID = apiResp.DMTID;
                                        procReq.DMTStatus = apiResp.DMTStatus;
                                        IsSaveOnboarding = true;

                                        procReq.IsAEPSUpdate = true;
                                        procReq.IsAEPSUpdateStatus = true;
                                        procReq.IsDMTUpdate = true;
                                        procReq.IsDMTUpdateStatus = true;
                                    }
                                }
                            }
                            else
                            {
                                var MGReq1 = new MGBCGetCodeRequest
                                {
                                    cpid = MGReq.cpid,
                                    secretkey = MGReq.secretkey,
                                    saltkey = MGReq.saltkey,
                                    emailid = outletAPIData.EmailID,
                                    phone1 = outletAPIData.MobileNo
                                };
                                var apiResp = MGAPIML.GetBCCode(MGReq1, outletAPIData.APIID);
                                if (apiResp != null)
                                {
                                    if (apiResp.Statuscode == ErrorCodes.One)
                                    {
                                        procReq.UserID = outletAPIData.UserID;
                                        procReq.OutletID = outletAPIData.OutletID;
                                        procReq.APIID = outletAPIData.APIID;
                                        procReq.APIOutletID = apiResp.APIOutletID;
                                        procReq.APIOutletStatus = ErrorCodes.One;
                                        procReq.KYCStatus = ErrorCodes.One;
                                        procReq.AEPSStatus = ErrorCodes.One;
                                        procReq.DMTStatus = ErrorCodes.One;
                                        procReq.APIOutletID = apiResp.APIOutletID;
                                        procReq.AEPSID = apiResp.AEPSID;
                                        procReq.DMTID = apiResp.DMTID;

                                        IsSaveOnboarding = true;
                                        procReq.IsVerifyStatusUpdate = true;
                                        procReq.IsDocVerifyStatusUpdate = true;
                                        procReq.IsAEPSUpdate = true;
                                        procReq.IsAEPSUpdateStatus = true;
                                        procReq.IsDMTUpdate = true;
                                        procReq.IsDMTUpdateStatus = true;
                                    }
                                }
                            }
                        }
                        #endregion
                        #region PSA
                        if (MDL._ValidateAPIOutletResp.SCode.In(ServiceCode.PSAService))
                        {
                            var MGReq = new MGPSARequest
                            {
                                requestid = outletAPIData.PANRequestID.ToString()
                            };
                            appSetting = GetApiPSAAppSetting(outletAPIData.APICode);
                            if ((appSetting.onboardingSetting.Token ?? string.Empty).Contains("&"))
                            {
                                var setSplit = appSetting.onboardingSetting.Token.Split('&');
                                if (setSplit.Length == 2)
                                {
                                    MGReq.securityKey = setSplit[0].Contains("=") ? setSplit[0].Split("=")[1] : string.Empty;
                                    MGReq.createdby = setSplit[1].Contains("=") ? setSplit[1].Split("=")[1] : string.Empty;
                                }
                            }
                            if (!string.IsNullOrEmpty(MGReq.securityKey) && !string.IsNullOrEmpty(MGReq.createdby))
                            {
                                IMahagramAPIML MGAPIML = new MahagramAPIML(_accessor, _env, appSetting.onboardingSetting.BaseURL);
                                var apiResp = MGAPIML.UTIAgentStatuscheck(MGReq, outletAPIData.APIID);
                                if (apiResp != null)
                                {
                                    if (apiResp.Statuscode == ErrorCodes.One)
                                    {
                                        procReq.UserID = outletAPIData.UserID;
                                        procReq.OutletID = outletAPIData.OutletID;
                                        procReq.APIID = outletAPIData.APIID;
                                        procReq.APIOutletID = outletAPIData.APIOutletID;
                                        procReq.PSAID = apiResp.PSAID;
                                        procReq.PSAStatus = apiResp.PSAStatus;
                                        IsSaveOnboarding = true;
                                        procReq.IsPANUpdateStatus = true;
                                    }
                                    else
                                    {
                                        outletStsChkResp.Msg = apiResp.Msg;
                                    }
                                }
                            }
                            else
                            {
                                outletStsChkResp.Msg = nameof(ErrorCodes.SystemErrorDown);
                                outletStsChkResp.ErrorCode = ErrorCodes.Unknown_Error;
                            }
                        }
                        #endregion
                    }
                    else if (MDL._ValidateAPIOutletResp.APICode.In(APICode.RPFINTECH, APICode.TPFINTECH))
                    {
                        var fintechAPIML = new FintechAPIML(_accessor, _env, outletAPIData.APICode, outletAPIData.APIID, _dal);
                        var apiResp = fintechAPIML.CheckOutletStatus(new FintechAPIRequestModel
                        {
                            data = new OutletRequest
                            {
                                MobileNo = outletAPIData.MobileNo
                            }
                        });
                        if (apiResp != null)
                        {
                            if (apiResp.Statuscode == ErrorCodes.One)
                            {
                                procReq.UserID = outletAPIData.UserID;
                                procReq.OutletID = outletAPIData.OutletID;
                                procReq.APIID = outletAPIData.APIID;
                                procReq.APIOutletID = apiResp.APIOutletID;
                                procReq.APIOutletStatus = apiResp.APIOutletStatus;
                                procReq.KYCStatus = apiResp.KYCStatus;
                                procReq.BBPSID = apiResp.BBPSID;
                                procReq.BBPSStatus = apiResp.BBPSStatus;
                                procReq.AEPSID = apiResp.AEPSID;
                                procReq.AEPSStatus = apiResp.AEPSStatus;
                                procReq.DMTID = apiResp.DMTID;
                                procReq.DMTStatus = apiResp.DMTStatus;
                                procReq.PSARequestID = apiResp.PSARequestID;
                                procReq.PSAID = apiResp.PSAID;
                                procReq.PSAStatus = apiResp.PSAStatus;
                                procReq.RailID = apiResp.RailID;
                                procReq.RailStatus = apiResp.RailStatus;
                                IsSaveOnboarding = true;
                                procReq.IsVerifyStatusUpdate = true;
                                procReq.IsDocVerifyStatusUpdate = true;
                                procReq.IsBBPSUpdate = true;
                                procReq.IsBBPSUpdateStatus = true;
                                procReq.IsAEPSUpdate = true;
                                procReq.IsAEPSUpdateStatus = true;
                                procReq.IsPANUpdateStatus = true;
                                procReq.IsPANRequestIDUpdate = procReq.PSARequestID > 0;
                                procReq.IsPANUpdate = !string.IsNullOrEmpty(procReq.PSAID);
                                procReq.IsRailUpdateStatus = true;
                                procReq.IsRailIDUpdate = true;
                            }
                        }
                    }
                    else if (MDL._ValidateAPIOutletResp.APICode == APICode.PANMITRA)
                    {
                        var panmitraML = new PANMitraML(_accessor, _env, _dal);
                        var apiResp = panmitraML.VLEIDStatus(new PANMitraOnboardRequest
                        {
                            vle_id = ApplicationSetting.VLEIDPrefix + outletAPIData.OutletID
                        }, outletAPIData.APIID, outletAPIData.OutletID);
                        if (apiResp != null)
                        {
                            if (apiResp.Statuscode == ErrorCodes.One)
                            {
                                procReq.UserID = outletAPIData.UserID;
                                procReq.OutletID = outletAPIData.OutletID;
                                procReq.APIID = outletAPIData.APIID;
                                procReq.APIOutletID = apiResp.APIOutletID;
                                procReq.APIOutletStatus = apiResp.APIOutletStatus;
                                procReq.KYCStatus = apiResp.KYCStatus;
                                procReq.BBPSID = apiResp.BBPSID;
                                procReq.BBPSStatus = apiResp.BBPSStatus;
                                procReq.AEPSID = apiResp.AEPSID;
                                procReq.AEPSStatus = apiResp.AEPSStatus;
                                procReq.DMTID = apiResp.DMTID;
                                procReq.DMTStatus = apiResp.DMTStatus;
                                procReq.PSARequestID = apiResp.PSARequestID;
                                procReq.PSAID = apiResp.PSAID;
                                procReq.PSAStatus = apiResp.PSAStatus;
                                IsSaveOnboarding = true;
                                procReq.IsVerifyStatusUpdate = true;
                                procReq.IsDocVerifyStatusUpdate = true;
                                procReq.IsBBPSUpdate = false;
                                procReq.IsBBPSUpdateStatus = false;
                                procReq.IsAEPSUpdate = false;
                                procReq.IsAEPSUpdateStatus = false;
                                procReq.IsPANUpdateStatus = true;
                                procReq.IsPANRequestIDUpdate = true;
                                procReq.IsPANUpdate = !string.IsNullOrEmpty(procReq.PSAID);
                            }
                        }
                    }
                    else if (MDL._ValidateAPIOutletResp.APICode.Equals(APICode.CASHPOINTINDIA))
                    {
                        var cpiML = new CashPointIndiaML(_accessor, _env, _dal);
                        var apiResp = cpiML.VLEIDStatus(new CashPointIndiaOnboardRequest
                        {
                            vle_id = ApplicationSetting.VLEIDPrefix + outletAPIData.OutletID
                        }, outletAPIData.APIID, outletAPIData.OutletID);
                        if (apiResp != null)
                        {
                            if (apiResp.Statuscode == ErrorCodes.One)
                            {
                                procReq.UserID = outletAPIData.UserID;
                                procReq.OutletID = outletAPIData.OutletID;
                                procReq.APIID = outletAPIData.APIID;
                                procReq.APIOutletID = apiResp.APIOutletID;
                                procReq.APIOutletStatus = apiResp.APIOutletStatus;
                                procReq.KYCStatus = apiResp.KYCStatus;
                                procReq.BBPSID = apiResp.BBPSID;
                                procReq.BBPSStatus = apiResp.BBPSStatus;
                                procReq.AEPSID = apiResp.AEPSID;
                                procReq.AEPSStatus = apiResp.AEPSStatus;
                                procReq.DMTID = apiResp.DMTID;
                                procReq.DMTStatus = apiResp.DMTStatus;
                                procReq.PSARequestID = apiResp.PSARequestID;
                                procReq.PSAID = apiResp.PSAID;
                                procReq.PSAStatus = apiResp.PSAStatus;
                                IsSaveOnboarding = true;
                                procReq.IsVerifyStatusUpdate = true;
                                procReq.IsDocVerifyStatusUpdate = true;
                                procReq.IsBBPSUpdate = false;
                                procReq.IsBBPSUpdateStatus = false;
                                procReq.IsAEPSUpdate = false;
                                procReq.IsAEPSUpdateStatus = false;
                                procReq.IsPANUpdateStatus = true;
                                procReq.IsPANRequestIDUpdate = true;
                                procReq.IsPANUpdate = !string.IsNullOrEmpty(procReq.PSAID);
                            }
                        }
                    }
                    else if (MDL._ValidateAPIOutletResp.APICode == APICode.SPRINT)
                    {
                        SprintBBPSML sprintBBPSML = new SprintBBPSML(_accessor, _env, _dal);
                        var apiResp = sprintBBPSML.GetCallbackURL(MDL._ValidateAPIOutletResp);
                        if (apiResp.Statuscode == ErrorCodes.One)
                        {
                            outletStsChkResp.Statuscode = apiResp.Statuscode;
                            outletStsChkResp.IsRedirection = apiResp.IsRedirection;
                            outletStsChkResp.RedirectionUrI = apiResp.RedirectURL;
                        }
                    }
                }
            }
            if (IsSaveOnboarding)
            {
                outletStsChkResp.IsAPIOutletExists = true;
                outletStsChkResp.Statuscode = ErrorCodes.One;
                outletStsChkResp.Msg = ErrorCodes.OutletRegistered;
                outletStsChkResp.ErrorCode = ErrorCodes.Transaction_Successful;
                IProcedure _proc = new ProcOutletAPIWiseCU(_dal);
                _proc.Call(procReq);
                if (procReq.APIOutletStatus == UserStatus.REJECTED || procReq.KYCStatus == UserStatus.REJECTED)
                {
                    outletStsChkResp.IsEditProfile = true;
                }
            }
            return outletStsChkResp;
        }
        public ServicePlusStatusModel CallBBPSServicePlus(OutletServicePlusReqModel MDL)
        {
            var slpusStatusMdl = new ServicePlusStatusModel
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var IsSaveOnboarding = false;
            var IsShowAPIError = false;
            var procReq = new OutletAPIStatusUpdateReq();
            if (MDL != null)
            {
                if (MDL._ValidateAPIOutletResp != null)
                {
                    var outletAPIData = MDL._ValidateAPIOutletResp;
                    var appSetting = GetApiAppSetting(outletAPIData.APICode);

                    if (MDL._ValidateAPIOutletResp.APICode == APICode.ROUNDPAY)
                    {
                        var RNDPAYReq = new RoundpayApiRequestModel
                        {
                            Phone1 = outletAPIData.MobileNo,
                            BaseUrl = appSetting.onboardingSetting.BaseURL,
                            Token = appSetting.onboardingSetting.Token,
                            APIID = outletAPIData.APIID,
                            OTP = MDL.ServicPlusOTP,
                            APIOutletID = outletAPIData.APIOutletID,
                            Scode = ServiceCode.BBPSService
                        };
                        IRoundpayApiML roundpayApiML = new RoundpayApiML(_accessor, _env);
                        var apiResp = roundpayApiML.ServicePlus(RNDPAYReq);
                        if (apiResp != null)
                        {
                            if (apiResp.Statuscode == ErrorCodes.One)
                            {
                                slpusStatusMdl.IsOTPRequired = apiResp.IsOTPRequired;
                                procReq.UserID = outletAPIData.UserID;
                                procReq.OutletID = outletAPIData.OutletID;
                                procReq.APIID = outletAPIData.APIID;
                                procReq.APIOutletID = outletAPIData.APIOutletID;
                                procReq.BBPSStatus = apiResp.BBPSStatus;
                                IsSaveOnboarding = true;
                                procReq.IsBBPSUpdateStatus = true;
                                if (apiResp.APIOutletStatus > 0)
                                {
                                    procReq.APIOutletStatus = apiResp.APIOutletStatus;
                                    procReq.IsVerifyStatusUpdate = true;
                                    procReq.IsVerifyStatusUpdate = true;
                                    procReq.IsDocVerifyStatusUpdate = true;
                                }
                            }
                            else
                            {
                                slpusStatusMdl.Msg = ErrorCodes.NotActivateService.Replace("{SERVICE}", "BBPS");
                            }
                        }
                    }
                    if (MDL._ValidateAPIOutletResp.APICode.In(APICode.RPFINTECH, APICode.TPFINTECH))
                    {
                        var fintechAPIML = new FintechAPIML(_accessor, _env, outletAPIData.APICode, outletAPIData.APIID, _dal);
                        var apiResp = fintechAPIML.CheckOutletServiceStatus(new FintechAPIRequestModel
                        {
                            SPkey = outletAPIData.APIOpCode,
                            data = new OutletRequest
                            {
                                OutletID = Convert.ToInt32(outletAPIData.APIOutletID),
                                OTPRefID = MDL._ValidateAPIOutletResp.OTPRefID,
                                OTP = MDL._ValidateAPIOutletResp.OTP,
                                PidData = MDL._ValidateAPIOutletResp.PIDATA
                            }
                        }, outletAPIData.SCode);
                        if (apiResp != null)
                        {
                            if (apiResp.Statuscode == ErrorCodes.One)
                            {
                                if (apiResp.BBPSStatus.In(UserStatus.ACTIVE, UserStatus.APPLIED, UserStatus.REJECTED))
                                {
                                    slpusStatusMdl.IsOTPRequired = apiResp.IsOTPRequired;
                                    procReq.UserID = outletAPIData.UserID;
                                    procReq.OutletID = outletAPIData.OutletID;
                                    procReq.APIID = outletAPIData.APIID;
                                    procReq.APIOutletID = outletAPIData.APIOutletID;
                                    procReq.BBPSStatus = apiResp.BBPSStatus;
                                    IsSaveOnboarding = true;
                                    procReq.IsBBPSUpdateStatus = true;
                                    if (apiResp.APIOutletStatus > 0)
                                    {
                                        procReq.APIOutletStatus = apiResp.APIOutletStatus;
                                        procReq.KYCStatus = apiResp.KYCStatus;
                                        procReq.IsVerifyStatusUpdate = true;
                                        procReq.IsDocVerifyStatusUpdate = true;
                                    }
                                }
                                else
                                {
                                    slpusStatusMdl.Msg = "(AP)" + apiResp.Msg;
                                }
                            }
                            else
                            {
                                IsShowAPIError = true;
                                if (!string.IsNullOrEmpty(apiResp.APIOutletID))
                                {
                                    procReq.UserID = outletAPIData.UserID;
                                    procReq.OutletID = outletAPIData.OutletID;
                                    procReq.APIID = outletAPIData.APIID;
                                    procReq.APIOutletID = outletAPIData.APIOutletID;
                                    if (apiResp.APIOutletStatus > 0)
                                    {
                                        procReq.APIOutletStatus = apiResp.APIOutletStatus;
                                        procReq.KYCStatus = apiResp.KYCStatus;
                                        procReq.IsVerifyStatusUpdate = true;
                                        procReq.IsDocVerifyStatusUpdate = true;
                                        IsSaveOnboarding = true;
                                    }

                                }
                                //slpusStatusMdl.Msg = ErrorCodes.NotActivateService.Replace("{SERVICE}", "BBPS");
                                slpusStatusMdl.Msg = "(AP)" + apiResp.Msg;
                            }
                        }
                    }
                }
            }
            if (IsSaveOnboarding)
            {
                if (IsShowAPIError)
                {
                    slpusStatusMdl.Statuscode = ErrorCodes.Minus1;
                    slpusStatusMdl.Msg = slpusStatusMdl.Msg;
                    slpusStatusMdl.ErrorCode = ErrorCodes.Unknown_Error;
                }
                else
                {
                    slpusStatusMdl.Statuscode = ErrorCodes.One;
                    slpusStatusMdl.Msg = nameof(ErrorCodes.Transaction_Successful);
                    slpusStatusMdl.ErrorCode = ErrorCodes.Transaction_Successful;
                }

                IProcedure _proc = new ProcOutletAPIWiseCU(_dal);
                _proc.Call(procReq);
            }
            return slpusStatusMdl;
        }
        public ServicePlusStatusModel CallAEPSServicePlus(OutletServicePlusReqModel MDL)
        {
            var slpusStatusMdl = new ServicePlusStatusModel
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var IsSaveOnboarding = false;
            var IsShowAPIError = false;
            var procReq = new OutletAPIStatusUpdateReq();
            if (MDL != null)
            {
                if (MDL._ValidateAPIOutletResp != null)
                {
                    var outletAPIData = MDL._ValidateAPIOutletResp;
                    var appSetting = GetApiAppSetting(outletAPIData.APICode);

                    if (MDL._ValidateAPIOutletResp.APICode == APICode.ROUNDPAY)
                    {
                        var RNDPAYReq = new RoundpayApiRequestModel
                        {
                            Phone1 = outletAPIData.MobileNo,
                            BaseUrl = appSetting.onboardingSetting.BaseURL,
                            Token = appSetting.onboardingSetting.Token,
                            APIID = outletAPIData.APIID,
                            APIOutletID = outletAPIData.APIOutletID,
                            Scode = ServiceCode.AEPS
                        };
                        IRoundpayApiML roundpayApiML = new RoundpayApiML(_accessor, _env);
                        var apiResp = roundpayApiML.ServicePlus(RNDPAYReq);
                        if (apiResp != null)
                        {
                            if (apiResp.Statuscode == ErrorCodes.One)
                            {
                                procReq.UserID = outletAPIData.UserID;
                                procReq.OutletID = outletAPIData.OutletID;
                                procReq.APIID = outletAPIData.APIID;
                                procReq.APIOutletID = outletAPIData.APIOutletID;
                                procReq.AEPSStatus = apiResp.AEPSStatus;
                                IsSaveOnboarding = true;
                                procReq.IsAEPSUpdateStatus = (apiResp.AEPSURL ?? string.Empty).Length > 0;
                                slpusStatusMdl.AEPSURL = apiResp.AEPSURL;
                            }
                            else
                            {
                                slpusStatusMdl.Msg = ErrorCodes.NotActivateService.Replace("{SERVICE}", "AEPS");
                            }
                        }
                    }
                    if (MDL._ValidateAPIOutletResp.APICode.In(APICode.RPFINTECH, APICode.TPFINTECH))
                    {
                        var fintechAPIML = new FintechAPIML(_accessor, _env, outletAPIData.APICode, outletAPIData.APIID, _dal);
                        var apiResp = fintechAPIML.CheckOutletServiceStatus(new FintechAPIRequestModel
                        {
                            SPkey = outletAPIData.APIOpCode,
                            data = new OutletRequest
                            {
                                OutletID = Convert.ToInt32(outletAPIData.APIOutletID),
                                OTP = MDL._ValidateAPIOutletResp.OTP,
                                OTPRefID = MDL._ValidateAPIOutletResp.OTPRefID,
                                PidData = MDL._ValidateAPIOutletResp.PIDATA,
                                BioAuthType=MDL._ValidateAPIOutletResp.BioAuthType
                            },
                            PartnerID = outletAPIData.PartnerID
                        }, outletAPIData.SCode);
                        if (apiResp != null)
                        {
                            if (apiResp.Statuscode == ErrorCodes.One)
                            {
                                slpusStatusMdl.AEPSURL = apiResp.AEPSURL;
                                if (apiResp.AEPSStatus.In(UserStatus.ACTIVE, UserStatus.APPLIED, UserStatus.REJECTED))
                                {
                                    slpusStatusMdl.IsOTPRequired = apiResp.IsOTPRequired;
                                    procReq.UserID = outletAPIData.UserID;
                                    procReq.OutletID = outletAPIData.OutletID;
                                    procReq.APIID = outletAPIData.APIID;
                                    procReq.APIOutletID = outletAPIData.APIOutletID;
                                    procReq.AEPSID = apiResp.AEPSID;
                                    procReq.AEPSStatus = apiResp.AEPSStatus;
                                    IsSaveOnboarding = true;
                                    procReq.IsAEPSUpdate = true;
                                    procReq.IsAEPSUpdateStatus = true;
                                    if (apiResp.APIOutletStatus > 0)
                                    {
                                        procReq.APIOutletStatus = apiResp.APIOutletStatus;
                                        procReq.KYCStatus = apiResp.KYCStatus;
                                        procReq.IsVerifyStatusUpdate = true;
                                        procReq.IsDocVerifyStatusUpdate = true;
                                    }

                                }
                                else
                                {
                                    slpusStatusMdl.Msg = "(AP)" + apiResp.Msg;
                                }
                            }
                            else
                            {
                                IsShowAPIError = true;
                                if (!string.IsNullOrEmpty(apiResp.APIOutletID))
                                {
                                    procReq.UserID = outletAPIData.UserID;
                                    procReq.OutletID = outletAPIData.OutletID;
                                    procReq.APIID = outletAPIData.APIID;
                                    procReq.APIOutletID = outletAPIData.APIOutletID;
                                    if (apiResp.APIOutletStatus > 0)
                                    {
                                        procReq.APIOutletStatus = apiResp.APIOutletStatus;
                                        procReq.KYCStatus = apiResp.KYCStatus;
                                        procReq.IsVerifyStatusUpdate = true;
                                        procReq.IsDocVerifyStatusUpdate = true;
                                        IsSaveOnboarding = true;
                                    }

                                }
                                //slpusStatusMdl.Msg = ErrorCodes.NotActivateService.Replace("{SERVICE}", "AEPS");
                                slpusStatusMdl.Msg = "(AP)" + apiResp.Msg;
                            }
                            slpusStatusMdl.IsBioMetricRequired = apiResp.IsBioMetricRequired;
                            slpusStatusMdl.IsOTPRequired = apiResp.IsOTPRequired;
                            slpusStatusMdl.OTPRefID = apiResp.OTPRefID;
                            slpusStatusMdl.BioAuthType = apiResp.BioAuthType;
                        }
                    }
                    if (MDL._ValidateAPIOutletResp.APICode == APICode.MAHAGRAM)
                    {
                        var MGReq = new MGInitiateRequest
                        {
                            bc_id = outletAPIData.AEPSID,
                            phone1 = outletAPIData.MobileNo,
                            ip = MDL.RequestIP,
                            userid = outletAPIData.OutletID
                        };
                        if ((appSetting.onboardingSetting.Token ?? string.Empty).Contains("&"))
                        {
                            var setSplit = appSetting.onboardingSetting.Token.Split('&');
                            if (setSplit.Length == 3)
                            {
                                MGReq.saltkey = setSplit[0].Contains("=") ? setSplit[0].Split("=")[1] : string.Empty;
                                MGReq.secretkey = setSplit[1].Contains("=") ? setSplit[1].Split("=")[1] : string.Empty;
                            }
                        }
                        IMahagramAPIML MGAPIML = new MahagramAPIML(_accessor, _env, appSetting.onboardingSetting.BaseURL);
                        var apiResp = MGAPIML.BCInitiate(MGReq, outletAPIData.APIID);
                        if (apiResp != null)
                        {
                            if (apiResp.Statuscode == ErrorCodes.One)
                            {
                                procReq.UserID = outletAPIData.UserID;
                                procReq.OutletID = outletAPIData.OutletID;
                                procReq.APIID = outletAPIData.APIID;
                                procReq.APIOutletID = outletAPIData.APIOutletID;
                                procReq.AEPSStatus = apiResp.AEPSStatus;
                                IsSaveOnboarding = true;
                                procReq.IsAEPSUpdateStatus = (apiResp.AEPSURL ?? string.Empty).Length > 0;
                                slpusStatusMdl.AEPSURL = apiResp.AEPSURL;
                            }
                            else
                            {
                                slpusStatusMdl.Msg = ErrorCodes.NotActivateService.Replace("{SERVICE}", "AEPS");
                            }
                        }
                    }
                    if (MDL._ValidateAPIOutletResp.APICode == APICode.FINGPAY)
                    {
                        //Do Logic
                    }

                }
            }
            if (IsSaveOnboarding)
            {
                if (IsShowAPIError)
                {
                    slpusStatusMdl.Statuscode = ErrorCodes.Minus1;
                    slpusStatusMdl.Msg = slpusStatusMdl.Msg;
                    slpusStatusMdl.ErrorCode = ErrorCodes.Unknown_Error;
                }
                else
                {
                    slpusStatusMdl.Statuscode = ErrorCodes.One;
                    slpusStatusMdl.Msg = nameof(ErrorCodes.Transaction_Successful);
                    slpusStatusMdl.ErrorCode = ErrorCodes.Transaction_Successful;
                }

                slpusStatusMdl.ServiceStatus = procReq.AEPSStatus;
                IProcedure _proc = new ProcOutletAPIWiseCU(_dal);
                _proc.Call(procReq);
            }
            return slpusStatusMdl;
        }
        public BCResponse CallBCService(OutletServicePlusReqModel MDL)
        {
            var OutReqRes = new BCResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            if (MDL != null)
            {
                if (MDL._ValidateAPIOutletResp != null)
                {
                    var outletAPIData = MDL._ValidateAPIOutletResp;
                    var appSetting = GetApiAppSetting(outletAPIData.APICode);

                    if (MDL._ValidateAPIOutletResp.APICode == APICode.ROUNDPAY && MDL._ValidateAPIOutletResp.SCode.Equals(ServiceCode.AEPS))
                    {
                        var RNDPAYReq = new RoundpayApiRequestModel
                        {
                            Phone1 = outletAPIData.MobileNo,
                            BaseUrl = appSetting.onboardingSetting.BaseURL,
                            Token = appSetting.onboardingSetting.Token,
                            APIID = outletAPIData.APIID,
                            APIOutletID = outletAPIData.APIOutletID,
                            Scode = ServiceCode.AEPS
                        };
                        IRoundpayApiML roundpayApiML = new RoundpayApiML(_accessor, _env);
                        var _apiRes = roundpayApiML.GetBCDetail(RNDPAYReq);
                        if (_apiRes != null)
                        {
                            if (_apiRes.Statuscode == ErrorCodes.One)
                            {
                                OutReqRes.Statuscode = ErrorCodes.One;
                                OutReqRes.Msg = nameof(ErrorCodes.Transaction_Successful);
                                OutReqRes.Table = _apiRes.Table;
                            }
                            else
                            {
                                OutReqRes.Msg = ErrorCodes.NotActivateService.Replace("{SERVICE}", "BC Detail");
                            }
                        }
                    }
                    else if (MDL._ValidateAPIOutletResp.APICode == APICode.MAHAGRAM)
                    {
                        var saltkey = string.Empty;
                        var secretkey = string.Empty;
                        var cpid = string.Empty;
                        if ((appSetting.onboardingSetting.Token ?? string.Empty).Contains("&"))
                        {
                            var setSplit = appSetting.onboardingSetting.Token.Split('&');
                            if (setSplit.Length == 3)
                            {
                                saltkey = setSplit[0].Contains("=") ? setSplit[0].Split("=")[1] : string.Empty;
                                secretkey = setSplit[1].Contains("=") ? setSplit[1].Split("=")[1] : string.Empty;
                                cpid = setSplit[2].Contains("=") ? setSplit[2].Split("=")[1] : string.Empty;
                            }
                        }
                        OutReqRes.Statuscode = ErrorCodes.One;
                        OutReqRes.Msg = nameof(ErrorCodes.Transaction_Successful);
                        OutReqRes.Table = new List<_BCResponse>
                        {
                            new _BCResponse
                            {
                                BCID=outletAPIData.AEPSID,
                                EmailId=outletAPIData.EmailID,
                                Mobileno=outletAPIData.MobileNo,
                                AepsOutletId=outletAPIData.APIOutletID,
                                UserId=outletAPIData.OutletID.ToString(),
                                CPID=cpid,
                                SaltKey=saltkey,
                                SecretKey=secretkey
                            }
                        };
                    }
                }
            }

            return OutReqRes;
        }
        public ServicePlusStatusModel CallPSAServicePlus(OutletServicePlusReqModel MDL)
        {
            var slpusStatusMdl = new ServicePlusStatusModel
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var IsSaveOnboarding = false;
            var IsShowAPIError = false;
            var procReq = new OutletAPIStatusUpdateReq();
            if (MDL != null)
            {
                if (MDL._ValidateAPIOutletResp != null)
                {
                    var outletAPIData = MDL._ValidateAPIOutletResp;
                    var appSetting = GetApiAppSetting(outletAPIData.APICode);

                    if (outletAPIData.APICode == APICode.ROUNDPAY)
                    {
                        var RNDPAYReq = new RoundpayApiRequestModel
                        {
                            Phone1 = outletAPIData.MobileNo,
                            BaseUrl = appSetting.onboardingSetting.BaseURL,
                            Token = appSetting.onboardingSetting.Token,
                            APIID = outletAPIData.APIID,
                            APIOutletID = outletAPIData.APIOutletID,
                            Scode = ServiceCode.PSAService
                        };
                        IRoundpayApiML roundpayApiML = new RoundpayApiML(_accessor, _env);
                        var apiResp = roundpayApiML.ServicePlus(RNDPAYReq);
                        if (apiResp != null)
                        {
                            if (apiResp.Statuscode == ErrorCodes.One)
                            {
                                procReq.UserID = outletAPIData.UserID;
                                procReq.OutletID = outletAPIData.OutletID;
                                procReq.APIID = outletAPIData.APIID;
                                procReq.APIOutletID = outletAPIData.APIOutletID;
                                procReq.PSAID = apiResp.PSAID;
                                procReq.PSARequestID = apiResp.PSARequestID;
                                procReq.PSAStatus = apiResp.PSAStatus;
                                IsSaveOnboarding = true;
                                procReq.IsPANUpdateStatus = true;
                                procReq.IsPANRequestIDUpdate = true;
                                procReq.IsPANUpdate = true;
                            }
                            else
                            {
                                slpusStatusMdl.Msg = ErrorCodes.NotActivateService.Replace("{SERVICE}", "PSA");
                            }
                        }
                    }
                    if (MDL._ValidateAPIOutletResp.APICode.In(APICode.RPFINTECH, APICode.TPFINTECH))
                    {
                        var fintechAPIML = new FintechAPIML(_accessor, _env, outletAPIData.APICode, outletAPIData.APIID, _dal);
                        var apiResp = fintechAPIML.CheckOutletServiceStatus(new FintechAPIRequestModel
                        {
                            SPkey = outletAPIData.APIOpCode,
                            data = new OutletRequest
                            {
                                OutletID = Convert.ToInt32(outletAPIData.APIOutletID),
                                OTPRefID = MDL._ValidateAPIOutletResp.OTPRefID,
                                OTP = MDL._ValidateAPIOutletResp.OTP,
                                PidData = MDL._ValidateAPIOutletResp.PIDATA
                            }
                        }, outletAPIData.SCode);
                        if (apiResp != null)
                        {
                            if (apiResp.Statuscode == ErrorCodes.One)
                            {
                                if (apiResp.PSAStatus.In(UserStatus.ACTIVE, UserStatus.APPLIED, UserStatus.REJECTED))
                                {
                                    slpusStatusMdl.IsOTPRequired = apiResp.IsOTPRequired;
                                    procReq.UserID = outletAPIData.UserID;
                                    procReq.OutletID = outletAPIData.OutletID;
                                    procReq.APIID = outletAPIData.APIID;
                                    procReq.APIOutletID = outletAPIData.APIOutletID;
                                    procReq.PSAID = apiResp.PSAID;
                                    procReq.PSAStatus = apiResp.PSAStatus;
                                    IsSaveOnboarding = true;
                                    procReq.IsPANUpdate = true;
                                    procReq.IsPANUpdateStatus = true;
                                    if (apiResp.APIOutletStatus > 0)
                                    {
                                        procReq.APIOutletStatus = apiResp.APIOutletStatus;
                                        procReq.KYCStatus = apiResp.KYCStatus;
                                        procReq.IsVerifyStatusUpdate = true;
                                        procReq.IsDocVerifyStatusUpdate = true;
                                    }
                                }
                                else
                                {
                                    slpusStatusMdl.Msg = "(AP)" + apiResp.Msg;
                                }
                            }
                            else
                            {
                                IsShowAPIError = true;
                                if (!string.IsNullOrEmpty(apiResp.APIOutletID))
                                {
                                    procReq.UserID = outletAPIData.UserID;
                                    procReq.OutletID = outletAPIData.OutletID;
                                    procReq.APIID = outletAPIData.APIID;
                                    procReq.APIOutletID = outletAPIData.APIOutletID;
                                    if (apiResp.APIOutletStatus > 0)
                                    {
                                        procReq.APIOutletStatus = apiResp.APIOutletStatus;
                                        procReq.KYCStatus = apiResp.KYCStatus;
                                        procReq.IsVerifyStatusUpdate = true;
                                        procReq.IsDocVerifyStatusUpdate = true;
                                        IsSaveOnboarding = true;
                                    }
                                }
                                slpusStatusMdl.Msg = "(AP)" + apiResp.Msg;
                                //slpusStatusMdl.Msg = ErrorCodes.NotActivateService.Replace("{SERVICE}", "PSA");
                            }
                        }
                    }
                }
            }
            if (IsSaveOnboarding)
            {
                if (IsShowAPIError)
                {
                    slpusStatusMdl.Statuscode = ErrorCodes.Minus1;
                    slpusStatusMdl.Msg = slpusStatusMdl.Msg;
                    slpusStatusMdl.ErrorCode = ErrorCodes.Unknown_Error;
                }
                else
                {
                    slpusStatusMdl.Statuscode = ErrorCodes.One;
                    slpusStatusMdl.Msg = nameof(ErrorCodes.Transaction_Successful);
                    slpusStatusMdl.ErrorCode = ErrorCodes.Transaction_Successful;
                }
                IProcedure _proc = new ProcOutletAPIWiseCU(_dal);
                _proc.Call(procReq);
            }
            return slpusStatusMdl;
        }
        public ServicePlusStatusModel CallDMTServicePlus(OutletServicePlusReqModel MDL)
        {
            var slpusStatusMdl = new ServicePlusStatusModel
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var IsSaveOnboarding = false;
            var IsShowAPIError = false;
            var procReq = new OutletAPIStatusUpdateReq();
            if (MDL != null)
            {
                if (MDL._ValidateAPIOutletResp != null)
                {
                    var outletAPIData = MDL._ValidateAPIOutletResp;
                    var appSetting = GetApiAppSetting(outletAPIData.APICode);

                    if (MDL._ValidateAPIOutletResp.APICode == APICode.ROUNDPAY)
                    {
                        return slpusStatusMdl;
                    }
                    if (MDL._ValidateAPIOutletResp.APICode.In(APICode.RPFINTECH, APICode.TPFINTECH))
                    {
                        var fintechAPIML = new FintechAPIML(_accessor, _env, outletAPIData.APICode, outletAPIData.APIID, _dal);
                        var apiResp = fintechAPIML.CheckOutletServiceStatus(new FintechAPIRequestModel
                        {
                            SPkey = outletAPIData.APIOpCode,
                            data = new OutletRequest
                            {
                                OutletID = Convert.ToInt32(outletAPIData.APIOutletID),
                                OTPRefID = MDL._ValidateAPIOutletResp.OTPRefID,
                                OTP = MDL._ValidateAPIOutletResp.OTP,
                                PidData = MDL._ValidateAPIOutletResp.PIDATA
                            }
                        }, outletAPIData.SCode);
                        if (apiResp != null)
                        {
                            if (apiResp.Statuscode == ErrorCodes.One)
                            {
                                if (apiResp.DMTStatus.In(UserStatus.ACTIVE, UserStatus.APPLIED, UserStatus.REJECTED))
                                {
                                    slpusStatusMdl.IsOTPRequired = apiResp.IsOTPRequired;
                                    procReq.UserID = outletAPIData.UserID;
                                    procReq.OutletID = outletAPIData.OutletID;
                                    procReq.APIID = outletAPIData.APIID;
                                    procReq.APIOutletID = outletAPIData.APIOutletID;
                                    procReq.DMTID = apiResp.DMTID;
                                    procReq.DMTStatus = apiResp.DMTStatus;
                                    IsSaveOnboarding = true;
                                    procReq.IsDMTUpdateStatus = true;
                                    if (apiResp.APIOutletStatus > 0)
                                    {
                                        procReq.APIOutletStatus = apiResp.APIOutletStatus;
                                        procReq.KYCStatus = apiResp.KYCStatus;
                                        procReq.IsVerifyStatusUpdate = true;
                                        procReq.IsDocVerifyStatusUpdate = true;
                                    }
                                }
                                else
                                {
                                    slpusStatusMdl.Msg = "(AP)" + apiResp.Msg;
                                }
                            }
                            else
                            {
                                IsShowAPIError = true;
                                if (!string.IsNullOrEmpty(apiResp.APIOutletID))
                                {
                                    procReq.UserID = outletAPIData.UserID;
                                    procReq.OutletID = outletAPIData.OutletID;
                                    procReq.APIID = outletAPIData.APIID;
                                    procReq.APIOutletID = outletAPIData.APIOutletID;
                                    if (apiResp.APIOutletStatus > 0)
                                    {
                                        procReq.APIOutletStatus = apiResp.APIOutletStatus;
                                        procReq.KYCStatus = apiResp.KYCStatus;
                                        procReq.IsVerifyStatusUpdate = true;
                                        procReq.IsDocVerifyStatusUpdate = true;
                                        IsSaveOnboarding = true;
                                    }

                                }
                                slpusStatusMdl.Msg = "(AP)" + apiResp.Msg;
                                //slpusStatusMdl.Msg = ErrorCodes.NotActivateService.Replace("{SERVICE}", "MoneyTransfer");
                            }
                        }
                    }
                    if (MDL._ValidateAPIOutletResp.APICode == APICode.MAHAGRAM)
                    {
                        procReq.UserID = outletAPIData.UserID;
                        procReq.OutletID = outletAPIData.OutletID;
                        procReq.APIID = outletAPIData.APIID;
                        procReq.APIOutletID = outletAPIData.APIOutletID;
                        procReq.DMTStatus = UserStatus.ACTIVE;
                        IsSaveOnboarding = true;
                        procReq.IsDMTUpdateStatus = true;
                    }
                }
            }
            if (IsSaveOnboarding)
            {
                if (IsShowAPIError)
                {
                    slpusStatusMdl.Statuscode = ErrorCodes.Minus1;
                    slpusStatusMdl.Msg = slpusStatusMdl.Msg;
                    slpusStatusMdl.ErrorCode = ErrorCodes.Unknown_Error;
                }
                else
                {
                    slpusStatusMdl.Statuscode = ErrorCodes.One;
                    slpusStatusMdl.Msg = nameof(ErrorCodes.Transaction_Successful);
                    slpusStatusMdl.ErrorCode = ErrorCodes.Transaction_Successful;
                }

                IProcedure _proc = new ProcOutletAPIWiseCU(_dal);
                _proc.Call(procReq);
            }
            return slpusStatusMdl;
        }
        public ServicePlusStatusModel CallRailServicePlus(OutletServicePlusReqModel MDL)
        {
            var slpusStatusMdl = new ServicePlusStatusModel
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var IsSaveOnboarding = false;
            var IsShowAPIError = false;
            var procReq = new OutletAPIStatusUpdateReq();
            if (MDL != null)
            {
                if (MDL._ValidateAPIOutletResp != null)
                {
                    var outletAPIData = MDL._ValidateAPIOutletResp;
                    var appSetting = GetApiAppSetting(outletAPIData.APICode);

                    if (MDL._ValidateAPIOutletResp.APICode.In(APICode.RPFINTECH, APICode.TPFINTECH))
                    {
                        var fintechAPIML = new FintechAPIML(_accessor, _env, outletAPIData.APICode, outletAPIData.APIID, _dal);
                        var apiResp = fintechAPIML.CheckOutletServiceStatus(new FintechAPIRequestModel
                        {
                            SPkey = outletAPIData.APIOpCode,
                            data = new OutletRequest
                            {
                                OutletID = Convert.ToInt32(outletAPIData.APIOutletID),
                                OTPRefID = MDL._ValidateAPIOutletResp.OTPRefID,
                                OTP = MDL._ValidateAPIOutletResp.OTP,
                                PidData = MDL._ValidateAPIOutletResp.PIDATA
                            }
                        }, outletAPIData.SCode);
                        if (apiResp != null)
                        {
                            if (apiResp.Statuscode == ErrorCodes.One)
                            {
                                if (apiResp.RailStatus.In(UserStatus.ACTIVE, UserStatus.APPLIED, UserStatus.REJECTED))
                                {
                                    slpusStatusMdl.IsOTPRequired = apiResp.IsOTPRequired;
                                    procReq.UserID = outletAPIData.UserID;
                                    procReq.OutletID = outletAPIData.OutletID;
                                    procReq.APIID = outletAPIData.APIID;
                                    procReq.APIOutletID = outletAPIData.APIOutletID;
                                    procReq.RailID = apiResp.RailID;
                                    procReq.RailStatus = apiResp.RailStatus;
                                    IsSaveOnboarding = true;
                                    procReq.IsRailUpdateStatus = true;
                                    if (apiResp.APIOutletStatus > 0)
                                    {
                                        procReq.APIOutletStatus = apiResp.APIOutletStatus;
                                        procReq.KYCStatus = apiResp.KYCStatus;
                                        procReq.IsVerifyStatusUpdate = true;
                                        procReq.IsDocVerifyStatusUpdate = true;
                                    }
                                }
                                else
                                {
                                    slpusStatusMdl.Msg = "(AP)" + apiResp.Msg;
                                }
                            }
                            else
                            {
                                IsShowAPIError = true;
                                if (!string.IsNullOrEmpty(apiResp.APIOutletID))
                                {
                                    procReq.UserID = outletAPIData.UserID;
                                    procReq.OutletID = outletAPIData.OutletID;
                                    procReq.APIID = outletAPIData.APIID;
                                    procReq.APIOutletID = outletAPIData.APIOutletID;
                                    if (apiResp.APIOutletStatus > 0)
                                    {
                                        procReq.APIOutletStatus = apiResp.APIOutletStatus;
                                        procReq.KYCStatus = apiResp.KYCStatus;
                                        procReq.IsVerifyStatusUpdate = true;
                                        procReq.IsDocVerifyStatusUpdate = true;
                                        IsSaveOnboarding = true;
                                    }

                                }
                                slpusStatusMdl.Msg = "(AP)" + apiResp.Msg;
                            }
                        }
                    }

                }
            }
            if (IsSaveOnboarding)
            {
                if (IsShowAPIError)
                {
                    slpusStatusMdl.Statuscode = ErrorCodes.Minus1;
                    slpusStatusMdl.Msg = slpusStatusMdl.Msg;
                    slpusStatusMdl.ErrorCode = ErrorCodes.Unknown_Error;
                }
                else
                {
                    slpusStatusMdl.Statuscode = ErrorCodes.One;
                    slpusStatusMdl.Msg = nameof(ErrorCodes.Transaction_Successful);
                    slpusStatusMdl.ErrorCode = ErrorCodes.Transaction_Successful;
                }

                IProcedure _proc = new ProcOutletAPIWiseCU(_dal);
                _proc.Call(procReq);
            }
            return slpusStatusMdl;
        }
        public OnboardingKyc GetKYCURL(int UserID, int OutletID)
        {
            var userML = new UserML(_accessor, _env, false);
            var _resp = new OnboardingKyc { Adhaar = string.Empty, Pan = string.Empty, ShopPhoto = string.Empty, PassportPhoto = string.Empty };
            var KYCList = userML.GetOnBoardKyc(UserID, OutletID);

            if (KYCList.Any())
            {
                var addhar = KYCList.Where(x => x.DocTypeID == DOCType.AADHAR).FirstOrDefault();
                if (addhar != null)
                    _resp.Adhaar = DOCType.DocFilePath + addhar.DOCURL;

                var Pan = KYCList.Where(x => x.DocTypeID == DOCType.PAN).FirstOrDefault();
                if (Pan != null)
                    _resp.Pan = DOCType.DocFilePath + Pan.DOCURL;

                var shopPhoto = KYCList.Where(x => x.DocTypeID == DOCType.ShopImage).FirstOrDefault();
                if (shopPhoto != null)
                    _resp.ShopPhoto = DOCType.DocFilePath + shopPhoto.DOCURL;

                var PassportPhoto = KYCList.Where(x => x.DocTypeID == DOCType.PHOTO).FirstOrDefault();
                if (PassportPhoto != null)
                    _resp.PassportPhoto = DOCType.DocFilePath + PassportPhoto.DOCURL;

                var CancelledCheque = KYCList.Where(x => x.DocTypeID == DOCType.CancelledCheque).FirstOrDefault();
                if (CancelledCheque != null)
                    _resp.CancelledCheque = DOCType.DocFilePath + CancelledCheque.DOCURL;
            }
            return _resp;
        }
        public async Task CallPSATransaction(TransactionServiceResp TSResp, TransactionResponse resp)
        {
            var appSetting = GetApiPSAAppSetting(TSResp.CurrentAPI.APICode);
            if (TSResp.CurrentAPI.APICode == APICode.ROUNDPAY)
            {
                var RNDPAYReq = new RoundpayApiRequestModel
                {
                    BaseUrl = appSetting.onboardingSetting.BaseURL,
                    Token = appSetting.onboardingSetting.Token,
                    totalcoupon = TSResp.TotalToken.ToString(),
                    psaid = TSResp.AccountNo,
                    agentid = TSResp.TID.ToString()
                };
                IRoundpayApiML roundpayApiML = new RoundpayApiML(_accessor, _env);
                var apiResp = await roundpayApiML.CouponRequest(RNDPAYReq).ConfigureAwait(false);
                resp.STATUS = apiResp.Status;
                resp.MSG = nameof(ErrorCodes.Transaction_Successful);
                resp.ERRORCODE = ErrorCodes.Transaction_Successful.ToString();
                resp.OPID = apiResp.LiveID ?? string.Empty;
                if (resp.STATUS == RechargeRespType.FAILED)
                {
                    if (apiResp.Msg.Contains("insuff"))
                    {
                        resp.MSG = ErrorCodes.Down;
                        resp.ERRORCODE = ErrorCodes.ServiceDown.ToString();
                    }
                    else
                    {
                        resp.MSG = nameof(ErrorCodes.Unknown_Error);
                        resp.ERRORCODE = ErrorCodes.Unknown_Error.ToString();
                    }
                }
                #region APILog
                var transactionHelper = new TransactionHelper(_dal, _accessor, _env);
                var req = new TransactionReqResp
                {
                    APIID = TSResp.CurrentAPI.ID,
                    TID = TSResp.TID,
                    Request = apiResp.Req,
                    Response = apiResp.Resp
                };
                var _ = transactionHelper.UpdateAPIResponse(req);
                #endregion
            }
            else if (TSResp.CurrentAPI.APICode == APICode.MAHAGRAM)
            {
                var MGReq = new MGCouponRequest
                {
                    psaid = TSResp.AccountNo,
                    transactionid = TSResp.TransactionID,
                    transactiondate = DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt"),
                    udf1 = TSResp.TID.ToString(),
                    udf2 = TSResp.OutletMobile,
                    udf3 = "TokenPurchage",
                    udf4 = "PSA",
                    udf5 = string.Empty
                };
                if (TSResp.OPID.Equals("Digital"))
                {
                    MGReq.totalcoupon_digital = TSResp.TotalToken.ToString();
                }
                if (TSResp.OPID.Equals("Physical"))
                {
                    MGReq.totalcoupon_physical = TSResp.TotalToken.ToString();
                }
                if ((appSetting.onboardingSetting.Token ?? string.Empty).Contains("&"))
                {
                    var setSplit = appSetting.onboardingSetting.Token.Split('&');
                    if (setSplit.Length == 2)
                    {
                        MGReq.securityKey = setSplit[0].Contains("=") ? setSplit[0].Split("=")[1] : string.Empty;
                        MGReq.createdby = setSplit[1].Contains("=") ? setSplit[1].Split("=")[1] : string.Empty;
                    }
                }
                IMahagramAPIML MGAPIML = new MahagramAPIML(_accessor, _env, appSetting.onboardingSetting.BaseURL);
                var apiResp = await MGAPIML.UTICouponRequest(MGReq).ConfigureAwait(false);
                resp.STATUS = apiResp.Status;
                resp.MSG = nameof(ErrorCodes.Transaction_Successful);
                resp.ERRORCODE = ErrorCodes.Transaction_Successful.ToString();
                resp.OPID = apiResp.LiveID ?? string.Empty;
                resp.VendorID = apiResp.VendorID ?? string.Empty;
                if (resp.STATUS == RechargeRespType.FAILED)
                {
                    if (apiResp.Msg.Contains("insuff"))
                    {
                        resp.MSG = ErrorCodes.Down;
                        resp.ERRORCODE = ErrorCodes.ServiceDown.ToString();
                    }
                    else
                    {
                        resp.MSG = nameof(ErrorCodes.Unknown_Error);
                        resp.ERRORCODE = ErrorCodes.Unknown_Error.ToString();
                    }
                }
                #region APILog
                var transactionHelper = new TransactionHelper(_dal, _accessor, _env);
                var req = new TransactionReqResp
                {
                    APIID = TSResp.CurrentAPI.ID,
                    TID = TSResp.TID,
                    Request = apiResp.Req,
                    Response = apiResp.Resp
                };
                var _ = transactionHelper.UpdateAPIResponse(req);
                #endregion
            }
            else if (TSResp.CurrentAPI.APICode.EndsWith("FNTH"))
            {
                string RequestURL = string.Format(appSetting.onboardingSetting.BaseURL, TSResp.AccountNo, TSResp.TotalToken, TSResp.CurrentAPI.APIOpCode, TSResp.TID, TSResp.CurrentAPI.APIOutletID);
                var apiStringResp = AppWebRequest.O.CallUsingHttpWebRequest_GET(RequestURL);
                if (apiStringResp != null)
                {
                    var apiResp = JsonConvert.DeserializeObject<TransactionResponse>(apiStringResp);

                    resp.STATUS = apiResp.STATUS;
                    resp.MSG = apiResp.MSG;
                    resp.ERRORCODE = apiResp.ERRORCODE;
                    resp.OPID = apiResp.OPID ?? string.Empty;
                    if (resp.STATUS == RechargeRespType.FAILED)
                    {
                        if (apiResp.MSG.Contains("insuff"))
                        {
                            resp.MSG = ErrorCodes.Down;
                            resp.ERRORCODE = ErrorCodes.ServiceDown.ToString();
                            resp.OPID = resp.MSG;
                        }
                    }
                }
                #region APILog
                var transactionHelper = new TransactionHelper(_dal, _accessor, _env);
                var req = new TransactionReqResp
                {
                    APIID = TSResp.CurrentAPI.ID,
                    TID = TSResp.TID,
                    Request = RequestURL,
                    Response = apiStringResp
                };
                var _ = transactionHelper.UpdateAPIResponse(req);
                #endregion
            }
            else if (TSResp.CurrentAPI.APICode.Equals(APICode.PANMITRA))
            {
                var pANMitraML = new PANMitraML(_accessor, _env, _dal);
                var apiResp = pANMitraML.CouponRequestAPI(new PANMitraRequest
                {
                    vle_id = TSResp.CurrentAPI.PANID,
                    qty = TSResp.TotalToken.ToString(),
                    type = TSResp.CurrentAPI.APIOpCode
                });
                resp.STATUS = apiResp.Status;
                resp.MSG = apiResp.Msg;
                resp.ERRORCODE = apiResp.ErrorCode.ToString();
                resp.OPID = apiResp.LiveID ?? string.Empty;
                if (resp.STATUS == RechargeRespType.FAILED)
                {
                    if (apiResp.Msg.Contains("insuff"))
                    {
                        resp.MSG = ErrorCodes.Down;
                        resp.ERRORCODE = ErrorCodes.ServiceDown.ToString();
                        resp.OPID = resp.MSG;
                    }
                }
                #region APILog
                var transactionHelper = new TransactionHelper(_dal, _accessor, _env);
                var req = new TransactionReqResp
                {
                    APIID = TSResp.CurrentAPI.ID,
                    TID = TSResp.TID,
                    Request = apiResp.Req,
                    Response = apiResp.Resp
                };
                var _ = transactionHelper.UpdateAPIResponse(req);
                #endregion
            }
            else if (TSResp.CurrentAPI.APICode.Equals(APICode.CASHPOINTINDIA))
            {
                var cpiML = new CashPointIndiaML(_accessor, _env, _dal);
                var apiResp = cpiML.CouponRequestAPI(new CashPointIndiaRequest
                {
                    vle_id = TSResp.CurrentAPI.PANID,
                    qty = TSResp.TotalToken.ToString(),
                    type = TSResp.CurrentAPI.APIOpCode
                });
                resp.STATUS = apiResp.Status;
                resp.MSG = apiResp.Msg;
                resp.ERRORCODE = apiResp.ErrorCode.ToString();
                resp.OPID = apiResp.LiveID ?? string.Empty;
                if (resp.STATUS == RechargeRespType.FAILED)
                {
                    if (apiResp.Msg.Contains("insuff"))
                    {
                        resp.MSG = ErrorCodes.Down;
                        resp.ERRORCODE = ErrorCodes.ServiceDown.ToString();
                        resp.OPID = resp.MSG;
                    }
                }
                #region APILog
                var transactionHelper = new TransactionHelper(_dal, _accessor, _env);
                var req = new TransactionReqResp
                {
                    APIID = TSResp.CurrentAPI.ID,
                    TID = TSResp.TID,
                    Request = apiResp.Req,
                    Response = apiResp.Resp
                };
                var _ = transactionHelper.UpdateAPIResponse(req);
                #endregion
            }
        }
        public IResponseStatus UpdateUIDTokenAgent(OutletEKYCRequest outletEKYCRequest)
        {
            var res = new ResponseStatus
            {
                ErrorCode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var yesBankML = new YesBankML(_accessor, _env, _dal);
            res = yesBankML.GetRDDataOfAgentOrSender(new Model.MoneyTransfer.MTAPIRequest
            {
                AadharNo = outletEKYCRequest.AadaarNo,
                PidData = outletEKYCRequest.PidData,
                APIOutletID = outletEKYCRequest.OutletID.ToString()
            });
            if (res.Statuscode == ErrorCodes.One)
            {
                IProcedure proc = new ProcUpdateOutletUIDToken(_dal);
                res = (ResponseStatus)proc.Call(new CommonReq
                {
                    LoginID = outletEKYCRequest.UserID,
                    CommonInt = outletEKYCRequest.OutletID,
                    CommonStr = res.CommonStr,
                    CommonStr2 = outletEKYCRequest.AadaarNo
                });
            }
            return res;
        }
        public bool ISKYCRequired(ValidateAPIOutletResp MDL)
        {
            if (MDL.APICode.EndsWith("FNTH"))
            {
                var fintechAPIML = new FintechAPIML(_accessor, _env, MDL.APICode, MDL.APIID, _dal);
                return fintechAPIML.CheckServiceReuiredKYC(MDL.APIOpCode);
            }
            return false;
        }
    }
}
