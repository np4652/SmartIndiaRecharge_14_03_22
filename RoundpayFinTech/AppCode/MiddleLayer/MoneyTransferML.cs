using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer.Dmt_Api;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.MoneyTransfer;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.StaticModel.MoneyTransfer;
using RoundpayFinTech.AppCode.ThirdParty.CyberPlate;
using RoundpayFinTech.AppCode.ThirdParty.Mahagram;
using RoundpayFinTech.AppCode.ThirdParty.Paytm;
using RoundpayFinTech.AppCode.ThirdParty.Zoop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using Validators;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public sealed class MoneyTransferML : IMoneyTransferML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration Configuration;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        private readonly IRequestInfo _info;
        private readonly UserML userML;
        public MoneyTransferML(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            _info = new RequestInfo(_accessor, _env);
            userML = new UserML(_accessor, _env, false);
            var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile(_env.IsProduction() ? "appsettings.json" : "appsettings.Development.json");
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }
        private ValidateAPIOutletResp GetOutletNew(int UserID, int OutletID, int OID, string SPKey = "")
        {
            var _req = new CommonReq
            {
                LoginID = UserID,
                CommonInt = OutletID,
                CommonInt2 = OID,
                CommonStr = SPKey ?? string.Empty
            };

            IProcedure _proc = new ProcValidateOutletForOperator(_dal);
            var resp = (ValidateAPIOutletResp)_proc.Call(_req);
            if (resp.Statuscode == ErrorCodes.Minus1)
            {
                return resp;
            }
            if (!resp.OPTypeID.In(OPTypes.DMR, OPTypes.Verification))
            {
                resp.Statuscode = ErrorCodes.Minus1;
                resp.Msg = "Invalid operator selection";
                resp.ErrorCode = 102;
                return resp;
            }
            if ((resp.APICode.Equals(APICode.RPFINTECH) || resp.APICode.EndsWith("FNTH")) && string.IsNullOrEmpty(resp.APIOpCode))
            {
                resp.Statuscode = ErrorCodes.Minus1;
                resp.Msg = ErrorCodes.Down;
                resp.ErrorCode = ErrorCodes.ServiceDown;
                return resp;
            }

            if (resp.IsOutletRequired)
            {
                if ((resp.FixedOutletID ?? string.Empty) != string.Empty)
                {
                    if (resp.DMTStatus != UserStatus.ACTIVE)
                    {
                        resp.DMTID = resp.FixedOutletID;
                        resp.DMTStatus = UserStatus.ACTIVE;
                        return resp;
                    }
                }
                else
                {
                    if (resp.DMTStatus != UserStatus.ACTIVE)
                    {
                        resp.Statuscode = ErrorCodes.Minus1;
                        resp.Msg = ErrorCodes.KYCPENDING;
                        resp.ErrorCode = ErrorCodes.Document_Applied;
                        return resp;
                    }
                }
                if ((resp.DMTID ?? string.Empty) == string.Empty)
                {
                    resp.Statuscode = ErrorCodes.Minus1;
                    resp.Msg = ErrorCodes.KYCPENDING;
                    resp.ErrorCode = ErrorCodes.Document_Applied;
                    return resp;
                }
            }
            if ((resp.FixedOutletID ?? string.Empty) != string.Empty && (resp.DMTID ?? string.Empty) == string.Empty)
            {
                resp.DMTID = resp.FixedOutletID;
                resp.DMTStatus = UserStatus.ACTIVE;
            }
            return resp;
        }
        public MSenderLoginResponse GetSender(MTCommonRequest request)
        {
            var senderLoginResponse = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Unknown_Error
            };
            if (!Validate.O.IsMobile(request.SenderMobile ?? string.Empty) || request.SenderMobile.Length < 10)
            {
                senderLoginResponse.Msg = ErrorCodes.InvalidParam + " Sender Mobile Number";
                return senderLoginResponse;
            }
            var _ValResp = GetOutletNew(request.UserID, request.OutletID, request.OID);
            if (_ValResp.Statuscode != ErrorCodes.One)
            {
                senderLoginResponse.Msg = _ValResp.Msg;
                senderLoginResponse.Statuscode = _ValResp.Statuscode;
                senderLoginResponse.ErrorCode = _ValResp.ErrorCode;
                return senderLoginResponse;
            }
            var tAPIRequest = new MTAPIRequest
            {
                APIGroupCode = _ValResp.APIGroupCode,
                APIOpCode = _ValResp.APIOpCode,
                APIID = _ValResp.APIID,
                TransactionID = _ValResp.TransactionID,
                APIOutletID = _ValResp.DMTID,
                SenderMobile = request.SenderMobile,
                RequestMode = request.RequestMode,
                UserID = request.UserID,
                ReferenceID = request.ReferenceID
            };
            try
            {
                if (_ValResp.APICode == APICode.AMAH)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new MobileWareML(_accessor, _env);
                    senderLoginResponse = moneyTransferAPIML.GetSender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.MRUY)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new MrupayML(_accessor, _env);
                    senderLoginResponse = moneyTransferAPIML.GetSender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.AIRTELBANK)/*airtel bank*/
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new AirtelBankML(_accessor, _env);
                    senderLoginResponse = moneyTransferAPIML.GetSender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.OPENBANK)/*open bank*/
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new OpenBankML(_accessor, _env);
                    senderLoginResponse = moneyTransferAPIML.GetSender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.ICICIBANK)/*icici bank*/
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new ICICIML(_accessor, _env);
                    senderLoginResponse = moneyTransferAPIML.GetSender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.ICICIBANKPAYOUT)/*icici bank*/
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new ICICIPayoutML(_accessor, _env, tAPIRequest.APIID);
                    senderLoginResponse = moneyTransferAPIML.GetSender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.PAYTM)/*paytm bank*/
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new PaytmML(_accessor, _env, _dal);
                    senderLoginResponse = moneyTransferAPIML.GetSender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.EKO)/*EKO*/
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new EKOML(_accessor, _env);
                    senderLoginResponse = moneyTransferAPIML.GetSender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.EKO2)/*EKO2*/
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new EKO2ML(_accessor, _env, _dal, tAPIRequest.APIID);
                    senderLoginResponse = moneyTransferAPIML.GetSender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.CYBERPLATPayTM)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new CyberPlateML(_dal, _ValResp.APIID);
                    senderLoginResponse = moneyTransferAPIML.GetSender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.CYBERPLAT)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new CyberPlateMLIPay(_dal);
                    senderLoginResponse = moneyTransferAPIML.GetSender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.MAHAGRAM)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new MahagramAPIML(_accessor, _env, _dal);
                    senderLoginResponse = moneyTransferAPIML.GetSender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.YESBANK)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new YesBankML(_accessor, _env, _dal);
                    senderLoginResponse = moneyTransferAPIML.GetSender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.PAYU)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new PayUML(_accessor, _env, _dal);
                    senderLoginResponse = moneyTransferAPIML.GetSender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.PAYUDMT)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new PayUDmtML(_accessor, _env, _dal);
                    senderLoginResponse = moneyTransferAPIML.GetSender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.INSTANTPayDirect)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new IPayPayoutDirectML(_accessor, _env, _dal);
                    senderLoginResponse = moneyTransferAPIML.GetSender(tAPIRequest);
                }
                else if (_ValResp.APICode.EndsWith(APICode.RAZORPAYOUT))
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new RZRPayoutML(_accessor, _env, _dal, _ValResp.APICode);
                    senderLoginResponse = moneyTransferAPIML.GetSender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.RECHARGEDADDY)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new RechargeDaddyML(_accessor, _env, _dal);
                    senderLoginResponse = moneyTransferAPIML.GetSender(tAPIRequest);
                }
                else if (_ValResp.APICode.Equals(APICode.PayOneMoney))
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new PayOneMoneyML(_accessor, _env, _dal);
                    senderLoginResponse = moneyTransferAPIML.GetSender(tAPIRequest);
                }
                else if (_ValResp.APICode.Equals(APICode.RPFINTECH) || _ValResp.APICode.EndsWith("FNTH"))
                {
                    if (_ValResp.APIOpCode.Equals(APIOPCODE.RPXPRESS) || _ValResp.APIOpCode.EndsWith("XPRESS"))
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new FintechPayoutML(_accessor, _env, _dal, _ValResp.APICode);
                        senderLoginResponse = moneyTransferAPIML.GetSender(tAPIRequest);
                    }
                    else
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new FintechAPIML(_accessor, _env, _ValResp.APICode, tAPIRequest.APIID, _dal);
                        senderLoginResponse = moneyTransferAPIML.GetSender(tAPIRequest);
                    }
                }
                else if (_ValResp.APICode == APICode.RBLMT)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new RBLML(_accessor, _env, _dal);
                    senderLoginResponse = moneyTransferAPIML.GetSender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.BILLAVENUE)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new BillAvenueMT_ML(_accessor, _env, APICode.BILLAVENUE, tAPIRequest.APIID, _dal);
                    senderLoginResponse = moneyTransferAPIML.GetSender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.INSTANTPAY)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new InstantPay_ML(_accessor, _env, APICode.INSTANTPAY, tAPIRequest.APIID, _dal);
                    senderLoginResponse = moneyTransferAPIML.GetSender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.EKOPAYOUT)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new EKOPayoutML(_accessor, _env, _dal);
                    senderLoginResponse = moneyTransferAPIML.GetSender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.Manual)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new ManualDMT_ML(_accessor, _env, _dal, APICode.Manual);
                    senderLoginResponse = moneyTransferAPIML.GetSender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.SECUREPAYMENT)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new SecurePaymentML(_accessor, _env, _dal, APICode.SECUREPAYMENT);
                    senderLoginResponse = moneyTransferAPIML.GetSender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.MMWFintech)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new MMWFintechML(_accessor, _env, APICode.MMWFintech, _ValResp.APIID, _dal);
                    senderLoginResponse = moneyTransferAPIML.GetSender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.SPRINT)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new SprintDMTML(_accessor, _env, APICode.SPRINT, _ValResp.APIID, _dal);
                    senderLoginResponse = moneyTransferAPIML.GetSender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.GOTERPay)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new GoterPayML(_accessor, _env, APICode.GOTERPay, _ValResp.APIID, _dal);
                    senderLoginResponse = moneyTransferAPIML.GetSender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.FINO)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new FINOML(_accessor, _env, _dal);
                    senderLoginResponse = moneyTransferAPIML.GetSender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.CASHFREE)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new CashFreePayoutML(_accessor, _env, _dal, _ValResp.APIGroupCode);
                    senderLoginResponse = moneyTransferAPIML.GetSender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.HYPTO)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new HyptoML(_accessor, _env, tAPIRequest.APIID, _dal);
                    senderLoginResponse = moneyTransferAPIML.GetSender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.FINODIN)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new FinodinML(_accessor, _env, _dal, APICode.FINODIN, _ValResp.APIID, _ValResp.APIGroupCode);
                    senderLoginResponse = moneyTransferAPIML.GetSender(tAPIRequest);
                }
                else
                {
                    senderLoginResponse.Msg = ErrorCodes.Down;
                    return senderLoginResponse;
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "CheckSender",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = tAPIRequest.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return senderLoginResponse;
        }
        public MSenderCreateResp SenderKYC(MTSenderDetail request)
        {
            var mSenderCreateResp = new MSenderCreateResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Unknown_Error
            };

            if (Validate.O.IsNumeric(request.NameOnKYC ?? string.Empty) || (request.NameOnKYC ?? string.Empty).Length > 100)
            {
                mSenderCreateResp.Msg = ErrorCodes.InvalidParam + " Name";
                return mSenderCreateResp;
            }
            if (!Validate.O.IsAADHAR(request.AadharNo ?? string.Empty))
            {
                mSenderCreateResp.Msg = ErrorCodes.InvalidParam + " Aadhar Number";
                return mSenderCreateResp;
            }
            if (!Validate.O.IsPAN(request.PANNo ?? string.Empty))
            {
                mSenderCreateResp.Msg = ErrorCodes.InvalidParam + " PAN Number";
                return mSenderCreateResp;
            }
            else
            {
                request.PANNo = request.PANNo.ToUpper();
            }

            if (!Validate.O.IsMobile(request.SenderMobile ?? string.Empty) || request.SenderMobile.Length < 10)
            {
                mSenderCreateResp.Msg = ErrorCodes.InvalidParam + "Sender Mobile Number";
                return mSenderCreateResp;
            }
            var AadharFrontName = ContentDispositionHeaderValue.Parse(request.AadharFront.ContentDisposition).FileName.Trim('"').Replace(" ", "");
            var AadharBackName = ContentDispositionHeaderValue.Parse(request.AadharBack.ContentDisposition).FileName.Trim('"').Replace(" ", "");
            var SenderPhotoName = ContentDispositionHeaderValue.Parse(request.SenderPhoto.ContentDisposition).FileName.Trim('"').Replace(" ", "");
            var PANName = ContentDispositionHeaderValue.Parse(request.PAN.ContentDisposition).FileName.Trim('"').Replace(" ", "");
            byte[] AadharFrontContent = null, AadharBackContent = null, SenderPhotoContent = null, PANContent = null;

            using (var ms = new MemoryStream())
            {
                request.AadharFront.CopyTo(ms);
                AadharFrontContent = ms.ToArray();
            }
            if (!Validate.O.IsFileAllowed(AadharFrontContent, Path.GetExtension(AadharFrontName)))
            {
                mSenderCreateResp.Msg = "Aadhar front image is not valid";
                return mSenderCreateResp;
            }
            using (var ms = new MemoryStream())
            {
                request.AadharBack.CopyTo(ms);
                AadharBackContent = ms.ToArray();
            }
            if (!Validate.O.IsFileAllowed(AadharBackContent, Path.GetExtension(AadharBackName)))
            {
                mSenderCreateResp.Msg = "Aadhar back image is not valid";
                return mSenderCreateResp;
            }
            using (var ms = new MemoryStream())
            {
                request.SenderPhoto.CopyTo(ms);
                SenderPhotoContent = ms.ToArray();
            }
            if (!Validate.O.IsFileAllowed(SenderPhotoContent, Path.GetExtension(SenderPhotoName)))
            {
                mSenderCreateResp.Msg = "Sender photo is not valid";
                return mSenderCreateResp;
            }
            using (var ms = new MemoryStream())
            {
                request.PAN.CopyTo(ms);
                PANContent = ms.ToArray();
            }
            if (!Validate.O.IsFileAllowed(PANContent, Path.GetExtension(PANName)))
            {
                mSenderCreateResp.Msg = "PAN Image is not valid";
                return mSenderCreateResp;
            }

            var _ValResp = GetOutletNew(request.UserID, request.OutletID, request.OID);
            if (_ValResp.Statuscode != ErrorCodes.One)
            {
                mSenderCreateResp.Msg = _ValResp.Msg;
                mSenderCreateResp.Statuscode = _ValResp.Statuscode;
                mSenderCreateResp.ErrorCode = _ValResp.ErrorCode;
                return mSenderCreateResp;
            }
            var tAPIRequest = new MTAPIRequest
            {
                APIGroupCode = _ValResp.APIGroupCode,
                APIOpCode = _ValResp.APIOpCode,
                APIID = _ValResp.APIID,
                TransactionID = _ValResp.TransactionID,
                APIOutletID = _ValResp.DMTID,
                SenderMobile = request.SenderMobile,
                RequestMode = request.RequestMode,
                ReferenceID = request.ReferenceID,
                NameOnKYC = request.NameOnKYC,
                AadharNo = request.AadharNo,
                PANNo = request.PANNo,
                UserID = request.UserID
            };
            try
            {

                if (_ValResp.APICode == APICode.EKO)
                {
                    tAPIRequest.AadharFrontURL = DOCType.SenderKYCPath + request.SenderMobile + nameof(request.AadharFront) + AadharFrontName;
                    tAPIRequest.AadharBackURL = DOCType.SenderKYCPath + request.SenderMobile + nameof(request.AadharBack) + AadharBackName;
                    tAPIRequest.SenderPhotoURL = DOCType.SenderKYCPath + request.SenderMobile + nameof(request.SenderPhoto) + SenderPhotoName;
                    tAPIRequest.PANURL = DOCType.SenderKYCPath + request.SenderMobile + nameof(request.PAN) + PANName;
                    using (FileStream fs = File.Create(tAPIRequest.AadharFrontURL))
                    {
                        request.AadharFront.CopyTo(fs);
                        fs.Flush();
                    }
                    using (FileStream fs = File.Create(tAPIRequest.AadharBackURL))
                    {
                        request.AadharBack.CopyTo(fs);
                        fs.Flush();
                    }
                    using (FileStream fs = File.Create(tAPIRequest.SenderPhotoURL))
                    {
                        request.SenderPhoto.CopyTo(fs);
                        fs.Flush();
                    }
                    using (FileStream fs = File.Create(tAPIRequest.PANURL))
                    {
                        request.PAN.CopyTo(fs);
                        fs.Flush();
                    }
                    IMoneyTransferAPIML moneyTransferAPIML = new EKOML(_accessor, _env);
                    mSenderCreateResp = moneyTransferAPIML.SenderKYC(tAPIRequest);
                    try
                    {
                        File.SetAttributes(tAPIRequest.AadharFrontURL, FileAttributes.Normal);
                        File.Delete(tAPIRequest.AadharFrontURL);
                        File.SetAttributes(tAPIRequest.AadharBackURL, FileAttributes.Normal);
                        File.Delete(tAPIRequest.AadharBackURL);
                        File.SetAttributes(tAPIRequest.SenderPhotoURL, FileAttributes.Normal);
                        File.Delete(tAPIRequest.SenderPhotoURL);
                        File.SetAttributes(tAPIRequest.PANURL, FileAttributes.Normal);
                        File.Delete(tAPIRequest.PANURL);
                    }
                    catch (Exception)
                    {

                    }
                }
                if (_ValResp.APICode == APICode.EKO2)
                {
                    tAPIRequest.AadharFrontURL = DOCType.SenderKYCPath + request.SenderMobile + nameof(request.AadharFront) + AadharFrontName;
                    tAPIRequest.AadharBackURL = DOCType.SenderKYCPath + request.SenderMobile + nameof(request.AadharBack) + AadharBackName;
                    tAPIRequest.SenderPhotoURL = DOCType.SenderKYCPath + request.SenderMobile + nameof(request.SenderPhoto) + SenderPhotoName;
                    tAPIRequest.PANURL = DOCType.SenderKYCPath + request.SenderMobile + nameof(request.PAN) + PANName;
                    using (FileStream fs = File.Create(tAPIRequest.AadharFrontURL))
                    {
                        request.AadharFront.CopyTo(fs);
                        fs.Flush();
                    }
                    using (FileStream fs = File.Create(tAPIRequest.AadharBackURL))
                    {
                        request.AadharBack.CopyTo(fs);
                        fs.Flush();
                    }
                    using (FileStream fs = File.Create(tAPIRequest.SenderPhotoURL))
                    {
                        request.SenderPhoto.CopyTo(fs);
                        fs.Flush();
                    }
                    using (FileStream fs = File.Create(tAPIRequest.PANURL))
                    {
                        request.PAN.CopyTo(fs);
                        fs.Flush();
                    }
                    IMoneyTransferAPIML moneyTransferAPIML = new EKO2ML(_accessor, _env, _dal, tAPIRequest.APIID);
                    mSenderCreateResp = moneyTransferAPIML.SenderKYC(tAPIRequest);
                    try
                    {
                        File.SetAttributes(tAPIRequest.AadharFrontURL, FileAttributes.Normal);
                        File.Delete(tAPIRequest.AadharFrontURL);
                        File.SetAttributes(tAPIRequest.AadharBackURL, FileAttributes.Normal);
                        File.Delete(tAPIRequest.AadharBackURL);
                        File.SetAttributes(tAPIRequest.SenderPhotoURL, FileAttributes.Normal);
                        File.Delete(tAPIRequest.SenderPhotoURL);
                        File.SetAttributes(tAPIRequest.PANURL, FileAttributes.Normal);
                        File.Delete(tAPIRequest.PANURL);
                    }
                    catch (Exception)
                    {

                    }
                }
                else
                {
                    mSenderCreateResp.Msg = ErrorCodes.Down;
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "DoSenderKYC",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return mSenderCreateResp;
        }
        public MSenderCreateResp SenderEKYC(MTSenderDetail request)
        {
            var mSenderCreateResp = new MSenderCreateResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Unknown_Error
            };

            if (Validate.O.IsNumeric(request.NameOnKYC ?? string.Empty) || (request.NameOnKYC ?? string.Empty).Length > 100)
            {
                mSenderCreateResp.Msg = ErrorCodes.InvalidParam + " Name";
                return mSenderCreateResp;
            }
            if (!Validate.O.IsAADHAR(request.AadharNo ?? string.Empty))
            {
                mSenderCreateResp.Msg = ErrorCodes.InvalidParam + " Aadhar Number";
                return mSenderCreateResp;
            }
            if (!Validate.O.IsMobile(request.SenderMobile ?? string.Empty) || request.SenderMobile.Length < 10)
            {
                mSenderCreateResp.Msg = ErrorCodes.InvalidParam + "Sender Mobile Number";
                return mSenderCreateResp;
            }
            if (string.IsNullOrEmpty(request.PidData))
            {
                mSenderCreateResp.Msg = "Invalid finger print detail";
                return mSenderCreateResp;
            }

            var _ValResp = GetOutletNew(request.UserID, request.OutletID, request.OID);
            if (_ValResp.Statuscode != ErrorCodes.One)
            {
                mSenderCreateResp.Msg = _ValResp.Msg;
                mSenderCreateResp.Statuscode = _ValResp.Statuscode;
                mSenderCreateResp.ErrorCode = _ValResp.ErrorCode;
                return mSenderCreateResp;
            }
            var tAPIRequest = new MTAPIRequest
            {
                APIGroupCode = _ValResp.APIGroupCode,
                APIOpCode = _ValResp.APIOpCode,
                APIID = _ValResp.APIID,
                TransactionID = _ValResp.TransactionID,
                APIOutletID = _ValResp.DMTID,
                SenderMobile = request.SenderMobile,
                RequestMode = request.RequestMode,
                ReferenceID = request.ReferenceID,
                NameOnKYC = request.NameOnKYC,
                AadharNo = request.AadharNo,
                PidData = request.PidData,
                UserID = request.UserID
            };
            try
            {
                if (_ValResp.APICode == APICode.YESBANK)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new YesBankML(_accessor, _env, _dal);
                    mSenderCreateResp = moneyTransferAPIML.SenderKYC(tAPIRequest);
                }
                else
                {
                    mSenderCreateResp.Msg = ErrorCodes.Down;
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "DoSenderKYC",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return mSenderCreateResp;
        }
        public MSenderCreateResp CreateSender(MTSenderDetail request)
        {
            var senderCreateResp = new MSenderCreateResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Unknown_Error
            };

            if (Validate.O.IsNumeric(request.FName ?? string.Empty) || (request.FName ?? "").Length > 100 || (request.FName ?? "").Length < 4 || Validate.O.HasSpecialChar(request.FName ?? string.Empty))
            {
                senderCreateResp.Msg = ErrorCodes.InvalidParam + " FirstName";
                return senderCreateResp;
            }
            if (Validate.O.IsNumeric(request.LName ?? "") || (request.LName ?? "").Length > 100 || Validate.O.HasSpecialChar(request.FName ?? string.Empty))
            {
                senderCreateResp.Msg = ErrorCodes.InvalidParam + " LastName";
                return senderCreateResp;
            }
            if (request.Pincode.ToString().Length != 6)
            {
                senderCreateResp.Msg = ErrorCodes.InvalidParam + " Pincode";
                return senderCreateResp;
            }
            if (string.IsNullOrWhiteSpace(request.Address))
            {
                senderCreateResp.Msg = ErrorCodes.InvalidParam + " Address";
                return senderCreateResp;
            }
            if (request.Address.Length > 150)
            {
                senderCreateResp.Msg = ErrorCodes.InvalidParam + " Address. Length not greater than 150";
                return senderCreateResp;
            }
            if (string.IsNullOrWhiteSpace(request.DOB) || !Validate.O.IsDateIn_dd_MMM_yyyy_Format(request.DOB ?? ""))
            {
                senderCreateResp.Msg = ErrorCodes.InvalidParam + " Date of brith";
                return senderCreateResp;
            }
            if (!Validate.O.IsUserAdult(DateTime.Today.Year, Convert.ToDateTime(request.DOB).Year))
            {
                senderCreateResp.Msg = ErrorCodes.InvalidParam + " User Should be 18!";
                return senderCreateResp;
            }
            if (!Validate.O.IsMobile(request.SenderMobile ?? string.Empty) || request.SenderMobile.Length < 10)
            {
                senderCreateResp.Msg = ErrorCodes.InvalidParam + "Sender Mobile Number";
                return senderCreateResp;
            }
            var userML = new UserML(_accessor, _env, false);
            var pincodeDetail = userML.GetPinCodeDetail(request.Pincode.ToString());
            if (string.IsNullOrWhiteSpace(pincodeDetail.City))
            {
                senderCreateResp.Msg = ErrorCodes.InvalidParam + " Pincode";
                return senderCreateResp;
            }
            var _ValResp = GetOutletNew(request.UserID, request.OutletID, request.OID);
            if (_ValResp.Statuscode != ErrorCodes.One)
            {
                senderCreateResp.Msg = _ValResp.Msg;
                senderCreateResp.Statuscode = _ValResp.Statuscode;
                senderCreateResp.ErrorCode = _ValResp.ErrorCode;
                return senderCreateResp;
            }

            var tAPIRequest = new MTAPIRequest
            {
                APIGroupCode = _ValResp.APIGroupCode,
                APIOpCode = _ValResp.APIOpCode,
                APIID = _ValResp.APIID,
                TransactionID = _ValResp.TransactionID,
                APIOutletID = _ValResp.DMTID,
                SenderMobile = request.SenderMobile,
                RequestMode = request.RequestMode,
                UserID = request.UserID,
                FirstName = request.FName ?? string.Empty,
                LastName = request.LName ?? string.Empty,
                Address = request.Address ?? string.Empty,
                Pincode = request.Pincode,
                City = pincodeDetail.Districtname ?? string.Empty,
                StateID = pincodeDetail.StateID,
                StateName = pincodeDetail.StateName ?? string.Empty,
                Districtname = pincodeDetail.Districtname ?? string.Empty,
                Area = pincodeDetail.Area ?? string.Empty,
                DOB = request.DOB ?? string.Empty,
                ReferenceID = request.ReferenceID ?? string.Empty,
                OTP = request.OTP ?? string.Empty
            };
            try
            {
                if (_ValResp.APICode == APICode.AMAH)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new MobileWareML(_accessor, _env);
                    senderCreateResp = moneyTransferAPIML.CreateSender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.MRUY)
                {

                    IMoneyTransferAPIML moneyTransferAPIML = new MrupayML(_accessor, _env);
                    senderCreateResp = moneyTransferAPIML.CreateSender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.AIRTELBANK)/*airtel create sender*/
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new AirtelBankML(_accessor, _env);
                    senderCreateResp = moneyTransferAPIML.CreateSender(tAPIRequest);
                    if (senderCreateResp.IsOTPGenerated)
                    {
                        OTPForDMT(senderCreateResp.OTP, tAPIRequest.SenderMobile, string.Empty, senderCreateResp.WID, request.UserID);
                    }
                }
                else if (_ValResp.APICode == APICode.OPENBANK)/*open bank create sender*/
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new OpenBankML(_accessor, _env);
                    senderCreateResp = moneyTransferAPIML.CreateSender(tAPIRequest);
                    if (senderCreateResp.IsOTPGenerated)
                    {
                        OTPForDMT(senderCreateResp.OTP, tAPIRequest.SenderMobile, string.Empty, senderCreateResp.WID, request.UserID);
                    }
                }
                else if (_ValResp.APICode == APICode.ICICIBANK)/*icici bank create sender*/
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new ICICIML(_accessor, _env);
                    senderCreateResp = moneyTransferAPIML.CreateSender(tAPIRequest);
                    if (senderCreateResp.IsOTPGenerated)
                    {
                        OTPForDMT(senderCreateResp.OTP, tAPIRequest.SenderMobile, string.Empty, senderCreateResp.WID, request.UserID);
                    }
                }
                else if (_ValResp.APICode == APICode.ICICIBANKPAYOUT)/*icici bank*/
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new ICICIPayoutML(_accessor, _env, tAPIRequest.APIID);
                    senderCreateResp = moneyTransferAPIML.CreateSender(tAPIRequest);
                    if (senderCreateResp.IsOTPGenerated)
                    {
                        OTPForDMT(senderCreateResp.OTP, tAPIRequest.SenderMobile, string.Empty, senderCreateResp.WID, request.UserID);
                    }
                }
                else if (_ValResp.APICode == APICode.PAYTM)/*paytm create sender*/
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new PaytmML(_accessor, _env, _dal);
                    senderCreateResp = moneyTransferAPIML.CreateSender(tAPIRequest);
                    if (senderCreateResp.IsOTPGenerated)
                    {
                        OTPForDMT(senderCreateResp.OTP, tAPIRequest.SenderMobile, string.Empty, senderCreateResp.WID, request.UserID);
                    }
                }
                else if (_ValResp.APICode == APICode.EKO)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new EKOML(_accessor, _env);
                    senderCreateResp = moneyTransferAPIML.CreateSender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.EKO2)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new EKO2ML(_accessor, _env, _dal, tAPIRequest.APIID);
                    senderCreateResp = moneyTransferAPIML.CreateSender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.CYBERPLATPayTM)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new CyberPlateML(_dal, _ValResp.APIID);
                    senderCreateResp = moneyTransferAPIML.GenerateOTP(tAPIRequest);
                    if (senderCreateResp.Statuscode == ErrorCodes.One)
                    {
                        var senderRequest = new SenderRequest
                        {
                            Address = tAPIRequest.Address,
                            Name = tAPIRequest.FirstName + " " + tAPIRequest.LastName,
                            Area = tAPIRequest.Area,
                            MobileNo = tAPIRequest.SenderMobile,
                            Pincode = tAPIRequest.Pincode.ToString(),
                            ReffID = senderCreateResp.ReferenceID,
                            UserID = tAPIRequest.UserID,
                            Dob = tAPIRequest.DOB,
                            StateID = tAPIRequest.StateID,
                            City = tAPIRequest.City
                        };
                        IProcedure _proc = new ProcUpdateSender(_dal);
                        var senderInfo = (SenderInfo)_proc.Call(senderRequest);
                        if (senderInfo.Statuscode == ErrorCodes.One)
                        {
                            senderCreateResp.ReferenceID = "S" + senderInfo.SelfRefID;
                            senderCreateResp.Statuscode = ErrorCodes.One;
                            senderCreateResp.IsOTPResendAvailble = true;
                        }
                    }
                }
                else if (_ValResp.APICode == APICode.CYBERPLAT)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new CyberPlateMLIPay(_dal);
                    senderCreateResp = moneyTransferAPIML.CreateSender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.RECHARGEDADDY)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new RechargeDaddyML(_accessor, _env, _dal);
                    senderCreateResp = moneyTransferAPIML.CreateSender(tAPIRequest);
                }
                else if (_ValResp.APICode.EndsWith("DADDY"))
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new RechargeDaddyML(_accessor, _env, _dal);
                    senderCreateResp = moneyTransferAPIML.CreateSender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.MAHAGRAM)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new MahagramAPIML(_accessor, _env, _dal, _ValResp.APIID);
                    senderCreateResp = moneyTransferAPIML.GenerateOTP(tAPIRequest);
                    if (senderCreateResp.Statuscode == ErrorCodes.One)
                    {
                        var senderRequest = new SenderRequest
                        {
                            Address = tAPIRequest.Address,
                            Name = tAPIRequest.FirstName + " " + tAPIRequest.LastName,
                            Area = tAPIRequest.Area,
                            MobileNo = tAPIRequest.SenderMobile,
                            Pincode = tAPIRequest.Pincode.ToString(),
                            ReffID = senderCreateResp.ReferenceID,
                            UserID = tAPIRequest.UserID,
                            Dob = tAPIRequest.DOB,
                            StateID = tAPIRequest.StateID,
                            City = tAPIRequest.City
                        };
                        IProcedure _proc = new ProcUpdateSender(_dal);
                        var senderInfo = (SenderInfo)_proc.Call(senderRequest);
                        if (senderInfo.Statuscode == ErrorCodes.One)
                        {
                            senderCreateResp.ReferenceID = "S" + senderInfo.SelfRefID;
                            senderCreateResp.Statuscode = ErrorCodes.One;
                            senderCreateResp.IsOTPResendAvailble = true;
                        }
                    }
                }
                else if (_ValResp.APICode == APICode.YESBANK)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new YesBankML(_accessor, _env, _dal);
                    senderCreateResp = moneyTransferAPIML.CreateSender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.PAYU)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new PayUML(_accessor, _env, _dal);
                    senderCreateResp = moneyTransferAPIML.CreateSender(tAPIRequest);
                    if (senderCreateResp.IsOTPGenerated)
                    {
                        OTPForDMT(senderCreateResp.OTP, tAPIRequest.SenderMobile, string.Empty, senderCreateResp.WID, request.UserID);
                    }
                }
                else if (_ValResp.APICode == APICode.PAYUDMT)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new PayUDmtML(_accessor, _env, _dal);
                    senderCreateResp = moneyTransferAPIML.CreateSender(tAPIRequest);
                    if (senderCreateResp.IsOTPGenerated)
                    {
                        OTPForDMT(senderCreateResp.OTP, tAPIRequest.SenderMobile, string.Empty, senderCreateResp.WID, request.UserID);
                    }
                }
                else if (_ValResp.APICode == APICode.INSTANTPayDirect)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new IPayPayoutDirectML(_accessor, _env, _dal);
                    senderCreateResp = moneyTransferAPIML.CreateSender(tAPIRequest);
                    if (senderCreateResp.IsOTPGenerated)
                    {
                        OTPForDMT(senderCreateResp.OTP, tAPIRequest.SenderMobile, string.Empty, senderCreateResp.WID, request.UserID);
                    }
                }
                else if (_ValResp.APICode.EndsWith(APICode.RAZORPAYOUT))
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new RZRPayoutML(_accessor, _env, _dal, _ValResp.APICode);
                    senderCreateResp = moneyTransferAPIML.CreateSender(tAPIRequest);
                    if (senderCreateResp.IsOTPGenerated)
                    {
                        OTPForDMT(senderCreateResp.OTP, tAPIRequest.SenderMobile, string.Empty, senderCreateResp.WID, request.UserID);
                    }
                }
                else if (_ValResp.APICode.Equals(APICode.RPFINTECH) || _ValResp.APICode.EndsWith("FNTH"))
                {
                    if (_ValResp.APIOpCode.Equals(APIOPCODE.RPXPRESS) || _ValResp.APIOpCode.EndsWith("XPRESS"))
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new FintechPayoutML(_accessor, _env, _dal, _ValResp.APICode);
                        senderCreateResp = moneyTransferAPIML.CreateSender(tAPIRequest);
                        if (senderCreateResp.IsOTPGenerated)
                        {
                            OTPForDMT(senderCreateResp.OTP, tAPIRequest.SenderMobile, string.Empty, senderCreateResp.WID, request.UserID);
                        }
                    }
                    else
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new FintechAPIML(_accessor, _env, _ValResp.APICode, tAPIRequest.APIID, _dal);
                        senderCreateResp = moneyTransferAPIML.CreateSender(tAPIRequest);
                    }
                }
                else if (_ValResp.APICode == APICode.RBLMT)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new RBLML(_accessor, _env, _dal);
                    senderCreateResp = moneyTransferAPIML.CreateSender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.BILLAVENUE)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new BillAvenueMT_ML(_accessor, _env, APICode.BILLAVENUE, tAPIRequest.APIID, _dal);
                    senderCreateResp = moneyTransferAPIML.CreateSender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.INSTANTPAY)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new InstantPay_ML(_accessor, _env, APICode.INSTANTPAY, tAPIRequest.APIID, _dal);
                    senderCreateResp = moneyTransferAPIML.CreateSender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.EKOPAYOUT)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new EKOPayoutML(_accessor, _env, _dal);
                    senderCreateResp = moneyTransferAPIML.CreateSender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.Manual)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new ManualDMT_ML(_accessor, _env, _dal, APICode.Manual);
                    senderCreateResp = moneyTransferAPIML.CreateSender(tAPIRequest);
                    if (senderCreateResp.IsOTPGenerated)
                    {
                        OTPForDMT(senderCreateResp.OTP, tAPIRequest.SenderMobile, string.Empty, senderCreateResp.WID, request.UserID);
                    }
                }
                else if (_ValResp.APICode == APICode.SECUREPAYMENT)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new SecurePaymentML(_accessor, _env, _dal, APICode.SECUREPAYMENT);
                    senderCreateResp = moneyTransferAPIML.CreateSender(tAPIRequest);
                    if (senderCreateResp.IsOTPGenerated)
                    {
                        OTPForDMT(senderCreateResp.OTP, tAPIRequest.SenderMobile, string.Empty, senderCreateResp.WID, request.UserID);
                    }
                }
                else if (_ValResp.APICode == APICode.MMWFintech)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new MMWFintechML(_accessor, _env, APICode.MMWFintech, _ValResp.APIID, _dal);
                    senderCreateResp = moneyTransferAPIML.CreateSender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.SPRINT)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new SprintDMTML(_accessor, _env, APICode.SPRINT, _ValResp.APIID, _dal);
                    senderCreateResp = moneyTransferAPIML.CreateSender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.GOTERPay)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new GoterPayML(_accessor, _env, APICode.GOTERPay, _ValResp.APIID, _dal);
                    senderCreateResp = moneyTransferAPIML.CreateSender(tAPIRequest);
                    if (senderCreateResp.IsOTPGenerated)
                    {
                        OTPForDMT(senderCreateResp.OTP, tAPIRequest.SenderMobile, string.Empty, senderCreateResp.WID, request.UserID);
                    }
                }
                else if (_ValResp.APICode == APICode.FINO)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new FINOML(_accessor, _env, _dal);
                    senderCreateResp = moneyTransferAPIML.CreateSender(tAPIRequest);
                    if (senderCreateResp.IsOTPGenerated)
                    {
                        OTPForDMT(senderCreateResp.OTP, tAPIRequest.SenderMobile, string.Empty, senderCreateResp.WID, request.UserID);
                    }
                }
                else if (_ValResp.APICode == APICode.CASHFREE)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new CashFreePayoutML(_accessor, _env, _dal, _ValResp.APIGroupCode);
                    senderCreateResp = moneyTransferAPIML.CreateSender(tAPIRequest);
                    if (senderCreateResp.IsOTPGenerated)
                    {
                        OTPForDMT(senderCreateResp.OTP, tAPIRequest.SenderMobile, string.Empty, senderCreateResp.WID, request.UserID);
                    }
                }
                else if (_ValResp.APICode == APICode.HYPTO)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new HyptoML(_accessor, _env, tAPIRequest.APIID, _dal);
                    senderCreateResp = moneyTransferAPIML.CreateSender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.FINODIN)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new FinodinML(_accessor, _env, _dal, APICode.FINODIN, _ValResp.APIID, _ValResp.APIGroupCode);
                    senderCreateResp = moneyTransferAPIML.CreateSender(tAPIRequest);
                }
                else
                {
                    senderCreateResp.Msg = ErrorCodes.Down;
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "CreateSender",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = request.UserID
                });
            }

            return senderCreateResp;
        }
        public MBeneficiaryResp GetBeneficiary(MTCommonRequest request)
        {
            var beneficiaryResp = new MBeneficiaryResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var _ValResp = GetOutletNew(request.UserID, request.OutletID, request.OID);
            if (_ValResp.Statuscode != ErrorCodes.One)
            {
                beneficiaryResp.Msg = _ValResp.Msg;
                beneficiaryResp.Statuscode = _ValResp.Statuscode;
                beneficiaryResp.ErrorCode = _ValResp.ErrorCode;
                return beneficiaryResp;
            }

            var tAPIRequest = new MTAPIRequest
            {
                APIGroupCode = _ValResp.APIGroupCode,
                APIOpCode = _ValResp.APIOpCode,
                APIID = _ValResp.APIID,
                TransactionID = _ValResp.TransactionID,
                APIOutletID = _ValResp.DMTID,
                SenderMobile = request.SenderMobile,
                RequestMode = request.RequestMode,
                UserID = request.UserID,
                ReferenceID = request.ReferenceID
            };
            try
            {

                if (_ValResp.APICode == APICode.AMAH)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new MobileWareML(_accessor, _env);
                    beneficiaryResp = moneyTransferAPIML.GetBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.MRUY)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new MrupayML(_accessor, _env);
                    beneficiaryResp = moneyTransferAPIML.GetBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.AIRTELBANK)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new AirtelBankML(_accessor, _env);
                    beneficiaryResp = moneyTransferAPIML.GetBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.OPENBANK)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new OpenBankML(_accessor, _env);
                    beneficiaryResp = moneyTransferAPIML.GetBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.ICICIBANK)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new ICICIML(_accessor, _env);
                    beneficiaryResp = moneyTransferAPIML.GetBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.PAYTM)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new PaytmML(_accessor, _env, _dal);
                    beneficiaryResp = moneyTransferAPIML.GetBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.ICICIBANKPAYOUT)/*icici bank*/
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new ICICIPayoutML(_accessor, _env, tAPIRequest.APIID);
                    beneficiaryResp = moneyTransferAPIML.GetBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.EKO)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new EKOML(_accessor, _env);
                    beneficiaryResp = moneyTransferAPIML.GetBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.EKO2)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new EKO2ML(_accessor, _env, _dal, tAPIRequest.APIID);
                    beneficiaryResp = moneyTransferAPIML.GetBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.CYBERPLATPayTM)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new CyberPlateML(_dal);
                    beneficiaryResp = moneyTransferAPIML.GetBeneficiary(tAPIRequest);
                    if (beneficiaryResp.Statuscode == ErrorCodes.One)
                    {
                        if (beneficiaryResp.Beneficiaries != null)
                        {
                            if (beneficiaryResp.Beneficiaries.Any())
                            {
                                var BeneIDs = string.Join(",", beneficiaryResp.Beneficiaries.Select(x => x.BeneID));
                                var commonReq = new CommonReq
                                {
                                    CommonStr = BeneIDs,
                                    CommonStr2 = APICode.CYBERPLATPayTM,
                                    CommonStr3 = request.SenderMobile,
                                    CommonBool = true
                                };
                                IProcedureAsync procedureAsync = new ProcGetBeneficiaryByBeneIDs(_dal);
                                beneficiaryResp.Beneficiaries = (List<MBeneDetail>)procedureAsync.Call(commonReq).Result;
                            }
                        }
                    }
                }
                else if (_ValResp.APICode == APICode.CYBERPLAT)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new CyberPlateMLIPay(_dal);
                    beneficiaryResp = moneyTransferAPIML.GetBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.RECHARGEDADDY)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new RechargeDaddyML(_accessor, _env, _dal);
                    beneficiaryResp = moneyTransferAPIML.GetBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode.Equals(APICode.PayOneMoney))
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new PayOneMoneyML(_accessor, _env, _dal);
                    beneficiaryResp = moneyTransferAPIML.GetBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.MAHAGRAM)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new MahagramAPIML(_accessor, _env, _dal);
                    beneficiaryResp = moneyTransferAPIML.GetBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.YESBANK)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new YesBankML(_accessor, _env, _dal);
                    beneficiaryResp = moneyTransferAPIML.GetBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.PAYU)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new PayUML(_accessor, _env, _dal);
                    beneficiaryResp = moneyTransferAPIML.GetBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.PAYUDMT)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new PayUDmtML(_accessor, _env, _dal);
                    beneficiaryResp = moneyTransferAPIML.GetBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.INSTANTPayDirect)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new IPayPayoutDirectML(_accessor, _env, _dal);
                    beneficiaryResp = moneyTransferAPIML.GetBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode.EndsWith(APICode.RAZORPAYOUT))
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new RZRPayoutML(_accessor, _env, _dal, _ValResp.APICode);
                    beneficiaryResp = moneyTransferAPIML.GetBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode.Equals(APICode.RPFINTECH) || _ValResp.APICode.EndsWith("FNTH"))
                {
                    if (_ValResp.APIOpCode.Equals(APIOPCODE.RPXPRESS) || _ValResp.APIOpCode.EndsWith("XPRESS"))
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new FintechPayoutML(_accessor, _env, _dal, _ValResp.APICode);
                        beneficiaryResp = moneyTransferAPIML.GetBeneficiary(tAPIRequest);
                    }
                    else
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new FintechAPIML(_accessor, _env, _ValResp.APICode, tAPIRequest.APIID, _dal);
                        beneficiaryResp = moneyTransferAPIML.GetBeneficiary(tAPIRequest);
                    }
                }
                else if (_ValResp.APICode == APICode.RBLMT)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new RBLML(_accessor, _env, _dal);
                    beneficiaryResp = moneyTransferAPIML.GetBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.BILLAVENUE)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new BillAvenueMT_ML(_accessor, _env, APICode.BILLAVENUE, tAPIRequest.APIID, _dal);
                    beneficiaryResp = moneyTransferAPIML.GetBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.INSTANTPAY)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new InstantPay_ML(_accessor, _env, APICode.INSTANTPAY, tAPIRequest.APIID, _dal);
                    beneficiaryResp = moneyTransferAPIML.GetBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.EKOPAYOUT)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new EKOPayoutML(_accessor, _env, _dal);
                    beneficiaryResp = moneyTransferAPIML.GetBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.Manual)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new ManualDMT_ML(_accessor, _env, _dal, APICode.Manual);
                    beneficiaryResp = moneyTransferAPIML.GetBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.SECUREPAYMENT)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new SecurePaymentML(_accessor, _env, _dal, APICode.SECUREPAYMENT);
                    beneficiaryResp = moneyTransferAPIML.GetBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.MMWFintech)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new MMWFintechML(_accessor, _env, APICode.MMWFintech, _ValResp.APIID, _dal);
                    beneficiaryResp = moneyTransferAPIML.GetBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.SPRINT)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new SprintDMTML(_accessor, _env, APICode.SPRINT, _ValResp.APIID, _dal);
                    beneficiaryResp = moneyTransferAPIML.GetBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.GOTERPay)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new GoterPayML(_accessor, _env, APICode.GOTERPay, _ValResp.APIID, _dal);
                    beneficiaryResp = moneyTransferAPIML.GetBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.FINO)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new FINOML(_accessor, _env, _dal);
                    beneficiaryResp = moneyTransferAPIML.GetBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.CASHFREE)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new CashFreePayoutML(_accessor, _env, _dal, _ValResp.APIGroupCode);
                    beneficiaryResp = moneyTransferAPIML.GetBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.HYPTO)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new HyptoML(_accessor, _env, tAPIRequest.APIID, _dal);
                    beneficiaryResp = moneyTransferAPIML.GetBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.FINODIN)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new FinodinML(_accessor, _env, _dal, APICode.FINODIN, _ValResp.APIID, _ValResp.APIGroupCode);
                    beneficiaryResp = moneyTransferAPIML.GetBeneficiary(tAPIRequest);
                }
                else
                {
                    beneficiaryResp.Msg = ErrorCodes.Down;
                    return beneficiaryResp;
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GetBeneficiary",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            return beneficiaryResp;
        }
        public MSenderLoginResponse VerifySender(MTOTPRequest request)
        {
            var senderLoginResponse = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Unknown_Error
            };
            if ((request.OTP ?? string.Empty).Length < 3)
            {
                senderLoginResponse.Msg = nameof(ErrorCodes.Invalid_OTP).Replace("_", " ") + " or Length";
                senderLoginResponse.ErrorCode = ErrorCodes.Invalid_OTP;
                return senderLoginResponse;
            }
            if (!Validate.O.IsMobile(request.SenderMobile ?? string.Empty) || request.SenderMobile.Length < 10)
            {
                senderLoginResponse.Msg = ErrorCodes.InvalidParam + " Sender Mobile Number";
                return senderLoginResponse;
            }
            var _ValResp = GetOutletNew(request.UserID, request.OutletID, request.OID);
            if (_ValResp.Statuscode != ErrorCodes.One)
            {
                senderLoginResponse.Msg = _ValResp.Msg;
                senderLoginResponse.Statuscode = _ValResp.Statuscode;
                senderLoginResponse.ErrorCode = _ValResp.ErrorCode;
                return senderLoginResponse;
            }
            var tAPIRequest = new MTAPIRequest
            {
                APIGroupCode = _ValResp.APIGroupCode,
                APIOpCode = _ValResp.APIOpCode,
                APIID = _ValResp.APIID,
                TransactionID = _ValResp.TransactionID,
                APIOutletID = _ValResp.DMTID,
                SenderMobile = request.SenderMobile,
                RequestMode = request.RequestMode,
                UserID = request.UserID,
                OTP = request.OTP,
                ReferenceID = request.ReferenceID,
                StateID = _ValResp.StateID,
                Pincode = Convert.ToInt32(_ValResp.Pincode),
                Address = _ValResp.Address,
                DOB = _ValResp.DOB
            };

            try
            {
                if (_ValResp.APICode == APICode.AMAH)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new MobileWareML(_accessor, _env);
                    senderLoginResponse = moneyTransferAPIML.VerifySender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.MRUY)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new MrupayML(_accessor, _env);
                    senderLoginResponse = moneyTransferAPIML.VerifySender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.AIRTELBANK)
                {
                    var req = new UserRequset
                    {
                        UserId = tAPIRequest.UserID
                    };
                    var uDetails = new UserDetail();
                    uDetails = userML.GetAppUserDetailByID(req);
                    tAPIRequest.UserMobile = uDetails.MobileNo;
                    IMoneyTransferAPIML moneyTransferAPIML = new AirtelBankML(_accessor, _env);
                    senderLoginResponse = moneyTransferAPIML.VerifySender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.OPENBANK)
                {
                    var req = new UserRequset
                    {
                        UserId = tAPIRequest.UserID
                    };
                    var uDetails = userML.GetAppUserDetailByID(req);
                    tAPIRequest.UserMobile = uDetails.MobileNo;
                    IMoneyTransferAPIML moneyTransferAPIML = new OpenBankML(_accessor, _env);
                    senderLoginResponse = moneyTransferAPIML.VerifySender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.ICICIBANK)
                {
                    var req = new UserRequset
                    {
                        UserId = tAPIRequest.UserID
                    };
                    var uDetails = userML.GetAppUserDetailByID(req);
                    tAPIRequest.UserMobile = uDetails.MobileNo;
                    IMoneyTransferAPIML moneyTransferAPIML = new ICICIML(_accessor, _env);
                    senderLoginResponse = moneyTransferAPIML.VerifySender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.ICICIBANKPAYOUT)
                {
                    var req = new UserRequset
                    {
                        UserId = tAPIRequest.UserID
                    };
                    var uDetails = userML.GetAppUserDetailByID(req);
                    tAPIRequest.UserMobile = uDetails.MobileNo;
                    IMoneyTransferAPIML moneyTransferAPIML = new ICICIPayoutML(_accessor, _env, tAPIRequest.APIID);
                    senderLoginResponse = moneyTransferAPIML.VerifySender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.PAYTM)
                {
                    var req = new UserRequset
                    {
                        UserId = tAPIRequest.UserID
                    };
                    var uDetails = userML.GetAppUserDetailByID(req);
                    tAPIRequest.UserMobile = uDetails.MobileNo;
                    IMoneyTransferAPIML moneyTransferAPIML = new PaytmML(_accessor, _env, _dal);
                    senderLoginResponse = moneyTransferAPIML.VerifySender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.EKO)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new EKOML(_accessor, _env);
                    senderLoginResponse = moneyTransferAPIML.VerifySender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.EKO2)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new EKO2ML(_accessor, _env, _dal, tAPIRequest.APIID);
                    senderLoginResponse = moneyTransferAPIML.VerifySender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.CYBERPLATPayTM)
                {
                    senderLoginResponse.Msg = nameof(ErrorCodes.Invalid_OTP).Replace("_", " ");
                    senderLoginResponse.ErrorCode = ErrorCodes.Invalid_OTP;
                    if (!string.IsNullOrEmpty(request.ReferenceID))
                    {
                        if (request.ReferenceID.ToUpper()[0] == 'S')
                        {
                            var SelfRefID = request.ReferenceID.Replace(request.ReferenceID[0].ToString(), string.Empty);
                            if (Validate.O.IsNumeric(SelfRefID))
                            {
                                var senderRequest = new SenderRequest
                                {

                                    MobileNo = request.SenderMobile,
                                    SelfRefID = Convert.ToInt32(SelfRefID),
                                    UserID = request.UserID
                                };
                                IProcedure _proc = new ProcUpdateSender(_dal);
                                var senderInfo = (SenderInfo)_proc.Call(senderRequest);
                                if (senderInfo.Statuscode == ErrorCodes.One)
                                {
                                    tAPIRequest.FirstName = (senderInfo.Name ?? string.Empty).Contains(" ") ? senderInfo.Name.Split(' ')[0] : senderInfo.Name;
                                    tAPIRequest.LastName = (senderInfo.Name ?? string.Empty).Contains(" ") ? senderInfo.Name.Split(' ')[1] : senderInfo.Name;
                                    tAPIRequest.Area = senderInfo.Area;
                                    tAPIRequest.Address = senderInfo.Address;
                                    tAPIRequest.Districtname = senderInfo.Districtname;
                                    tAPIRequest.StateName = senderInfo.Statename;
                                    tAPIRequest.Pincode = Convert.ToInt32(senderInfo.Pincode ?? "0");
                                    tAPIRequest.ReferenceID = senderInfo.ReffID;
                                    IMoneyTransferAPIML moneyTransferAPIML = new CyberPlateML(_dal);
                                    senderLoginResponse = moneyTransferAPIML.VerifySender(tAPIRequest);
                                }
                            }
                        }
                    }
                }
                else if (_ValResp.APICode == APICode.CYBERPLAT)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new CyberPlateMLIPay(_dal);
                    senderLoginResponse = moneyTransferAPIML.VerifySender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.RECHARGEDADDY)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new RechargeDaddyML(_accessor, _env, _dal);
                    senderLoginResponse = moneyTransferAPIML.VerifySender(tAPIRequest);
                }
                else if (_ValResp.APICode.Equals(APICode.PayOneMoney))
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new PayOneMoneyML(_accessor, _env, _dal);
                    senderLoginResponse = moneyTransferAPIML.VerifySender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.MAHAGRAM)
                {
                    senderLoginResponse.Msg = nameof(ErrorCodes.Invalid_OTP).Replace("_", " ");
                    senderLoginResponse.ErrorCode = ErrorCodes.Invalid_OTP;
                    if (!string.IsNullOrEmpty(request.ReferenceID))
                    {
                        if (request.ReferenceID.ToUpper()[0] == 'S')
                        {
                            var SelfRefID = request.ReferenceID.Replace(request.ReferenceID[0].ToString(), string.Empty);
                            if (Validate.O.IsNumeric(SelfRefID))
                            {
                                var senderRequest = new SenderRequest
                                {

                                    MobileNo = request.SenderMobile,
                                    SelfRefID = Convert.ToInt32(SelfRefID),
                                    UserID = request.UserID
                                };
                                IProcedure _proc = new ProcUpdateSender(_dal);
                                var senderInfo = (SenderInfo)_proc.Call(senderRequest);
                                if (senderInfo.Statuscode == ErrorCodes.One)
                                {
                                    tAPIRequest.FirstName = (senderInfo.Name ?? string.Empty).Contains(" ") ? senderInfo.Name.Split(' ')[0] : senderInfo.Name;
                                    tAPIRequest.LastName = (senderInfo.Name ?? string.Empty).Contains(" ") ? senderInfo.Name.Split(' ')[1] : senderInfo.Name;
                                    tAPIRequest.Area = senderInfo.Area;
                                    tAPIRequest.Address = senderInfo.Address;
                                    tAPIRequest.Districtname = senderInfo.Districtname;
                                    tAPIRequest.StateName = senderInfo.MahagramStateCode;
                                    tAPIRequest.Pincode = Convert.ToInt32(senderInfo.Pincode ?? "0");
                                    tAPIRequest.DOB = senderInfo.Dob;

                                    IMoneyTransferAPIML moneyTransferAPIML = new MahagramAPIML(_accessor, _env, _dal);
                                    senderLoginResponse = moneyTransferAPIML.VerifySender(tAPIRequest);
                                }
                            }
                        }
                    }
                }
                else if (_ValResp.APICode == APICode.YESBANK)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new YesBankML(_accessor, _env, _dal);
                    senderLoginResponse = moneyTransferAPIML.VerifySender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.PAYU)
                {
                    var req = new UserRequset
                    {
                        UserId = tAPIRequest.UserID
                    };
                    var uDetails = userML.GetAppUserDetailByID(req);
                    tAPIRequest.UserMobile = uDetails.MobileNo;
                    IMoneyTransferAPIML moneyTransferAPIML = new PayUML(_accessor, _env, _dal);
                    senderLoginResponse = moneyTransferAPIML.VerifySender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.PAYUDMT)
                {
                    var req = new UserRequset
                    {
                        UserId = tAPIRequest.UserID
                    };
                    var uDetails = userML.GetAppUserDetailByID(req);
                    tAPIRequest.UserMobile = uDetails.MobileNo;
                    IMoneyTransferAPIML moneyTransferAPIML = new PayUDmtML(_accessor, _env, _dal);
                    senderLoginResponse = moneyTransferAPIML.VerifySender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.INSTANTPayDirect)
                {
                    var req = new UserRequset
                    {
                        UserId = tAPIRequest.UserID
                    };
                    var uDetails = userML.GetAppUserDetailByID(req);
                    tAPIRequest.UserMobile = uDetails.MobileNo;
                    IMoneyTransferAPIML moneyTransferAPIML = new IPayPayoutDirectML(_accessor, _env, _dal);
                    senderLoginResponse = moneyTransferAPIML.VerifySender(tAPIRequest);
                }
                else if (_ValResp.APICode.EndsWith(APICode.RAZORPAYOUT))
                {
                    var uDetails = userML.GetAppUserDetailByID(new UserRequset
                    {
                        UserId = tAPIRequest.UserID
                    });
                    tAPIRequest.UserMobile = uDetails.MobileNo;
                    IMoneyTransferAPIML moneyTransferAPIML = new RZRPayoutML(_accessor, _env, _dal, _ValResp.APICode);
                    senderLoginResponse = moneyTransferAPIML.VerifySender(tAPIRequest);
                }
                else if (_ValResp.APICode.Equals(APICode.RPFINTECH) || _ValResp.APICode.EndsWith("FNTH"))
                {
                    if (_ValResp.APIOpCode.Equals(APIOPCODE.RPXPRESS) || _ValResp.APIOpCode.EndsWith("XPRESS"))
                    {
                        var uDetails = userML.GetAppUserDetailByID(new UserRequset
                        {
                            UserId = tAPIRequest.UserID
                        });
                        tAPIRequest.UserMobile = uDetails.MobileNo;
                        IMoneyTransferAPIML moneyTransferAPIML = new FintechPayoutML(_accessor, _env, _dal, _ValResp.APICode);
                        senderLoginResponse = moneyTransferAPIML.VerifySender(tAPIRequest);
                    }
                    else
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new FintechAPIML(_accessor, _env, _ValResp.APICode, tAPIRequest.APIID, _dal);
                        senderLoginResponse = moneyTransferAPIML.VerifySender(tAPIRequest);
                    }
                }
                else if (_ValResp.APICode == APICode.RBLMT)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new RBLML(_accessor, _env, _dal);
                    senderLoginResponse = moneyTransferAPIML.VerifySender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.BILLAVENUE)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new BillAvenueMT_ML(_accessor, _env, APICode.BILLAVENUE, tAPIRequest.APIID, _dal);
                    senderLoginResponse = moneyTransferAPIML.VerifySender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.INSTANTPAY)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new InstantPay_ML(_accessor, _env, APICode.INSTANTPAY, tAPIRequest.APIID, _dal);
                    senderLoginResponse = moneyTransferAPIML.VerifySender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.EKOPAYOUT)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new EKOPayoutML(_accessor, _env, _dal);
                    senderLoginResponse = moneyTransferAPIML.VerifySender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.Manual)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new ManualDMT_ML(_accessor, _env, _dal, APICode.Manual);
                    senderLoginResponse = moneyTransferAPIML.VerifySender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.SECUREPAYMENT)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new SecurePaymentML(_accessor, _env, _dal, APICode.SECUREPAYMENT);
                    senderLoginResponse = moneyTransferAPIML.VerifySender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.MMWFintech)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new MMWFintechML(_accessor, _env, APICode.MMWFintech, _ValResp.APIID, _dal);
                    senderLoginResponse = moneyTransferAPIML.VerifySender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.SPRINT)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new SprintDMTML(_accessor, _env, APICode.SPRINT, _ValResp.APIID, _dal);
                    senderLoginResponse = moneyTransferAPIML.VerifySender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.GOTERPay)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new GoterPayML(_accessor, _env, APICode.GOTERPay, _ValResp.APIID, _dal);
                    senderLoginResponse = moneyTransferAPIML.VerifySender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.FINO)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new FINOML(_accessor, _env, _dal);
                    senderLoginResponse = moneyTransferAPIML.VerifySender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.CASHFREE)
                {
                    var req = new UserRequset
                    {
                        UserId = tAPIRequest.UserID
                    };
                    var uDetails = userML.GetAppUserDetailByID(req);
                    tAPIRequest.UserMobile = uDetails.MobileNo;
                    IMoneyTransferAPIML moneyTransferAPIML = new CashFreePayoutML(_accessor, _env, _dal, _ValResp.APIGroupCode);
                    senderLoginResponse = moneyTransferAPIML.VerifySender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.HYPTO)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new HyptoML(_accessor, _env, tAPIRequest.APIID, _dal);
                    senderLoginResponse = moneyTransferAPIML.VerifySender(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.FINODIN)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new FinodinML(_accessor, _env, _dal, APICode.FINODIN, _ValResp.APIID, _ValResp.APIGroupCode);
                    senderLoginResponse = moneyTransferAPIML.VerifySender(tAPIRequest);
                }
                else
                {
                    senderLoginResponse.Msg = ErrorCodes.Down;
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "VerifySender",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            if (senderLoginResponse.ErrorCode == ErrorCodes.Unknown_Error)
            {
                senderLoginResponse.ErrorCode = 202;
            }
            return senderLoginResponse;
        }
        public MSenderLoginResponse CreateBeneficiary(MTBeneficiaryAddRequest request)
        {
            var senderLoginResponse = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Unknown_Error
            };
            if ((request.BeneDetail.BeneName ?? string.Empty).Contains("."))
            {
                request.BeneDetail.BeneName = request.BeneDetail.BeneName.Replace(".", "");
            }
            if (Validate.O.IsNumeric(request.BeneDetail.BeneName ?? "") || (request.BeneDetail.BeneName ?? "").Length > 100 || (request.BeneDetail.BeneName ?? "").Length < 4 || Validate.O.HasSpecialChar(request.BeneDetail.BeneName ?? string.Empty))
            {
                senderLoginResponse.Msg = ErrorCodes.InvalidParam + " BeneName";
                return senderLoginResponse;
            }
            if (!Validate.O.IsMobile(request.BeneDetail.MobileNo ?? "") || request.BeneDetail.MobileNo.Length < 10)
            {
                senderLoginResponse.Msg = ErrorCodes.InvalidParam + "Beni Mobile Number";
                return senderLoginResponse;
            }
            if (!Validate.O.IsMobile(request.SenderMobile ?? "") || request.SenderMobile.Length < 10)
            {
                senderLoginResponse.Msg = ErrorCodes.InvalidParam + "Sender Mobile Number";
                return senderLoginResponse;
            }
            if (string.IsNullOrWhiteSpace(request.BeneDetail.IFSC) || request.BeneDetail.IFSC.Length < 11)
            {
                senderLoginResponse.Msg = ErrorCodes.InvalidParam + "IFSC";
                return senderLoginResponse;
            }
            if (!Validate.O.IsValidBankAccountNo(request.BeneDetail.AccountNo ?? "") || (Validate.O.ReplaceAllSpecials(request.BeneDetail.AccountNo ?? "").Replace(" ", "") != (request.BeneDetail.AccountNo ?? "")))
            {
                senderLoginResponse.Msg = ErrorCodes.InvalidParam + "AccountNo";
                return senderLoginResponse;
            }
            request.BeneDetail.AccountNo = request.BeneDetail.AccountNo.Replace(" ", string.Empty);
            if (Validate.O.IsNumeric(request.BeneDetail.BankName ?? "") || (request.BeneDetail.BankName ?? "").Trim().Length > 100 || (request.BeneDetail.BankName ?? "").Trim().Length == 0)
            {
                senderLoginResponse.Msg = ErrorCodes.InvalidParam + " BankName";
                return senderLoginResponse;
            }
            var userML = new UserML(_accessor, _env);
            var _ValResp = GetOutletNew(request.UserID, request.OutletID, request.OID);
            if (_ValResp.Statuscode != ErrorCodes.One)
            {
                senderLoginResponse.Msg = _ValResp.Msg;
                senderLoginResponse.Statuscode = _ValResp.Statuscode;
                senderLoginResponse.ErrorCode = _ValResp.ErrorCode;
                return senderLoginResponse;
            }

            var tAPIRequest = new MTAPIRequest
            {
                APIGroupCode = _ValResp.APIGroupCode,
                APIOpCode = _ValResp.APIOpCode,
                APIID = _ValResp.APIID,
                TransactionID = _ValResp.TransactionID,
                APIOutletID = _ValResp.DMTID,
                SenderMobile = request.SenderMobile,
                RequestMode = request.RequestMode,
                UserID = request.UserID,
                ReferenceID = request.ReferenceID,
                mBeneDetail = request.BeneDetail,
                BankID = request.BeneDetail.BankID,
                APICode = _ValResp.APICode,
                StateID = _ValResp.StateID,
                Pincode = Convert.ToInt32(_ValResp.Pincode),
                DOB = _ValResp.DOB,
                Address = _ValResp.Address,
                City = _ValResp.City,
                StateName = _ValResp.StateName
            };
            try
            {
                if (_ValResp.APICode == APICode.AMAH)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new MobileWareML(_accessor, _env);
                    senderLoginResponse = moneyTransferAPIML.CreateBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.MRUY)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new MrupayML(_accessor, _env);
                    senderLoginResponse = moneyTransferAPIML.CreateBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.AIRTELBANK)
                {
                    var req = new UserRequset
                    {
                        UserId = request.UserID
                    };
                    var uDetails = userML.GetAppUserDetailByID(req);
                    tAPIRequest.UserMobile = uDetails.MobileNo;
                    tAPIRequest.StateID = uDetails.StateID;
                    IMoneyTransferAPIML moneyTransferAPIML = new AirtelBankML(_accessor, _env);
                    senderLoginResponse = moneyTransferAPIML.CreateBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.OPENBANK)
                {
                    var req = new UserRequset
                    {
                        UserId = request.UserID
                    };
                    var uDetails = userML.GetAppUserDetailByID(req);
                    tAPIRequest.UserMobile = uDetails.MobileNo;
                    tAPIRequest.StateID = uDetails.StateID;
                    IMoneyTransferAPIML moneyTransferAPIML = new OpenBankML(_accessor, _env);
                    senderLoginResponse = moneyTransferAPIML.CreateBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.ICICIBANK)
                {
                    var req = new UserRequset
                    {
                        UserId = request.UserID
                    };
                    var uDetails = userML.GetAppUserDetailByID(req);
                    tAPIRequest.UserMobile = uDetails.MobileNo;
                    tAPIRequest.StateID = uDetails.StateID;
                    IMoneyTransferAPIML moneyTransferAPIML = new ICICIML(_accessor, _env);
                    senderLoginResponse = moneyTransferAPIML.CreateBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.ICICIBANKPAYOUT)/*icici bank*/
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new ICICIPayoutML(_accessor, _env, tAPIRequest.APIID);
                    var uDetails = userML.GetAppUserDetailByID(new UserRequset
                    {
                        UserId = request.UserID
                    });
                    tAPIRequest.UserMobile = uDetails.MobileNo;
                    tAPIRequest.StateID = uDetails.StateID;
                    senderLoginResponse = moneyTransferAPIML.CreateBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.PAYTM)
                {
                    var req = new UserRequset
                    {
                        UserId = request.UserID
                    };
                    var uDetails = userML.GetAppUserDetailByID(req);
                    tAPIRequest.UserMobile = uDetails.MobileNo;
                    tAPIRequest.StateID = uDetails.StateID;
                    IMoneyTransferAPIML moneyTransferAPIML = new PaytmML(_accessor, _env, _dal);
                    senderLoginResponse = moneyTransferAPIML.CreateBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.EKO)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new EKOML(_accessor, _env);
                    senderLoginResponse = moneyTransferAPIML.CreateBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.EKO2)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new EKO2ML(_accessor, _env, _dal, tAPIRequest.APIID);
                    senderLoginResponse = moneyTransferAPIML.CreateBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.CYBERPLATPayTM)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new CyberPlateML(_dal);
                    senderLoginResponse = moneyTransferAPIML.CreateBeneficiary(tAPIRequest);
                    if (senderLoginResponse.Statuscode == ErrorCodes.One)
                    {
                        var beneficiaryModel = new BeneficiaryModel
                        {
                            IFSC = tAPIRequest.mBeneDetail.IFSC,
                            Account = tAPIRequest.mBeneDetail.AccountNo,
                            BankName = tAPIRequest.mBeneDetail.BankName,
                            Name = tAPIRequest.mBeneDetail.BeneName,
                            BeneID = senderLoginResponse.BeneID,
                            SenderNo = request.SenderMobile,
                            APICode = _ValResp.APICode,
                            BankID = tAPIRequest.mBeneDetail.BankID
                        };
                        IProcedure _proc = new ProcUpdateBeneficiaryNew(_dal);
                        beneficiaryModel = (BeneficiaryModel)_proc.Call(beneficiaryModel);
                    }
                }
                else if (_ValResp.APICode == APICode.CYBERPLAT)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new CyberPlateMLIPay(_dal);
                    senderLoginResponse = moneyTransferAPIML.CreateBeneficiary(tAPIRequest);
                    if (senderLoginResponse.Statuscode == ErrorCodes.One)
                    {
                        var beneficiaryModel = new BeneficiaryModel
                        {
                            IFSC = tAPIRequest.mBeneDetail.IFSC,
                            Account = tAPIRequest.mBeneDetail.AccountNo,
                            BankName = tAPIRequest.mBeneDetail.BankName,
                            Name = tAPIRequest.mBeneDetail.BeneName,
                            BeneID = senderLoginResponse.BeneID,
                            SenderNo = request.SenderMobile,
                            APICode = _ValResp.APICode,
                            BankID = tAPIRequest.mBeneDetail.BankID
                        };
                        IProcedure _proc = new ProcUpdateBeneficiaryNew(_dal);
                        beneficiaryModel = (BeneficiaryModel)_proc.Call(beneficiaryModel);
                    }
                }
                else if (_ValResp.APICode == APICode.RECHARGEDADDY)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new RechargeDaddyML(_accessor, _env, _dal);
                    senderLoginResponse = moneyTransferAPIML.CreateBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode.Equals(APICode.PayOneMoney))
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new PayOneMoneyML(_accessor, _env, _dal);
                    senderLoginResponse = moneyTransferAPIML.CreateBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.MAHAGRAM)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new MahagramAPIML(_accessor, _env, _dal);
                    senderLoginResponse = moneyTransferAPIML.CreateBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.YESBANK)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new YesBankML(_accessor, _env, _dal);
                    senderLoginResponse = moneyTransferAPIML.CreateBeneficiary(tAPIRequest);
                    if (senderLoginResponse.Statuscode == ErrorCodes.One)
                    {
                        var beneficiaryModel = new BeneficiaryModel
                        {
                            IFSC = tAPIRequest.mBeneDetail.IFSC,
                            Account = tAPIRequest.mBeneDetail.AccountNo,
                            BankName = tAPIRequest.mBeneDetail.BankName,
                            Name = tAPIRequest.mBeneDetail.BeneName,
                            BeneID = senderLoginResponse.BeneID,
                            SenderNo = request.SenderMobile,
                            APICode = _ValResp.APICode,
                            BankID = tAPIRequest.mBeneDetail.BankID,
                            TransMode = tAPIRequest.mBeneDetail.TransMode
                        };
                        IProcedure _proc = new ProcUpdateBeneficiaryNew(_dal);
                        beneficiaryModel = (BeneficiaryModel)_proc.Call(beneficiaryModel);
                    }
                }
                else if (_ValResp.APICode == APICode.PAYU)
                {
                    var req = new UserRequset
                    {
                        UserId = request.UserID
                    };
                    var uDetails = userML.GetAppUserDetailByID(req);
                    tAPIRequest.UserMobile = uDetails.MobileNo;
                    tAPIRequest.StateID = uDetails.StateID;

                    IMoneyTransferAPIML moneyTransferAPIML = new PayUML(_accessor, _env, _dal);
                    senderLoginResponse = moneyTransferAPIML.CreateBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.PAYUDMT)
                {
                    var req = new UserRequset
                    {
                        UserId = request.UserID
                    };
                    var uDetails = userML.GetAppUserDetailByID(req);
                    tAPIRequest.UserMobile = uDetails.MobileNo;
                    tAPIRequest.StateID = uDetails.StateID;

                    IMoneyTransferAPIML moneyTransferAPIML = new PayUDmtML(_accessor, _env, _dal);
                    senderLoginResponse = moneyTransferAPIML.CreateBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.INSTANTPayDirect)
                {
                    var req = new UserRequset
                    {
                        UserId = request.UserID
                    };
                    var uDetails = userML.GetAppUserDetailByID(req);
                    tAPIRequest.UserMobile = uDetails.MobileNo;
                    tAPIRequest.StateID = uDetails.StateID;

                    IMoneyTransferAPIML moneyTransferAPIML = new IPayPayoutDirectML(_accessor, _env, _dal);
                    senderLoginResponse = moneyTransferAPIML.CreateBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode.EndsWith(APICode.RAZORPAYOUT))
                {
                    var uDetails = userML.GetAppUserDetailByID(new UserRequset
                    {
                        UserId = request.UserID
                    });
                    tAPIRequest.UserMobile = uDetails.MobileNo;
                    tAPIRequest.StateID = uDetails.StateID;

                    IMoneyTransferAPIML moneyTransferAPIML = new IPayPayoutDirectML(_accessor, _env, _dal, _ValResp.APICode);
                    senderLoginResponse = moneyTransferAPIML.CreateBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode.Equals(APICode.RPFINTECH) || _ValResp.APICode.EndsWith("FNTH"))
                {
                    if (_ValResp.APIOpCode.Equals(APIOPCODE.RPXPRESS) || _ValResp.APIOpCode.EndsWith("XPRESS"))
                    {
                        var uDetails = userML.GetAppUserDetailByID(new UserRequset
                        {
                            UserId = request.UserID
                        });
                        tAPIRequest.UserMobile = uDetails.MobileNo;
                        tAPIRequest.StateID = uDetails.StateID;

                        IMoneyTransferAPIML moneyTransferAPIML = new FintechPayoutML(_accessor, _env, _dal, _ValResp.APICode);
                        senderLoginResponse = moneyTransferAPIML.CreateBeneficiary(tAPIRequest);
                    }
                    else
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new FintechAPIML(_accessor, _env, _ValResp.APICode, tAPIRequest.APIID, _dal);
                        senderLoginResponse = moneyTransferAPIML.CreateBeneficiary(tAPIRequest);
                    }
                }
                else if (_ValResp.APICode == APICode.RBLMT)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new RBLML(_accessor, _env, _dal);
                    senderLoginResponse = moneyTransferAPIML.CreateBeneficiary(tAPIRequest);
                    if (senderLoginResponse.Statuscode == ErrorCodes.One)
                    {
                        var beneficiaryModel = new BeneficiaryModel
                        {
                            IFSC = tAPIRequest.mBeneDetail.IFSC,
                            Account = tAPIRequest.mBeneDetail.AccountNo,
                            BankName = tAPIRequest.mBeneDetail.BankName,
                            Name = tAPIRequest.mBeneDetail.BeneName,
                            BeneID = senderLoginResponse.BeneID,
                            SenderNo = request.SenderMobile,
                            APICode = _ValResp.APICode,
                            BankID = tAPIRequest.mBeneDetail.BankID,
                            TransMode = tAPIRequest.mBeneDetail.TransMode
                        };
                        IProcedure _proc = new ProcUpdateBeneficiaryNew(_dal);
                        beneficiaryModel = (BeneficiaryModel)_proc.Call(beneficiaryModel);
                    }
                }
                else if (_ValResp.APICode == APICode.BILLAVENUE)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new BillAvenueMT_ML(_accessor, _env, APICode.BILLAVENUE, tAPIRequest.APIID, _dal);
                    senderLoginResponse = moneyTransferAPIML.CreateBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.INSTANTPAY)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new InstantPay_ML(_accessor, _env, APICode.INSTANTPAY, tAPIRequest.APIID, _dal);
                    senderLoginResponse = moneyTransferAPIML.CreateBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.EKOPAYOUT)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new EKOPayoutML(_accessor, _env, _dal);
                    senderLoginResponse = moneyTransferAPIML.CreateBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.Manual)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new ManualDMT_ML(_accessor, _env, _dal, APICode.Manual);
                    senderLoginResponse = moneyTransferAPIML.CreateBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.SECUREPAYMENT)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new SecurePaymentML(_accessor, _env, _dal, APICode.SECUREPAYMENT);
                    senderLoginResponse = moneyTransferAPIML.CreateBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.MMWFintech)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new MMWFintechML(_accessor, _env, APICode.MMWFintech, _ValResp.APIID, _dal);
                    senderLoginResponse = moneyTransferAPIML.CreateBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.SPRINT)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new SprintDMTML(_accessor, _env, APICode.SPRINT, _ValResp.APIID, _dal);
                    senderLoginResponse = moneyTransferAPIML.CreateBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.GOTERPay)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new GoterPayML(_accessor, _env, APICode.GOTERPay, _ValResp.APIID, _dal);
                    senderLoginResponse = moneyTransferAPIML.CreateBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.FINO)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new FINOML(_accessor, _env, _dal);
                    senderLoginResponse = moneyTransferAPIML.CreateBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.CASHFREE)
                {
                    var req = new UserRequset
                    {
                        UserId = request.UserID
                    };
                    var uDetails = userML.GetAppUserDetailByID(req);
                    tAPIRequest.UserMobile = uDetails.MobileNo;
                    tAPIRequest.EmailID = uDetails.EmailID;
                    tAPIRequest.StateName = uDetails.State;
                    tAPIRequest.Address = uDetails.Address;
                    tAPIRequest.City = uDetails.City;
                    tAPIRequest.Pincode = Convert.ToInt32(uDetails.Pincode ?? "0");

                    IMoneyTransferAPIML moneyTransferAPIML = new CashFreePayoutML(_accessor, _env, _dal, _ValResp.APIGroupCode);
                    senderLoginResponse = moneyTransferAPIML.CreateBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.HYPTO)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new HyptoML(_accessor, _env, tAPIRequest.APIID, _dal);
                    senderLoginResponse = moneyTransferAPIML.CreateBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.FINODIN)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new FinodinML(_accessor, _env, _dal, APICode.FINODIN, _ValResp.APIID, _ValResp.APIGroupCode);
                    senderLoginResponse = moneyTransferAPIML.CreateBeneficiary(tAPIRequest);
                }
                else
                {
                    senderLoginResponse.Msg = ErrorCodes.Down;
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "CreateBeneficary",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            return senderLoginResponse;
        }
        public MSenderCreateResp GenerateOTP(MTBeneficiaryAddRequest request)
        {
            var senderCreateResp = new MSenderCreateResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var _ValResp = GetOutletNew(request.UserID, request.OutletID, request.OID);
            if (_ValResp.Statuscode != ErrorCodes.One)
            {
                senderCreateResp.Msg = _ValResp.Msg;
                senderCreateResp.Statuscode = _ValResp.Statuscode;
                senderCreateResp.ErrorCode = _ValResp.ErrorCode;
                return senderCreateResp;
            }
            var tAPIRequest = new MTAPIRequest
            {
                APIGroupCode = _ValResp.APIGroupCode,
                APIOpCode = _ValResp.APIOpCode,
                APIID = _ValResp.APIID,
                TransactionID = _ValResp.TransactionID,
                APIOutletID = _ValResp.DMTID,
                SenderMobile = request.SenderMobile,
                RequestMode = request.RequestMode,
                UserID = request.UserID,
                ReferenceID = request.ReferenceID,
                mBeneDetail = new MBeneDetail
                {
                    BeneID = request.BeneDetail.BeneID
                },
                StateID = _ValResp.StateID,
                Pincode = Convert.ToInt32(_ValResp.Pincode),
                Address = _ValResp.Address,
                DOB = _ValResp.DOB
            };
            try
            {
                if (_ValResp.APICode == APICode.MAHAGRAM)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new MahagramAPIML(_accessor, _env, _dal);
                    senderCreateResp = moneyTransferAPIML.GenerateOTP(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.YESBANK)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new YesBankML(_accessor, _env, _dal);
                    senderCreateResp = moneyTransferAPIML.GenerateOTP(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.RECHARGEDADDY)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new RechargeDaddyML(_accessor, _env, _dal);
                    senderCreateResp = moneyTransferAPIML.GenerateOTP(tAPIRequest);
                }
                else if (_ValResp.APICode.Equals(APICode.PayOneMoney))
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new PayOneMoneyML(_accessor, _env, _dal);
                    senderCreateResp = moneyTransferAPIML.GenerateOTP(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.RBLMT)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new RBLML(_accessor, _env, _dal);
                    senderCreateResp = moneyTransferAPIML.GenerateOTP(tAPIRequest);
                }
                else if (_ValResp.APICode.Equals(APICode.RPFINTECH) || _ValResp.APICode.EndsWith("FNTH"))
                {
                    if (_ValResp.APIOpCode.Equals(APIOPCODE.RPXPRESS) || _ValResp.APIOpCode.EndsWith("XPRESS"))
                    {
                        senderCreateResp.Msg = ErrorCodes.Down;
                    }
                    else
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new FintechAPIML(_accessor, _env, _ValResp.APICode, tAPIRequest.APIID, _dal);
                        senderCreateResp = moneyTransferAPIML.GenerateOTP(tAPIRequest);
                    }
                }
                else if (_ValResp.APICode == APICode.MMWFintech)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new MMWFintechML(_accessor, _env, APICode.MMWFintech, _ValResp.APIID, _dal);
                    senderCreateResp = moneyTransferAPIML.GenerateOTP(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.SPRINT)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new SprintDMTML(_accessor, _env, APICode.SPRINT, _ValResp.APIID, _dal);
                    senderCreateResp = moneyTransferAPIML.GenerateOTP(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.FINODIN)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new FinodinML(_accessor, _env, _dal, APICode.FINODIN, _ValResp.APIID, _ValResp.APIGroupCode);
                    senderCreateResp = moneyTransferAPIML.GenerateOTP(tAPIRequest);
                }
                else
                {
                    senderCreateResp.Msg = ErrorCodes.Down;
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GenerateOTP",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            return senderCreateResp;
        }
        public MSenderCreateResp SenderResendOTP(MTOTPRequest request)
        {
            var senderCreateResp = new MSenderCreateResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Unknown_Error
            };
            var _ValResp = GetOutletNew(request.UserID, request.OutletID, request.OID);
            if (_ValResp.Statuscode != ErrorCodes.One)
            {
                senderCreateResp.Msg = _ValResp.Msg;
                senderCreateResp.Statuscode = _ValResp.Statuscode;
                senderCreateResp.ErrorCode = _ValResp.ErrorCode;
                return senderCreateResp;
            }
            var tAPIRequest = new MTAPIRequest
            {
                APIGroupCode = _ValResp.APIGroupCode,
                APIOpCode = _ValResp.APIOpCode,
                APIID = _ValResp.APIID,
                TransactionID = _ValResp.TransactionID,
                APIOutletID = _ValResp.DMTID,
                SenderMobile = request.SenderMobile,
                RequestMode = request.RequestMode,
                UserID = request.UserID,
                ReferenceID = request.ReferenceID
            };
            try
            {
                if (_ValResp.APICode == APICode.AMAH)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new MobileWareML(_accessor, _env);
                    senderCreateResp = moneyTransferAPIML.SenderResendOTP(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.MRUY)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new MrupayML(_accessor, _env);
                    senderCreateResp = moneyTransferAPIML.SenderResendOTP(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.EKO)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new EKOML(_accessor, _env);
                    senderCreateResp = moneyTransferAPIML.SenderResendOTP(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.EKO2)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new EKO2ML(_accessor, _env, _dal, tAPIRequest.APIID);
                    senderCreateResp = moneyTransferAPIML.SenderResendOTP(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.MAHAGRAM)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new MahagramAPIML(_accessor, _env, _dal);
                    senderCreateResp = moneyTransferAPIML.SenderResendOTP(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.YESBANK)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new YesBankML(_accessor, _env, _dal);
                    senderCreateResp = moneyTransferAPIML.SenderResendOTP(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.RBLMT)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new RBLML(_accessor, _env, _dal);
                    senderCreateResp = moneyTransferAPIML.SenderResendOTP(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.RECHARGEDADDY)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new RechargeDaddyML(_accessor, _env, _dal);
                    senderCreateResp = moneyTransferAPIML.SenderResendOTP(tAPIRequest);
                }
                else if (_ValResp.APICode.Equals(APICode.PayOneMoney))
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new PayOneMoneyML(_accessor, _env, _dal);
                    senderCreateResp = moneyTransferAPIML.SenderResendOTP(tAPIRequest);
                }
                else if (_ValResp.APICode.Equals(APICode.RPFINTECH) || _ValResp.APICode.EndsWith("FNTH"))
                {
                    if (_ValResp.APIOpCode.Equals(APIOPCODE.RPXPRESS) || _ValResp.APIOpCode.EndsWith("XPRESS"))
                    {
                        senderCreateResp.Msg = ErrorCodes.Down;
                    }
                    else
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new FintechAPIML(_accessor, _env, _ValResp.APICode, tAPIRequest.APIID, _dal);
                        senderCreateResp = moneyTransferAPIML.SenderResendOTP(tAPIRequest);
                    }
                }
                else if (_ValResp.APICode == APICode.RECHARGEDADDY)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new MMWFintechML(_accessor, _env, APICode.MMWFintech, _ValResp.APIID, _dal);
                    senderCreateResp = moneyTransferAPIML.SenderResendOTP(tAPIRequest);
                }
                else
                {
                    senderCreateResp.Msg = ErrorCodes.Down;
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "SenderResendOTP",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = request.UserID
                });
            }
            return senderCreateResp;
        }
        public MSenderLoginResponse ValidateBeneficiary(MBeneVerifyRequest request)
        {
            var senderLoginResponse = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Unknown_Error
            };
            if ((request.OTP ?? string.Empty).Length < 3)
            {
                senderLoginResponse.Msg = nameof(ErrorCodes.Invalid_OTP).Replace("_", " ") + " or Length";
                senderLoginResponse.ErrorCode = ErrorCodes.Invalid_OTP;
                return senderLoginResponse;
            }
            var _ValResp = GetOutletNew(request.UserID, request.OutletID, request.OID);
            if (_ValResp.Statuscode != ErrorCodes.One)
            {
                senderLoginResponse.Msg = _ValResp.Msg;
                senderLoginResponse.Statuscode = _ValResp.Statuscode;
                senderLoginResponse.ErrorCode = _ValResp.ErrorCode;
                return senderLoginResponse;
            }
            var tAPIRequest = new MTAPIRequest
            {
                APIGroupCode = _ValResp.APIGroupCode,
                APIOpCode = _ValResp.APIOpCode,
                APIID = _ValResp.APIID,
                TransactionID = _ValResp.TransactionID,
                APIOutletID = _ValResp.DMTID,
                SenderMobile = request.SenderMobile,
                RequestMode = request.RequestMode,
                UserID = request.UserID,
                ReferenceID = request.ReferenceID,
                OTP = request.OTP,
                mBeneDetail = new MBeneDetail
                {
                    AccountNo = request.AccountNo,
                    MobileNo = request.MobileNo,
                    BeneID = request.BeneficiaryID
                }
            };
            try
            {
                if (_ValResp.APICode == APICode.MAHAGRAM)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new MahagramAPIML(_accessor, _env, _dal);
                    senderLoginResponse = moneyTransferAPIML.ValidateBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.YESBANK)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new YesBankML(_accessor, _env, _dal);
                    senderLoginResponse = moneyTransferAPIML.ValidateBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.RBLMT)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new RBLML(_accessor, _env, _dal);
                    senderLoginResponse = moneyTransferAPIML.ValidateBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.RECHARGEDADDY)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new RechargeDaddyML(_accessor, _env, _dal);
                    senderLoginResponse = moneyTransferAPIML.ValidateBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode.Equals(APICode.PayOneMoney))
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new PayOneMoneyML(_accessor, _env, _dal);
                    senderLoginResponse = moneyTransferAPIML.ValidateBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode.Equals(APICode.RPFINTECH) || _ValResp.APICode.EndsWith("FNTH"))
                {
                    if (_ValResp.APIOpCode.Equals(APIOPCODE.RPXPRESS) || _ValResp.APIOpCode.EndsWith("XPRESS"))
                    {
                        senderLoginResponse.Msg = ErrorCodes.Down;
                    }
                    else
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new FintechAPIML(_accessor, _env, _ValResp.APICode, tAPIRequest.APIID, _dal);
                        senderLoginResponse = moneyTransferAPIML.ValidateBeneficiary(tAPIRequest);
                    }
                }
                else if (_ValResp.APICode == APICode.EKOPAYOUT)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new EKOPayoutML(_accessor, _env, _dal);
                    senderLoginResponse = moneyTransferAPIML.ValidateBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.SECUREPAYMENT)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new SecurePaymentML(_accessor, _env, _dal, APICode.SECUREPAYMENT);
                    senderLoginResponse = moneyTransferAPIML.ValidateBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.MMWFintech)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new MMWFintechML(_accessor, _env, APICode.MMWFintech, _ValResp.APIID, _dal);
                    senderLoginResponse = moneyTransferAPIML.ValidateBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.GOTERPay)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new GoterPayML(_accessor, _env, APICode.GOTERPay, _ValResp.APIID, _dal);
                    senderLoginResponse = moneyTransferAPIML.ValidateBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.FINO)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new FINOML(_accessor, _env, _dal);
                    senderLoginResponse = moneyTransferAPIML.ValidateBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.FINODIN)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new FinodinML(_accessor, _env, _dal, APICode.FINODIN, _ValResp.APIID, _ValResp.APIGroupCode);
                    senderLoginResponse = moneyTransferAPIML.ValidateBeneficiary(tAPIRequest);
                }
                else
                {
                    senderLoginResponse.Msg = ErrorCodes.Down;
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "ValidateBeneficiary",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            return senderLoginResponse;
        }
        public MSenderLoginResponse RemoveBeneficiary(MBeneVerifyRequest request)
        {
            var senderLoginResponse = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Unknown_Error
            };
            if (!Validate.O.IsMobile(request.SenderMobile ?? string.Empty) || (request.SenderMobile ?? string.Empty).Length < 10)
            {
                senderLoginResponse.Msg = ErrorCodes.InvalidParam + " Sender Mobile Number";
                return senderLoginResponse;
            }
            if (string.IsNullOrWhiteSpace(request.BeneficiaryID))
            {
                senderLoginResponse.Msg = ErrorCodes.InvalidParam + " Beneficiary Id";
                return senderLoginResponse;
            }
            var _ValResp = GetOutletNew(request.UserID, request.OutletID, request.OID);
            if (_ValResp.Statuscode != ErrorCodes.One)
            {
                senderLoginResponse.Msg = _ValResp.Msg;
                senderLoginResponse.Statuscode = _ValResp.Statuscode;
                senderLoginResponse.ErrorCode = _ValResp.ErrorCode;
                return senderLoginResponse;
            }
            var tAPIRequest = new MTAPIRequest
            {
                APIGroupCode = _ValResp.APIGroupCode,
                APIOpCode = _ValResp.APIOpCode,
                APIID = _ValResp.APIID,
                TransactionID = _ValResp.TransactionID,
                APIOutletID = _ValResp.DMTID,
                SenderMobile = request.SenderMobile,
                RequestMode = request.RequestMode,
                UserID = request.UserID,
                ReferenceID = request.ReferenceID,
                OTP = request.OTP,
                mBeneDetail = new MBeneDetail
                {
                    BeneID = request.BeneficiaryID
                }
            };
            try
            {
                if (_ValResp.APICode == APICode.AMAH)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new MobileWareML(_accessor, _env);
                    senderLoginResponse = moneyTransferAPIML.RemoveBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.MRUY)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new MrupayML(_accessor, _env);
                    senderLoginResponse = moneyTransferAPIML.RemoveBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.EKO)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new EKOML(_accessor, _env);
                    senderLoginResponse = moneyTransferAPIML.RemoveBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.EKO2)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new EKO2ML(_accessor, _env, _dal, tAPIRequest.APIID);
                    senderLoginResponse = moneyTransferAPIML.RemoveBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.CYBERPLATPayTM)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new CyberPlateML(_dal);
                    senderLoginResponse = moneyTransferAPIML.RemoveBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.CYBERPLAT)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new CyberPlateMLIPay(_dal);
                    senderLoginResponse = moneyTransferAPIML.RemoveBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.YESBANK)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new YesBankML(_accessor, _env, _dal);
                    if (Validate.O.IsNumeric(request.OTP ?? string.Empty))
                    {
                        senderLoginResponse = moneyTransferAPIML.RemoveBeneficiaryValidate(tAPIRequest);
                    }
                    else
                    {
                        senderLoginResponse = moneyTransferAPIML.RemoveBeneficiary(tAPIRequest);
                    }
                }
                else if (_ValResp.APICode == APICode.OPENBANK)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new OpenBankML(_accessor, _env);
                    senderLoginResponse = moneyTransferAPIML.RemoveBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.PAYTM)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new PaytmML(_accessor, _env, _dal);
                    senderLoginResponse = moneyTransferAPIML.RemoveBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.PAYU)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new PayUML(_accessor, _env, _dal);
                    senderLoginResponse = moneyTransferAPIML.RemoveBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.PAYUDMT)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new PayUDmtML(_accessor, _env, _dal);
                    senderLoginResponse = moneyTransferAPIML.RemoveBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.INSTANTPayDirect)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new IPayPayoutDirectML(_accessor, _env, _dal);
                    senderLoginResponse = moneyTransferAPIML.RemoveBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode.EndsWith(APICode.RAZORPAYOUT))
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new RZRPayoutML(_accessor, _env, _dal, _ValResp.APICode);
                    senderLoginResponse = moneyTransferAPIML.RemoveBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.RECHARGEDADDY)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new RechargeDaddyML(_accessor, _env, _dal);
                    senderLoginResponse = moneyTransferAPIML.RemoveBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode.Equals(APICode.PayOneMoney))
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new PayOneMoneyML(_accessor, _env, _dal);
                    senderLoginResponse = moneyTransferAPIML.RemoveBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode.Equals(APICode.RPFINTECH) || _ValResp.APICode.EndsWith("FNTH"))
                {
                    if (_ValResp.APIOpCode.Equals(APIOPCODE.RPXPRESS) || _ValResp.APIOpCode.EndsWith("XPRESS"))
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new FintechPayoutML(_accessor, _env, _dal, _ValResp.APICode);
                        senderLoginResponse = moneyTransferAPIML.RemoveBeneficiary(tAPIRequest);
                    }
                    else
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new FintechAPIML(_accessor, _env, _ValResp.APICode, tAPIRequest.APIID, _dal);
                        senderLoginResponse = moneyTransferAPIML.RemoveBeneficiary(tAPIRequest);
                    }
                }
                else if (_ValResp.APICode == APICode.RBLMT)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new RBLML(_accessor, _env, _dal);
                    if (Validate.O.IsNumeric(request.OTP ?? string.Empty))
                    {
                        senderLoginResponse = moneyTransferAPIML.RemoveBeneficiaryValidate(tAPIRequest);
                    }
                    else
                    {
                        senderLoginResponse = moneyTransferAPIML.RemoveBeneficiary(tAPIRequest);
                    }
                }
                else if (_ValResp.APICode == APICode.ICICIBANKPAYOUT)/*icici bank*/
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new ICICIPayoutML(_accessor, _env, tAPIRequest.APIID);
                    senderLoginResponse = moneyTransferAPIML.RemoveBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.BILLAVENUE)/*icici bank*/
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new BillAvenueMT_ML(_accessor, _env, APICode.BILLAVENUE, tAPIRequest.APIID, _dal);
                    senderLoginResponse = moneyTransferAPIML.RemoveBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.INSTANTPAY)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new InstantPay_ML(_accessor, _env, APICode.INSTANTPAY, tAPIRequest.APIID, _dal);
                    if (string.IsNullOrEmpty(tAPIRequest.OTP))
                    {
                        senderLoginResponse = moneyTransferAPIML.RemoveBeneficiary(tAPIRequest);
                    }
                    else
                    {
                        senderLoginResponse = moneyTransferAPIML.RemoveBeneficiaryValidate(tAPIRequest);
                    }
                }
                else if (_ValResp.APICode == APICode.EKOPAYOUT)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new EKOPayoutML(_accessor, _env, _dal);
                    senderLoginResponse = moneyTransferAPIML.RemoveBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.Manual)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new ManualDMT_ML(_accessor, _env, _dal, APICode.Manual);
                    senderLoginResponse = moneyTransferAPIML.RemoveBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.SECUREPAYMENT)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new SecurePaymentML(_accessor, _env, _dal, APICode.SECUREPAYMENT);
                    senderLoginResponse = moneyTransferAPIML.RemoveBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.MMWFintech)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new MMWFintechML(_accessor, _env, APICode.MMWFintech, _ValResp.APIID, _dal);
                    senderLoginResponse = moneyTransferAPIML.RemoveBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.FINO)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new FINOML(_accessor, _env, _dal);
                    senderLoginResponse = moneyTransferAPIML.RemoveBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.CASHFREE)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new CashFreePayoutML(_accessor, _env, _dal, _ValResp.APIGroupCode);
                    senderLoginResponse = moneyTransferAPIML.RemoveBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.HYPTO)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new HyptoML(_accessor, _env, tAPIRequest.APIID, _dal);
                    senderLoginResponse = moneyTransferAPIML.RemoveBeneficiary(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.FINODIN)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new FinodinML(_accessor, _env, _dal, APICode.FINODIN, _ValResp.APIID, _ValResp.APIGroupCode);
                    senderLoginResponse = moneyTransferAPIML.RemoveBeneficiary(tAPIRequest);
                }
                else
                {
                    senderLoginResponse.Msg = ErrorCodes.Down;
                    return senderLoginResponse;
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "RemoveBeneficiary",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            return senderLoginResponse;
        }
        public MSenderLoginResponse ValidateRemoveBeneficiary(MBeneVerifyRequest request)
        {
            var senderLoginResponse = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Unknown_Error
            };
            if ((request.OTP ?? string.Empty).Length < 3)
            {
                senderLoginResponse.Msg = nameof(ErrorCodes.Invalid_OTP).Replace("_", " ") + " or Length";
                senderLoginResponse.ErrorCode = ErrorCodes.Invalid_OTP;
                return senderLoginResponse;
            }
            var _ValResp = GetOutletNew(request.UserID, request.OutletID, request.OID);
            if (_ValResp.Statuscode != ErrorCodes.One)
            {
                senderLoginResponse.Msg = _ValResp.Msg;
                senderLoginResponse.Statuscode = _ValResp.Statuscode;
                senderLoginResponse.ErrorCode = _ValResp.ErrorCode;
                return senderLoginResponse;
            }
            var tAPIRequest = new MTAPIRequest
            {
                APIGroupCode = _ValResp.APIGroupCode,
                APIOpCode = _ValResp.APIOpCode,
                APIID = _ValResp.APIID,
                TransactionID = _ValResp.TransactionID,
                APIOutletID = _ValResp.DMTID,
                SenderMobile = request.SenderMobile,
                RequestMode = request.RequestMode,
                UserID = request.UserID,
                ReferenceID = request.ReferenceID,
                OTP = request.OTP,
                mBeneDetail = new MBeneDetail
                {
                    AccountNo = request.AccountNo,
                    MobileNo = request.MobileNo
                }
            };
            try
            {
                if (_ValResp.APICode == APICode.YESBANK)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new YesBankML(_accessor, _env, _dal);
                    senderLoginResponse = moneyTransferAPIML.RemoveBeneficiaryValidate(tAPIRequest);
                }
                if (_ValResp.APICode == APICode.RBLMT)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new RBLML(_accessor, _env, _dal);
                    senderLoginResponse = moneyTransferAPIML.RemoveBeneficiaryValidate(tAPIRequest);
                }
                if (_ValResp.APICode == APICode.RECHARGEDADDY)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new RechargeDaddyML(_accessor, _env, _dal);
                    senderLoginResponse = moneyTransferAPIML.RemoveBeneficiaryValidate(tAPIRequest);
                }
                else if (_ValResp.APICode.Equals(APICode.PayOneMoney))
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new PayOneMoneyML(_accessor, _env, _dal);
                    senderLoginResponse = moneyTransferAPIML.RemoveBeneficiaryValidate(tAPIRequest);
                }
                else if (_ValResp.APICode.Equals(APICode.RPFINTECH) || _ValResp.APICode.EndsWith("FNTH"))
                {
                    if (_ValResp.APIOpCode.Equals(APIOPCODE.RPXPRESS) || _ValResp.APIOpCode.EndsWith("XPRESS"))
                    {
                        senderLoginResponse.Msg = ErrorCodes.Down;
                    }
                    else
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new FintechAPIML(_accessor, _env, _ValResp.APICode, tAPIRequest.APIID, _dal);
                        senderLoginResponse = moneyTransferAPIML.RemoveBeneficiaryValidate(tAPIRequest);
                    }
                }
                else if (_ValResp.APICode == APICode.INSTANTPAY)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new InstantPay_ML(_accessor, _env, APICode.INSTANTPAY, tAPIRequest.APIID, _dal);
                    senderLoginResponse = moneyTransferAPIML.RemoveBeneficiaryValidate(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.SECUREPAYMENT)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new SecurePaymentML(_accessor, _env, _dal, APICode.SECUREPAYMENT);
                    senderLoginResponse = moneyTransferAPIML.RemoveBeneficiaryValidate(tAPIRequest);
                }
                else if (_ValResp.APICode == APICode.FINODIN)
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new FinodinML(_accessor, _env, _dal, APICode.FINODIN, _ValResp.APIID, _ValResp.APIGroupCode);
                    senderLoginResponse = moneyTransferAPIML.RemoveBeneficiaryValidate(tAPIRequest);
                }
                else
                {
                    senderLoginResponse.Msg = ErrorCodes.Down;
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "ValidateRemoveBeneficiary",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            return senderLoginResponse;
        }
        public MDMTResponse VerifyAccount(MBeneVerifyRequest request)
        {
            var mDMTResponse = new MDMTResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = RechargeRespType._PENDING,
                LiveID = string.Empty,
                APIRequestID = string.Empty,
                TransactionID = string.Empty
            };
            if (string.IsNullOrWhiteSpace(request.AccountNo))
            {
                mDMTResponse.Statuscode = RechargeRespType.FAILED;
                mDMTResponse.Msg = ErrorCodes.InvalidParam + "AccountNo";
                mDMTResponse.ErrorCode = ErrorCodes.Invalid_Parameter;
                return mDMTResponse;
            }
            request.AccountNo = request.AccountNo.Replace(" ", string.Empty);
            if (string.IsNullOrWhiteSpace(request.IFSC) || request.IFSC.Length < 11)
            {
                mDMTResponse.Msg = ErrorCodes.InvalidParam + "IFSC";
                mDMTResponse.ErrorCode = ErrorCodes.Invalid_Parameter;
                return mDMTResponse;
            }
            var _ValResp = GetOutletNew(request.UserID, request.OutletID, 0, SPKeys.AccountVerification);
            if (_ValResp.Statuscode != ErrorCodes.One)
            {
                mDMTResponse.Msg = _ValResp.Msg;
                mDMTResponse.Statuscode = _ValResp.Statuscode;
                mDMTResponse.ErrorCode = _ValResp.ErrorCode;
                return mDMTResponse;
            }
            if (_ValResp.APICode == APICode.MAHAGRAM)
            {
                if (request.BankID < 1)
                {
                    mDMTResponse.Statuscode = RechargeRespType.FAILED;
                    mDMTResponse.Msg = ErrorCodes.InvalidParam + " BankID";
                    mDMTResponse.ErrorCode = ErrorCodes.Invalid_Parameter;
                    return mDMTResponse;
                }
            }
            try
            {
                string _VerificationAPICode = string.Empty;
                _VerificationAPICode = Configuration["DMR:" + _ValResp.APICode + ":VerificationAPICode"];
                _VerificationAPICode = string.IsNullOrEmpty(_VerificationAPICode) ? _ValResp.APICode : _VerificationAPICode;
                IProcedure _proc = new ProcVerifyDMTAccount(_dal);
                var procDMTRes = (DMRTransactionResponse)_proc.Call(new DMRTransactionRequest
                {
                    UserID = request.UserID,
                    OutletID = request.OutletID,
                    AccountNo = request.AccountNo,
                    APIRequestID = request.APIRequestID,
                    RequestModeID = request.RequestMode,
                    RequestIP = _info.GetRemoteIP(),
                    SenderNo = request.SenderMobile,
                    APIID = _ValResp.APIID,
                    IFSC = request.IFSC,
                    IMEI = request.IMEI,
                    BankID = request.BankID,
                    OID = _ValResp.OID,
                    AccountTableID = request.AccountTableID,
                    IsInternal = request.IsInternal
                });
                mDMTResponse.ErrorCode = procDMTRes.ErrorCode;
                mDMTResponse.TransactionID = procDMTRes.TransactionID ?? string.Empty;
                mDMTResponse.Balance = procDMTRes.Balance;
                mDMTResponse.Bank = procDMTRes.Bank;

                if (procDMTRes.Statuscode == ErrorCodes.Minus1)
                {
                    mDMTResponse.Statuscode = RechargeRespType.FAILED;
                    mDMTResponse.Msg = procDMTRes.Msg;
                    return mDMTResponse;
                }
                var tAPIRequest = new MTAPIRequest
                {
                    APIGroupCode = _ValResp.APIGroupCode,
                    APIOpCode = _ValResp.APIOpCode,
                    APIID = _ValResp.APIID,
                    TransactionID = procDMTRes.TransactionID,
                    TID = procDMTRes.TID,
                    APIOutletID = _ValResp.DMTID,
                    SenderMobile = request.SenderMobile,
                    RequestMode = request.RequestMode,
                    UserID = request.UserID,
                    ReferenceID = request.ReferenceID,
                    OTP = request.OTP,
                    IPAddress = _info.GetRemoteIP(),
                    mBeneDetail = new MBeneDetail
                    {
                        BeneID = request.BeneficiaryID,
                        IFSC = request.IFSC,
                        AccountNo = request.AccountNo,
                        BankName = request.Bank,
                        MobileNo = request.SenderMobile,
                        BeneName = request.BeneficiaryName,
                        BankID = request.BankID
                    },
                    TransMode = request.TransMode,
                    Pincode = Convert.ToInt32(_ValResp.Pincode),
                    Address = _ValResp.Address,
                    DOB = _ValResp.DOB,
                    StateID = _ValResp.StateID

                };
                var APIRes = new DMRTransactionResponse
                {
                    Statuscode = RechargeRespType.PENDING,
                    Msg = RechargeRespType._PENDING,
                    LiveID = string.Empty,
                    TransactionID = string.Empty
                };
                mDMTResponse.APIRequestID = request.APIRequestID;
                if (string.IsNullOrEmpty(procDMTRes.AccountHolder))
                {
                    if (_VerificationAPICode.Equals(APICode.AMAH))
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new MobileWareML(_accessor, _env);
                        APIRes = moneyTransferAPIML.VerifyAccount(tAPIRequest);
                    }
                    else if (_VerificationAPICode.Equals(APICode.MRUY))
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new MrupayML(_accessor, _env);
                        APIRes = moneyTransferAPIML.VerifyAccount(tAPIRequest);
                    }
                    else if (_VerificationAPICode.Equals(APICode.AIRTELBANK))
                    {
                        var req = new UserRequset
                        {
                            UserId = request.UserID
                        };
                        var uDetails = new UserDetail();
                        uDetails = userML.GetAppUserDetailByID(req);
                        tAPIRequest.UserMobile = uDetails.MobileNo;
                        tAPIRequest.StateID = uDetails.StateID;
                        IMoneyTransferAPIML moneyTransferAPIML = new AirtelBankML(_accessor, _env);
                        APIRes = moneyTransferAPIML.VerifyAccount(tAPIRequest);
                    }
                    else if (_VerificationAPICode.Equals(APICode.EKO))
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new EKOML(_accessor, _env);
                        APIRes = moneyTransferAPIML.VerifyAccount(tAPIRequest);
                    }
                    else if (_VerificationAPICode.Equals(APICode.EKO2))
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new EKO2ML(_accessor, _env, _dal, tAPIRequest.APIID);
                        APIRes = moneyTransferAPIML.VerifyAccount(tAPIRequest);
                    }
                    else if (_VerificationAPICode.Equals(APICode.ICICIBANKPAYOUT))/*icici bank*/
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new ICICIPayoutML(_accessor, _env, tAPIRequest.APIID);
                        APIRes = moneyTransferAPIML.VerifyAccount(tAPIRequest);
                    }
                    else if (_VerificationAPICode.Equals(APICode.CYBERPLATPayTM))
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new CyberPlateML(_dal);
                        APIRes = moneyTransferAPIML.VerifyAccount(tAPIRequest);
                    }
                    else if (_VerificationAPICode.Equals(APICode.CYBERPLAT))
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new CyberPlateMLIPay(_dal);
                        APIRes = moneyTransferAPIML.VerifyAccount(tAPIRequest);
                    }
                    else if (_VerificationAPICode.Equals(APICode.MAHAGRAM))
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new MahagramAPIML(_accessor, _env, _dal);
                        APIRes = moneyTransferAPIML.VerifyAccount(tAPIRequest);
                    }
                    else if (_VerificationAPICode.Equals(APICode.OPENBANK))
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new OpenBankML(_accessor, _env);
                        APIRes = moneyTransferAPIML.VerifyAccount(tAPIRequest);
                    }
                    else if (_VerificationAPICode.Equals(APICode.PAYTM))
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new PaytmML(_accessor, _env, _dal);
                        APIRes = moneyTransferAPIML.VerifyAccount(tAPIRequest);
                    }
                    else if (_VerificationAPICode.Equals(APICode.YESBANK))
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new YesBankML(_accessor, _env, _dal);
                        APIRes = moneyTransferAPIML.VerifyAccount(tAPIRequest);
                    }
                    else if (_VerificationAPICode.Equals(APICode.PAYU))
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new PayUML(_accessor, _env, _dal);
                        APIRes = moneyTransferAPIML.VerifyAccount(tAPIRequest);
                    }
                    else if (_VerificationAPICode.Equals(APICode.PAYUDMT))
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new PayUDmtML(_accessor, _env, _dal);
                        APIRes = moneyTransferAPIML.VerifyAccount(tAPIRequest);
                    }
                    else if (_VerificationAPICode.Equals(APICode.INSTANTPayDirect))
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new IPayPayoutDirectML(_accessor, _env, _dal);
                        APIRes = moneyTransferAPIML.VerifyAccount(tAPIRequest);
                    }
                    else if (_VerificationAPICode.EndsWith(APICode.RAZORPAYOUT))
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new RZRPayoutML(_accessor, _env, _dal, _VerificationAPICode);
                        APIRes = moneyTransferAPIML.VerifyAccount(tAPIRequest);
                    }
                    else if (_VerificationAPICode.Equals(APICode.RECHARGEDADDY))
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new RechargeDaddyML(_accessor, _env, _dal);
                        APIRes = moneyTransferAPIML.VerifyAccount(tAPIRequest);
                    }
                    else if (_ValResp.APICode.Equals(APICode.PayOneMoney))
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new PayOneMoneyML(_accessor, _env, _dal);
                        APIRes = moneyTransferAPIML.VerifyAccount(tAPIRequest);
                    }
                    else if (_VerificationAPICode.Equals(APICode.RPFINTECH) || _VerificationAPICode.EndsWith("FNTH"))
                    {
                        if (_VerificationAPICode.Equals(APIOPCODE.RPXPRESS) || _VerificationAPICode.EndsWith("XPRESS"))
                        {
                            IMoneyTransferAPIML moneyTransferAPIML = new FintechPayoutML(_accessor, _env, _dal, _VerificationAPICode);
                            APIRes = moneyTransferAPIML.VerifyAccount(tAPIRequest);
                        }
                        else
                        {
                            if (!_VerificationAPICode.Equals(_ValResp.APIOpCode))
                            {
                                tAPIRequest.APIOpCode = "DMT";
                                //tAPIRequest.APIOutletID = "11191";
                            }
                            IMoneyTransferAPIML moneyTransferAPIML = new FintechAPIML(_accessor, _env, _VerificationAPICode, tAPIRequest.APIID, _dal);
                            APIRes = moneyTransferAPIML.VerifyAccount(tAPIRequest);
                        }
                    }
                    else if (_VerificationAPICode.Equals(APICode.RBLMT))
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new RBLML(_accessor, _env, _dal);
                        APIRes = moneyTransferAPIML.VerifyAccount(tAPIRequest);
                    }
                    else if (_VerificationAPICode == APICode.BILLAVENUE)
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new BillAvenueMT_ML(_accessor, _env, APICode.BILLAVENUE, tAPIRequest.APIID, _dal);
                        APIRes = moneyTransferAPIML.VerifyAccount(tAPIRequest);
                    }
                    else if (_VerificationAPICode == APICode.INSTANTPAY)
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new InstantPay_ML(_accessor, _env, APICode.INSTANTPAY, tAPIRequest.APIID, _dal);
                        APIRes = moneyTransferAPIML.VerifyAccount(tAPIRequest);
                    }
                    else if (_VerificationAPICode == APICode.EKOPAYOUT)
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new EKOPayoutML(_accessor, _env, _dal);
                        APIRes = moneyTransferAPIML.VerifyAccount(tAPIRequest);
                    }
                    else if (_VerificationAPICode == APICode.Manual)
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new ManualDMT_ML(_accessor, _env, _dal, APICode.Manual);
                        APIRes = moneyTransferAPIML.VerifyAccount(tAPIRequest);
                    }
                    else if (_VerificationAPICode == APICode.SECUREPAYMENT)
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new SecurePaymentML(_accessor, _env, _dal, APICode.SECUREPAYMENT);
                        APIRes = moneyTransferAPIML.VerifyAccount(tAPIRequest);
                    }
                    else if (_VerificationAPICode == APICode.APIBX)
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new APIBoxMTML(_accessor, _env, tAPIRequest.APIID, _dal);
                        APIRes = moneyTransferAPIML.VerifyAccount(tAPIRequest);
                    }
                    else if (_VerificationAPICode == APICode.HYPTO)
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new HyptoML(_accessor, _env, tAPIRequest.APIID, _dal);
                        APIRes = moneyTransferAPIML.VerifyAccount(tAPIRequest);
                    }
                    else if (_VerificationAPICode == APICode.MMWFintech)
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new MMWFintechML(_accessor, _env, APICode.MMWFintech, _ValResp.APIID, _dal);
                        APIRes = moneyTransferAPIML.VerifyAccount(tAPIRequest);
                    }
                    else if (_VerificationAPICode == APICode.SPRINT)
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new SprintDMTML(_accessor, _env, APICode.SPRINT, _ValResp.APIID, _dal);
                        APIRes = moneyTransferAPIML.VerifyAccount(tAPIRequest);
                    }
                    else if (_VerificationAPICode == APICode.GOTERPay)
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new GoterPayML(_accessor, _env, APICode.GOTERPay, _ValResp.APIID, _dal);
                        APIRes = moneyTransferAPIML.VerifyAccount(tAPIRequest);
                    }
                    else if (_VerificationAPICode == APICode.FINO)
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new FINOML(_accessor, _env, _dal);
                        APIRes = moneyTransferAPIML.VerifyAccount(tAPIRequest);
                    }
                    else if (_VerificationAPICode == APICode.ZOOP)
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new ZoopML(_accessor, _env, _dal, tAPIRequest.APIID);
                        APIRes = moneyTransferAPIML.VerifyAccount(tAPIRequest);
                    }
                    else if (_VerificationAPICode.Equals(APICode.CASHFREE))
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new CashFreePayoutML(_accessor, _env, _dal, _ValResp.APIGroupCode);
                        APIRes = moneyTransferAPIML.VerifyAccount(tAPIRequest);
                    }
                    else if (_VerificationAPICode.Equals(APICode.FINODIN))
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new FinodinML(_accessor, _env, _dal, APICode.FINODIN, _ValResp.APIID, _ValResp.APIGroupCode);
                        APIRes = moneyTransferAPIML.VerifyAccount(tAPIRequest);
                    }
                    else
                    {
                        mDMTResponse.Msg = ErrorCodes.Down;
                        return mDMTResponse;
                    }
                }
                else
                {
                    APIRes.BeneName = procDMTRes.AccountHolder;
                    APIRes.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                    APIRes.Statuscode = RechargeRespType.SUCCESS;
                    APIRes.ErrorCode = ErrorCodes.Transaction_Successful;
                    APIRes.LiveID = procDMTRes.LiveID;
                }
                mDMTResponse.ErrorCode = APIRes.ErrorCode;
                mDMTResponse.Msg = APIRes.Msg;
                mDMTResponse.LiveID = APIRes.LiveID;
                mDMTResponse.Statuscode = APIRes.Statuscode;
                mDMTResponse.BeneName = APIRes.BeneName;
                APIRes.TID = tAPIRequest.TID;
                APIRes.TransactionID = tAPIRequest.TransactionID;
                new ProcUpdateDMRTransaction(_dal).Call(APIRes);
            }

            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "VerifyAccount",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            return mDMTResponse;
        }
        public MDMTResponse AccountTransfer(MBeneVerifyRequest request)
        {
            var mDMTResponse = new MDMTResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = RechargeRespType._PENDING,
                LiveID = string.Empty,
                APIRequestID = string.Empty,
                TransactionID = string.Empty
            };
            if (string.IsNullOrWhiteSpace(request.AccountNo))
            {
                mDMTResponse.Statuscode = RechargeRespType.FAILED;
                mDMTResponse.Msg = ErrorCodes.InvalidParam + " AccountNo";
                return mDMTResponse;
            }
            request.AccountNo = request.AccountNo.Replace(" ", string.Empty);
            if (string.IsNullOrWhiteSpace(request.IFSC) || request.IFSC.Length != 11)
            {
                mDMTResponse.Statuscode = RechargeRespType.FAILED;
                mDMTResponse.Msg = ErrorCodes.InvalidParam + " IFSC";
                return mDMTResponse;
            }
            if (request.Amount < 1)
            {
                mDMTResponse.Statuscode = RechargeRespType.FAILED;
                mDMTResponse.Msg = ErrorCodes.InvalidParam + " Amount";
                return mDMTResponse;
            }
            var _ValResp = GetOutletNew(request.UserID, request.OutletID, request.OID);
            if (_ValResp.Statuscode != ErrorCodes.One)
            {
                mDMTResponse.Statuscode = RechargeRespType.FAILED;
                mDMTResponse.Msg = _ValResp.Msg;
                mDMTResponse.ErrorCode = _ValResp.ErrorCode;
                return mDMTResponse;
            }
            if (_ValResp.APICode == APICode.MAHAGRAM)
            {
                if (request.BankID < 1)
                {
                    mDMTResponse.Statuscode = RechargeRespType.FAILED;
                    mDMTResponse.Msg = ErrorCodes.InvalidParam + " BankID";
                    return mDMTResponse;
                }
            }

            _ValResp.MaxLimitPerTransaction = _ValResp.MaxLimitPerTransaction < 1 ? 5000 : _ValResp.MaxLimitPerTransaction;
            _ValResp.CBA = _ValResp.CBA < 1 ? 5000 : _ValResp.CBA;
            if (_ValResp.CBA > _ValResp.MaxLimitPerTransaction)
            {
                mDMTResponse.Statuscode = RechargeRespType.FAILED;
                mDMTResponse.Msg = "(CBA)Operator Down";
                mDMTResponse.ErrorCode = 152;
                return mDMTResponse;
            }

            mDMTResponse.APIRequestID = request.APIRequestID;
            if (request.RequestMode == RequestMode.API && _ValResp.MaxLimitPerTransaction < request.Amount)
            {
                mDMTResponse.Statuscode = RechargeRespType.FAILED;
                mDMTResponse.Msg = string.Format("Transaction Amount Should be between {0}-{1}", "100", _ValResp.MaxLimitPerTransaction);
                mDMTResponse.ErrorCode = DMTErrorCodes.Transaction_Amount_Should_be_between_Min_Max;
                return mDMTResponse;
            }
            var Helper = ConverterHelper.O.SplitAmounts(request.Amount, 0, _ValResp.MaxLimitPerTransaction);
            var TransactedAmount = 0;
            foreach (var item in Helper)
            {
                try
                {
                    IProcedure _proc = new ProcAccountTransfer(_dal);
                    var procDMTRes = (DMRTransactionResponse)_proc.Call(new DMRTransactionRequest
                    {
                        UserID = request.UserID,
                        OutletID = request.OutletID,
                        AccountNo = request.AccountNo,
                        APIRequestID = request.APIRequestID,
                        RequestModeID = request.RequestMode,
                        RequestIP = _info.GetRemoteIP(),
                        SenderNo = request.SenderMobile,
                        APIID = _ValResp.APIID,
                        IFSC = request.IFSC,
                        IMEI = request.IMEI,
                        BankID = request.BankID,
                        OID = request.OID,
                        APISenderLimit = _ValResp.MaxLimitPerTransaction,
                        AmountWithoutSplit = request.Amount,
                        CBA = _ValResp.CBA,
                        SecureKey = request.SecureKey,
                        Bank = request.Bank,
                        GroupID = _ValResp.TransactionID,
                        BeneName = request.BeneficiaryName,
                        TransMode = request.TransMode,
                        AmountR = item.Amount,
                        TransactedAmount = TransactedAmount,

                    });
                    mDMTResponse.TID = procDMTRes.TID;
                    mDMTResponse.ErrorCode = procDMTRes.ErrorCode;
                    mDMTResponse.TransactionID = procDMTRes.TransactionID ?? string.Empty;
                    mDMTResponse.Balance = procDMTRes.Balance;
                    if (procDMTRes.Statuscode == ErrorCodes.Minus1)
                    {
                        mDMTResponse.Statuscode = RechargeRespType.FAILED;
                        mDMTResponse.Msg = procDMTRes.Msg;
                        mDMTResponse.ErrorCode = procDMTRes.ErrorCode;
                        return mDMTResponse;
                    }
                    request.TransMode = procDMTRes.TransMode;
                    TransactedAmount += item.Amount;
                    var tAPIRequest = new MTAPIRequest
                    {
                        APIGroupCode = _ValResp.APIGroupCode,
                        APIOpCode = _ValResp.APIOpCode,
                        APIID = _ValResp.APIID,
                        TransactionID = _ValResp.TransactionID,
                        TID = procDMTRes.TID,
                        APIOutletID = _ValResp.DMTID,
                        Address = _ValResp.Address,
                        DOB = _ValResp.DOB,
                        StateID = _ValResp.StateID,
                        SenderMobile = request.SenderMobile,
                        RequestMode = request.RequestMode,
                        UserID = request.UserID,
                        ReferenceID = request.ReferenceID,
                        OTP = request.OTP,
                        IPAddress = _info.GetRemoteIP(),
                        WebsiteName = _ValResp.WebsiteName,
                        mBeneDetail = new MBeneDetail
                        {
                            BeneID = request.BeneficiaryID,
                            IFSC = request.IFSC,
                            AccountNo = request.AccountNo,
                            BankName = request.Bank,
                            BeneName = request.BeneficiaryName,
                            MobileNo = request.SenderMobile,
                            BankID = request.BankID
                        },
                        Amount = item.Amount,
                        TransMode = request.TransMode,
                        Lattitude = (_ValResp.Latlong ?? string.Empty).Contains(",") ? _ValResp.Latlong.Split(',')[0] : (_ValResp.Latlong ?? string.Empty),
                        Longitude = (_ValResp.Latlong ?? string.Empty).Contains(",") ? _ValResp.Latlong.Split(',')[1] : (_ValResp.Latlong ?? string.Empty),
                        PANNo = _ValResp.PAN,
                        Pincode = Convert.ToInt32(string.IsNullOrEmpty(_ValResp.Pincode) ? "123456" : _ValResp.Pincode)
                    };
                    var req = new UserRequset
                    {
                        UserId = request.UserID
                    };
                    var uDetails = new UserDetail();
                    uDetails = userML.GetAppUserDetailByID(req);
                    tAPIRequest.UserName = uDetails.OutletName;
                    tAPIRequest.UserMobile = uDetails.MobileNo;
                    tAPIRequest.StateID = uDetails.StateID;
                    tAPIRequest.EmailID = uDetails.EmailID;
                    var APIRes = new DMRTransactionResponse
                    {
                        Statuscode = RechargeRespType.PENDING,
                        Msg = RechargeRespType._PENDING,
                        LiveID = string.Empty,
                        TransactionID = string.Empty
                    };
                    if (_ValResp.APICode.Equals(APICode.AMAH))
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new MobileWareML(_accessor, _env);
                        APIRes = moneyTransferAPIML.AccountTransfer(tAPIRequest);
                    }
                    else if (_ValResp.APICode.Equals(APICode.MRUY))
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new MrupayML(_accessor, _env);
                        APIRes = moneyTransferAPIML.AccountTransfer(tAPIRequest);
                    }
                    else if (_ValResp.APICode.Equals(APICode.AIRTELBANK))
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new AirtelBankML(_accessor, _env);
                        APIRes = moneyTransferAPIML.AccountTransfer(tAPIRequest);
                    }
                    else if (_ValResp.APICode.Equals(APICode.OPENBANK))
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new OpenBankML(_accessor, _env);
                        APIRes = moneyTransferAPIML.AccountTransfer(tAPIRequest);
                    }
                    else if (_ValResp.APICode.Equals(APICode.ICICIBANK))
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new ICICIML(_accessor, _env);
                        APIRes = moneyTransferAPIML.AccountTransfer(tAPIRequest);
                    }
                    else if (_ValResp.APICode == APICode.ICICIBANKPAYOUT)/*icici bank*/
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new ICICIPayoutML(_accessor, _env, tAPIRequest.APIID);
                        APIRes = moneyTransferAPIML.AccountTransfer(tAPIRequest);
                    }
                    else if (_ValResp.APICode.Equals(APICode.PAYTM))
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new PaytmML(_accessor, _env, _dal, _ValResp.APIID);
                        APIRes = moneyTransferAPIML.AccountTransfer(tAPIRequest);
                    }
                    else if (_ValResp.APICode.Equals(APICode.EKO))
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new EKOML(_accessor, _env);
                        APIRes = moneyTransferAPIML.AccountTransfer(tAPIRequest);
                    }
                    else if (_ValResp.APICode.Equals(APICode.EKO2))
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new EKO2ML(_accessor, _env, _dal, tAPIRequest.APIID);
                        APIRes = moneyTransferAPIML.AccountTransfer(tAPIRequest);
                    }
                    else if (_ValResp.APICode.Equals(APICode.CYBERPLATPayTM))
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new CyberPlateML(_dal);
                        APIRes = moneyTransferAPIML.AccountTransfer(tAPIRequest);
                    }
                    else if (_ValResp.APICode.Equals(APICode.CYBERPLAT))
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new CyberPlateMLIPay(_dal);
                        APIRes = moneyTransferAPIML.AccountTransfer(tAPIRequest);
                    }
                    else if (_ValResp.APICode.Equals(APICode.MAHAGRAM))
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new MahagramAPIML(_accessor, _env, _dal);
                        APIRes = moneyTransferAPIML.AccountTransfer(tAPIRequest);
                    }
                    else if (_ValResp.APICode.Equals(APICode.YESBANK))
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new YesBankML(_accessor, _env, _dal);
                        APIRes = moneyTransferAPIML.AccountTransfer(tAPIRequest);
                    }
                    else if (_ValResp.APICode.Equals(APICode.PAYU))
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new PayUML(_accessor, _env, _dal);
                        APIRes = moneyTransferAPIML.AccountTransfer(tAPIRequest);
                    }
                    else if (_ValResp.APICode.Equals(APICode.PAYUDMT))
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new PayUDmtML(_accessor, _env, _dal);
                        APIRes = moneyTransferAPIML.AccountTransfer(tAPIRequest);
                    }
                    else if (_ValResp.APICode.Equals(APICode.INSTANTPayDirect))
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new IPayPayoutDirectML(_accessor, _env, _dal);
                        APIRes = moneyTransferAPIML.AccountTransfer(tAPIRequest);
                    }
                    else if (_ValResp.APICode.EndsWith(APICode.RAZORPAYOUT))
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new RZRPayoutML(_accessor, _env, _dal, _ValResp.APICode);
                        APIRes = moneyTransferAPIML.AccountTransfer(tAPIRequest);
                    }
                    else if (_ValResp.APICode.Equals(APICode.RECHARGEDADDY))
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new RechargeDaddyML(_accessor, _env, _dal);
                        APIRes = moneyTransferAPIML.AccountTransfer(tAPIRequest);
                    }
                    else if (_ValResp.APICode.Equals(APICode.PayOneMoney))
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new PayOneMoneyML(_accessor, _env, _dal);
                        APIRes = moneyTransferAPIML.AccountTransfer(tAPIRequest);
                    }
                    else if (_ValResp.APICode.Equals(APICode.RPFINTECH) || _ValResp.APICode.EndsWith("FNTH"))
                    {
                        if (_ValResp.APIOpCode.Equals(APIOPCODE.RPXPRESS) || _ValResp.APIOpCode.EndsWith("XPRESS"))
                        {
                            IMoneyTransferAPIML moneyTransferAPIML = new FintechPayoutML(_accessor, _env, _dal, _ValResp.APICode);
                            APIRes = moneyTransferAPIML.AccountTransfer(tAPIRequest);
                        }
                        else
                        {
                            IMoneyTransferAPIML moneyTransferAPIML = new FintechAPIML(_accessor, _env, _ValResp.APICode, tAPIRequest.APIID, _dal);
                            APIRes = moneyTransferAPIML.AccountTransfer(tAPIRequest);
                        }
                    }
                    else if (_ValResp.APICode.Equals(APICode.RBLMT))
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new RBLML(_accessor, _env, _dal);
                        APIRes = moneyTransferAPIML.AccountTransfer(tAPIRequest);
                    }
                    else if (_ValResp.APICode.Equals(APICode.BILLAVENUE))
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new BillAvenueMT_ML(_accessor, _env, APICode.BILLAVENUE, tAPIRequest.APIID, _dal);
                        APIRes = moneyTransferAPIML.AccountTransfer(tAPIRequest);
                    }
                    else if (_ValResp.APICode.Equals(APICode.INSTANTPAY))
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new InstantPay_ML(_accessor, _env, APICode.INSTANTPAY, tAPIRequest.APIID, _dal);
                        APIRes = moneyTransferAPIML.AccountTransfer(tAPIRequest);
                    }
                    else if (_ValResp.APICode == APICode.EKOPAYOUT)
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new EKOPayoutML(_accessor, _env, _dal);
                        APIRes = moneyTransferAPIML.AccountTransfer(tAPIRequest);
                    }
                    else if (_ValResp.APICode == APICode.Manual)
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new ManualDMT_ML(_accessor, _env, _dal, APICode.Manual);
                        APIRes = moneyTransferAPIML.AccountTransfer(tAPIRequest);
                    }
                    else if (_ValResp.APICode == APICode.SECUREPAYMENT)
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new SecurePaymentML(_accessor, _env, _dal, APICode.SECUREPAYMENT);
                        APIRes = moneyTransferAPIML.AccountTransfer(tAPIRequest);
                    }
                    else if (_ValResp.APICode == APICode.MMWFintech)
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new MMWFintechML(_accessor, _env, APICode.MMWFintech, _ValResp.APIID, _dal);
                        APIRes = moneyTransferAPIML.AccountTransfer(tAPIRequest);
                    }
                    else if (_ValResp.APICode == APICode.SPRINT)
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new SprintDMTML(_accessor, _env, APICode.SPRINT, _ValResp.APIID, _dal);
                        APIRes = moneyTransferAPIML.AccountTransfer(tAPIRequest);
                    }
                    else if (_ValResp.APICode == APICode.GOTERPay)
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new GoterPayML(_accessor, _env, APICode.GOTERPay, _ValResp.APIID, _dal);
                        APIRes = moneyTransferAPIML.AccountTransfer(tAPIRequest);
                    }
                    else if (_ValResp.APICode == APICode.FINO)
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new FINOML(_accessor, _env, _dal);
                        APIRes = moneyTransferAPIML.AccountTransfer(tAPIRequest);
                    }
                    else if (_ValResp.APICode.Equals(APICode.CASHFREE))
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new CashFreePayoutML(_accessor, _env, _dal, _ValResp.APIGroupCode);
                        APIRes = moneyTransferAPIML.AccountTransfer(tAPIRequest);
                    }
                    else if (_ValResp.APICode == APICode.HYPTO)
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new HyptoML(_accessor, _env, tAPIRequest.APIID, _dal);
                        APIRes = moneyTransferAPIML.AccountTransfer(tAPIRequest);
                    }
                    else if (_ValResp.APICode == APICode.FINODIN)
                    {
                        IMoneyTransferAPIML moneyTransferAPIML = new FinodinML(_accessor, _env, _dal, APICode.FINODIN, _ValResp.APIID, _ValResp.APIGroupCode);
                        APIRes = moneyTransferAPIML.AccountTransfer(tAPIRequest);
                    }
                    else
                    {
                        mDMTResponse.Msg = ErrorCodes.Down;
                        return mDMTResponse;
                    }

                    mDMTResponse.ErrorCode = APIRes.ErrorCode;
                    mDMTResponse.Msg = APIRes.Msg;
                    mDMTResponse.LiveID = APIRes.LiveID;
                    mDMTResponse.Statuscode = APIRes.Statuscode;
                    APIRes.TID = mDMTResponse.TID;
                    APIRes.TransactionID = tAPIRequest.TransactionID;
                    if (_ValResp.APICode != APICode.Manual)
                    {
                        var sameUpdate = (ResponseStatus)(new ProcUpdateDMRTransaction(_dal)).Call(APIRes);
                        if (sameUpdate.Statuscode == ErrorCodes.Minus1)
                        {
                            procDMTRes.IsInternalSender = false;
                            if (_ValResp.APICode == APICode.Manual)
                            {
                                if (sameUpdate.Msg.Contains((char)160))
                                {
                                    mDMTResponse.LiveID = sameUpdate.Msg.Split((char)160)[1].ToString();
                                    mDMTResponse.Msg = sameUpdate.Msg.Split((char)160)[0].ToString();
                                }
                            }

                        }
                    }
                    if (APIRes.Statuscode == RechargeRespType.SUCCESS && procDMTRes.IsInternalSender)
                    {
                        var senderRequest = (SenderRequest)new ProcGetSender(_dal).Call(request.SenderMobile);
                        //Only for Internal Sender
                        new AlertML(_accessor, _env, false).PayoutSMS(new AlertReplacementModel
                        {
                            LoginID = request.UserID,
                            UserMobileNo = request.SenderMobile,
                            WID = 1,
                            Amount = tAPIRequest.Amount,
                            AccountNo = tAPIRequest.mBeneDetail.AccountNo,
                            SenderName = senderRequest.Name,
                            TransMode = tAPIRequest.TransMode,
                            UTRorRRN = APIRes.LiveID,
                            IFSC = tAPIRequest.mBeneDetail.IFSC,
                            BrandName = mDMTResponse.BrandName
                        });
                    }
                }
                catch (Exception ex)
                {
                    var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                    {
                        ClassName = GetType().Name,
                        FuncName = "AccountTransfer",
                        Error = ex.Message,
                        LoginTypeID = LoginType.ApplicationUser,
                        UserId = request.UserID
                    });
                }
            }
            mDMTResponse.GroupID = _ValResp.TransactionID;
            return mDMTResponse;
        }
        public ChargeAmount GetCharge(MTGetChargeRequest request)
        {
            var mDMTResponse = new ChargeAmount
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = RechargeRespType._PENDING,
                Charged = 0
            };

            var _ValResp = GetOutletNew(request.UserID, request.OutletID, request.OID);
            if (_ValResp.Statuscode != ErrorCodes.One)
            {
                mDMTResponse.Msg = _ValResp.Msg;
                mDMTResponse.Statuscode = _ValResp.Statuscode;
                mDMTResponse.ErrorCode = _ValResp.ErrorCode;
                return mDMTResponse;
            }

            _ValResp.MaxLimitPerTransaction = _ValResp.MaxLimitPerTransaction < 1 ? 5000 : _ValResp.MaxLimitPerTransaction;
            _ValResp.CBA = _ValResp.CBA < 1 ? 5000 : _ValResp.CBA;

            var Helper = ConverterHelper.O.SplitAmounts(request.Amount, 0, _ValResp.MaxLimitPerTransaction);
            foreach (var item in Helper)
            {
                try
                {
                    IProcedure _proc = new ProcGetDMRChargeP(_dal);
                    var procDMTRes = (ChargeAmount)_proc.Call(new CommonReq
                    {
                        LoginID = request.UserID,
                        CommonDecimal = item.Amount,
                        CommonInt = request.OID,
                        CommonInt2 = _ValResp.MaxLimitPerTransaction,
                        CommonInt3 = _ValResp.CBA
                    });
                    mDMTResponse.Statuscode = procDMTRes.Statuscode;
                    mDMTResponse.Msg = procDMTRes.Msg;
                    if (procDMTRes.Statuscode == ErrorCodes.Minus1)
                    {
                        break;
                    }
                    else
                    {
                        mDMTResponse.Charged = mDMTResponse.Charged + procDMTRes.Charged;
                    }

                }
                catch (Exception ex)
                {
                    var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                    {
                        ClassName = GetType().Name,
                        FuncName = "GetCharge",
                        Error = ex.Message,
                        LoginTypeID = LoginType.ApplicationUser,
                        UserId = request.UserID
                    });
                }
            }

            return mDMTResponse;
        }
        private void OTPForDMT(string OTP, string MobileNo, string EmailID, int WID, int UserID)
        {
            IUserML uml = new UserML(_accessor, _env);
            var alertData = uml.GetUserDeatilForAlert(UserID);
            if (alertData.Statuscode == ErrorCodes.One)
            {
                IAlertML alertMl = new AlertML(_accessor, _env);
                alertData.OTP = OTP;
                alertData.UserMobileNo = MobileNo;
                alertData.EmailID = EmailID;
                alertData.WID = WID;
                alertData.UserID = UserID;
                alertMl.OTPSMS(alertData);
                //if (!string.IsNullOrEmpty(EmailID))
                //    alertMl.OTPEmail(alertData);
            }
        }
        public MDMTResponse DoUPIPaymentService(MBeneVerifyRequest request)
        {
            var mDMTResponse = new MDMTResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = RechargeRespType._PENDING,
                LiveID = string.Empty,
                APIRequestID = string.Empty,
                TransactionID = string.Empty
            };
            if (string.IsNullOrWhiteSpace(request.AccountNo))
            {
                mDMTResponse.Statuscode = RechargeRespType.FAILED;
                mDMTResponse.Msg = ErrorCodes.InvalidParam + " AccountNo";
                return mDMTResponse;
            }
            if (request.Amount < 1)
            {
                mDMTResponse.Statuscode = RechargeRespType.FAILED;
                mDMTResponse.Msg = ErrorCodes.InvalidParam + " Amount";
                return mDMTResponse;
            }
            IProcedure proc = new ProcUPIPaymentService(_dal);
            var procRes = (UPIPaymentProcRes)proc.Call(new UPIPaymentProcReq
            {
                AccountNo = request.AccountNo,
                AmountR = request.Amount,
                APIRequestID = request.APIRequestID,
                UserID = request.UserID,
                RequestMode = request.RequestMode,
                IPAddress = _info.GetRemoteIP()
            });

            if (procRes.Statuscode == ErrorCodes.One)
            {
                var tAPIRequest = new MTAPIRequest
                {
                    APIID = procRes.APIID,
                    TransactionID = procRes.TransactionID,
                    TID = procRes.TID,
                    RequestMode = request.RequestMode,
                    UserID = request.UserID,
                    ReferenceID = request.ReferenceID,
                    UserName = request.BeneficiaryName,
                    IPAddress = _info.GetRemoteIP(),
                    SenderMobile = procRes.OutletMobile,
                    mBeneDetail = new MBeneDetail
                    {
                        AccountNo = request.AccountNo,
                        BeneName = request.BeneficiaryName,
                        MobileNo = procRes.OutletMobile
                    },
                    Amount = request.Amount,
                    IsPayout = true,
                    EmailID = procRes.EmailID,
                    TransMode = procRes.OpCode,
                    Lattitude = (procRes.Latlong ?? string.Empty).Contains(",") ? procRes.Latlong.Split(',')[0] : (procRes.Latlong ?? string.Empty),
                    Longitude = (procRes.Latlong ?? string.Empty).Contains(",") ? procRes.Latlong.Split(',')[1] : (procRes.Latlong ?? string.Empty)
                };
                var APIRes = new DMRTransactionResponse
                {
                    Statuscode = RechargeRespType.PENDING,
                    Msg = RechargeRespType._PENDING,
                    LiveID = string.Empty,
                    TransactionID = string.Empty
                };
                if (procRes.APICode == APICode.INSTANTPayDirect)
                {

                    IPayPayoutDirectML apiML = new IPayPayoutDirectML(_accessor, _env, _dal);
                    APIRes = apiML.UPIAccountTransfer(tAPIRequest);
                }
                if (procRes.APICode == APICode.RPFINTECH)
                {
                    tAPIRequest.TransMode = nameof(PaymentMode_.UPI);
                    var moneyTransferAPIML = new FintechPayoutML(_accessor, _env, _dal, APICode.RPFINTECH);
                    APIRes = moneyTransferAPIML.AccountTransfer(tAPIRequest);
                }
                if (procRes.APICode.Equals(APICode.RAZORPAYOUT) || procRes.APICode.EndsWith("RZRPOT"))
                {
                    IMoneyTransferAPIML moneyTransferAPIML = new RZRPayoutML(_accessor, _env, _dal, procRes.APICode);
                    tAPIRequest.TransMode = nameof(PaymentMode_.UPI);
                    APIRes = moneyTransferAPIML.AccountTransfer(tAPIRequest);
                }
                mDMTResponse.ErrorCode = APIRes.ErrorCode;
                mDMTResponse.Msg = APIRes.Msg;
                mDMTResponse.LiveID = APIRes.LiveID;
                mDMTResponse.Statuscode = APIRes.Statuscode;
                mDMTResponse.TransactionID = tAPIRequest.TransactionID;
                // APIRes.TID = mDMTResponse.TID;
                APIRes.TID = tAPIRequest.TID;
                APIRes.TransactionID = tAPIRequest.TransactionID;
                if (procRes.APICode != APICode.Manual)
                    new ProcUpdateDMRTransaction(_dal).Call(APIRes);
            }
            else
            {
                mDMTResponse.Statuscode = procRes.Statuscode;
                mDMTResponse.Msg = procRes.Msg;
                mDMTResponse.ErrorCode = procRes.ErrorCode;
            }
            return mDMTResponse;
        }
        public void GetBalance()
        {
            FINOML opml = new FINOML(_accessor, _env, _dal);
            opml.GetBalance();
        }

    }
}
