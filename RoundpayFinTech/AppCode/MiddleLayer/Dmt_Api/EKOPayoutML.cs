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
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.MoneyTransfer;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.ThirdParty.Eko;
using RoundpayFinTech.AppCode.ThirdParty.Instantpay;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Validators;

namespace RoundpayFinTech.AppCode.MiddleLayer.Dmt_Api
{
    public class EKOPayoutML : IMoneyTransferAPIML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration Configuration;
        private readonly IConnectionConfiguration _c;
        private readonly InstantPayAppSetting appSetting;
        private readonly EKOAppSetting _eKOAppSetting;
        private readonly IDAL _dal;

        public EKOPayoutML(IHttpContextAccessor accessor, IHostingEnvironment env, IDAL dal)
        {
            _accessor = accessor;
            _env = env;
            var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile((_env.IsProduction() ? "appsettings.json" : "appsettings.Development.json"));
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            _dal = dal;
            _eKOAppSetting = EKOPayoutSetting();
        }
        private EKOAppSetting EKOPayoutSetting()
        {
            var setting = new EKOAppSetting();
            try
            {
                setting = new EKOAppSetting
                {
                    EKOKey = Configuration["DMR:EKOP:EKOKey"],
                    InitiatorKey = Configuration["DMR:EKOP:InitiatorKey"],
                    DeveloperKey = Configuration["DMR:EKOP:DeveloperKey"],
                    BaseURL = Configuration["DMR:EKOP:BaseURL"],
                    OnBoardingURL = Configuration["DMR:EKOP:OnBoardingURL"]
                };
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "EKO2AppSetting",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                });
            }
            return setting;
        }
        public MSenderCreateResp CreateSender(MTAPIRequest request)
        {
            var res = new MSenderCreateResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Unknown_Error
            };
            try
            {
                var dbres = (new ProcUpdateSender(_dal).Call(new SenderRequest
                {
                    Name = request.FirstName + " " + request.LastName,
                    MobileNo = request.SenderMobile,
                    Pincode = request.Pincode.ToString(),
                    Address = request.Address,
                    City = request.City,
                    StateID = request.StateID,
                    AadharNo = request.AadharNo,
                    Dob = request.DOB,
                    UserID = request.UserID
                })) as SenderInfo;
                if (dbres.Statuscode == ErrorCodes.Minus1)
                {
                    res.Msg = dbres.Msg;
                    return res;
                }
                if (dbres.Statuscode == ErrorCodes.One && dbres._VerifyStatus != ErrorCodes.Two)
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = nameof(DMTErrorCodes.Partial_Registration_Please_verify_sender_with_OTP).Replace("_", " ");
                    res.ErrorCode = DMTErrorCodes.Partial_Registration_Please_verify_sender_with_OTP;
                    res.IsOTPGenerated = true;
                    res.OTP = dbres.OTP;
                    res.WID = dbres.WID;
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "CreateSender",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = request.APIID,
                Method = "CreateSender",
                RequestModeID = request.RequestMode,
                Request = "Internal",
                Response = JsonConvert.SerializeObject(res),
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            });
            return res;
        }

        public MSenderLoginResponse GetSender(MTAPIRequest request)
        {
            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            try
            {
                var senderRequest = (SenderRequest)new ProcGetSender(_dal).Call(request.SenderMobile);
                if (string.IsNullOrWhiteSpace(senderRequest.Name))
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = nameof(DMTErrorCodes.Sender_Not_Found).Replace("_", " ");
                    res.IsSenderNotExists = true;
                    res.ErrorCode = DMTErrorCodes.Sender_Not_Found;
                    return res;
                }
                else if (senderRequest._VerifyStatus == ErrorCodes.Two)
                {
                    var procSender = new ProcGetSenderLimit(_dal);
                    var senderLimit = (SenderLimitModel)procSender.Call(new CommonReq
                    {
                        CommonInt = senderRequest.ID,
                        CommonInt2 = request.APIID
                    }).Result;
                    res.RemainingLimit = senderLimit.SenderLimit - senderLimit.LimitUsed;
                    res.AvailbleLimit = senderLimit.SenderLimit;
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = nameof(DMTErrorCodes.Detail_Found_Successfully).Replace("_", " ");
                    res.ErrorCode = DMTErrorCodes.Detail_Found_Successfully;
                    res.SenderMobile = request.SenderMobile;
                    res.KYCStatus = SenderKYCStatus.ACTIVE;
                    res.SenderName = senderRequest.Name;
                    res.IsNotCheckLimit = senderRequest.IsNotCheckLimit;
                }
                else if (senderRequest._VerifyStatus == ErrorCodes.One)
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = nameof(DMTErrorCodes.Sender_Not_Found).Replace("_", " ");
                    res.IsSenderNotExists = true;
                    res.ErrorCode = DMTErrorCodes.Sender_Not_Found;
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GetSender",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public MSenderCreateResp SenderKYC(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }

        public MSenderCreateResp SenderEKYC(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }

        public MSenderCreateResp SenderResendOTP(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }

        public MSenderLoginResponse VerifySender(MTAPIRequest request)
        {
            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Unknown_Error
            };
            try
            {
                var req = new CommonReq
                {
                    CommonStr = request.SenderMobile,
                    CommonStr2 = request.OTP,
                    CommonInt = request.UserID
                };
                var senderRes = (SenderInfo)new ProcVerySenderOTP(_dal).Call(req);
                if (senderRes.Statuscode == ErrorCodes.Minus1)
                {
                    res.Msg = senderRes.Msg;
                    return res;
                }
                else if (senderRes.Statuscode == ErrorCodes.One)
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = ErrorCodes.DMTSCS;
                    res.ErrorCode = ErrorCodes.Transaction_Successful;
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
            return res;
        }

        public MSenderLoginResponse CreateBeneficiary(MTAPIRequest request)
        {
            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Transaction_Successful
            };
            try
            {
                var param = new BenificiaryDetail
                {
                    _SenderMobileNo = request.SenderMobile,
                    _Name = request.mBeneDetail.BeneName,
                    _AccountNumber = request.mBeneDetail.AccountNo,
                    _MobileNo = request.mBeneDetail.MobileNo,
                    _IFSC = request.mBeneDetail.IFSC,
                    _BankName = request.mBeneDetail.BankName,
                    _EntryBy = request.UserID,
                    _VerifyStatus = 1,
                    _APICode = request.APICode,
                    _BankID = request.mBeneDetail.BankID
                };
                var resdb = (BenificiaryModel)new ProcAddBenificiary(_dal).Call(param);
                if (resdb.Statuscode == ErrorCodes.One)
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = ErrorCodes.BENESCS;
                    res.ErrorCode = ErrorCodes.Transaction_Successful;
                }
                else
                {
                    res.Msg = resdb.Msg;
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "CreateBeneficiary",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            return res;
        }

        public MBeneficiaryResp GetBeneficiary(MTAPIRequest request)
        {
            var res = new MBeneficiaryResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(DMTErrorCodes.Beneficiary_not_found).Replace("_", " "),
                ErrorCode = DMTErrorCodes.Beneficiary_not_found
            };
            try
            {
                var resDB = (new ProcGetBenificiary(_dal).Call(new DMTReq { SenderNO = request.SenderMobile })) as BenificiaryModel;
                res.Statuscode = resDB.Statuscode;
                res.Msg = resDB.Msg;
                if (resDB != null && resDB.Statuscode == ErrorCodes.One)
                {
                    var Beneficiaries = new List<MBeneDetail>();
                    if (resDB.benificiaries != null && resDB.benificiaries.Count > 0)
                    {
                        foreach (var r in resDB.benificiaries)
                        {
                            Beneficiaries.Add(new MBeneDetail
                            {
                                AccountNo = r._AccountNumber,
                                BankName = r._BankName,
                                IFSC = r._IFSC,
                                BeneName = r._Name,
                                MobileNo = r._BankName,
                                BeneID = r._ID.ToString()
                            });
                        }
                    }
                    res.Beneficiaries = Beneficiaries;
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
            return res;
        }

        public MSenderCreateResp GenerateOTP(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }

        public MSenderLoginResponse ValidateBeneficiary(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }

        public MSenderLoginResponse RemoveBeneficiary(MTAPIRequest request)
        {
            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Unknown_Error
            };
            try
            {
                var _res = (ResponseStatus)new ProcRemoveBeneficiaryNew(_dal).Call(new CommonReq
                {
                    LoginID = request.UserID,
                    CommonInt = Convert.ToInt32(request.mBeneDetail.BeneID),
                    CommonStr = request.SenderMobile
                });
                res.Statuscode = _res.Statuscode;
                res.Msg = _res.Msg;
                res.ErrorCode = _res.ErrorCode;
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
            return res;
        }

        public MSenderLoginResponse RemoveBeneficiaryValidate(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }

        public DMRTransactionResponse VerifyAccount(MTAPIRequest request)
        {
            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = RechargeRespType._PENDING,
                VendorID = "",
                LiveID = ""
            };
            var apiRequest = new IPayPayoutRequest
            {
                token = appSetting.Token,
                request = new IPayPayoutRequestHelper
                {
                    sp_key = "DPN",
                    external_ref = "TID" + request.TID,
                    credit_account = request.mBeneDetail.AccountNo,
                    credit_amount = "1",
                    credit_rmn = string.Empty,
                    ifs_code = request.mBeneDetail.IFSC,
                    bene_name = request.mBeneDetail.BeneName ?? string.Empty,
                    upi_mode = string.Empty,
                    vpa = string.Empty,
                    latitude = (string.IsNullOrEmpty(request.Lattitude) ? "26.8678" : request.Lattitude).PadRight(7,'0'),
                    longitude = (string.IsNullOrEmpty(request.Longitude) ? "80.9832" : request.Longitude).PadRight(7,'0'),
                    endpoint_ip = request.IPAddress,
                    otp_auth = "0",
                    otp = string.Empty,
                    remarks = "verification",
                    alert_mobile = request.SenderMobile,
                    alert_email = request.EmailID
                }
            };
            string URL = appSetting.BaseURL + "ws/payouts/direct";
            string resp = string.Empty;
            try
            {
                resp = AppWebRequest.O.PostJsonDataUsingHWR(URL, apiRequest);
                if (resp != null)
                {
                    var apiResp = JsonConvert.DeserializeObject<IPayPayoutResponse>(resp);
                    if (apiResp.statuscode != null)
                    {
                        if (apiResp.statuscode.Equals("TXN"))
                        {
                            if (apiResp.data != null)
                            {
                                if (apiResp.data.payout != null)
                                {
                                    res.LiveID = apiResp.data.payout.credit_refid.ToString();
                                    res.Statuscode = RechargeRespType.SUCCESS;
                                    res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                    res.ErrorCode = ErrorCodes.Transaction_Successful;
                                    res.BeneName = apiResp.data.payout.name;
                                }
                                res.VendorID = apiResp.data.ipay_id;
                            }
                        }
                        else if (apiResp.statuscode.Equals("ERR"))
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = apiResp.status.Contains("suff") ? ErrorCodes.Down : apiResp.status;
                            res.ErrorCode = ErrorCodes.Unknown_Error;
                            res.LiveID = res.Msg;
                        }
                        else
                        {
                            res.Statuscode = RechargeRespType.PENDING;
                            res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Transaction_Successful;
                            if (apiResp.data != null)
                            {
                                if (apiResp.data.payout != null)
                                {
                                    res.LiveID = apiResp.data.payout.credit_refid.ToString();
                                    res.BeneName = apiResp.data.payout.name;
                                }
                                res.VendorID = apiResp.data.ipay_id;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                resp = " Exception:" + ex.Message + " | " + resp;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "VerifyAccount",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            res.Request = URL + "?" + JsonConvert.SerializeObject(apiRequest);
            var dMTReq = new DMTReqRes
            {
                APIID = request.APIID,
                Method = "VerifyAccount",
                RequestModeID = request.RequestMode,
                Request = res.Request,
                Response = resp ?? string.Empty,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TID.ToString()
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            res.Request = dMTReq.Request;
            res.Response = dMTReq.Response;
            if (res.Statuscode == RechargeRespType.PENDING)
            {
                var tempres = LoopStatusCheck(request.TID, request.TransactionID, request.RequestMode, request.UserID, request.APIID).Result;
                res.Statuscode = tempres.Statuscode;
                res.LiveID = tempres.LiveID;
                res.VendorID = tempres.VendorID;
                res.Msg = tempres.Msg;
                res.ErrorCode = tempres.ErrorCode;
                res.TID = request.TID;
            }
            return res;
        }

        public DMRTransactionResponse AccountTransfer(MTAPIRequest request)
        {
            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = RechargeRespType._PENDING,
                VendorID = "",
                LiveID = "",
                ErrorCode = ErrorCodes.Request_Accpeted
            };

            var beneficiaryModel = new BeneficiaryModel
            {
                ID = Convert.ToInt32(request.mBeneDetail.BeneID)
            };
            var senderRequest = new SenderRequest();
            IProcedure _proc = new GetBeneficaryByID(_dal);
            if (request.IsPayout)
            {
                beneficiaryModel.Name = request.mBeneDetail.BeneName;
                senderRequest.MobileNo = request.SenderMobile;
                senderRequest.Name = request.UserName;
            }
            else
            {
                beneficiaryModel = (BeneficiaryModel)_proc.Call(beneficiaryModel);
                senderRequest = (SenderRequest)new ProcGetSender(_dal).Call(request.SenderMobile);
            }

            var eKORequest = new EKORequest
            {
                amount = request.Amount,
                payment_mode = request.TransMode.Equals("IMPS") ? 5 : 4,
                client_ref_id = request.TID.ToString(),
                recipient_name = request.mBeneDetail.BeneName,
                ifsc = request.mBeneDetail.IFSC,
                account = request.mBeneDetail.AccountNo,
                service_code = 45,
                sender_name = senderRequest.Name,
                timestamp = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'"),
                user_code = request.APIOutletID

                //source = NEWCONNECT
                //tag = Logistic
                //beneficiary_account_type = 1
                //latlong = string.Format("{0},{1}", request.Lattitude.PadRight(11, '0'), request.Longitude.PadRight(11, '0')),
                //pincode = request.Pincode.ToString(),
            };
            string ekoTimeStamp = EKOAPIHelper.O.GetSecretKeyTimeStamp();
            string URL = _eKOAppSetting.BaseURL + string.Format("agent/user_code:{0}/settlement", eKORequest.user_code);
            var headers = new Dictionary<string, string>
            {
                { EKOAPIHelper.SecretKeyTimeStamp, ekoTimeStamp },
                { EKOAPIHelper.SecretKey, EKOAPIHelper.O.GetSecretKey(ekoTimeStamp,_eKOAppSetting.EKOKey) },
                { ContentType.Self,ContentType.x_wwww_from_urlencoded},
                { EKOAPIHelper.DeveloperKey, _eKOAppSetting.DeveloperKey }
            };
            
            StringBuilder sbPostData = new StringBuilder("initiator_id={initiator_id}&amount={amount}&payment_mode={payment_mode}&client_ref_id={client_ref_id}&recipient_name={recipient_name}&ifsc={ifsc}&account={account}&service_code=45&sender_name={sender_name}");
            sbPostData.Replace("{initiator_id}", (_eKOAppSetting.InitiatorKey ?? string.Empty));
            sbPostData.Replace("{amount}", eKORequest.amount.ToString());
            sbPostData.Replace("{payment_mode}", eKORequest.payment_mode.ToString());
            sbPostData.Replace("{client_ref_id}", eKORequest.client_ref_id);
            sbPostData.Replace("{recipient_name}", eKORequest.recipient_name);
            sbPostData.Replace("{ifsc}", eKORequest.ifsc);
            sbPostData.Replace("{account}", eKORequest.account);
            sbPostData.Replace("{sender_name}", eKORequest.sender_name);

            string response = string.Empty;
            try
            {
                response = AppWebRequest.O.HWRPostAsync(URL, sbPostData.ToString(), headers).Result;
                var ekoResp = JsonConvert.DeserializeObject<EKOClassess>(response);
                if (ekoResp != null)
                {
                    if (ekoResp.status != null)
                    {
                        if (ekoResp.status == 0)
                        {
                            res.VendorID = ekoResp.data.tid ?? "";
                            res.LiveID = ekoResp.data.bank_ref_num ?? "";
                            
                            int ekoTxStatus = Convert.ToInt16(Validate.O.IsNumeric(ekoResp.data.tx_status ?? string.Empty) ? ekoResp.data.tx_status : "-1");
                            if (ekoTxStatus == 0)
                            {
                                res.Statuscode = RechargeRespType.SUCCESS;
                                res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                res.ErrorCode = ErrorCodes.Transaction_Successful;
                            }
                            else
                            {
                                IErrorCodeML errorCodeML = new ErrorCodeML(_accessor, _env, false);
                                var eFromDB = errorCodeML.GetAPIErrorCodeDescription(request.APIGroupCode, nameof(ekoResp.status) + ekoResp.status + "_" + ekoResp.response_type_id);
                                if (!string.IsNullOrEmpty(eFromDB.Code))
                                {
                                    res.Statuscode = eFromDB.Status;
                                    res.Msg = eFromDB.Error.Replace("{REPLACE}", ekoResp.message);
                                    res.ErrorCode = Convert.ToInt32(eFromDB.Code.Trim());
                                    res.LiveID = res.Msg;
                                }
                                res.Statuscode = res.Statuscode == 0 ? RechargeRespType.PENDING : res.Statuscode;
                            }
                        }
                        else
                        {
                            IErrorCodeML errorCodeML = new ErrorCodeML(_accessor, _env, false);
                            var eFromDB = errorCodeML.GetAPIErrorCodeDescription(request.APIGroupCode, nameof(ekoResp.status) + ekoResp.status + "_" + ekoResp.response_type_id);
                            if (!string.IsNullOrEmpty(eFromDB.Code))
                            {
                                res.Statuscode = eFromDB.Status;
                                res.Msg = eFromDB.Error.Replace("{REPLACE}", ekoResp.message);
                                res.ErrorCode = Convert.ToInt32(eFromDB.Code.Trim());
                                res.LiveID = res.Msg;
                            }
                            res.Statuscode = res.Statuscode == 0 ? RechargeRespType.PENDING : res.Statuscode;
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "AccountTransfer",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            res.Request = URL + JsonConvert.SerializeObject(eKORequest) + JsonConvert.SerializeObject(headers) + "?" + sbPostData.ToString();
            res.Response = response;
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = request.APIID,
                Method = "AccountTransfer",
                RequestModeID = request.RequestMode,
                Request = res.Request,
                Response = response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TID.ToString()
            });
            return res;
        }

        public DMRTransactionResponse GetTransactionStatus(int TID, string TransactionID, int RequestMode, int UserID, int APIID)
        {
            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = nameof(RechargeRespType.PENDING),
                ErrorCode = ErrorCodes.Request_Accpeted
            };
            var fromD = ConnectionStringHelper.O.ConvertTransactionIDTo_dd_MMM_yyyy(TransactionID);

            var dataRequest = new
            {
                token = appSetting.Token,
                request = new
                {
                    external_id = "TID" + TID,
                    transaction_date = Convert.ToDateTime(fromD).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                }
            };
            string URL = appSetting.BaseURL + "ws/status/checkbyexternalid";
            string resp = string.Empty;
            try
            {
                resp = AppWebRequest.O.PostJsonDataUsingHWR(URL, dataRequest);
                if (resp != null)
                {
                    var ipaystsResp = JsonConvert.DeserializeObject<IPayStatusCheckExternalResponse>(resp);
                    if (ipaystsResp != null)
                    {
                        if (ipaystsResp.statuscode.Equals("TXN"))
                        {
                            if (ipaystsResp.data != null)
                            {
                                res.VendorID = ipaystsResp.data.order_id;
                                res.LiveID = ipaystsResp.data.serviceprovider_id;
                                if (ipaystsResp.data.additional_details != null)
                                {
                                    res.BeneName = ipaystsResp.data.additional_details.beneficiary_name;
                                }
                                if (ipaystsResp.data.transaction_status.Equals(RechargeRespType._SUCCESS))
                                {
                                    res.Statuscode = RechargeRespType.SUCCESS;
                                    res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                    res.ErrorCode = ErrorCodes.Transaction_Successful;
                                }
                                if (ipaystsResp.data.transaction_status.In(RechargeRespType._FAILED,RechargeRespType._REFUND))
                                {
                                    res.LiveID = ipaystsResp.data.transaction_description.Contains("suffi") ? ErrorCodes.Down : ipaystsResp.data.transaction_description;
                                    res.Statuscode = RechargeRespType.FAILED;
                                    res.Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " ");
                                    res.ErrorCode = ErrorCodes.Unknown_Error;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                resp = " Exception:" + ex.Message + " | " + resp;
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GetTransactionStatus",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            var dMTReq = new DMTReqRes
            {
                APIID = APIID,
                Method = "GetTransactionStatus",
                RequestModeID = RequestMode,
                Request = URL + "?" + JsonConvert.SerializeObject(dataRequest),
                Response = resp,
                SenderNo = string.Empty,
                UserID = UserID,
                TID = TID.ToString()
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            res.Request = dMTReq.Request;
            res.Response = resp;
            return res;
        }
        private async Task<DMRTransactionResponse> LoopStatusCheck(int TID, string TransactionID, int RequestMode, int UserID, int APIID)
        {
            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = nameof(ErrorCodes.Request_Accpeted)
            };
            int i = 0;
            while (i < 10)
            {
                i++;
                if (res.Statuscode == RechargeRespType.PENDING)
                {
                    await Task.Delay(10 * 1000).ConfigureAwait(false);
                    res = GetTransactionStatus(TID, TransactionID, RequestMode, UserID, APIID);
                    if (res.Statuscode != RechargeRespType.PENDING)
                    {
                        i = 10;
                    }
                }
                else
                {
                    i = 10;
                }
            }
            return res;
        }
    }
}
