using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Recharge;
using RoundpayFinTech.AppCode.Model.SDK;
using RoundpayFinTech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.StaticModel.MoneyTransfer;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Validators;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public class SDKML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        private readonly IRequestInfo _info;
        public SDKML(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            _info = new RequestInfo(_accessor, _env);
        }
        public SDKDetailResponse GetSDKDetail(SDKRequest sDKRequest)
        {
            var sdkResponse = new SDKDetailResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            #region CodeValidation
            if (sDKRequest.UserID < 1)
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " UserID";
                return sdkResponse;
            }
            if (string.IsNullOrEmpty(sDKRequest.PIN))
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " PIN";
                return sdkResponse;
            }
            if (string.IsNullOrEmpty(sDKRequest.SPKey))
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " SPKey";
                return sdkResponse;
            }
            if (sDKRequest.OutletID < 10000)
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " OutletID";
                return sdkResponse;
            }
            #endregion
            #region ValidationFromDB
            IProcedure procValidate = new ProcValidateSDKLoginRequest(_dal);
            var procres = (SDKResponse)procValidate.Call(sDKRequest);
            sdkResponse.Statuscode = procres.Statuscode;
            sdkResponse.Msg = procres.Msg;
            if (sdkResponse.Statuscode == ErrorCodes.One)
            {
                BBPSML bbpsML = new BBPSML(_accessor, _env, false);
                var onboadresp = bbpsML.CheckOutletStatusForServices(new CheckOutletStatusReqModel
                {
                    LoginTypeID = LoginType.ApplicationUser,
                    LoginID = sDKRequest.UserID ?? 0,
                    OID = procres.OID,
                    OutletID = sDKRequest.OutletID ?? 0,
                    OTP = string.Empty,
                    RMode = RequestMode.SDK,
                    PartnerID = sDKRequest.PartnerID ?? 0,
                    Token = sDKRequest.Token ?? string.Empty,
                    OTPRefID = 0,
                    PidData = string.Empty
                });
                if (onboadresp.IsShowMsg)
                {
                    sdkResponse.Statuscode = onboadresp.Statuscode;
                    sdkResponse.Msg = onboadresp.Msg;
                }                
                sdkResponse.data = new
                {
                    onboadresp.SDKType,
                    onboadresp.SDKDetail
                };
            }
            #endregion
            return sdkResponse;
        }
        public async Task<SDKDetailResponse> InitiateMiniBank(SDKIntitateRequest sDKRequest)
        {
            var sdkResponse = new SDKDetailResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            #region CodeValidation
            if (sDKRequest.UserID < 1)
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " UserID";
                return sdkResponse;
            }
            if (string.IsNullOrEmpty(sDKRequest.PIN))
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " PIN";
                return sdkResponse;
            }
            if (string.IsNullOrEmpty(sDKRequest.SPKey))
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " SPKey";
                return sdkResponse;
            }
            if (sDKRequest.OutletID < 10000)
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " OutletID";
                return sdkResponse;
            }
            if (!sDKRequest.SPKey.In(SPKeys.CashAtPOS, SPKeys.CreditCardInternational, SPKeys.CreditCardPremium, SPKeys.CreditCardsStandard, SPKeys.DebitCardRupay, SPKeys.DebitCards, SPKeys.MATMCashWithdrawal, SPKeys.MATMCashDeposit, SPKeys.MposEMI, SPKeys.MposUPI, SPKeys.MposWallet))
            {
                sdkResponse.Msg = "Invalid choice of services";
                return sdkResponse;
            }
            #endregion
            #region ValidationFromDB
            IProcedure procValidate = new ProcValidateSDKLoginRequest(_dal);
            var procres = (SDKResponse)procValidate.Call(sDKRequest);
            sdkResponse.Statuscode = procres.Statuscode;
            sdkResponse.Msg = procres.Msg;
            if (sdkResponse.Statuscode == ErrorCodes.One)
            {
                BBPSML bbpsML = new BBPSML(_accessor, _env, false);
                var onboadresp = bbpsML.CheckOutletStatusForServices(new CheckOutletStatusReqModel
                {
                    LoginTypeID = LoginType.ApplicationUser,
                    LoginID = sDKRequest.UserID ?? 0,
                    OID = procres.OID,
                    OutletID = sDKRequest.OutletID ?? 0,
                    OTP = string.Empty,
                    RMode = RequestMode.SDK,
                    PartnerID = sDKRequest.PartnerID ?? 0,
                    Token = sDKRequest.Token ?? string.Empty,
                    OTPRefID = 0,
                    PidData = string.Empty
                });

                if (onboadresp.SDKType != sDKRequest.SDKType)
                {
                    sdkResponse.Statuscode = ErrorCodes.Minus1;
                    sdkResponse.Msg = "(INVSDK)Request is unmatched";
                }
                var _APICode = string.Empty;
                if (sDKRequest.SDKType == AEPSInterfaceType.FINGPAY)
                {
                    _APICode = APICode.FINGPAY;
                }
                else if (sDKRequest.SDKType == AEPSInterfaceType.MAHAGRAM)
                {
                    _APICode = APICode.MAHAGRAM;
                }
                IMiniBankML miniBankML = new AEPSML(_accessor, _env, false);
                var mbResp = await miniBankML.MakeMiniBankTransaction(new MiniBankTransactionServiceReq
                {
                    AmountR = sDKRequest.Amount ?? 0,
                    OID = procres.OID,
                    TXNType = "CW",
                    APICode = _APICode,
                    LoginID = sDKRequest.UserID ?? 0,
                    OutletIDSelf = sDKRequest.OutletID ?? 0,
                    RequestModeID = RequestMode.SDK
                }).ConfigureAwait(false);
                sdkResponse.Statuscode = mbResp.Statuscode;
                sdkResponse.Msg = mbResp.Msg;
                if (mbResp.Statuscode == ErrorCodes.One)
                {
                    sdkResponse.data = new { mbResp.TID };
                }
            }
            #endregion
            return sdkResponse;
        }
        public SDKDetailResponse UpdateMiniBankStatus(SDKIntitateRequest sDKRequest)
        {
            var sdkResponse = new SDKDetailResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            try
            {
                if (sDKRequest != null)
                {
                    IMiniBankML miniBankML = new AEPSML(_accessor, _env, false);
                    var mbStsChk = miniBankML.MBStatusCheck(new MBStatusCheckRequest
                    {
                        TID = sDKRequest.TID ?? 0,
                        APIStatus = sDKRequest.APIStatus ?? 0,
                        VendorID = sDKRequest.VendorID,
                        RequestPage = nameof(RequestMode.SDK),
                        OutletID = sDKRequest.OutletID ?? 0,
                        Amount = sDKRequest.Amount ?? 0,
                        SDKMsg = sDKRequest.SDKMsg,
                        AccountNo = sDKRequest.AccountNo,
                        BankName = sDKRequest.BankName
                    });
                    sdkResponse.data = mbStsChk;
                    if (mbStsChk != null)
                    {
                        sdkResponse.Statuscode = mbStsChk.Statuscode;
                        sdkResponse.Msg = mbStsChk.Msg;
                    }
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "UpdateMiniBankStatus",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = sDKRequest.TID ?? 0
                });
            }

            return sdkResponse;
        }
        public SDKDetailResponse GetBank()
        {
            IBankML bankML = new BankML(_accessor, _env, false);
            return new SDKDetailResponse
            {
                Statuscode = ErrorCodes.One,
                Msg = ErrorCodes.SUCCESS,
                data = bankML.AEPSBankMasters()
            };
        }
        public async Task<SDKDetailResponse> AEPSWithdrawal(SDKIntitateRequest sDKRequest)
        {
            var sdkResponse = new SDKDetailResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            #region CodeValidation
            if (sDKRequest.UserID < 1)
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " UserID";
                return sdkResponse;
            }
            if (string.IsNullOrEmpty(sDKRequest.PIN))
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " PIN";
                return sdkResponse;
            }
            if (string.IsNullOrEmpty(sDKRequest.SPKey))
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " SPKey";
                return sdkResponse;
            }
            if (sDKRequest.OutletID < 10000)
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " OutletID";
                return sdkResponse;
            }
            if (!sDKRequest.SPKey.In(SPKeys.AepsCashWithdrawal, SPKeys.Aadharpay))
            {
                sdkResponse.Msg = "Invalid choice of services";
                return sdkResponse;
            }
            if (sDKRequest.pidData == null)
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " pidData";
                return sdkResponse;
            }
            if (!Validate.O.IsAADHAR(sDKRequest.Aadhar ?? string.Empty))
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " Aadhar";
                return sdkResponse;
            }
            if (string.IsNullOrEmpty(sDKRequest.BankIIN))
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " BankIIN";
                return sdkResponse;
            }
            if ((sDKRequest.Amount ?? 0) < 1)
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " Amount";
                return sdkResponse;
            }
            if (sDKRequest.SDKType != AEPSInterfaceType.FINGPAY)
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " SDK selection not allowed";
                return sdkResponse;
            }
            #endregion
            #region ValidationFromDB
            IProcedure procValidate = new ProcValidateSDKLoginRequest(_dal);
            var procres = (SDKResponse)procValidate.Call(sDKRequest);
            sdkResponse.Statuscode = procres.Statuscode;
            sdkResponse.Msg = procres.Msg;
            if (sdkResponse.Statuscode == ErrorCodes.One)
            {
                BBPSML bbpsML = new BBPSML(_accessor, _env, false);
                var onboadresp = bbpsML.CheckOutletStatusForServices(new CheckOutletStatusReqModel
                {
                    LoginTypeID = LoginType.ApplicationUser,
                    LoginID = sDKRequest.UserID ?? 0,
                    OID = procres.OID,
                    OutletID = sDKRequest.OutletID ?? 0,
                    OTP = string.Empty,
                    RMode = RequestMode.SDK,
                    PartnerID = sDKRequest.PartnerID ?? 0,
                    Token = sDKRequest.Token ?? string.Empty,
                    OTPRefID = 0,
                    PidData = string.Empty
                });

                if (onboadresp.SDKType != sDKRequest.SDKType)
                {
                    sdkResponse.Statuscode = ErrorCodes.Minus1;
                    sdkResponse.Msg = "(INVSDK)Request is unmatched";
                }
                IDeviceML deviceMl = new DeviceML(_accessor, _env, false);
                var withRes = new WithdrawlResponse();
                if (sDKRequest.SPKey == SPKeys.AepsCashWithdrawal)
                {
                    withRes = await deviceMl.Withdrawl(sDKRequest.pidData, sDKRequest.Aadhar, sDKRequest.BankIIN, sDKRequest.SDKType ?? 0, sDKRequest.Amount ?? 0, sDKRequest.UserID ?? 0, sDKRequest.OutletID ?? 0, RequestMode.SDK, procres.OID, sDKRequest.IMEI, sDKRequest.Lattitude, sDKRequest.Longitude,sDKRequest.pidDataXML).ConfigureAwait(false);
                }
                else
                {
                    withRes = await deviceMl.Aadharpay(sDKRequest.pidData, sDKRequest.Aadhar, sDKRequest.BankIIN, sDKRequest.SDKType ?? 0, sDKRequest.Amount ?? 0, sDKRequest.UserID ?? 0, sDKRequest.OutletID ?? 0, RequestMode.SDK, procres.OID, sDKRequest.IMEI, sDKRequest.Lattitude, sDKRequest.Longitude, sDKRequest.pidDataXML).ConfigureAwait(false);
                }
                sdkResponse.Statuscode = withRes.Statuscode;
                sdkResponse.Msg = withRes.Msg;
                if (withRes.Statuscode == ErrorCodes.One)
                {
                    sdkResponse.data = new
                    {
                        withRes.Status,
                        withRes.LiveID,
                        withRes.TransactionID,
                        ServerDate = DateTime.Now.ToString("dd MMM yyyy HH:mm:ss", CultureInfo.InvariantCulture),
                        withRes.Balance
                    };
                }
            }
            #endregion
            return sdkResponse;
        }

        public async Task<SDKDetailResponse> GetBalanceAEPS(SDKIntitateRequest sDKRequest)
        {
            var sdkResponse = new SDKDetailResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            #region CodeValidation
            if (sDKRequest.UserID < 1)
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " UserID";
                return sdkResponse;
            }
            if (string.IsNullOrEmpty(sDKRequest.PIN))
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " PIN";
                return sdkResponse;
            }
            if (string.IsNullOrEmpty(sDKRequest.SPKey))
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " SPKey";
                return sdkResponse;
            }
            if (sDKRequest.OutletID < 10000)
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " OutletID";
                return sdkResponse;
            }
            if (!sDKRequest.SPKey.In(SPKeys.AepsCashWithdrawal))
            {
                sdkResponse.Msg = "Invalid choice of services";
                return sdkResponse;
            }
            if (sDKRequest.pidData == null)
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " pidData";
                return sdkResponse;
            }
            if (!Validate.O.IsAADHAR(sDKRequest.Aadhar ?? string.Empty))
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " Aadhar";
                return sdkResponse;
            }
            if (string.IsNullOrEmpty(sDKRequest.BankIIN))
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " BankIIN";
                return sdkResponse;
            }
            if (sDKRequest.SDKType != AEPSInterfaceType.FINGPAY)
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " SDK selection not allowed";
                return sdkResponse;
            }
            #endregion
            #region ValidationFromDB
            IProcedure procValidate = new ProcValidateSDKLoginRequest(_dal);
            var procres = (SDKResponse)procValidate.Call(sDKRequest);
            sdkResponse.Statuscode = procres.Statuscode;
            sdkResponse.Msg = procres.Msg;
            if (sdkResponse.Statuscode == ErrorCodes.One)
            {
                BBPSML bbpsML = new BBPSML(_accessor, _env, false);
                var onboadresp = bbpsML.CheckOutletStatusForServices(new CheckOutletStatusReqModel
                {
                    LoginTypeID = LoginType.ApplicationUser,
                    LoginID = sDKRequest.UserID ?? 0,
                    OID = procres.OID,
                    OutletID = sDKRequest.OutletID ?? 0,
                    OTP = string.Empty,
                    RMode = RequestMode.SDK,
                    PartnerID = sDKRequest.PartnerID ?? 0,
                    Token = sDKRequest.Token ?? string.Empty,
                    OTPRefID = 0,
                    PidData = sDKRequest.pidDataXML
                });

                if (onboadresp.SDKType != sDKRequest.SDKType)
                {
                    sdkResponse.Statuscode = ErrorCodes.Minus1;
                    sdkResponse.Msg = "(INVSDK)Request is unmatched";
                }
                IDeviceML deviceMl = new DeviceML(_accessor, _env, false);
                var balRes = await deviceMl.CheckBalance(sDKRequest.pidData, sDKRequest.Aadhar, sDKRequest.BankIIN, sDKRequest.SDKType ?? 0, sDKRequest.UserID ?? 0, sDKRequest.OutletID ?? 0, RequestMode.SDK, procres.OID, sDKRequest.IMEI, sDKRequest.Lattitude, sDKRequest.Longitude, sDKRequest.pidDataXML).ConfigureAwait(false);
                sdkResponse.Statuscode = balRes.Statuscode;
                sdkResponse.Msg = balRes.Msg;
                if (balRes.Statuscode == ErrorCodes.One)
                {
                    sdkResponse.data = new
                    {
                        balRes.Balance
                    };
                }
            }
            #endregion
            return sdkResponse;
        }

        public SDKDetailResponse GetMiniStatement(SDKIntitateRequest sDKRequest)
        {
            var sdkResponse = new SDKDetailResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            #region CodeValidation
            if (sDKRequest.UserID < 1)
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " UserID";
                return sdkResponse;
            }
            if (string.IsNullOrEmpty(sDKRequest.PIN))
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " PIN";
                return sdkResponse;
            }
            if (string.IsNullOrEmpty(sDKRequest.SPKey))
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " SPKey";
                return sdkResponse;
            }
            if (sDKRequest.OutletID < 10000)
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " OutletID";
                return sdkResponse;
            }
            if (!sDKRequest.SPKey.In(SPKeys.AepsCashWithdrawal, SPKeys.AepsMiniStatement))
            {
                sdkResponse.Msg = "Invalid choice of services";
                return sdkResponse;
            }
            sDKRequest.SPKey = SPKeys.AepsMiniStatement;
            if (sDKRequest.pidData == null)
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " pidData";
                return sdkResponse;
            }
            if (!Validate.O.IsAADHAR(sDKRequest.Aadhar ?? string.Empty))
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " Aadhar";
                return sdkResponse;
            }
            if (string.IsNullOrEmpty(sDKRequest.BankIIN))
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " BankIIN";
                return sdkResponse;
            }
            if (string.IsNullOrEmpty(sDKRequest.BankName))
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " BankName";
                return sdkResponse;
            }
            if (sDKRequest.SDKType != AEPSInterfaceType.FINGPAY)
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " SDK selection not allowed";
                return sdkResponse;
            }
            #endregion
            #region ValidationFromDB
            IProcedure procValidate = new ProcValidateSDKLoginRequest(_dal);
            var procres = (SDKResponse)procValidate.Call(sDKRequest);
            sdkResponse.Statuscode = procres.Statuscode;
            sdkResponse.Msg = procres.Msg;
            if (sdkResponse.Statuscode == ErrorCodes.One)
            {
                BBPSML bbpsML = new BBPSML(_accessor, _env, false);
                var onboadresp = bbpsML.CheckOutletStatusForServices(new CheckOutletStatusReqModel
                {
                    LoginTypeID = LoginType.ApplicationUser,
                    LoginID = sDKRequest.UserID ?? 0,
                    OID = procres.OID,
                    OutletID = sDKRequest.OutletID ?? 0,
                    OTP = string.Empty,
                    RMode = RequestMode.SDK,
                    PartnerID = sDKRequest.PartnerID ?? 0,
                    Token = sDKRequest.Token ?? string.Empty,
                    OTPRefID = 0,
                    PidData = string.Empty
                });

                if (onboadresp.SDKType != sDKRequest.SDKType)
                {
                    sdkResponse.Statuscode = ErrorCodes.Minus1;
                    sdkResponse.Msg = "(INVSDK)Request is unmatched";
                }
                IDeviceML deviceMl = new DeviceML(_accessor, _env, false);
                var miniStatementResponse = deviceMl.MiniStatement(sDKRequest.pidData, sDKRequest.Aadhar, sDKRequest.BankName, sDKRequest.BankIIN, sDKRequest.SDKType ?? 0, sDKRequest.UserID ?? 0, sDKRequest.OutletID ?? 0, RequestMode.SDK, procres.OID, string.Empty, sDKRequest.IMEI, sDKRequest.Lattitude, sDKRequest.Longitude, sDKRequest.pidDataXML);
                sdkResponse.Statuscode = miniStatementResponse.Statuscode;
                sdkResponse.Msg = miniStatementResponse.Msg;
                if (miniStatementResponse.Statuscode > 0)
                {
                    sdkResponse.data = new
                    {
                        miniStatementResponse.Balance,
                        miniStatementResponse.Statements
                    };
                }
            }
            #endregion
            return sdkResponse;
        }

        public async Task<SDKDetailResponse> DepositGenerateOTP(SDKIntitateRequest sDKRequest)
        {
            var sdkResponse = new SDKDetailResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            #region CodeValidation
            if (sDKRequest.UserID < 1)
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " UserID";
                return sdkResponse;
            }
            if (string.IsNullOrEmpty(sDKRequest.PIN))
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " PIN";
                return sdkResponse;
            }
            if (string.IsNullOrEmpty(sDKRequest.SPKey))
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " SPKey";
                return sdkResponse;
            }
            if (sDKRequest.OutletID < 10000)
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " OutletID";
                return sdkResponse;
            }
            if (!sDKRequest.SPKey.In(SPKeys.AepsCashDeposit, SPKeys.MATMCashDeposit))
            {
                sdkResponse.Msg = "Invalid choice of services";
                return sdkResponse;
            }

            if (!Validate.O.IsAADHAR(sDKRequest.Aadhar ?? string.Empty))
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " Aadhar";
                return sdkResponse;
            }
            if (string.IsNullOrEmpty(sDKRequest.BankIIN))
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " BankIIN";
                return sdkResponse;
            }
            if ((sDKRequest.Amount ?? 0) < 1)
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " Amount";
                return sdkResponse;
            }
            if (sDKRequest.SDKType != AEPSInterfaceType.FINGPAY)
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " SDK selection not allowed";
                return sdkResponse;
            }
            #endregion
            #region ValidationFromDB
            IProcedure procValidate = new ProcValidateSDKLoginRequest(_dal);
            var procres = (SDKResponse)procValidate.Call(sDKRequest);
            sdkResponse.Statuscode = procres.Statuscode;
            sdkResponse.Msg = procres.Msg;
            if (sdkResponse.Statuscode == ErrorCodes.One)
            {
                BBPSML bbpsML = new BBPSML(_accessor, _env, false);
                var onboadresp = bbpsML.CheckOutletStatusForServices(new CheckOutletStatusReqModel
                {
                    LoginTypeID = LoginType.ApplicationUser,
                    LoginID = sDKRequest.UserID??0,
                    OID = procres.OID,
                    OutletID = sDKRequest.OutletID ?? 0,
                    RMode = RequestMode.SDK,
                    PidData = string.Empty,
                    PartnerID= sDKRequest.PartnerID ?? 0,
                    Token= sDKRequest.Token ?? string.Empty
                });

                if (onboadresp.SDKType != sDKRequest.SDKType)
                {
                    sdkResponse.Statuscode = ErrorCodes.Minus1;
                    sdkResponse.Msg = "(INVSDK)Request is unmatched";
                }
                IDeviceML deviceMl = new DeviceML(_accessor, _env, false);
                var depositRes = await deviceMl.DepositGenerateOTP(new DepositRequest
                {
                    UserID = sDKRequest.UserID ?? 0,
                    OutletID = sDKRequest.OutletID ?? 0,
                    InterfaceType = onboadresp.SDKType,
                    AccountNo = sDKRequest.Aadhar,
                    IIN = sDKRequest.BankIIN,
                    Amount = sDKRequest.Amount ?? 0,
                    RMode = RequestMode.SDK
                }).ConfigureAwait(false);
                sdkResponse.Statuscode = depositRes.Statuscode;
                sdkResponse.Msg = depositRes.Msg;
                if (depositRes.Statuscode == ErrorCodes.One)
                {
                    sdkResponse.data = new
                    {
                        IsOTPRequired = depositRes.Statuscode == ErrorCodes.One,
                        Reff1 = depositRes.TransactionID,
                        Reff2 = depositRes.RefferenceNo,
                        Reff3 = depositRes.VendorID
                    };
                }
            }
            #endregion
            return sdkResponse;
        }
        public async Task<SDKDetailResponse> VerifyDepositOTP(SDKIntitateRequest sDKRequest)
        {
            var sdkResponse = new SDKDetailResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            #region CodeValidation
            if (sDKRequest.UserID < 1)
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " UserID";
                return sdkResponse;
            }
            if (string.IsNullOrEmpty(sDKRequest.PIN))
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " PIN";
                return sdkResponse;
            }
            if (string.IsNullOrEmpty(sDKRequest.SPKey))
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " SPKey";
                return sdkResponse;
            }
            if (sDKRequest.OutletID < 10000)
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " OutletID";
                return sdkResponse;
            }
            if (!sDKRequest.SPKey.In(SPKeys.AepsCashDeposit, SPKeys.MATMCashDeposit))
            {
                sdkResponse.Msg = "Invalid choice of services";
                return sdkResponse;
            }

            if (!Validate.O.IsAADHAR(sDKRequest.Aadhar ?? string.Empty))
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " Aadhar";
                return sdkResponse;
            }
            if (string.IsNullOrEmpty(sDKRequest.BankIIN))
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " BankIIN";
                return sdkResponse;
            }
            if ((sDKRequest.Amount ?? 0) < 1)
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " Amount";
                return sdkResponse;
            }
            if (sDKRequest.SDKType != AEPSInterfaceType.FINGPAY)
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " SDK selection not allowed";
                return sdkResponse;
            }
            #endregion
            #region ValidationFromDB
            IProcedure procValidate = new ProcValidateSDKLoginRequest(_dal);
            var procres = (SDKResponse)procValidate.Call(sDKRequest);
            sdkResponse.Statuscode = procres.Statuscode;
            sdkResponse.Msg = procres.Msg;
            if (sdkResponse.Statuscode == ErrorCodes.One)
            {
                BBPSML bbpsML = new BBPSML(_accessor, _env, false);
                var onboadresp = bbpsML.CheckOutletStatusForServices(new CheckOutletStatusReqModel
                {
                    LoginTypeID = LoginType.ApplicationUser,
                    LoginID = sDKRequest.UserID ?? 0,
                    OID = procres.OID,
                    OutletID = sDKRequest.OutletID ?? 0,
                    RMode = RequestMode.SDK,
                    PidData = string.Empty,
                    PartnerID = sDKRequest.PartnerID ?? 0,
                    Token = sDKRequest.Token ?? string.Empty
                });

                if (onboadresp.SDKType != sDKRequest.SDKType)
                {
                    sdkResponse.Statuscode = ErrorCodes.Minus1;
                    sdkResponse.Msg = "(INVSDK)Request is unmatched";
                }
                IDeviceML deviceMl = new DeviceML(_accessor, _env, false);
                var depositRes = await deviceMl.DepositVerifyOTP(new DepositRequest
                {
                    UserID = sDKRequest.UserID ?? 0,
                    OutletID = sDKRequest.OutletID ?? 0,
                    InterfaceType = onboadresp.SDKType,
                    AccountNo = sDKRequest.Aadhar,
                    IIN = sDKRequest.BankIIN,
                    Amount = sDKRequest.Amount ?? 0,
                    RMode = RequestMode.SDK,
                    OTP = sDKRequest.OTP,
                    Reff1 = sDKRequest.Reff1,
                    Reff2 = sDKRequest.Reff2,
                    Reff3 = sDKRequest.Reff3
                }).ConfigureAwait(false);
                sdkResponse.Statuscode = depositRes.Statuscode;
                sdkResponse.Msg = depositRes.Msg;
                if (depositRes.Statuscode == ErrorCodes.One)
                {
                    sdkResponse.data = new
                    {
                        Status = depositRes.Status,
                        LiveID = depositRes.LiveID,
                        TransactionID = depositRes.TransactionID,
                        ServerDate = DateTime.Now.ToString("dd MMM yyyy HH:mm:ss", CultureInfo.InvariantCulture),
                        Balance = depositRes.Balance,
                        BeneficiaryName = depositRes.BeneficaryName
                    };
                }
            }
            #endregion
            return sdkResponse;
        }
        public async Task<SDKDetailResponse> DepositAccount(SDKIntitateRequest sDKRequest)
        {
            var sdkResponse = new SDKDetailResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            #region CodeValidation
            if (sDKRequest.UserID < 1)
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " UserID";
                return sdkResponse;
            }
            if (string.IsNullOrEmpty(sDKRequest.PIN))
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " PIN";
                return sdkResponse;
            }
            if (string.IsNullOrEmpty(sDKRequest.SPKey))
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " SPKey";
                return sdkResponse;
            }
            if (sDKRequest.OutletID < 10000)
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " OutletID";
                return sdkResponse;
            }
            if (!sDKRequest.SPKey.In(SPKeys.AepsCashDeposit, SPKeys.MATMCashDeposit))
            {
                sdkResponse.Msg = "Invalid choice of services";
                return sdkResponse;
            }

            if (!Validate.O.IsAADHAR(sDKRequest.Aadhar ?? string.Empty))
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " Aadhar";
                return sdkResponse;
            }
            if (string.IsNullOrEmpty(sDKRequest.BankIIN))
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " BankIIN";
                return sdkResponse;
            }
            if ((sDKRequest.Amount ?? 0) < 1)
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " Amount";
                return sdkResponse;
            }
            if (sDKRequest.SDKType != AEPSInterfaceType.FINGPAY)
            {
                sdkResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " SDK selection not allowed";
                return sdkResponse;
            }
            #endregion
            #region ValidationFromDB
            IProcedure procValidate = new ProcValidateSDKLoginRequest(_dal);
            var procres = (SDKResponse)procValidate.Call(sDKRequest);
            sdkResponse.Statuscode = procres.Statuscode;
            sdkResponse.Msg = procres.Msg;
            if (sdkResponse.Statuscode == ErrorCodes.One)
            {
                BBPSML bbpsML = new BBPSML(_accessor, _env, false);
                var onboadresp = bbpsML.CheckOutletStatusForServices(new CheckOutletStatusReqModel
                {
                    LoginTypeID = LoginType.ApplicationUser,
                    LoginID = sDKRequest.UserID ?? 0,
                    OID = procres.OID,
                    OutletID = sDKRequest.OutletID ?? 0,
                    RMode = RequestMode.SDK,
                    PidData = string.Empty,
                    PartnerID = sDKRequest.PartnerID ?? 0,
                    Token = sDKRequest.Token ?? string.Empty
                });

                if (onboadresp.SDKType != sDKRequest.SDKType)
                {
                    sdkResponse.Statuscode = ErrorCodes.Minus1;
                    sdkResponse.Msg = "(INVSDK)Request is unmatched";
                }
                IDeviceML deviceMl = new DeviceML(_accessor, _env, false);
                var depositRes = await deviceMl.DepositAccount(new DepositRequest
                {
                    UserID = sDKRequest.UserID ?? 0,
                    OutletID = sDKRequest.OutletID ?? 0,
                    InterfaceType = onboadresp.SDKType,
                    AccountNo = sDKRequest.Aadhar,
                    IIN = sDKRequest.BankIIN,
                    Amount = sDKRequest.Amount ?? 0,
                    RMode = RequestMode.SDK,
                    OTP = sDKRequest.OTP,
                    Reff1 = sDKRequest.Reff1,
                    Reff2 = sDKRequest.Reff2,
                    Reff3 = sDKRequest.Reff3
                }).ConfigureAwait(false);
                sdkResponse.Statuscode = depositRes.Statuscode;
                sdkResponse.Msg = depositRes.Msg;
                if (depositRes.Statuscode == ErrorCodes.One)
                {
                    sdkResponse.data = new
                    {
                        Status = depositRes.Status,
                        LiveID = depositRes.LiveID,
                        TransactionID = depositRes.TransactionID,
                        ServerDate = DateTime.Now.ToString("dd MMM yyyy HH:mm:ss", CultureInfo.InvariantCulture),
                        Balance = depositRes.Balance,
                        BeneficiaryName = depositRes.BeneficaryName
                    };
                }
            }
            #endregion
            return sdkResponse;
        }

        public async Task SaveAPILog(APIReqResp aPIReqResp)
        {
            IProcedureAsync _proc = new ProcLogAPIUserReqResp(_dal);
            await _proc.Call(aPIReqResp).ConfigureAwait(false);
        }
    }
}
