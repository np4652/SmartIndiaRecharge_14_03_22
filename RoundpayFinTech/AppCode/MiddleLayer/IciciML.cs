using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using Validators;
using RoundpayFinTech.AppCode.Interfaces;
using System;
using System.IO;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Model;
using Microsoft.Extensions.Configuration;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.ThirdParty.AxisBank;
using RoundpayFinTech.AppCode.Configuration;
using RoundpayFinTech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.MiddleLayer.Dmt_Api;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public class IciciML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly LoginResponse _lr;
        private readonly ISession _session;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        private readonly IUserML userML;
        private readonly IRequestInfo _rinfo;
        private readonly ICICIAppSetting _ICICIAppSetting;
        public IConfigurationRoot Configuration { get; }

        RsaKeyParameters publicKey;
        public IciciML(IHttpContextAccessor accessor, IHostingEnvironment env, bool InSession = true)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            _rinfo = new RequestInfo(_accessor, _env);
            var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile((_env.IsProduction() ? "appsettings.json" : "appsettings.Development.json"));
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            if (InSession)
            {
                _session = _accessor.HttpContext.Session;
                _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
                userML = new UserML(_lr);
            }
            _ICICIAppSetting = ICICIAppSettings();
        }

        private ICICIAppSetting ICICIAppSettings()
        {
            var iciciAppSetting = new ICICIAppSetting();
            try
            {
                iciciAppSetting.CustomerCode = Configuration["ICICICallback:CustomerCode"];
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "ICICIAppSetting",
                    Error = "AppSetting not found:" + ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return iciciAppSetting;
        }

        #region Security
        public string DycryptUsingPublicKey(byte[] data)
        {
            FileStream stream = null;
            try
            {
                string path = _env.ContentRootPath + "\\roundpay_net.crt";
                IBufferedCipher cipher = CipherUtilities.GetCipher("RSA/ECB/PKCS1Padding");
                stream = new FileStream(path, FileMode.Open);
                //stream = new FileStream(HttpContext.Current.Server.MapPath("fingpay.cer"), FileMode.Open);
                X509CertificateParser certParser = new X509CertificateParser();
                Org.BouncyCastle.X509.X509Certificate certificate = certParser.ReadCertificate(stream);
                this.publicKey = (RsaKeyParameters)certificate.GetPublicKey();
                //this.certExpiryDate = certificate.NotAfter;
                cipher.Init(true, publicKey);
                stream.Close();
                return Convert.ToBase64String(cipher.DoFinal(data));

            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public string EncryptUsingPublicKey(byte[] data)
        {
            FileStream stream = null;
            try
            {
                string path = _env.ContentRootPath + "\\Roundpaypublic.cer";
                IBufferedCipher cipher = CipherUtilities.GetCipher("RSA/ECB/PKCS1Padding");
                stream = new FileStream(path, FileMode.Open);
                //stream = new FileStream(HttpContext.Current.Server.MapPath("fingpay.cer"), FileMode.Open);
                X509CertificateParser certParser = new X509CertificateParser();
                Org.BouncyCastle.X509.X509Certificate certificate = certParser.ReadCertificate(stream);
                this.publicKey = (RsaKeyParameters)certificate.GetPublicKey();
                //this.certExpiryDate = certificate.NotAfter;
                cipher.Init(true, publicKey);
                stream.Close();
                //cipher.DoFinal()
                return Convert.ToBase64String(cipher.DoFinal(data));

            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        public byte[] ConvertFromBase64String(string data)
        {
            byte[] inputbyteArray = Convert.FromBase64String(data);
            return inputbyteArray;
        }
        public string Encryption(byte[] Data)
        {
            FileStream stream = null;
            try
            {
                string path = _env.ContentRootPath + "\\Roundpaypublic.cer";
                IBufferedCipher cipher = CipherUtilities.GetCipher("RSA/ECB/PKCS1Padding");
                stream = new FileStream(path, FileMode.Open);
                //stream = new FileStream(HttpContext.Current.Server.MapPath("fingpay.cer"), FileMode.Open);
                X509CertificateParser certParser = new X509CertificateParser();
                X509Certificate certificate = certParser.ReadCertificate(stream);

                RsaKeyParameters RSAKey1 = (RsaKeyParameters)certificate.GetPublicKey();
                RSAParameters RSAKey; //DotNetUtilities.ToRSAParameters(RSAKey1);

                byte[] encryptedData;
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {
                    // RSA.ImportParameters(RSAKey);

                    encryptedData = RSA.Encrypt(Data, false);
                }
                return Convert.ToBase64String(encryptedData);
            }
            catch (CryptographicException e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
        public string Decryption(byte[] Data)
        {
            FileStream stream = null;
            try
            {
                string path = _env.ContentRootPath + "\\roundpay_net.crt";
                IBufferedCipher cipher = CipherUtilities.GetCipher("RSA/ECB/PKCS1Padding");
                stream = new FileStream(path, FileMode.Open);
                X509CertificateParser certParser = new X509CertificateParser();
                X509Certificate certificate = certParser.ReadCertificate(stream);

                //System.Security.Cryptography.X509Certificates.X509Certificate2 certificate2 = certParser.ReadCertificate(stream);

                RsaKeyParameters RSAKey1 = (RsaKeyParameters)certificate.GetPublicKey();
                RSAParameters RSAKey;// DotNetUtilities.ToRSAParameters(RSAKey1);
                byte[] decryptedData;
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {
                    //RSA.ImportParameters(RSAKey);
                    decryptedData = RSA.Decrypt(Data, false);
                }

                return Convert.ToBase64String(decryptedData);
            }
            catch (CryptographicException e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }
        }
        #endregion

        public ICICIModelResp ValidateICICIData(ICICIModelReq req)
        {
            //12 Partial Succees,06 UTR rejected
            var resp = new ICICIModelResp
            {
                CODE = "06"
            };
            if ((req.ClientCode ?? string.Empty) == _ICICIAppSetting.CustomerCode)
            {
                req.CustomerCode = req.ClientCode;
                req.VirtualACCode = req.VirtualAccountNumber;
                req.MODE = req.Mode;
                req.SENDERREMARK = req.SenderRemark;
                req.AMT = req.Amount;
                req.CustomerAccountNo = req.ClientAccountNo;
                req.PayeeName = req.PayerName;
                req.PayeeAccountNumber = req.PayerAccNumber;
                req.PayeeBankIFSC = req.PayerBankIFSC;
                req.PayeePaymentDate = req.PayerPaymentDate;
            }
            if ((req.CustomerCode ?? string.Empty) != _ICICIAppSetting.CustomerCode)
            {
                resp.SuccessANDRejected = "Invalid CustomerCode";
                return resp;
            }
            if (!req.VirtualACCode.StartsWith(_ICICIAppSetting.CustomerCode))
            {
                resp.SuccessANDRejected = "Invalid Virtual AC Code";
                return resp;
            }
            if (req.MODE != "NEFT" && req.MODE != "RTGS" && req.MODE != "UPI" && req.MODE != "IMPS" && req.MODE != "FT")
            {
                resp.SuccessANDRejected = "Invalid MODE";
                return resp;
            }
            if ((req.UTR ?? "") == "")
            {
                resp.SuccessANDRejected = "Rejected";
                return resp;
            }
            if ((req.CustomerAccountNo ?? "") == "")
            {
                resp.SuccessANDRejected = "Invalid CustomerAccountNo";
                return resp;
            }
            if (!Validate.O.IsNumeric(req.AMT) && Convert.ToInt32(req.AMT) <= 0)
            {
                resp.SuccessANDRejected = "Invalid AMT";
                return resp;
            }
            if ((req.PayeeName ?? "") == "")
            {
                resp.SuccessANDRejected = "Invalid PayeeName";
                return resp;
            }
            if ((req.PayeeAccountNumber ?? "") == "")
            {
                resp.SuccessANDRejected = "Invalid PayeeAccountNumber";
                return resp;
            }
            if (((req.PayeeBankIFSC ?? "") == "") && (req.MODE != "FT"))
            {
                resp.SuccessANDRejected = "Invalid PayeeBankIFSC";
                return resp;
            }
            if ((req.PayeePaymentDate ?? "") == "")
            {
                resp.SuccessANDRejected = "Invalid PayeePaymentDate";
                return resp;
            }
            if ((req.BankInternalTransactionNumber ?? "") == "")
            {
                resp.SuccessANDRejected = "Invalid BankInternalTransactionNumber";
                return resp;
            }
            ///////////////////////// Update Code implementation //////////////////////////////
            IProcedure proc = new ProcICICIRequest(_dal);
            req.IP = _rinfo.GetRemoteIP();
            req.Browser = _rinfo.GetBrowserFullInfo();
            var procCallResp = (ResponseStatus)proc.Call(req);
            if (procCallResp.Statuscode == ErrorCodes.One)
            {
                resp.CODE = "11";
                resp.SuccessANDRejected = "Successfull Transaction";
                return resp;
            }
            resp.SuccessANDRejected = procCallResp.Msg;
            return resp;
        }
        public ICICIModelResp ValidateICICIDataQR(ICICIModelReq req)
        {
            var resp = new ICICIModelResp
            {
                CODE = "06"
            };
            if (!Validate.O.IsNumeric(req.AMT) && Convert.ToDecimal(req.AMT) <= 0)
            {
                resp.SuccessANDRejected = "Invalid AMT";
                return resp;
            }
            req.AMT = req.AMT.Split('.')[0];
            ///////////////////////// Update Code implementation //////////////////////////////
            IProcedure proc = new ProcICICIRequest(_dal);
            req.IP = _rinfo.GetRemoteIP();
            req.Browser = _rinfo.GetBrowserFullInfo();
            req.IsQRICICI = true;
            var procCallResp = (ResponseStatus)proc.Call(req);
            if (procCallResp.Statuscode == ErrorCodes.One)
            {
                resp.CODE = "11";
                resp.SuccessANDRejected = "Successfull Transaction";
                return resp;
            }
            resp.SuccessANDRejected = procCallResp.Msg;
            return resp;
        }
        public ICICIModelResp ValidateAxisData(AxisBankResp req)
        {
            //12 Partial Succees,06 UTR rejected
            var resp = new ICICIModelResp
            {
                CODE = "06"
            };


            if ((req.UTR ?? "") == "")
            {
                resp.SuccessANDRejected = "Invalid UTR";
                return resp;
            }
            if ((req.REMITTERACCOUNTNUMBER ?? "") == "")
            {
                resp.SuccessANDRejected = "Invalid REMITTERACCOUNTNUMBER";
                return resp;
            }
            if (!Validate.O.IsNumeric(req.AMOUNT ?? string.Empty) && Convert.ToInt32(req.AMOUNT ?? string.Empty) <= 0)
            {
                resp.SuccessANDRejected = "Invalid AMT";
                return resp;
            }




            ///////////////////////// Update Code implementation //////////////////////////////
            IProcedure proc = new ProcICICIRequest(_dal);
            var procReq = new ICICIModelReq
            {
                UTR = req.UTR,
                BankInternalTransactionNumber = req.UTR,
                CustomerCode = req.CORPCODE,
                VirtualACCode = req.REMITTERACCOUNTNUMBER,
                CustomerAccountNo = req.REMITTERACCOUNTNUMBER,
                AMT = req.AMOUNT,
                PayeeName = req.REMITTERACCOUNTNAME,
                PayeeAccountNumber = req.REMITTERACCOUNTNAME,
                PayeePaymentDate = string.Empty,
                IsAxisBank = true,
                PayeeBankIFSC = string.Empty,
                SENDERREMARK = req.DUMMY1,
                MODE = string.Empty,
                IP = _rinfo.GetRemoteIP(),
                Browser = _rinfo.GetBrowserFullInfo()
            };
            var procCallResp = (ResponseStatus)proc.Call(procReq);
            if (procCallResp.Statuscode == ErrorCodes.One)
            {
                resp.CODE = "11";
                resp.SuccessANDRejected = "Successfull Transaction";
                return resp;
            }
            resp.SuccessANDRejected = procCallResp.Msg;
            return resp;
        }

        public IResponseStatus ValidateIpayData(string ipay_id, string agent_id, string opr_id, decimal amount, string sp_key, string ssp_key, string optional1, string optional2, string optional3, string optional4, string status, string res_code, string res_msg)
        {
            var resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1
            };


            if (string.IsNullOrEmpty(opr_id))
            {
                resp.Msg = "Invalid opr_id";
                return resp;
            }
            if ((status ?? "") != RechargeRespType._SUCCESS)
            {
                resp.Msg = "Invalid status";
                return resp;
            }
            if (amount <= 0)
            {
                resp.Msg = "Invalid Amount";
                return resp;
            }
            ///////////////////////// Update Code implementation //////////////////////////////
            IProcedure proc = new ProcICICIRequest(_dal);
            var procReq = new ICICIModelReq
            {
                UTR = opr_id,
                BankInternalTransactionNumber = opr_id,
                CustomerCode = string.Empty,
                VirtualACCode = string.Empty,
                CustomerAccountNo = string.Empty,
                AMT = amount.ToString(),
                PayeeName = optional1,
                PayeeAccountNumber = optional2,
                PayeePaymentDate = string.Empty,
                IsIPay = true,
                PayeeBankIFSC = optional3,
                SENDERREMARK = optional1,
                MODE = ssp_key,
                IP = _rinfo.GetRemoteIP(),
                Browser = _rinfo.GetBrowserFullInfo()
            };
            resp = (ResponseStatus)proc.Call(procReq);

            return resp;
        }
        public IResponseStatus ValidateRazorpayData(string CustomerID, int Amount, string payment_id, string status, string LiveID)
        {
            var resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1
            };
            if (string.IsNullOrEmpty(LiveID))
            {
                resp.Msg = "Invalid opr_id";
                return resp;
            }
            if ((status ?? "") != "captured")
            {
                resp.Msg = "Invalid status";
                return resp;
            }
            if (Amount <= 0)
            {
                resp.Msg = "Invalid Amount";
                return resp;
            }
            ///////////////////////// Update Code implementation //////////////////////////////
            IProcedure proc = new ProcICICIRequest(_dal);
            var procReq = new ICICIModelReq
            {
                UTR = LiveID,
                BankInternalTransactionNumber = LiveID,
                CustomerCode = string.Empty,
                VirtualACCode = CustomerID,
                CustomerAccountNo = string.Empty,
                AMT = Amount.ToString(),
                IsRazorpay = true,
                IP = _rinfo.GetRemoteIP(),
                Browser = _rinfo.GetBrowserFullInfo()
            };
            resp = (ResponseStatus)proc.Call(procReq);

            return resp;
        }
        public IResponseStatus ValidateCashfreeData(string CustomerID, string Amount, string payment_id, string status, string LiveID)
        {
            var resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1
            };
            if (string.IsNullOrEmpty(LiveID))
            {
                resp.Msg = "Invalid opr_id";
                return resp;
            }
            if ((status ?? "") != "AMOUNT_COLLECTED")
            {
                resp.Msg = "Invalid status";
                return resp;
            }
            if (Convert.ToDouble((string.IsNullOrEmpty(Amount) ? "0" : Amount)) <= 0)
            {
                resp.Msg = "Invalid Amount";
                return resp;
            }
            CashFreePayoutML cashFreePayoutML = new CashFreePayoutML(_accessor, _env, _dal, string.Empty);
            var stsCheckResp = cashFreePayoutML.StatusCheckICollect(payment_id);
            if (stsCheckResp.Statuscode == ErrorCodes.Minus1)
            {
                resp.Msg = stsCheckResp.Msg;
                return resp;
            }
            else
            {
                if (stsCheckResp.Amount != Amount)
                {
                    resp.Msg = "Invalid Request";
                    return resp;
                }
                if (stsCheckResp.UTR != LiveID)
                {
                    resp.Msg = "Invalid Request";
                    return resp;
                }
            }
            IProcedure proc = new ProcICICIRequest(_dal);
            var procReq = new ICICIModelReq
            {
                UTR = LiveID,
                BankInternalTransactionNumber = LiveID,
                CustomerCode = string.Empty,
                VirtualACCode = CustomerID,
                CustomerAccountNo = string.Empty,
                AMT = Amount,
                IP = _rinfo.GetRemoteIP(),
                CollectType = CollectTypes.CashFree,
                Browser = _rinfo.GetBrowserFullInfo()
            };
            resp = (ResponseStatus)proc.Call(procReq);

            return resp;
        }
    }


    #region Model
    public class ICICIModelReq
    {
        public string CustomerCode { get; set; }
        public string ClientCode { get; set; }//new
        public string VirtualACCode { get; set; }
        public string VirtualAccountNumber { get; set; }//new
        public string MODE { get; set; }
        public string Mode { get; set; } //new 
        public string UTR { get; set; }
        public string SENDERREMARK { get; set; }
        public string SenderRemark { get; set; } //new
        public string CustomerAccountNo { get; set; }
        public string ClientAccountNo { get; set; } //new
        public string AMT { get; set; }
        public string Amount { get; set; } //new
        public string PayeeName { get; set; }
        public string PayerName { get; set; } //new
        public string PayeeAccountNumber { get; set; }
        public string PayerAccNumber { get; set; }//new 
        public string PayeeBankIFSC { get; set; }
        public string PayerBankIFSC { get; set; }//new
        public string PayeePaymentDate { get; set; }
        public string PayerPaymentDate { get; set; }//new
        public string BankInternalTransactionNumber { get; set; }
        public string IP { get; set; }//Internal vairables
        public string Browser { get; set; }//Internal vairables
        public bool IsAxisBank { get; set; }
        public bool IsQRICICI { get; set; }
        public bool IsIPay { get; set; }
        public bool IsRazorpay { get; set; }
        public int CollectType { get; set; }
    }

    public class ICICIModelResp
    {
        [JsonProperty("SuccessANDRejected")]
        public string SuccessANDRejected { get; set; }
        [JsonProperty("CODE")]
        public string CODE { get; set; }
    }
    public class ICICIModelRespp
    {
        [JsonProperty("Response")]
        public string SuccessANDRejected { get; set; }
        [JsonProperty("Code")]
        public string CODE { get; set; }
    }
    public class ICICIAppSetting
    {
        public string CustomerCode { get; set; }
        public string BC { get; set; }
        public string PassCode { get; set; }
        public string BaseURL { get; set; }
    }
    #endregion
}
