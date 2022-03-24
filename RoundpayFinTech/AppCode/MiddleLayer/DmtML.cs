using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer.Dmt_Api;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.StaticModel.MoneyTransfer;
using RoundpayFinTech.AppCode.ThirdParty.CyberPlate;
using RoundpayFinTech.AppCode.ThirdParty.Mahagram;
using RoundpayFinTech.AppCode.ThirdParty.Paytm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Validators;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public class DmtML : IDmtML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        private string response;
        private readonly IRequestInfo _info;
        private readonly IResourceML _resourceML;
        private readonly UserML userML;
        public DmtML(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            _info = new RequestInfo(_accessor, _env);
            _resourceML = new ResourceML(_accessor, _env);
            userML = new UserML(_accessor, _env);
        }
        public DmtML(IHttpContextAccessor accessor, IHostingEnvironment env, IDAL dal, IRequestInfo info)
        {
            _accessor = accessor;
            _env = env;
            _dal = dal;
            _info = info;
            _resourceML = new ResourceML(_accessor, _env);
            userML = new UserML(_accessor, _env);
        }
        public ValidateAPIOutletResp GetOutlet(DMTReq dMTReq)
        {
            var resp = new ValidateAPIOutletResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            var _req = new CommonReq
            {
                LoginID = dMTReq.UserID,
                CommonInt = dMTReq.OutletID,
                IsListType = dMTReq.IsValidate
            };
            IProcedure _proc = new ProcValidateAPIRequestOutlet(_dal);
            resp = (ValidateAPIOutletResp)_proc.Call(_req);
            return resp;
        }
        private ValidateAPIOutletResp GetOutletNew(DMTReq dMTReq)
        {
            var resp = new ValidateAPIOutletResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            var _req = new CommonReq
            {
                LoginID = dMTReq.UserID,
                CommonInt = dMTReq.OutletID,
                CommonStr = "DMT"
            };
            IProcedure _proc = new ProcValidateOutletForOperator(_dal);
            resp = (ValidateAPIOutletResp)_proc.Call(_req);
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
            return resp;
        }
        public async Task<IResponseStatus> CheckSender(DMTReq dMTReq)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            if (!Validate.O.IsMobile(dMTReq.SenderNO ?? "") || dMTReq.SenderNO.Length < 10)
            {
                res.Msg = ErrorCodes.InvalidParam + "Sender Mobile Number";
                return res;
            }
            var _ValResp = GetOutletNew(dMTReq);
            if (_ValResp.Statuscode != ErrorCodes.One)
            {
                res.Msg = _ValResp.Msg;
                return res;
            }
            try
            {
                dMTReq.ApiID = _ValResp.APIID;
                dMTReq.TID = _ValResp.TransactionID;

                if (_ValResp.APICode == APICode.AMAH)
                {
                    var wareWL = new MobileWareML(_accessor, _env);
                    res = wareWL.CheckSender(dMTReq);
                }
                else if (_ValResp.APICode == APICode.MRUY)
                {
                    dMTReq.APIOutletID = _ValResp.DMTID;
                    var wareWL = new MrupayML(_accessor, _env);
                    res = wareWL.CheckSender(dMTReq);
                }
                else if (_ValResp.APICode == APICode.AIRTELBANK)/*airtel bank*/
                {
                    dMTReq.APIOutletID = _ValResp.DMTID;
                    var airtelBankML = new AirtelBankML(_accessor, _env);
                    res = await airtelBankML.CheckSender(dMTReq).ConfigureAwait(false);
                }
                else if (_ValResp.APICode == APICode.OPENBANK)/*open bank*/
                {
                    dMTReq.APIOutletID = _ValResp.DMTID;
                    var openBankML = new OpenBankML(_accessor, _env, _ValResp.APIID);
                    res = await openBankML.CheckSender(dMTReq).ConfigureAwait(false);
                }
                else if (_ValResp.APICode == APICode.ICICIBANK)/*icici bank*/
                {
                    dMTReq.APIOutletID = _ValResp.DMTID;
                    var iCICIML = new ICICIML(_accessor, _env, _ValResp.APIID);
                    res = await iCICIML.CheckSender(dMTReq).ConfigureAwait(false);
                }
                else if (_ValResp.APICode == APICode.PAYTM)/*paytm bank*/
                {
                    dMTReq.APIOutletID = _ValResp.DMTID;
                    var paytmML = new PaytmML(_accessor, _env, _ValResp.APIID);
                    res = await paytmML.CheckSender(dMTReq).ConfigureAwait(false);
                }
                else if (_ValResp.APICode == APICode.EKO)/*EKO*/
                {
                    dMTReq.APIOutletID = _ValResp.DMTID;
                    var eKOML = new EKOML(_accessor, _env, _ValResp.APIID);
                    res = await eKOML.GetCustomerInformation(dMTReq).ConfigureAwait(false);
                }
                else if (_ValResp.APICode == APICode.CYBERPLATPayTM)
                {
                    dMTReq.APIOutletID = _ValResp.DMTID;
                    ICyberDMTMLPayTM cyberPTM = new CyberPlateML(_dal, _ValResp.APIID);
                    res = cyberPTM.RemitterDetailsPTM(dMTReq);
                }
                else if (_ValResp.APICode == APICode.CYBERPLAT)
                {
                    ICyberDMTMLIPay cyberIpay = new CyberPlateML(_dal, _ValResp.APIID);
                    var beniRespones = cyberIpay.RemitterDetailsIPay(dMTReq);
                    res.Statuscode = beniRespones.Statuscode;
                    res.ReffID = beniRespones.ReffID;
                    res.Msg = beniRespones.Msg;
                    res.ErrorCode = beniRespones.ErrorCode;
                    res.CommonInt = beniRespones.CommonInt;
                    res.CommonStr = beniRespones.CommonStr;
                    res.CommonStr2 = beniRespones.CommonStr2;
                }
                else if (_ValResp.APICode == APICode.MAHAGRAM)
                {
                    dMTReq.APIOutletID = _ValResp.DMTID;
                    var mahagram = new MahagramAPIML(_accessor, _env, _dal, _ValResp.APIID);
                    res = await mahagram.GetMGSender(dMTReq).ConfigureAwait(false);
                }
                else
                {
                    res.Msg = ErrorCodes.Down;
                    return res;
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "CheckSender",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = dMTReq.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }
        public async Task<IResponseStatus> DoSenderKYC(CreateSen _req)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };

            if (Validate.O.IsNumeric(_req.senderRequest.NameOnKYC ?? string.Empty) || (_req.senderRequest.NameOnKYC ?? string.Empty).Length > 100)
            {
                res.Msg = ErrorCodes.InvalidParam + " Name";
                return res;
            }
            if (!Validate.O.IsAADHAR(_req.senderRequest.AadharNo ?? string.Empty))
            {
                res.Msg = ErrorCodes.InvalidParam + " Aadhar Number";
                return res;
            }
            if (!Validate.O.IsPAN(_req.senderRequest.PANNo ?? string.Empty))
            {
                res.Msg = ErrorCodes.InvalidParam + " PAN Number";
                return res;
            }
            else
            {
                _req.senderRequest.PANNo = _req.senderRequest.PANNo.ToUpper();
            }

            if (!Validate.O.IsMobile(_req.senderRequest.MobileNo ?? "") || _req.senderRequest.MobileNo.Length < 10)
            {
                res.Msg = ErrorCodes.InvalidParam + "Sender Mobile Number";
                return res;
            }
            var AadharFrontName = ContentDispositionHeaderValue.Parse(_req.senderRequest.AadharFront.ContentDisposition).FileName.Trim('"').Replace(" ", "");
            var AadharBackName = ContentDispositionHeaderValue.Parse(_req.senderRequest.AadharBack.ContentDisposition).FileName.Trim('"').Replace(" ", "");
            var SenderPhotoName = ContentDispositionHeaderValue.Parse(_req.senderRequest.SenderPhoto.ContentDisposition).FileName.Trim('"').Replace(" ", "");
            var PANName = ContentDispositionHeaderValue.Parse(_req.senderRequest.PAN.ContentDisposition).FileName.Trim('"').Replace(" ", "");
            byte[] AadharFrontContent = null, AadharBackContent = null, SenderPhotoContent = null, PANContent = null;

            using (var ms = new MemoryStream())
            {
                _req.senderRequest.AadharFront.CopyTo(ms);
                AadharFrontContent = ms.ToArray();
            }
            if (!Validate.O.IsFileAllowed(AadharFrontContent, Path.GetExtension(AadharFrontName)))
            {
                res.Msg = "Aadhar front image is not valid";
                return res;
            }
            using (var ms = new MemoryStream())
            {
                _req.senderRequest.AadharBack.CopyTo(ms);
                AadharBackContent = ms.ToArray();
            }
            if (!Validate.O.IsFileAllowed(AadharBackContent, Path.GetExtension(AadharBackName)))
            {
                res.Msg = "Aadhar back image is not valid";
                return res;
            }
            using (var ms = new MemoryStream())
            {
                _req.senderRequest.SenderPhoto.CopyTo(ms);
                SenderPhotoContent = ms.ToArray();
            }
            if (!Validate.O.IsFileAllowed(SenderPhotoContent, Path.GetExtension(SenderPhotoName)))
            {
                res.Msg = "Sender photo is not valid";
                return res;
            }
            using (var ms = new MemoryStream())
            {
                _req.senderRequest.PAN.CopyTo(ms);
                PANContent = ms.ToArray();
            }
            if (!Validate.O.IsFileAllowed(PANContent, Path.GetExtension(PANName)))
            {
                res.Msg = "PAN Image is not valid";
                return res;
            }

            var dMTReq = _req.dMTReq;
            var _ValResp = GetOutletNew(dMTReq);
            if (_ValResp.Statuscode != ErrorCodes.One)
            {
                res.Msg = _ValResp.Msg;
            }
            try
            {
                dMTReq.ApiID = _ValResp.APIID;
                dMTReq.TID = _ValResp.TransactionID;
                if (_ValResp.APICode == APICode.EKO)
                {
                    var aadharFrontURL = DOCType.SenderKYCPath + _req.dMTReq.SenderNO + nameof(_req.senderRequest.AadharFront) + AadharFrontName;
                    var aadharBackURL = DOCType.SenderKYCPath + _req.dMTReq.SenderNO + nameof(_req.senderRequest.AadharBack) + AadharBackName;
                    var SenderPhotoURL = DOCType.SenderKYCPath + _req.dMTReq.SenderNO + nameof(_req.senderRequest.SenderPhoto) + SenderPhotoName;
                    var PANURL = DOCType.SenderKYCPath + _req.dMTReq.SenderNO + nameof(_req.senderRequest.PAN) + PANName;
                    using (FileStream fs = File.Create(aadharFrontURL))
                    {
                        _req.senderRequest.AadharFront.CopyTo(fs);
                        fs.Flush();
                    }
                    using (FileStream fs = File.Create(aadharBackURL))
                    {
                        _req.senderRequest.AadharBack.CopyTo(fs);
                        fs.Flush();
                    }
                    using (FileStream fs = File.Create(SenderPhotoURL))
                    {
                        _req.senderRequest.SenderPhoto.CopyTo(fs);
                        fs.Flush();
                    }
                    using (FileStream fs = File.Create(PANURL))
                    {
                        _req.senderRequest.PAN.CopyTo(fs);
                        fs.Flush();
                    }
                    var eKOML = new EKOML(_accessor, _env, _ValResp.APIID);
                    res = await eKOML.CreateCustomerforKYC(_req, aadharFrontURL, aadharBackURL, SenderPhotoURL, PANURL);
                    try
                    {
                        File.SetAttributes(aadharFrontURL, FileAttributes.Normal);
                        File.Delete(aadharFrontURL);
                        File.SetAttributes(aadharBackURL, FileAttributes.Normal);
                        File.Delete(aadharBackURL);
                        File.SetAttributes(SenderPhotoURL, FileAttributes.Normal);
                        File.Delete(SenderPhotoURL);
                        File.SetAttributes(PANURL, FileAttributes.Normal);
                        File.Delete(PANURL);
                    }
                    catch (Exception)
                    {

                    }
                }
                else
                {
                    res.Msg = ErrorCodes.Down;
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "DoSenderKYC",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = dMTReq.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }
        public async Task<IResponseStatus> CreateSender(CreateSen _req)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };

            if (Validate.O.IsNumeric(_req.senderRequest.Name ?? "") || (_req.senderRequest.Name ?? "").Length > 100)
            {
                res.Msg = ErrorCodes.InvalidParam + " Name";
                return res;
            }
            if (Validate.O.IsNumeric(_req.senderRequest.LastName ?? "") || (_req.senderRequest.LastName ?? "").Length > 100)
            {
                res.Msg = ErrorCodes.InvalidParam + " LastName";
                return res;
            }
            if (string.IsNullOrWhiteSpace(_req.senderRequest.Pincode) || _req.senderRequest.Pincode.Length != 6)
            {
                res.Msg = ErrorCodes.InvalidParam + " Pincode";
                return res;
            }
            if (string.IsNullOrWhiteSpace(_req.senderRequest.Address))
            {
                res.Msg = ErrorCodes.InvalidParam + " Address";
                return res;
            }
            if (_req.senderRequest.Address.Length > 150)
            {
                res.Msg = ErrorCodes.InvalidParam + " Address. Length not greater than 150";
                return res;
            }
            if (string.IsNullOrWhiteSpace(_req.senderRequest.Dob) || !Validate.O.IsDateIn_dd_MMM_yyyy_Format(_req.senderRequest.Dob ?? ""))
            {
                res.Msg = ErrorCodes.InvalidParam + " Date of brith";
                return res;
            }
            if (!Validate.O.IsMobile(_req.senderRequest.MobileNo ?? "") || _req.senderRequest.MobileNo.Length < 10)
            {
                res.Msg = ErrorCodes.InvalidParam + "Sender Mobile Number";
                return res;
            }
            var userML = new UserML(_accessor, _env, false);
            var pincodeDetail = userML.GetPinCodeDetail(_req.senderRequest.Pincode);
            if (string.IsNullOrWhiteSpace(pincodeDetail.City))
            {
                res.Msg = ErrorCodes.InvalidParam + " Pincode";
                return res;
            }

            _req.senderRequest.City = pincodeDetail.Districtname;
            _req.senderRequest.StateID = pincodeDetail.StateID;
            _req.senderRequest.Area = pincodeDetail.Area;
            var dMTReq = _req.dMTReq;
            var _ValResp = GetOutletNew(dMTReq);
            if (_ValResp.Statuscode != ErrorCodes.One)
            {
                res.Msg = _ValResp.Msg;
            }
            try
            {
                dMTReq.ApiID = _ValResp.APIID;
                dMTReq.TID = _ValResp.TransactionID;
                if (_ValResp.APICode == APICode.AMAH)
                {
                    var wareWL = new MobileWareML(_accessor, _env);
                    res = wareWL.CreateSender(_req, 1);
                }
                else if (_ValResp.APICode == APICode.MRUY)
                {
                    dMTReq.APIOutletID = _ValResp.DMTID;
                    var wareWL = new MrupayML(_accessor, _env);
                    res = wareWL.CreateSender(_req);
                }
                else if (_ValResp.APICode == APICode.AIRTELBANK)/*airtel create sender*/
                {
                    dMTReq.APIOutletID = _ValResp.DMTID;
                    var wareWL = new AirtelBankML(_accessor, _env);
                    res = wareWL.CreateSender(_req);
                    if (res.CommonInt == ErrorCodes.One)
                    {
                        OTPForDMT(res.CommonStr, _req.senderRequest.MobileNo, string.Empty, res.CommonInt2, dMTReq.UserID);
                        res.CommonStr = string.Empty;
                        res.CommonInt2 = 0;
                    }
                }
                else if (_ValResp.APICode == APICode.OPENBANK)/*open bank create sender*/
                {
                    dMTReq.APIOutletID = _ValResp.DMTID;
                    var openBankML = new OpenBankML(_accessor, _env, _ValResp.APIID);
                    res = openBankML.CreateSender(_req);
                    if (res.CommonInt == ErrorCodes.One)
                    {
                        OTPForDMT(res.CommonStr, _req.senderRequest.MobileNo, string.Empty, res.CommonInt2, dMTReq.UserID);
                        res.CommonStr = string.Empty;
                        res.CommonInt2 = 0;
                    }
                }
                else if (_ValResp.APICode == APICode.ICICIBANK)/*icici bank create sender*/
                {
                    dMTReq.APIOutletID = _ValResp.DMTID;
                    var iCICIML = new ICICIML(_accessor, _env, _ValResp.APIID);
                    res = iCICIML.CreateSender(_req);
                    if (res.CommonInt == ErrorCodes.One)
                    {
                        OTPForDMT(res.CommonStr, _req.senderRequest.MobileNo, string.Empty, res.CommonInt2, dMTReq.UserID);
                        res.CommonStr = string.Empty;
                        res.CommonInt2 = 0;
                    }
                }
                else if (_ValResp.APICode == APICode.PAYTM)/*paytm create sender*/
                {
                    dMTReq.APIOutletID = _ValResp.DMTID;
                    var paytmML = new PaytmML(_accessor, _env, _ValResp.APIID);
                    res = paytmML.CreateSender(_req);
                    if (res.CommonInt == ErrorCodes.One)
                    {
                        OTPForDMT(res.CommonStr, _req.senderRequest.MobileNo, string.Empty, res.CommonInt2, dMTReq.UserID);
                        res.CommonStr = string.Empty;
                        res.CommonInt2 = 0;
                    }
                }
                else if (_ValResp.APICode == APICode.EKO)
                {
                    var eKOML = new EKOML(_accessor, _env, _ValResp.APIID);
                    res = await eKOML.CreateCustomer(_req).ConfigureAwait(false);
                }
                else if (_ValResp.APICode == APICode.CYBERPLATPayTM)
                {
                    dMTReq.APIOutletID = _ValResp.DMTID;
                    ICyberDMTMLPayTM cyberPTM = new CyberPlateML(_dal, _ValResp.APIID);
                    res = cyberPTM.GetOTPForRegistrationPTM(dMTReq);
                    if (res.Statuscode == ErrorCodes.One)
                    {
                        var senderRequest = new SenderRequest
                        {
                            Address = _req.senderRequest.Address,
                            Name = _req.senderRequest.Name + " " + _req.senderRequest.LastName,
                            Area = _req.senderRequest.Area,
                            MobileNo = _req.senderRequest.MobileNo,
                            Pincode = _req.senderRequest.Pincode,
                            ReffID = res.ReffID,
                            UserID = _req.dMTReq.UserID,
                            Dob = _req.senderRequest.Dob,
                            StateID = _req.senderRequest.StateID,
                            City = _req.senderRequest.City
                        };
                        res.ReffID = string.Empty;
                        IProcedure _proc = new ProcUpdateSender(_dal);
                        var senderInfo = (SenderInfo)_proc.Call(senderRequest);
                        if (senderInfo.Statuscode == ErrorCodes.One)
                        {
                            res.ReffID = "S" + senderInfo.SelfRefID;
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = nameof(DMTErrorCodes.Partial_Registration_Please_verify_sender_with_OTP).Replace("_", " ");
                            res.CommonInt = ErrorCodes.One;
                            res.ErrorCode = DMTErrorCodes.Partial_Registration_Please_verify_sender_with_OTP;
                            res.CommonBool = true;
                        }
                    }
                }
                else if (_ValResp.APICode == APICode.CYBERPLAT)
                {
                    ICyberDMTMLIPay cyberIPay = new CyberPlateML(_dal, _ValResp.APIID);
                    res = cyberIPay.RemitterRegistrationIPay(_req);
                }
                else if (_ValResp.APICode == APICode.MAHAGRAM)
                {
                    dMTReq.APIOutletID = _ValResp.DMTID;
                    var mahagram = new MahagramAPIML(_accessor, _env, _dal, _ValResp.APIID);
                    res = await mahagram.MGGenerateAirtelOTP(dMTReq);
                    if (res.Statuscode == ErrorCodes.One)
                    {
                        var senderRequest = new SenderRequest
                        {
                            Address = _req.senderRequest.Address,
                            Name = _req.senderRequest.Name + " " + _req.senderRequest.LastName,
                            Area = _req.senderRequest.Area,
                            MobileNo = _req.senderRequest.MobileNo,
                            Pincode = _req.senderRequest.Pincode,
                            ReffID = _req.senderRequest.MobileNo,
                            UserID = _req.dMTReq.UserID,
                            Dob = _req.senderRequest.Dob,
                            StateID = _req.senderRequest.StateID,
                            City = _req.senderRequest.City
                        };
                        res.ReffID = string.Empty;
                        IProcedure _proc = new ProcUpdateSender(_dal);
                        var senderInfo = (SenderInfo)_proc.Call(senderRequest);
                        if (senderInfo.Statuscode == ErrorCodes.One)
                        {
                            res.ReffID = "S" + senderInfo.SelfRefID;
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = nameof(DMTErrorCodes.Partial_Registration_Please_verify_sender_with_OTP).Replace("_", " ");
                            res.CommonInt = ErrorCodes.One;
                            res.ErrorCode = DMTErrorCodes.Partial_Registration_Please_verify_sender_with_OTP;
                            res.CommonBool = true;
                        }
                    }
                }
                else
                {
                    res.Msg = ErrorCodes.Down;
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "CreateSender",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = dMTReq.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }
        public async Task<IResponseStatus> SenderResendOTP(DMTReq dMTReq)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var _ValResp = GetOutletNew(dMTReq);
            if (_ValResp.Statuscode != ErrorCodes.One)
            {
                res.Msg = _ValResp.Msg;
            }
            try
            {
                dMTReq.ApiID = _ValResp.APIID;
                dMTReq.TID = _ValResp.TransactionID;
                if (_ValResp.APICode == APICode.AMAH)
                {
                    var wareWL = new MobileWareML(_accessor, _env);
                    res = wareWL.SenderOTPResend(dMTReq, 1);
                }
                else if (_ValResp.APICode == APICode.MRUY)
                {
                    dMTReq.APIOutletID = _ValResp.DMTID;
                    var wareWL = new MrupayML(_accessor, _env);
                    res = wareWL.SenderOTPResend(dMTReq);
                }
                else if (_ValResp.APICode == APICode.EKO)
                {
                    dMTReq.APIOutletID = _ValResp.DMTID;
                    var eKOML = new EKOML(_accessor, _env, _ValResp.APIID);
                    res = await eKOML.ResendOTP(dMTReq).ConfigureAwait(false);
                }
                else if (_ValResp.APICode == APICode.MAHAGRAM)
                {
                    dMTReq.APIOutletID = _ValResp.DMTID;
                    var mahagram = new MahagramAPIML(_accessor, _env, _dal, _ValResp.APIID);
                    res = await mahagram.MGReGenerateAirtelOTP(dMTReq).ConfigureAwait(false);
                }
                else
                {
                    res.Msg = ErrorCodes.Down;
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "SenderResendOTP",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = dMTReq.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }
        public async Task<IResponseStatus> VerifySender(DMTReq dMTReq, string OTP)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            if (string.IsNullOrWhiteSpace(OTP) || OTP.Length < 3)
            {
                res.Msg = nameof(ErrorCodes.Invalid_OTP).Replace("_", " ") + " or Length";
                res.ErrorCode = ErrorCodes.Invalid_OTP;
                return res;
            }
            var _ValResp = GetOutletNew(dMTReq);
            if (_ValResp.Statuscode != ErrorCodes.One)
            {
                res.Msg = _ValResp.Msg;
                return res;
            }
            try
            {
                dMTReq.ApiID = _ValResp.APIID;
                dMTReq.TID = _ValResp.TransactionID;
                if (_ValResp.APICode == APICode.AMAH)
                {
                    var wareWL = new MobileWareML(_accessor, _env);
                    res = wareWL.VerifySender(dMTReq, OTP);
                }
                else if (_ValResp.APICode == APICode.MRUY)
                {
                    dMTReq.APIOutletID = _ValResp.DMTID;
                    var wareWL = new MrupayML(_accessor, _env);
                    res = wareWL.VerifySender(dMTReq, OTP);
                }
                else if (_ValResp.APICode == APICode.AIRTELBANK)
                {
                    dMTReq.APIOutletID = _ValResp.DMTID;
                    var req = new UserRequset
                    {
                        UserId = dMTReq.UserID
                    };
                    var uDetails = new UserDetail();
                    uDetails = userML.GetAppUserDetailByID(req);
                    dMTReq.UserMobileNo = uDetails.MobileNo;
                    dMTReq.StateID = uDetails.StateID;
                    var wareWL = new AirtelBankML(_accessor, _env);
                    res = wareWL.VerifySender(dMTReq, OTP);
                }
                else if (_ValResp.APICode == APICode.OPENBANK)
                {
                    dMTReq.APIOutletID = _ValResp.DMTID;
                    var req = new UserRequset
                    {
                        UserId = dMTReq.UserID
                    };
                    var uDetails = new UserDetail();
                    uDetails = userML.GetAppUserDetailByID(req);
                    dMTReq.UserMobileNo = uDetails.MobileNo;
                    var obML = new OpenBankML(_accessor, _env, _ValResp.APIID);
                    res = obML.VerifySender(dMTReq, OTP);
                }
                else if (_ValResp.APICode == APICode.ICICIBANK)
                {
                    dMTReq.APIOutletID = _ValResp.DMTID;
                    var req = new UserRequset
                    {
                        UserId = dMTReq.UserID
                    };
                    var uDetails = new UserDetail();
                    uDetails = userML.GetAppUserDetailByID(req);
                    dMTReq.UserMobileNo = uDetails.MobileNo;
                    var obML = new ICICIML(_accessor, _env, _ValResp.APIID);
                    res = obML.VerifySender(dMTReq, OTP);
                }
                else if (_ValResp.APICode == APICode.PAYTM)
                {
                    dMTReq.APIOutletID = _ValResp.DMTID;
                    var req = new UserRequset
                    {
                        UserId = dMTReq.UserID
                    };
                    var uDetails = new UserDetail();
                    uDetails = userML.GetAppUserDetailByID(req);
                    dMTReq.UserMobileNo = uDetails.MobileNo;
                    var paytmML = new PaytmML(_accessor, _env, _ValResp.APIID);
                    res = paytmML.VerifySender(dMTReq, OTP);
                }
                else if (_ValResp.APICode == APICode.EKO)
                {
                    var eKOML = new EKOML(_accessor, _env, _ValResp.APIID);
                    res = await eKOML.VerifyCustomerIdentity(dMTReq, OTP).ConfigureAwait(false);
                }
                else if (_ValResp.APICode == APICode.CYBERPLATPayTM)
                {
                    dMTReq.APIOutletID = _ValResp.DMTID;
                    res.Msg = nameof(ErrorCodes.Invalid_OTP).Replace("_", " ");
                    res.ErrorCode = ErrorCodes.Invalid_OTP;
                    if (!string.IsNullOrEmpty(dMTReq.ReffID))
                    {
                        if (dMTReq.ReffID.ToUpper()[0] == 'S')
                        {
                            var SelfRefID = dMTReq.ReffID.Replace(dMTReq.ReffID[0].ToString(), string.Empty);
                            if (Validate.O.IsNumeric(SelfRefID))
                            {
                                var senderRequest = new SenderRequest
                                {

                                    MobileNo = dMTReq.SenderNO,
                                    SelfRefID = Convert.ToInt32(SelfRefID),
                                    UserID = dMTReq.UserID
                                };
                                IProcedure _proc = new ProcUpdateSender(_dal);
                                var senderInfo = (SenderInfo)_proc.Call(senderRequest);
                                if (senderInfo.Statuscode == ErrorCodes.One)
                                {
                                    var createSen = new CreateSen
                                    {
                                        OTP = OTP,
                                        senderRequest = new SenderRequest
                                        {
                                            Name = (senderInfo.Name ?? string.Empty).Contains(" ") ? senderInfo.Name.Split(' ')[0] : senderInfo.Name,
                                            LastName = (senderInfo.Name ?? string.Empty).Contains(" ") ? senderInfo.Name.Split(' ')[1] : senderInfo.Name,
                                            Area = senderInfo.Area,
                                            Address = senderInfo.Address,
                                            Districtname = senderInfo.Districtname,
                                            City = senderInfo.City,
                                            Statename = senderInfo.Statename,
                                            Pincode = senderInfo.Pincode,
                                            MobileNo = senderInfo.MobileNo
                                        },
                                        dMTReq = dMTReq
                                    };
                                    var cyberPTM = new CyberPlateML(_dal, _ValResp.APIID);
                                    res = cyberPTM.RemitterRegistrationPTM(createSen, senderInfo.ReffID);
                                }
                            }
                        }
                    }
                }
                else if (_ValResp.APICode == APICode.CYBERPLAT)
                {
                    ICyberDMTMLIPay cyberIpay = new CyberPlateML(_dal, _ValResp.APIID);
                    res = cyberIpay.RemitterOTPVerifyIPay(dMTReq, OTP);
                }
                else if (_ValResp.APICode == APICode.MAHAGRAM)
                {
                    dMTReq.APIOutletID = _ValResp.DMTID;
                    res.Msg = nameof(ErrorCodes.Invalid_OTP).Replace("_", " ");
                    res.ErrorCode = ErrorCodes.Invalid_OTP;
                    if (!string.IsNullOrEmpty(dMTReq.ReffID))
                    {
                        if (dMTReq.ReffID.ToUpper()[0] == 'S')
                        {
                            var SelfRefID = dMTReq.ReffID.Replace(dMTReq.ReffID[0].ToString(), string.Empty);
                            if (Validate.O.IsNumeric(SelfRefID))
                            {
                                var senderRequest = new SenderRequest
                                {

                                    MobileNo = dMTReq.SenderNO,
                                    SelfRefID = Convert.ToInt32(SelfRefID),
                                    UserID = dMTReq.UserID
                                };
                                IProcedure _proc = new ProcUpdateSender(_dal);
                                var senderInfo = (SenderInfo)_proc.Call(senderRequest);
                                if (senderInfo.Statuscode == ErrorCodes.One)
                                {
                                    var createSen = new CreateSen
                                    {
                                        OTP = OTP,
                                        senderRequest = new SenderRequest
                                        {
                                            Name = (senderInfo.Name ?? string.Empty).Contains(" ") ? senderInfo.Name.Split(' ')[0] : senderInfo.Name,
                                            LastName = (senderInfo.Name ?? string.Empty).Contains(" ") ? senderInfo.Name.Split(' ')[1] : senderInfo.Name,
                                            Area = senderInfo.Area,
                                            Address = senderInfo.Address,
                                            Districtname = senderInfo.Districtname,
                                            City = senderInfo.City,
                                            Statename = senderInfo.Statename,
                                            Pincode = senderInfo.Pincode,
                                            MobileNo = senderInfo.MobileNo,
                                            Dob = !Validate.O.IsDateIn_dd_MMM_yyyy_Format(senderInfo.Dob) ? DateTime.Now.ToString("dd-MM-yyyy") : Convert.ToDateTime(senderInfo.Dob.Replace(" ", "/")).ToString("dd-MM-yyyy"),
                                            MahagramStateCode = senderInfo.MahagramStateCode
                                        },
                                        dMTReq = dMTReq
                                    };

                                    var mahagram = new MahagramAPIML(_accessor, _env, _dal, _ValResp.APIID);
                                    res = await mahagram.MGAPICustRegistration(createSen).ConfigureAwait(false);
                                }
                            }
                        }
                    }
                }
                else
                {
                    res.Msg = ErrorCodes.Down;
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "VerifySender",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = dMTReq.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }
        public async Task<IResponseStatus> CreateBeneficiary(AddBeni addBeni, DMTReq dMTReq)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            if (Validate.O.IsNumeric(addBeni.BeneName ?? "") || (addBeni.BeneName ?? "").Length > 100)
            {
                res.Msg = ErrorCodes.InvalidParam + " Name";
                return res;
            }
            if (!Validate.O.IsMobile(addBeni.MobileNo ?? "") || addBeni.MobileNo.Length < 10)
            {
                res.Msg = ErrorCodes.InvalidParam + "Beni Mobile Number";
                return res;
            }
            if (!Validate.O.IsMobile(addBeni.SenderMobileNo ?? "") || addBeni.SenderMobileNo.Length < 10)
            {
                res.Msg = ErrorCodes.InvalidParam + "Sender Mobile Number";
                return res;
            }
            if (string.IsNullOrWhiteSpace(addBeni.IFSC) || addBeni.IFSC.Length < 11)
            {
                res.Msg = ErrorCodes.InvalidParam + "IFSC";
                return res;
            }
            if (!Validate.O.IsValidBankAccountNo(addBeni.AccountNo ?? ""))
            {
                res.Msg = ErrorCodes.InvalidParam + "AccountNo";
                return res;
            }
            if (Validate.O.IsNumeric(addBeni.BankName ?? "") || (addBeni.BankName ?? "").Length > 100)
            {
                res.Msg = ErrorCodes.InvalidParam + " BankName";
                return res;
            }
            var userML = new UserML(_accessor, _env);
            var _ValResp = GetOutletNew(dMTReq);
            if (_ValResp.Statuscode != ErrorCodes.One)
            {
                res.Msg = _ValResp.Msg;
            }
            try
            {
                dMTReq.ApiID = _ValResp.APIID;
                dMTReq.TID = _ValResp.TransactionID;
                if (_ValResp.APICode == APICode.AMAH)
                {
                    var wareWL = new MobileWareML(_accessor, _env);
                    res = wareWL.CreateBeneficiary(addBeni, dMTReq);
                }
                else if (_ValResp.APICode == APICode.MRUY)
                {
                    dMTReq.APIOutletID = _ValResp.DMTID;
                    var wareWL = new MrupayML(_accessor, _env);
                    res = wareWL.CreateBeneficiary(addBeni, dMTReq);
                }
                else if (_ValResp.APICode == APICode.AIRTELBANK)
                {

                    dMTReq.APIOutletID = _ValResp.DMTID;
                    var req = new UserRequset
                    {
                        UserId = dMTReq.UserID
                    };
                    var uDetails = new UserDetail();
                    uDetails = userML.GetAppUserDetailByID(req);
                    dMTReq.UserMobileNo = uDetails.MobileNo;
                    dMTReq.StateID = uDetails.StateID;
                    var wareWL = new AirtelBankML(_accessor, _env);
                    res = wareWL.CreateBeneficiary(addBeni, dMTReq);
                }
                else if (_ValResp.APICode == APICode.OPENBANK)
                {

                    dMTReq.APIOutletID = _ValResp.DMTID;
                    var req = new UserRequset
                    {
                        UserId = dMTReq.UserID
                    };
                    var uDetails = new UserDetail();
                    uDetails = userML.GetAppUserDetailByID(req);
                    dMTReq.UserMobileNo = uDetails.MobileNo;
                    dMTReq.StateID = uDetails.StateID;
                    var obML = new OpenBankML(_accessor, _env, _ValResp.APIID);
                    res = obML.CreateBeneficiary(addBeni, dMTReq);
                }
                else if (_ValResp.APICode == APICode.ICICIBANK)
                {

                    dMTReq.APIOutletID = _ValResp.DMTID;
                    var req = new UserRequset
                    {
                        UserId = dMTReq.UserID
                    };
                    var uDetails = new UserDetail();
                    uDetails = userML.GetAppUserDetailByID(req);
                    dMTReq.UserMobileNo = uDetails.MobileNo;
                    dMTReq.StateID = uDetails.StateID;
                    var iciciML = new ICICIML(_accessor, _env, _ValResp.APIID);
                    res = iciciML.CreateBeneficiary(addBeni, dMTReq);
                }
                else if (_ValResp.APICode == APICode.PAYTM)
                {

                    dMTReq.APIOutletID = _ValResp.DMTID;
                    var req = new UserRequset
                    {
                        UserId = dMTReq.UserID
                    };
                    var uDetails = new UserDetail();
                    uDetails = userML.GetAppUserDetailByID(req);
                    dMTReq.UserMobileNo = uDetails.MobileNo;
                    dMTReq.StateID = uDetails.StateID;
                    var paytmML = new PaytmML(_accessor, _env, _ValResp.APIID);
                    res = paytmML.CreateBeneficiary(addBeni, dMTReq);
                }
                else if (_ValResp.APICode == APICode.EKO)
                {
                    dMTReq.APIOutletID = _ValResp.DMTID;
                    var eKOML = new EKOML(_accessor, _env, _ValResp.APIID);
                    res = await eKOML.AddRecipient(addBeni, dMTReq).ConfigureAwait(false);
                }
                else if (_ValResp.APICode == APICode.CYBERPLATPayTM)
                {
                    dMTReq.APIOutletID = _ValResp.DMTID;
                    ICyberDMTMLPayTM cyberPTM = new CyberPlateML(_dal, _ValResp.APIID);
                    res = cyberPTM.BeneficiaryRegistrationPTM(addBeni, dMTReq);
                    if (res.Statuscode == ErrorCodes.One)
                    {
                        var beneficiaryModel = new BeneficiaryModel
                        {
                            IFSC = addBeni.IFSC,
                            Account = addBeni.AccountNo,
                            BankName = addBeni.BankName,
                            Name = addBeni.BeneName,
                            BeneID = res.CommonStr4,
                            SenderNo = dMTReq.SenderNO,
                            APICode = _ValResp.APICode,
                            BankID = addBeni.BankID
                        };
                        IProcedure _proc = new ProcUpdateBeneficiary(_dal);
                        beneficiaryModel = (BeneficiaryModel)_proc.Call(beneficiaryModel);
                    }
                }
                else if (_ValResp.APICode == APICode.CYBERPLAT)
                {
                    ICyberDMTMLIPay cyberIPay = new CyberPlateML(_dal, _ValResp.APIID);
                    res = cyberIPay.BeneficiaryRegistration(addBeni, dMTReq);
                }
                else if (_ValResp.APICode == APICode.MAHAGRAM)
                {
                    dMTReq.APIOutletID = _ValResp.DMTID;
                    var mahagram = new MahagramAPIML(_accessor, _env, _dal, _ValResp.APIID);
                    res = await mahagram.MGAirtelBeneAdd(addBeni, dMTReq).ConfigureAwait(false);
                }
                else
                {
                    res.Msg = ErrorCodes.Down;
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "CheckSander",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = dMTReq.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }
        public async Task<BeniRespones> GetBeneficiary(DMTReq dMTReq)
        {
            var res = new BeniRespones
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var _ValResp = GetOutletNew(dMTReq);
            if (_ValResp.Statuscode != ErrorCodes.One)
            {
                res.Msg = _ValResp.Msg;
                return res;
            }
            try
            {
                dMTReq.ApiID = _ValResp.APIID;
                dMTReq.TID = _ValResp.TransactionID;
                if (_ValResp.APICode == APICode.AMAH)
                {
                    var wareWL = new MobileWareML(_accessor, _env);
                    res = wareWL.GetBeneficiary(dMTReq);
                }
                else if (_ValResp.APICode == APICode.MRUY)
                {
                    dMTReq.APIOutletID = _ValResp.DMTID;
                    var wareWL = new MrupayML(_accessor, _env);
                    res = wareWL.GetBeneficiary(dMTReq);
                }
                else if (_ValResp.APICode == APICode.AIRTELBANK)
                {
                    dMTReq.APIOutletID = _ValResp.DMTID;
                    var wareWL = new AirtelBankML(_accessor, _env);
                    res = wareWL.GetBeneficiary(dMTReq);
                }
                else if (_ValResp.APICode == APICode.OPENBANK)
                {
                    dMTReq.APIOutletID = _ValResp.DMTID;
                    var obML = new OpenBankML(_accessor, _env, _ValResp.APIID);
                    res = obML.GetBeneficiary(dMTReq);
                }
                else if (_ValResp.APICode == APICode.ICICIBANK)
                {
                    dMTReq.APIOutletID = _ValResp.DMTID;
                    var iCICIML = new ICICIML(_accessor, _env, _ValResp.APIID);
                    res = iCICIML.GetBeneficiary(dMTReq);
                }
                else if (_ValResp.APICode == APICode.PAYTM)
                {
                    dMTReq.APIOutletID = _ValResp.DMTID;
                    var paytmML = new PaytmML(_accessor, _env, _ValResp.APIID);
                    res = paytmML.GetBeneficiary(dMTReq);
                }
                else if (_ValResp.APICode == APICode.EKO)
                {
                    var eKOML = new EKOML(_accessor, _env, _ValResp.APIID);
                    res = await eKOML.GetListofRecipients(dMTReq).ConfigureAwait(false);
                }
                else if (_ValResp.APICode == APICode.CYBERPLATPayTM)
                {
                    dMTReq.APIOutletID = _ValResp.DMTID;
                    var cyberPTM = new CyberPlateML(_dal, _ValResp.APIID);
                    res = cyberPTM.ListOfBeneficiaryPTM(dMTReq);
                    if (res.Statuscode == ErrorCodes.One)
                    {
                        if (res.addBeni != null)
                        {
                            if (res.addBeni.Count > 0)
                            {
                                var BeneIDs = string.Join(",", res.addBeni.Select(x => x.BeneID));
                                var commonReq = new CommonReq
                                {
                                    CommonStr = BeneIDs,
                                    CommonStr2 = APICode.CYBERPLATPayTM,
                                    CommonStr3 = dMTReq.SenderNO
                                };
                                IProcedureAsync procedureAsync = new ProcGetBeneficiaryByBeneIDs(_dal);
                                res.addBeni = (List<AddBeni>)await procedureAsync.Call(commonReq).ConfigureAwait(false);
                            }
                        }
                    }
                }
                else if (_ValResp.APICode == APICode.CYBERPLAT)
                {
                    ICyberDMTMLIPay cyberIpay = new CyberPlateML(_dal, _ValResp.APIID);
                    res = cyberIpay.RemitterDetailsIPay(dMTReq);
                }
                else if (_ValResp.APICode == APICode.MAHAGRAM)
                {
                    dMTReq.APIOutletID = _ValResp.DMTID;
                    var mahagram = new MahagramAPIML(_accessor, _env, _dal, _ValResp.APIID);
                    res = await mahagram.GetMGAirtelBeneDetails(dMTReq).ConfigureAwait(false);
                }
                else
                {
                    res.Msg = ErrorCodes.Down;
                    return res;
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GetBeneficiary",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = dMTReq.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }
        public async Task<IResponseStatus> GenerateOTP(DMTReq dMTReq)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var _ValResp = GetOutletNew(dMTReq);
            if (_ValResp.Statuscode != ErrorCodes.One)
            {
                res.Msg = _ValResp.Msg;
                return res;
            }
            try
            {
                dMTReq.ApiID = _ValResp.APIID;
                dMTReq.TID = _ValResp.TransactionID;
                if (_ValResp.APICode == APICode.MAHAGRAM)
                {
                    dMTReq.APIOutletID = _ValResp.DMTID;
                    var mahagram = new MahagramAPIML(_accessor, _env, _dal, _ValResp.APIID);
                    res = await mahagram.MGGenerateAirtelOTP(dMTReq).ConfigureAwait(false);
                }
                else
                {
                    res.Msg = ErrorCodes.Down;
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "VerifyBeneficiary",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = dMTReq.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }
        public async Task<IResponseStatus> ValidateBeneficiary(DMTReq dMTReq, string BeneMobile, string AccountNo, string OTP)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            if (string.IsNullOrWhiteSpace(OTP) || OTP.Length < 3)
            {
                res.Msg = nameof(ErrorCodes.Invalid_OTP).Replace("_", " ") + " or Length";
                res.ErrorCode = ErrorCodes.Invalid_OTP;
                return res;
            }
            var _ValResp = GetOutletNew(dMTReq);
            if (_ValResp.Statuscode != ErrorCodes.One)
            {
                res.Msg = _ValResp.Msg;
                return res;
            }
            try
            {
                dMTReq.ApiID = _ValResp.APIID;
                dMTReq.TID = _ValResp.TransactionID;
                if (_ValResp.APICode == APICode.MAHAGRAM)
                {
                    dMTReq.APIOutletID = _ValResp.DMTID;
                    var mahagram = new MahagramAPIML(_accessor, _env, _dal, _ValResp.APIID);
                    res = await mahagram.MGVerifyBeneOTP(dMTReq, BeneMobile, AccountNo, OTP).ConfigureAwait(false);
                }
                else
                {
                    res.Msg = ErrorCodes.Down;
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "VerifyBeneficiary",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = dMTReq.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }
        public IResponseStatus CheckRepeativeTransaction(int SessionID, int UserID, decimal Amount,string AccountNo,int ID)
        {
            IProcedure proc = new ProcPreventDMTRepeat(_dal);
            return (ResponseStatus)proc.Call(new CommonReq
            {
                CommonInt = SessionID,
                LoginID = UserID,
                CommonDecimal = Amount,
                CommonStr=AccountNo,
                CommonInt2=ID
            });
        }
        public async Task<DMRTransactionResponse> SendMoney(DMTReq dMTReq, ReqSendMoney sendMoney)
        {
            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = RechargeRespType._PENDING,
                LiveID = "",
                VendorID = "",
                TransactionID = ""
            };
            if (string.IsNullOrWhiteSpace(sendMoney.BeneID))
            {
                res.Statuscode = RechargeRespType.FAILED;
                res.Msg = ErrorCodes.InvalidParam + " Detail";
                return res;
            }
            if (string.IsNullOrWhiteSpace(sendMoney.AccountNo))
            {
                res.Statuscode = RechargeRespType.FAILED;
                res.Msg = ErrorCodes.InvalidParam + " AccountNo";
                return res;
            }
            if (sendMoney.Amount < 1)
            {
                res.Statuscode = RechargeRespType.FAILED;
                res.Msg = ErrorCodes.InvalidParam + " Amount";
                return res;
            }
            var _ValResp = GetOutletNew(dMTReq);
            if (_ValResp.Statuscode != ErrorCodes.One)
            {
                res.Statuscode = RechargeRespType.FAILED;
                res.Msg = _ValResp.Msg;
                return res;
            }
            if (_ValResp.APICode == APICode.MAHAGRAM)
            {
                if (sendMoney.BankID < 1)
                {
                    res.Statuscode = RechargeRespType.FAILED;
                    res.Msg = ErrorCodes.InvalidParam + " BankID";
                    return res;
                }
            }
            if (_ValResp.APICode == APICode.ICICIBANK)
            {
                if (!sendMoney.Channel)
                {
                    res.Statuscode = RechargeRespType.FAILED;
                    res.Msg = "Currently IMPS is allowed only!";
                    return res;
                }
            }
            bool IsSuccess = false;
            try
            {
                dMTReq.ApiID = _ValResp.APIID;
                dMTReq.TID = _ValResp.TransactionID;
                _ValResp.OType = 14;
                var Helper = ConverterHelper.O.SplitAmounts(sendMoney.Amount, 0, 0);
                foreach (var item in Helper)
                {
                    sendMoney.Amount = item.Amount;
                    var proRes = DoDMT(dMTReq, sendMoney, _ValResp);
                    res.TID = proRes.TID;
                    res.TransactionID = proRes.TransactionID;
                    res.Balance = proRes.Balance;
                    res.ErrorCode = proRes.ErrorCode;
                    if (proRes.Statuscode == ErrorCodes.Minus1)
                    {
                        res.Statuscode = RechargeRespType.FAILED;
                        res.Msg = proRes.Msg;
                        res.ErrorCode = proRes.ErrorCode;
                        return res;
                    }
                    dMTReq.TID = res.TID.ToString();
                    res.PanNo = proRes.PanNo;
                    res.Pincode = proRes.Pincode;
                    res.LatLong = proRes.LatLong;
                    if (_ValResp.APICode == APICode.AMAH)
                    {
                        var wareWL = new MobileWareML(_accessor, _env);
                        res = wareWL.SendMoney(dMTReq, sendMoney, res);
                    }
                    else if (_ValResp.APICode == APICode.MRUY)
                    {
                        dMTReq.APIOutletID = _ValResp.DMTID;
                        var wareWL = new MrupayML(_accessor, _env);
                        res = wareWL.SendMoney(dMTReq, sendMoney, res);
                    }
                    else if (_ValResp.APICode == APICode.AIRTELBANK)
                    {
                        var req = new UserRequset
                        {
                            UserId = dMTReq.UserID
                        };
                        var uDetails = new UserDetail();
                        uDetails = userML.GetAppUserDetailByID(req);
                        dMTReq.UserMobileNo = uDetails.MobileNo;
                        dMTReq.StateID = uDetails.StateID;
                        var wareWL = new AirtelBankML(_accessor, _env);
                        res = await wareWL.SendMoney(dMTReq, sendMoney, res).ConfigureAwait(false);
                    }
                    else if (_ValResp.APICode == APICode.OPENBANK)
                    {
                        var req = new UserRequset
                        {
                            UserId = dMTReq.UserID
                        };
                        var uDetails = new UserDetail();
                        uDetails = userML.GetAppUserDetailByID(req);
                        dMTReq.UserMobileNo = uDetails.MobileNo;
                        dMTReq.EmailID = uDetails.EmailID;
                        var obML = new OpenBankML(_accessor, _env, _ValResp.APIID);
                        res = await obML.InitiatePayouts(dMTReq, sendMoney, res).ConfigureAwait(false);
                    }
                    else if (_ValResp.APICode == APICode.ICICIBANK)
                    {
                        var req = new UserRequset
                        {
                            UserId = dMTReq.UserID
                        };
                        var uDetails = new UserDetail();
                        uDetails = userML.GetAppUserDetailByID(req);
                        dMTReq.UserMobileNo = uDetails.MobileNo;
                        dMTReq.EmailID = uDetails.EmailID;
                        var obML = new ICICIML(_accessor, _env, _ValResp.APIID);
                        res = await obML.P2AAccountFundTransfer(dMTReq, sendMoney, res).ConfigureAwait(false);
                    }
                    else if (_ValResp.APICode == APICode.PAYTM)
                    {
                        var req = new UserRequset
                        {
                            UserId = dMTReq.UserID
                        };
                        var uDetails = new UserDetail();
                        uDetails = userML.GetAppUserDetailByID(req);
                        dMTReq.UserMobileNo = uDetails.MobileNo;
                        dMTReq.EmailID = uDetails.EmailID;
                        dMTReq.ChanelType = sendMoney.Channel ? PaymentMode_.PAYTM_IMPS : PaymentMode_.PAYTM_Neft;
                        var paytmML = new PaytmML(_accessor, _env, _ValResp.APIID);
                        res = await paytmML.SendMoneyPayout(dMTReq, sendMoney, res).ConfigureAwait(false);
                    }
                    else if (_ValResp.APICode == APICode.EKO)
                    {
                        dMTReq.APIOutletID = _ValResp.DMTID;
                        var eKOML = new EKOML(_accessor, _env, _ValResp.APIID);
                        res = await eKOML.InitiateTransaction(dMTReq, sendMoney, res).ConfigureAwait(false);
                    }
                    else if (_ValResp.APICode == APICode.CYBERPLATPayTM)
                    {
                        dMTReq.APIOutletID = _ValResp.DMTID;
                        var cyberPTM = new CyberPlateML(_dal, _ValResp.APIID);
                        res = cyberPTM.FundTransferPTM(dMTReq, sendMoney, res);
                    }
                    else if (_ValResp.APICode == APICode.CYBERPLAT)
                    {
                        ICyberDMTMLIPay cyberIpay = new CyberPlateML(_dal, _ValResp.APIID);
                        res = cyberIpay.FundTransferIPay(dMTReq, sendMoney, res);
                    }
                    else if (_ValResp.APICode == APICode.MAHAGRAM)
                    {
                        dMTReq.APIOutletID = _ValResp.DMTID;
                        var mahagram = new MahagramAPIML(_accessor, _env, _dal, _ValResp.APIID);
                        res = await mahagram.MGPayMode(dMTReq, sendMoney, res).ConfigureAwait(false);
                    }
                    else
                    {
                        res.Msg = ErrorCodes.Down;
                        return res;
                    }
                    new ProcUpdateDMRTransaction(_dal).Call(res);
                    if ((res.Statuscode == RechargeRespType.SUCCESS || res.Statuscode == RechargeRespType.PENDING) && !IsSuccess)
                    {
                        IsSuccess = true;
                    }
                }
                res.GroupID = _ValResp.TransactionID;
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "SendMoney",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = dMTReq.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            res.Request = "";
            res.Response = "";
            if (IsSuccess)
            {
                res.Statuscode = RechargeRespType.SUCCESS;
            }
            return res;
        }
        public DMRTransactionResponse DoDMT(DMTReq dMTReq, ReqSendMoney sendMoney, ValidateAPIOutletResp _ValResp)
        {
            var dMR = new DMRTransactionRequest { AccountNo = sendMoney.AccountNo, AmountR = sendMoney.Amount, APIID = dMTReq.ApiID, APIRequestID = sendMoney.APIRequestID, BeneID = sendMoney.BeneID, IFSC = sendMoney.IFSC, OutletID = dMTReq.OutletID, RequestIP = _info.GetRemoteIP(), RequestModeID = dMTReq.RequestMode, TransMode = sendMoney.Channel ? "IMPS" : "NEFT", UserID = dMTReq.UserID, GroupID = _ValResp.TransactionID, OPType = _ValResp.OType, Bank = sendMoney.Bank, SenderNo = dMTReq.SenderNO, BeneName = sendMoney.BeneName, SecureKey = sendMoney.SecKey, BankID = sendMoney.BankID };
            IProcedure procDMR = new ProcDMRTransaction(_dal);
            return (DMRTransactionResponse)procDMR.Call(dMR);
        }
        public async Task<IResponseStatus> DeleteBeneficiary(DMTReq dMTReq, string BeniID, string OTP)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            if (!Validate.O.IsMobile(dMTReq.SenderNO ?? "") || dMTReq.SenderNO.Length < 10)
            {
                res.Msg = ErrorCodes.InvalidParam + "Sender Mobile Number";
                return res;
            }
            if (string.IsNullOrWhiteSpace(BeniID))
            {
                res.Msg = ErrorCodes.InvalidParam + "Beneficiary Id";
                return res;
            }
            ValidateAPIOutletResp _ValResp = GetOutletNew(dMTReq);
            if (_ValResp.Statuscode != ErrorCodes.One)
            {
                res.Msg = _ValResp.Msg;
                return res;
            }
            try
            {
                dMTReq.ApiID = _ValResp.APIID;
                dMTReq.TID = _ValResp.TransactionID;
                if (_ValResp.APICode == APICode.AMAH)
                {
                    var wareWL = new MobileWareML(_accessor, _env);
                    res = wareWL.DeleteBeneficiary(dMTReq, BeniID);
                }
                else if (_ValResp.APICode == APICode.MRUY)
                {
                    dMTReq.APIOutletID = _ValResp.DMTID;
                    var wareWL = new MrupayML(_accessor, _env);
                    res = wareWL.DeleteBeneficiary(dMTReq, BeniID);
                }
                else if (_ValResp.APICode == APICode.EKO)
                {
                    dMTReq.APIOutletID = _ValResp.DMTID;
                    var eKOML = new EKOML(_accessor, _env, _ValResp.APIID);
                    res = await eKOML.RemoveRecipient(dMTReq, BeniID).ConfigureAwait(false);
                }
                else if (_ValResp.APICode == APICode.CYBERPLATPayTM)
                {
                    IProcedure procRemoveBeneficiary = new ProcRemoveBeneficiary(_dal);
                    var beneRemoveReq = new CommonReq
                    {
                        CommonStr = BeniID
                    };
                    res = (ResponseStatus)procRemoveBeneficiary.Call(beneRemoveReq);
                }
                else if (_ValResp.APICode == APICode.CYBERPLAT)
                {
                    ICyberDMTMLIPay cyberIpay = new CyberPlateML(_dal, _ValResp.APIID);
                    if ((OTP ?? string.Empty).Length < 2)
                    {
                        res = cyberIpay.BeneficiaryDeleteIPay(dMTReq, BeniID);
                    }
                    else
                    {
                        res = cyberIpay.BeneficiaryDeleteValidateIPay(dMTReq, BeniID, OTP);
                    }

                }
                else
                {
                    res.Msg = ErrorCodes.Down;
                    return res;
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "CheckSender",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = dMTReq.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }
        public async Task<DMRTransactionResponse> Verification(DMTReq dMTReq, ReqSendMoney veri)
        {
            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = RechargeRespType._PENDING,
                LiveID = "",
                VendorID = "",
                TransactionID = "",
            };
            if (string.IsNullOrWhiteSpace(veri.AccountNo))
            {
                res.Statuscode = RechargeRespType.FAILED;
                res.Msg = ErrorCodes.InvalidParam + "AccountNo";
                return res;
            }
            if (string.IsNullOrWhiteSpace(veri.IFSC) || veri.IFSC.Length < 11)
            {
                res.Msg = ErrorCodes.InvalidParam + "IFSC";
                return res;
            }
            var _ValResp = GetOutletNew(dMTReq);
            if (_ValResp.Statuscode != ErrorCodes.One)
            {
                res.Statuscode = RechargeRespType.FAILED;
                res.Msg = _ValResp.Msg;
                return res;
            }
            if (_ValResp.APICode == APICode.MAHAGRAM)
            {
                if (veri.BankID < 1)
                {
                    res.Statuscode = RechargeRespType.FAILED;
                    res.Msg = ErrorCodes.InvalidParam + " BankID";
                    return res;
                }
            }
            try
            {
                dMTReq.ApiID = _ValResp.APIID;
                dMTReq.TID = _ValResp.TransactionID;
                _ValResp.OType = 15;
                var proRes = DoDMT(dMTReq, veri, _ValResp);
                res.ErrorCode = proRes.ErrorCode;
                res.TID = proRes.TID;
                res.TransactionID = proRes.TransactionID ?? string.Empty;
                res.Balance = proRes.Balance;
                if (proRes.Statuscode == ErrorCodes.Minus1)
                {
                    res.Statuscode = RechargeRespType.FAILED;
                    res.Msg = proRes.Msg;
                    res.ErrorCode = proRes.ErrorCode;
                    return res;
                }
                dMTReq.TID = res.TID.ToString();
                if (_ValResp.APICode == APICode.AMAH)
                {
                    var wareWL = new MobileWareML(_accessor, _env);
                    res = wareWL.Verification(dMTReq, veri, res);
                }
                else if (_ValResp.APICode == APICode.MRUY)
                {
                    dMTReq.APIOutletID = _ValResp.DMTID;
                    var wareWL = new MrupayML(_accessor, _env);
                    res = wareWL.Verification(dMTReq, veri, res);
                }
                else if (_ValResp.APICode == APICode.AIRTELBANK)
                {
                    var wareWL = new AirtelBankML(_accessor, _env);
                    veri.Amount = 1;
                    var req = new UserRequset
                    {
                        UserId = dMTReq.UserID
                    };
                    var uDetails = new UserDetail();
                    uDetails = userML.GetAppUserDetailByID(req);
                    dMTReq.UserMobileNo = uDetails.MobileNo;
                    dMTReq.StateID = uDetails.StateID;
                    res = await wareWL.SendMoney(dMTReq, veri, res).ConfigureAwait(false);
                }
                else if (_ValResp.APICode == APICode.EKO)
                {
                    dMTReq.APIOutletID = _ValResp.DMTID;
                    var eKOML = new EKOML(_accessor, _env, _ValResp.APIID);
                    res = await eKOML.AccountVerification(dMTReq, veri, res).ConfigureAwait(false);
                }
                else if (_ValResp.APICode == APICode.CYBERPLATPayTM)
                {
                    dMTReq.APIOutletID = _ValResp.DMTID;
                    var cyberPTM = new CyberPlateML(_dal, _ValResp.APIID);
                    res = cyberPTM.BeneAccountValidationPTM(dMTReq, veri, res);
                }
                else if (_ValResp.APICode == APICode.CYBERPLAT)
                {
                    ICyberDMTMLIPay cyberIpay = new CyberPlateML(_dal, _ValResp.APIID);
                    res = cyberIpay.BeneAccountValidationIPay(dMTReq, veri, res);
                }
                else if (_ValResp.APICode == APICode.MAHAGRAM)
                {
                    dMTReq.APIOutletID = _ValResp.DMTID;
                    var mahagram = new MahagramAPIML(_accessor, _env, _dal, _ValResp.APIID);
                    res = await mahagram.MGApiVerifybene(dMTReq, veri, res).ConfigureAwait(false);
                }
                else if (_ValResp.APICode == APICode.PAYTM)
                {
                    var paytmlML = new PaytmML(_accessor, _env, _ValResp.APIID);
                    res = await paytmlML.VerifyBeneficiary(dMTReq, veri, res).ConfigureAwait(false);
                }
                else if (_ValResp.APICode == APICode.OPENBANK)
                {
                    var openBank = new OpenBankML(_accessor, _env, _ValResp.APIID);
                    res = openBank.Verification(dMTReq, veri, res);
                }
                else
                {
                    res.Msg = ErrorCodes.Down;
                    return res;
                }
                new ProcUpdateDMRTransaction(_dal).Call(res);
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Account Verification",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = dMTReq.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            res.Request = string.Empty;
            res.Response = string.Empty;
            return res;
        }
        public TransactionDetail DMRReceipt(CommonReq req)
        {
            IProcedure procDMR = new ProcDMRTransaction(_dal);
            return (TransactionDetail)procDMR.Call(req);
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
                alertMl.OTPEmail(alertData);
                if (!string.IsNullOrEmpty(EmailID))
                    alertMl.OTPEmail(alertData);
            }
        }

    }
}
