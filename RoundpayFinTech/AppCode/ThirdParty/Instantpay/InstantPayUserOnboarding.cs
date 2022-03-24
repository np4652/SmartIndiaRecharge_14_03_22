using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.StaticModel;
using Fintech.AppCode.WebRequest;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.MoneyTransfer;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.ThirdParty.Instantpay;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Xml.Serialization;

namespace RoundpayFinTech.AppCode.MiddleLayer.Dmt_Api
{
    public partial class InstantPayUserOnboarding
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration Configuration;

        private readonly string _APICode;
        private readonly int _APIID;
        private readonly IDAL _dal;
        private readonly IPaySetting apiSetting;


        public InstantPayUserOnboarding(IHttpContextAccessor accessor, IHostingEnvironment env, string APICode, int APIID, IDAL dal)
        {
            _APICode = APICode;
            _APIID = APIID;
            _accessor = accessor;
            _env = env;
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile((_env.IsProduction() ? "appsettings.json" : "appsettings.Development.json"));
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            _dal = dal;
            apiSetting = AppSetting();
        }
        public IPaySetting AppSetting()
        {
            var setting = new IPaySetting();
            try
            {
                setting = new IPaySetting
                {
                    Token = Configuration["DMR:" + _APICode + ":Token"],
                    ClientID = Configuration["DMR:" + _APICode + ":ClientID"],
                    EncryptionKey = Configuration["DMR:" + _APICode + ":EncryptionKey"],
                    ClientSecret = Configuration["DMR:" + _APICode + ":ClientSecret"],
                    ENV = Configuration["DMR:" + _APICode + ":ENV"],
                    BaseURL = Configuration["DMR:" + _APICode + ":BaseURL"],
                    AEPSBaseURL = Configuration["DMR:" + _APICode + ":AEPSBaseURL"],
                    OnboardURL = Configuration["DMR:" + _APICode + ":OnboardURL"],
                    IPAddress = Configuration["DMR:" + _APICode + ":IPAddress"]
                };
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "IPaySetting:" + (_APICode ?? string.Empty),
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                });
            }
            return setting;
        }
        private string AESEncryption(string data)
        {
            string EncryptionKey = apiSetting.EncryptionKey;
            string iniVector;
            byte[] IV = System.Text.ASCIIEncoding.ASCII.GetBytes("91543c0ce2ff7bf4");
            byte[] clearBytes = System.Text.Encoding.Unicode.GetBytes(data);
            AesCryptoServiceProvider crypt_provider;
            crypt_provider = new AesCryptoServiceProvider();
            crypt_provider.KeySize = 256;
            crypt_provider.Key = System.Text.ASCIIEncoding.ASCII.GetBytes(EncryptionKey);
            crypt_provider.IV = IV;
            crypt_provider.Mode = CipherMode.CBC;
            crypt_provider.Padding = PaddingMode.PKCS7;
            ICryptoTransform transform = crypt_provider.CreateEncryptor();
            byte[] encrypted_bytes = transform.TransformFinalBlock(System.Text.ASCIIEncoding.ASCII.GetBytes(data), 0, data.Length);
            byte[] encryptedData = new byte[encrypted_bytes.Length + IV.Length];
            IV.CopyTo(encryptedData, 0);
            encrypted_bytes.CopyTo(encryptedData, IV.Length);
            data = Convert.ToBase64String(encryptedData);
            return data;
        }
        public OutletAPIStatusUpdate SignupInitiate(ValidateAPIOutletResp _ValidateAPIOutletResp)
        {
            var _resp = new OutletAPIStatusUpdate
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var latitude = Convert.ToDouble((_ValidateAPIOutletResp.Latlong ?? string.Empty).Split(',')[0].PadRight(10, '0'));
            var longitude = Convert.ToDouble((_ValidateAPIOutletResp.Latlong ?? string.Empty).Split(',')[1].PadRight(10, '0'));
            var AadharNo = AESEncryption((_ValidateAPIOutletResp.AADHAR ?? string.Empty));
            var req = new
            {
                mobile = _ValidateAPIOutletResp.MobileNo,
                pan = _ValidateAPIOutletResp.PAN,
                email = _ValidateAPIOutletResp.EmailID,
                aadhaar = AadharNo,
                latitude = latitude,
                longitude = longitude,
                consent = "Y"
            };
            var headerDick = new Dictionary<string, string>
            {
                {"X-Ipay-Endpoint-Ip",apiSetting.IPAddress},
                {"X-Ipay-Client-Secret",apiSetting.ClientSecret??string.Empty },
                {"X-Ipay-Client-Id",apiSetting.ClientID??string.Empty },
                {"X-Ipay-Auth-Code","1" },
                {"Content-Type",ContentType.application_json }

            };
            string response = string.Empty;
            var _URL = apiSetting.OnboardURL + "user/outlet/signup/initiate";
            var request = _URL + "?H:" + JsonConvert.SerializeObject(headerDick) + "B:" + JsonConvert.SerializeObject(req);
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, req, headerDick).Result;//@"{""statuscode"":""TXN"",""actcode"":null,""status"":""Transaction Successful"",""data"":{""aadhaar"":""xxxxxxxx8758"",""otpReferenceID"":""rtrqc910p4sug3h1ti2a729i06"",""hash"":""VO+WA620GDvtxkFCMTZ5XCgmXVucbDsxYqQLY6gmg+j1A4axCaB5MRa\/Ds9UJXYWfcD5GDQvFJpNUNLVUgIPeVFCA1bKZj\/s1p7mK7XTzHczdSpq2kvNcwa4BUPN5fc\/UvsGztm+VdsfSd6t6nuf1QeOoiHbOHc6+38SHZ8Nm+g4DcjacBqbXVGN1oizxe+UvF31m8OJz4agYC0sdYaoylwygEem0Uc4JJcBJ6x3\/H61S+0t558GZHR8ZwM6G6nO""},""timestamp"":""2022-02-09 15:37:42"",""ipay_uuid"":""h005958eb32e-a96a-4bcc-b640-241238565dec"",""orderid"":""1220209153720ZZVGU"",""environment"":""LIVE""}";//
                var _apiRes = JsonConvert.DeserializeObject<IPSignupResponse>(response);
                if (_apiRes != null)
                {
                    if (_apiRes.statuscode.Equals("TXN"))
                    {
                        _resp.Statuscode = ErrorCodes.One;
                        _resp.Msg = _apiRes.status;
                        _resp.IsOTPRequired = true;
                        if (_apiRes.data != null)
                        {
                            _resp.APIReferenceID = _apiRes.data.otpReferenceID;
                            _resp.APIHash = _apiRes.data.hash;
                        }
                    }
                    else
                    {
                        _resp.Statuscode = ErrorCodes.Minus1;
                        _resp.Msg = _apiRes.status;
                    }
                }

            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "SignupInitiate",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = 1
                });
            }
            #region APILogOutletRegistration
            new ProcUpdateLogOnboardingReqResp(_dal).Call(new OnboardingLog
            {
                APIID = _APIID,
                Method = "InstantPayUserOnboarding.SignupInitiate",
                Request = request,
                Response = response

            });
            #endregion
            return _resp;
        }
        public OutletAPIStatusUpdate SignupValidate(ValidateAPIOutletResp _ValidateAPIOutletResp)
        {
            var _resp = new OutletAPIStatusUpdate
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var req = new
            {
                otpReferenceID = _ValidateAPIOutletResp.APIReferenceID,
                hash = _ValidateAPIOutletResp.APIHash,
                otp = _ValidateAPIOutletResp.OTP
            };
            var headerDick = new Dictionary<string, string>
            {
                {"X-Ipay-Endpoint-Ip",apiSetting.IPAddress },
                {"X-Ipay-Client-Secret",apiSetting.ClientSecret??string.Empty },
                {"X-Ipay-Client-Id",apiSetting.ClientID??string.Empty },
                {"X-Ipay-Auth-Code","1" },
                {"Content-Type",ContentType.application_json }

            };
            string response = string.Empty;
            var _URL = apiSetting.OnboardURL + "user/outlet/signup/validate";
            var request = _URL + "?H:" + JsonConvert.SerializeObject(headerDick) + "B:" + JsonConvert.SerializeObject(req);
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(_URL, req, headerDick).Result;
                if (!string.IsNullOrEmpty(response))
                {
                    response = response.Replace("[]", "{}");
                }
                var _apiRes = JsonConvert.DeserializeObject<IPSignupValidateResponse>(response);
                if (_apiRes != null)
                {
                    if (_apiRes.statuscode.Equals("TXN"))
                    {
                        _resp.Statuscode = ErrorCodes.One;
                        _resp.Msg = _apiRes.status;
                        if (_apiRes.data != null)
                        {
                            _resp.APIOutletID = _apiRes.data.outletId.ToString();
                            _resp.AEPSID = _resp.APIOutletID;
                            _resp.AEPSStatus = UserStatus.ACTIVE;
                            _resp.APIOutletStatus = UserStatus.ACTIVE;
                            _resp.DMTID = _resp.APIOutletID;
                            _resp.DMTStatus = UserStatus.ACTIVE;
                            _resp.KYCStatus = UserStatus.ACTIVE;
                            _resp.IsOnboardedOnAPI = true;
                        }
                    }
                    else
                    {
                        _resp.Statuscode = ErrorCodes.Minus1;
                        _resp.Msg = _apiRes.status;
                    }
                }

            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "SignupValidate",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = 1
                });
            }
            #region APILogOutletRegistration
            new ProcUpdateLogOnboardingReqResp(_dal).Call(new OnboardingLog
            {
                APIID = _APIID,
                Method = "SignupValidate",
                Request = request,
                Response = response

            });
            #endregion
            return _resp;
        }


    }
}