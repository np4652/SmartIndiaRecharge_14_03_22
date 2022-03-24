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
using RoundpayFinTech.AppCode.Model.SDK;
using RoundpayFinTech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.StaticModel.MoneyTransfer;
using RoundpayFinTech.AppCode.ThirdParty.Fingpay;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Validators;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public class DeviceML : IDeviceML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly LoginResponse _lr;
        private readonly ISession _session;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        private readonly IUserML userML;
        private readonly IRequestInfo _rinfo;

        public DeviceML(IHttpContextAccessor accessor, IHostingEnvironment env, bool InSession = true)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _rinfo = new RequestInfo(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            if (InSession)
            {
                _session = _accessor.HttpContext.Session;
                _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
                userML = new UserML(_lr);
            }
        }
        public SDKVendorDetail APIOutletDetail(int InterfaceType, int OutletID)
        {
            var res = new SDKVendorDetail { };
            if (InterfaceType == AEPSInterfaceType.FINGPAY)
            {
                FingpayML fingpayML = new FingpayML(_dal);
                IAPISetting aPISetting = new OnboardingML(_accessor, _env);
                var fingPaySetting = aPISetting.GetFingpay();
                res.SuperMerchantID = fingPaySetting.superMerchantId;
                res.APIOutletID = string.Format("FP{0}", OutletID);
            }
            return res;
        }
        public async Task<BalanceEquiryResp> CheckBalance(string _pidData, string aadhar, string bank, int _InterfaceType, int OID, string IMEI, string Lattitude, string Longitude)
        {
            var response = new BalanceEquiryResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.DeviceNotFound
            };
            if (string.IsNullOrEmpty(_pidData))
            {
                return response;
            }
            //if (string.IsNullOrEmpty(Lattitude) || string.IsNullOrEmpty(Longitude))
            //{
            //    response.Msg = ErrorCodes.InvalidParam + " Lattitude and Longitude";
            //    return response;
            //}
            if (string.IsNullOrEmpty(aadhar))
            {
                response.Msg = ErrorCodes.InvalidParam + " Aadhar";
                return response;
            }
            if (string.IsNullOrEmpty(bank))
            {
                response.Msg = ErrorCodes.InvalidParam + " Bank IIN";
                return response;
            }
            int reqMode = RequestMode.PANEL;
            PidData pidData = new PidData();
            pidData = XMLHelper.O.DesrializeToObject(pidData, _pidData, "PidData", true);
            var usersDataForAEPS = ValidateUsersForAEPS(_lr.UserID, _lr.OutletID, _InterfaceType);
            if (usersDataForAEPS.Statuscode == ErrorCodes.One)
            {
                IMiniBankML miniBankML = new AEPSML(_accessor, _env, false);
                var aepsTranResp = await miniBankML.MakeMiniBankTransaction(new MiniBankTransactionServiceReq
                {
                    APIOpCode = SPKeys.AepsCashWithdrawal,
                    TXNType = "BE",
                    APICode = usersDataForAEPS.APICode,
                    LoginID = _lr.UserID,
                    OutletID = aadhar,
                    OutletIDSelf = _lr.OutletID,
                    RequestModeID = reqMode,
                    VendorID = Validate.O.MaskAadhar(aadhar),
                    BankIIN = bank,
                    OID = OID,
                    OpTypeID = OPTypes.AEPS
                }).ConfigureAwait(false);
                if (aepsTranResp.Statuscode == ErrorCodes.Minus1)
                {
                    response.Msg = aepsTranResp.Msg;
                    return response;
                }
                if (usersDataForAEPS.APICode == APICode.FINGPAY)
                {
                    FingpayML fingpayML = new FingpayML(_dal);
                    IAPISetting aPISetting = new OnboardingML(_accessor, _env);
                    var fingPaySetting = aPISetting.GetFingpay();
                    fingPaySetting.MERCHANTName = aepsTranResp.OutletName;
                    response = fingpayML.GetBalance(fingPaySetting, usersDataForAEPS, pidData, aadhar, bank, aepsTranResp.TransactionID, aepsTranResp.APIOutletID, IMEI, Lattitude, Longitude);
                }
                else if (usersDataForAEPS.APICode == APICode.SPRINT)
                {
                    SprintBBPSML sprintAEPS = new SprintBBPSML(_accessor, _env, _dal);
                    response = sprintAEPS.Enquiry(new AEPSUniversalRequest
                    {
                        AdharNo = aadhar,
                        BankIIN = bank,
                        IPAddress = _rinfo.GetRemoteIP(),
                        Lattitude = Lattitude,
                        Longitude = Longitude,
                        APIOpCode = aepsTranResp.BillOpCode,
                        APIOutletID = aepsTranResp.APIOutletID,
                        MobileNo = usersDataForAEPS.MobileNo,
                        PIDDATA = _pidData,
                        RequestMode = reqMode,
                        TID = aepsTranResp.TID,
                        TransactionID = aepsTranResp.TransactionID
                    });
                }
                else if (usersDataForAEPS.APICode == APICode.INSTANTPAY)
                {
                    InstantPayUserOnboarding instantPay = new InstantPayUserOnboarding(_accessor,_env, usersDataForAEPS.APICode, usersDataForAEPS.APIID,_dal);
                    response =instantPay.AEPSBalanceEnquiry(new AEPSUniversalRequest
                    {
                        AdharNo = aadhar,
                        BankIIN = bank,
                        IPAddress = _rinfo.GetRemoteIP(),
                        Lattitude = Lattitude,
                        Longitude = Longitude,
                        APIOpCode = aepsTranResp.BillOpCode,
                        APIOutletID = aepsTranResp.APIOutletID,
                        MobileNo = usersDataForAEPS.MobileNo,
                        PIDData= pidData,
                        RequestMode = reqMode,
                        TID = aepsTranResp.TID,
                        TransactionID = aepsTranResp.TransactionID
                    });
                }
                DeviceLog(usersDataForAEPS.APICode, "GetBalance", response.Req, response.Resp);
                response.Req = string.Empty;
                response.Resp = string.Empty;
            }
            return response;
        }
        public async Task<BalanceEquiryResp> CheckBalance(PidData pidData, string aadhar, string bank, int _InterfaceType, int UserID, int OutletID, int RMode, int OID, string IMEI, string Lattitude, string Longitude, string _PIDData)
        {
            var response = new BalanceEquiryResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.DeviceNotFound
            };
            if (pidData == null)
            {
                return response;
            }
            //if (string.IsNullOrEmpty(Lattitude) || string.IsNullOrEmpty(Longitude))
            //{
            //    response.Msg = ErrorCodes.InvalidParam + " Lattitude and Longitude";
            //    return response;
            //}
            if (string.IsNullOrEmpty(aadhar))
            {
                response.Msg = ErrorCodes.InvalidParam + " Aadhar";
                return response;
            }
            if (string.IsNullOrEmpty(bank))
            {
                response.Msg = ErrorCodes.InvalidParam + " Bank IIN";
                return response;
            }
            var usersDataForAEPS = ValidateUsersForAEPS(UserID, OutletID, _InterfaceType);
            if (usersDataForAEPS.Statuscode == ErrorCodes.One)
            {
                IMiniBankML miniBankML = new AEPSML(_accessor, _env, false);
                var aepsTranResp = await miniBankML.MakeMiniBankTransaction(new MiniBankTransactionServiceReq
                {
                    APIOpCode = SPKeys.AepsCashWithdrawal,
                    TXNType = "BE",
                    APICode = usersDataForAEPS.APICode,
                    LoginID = UserID,
                    OutletID = aadhar,
                    OutletIDSelf = OutletID,
                    RequestModeID = RMode,
                    VendorID = Validate.O.MaskAadhar(aadhar),
                    BankIIN = bank,
                    OID = OID,
                    OpTypeID = OPTypes.AEPS
                }).ConfigureAwait(false);
                if (aepsTranResp.Statuscode == ErrorCodes.Minus1)
                {
                    response.Msg = aepsTranResp.Msg;
                    return response;
                }
                if (usersDataForAEPS.APICode == APICode.FINGPAY)
                {
                    FingpayML fingpayML = new FingpayML(_dal);
                    IAPISetting aPISetting = new OnboardingML(_accessor, _env);
                    var fingPaySetting = aPISetting.GetFingpay();
                    fingPaySetting.MERCHANTName = aepsTranResp.OutletName;
                    response = fingpayML.GetBalance(fingPaySetting, usersDataForAEPS, pidData, aadhar, bank, aepsTranResp.TransactionID, aepsTranResp.APIOutletID, IMEI, Lattitude, Longitude);

                }
                else if (usersDataForAEPS.APICode == APICode.SPRINT)
                {
                    SprintBBPSML sprintAEPS = new SprintBBPSML(_accessor, _env, _dal);
                    response = sprintAEPS.Enquiry(new AEPSUniversalRequest
                    {
                        AdharNo = aadhar,
                        BankIIN = bank,
                        IPAddress = _rinfo.GetRemoteIP(),
                        Lattitude = Lattitude,
                        Longitude = Longitude,
                        APIOpCode = aepsTranResp.BillOpCode,
                        APIOutletID = aepsTranResp.APIOutletID,
                        MobileNo = usersDataForAEPS.MobileNo,
                        PIDDATA = _PIDData,
                        RequestMode = RMode,
                        TID = aepsTranResp.TID,
                        TransactionID = aepsTranResp.TransactionID
                    });
                }
                else if (usersDataForAEPS.APICode == APICode.INSTANTPAY)
                {
                    InstantPayUserOnboarding instantPay = new InstantPayUserOnboarding(_accessor, _env, usersDataForAEPS.APICode, usersDataForAEPS.APIID, _dal);
                    response = instantPay.AEPSBalanceEnquiry(new AEPSUniversalRequest
                    {
                        AdharNo = aadhar,
                        BankIIN = bank,
                        IPAddress = _rinfo.GetRemoteIP(),
                        Lattitude = Lattitude,
                        Longitude = Longitude,
                        APIOpCode = aepsTranResp.BillOpCode,
                        APIOutletID = aepsTranResp.APIOutletID,
                        MobileNo = usersDataForAEPS.MobileNo,
                        PIDData = pidData,
                        RequestMode = RMode,
                        TID = aepsTranResp.TID,
                        TransactionID = aepsTranResp.TransactionID
                    });
                }
                DeviceLog(usersDataForAEPS.APICode, "GetBalance", response.Req, response.Resp);
                response.Req = string.Empty;
                response.Resp = string.Empty;
            }
            return response;
        }
        public async Task<WithdrawlResponse> Withdrawl(string _pidData, string aadhar, string bank, int _InterfaceType, int Amount, int OID, string IMEI, string Lattitude, string Longitude)
        {
            var response = new WithdrawlResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.DeviceNotFound
            };
            if (string.IsNullOrEmpty(_pidData))
            {
                return response;
            }
            //if (string.IsNullOrEmpty(Lattitude) || string.IsNullOrEmpty(Longitude))
            //{
            //    response.Msg = ErrorCodes.InvalidParam + " Lattitude and Longitude";
            //    return response;
            //}
            if (string.IsNullOrEmpty(aadhar))
            {
                response.Msg = ErrorCodes.InvalidParam + " Aadhar";
                return response;
            }
            if (string.IsNullOrEmpty(bank))
            {
                response.Msg = ErrorCodes.InvalidParam + " Bank IIN";
                return response;
            }
            int RMode = RequestMode.PANEL;
            PidData pidData = new PidData();
            pidData = XMLHelper.O.DesrializeToObject(pidData, _pidData, "PidData", true);
            var usersDataForAEPS = ValidateUsersForAEPS(_lr.UserID, _lr.OutletID, _InterfaceType);
            if (usersDataForAEPS.Statuscode == ErrorCodes.One)
            {
                IMiniBankML miniBankML = new AEPSML(_accessor, _env, false);
                var aepsTranResp = await miniBankML.MakeMiniBankTransaction(new MiniBankTransactionServiceReq
                {
                    AmountR = Amount,
                    APIOpCode = SPKeys.AepsCashWithdrawal,
                    TXNType = "CW",
                    APICode = usersDataForAEPS.APICode,
                    LoginID = _lr.UserID,
                    OutletID = aadhar,
                    OutletIDSelf = _lr.OutletID,
                    RequestModeID = RMode,
                    VendorID = Validate.O.MaskAadhar(aadhar),
                    BankIIN = bank,
                    OID = OID,
                    OpTypeID = OPTypes.AEPS
                }).ConfigureAwait(false);
                if (aepsTranResp.Statuscode == ErrorCodes.Minus1)
                {
                    response.Msg = aepsTranResp.Msg;
                    return response;
                }
                if (usersDataForAEPS.APICode == APICode.FINGPAY)
                {
                    FingpayML fingpayML = new FingpayML(_dal);
                    IAPISetting aPISetting = new OnboardingML(_accessor, _env);
                    var fingPaySetting = aPISetting.GetFingpay();
                    fingPaySetting.MERCHANTName = aepsTranResp.OutletName;
                    response = fingpayML.Withdraw(fingPaySetting, usersDataForAEPS, pidData, aadhar, bank, Amount, aepsTranResp.TransactionID, aepsTranResp.APIOutletID, IMEI, Lattitude, Longitude);

                    response.TransactionID = aepsTranResp.TransactionID;
                }
                else if (usersDataForAEPS.APICode == APICode.SPRINT)
                {
                    SprintBBPSML sprintAEPS = new SprintBBPSML(_accessor, _env, _dal);
                    response = sprintAEPS.Withdrawal(new AEPSUniversalRequest
                    {
                        AdharNo = aadhar,
                        BankIIN = bank,
                        IPAddress = _rinfo.GetRemoteIP(),
                        Lattitude = Lattitude,
                        Longitude = Longitude,
                        APIOpCode = aepsTranResp.BillOpCode,
                        APIOutletID = aepsTranResp.APIOutletID,
                        MobileNo = usersDataForAEPS.MobileNo,
                        PIDDATA = _pidData,
                        RequestMode = RMode,
                        TID = aepsTranResp.TID,
                        TransactionID = aepsTranResp.TransactionID,
                        Amount = Amount
                    });
                    response.TransactionID = aepsTranResp.TransactionID;
                }
                else if (usersDataForAEPS.APICode == APICode.INSTANTPAY)
                {
                    InstantPayUserOnboarding instantPay = new InstantPayUserOnboarding(_accessor, _env, usersDataForAEPS.APICode, usersDataForAEPS.APIID, _dal);
                    response = instantPay.Withdrawal(new AEPSUniversalRequest
                    {
                        Amount= Amount,
                        AdharNo = aadhar,
                        BankIIN = bank,
                        IPAddress = _rinfo.GetRemoteIP(),
                        Lattitude = Lattitude,
                        Longitude = Longitude,
                        APIOpCode = aepsTranResp.BillOpCode,
                        APIOutletID = aepsTranResp.APIOutletID,
                        MobileNo = usersDataForAEPS.MobileNo,
                        PIDData = pidData,
                        RequestMode = RMode,
                        TID = aepsTranResp.TID,
                        TransactionID = aepsTranResp.TransactionID
                    });
                }
                DeviceLog(usersDataForAEPS.APICode, "Withdrawl", response.Req, response.Resp);
                if (response.Status.In(RechargeRespType.PENDING, RechargeRespType.SUCCESS, RechargeRespType.FAILED))
                {
                    var mbResp = miniBankML.UpdateMiniBankResponse(new MiniBankTransactionServiceResp
                    {
                        Amount = Amount,
                        TID = aepsTranResp.TID,
                        TransactionID = aepsTranResp.TransactionID,
                        VendorID = response.VendorID,
                        LT = 1,
                        LoginID = _lr.UserID,
                        LiveID = response.LiveID,
                        Statuscode = response.Status,
                        RequestPage = "SameSession",
                        Req = response.Req,
                        Resp = response.Resp,
                        BankBalance = response.Balance.ToString(),
                        CardNumber = Validate.O.MaskAadhar(aadhar)
                    });
                }
            }
            response.VendorID = string.Empty;
            response.Req = string.Empty;
            response.Resp = string.Empty;
            return response;
        }
        public async Task<WithdrawlResponse> Withdrawl(PidData pidData, string aadhar, string bank, int _InterfaceType, int Amount, int UserID, int OutletID, int RMode, int OID, string IMEI, string Lattitude, string Longitude, string _PIDData)
        {
            var response = new WithdrawlResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.DeviceNotFound
            };
            if (pidData == null)
            {
                return response;
            }

            //if (string.IsNullOrEmpty(Lattitude) || string.IsNullOrEmpty(Longitude))
            //{
            //    response.Msg = ErrorCodes.InvalidParam + " Lattitude and Longitude";
            //    return response;
            //}
            if (string.IsNullOrEmpty(aadhar))
            {
                response.Msg = ErrorCodes.InvalidParam + " Aadhar";
                return response;
            }
            if (string.IsNullOrEmpty(bank))
            {
                response.Msg = ErrorCodes.InvalidParam + " Bank IIN";
                return response;
            }
            var usersDataForAEPS = ValidateUsersForAEPS(UserID, OutletID, _InterfaceType);
            if (usersDataForAEPS.Statuscode == ErrorCodes.One)
            {
                IMiniBankML miniBankML = new AEPSML(_accessor, _env, false);
                var aepsTranResp = await miniBankML.MakeMiniBankTransaction(new MiniBankTransactionServiceReq
                {
                    AmountR = Amount,
                    APIOpCode = SPKeys.AepsCashWithdrawal,
                    TXNType = "CW",
                    APICode = usersDataForAEPS.APICode,
                    LoginID = UserID,
                    OutletID = aadhar,
                    OutletIDSelf = OutletID,
                    RequestModeID = RMode,
                    VendorID = Validate.O.MaskAadhar(aadhar),
                    BankIIN = bank,
                    OID = OID,
                    OpTypeID = OPTypes.AEPS
                }).ConfigureAwait(false);
                if (aepsTranResp.Statuscode == ErrorCodes.Minus1)
                {
                    response.Msg = aepsTranResp.Msg;
                    return response;
                }
                if (usersDataForAEPS.APICode == APICode.FINGPAY)
                {
                    FingpayML fingpayML = new FingpayML(_dal);
                    IAPISetting aPISetting = new OnboardingML(_accessor, _env);
                    var fingPaySetting = aPISetting.GetFingpay();
                    fingPaySetting.MERCHANTName = aepsTranResp.OutletName;
                    response = fingpayML.Withdraw(fingPaySetting, usersDataForAEPS, pidData, aadhar, bank, Amount, aepsTranResp.TransactionID, aepsTranResp.APIOutletID, IMEI, Lattitude, Longitude);

                    response.TransactionID = aepsTranResp.TransactionID;
                }
                else if (usersDataForAEPS.APICode == APICode.SPRINT)
                {
                    SprintBBPSML sprintAEPS = new SprintBBPSML(_accessor, _env, _dal);
                    response = sprintAEPS.Withdrawal(new AEPSUniversalRequest
                    {
                        AdharNo = aadhar,
                        BankIIN = bank,
                        IPAddress = _rinfo.GetRemoteIP(),
                        Lattitude = Lattitude,
                        Longitude = Longitude,
                        APIOpCode = aepsTranResp.BillOpCode,
                        APIOutletID = aepsTranResp.APIOutletID,
                        MobileNo = usersDataForAEPS.MobileNo,
                        PIDDATA = _PIDData,
                        RequestMode = RMode,
                        TID = aepsTranResp.TID,
                        TransactionID = aepsTranResp.TransactionID,
                        Amount = Amount
                    });
                    response.TransactionID = aepsTranResp.TransactionID;
                }
                else if (usersDataForAEPS.APICode == APICode.INSTANTPAY)
                {
                    InstantPayUserOnboarding instantPay = new InstantPayUserOnboarding(_accessor, _env, usersDataForAEPS.APICode, usersDataForAEPS.APIID, _dal);
                    response = instantPay.Withdrawal(new AEPSUniversalRequest
                    {
                        Amount = Amount,
                        AdharNo = aadhar,
                        BankIIN = bank,
                        IPAddress = _rinfo.GetRemoteIP(),
                        Lattitude = Lattitude,
                        Longitude = Longitude,
                        APIOpCode = aepsTranResp.BillOpCode,
                        APIOutletID = aepsTranResp.APIOutletID,
                        MobileNo = usersDataForAEPS.MobileNo,
                        PIDData = pidData,
                        RequestMode = RMode,
                        TID = aepsTranResp.TID,
                        TransactionID = aepsTranResp.TransactionID
                    });
                }
                DeviceLog(usersDataForAEPS.APICode, "Withdrawl", response.Req, response.Resp);
                if (response.Status.In(RechargeRespType.PENDING, RechargeRespType.SUCCESS, RechargeRespType.FAILED))
                {
                    var mbResp = miniBankML.UpdateMiniBankResponse(new MiniBankTransactionServiceResp
                    {
                        Amount = Amount,
                        TID = aepsTranResp.TID,
                        TransactionID = aepsTranResp.TransactionID,
                        VendorID = response.VendorID,
                        LT = 1,
                        LoginID = UserID,
                        LiveID = response.LiveID,
                        Statuscode = response.Status,
                        RequestPage = "SameSession",
                        Req = response.Req,
                        Resp = response.Resp,
                        BankBalance = response.Balance.ToString(),
                        CardNumber = Validate.O.MaskAadhar(aadhar)
                    });
                }
            }
            response.VendorID = string.Empty;
            response.Req = string.Empty;
            response.Resp = string.Empty;
            return response;
        }
        public async Task<WithdrawlResponse> Aadharpay(PidData pidData, string aadhar, string bank, int _InterfaceType, int Amount, int UserID, int OutletID, int RMode, int OID, string IMEI, string Lattitude, string Longitude, string _PIDData)
        {
            var response = new WithdrawlResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.DeviceNotFound
            };
            if (pidData == null)
            {
                return response;
            }
            //if (string.IsNullOrEmpty(Lattitude) || string.IsNullOrEmpty(Longitude))
            //{
            //    response.Msg = ErrorCodes.InvalidParam + " Lattitude and Longitude";
            //    return response;
            //}
            if (string.IsNullOrEmpty(aadhar))
            {
                response.Msg = ErrorCodes.InvalidParam + " Aadhar";
                return response;
            }
            if (string.IsNullOrEmpty(bank))
            {
                response.Msg = ErrorCodes.InvalidParam + " Bank IIN";
                return response;
            }
            var usersDataForAEPS = ValidateUsersForAEPS(UserID, OutletID, _InterfaceType);
            if (usersDataForAEPS.Statuscode == ErrorCodes.One)
            {
                IMiniBankML miniBankML = new AEPSML(_accessor, _env, false);
                var aepsTranResp = await miniBankML.MakeMiniBankTransaction(new MiniBankTransactionServiceReq
                {
                    AmountR = Amount,
                    APIOpCode = SPKeys.Aadharpay,
                    TXNType = "CW",
                    APICode = usersDataForAEPS.APICode,
                    LoginID = UserID,
                    OutletID = aadhar,
                    OutletIDSelf = OutletID,
                    RequestModeID = RMode,
                    VendorID = Validate.O.MaskAadhar(aadhar),
                    BankIIN = bank,
                    OID = OID,
                    OpTypeID = OPTypes.AEPS
                }).ConfigureAwait(false);
                if (aepsTranResp.Statuscode == ErrorCodes.Minus1)
                {
                    response.Msg = aepsTranResp.Msg;
                    return response;
                }
                if (usersDataForAEPS.APICode == APICode.FINGPAY)
                {
                    FingpayML fingpayML = new FingpayML(_dal);
                    IAPISetting aPISetting = new OnboardingML(_accessor, _env);
                    var fingPaySetting = aPISetting.GetFingpay();
                    fingPaySetting.MERCHANTName = aepsTranResp.OutletName;
                    response = fingpayML.AadharPay(fingPaySetting, usersDataForAEPS, pidData, aadhar, bank, Amount, aepsTranResp.TransactionID, aepsTranResp.APIOutletID, IMEI, Lattitude, Longitude);
                    response.TransactionID = aepsTranResp.TransactionID;
                }
                else if (usersDataForAEPS.APICode == APICode.SPRINT)
                {
                    SprintBBPSML sprintAEPS = new SprintBBPSML(_accessor, _env, _dal);
                    response = sprintAEPS.AadharPay(new AEPSUniversalRequest
                    {
                        AdharNo = aadhar,
                        BankIIN = bank,
                        IPAddress = _rinfo.GetRemoteIP(),
                        Lattitude = Lattitude,
                        Longitude = Longitude,
                        APIOpCode = aepsTranResp.BillOpCode,
                        APIOutletID = aepsTranResp.APIOutletID,
                        MobileNo = usersDataForAEPS.MobileNo,
                        PIDDATA = _PIDData,
                        RequestMode = RMode,
                        TID = aepsTranResp.TID,
                        TransactionID = aepsTranResp.TransactionID,
                        Amount = Amount
                    });
                    response.TransactionID = aepsTranResp.TransactionID;
                }
                else if (usersDataForAEPS.APICode == APICode.INSTANTPAY)
                {
                    InstantPayUserOnboarding instantPay = new InstantPayUserOnboarding(_accessor, _env, usersDataForAEPS.APICode, usersDataForAEPS.APIID, _dal);
                    response = instantPay.AadharPay(new AEPSUniversalRequest
                    {
                        Amount = Amount,
                        AdharNo = aadhar,
                        BankIIN = bank,
                        IPAddress = _rinfo.GetRemoteIP(),
                        Lattitude = Lattitude,
                        Longitude = Longitude,
                        APIOpCode = aepsTranResp.BillOpCode,
                        APIOutletID = aepsTranResp.APIOutletID,
                        MobileNo = usersDataForAEPS.MobileNo,
                        PIDData = pidData,
                        RequestMode = RMode,
                        TID = aepsTranResp.TID,
                        TransactionID = aepsTranResp.TransactionID
                    });
                }
                DeviceLog(usersDataForAEPS.APICode, "Aadharpay", response.Req, response.Resp);
                if (response.Status.In(RechargeRespType.PENDING, RechargeRespType.SUCCESS, RechargeRespType.FAILED))
                {
                    var mbResp = miniBankML.UpdateMiniBankResponse(new MiniBankTransactionServiceResp
                    {
                        Amount = Amount,
                        TID = aepsTranResp.TID,
                        TransactionID = aepsTranResp.TransactionID,
                        VendorID = response.VendorID,
                        LT = 1,
                        LoginID = UserID,
                        LiveID = response.LiveID,
                        Statuscode = response.Status,
                        RequestPage = "SameSession",
                        Req = response.Req,
                        Resp = response.Resp,
                        BankBalance = response.Balance.ToString(),
                        CardNumber = Validate.O.MaskAadhar(aadhar)
                    });
                }
            }
            response.VendorID = string.Empty;
            response.Req = string.Empty;
            response.Resp = string.Empty;
            return response;
        }
        public async Task<WithdrawlResponse> Aadharpay(string _pidData, string aadhar, string bank, int _InterfaceType, int Amount, int OID, string IMEI, string Lattitude, string Longitude)
        {
            var response = new WithdrawlResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.DeviceNotFound
            };
            if (string.IsNullOrEmpty(_pidData))
            {
                return response;
            }
            //if (string.IsNullOrEmpty(Lattitude) || string.IsNullOrEmpty(Longitude))
            //{
            //    response.Msg = ErrorCodes.InvalidParam + " Lattitude and Longitude";
            //    return response;
            //}
            if (string.IsNullOrEmpty(aadhar))
            {
                response.Msg = ErrorCodes.InvalidParam + " Aadhar";
                return response;
            }
            if (string.IsNullOrEmpty(bank))
            {
                response.Msg = ErrorCodes.InvalidParam + " Bank IIN";
                return response;
            }
            PidData pidData = new PidData();
            pidData = XMLHelper.O.DesrializeToObject(pidData, _pidData, "PidData", true);
            int RMode = RequestMode.PANEL;
            var usersDataForAEPS = ValidateUsersForAEPS(_lr.UserID, _lr.OutletID, _InterfaceType);
            if (usersDataForAEPS.Statuscode == ErrorCodes.One)
            {
                IMiniBankML miniBankML = new AEPSML(_accessor, _env, false);
                var aepsTranResp = await miniBankML.MakeMiniBankTransaction(new MiniBankTransactionServiceReq
                {
                    AmountR = Amount,
                    APIOpCode = SPKeys.Aadharpay,
                    TXNType = "CW",
                    APICode = usersDataForAEPS.APICode,
                    LoginID = _lr.UserID,
                    OutletID = aadhar,
                    OutletIDSelf = _lr.OutletID,
                    RequestModeID = RMode,
                    VendorID = Validate.O.MaskAadhar(aadhar),
                    BankIIN = bank,
                    OID = OID,
                    OpTypeID = OPTypes.AEPS
                }).ConfigureAwait(false);
                if (aepsTranResp.Statuscode == ErrorCodes.Minus1)
                {
                    response.Msg = aepsTranResp.Msg;
                    return response;
                }
                if (usersDataForAEPS.APICode == APICode.FINGPAY)
                {
                    FingpayML fingpayML = new FingpayML(_dal);
                    IAPISetting aPISetting = new OnboardingML(_accessor, _env);
                    var fingPaySetting = aPISetting.GetFingpay();
                    fingPaySetting.MERCHANTName = aepsTranResp.OutletName;
                    response = fingpayML.AadharPay(fingPaySetting, usersDataForAEPS, pidData, aadhar, bank, Amount, aepsTranResp.TransactionID, aepsTranResp.APIOutletID, IMEI, Lattitude, Longitude);
                    response.TransactionID = aepsTranResp.TransactionID;
                }
                else if (usersDataForAEPS.APICode == APICode.SPRINT)
                {
                    SprintBBPSML sprintAEPS = new SprintBBPSML(_accessor, _env, _dal);
                    response = sprintAEPS.AadharPay(new AEPSUniversalRequest
                    {
                        AdharNo = aadhar,
                        BankIIN = bank,
                        IPAddress = _rinfo.GetRemoteIP(),
                        Lattitude = Lattitude,
                        Longitude = Longitude,
                        APIOpCode = aepsTranResp.BillOpCode,
                        APIOutletID = aepsTranResp.APIOutletID,
                        MobileNo = usersDataForAEPS.MobileNo,
                        PIDDATA = _pidData,
                        RequestMode = RMode,
                        TID = aepsTranResp.TID,
                        TransactionID = aepsTranResp.TransactionID,
                        Amount = Amount
                    });
                    response.TransactionID = aepsTranResp.TransactionID;
                }
                else if (usersDataForAEPS.APICode == APICode.INSTANTPAY)
                {
                    InstantPayUserOnboarding instantPay = new InstantPayUserOnboarding(_accessor, _env, usersDataForAEPS.APICode, usersDataForAEPS.APIID, _dal);
                    response = instantPay.AadharPay(new AEPSUniversalRequest
                    {
                        Amount = Amount,
                        AdharNo = aadhar,
                        BankIIN = bank,
                        IPAddress = _rinfo.GetRemoteIP(),
                        Lattitude = Lattitude,
                        Longitude = Longitude,
                        APIOpCode = aepsTranResp.BillOpCode,
                        APIOutletID = aepsTranResp.APIOutletID,
                        MobileNo = usersDataForAEPS.MobileNo,
                        PIDData = pidData,
                        RequestMode = RMode,
                        TID = aepsTranResp.TID,
                        TransactionID = aepsTranResp.TransactionID
                    });
                }
                DeviceLog(usersDataForAEPS.APICode, "Aadharpay", response.Req, response.Resp);
                if (response.Status.In(RechargeRespType.PENDING, RechargeRespType.SUCCESS, RechargeRespType.FAILED))
                {
                    var mbResp = miniBankML.UpdateMiniBankResponse(new MiniBankTransactionServiceResp
                    {
                        Amount = Amount,
                        TID = aepsTranResp.TID,
                        TransactionID = aepsTranResp.TransactionID,
                        VendorID = response.VendorID,
                        LT = 1,
                        LoginID = _lr.UserID,
                        LiveID = response.LiveID,
                        Statuscode = response.Status,
                        RequestPage = "SameSession",
                        Req = response.Req,
                        Resp = response.Resp,
                        BankBalance = response.Balance.ToString(),
                        CardNumber = Validate.O.MaskAadhar(aadhar)
                    });
                }
            }
            response.VendorID = string.Empty;
            response.Req = string.Empty;
            response.Resp = string.Empty;
            return response;
        }
        public MiniStatementResponse MiniStatement(PidData pidData, string aadhar, string Bank, string BankIIN, int _InterfaceType, int UserID, int OutletID, int RMode, int OID, string SPKey, string IMEI, string Lattitude, string Longitude, string _PIDData)
        {
            var response = new MiniStatementResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.DeviceNotFound
            };
            if (pidData == null)
            {
                return response;
            }

            //if (string.IsNullOrEmpty(Lattitude) || string.IsNullOrEmpty(Longitude))
            //{
            //    response.Msg = ErrorCodes.InvalidParam + " Lattitude and Longitude";
            //    return response;
            //}
            if (string.IsNullOrEmpty(aadhar))
            {
                response.Msg = ErrorCodes.InvalidParam + " Aadhar";
                return response;
            }
            if (string.IsNullOrEmpty(BankIIN))
            {
                response.Msg = ErrorCodes.InvalidParam + " Bank IIN";
                return response;
            }
            var usersDataForAEPS = ValidateUsersForAEPS(UserID, OutletID, _InterfaceType);
            if (usersDataForAEPS.Statuscode == ErrorCodes.One)
            {
                var aEPSML = new AEPSML(_accessor, _env, false);
                var aepsTranResp = aEPSML.MakeAEPSMiniStmtTransaction(new InititateMiniStatementTransactionRequest
                {
                    UserID = UserID,
                    OutletID = OutletID,
                    OID = OID,
                    BankName = Bank,
                    BankIIN = BankIIN,
                    APICode = usersDataForAEPS.APICode,
                    RequestModeID = RMode,
                    AccountNo = Validate.O.MaskAadhar(aadhar),
                    SPKey = SPKey
                });
                if (aepsTranResp.Statuscode == ErrorCodes.Minus1)
                {
                    response.Msg = aepsTranResp.Msg;
                    return response;
                }
                if (usersDataForAEPS.APICode == APICode.FINGPAY)
                {
                    FingpayML fingpayML = new FingpayML(_dal);
                    IAPISetting aPISetting = new OnboardingML(_accessor, _env);
                    var fingPaySetting = aPISetting.GetFingpay();
                    fingPaySetting.MERCHANTName = aepsTranResp.OutletName;
                    response = fingpayML.MiniStatement(fingPaySetting, usersDataForAEPS, pidData, aadhar, BankIIN, aepsTranResp.TransactionID, aepsTranResp.APIOutletID, IMEI, Lattitude, Longitude);
                }
                else if (usersDataForAEPS.APICode == APICode.SPRINT)
                {
                    SprintBBPSML sprintAEPS = new SprintBBPSML(_accessor, _env, _dal);
                    response = sprintAEPS.MiniStatement(new AEPSUniversalRequest
                    {
                        AdharNo = aadhar,
                        BankIIN = BankIIN,
                        IPAddress = _rinfo.GetRemoteIP(),
                        Lattitude = Lattitude,
                        Longitude = Longitude,
                        APIOpCode = aepsTranResp.BillOpCode,
                        APIOutletID = aepsTranResp.APIOutletID,
                        MobileNo = usersDataForAEPS.MobileNo,
                        PIDDATA = _PIDData,
                        RequestMode = RMode,
                        TID = aepsTranResp.TID,
                        TransactionID = aepsTranResp.TransactionID
                    });
                }
                else if (usersDataForAEPS.APICode == APICode.INSTANTPAY)
                {
                    InstantPayUserOnboarding instantPay = new InstantPayUserOnboarding(_accessor, _env, usersDataForAEPS.APICode, usersDataForAEPS.APIID, _dal);
                    response = instantPay.MiniStatement(new AEPSUniversalRequest
                    {
                        AdharNo = aadhar,
                        BankIIN = BankIIN,
                        IPAddress = _rinfo.GetRemoteIP(),
                        Lattitude = Lattitude,
                        Longitude = Longitude,
                        APIOpCode = aepsTranResp.BillOpCode,
                        APIOutletID = aepsTranResp.APIOutletID,
                        MobileNo = usersDataForAEPS.MobileNo,
                        PIDData = pidData,
                        RequestMode = RMode,
                        TID = aepsTranResp.TID,
                        TransactionID = aepsTranResp.TransactionID
                    });
                }
                DeviceLog(usersDataForAEPS.APICode, "MiniStatement", response.Req, response.Resp);
                if (response.Status.In(RechargeRespType.SUCCESS, RechargeRespType.FAILED))
                {
                    var mbResp = aEPSML.UpdateMiniBankResponse(new MiniBankTransactionServiceResp
                    {
                        TID = aepsTranResp.TID,
                        TransactionID = aepsTranResp.TransactionID,
                        VendorID = response.VendorID,
                        LT = 1,
                        LoginID = UserID,
                        LiveID = response.LiveID,
                        Statuscode = response.Status,
                        RequestPage = "SameSession",
                        Req = response.Req,
                        Resp = response.Resp,
                        BankBalance = response.Balance.ToString(),
                        CardNumber = Validate.O.MaskAadhar(aadhar)
                    });
                }
                response.Statuscode = response.Status == RechargeRespType.FAILED ? ErrorCodes.Minus1 : response.Statuscode;
                response.Req = string.Empty;
                response.Resp = string.Empty;
            }
            return response;
        }

        public async Task<DepositResponse> DepositGenerateOTP(DepositRequest depositRequest)
        {
            var response = new DepositResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var usersDataForAEPS = ValidateUsersForAEPS(depositRequest.UserID, depositRequest.OutletID, depositRequest.InterfaceType);
            if (usersDataForAEPS.Statuscode == ErrorCodes.One)
            {
                if (usersDataForAEPS.APICode == APICode.FINGPAY)
                {
                    FingpayML fingpayML = new FingpayML(_dal);
                    IAPISetting aPISetting = new OnboardingML(_accessor, _env);
                    var fingPaySetting = aPISetting.GetFingpay();
                    response = fingpayML.GenerateDepositOTP(fingPaySetting, usersDataForAEPS, depositRequest.AccountNo, depositRequest.IIN, depositRequest.Amount, usersDataForAEPS.TransactionID, usersDataForAEPS.APIOutletID, depositRequest.IMEI, depositRequest.Lattitude, depositRequest.Longitude);
                    DeviceLog(APICode.FINGPAY, "DepositGenerateOTP", response.Req, response.Resp);
                    response.TransactionID = usersDataForAEPS.TransactionID;
                }
            }
            response.Req = string.Empty;
            response.Resp = string.Empty;
            return response;
        }
        public async Task<DepositResponse> DepositVerifyOTP(DepositRequest depositRequest)
        {
            var response = new DepositResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var usersDataForAEPS = ValidateUsersForAEPS(depositRequest.UserID, depositRequest.OutletID, depositRequest.InterfaceType);
            if (usersDataForAEPS.Statuscode == ErrorCodes.One)
            {
                if (usersDataForAEPS.APICode == APICode.FINGPAY)
                {
                    FingpayML fingpayML = new FingpayML(_dal);
                    IAPISetting aPISetting = new OnboardingML(_accessor, _env);
                    var fingPaySetting = aPISetting.GetFingpay();
                    response = fingpayML.ValidateDepositOTP(fingPaySetting, usersDataForAEPS, depositRequest.AccountNo, depositRequest.IIN, depositRequest.Amount, depositRequest.Reff1, usersDataForAEPS.APIOutletID, depositRequest.OTP, depositRequest.Reff2, depositRequest.Reff3, depositRequest.IMEI, depositRequest.Lattitude, depositRequest.Longitude);
                    DeviceLog(APICode.FINGPAY, "DepositGenerateVerify", response.Req, response.Resp);
                    response.TransactionID = usersDataForAEPS.TransactionID;
                }
            }
            response.Req = string.Empty;
            response.Resp = string.Empty;
            return response;
        }
        public async Task<DepositResponse> DepositAccount(DepositRequest depositRequest)
        {
            var response = new DepositResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var usersDataForAEPS = ValidateUsersForAEPS(depositRequest.UserID, depositRequest.OutletID, depositRequest.InterfaceType);
            if (usersDataForAEPS.Statuscode == ErrorCodes.One)
            {
                IMiniBankML miniBankML = new AEPSML(_accessor, _env, false);
                var aepsTranResp = await miniBankML.MakeMiniBankTransaction(new MiniBankTransactionServiceReq
                {
                    AmountR = depositRequest.Amount,
                    APIOpCode = SPKeys.AepsCashDeposit,
                    TXNType = "DP",
                    APICode = usersDataForAEPS.APICode,
                    LoginID = depositRequest.UserID,
                    OutletID = string.Empty,
                    OutletIDSelf = depositRequest.OutletID,
                    RequestModeID = depositRequest.RMode,
                    VendorID = response.VendorID,
                    BankIIN = depositRequest.IIN,
                    OID = depositRequest.OID,
                    OpTypeID = OPTypes.AEPS,
                    AccountNo = depositRequest.AccountNo,
                    TransactionID = depositRequest.Reff1
                }).ConfigureAwait(false);
                if (aepsTranResp.Statuscode == ErrorCodes.Minus1)
                {
                    response.Msg = aepsTranResp.Msg;
                    return response;
                }
                if (usersDataForAEPS.APICode == APICode.FINGPAY)
                {
                    FingpayML fingpayML = new FingpayML(_dal);
                    IAPISetting aPISetting = new OnboardingML(_accessor, _env);
                    var fingPaySetting = aPISetting.GetFingpay();
                    fingPaySetting.MERCHANTName = aepsTranResp.OutletName;
                    response = fingpayML.DepositTransaction(fingPaySetting, usersDataForAEPS, depositRequest.AccountNo, depositRequest.IIN, depositRequest.Amount, depositRequest.Reff1, usersDataForAEPS.APIOutletID, depositRequest.OTP, depositRequest.Reff2, depositRequest.Reff3, depositRequest.IMEI, depositRequest.Lattitude, depositRequest.Longitude);
                    DeviceLog(APICode.FINGPAY, "DepositAccount", response.Req, response.Resp);
                    response.TransactionID = aepsTranResp.TransactionID;
                }
                if (response.Status.In(RechargeRespType.PENDING, RechargeRespType.SUCCESS, RechargeRespType.FAILED))
                {
                    var mbResp = miniBankML.UpdateMiniBankResponse(new MiniBankTransactionServiceResp
                    {
                        Amount = depositRequest.Amount,
                        TID = aepsTranResp.TID,
                        TransactionID = aepsTranResp.TransactionID,
                        VendorID = response.VendorID,
                        LT = 1,
                        LoginID = depositRequest.UserID,
                        LiveID = response.LiveID,
                        Statuscode = response.Status,
                        RequestPage = "SameSession",
                        Req = response.Req,
                        Resp = response.Resp,
                        BankBalance = response.Balance.ToString(),
                        CardNumber = depositRequest.AccountNo
                    });
                }
            }
            response.Req = string.Empty;
            response.Resp = string.Empty;
            return response;
        }


        public List<DeviceMaster> DeviceMasters()
        {
            var deviceMasters = new List<DeviceMaster>();
            if (_lr.LoginTypeID == LoginType.ApplicationUser)
            {
                var commonReq = new CommonReq
                {
                    LoginID = _lr.UserID,
                    LoginTypeID = _lr.LoginTypeID,
                    CommonInt = 0
                };
                IProcedure _proc = new ProcGetDeviceMaster(_dal);
                deviceMasters = (List<DeviceMaster>)_proc.Call(commonReq);
            }
            return deviceMasters;
        }
        public bool DeviceSave(DeviceMaster obj)
        {
            bool resp = false;
            if (_lr.LoginTypeID == LoginType.ApplicationUser)
            {
                var commonReq = new CommonReq
                {
                    LoginID = _lr.UserID,
                    LoginTypeID = _lr.LoginTypeID,
                    CommonInt = 0,
                    CommonStr = obj.DeviceName,
                    CommonInt2 = obj.ID
                };
                IProcedure _proc = new ProcUpdateUserDeviceID(_dal);
                resp = (bool)_proc.Call(commonReq);
            }
            return resp;
        }
        private UserDataForAEPS ValidateUsersForAEPS(int UserID, int OutletID, int InterfaceType)
        {
            string _APICode = string.Empty;
            if (InterfaceType == AEPSInterfaceType.FINGPAY)
            {
                _APICode = APICode.FINGPAY;
            }
            else if (InterfaceType == AEPSInterfaceType.SPRINT)
            {
                _APICode = APICode.SPRINT;
            }
            else if (InterfaceType == AEPSInterfaceType.INSTANTPAY) {
                _APICode = APICode.INSTANTPAY;
            }
            IProcedure proc = new ProcGetUsersDataForAEPS(_dal);
            return (UserDataForAEPS)proc.Call(new CommonReq
            {
                LoginID = UserID,
                CommonInt = OutletID,
                CommonStr = _APICode
            });
        }
        private void DeviceLog(string apiCode, string Method, string Req, string Rsp)
        {
            IAEPSControllerHelper aepsML = new AEPSML(_accessor, _env, false);
            aepsML.SaveAEPSLog(apiCode, Method, Req, Rsp);
        }

        public List<MasterVendorModel> VendorMaster(MasterVendorModel req)
        {
            var res = new List<MasterVendorModel>();
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin)
            {
                req.LoginID = _lr.UserID;
                req.LoginTypeID = _lr.LoginTypeID;
                IProcedure _proc = new ProcGetVendorMaster(_dal);
                res = (List<MasterVendorModel>)_proc.Call(req);
            }
            return res;
        }

        public IResponseStatus VendorMasterCU(MasterVendorModel req)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin)
            {
                req.LoginID = _lr.UserID;
                req.LoginTypeID = _lr.LoginTypeID;
                IProcedure _proc = new ProcVendorMasterCU(_dal);
                res = (ResponseStatus)_proc.Call(req);
            }
            return res;
        }

        public IResponseStatus VendorMasterToggle(MasterVendorModel req)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin)
            {
                req.LoginID = _lr.UserID;
                req.LoginTypeID = _lr.LoginTypeID;
                IProcedure _proc = new ProcVendorMasterToggle(_dal);
                res = (ResponseStatus)_proc.Call(req);
            }
            return res;
        }

        public List<MasterDeviceModel> DeviceModelMaster(MasterDeviceModel req)
        {
            var res = new List<MasterDeviceModel>();
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin)
            {
                req.LoginID = _lr.UserID;
                req.LoginTypeID = _lr.LoginTypeID;
                IProcedure _proc = new ProcDeviceModelMaster(_dal);
                res = (List<MasterDeviceModel>)_proc.Call(req);
            }
            return res;
        }

        public IResponseStatus DeviceModelMasterCU(MasterDeviceModel req)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin)
            {
                req.LoginID = _lr.UserID;
                req.LoginTypeID = _lr.LoginTypeID;
                IProcedure _proc = new ProcDeviceModelMasterCU(_dal);
                res = (ResponseStatus)_proc.Call(req);
            }
            return res;
        }

        public IResponseStatus DeviceModelMasterToggle(MasterDeviceModel req)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin)
            {
                req.LoginID = _lr.UserID;
                req.LoginTypeID = _lr.LoginTypeID;
                IProcedure _proc = new ProcDeviceModelMasterToggle(_dal);
                res = (ResponseStatus)_proc.Call(req);
            }
            return res;
        }

        public VendorBindOperators VendorOperatorAllocation(int id)
        {
            var res = new VendorBindOperators();
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin)
            {
                IProcedure _proc = new ProcGetVendorOperators(_dal);
                res = (VendorBindOperators)_proc.Call(id);
            }
            return res;
        }

        public IResponseStatus VendorOperatorUpdate(VendorBindOperators req)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin)
            {
                IProcedure _proc = new ProcVendorMasterOperatorCU(_dal);
                res = (ResponseStatus)_proc.Call(req);
            }
            return res;
        }

        public IEnumerable<MasterVendorModel> VendorDDl()
        {
            List<MasterVendorModel> lst = new List<MasterVendorModel>();
            if (_lr.LoginTypeID == LoginType.ApplicationUser)
            {
                MasterVendorModel req = new MasterVendorModel
                {
                    LoginID = _lr.UserID,
                    LoginTypeID = _lr.LoginTypeID
                };
                IProcedure _proc = new ProcGetVendorMaster(_dal);
                lst = (List<MasterVendorModel>)_proc.Call(req);
                MasterVendorModel defaultItem = new MasterVendorModel
                {
                    ID = 0,
                    VendorName = ":: Select Vendor ::"
                };
                lst.Insert(0, defaultItem);
            }
            return lst;
        }

        public List<MPosDeviceInventoryModel> MPosDevice(MPosDeviceInventoryModel req)
        {
            IUserML userML = new UserML(_accessor, _env);
            var res = new List<MPosDeviceInventoryModel>();
            if ((_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin) || userML.IsCustomerCareAuthorised(ActionCodes.ShowMPOSDInventory))
            {
                req.LoginID = _lr.UserID;
                req.LoginTypeID = _lr.LoginTypeID;
                IProcedure _proc = new ProcMPosDevice(_dal);
                res = (List<MPosDeviceInventoryModel>)_proc.Call(req);
            }
            return res;
        }

        public IResponseStatus MPosDeviceCU(MPosDeviceInventoryModel req)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin)
            {
                req.LoginID = _lr.UserID;
                req.LoginTypeID = _lr.LoginTypeID;
                IProcedure _proc = new ProcMPosDeviceCU(_dal);
                res = (ResponseStatus)_proc.Call(req);
            }
            return res;
        }

        public IEnumerable<MasterDeviceModel> DeviceModelDDl(int vendorId)
        {
            List<MasterDeviceModel> lst = new List<MasterDeviceModel>();
            if (_lr.LoginTypeID == LoginType.ApplicationUser)
            {
                IProcedure _proc = new ProcDeviceModelDdl(_dal);
                lst = (List<MasterDeviceModel>)_proc.Call(vendorId);
                MasterDeviceModel defaultItem = new MasterDeviceModel
                {
                    ID = 0,
                    ModelName = ":: Select Device ::"
                };
                lst.Insert(0, defaultItem);
            }
            return lst;
        }

        public IResponseStatus MPosDeviceAssignMap(MPosDeviceInventoryModel req, bool IsMap = false)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin)
            {
                if (IsMap) { req.UserId = 0; req.OutletId = int.Parse(Validate.O.LoginID(req.UserName)); }
                else { req.OutletId = 0; req.UserId = int.Parse(Validate.O.LoginID(req.UserName)); }
                req.Browser = _rinfo.GetBrowser();
                req.IPAddress = _rinfo.GetRemoteIP();
                req.LoginID = _lr.UserID;
                req.LoginTypeID = _lr.LoginTypeID;
                IProcedure _proc = new ProcMPosDeviceAssignment(_dal);
                res = (ResponseStatus)_proc.Call(req);
            }
            return res;
        }

        public IResponseStatus MPosDeviceUnAssignUnMap(MPosDeviceInventoryModel req)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin)
            {
                req.LoginID = _lr.UserID;
                req.LoginTypeID = _lr.LoginTypeID;
                IProcedure _proc = new ProcMPosDeviceUnAssignUnMap(_dal);
                res = (ResponseStatus)_proc.Call(req);
            }
            return res;
        }

        public IResponseStatus MPosToggle(MPosDeviceInventoryModel req)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin)
            {
                req.LoginID = _lr.UserID;
                req.LoginTypeID = _lr.LoginTypeID;
                IProcedure _proc = new ProcMPosDeviceToggle(_dal);
                res = (ResponseStatus)_proc.Call(req);
            }
            return res;
        }
    }

}
