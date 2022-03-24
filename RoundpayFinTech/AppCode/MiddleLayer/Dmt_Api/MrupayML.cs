using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System.IO;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.StaticModel.MoneyTransfer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Fintech.AppCode.WebRequest;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.MoneyTransfer;

namespace RoundpayFinTech.AppCode.MiddleLayer.Dmt_Api
{
    public partial class MrupayML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration Configuration;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;

        public MrupayML(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            bool IsProd = _env.IsProduction();
            var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile((IsProd ? "appsettings.json" : "appsettings.Development.json"));
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
        }
        private string GetApiDetail()
        {
            try
            {
                return Configuration["DMR:MRUY:BaseURL"];
            }
            catch
            {
            }
            return string.Empty;
        }
        public ResponseStatus CheckSender(DMTReq _req)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            string url = GetApiDetail().Replace("{fname}", "Check_Sender");
            string response = string.Empty, request = url + "&SenderMobileNo=" + _req.SenderNO + "&OutletID=" + _req.APIOutletID + "&SessionId=";
            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_GET(request);
                DMR_Common _apiRes = JsonConvert.DeserializeObject<DMR_Common>(response);
                if (_apiRes.status == "SUCCESS")
                {
                    res = GetSenderLimit(_req);
                }
                else if (_apiRes.status == "FAILED")
                {
                    if (_apiRes.msg == "Sender Not Exist")
                    {
                        res.Statuscode = ErrorCodes.One;
                        res.Msg = nameof(DMTErrorCodes.Sender_Not_Found).Replace("_", " ");
                        res.CommonInt = ErrorCodes.One;
                        res.ErrorCode = DMTErrorCodes.Sender_Not_Found;
                    }
                    else
                    {
                        res.Msg = _apiRes.msg;
                    }
                }
                else
                {
                    res.Msg = _apiRes.msg;
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
                    LoginTypeID = _req.LT,
                    UserId = _req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            var dMTReq = new DMTReqRes
            {
                APIID = _req.ApiID,
                Method = "CheckSender",
                RequestModeID = _req.RequestMode,
                Request = request,
                Response = response,
                SenderNo = _req.SenderNO,
                UserID = _req.UserID,
                TID = _req.TID
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            return res;
        }
        private ResponseStatus GetSenderLimit(DMTReq _req)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            string url = GetApiDetail().Replace("{fname}", "Get_Sender_Limit");
            string response = string.Empty, request = url + "&SenderMobileNo=" + _req.SenderNO + "&OutletID=" + _req.APIOutletID;
            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_GET(request);
                var _apiRes = JsonConvert.DeserializeObject<SenderLimit>(response);
                if (_apiRes.status == "SUCCESS")
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = nameof(DMTErrorCodes.Detail_Found_Successfully).Replace("_", " ");
                    res.ErrorCode = DMTErrorCodes.Detail_Found_Successfully;
                    res.CommonStr = _apiRes.Remaining_Limit;
                    res.CommonStr2 = _req.SenderNO;
                }
                else
                {
                    res.Msg = _apiRes.msg;
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GetSenderLimit",
                    Error = ex.Message,
                    LoginTypeID = _req.LT,
                    UserId = _req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            var dMTReq = new DMTReqRes
            {
                APIID = _req.ApiID,
                Method = "GetSenderLimit",
                RequestModeID = _req.RequestMode,
                Request = request,
                Response = response,
                SenderNo = _req.SenderNO,
                UserID = _req.UserID,
                TID = _req.TID
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            return res;
        }

        public ResponseStatus SenderOTPResend(DMTReq _req)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            string url = GetApiDetail().Replace("{fname}", "Sender_Create_Resend_OTP");
            string response = "", request = "";
            try
            {

                SenderRequest senderRequest = (SenderRequest)new ProcGetSender(_dal).Call(_req.SenderNO);
                if (string.IsNullOrWhiteSpace(senderRequest.Name))
                {
                    res.Msg = ErrorCodes.DMTSNE;
                    return res;
                }
                request = url + "&SenderMobileNo=" + _req.SenderNO + "&OutletID=" + _req.APIOutletID + "&OTP_Reference=" + senderRequest.ReffID;
                response = AppWebRequest.O.CallUsingHttpWebRequest_GET(request);
                DMR_Common _apiRes = JsonConvert.DeserializeObject<DMR_Common>(response);
                if (_apiRes.status == "SUCCESS")
                {
                    res = GetSenderLimit(_req);
                }
                else if (_apiRes.status == "FAILED")
                {
                    if (_apiRes.msg == "Sender Not Exist")
                    {
                        res.Statuscode = ErrorCodes.One;
                        res.Msg = ErrorCodes.DMTSNE;
                        res.CommonInt = ErrorCodes.One;
                    }
                    else
                    {
                        res.Msg = _apiRes.msg;
                    }
                }
                else
                {
                    res.Msg = _apiRes.msg;
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "SenderOTPResend",
                    Error = ex.Message,
                    LoginTypeID = _req.LT,
                    UserId = _req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            DMTReqRes dMTReq = new DMTReqRes
            {
                APIID = _req.ApiID,
                Method = "CheckSander",
                RequestModeID = _req.RequestMode,
                Request = request + "|" + JsonConvert.SerializeObject(_req),
                Response = response + "|" + JsonConvert.SerializeObject(res),
                SenderNo = _req.SenderNO,
                UserID = _req.UserID,
                TID = _req.TID
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            return res;
        }
        public ResponseStatus CreateSender(CreateSen _req)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            string url = GetApiDetail().Replace("{fname}", "Create_Sender");
            string response = "", request = "";
            var param = new MRuyCreateSender
            {
                Address = _req.senderRequest.Address,
                FirstName = _req.senderRequest.Name,
                LastName = _req.senderRequest.LastName,
                Area = _req.senderRequest.Area,
                Mobileno = _req.senderRequest.MobileNo,
                pincode = _req.senderRequest.Pincode
            };
            try
            {
                request = url + "&SenderMobileNo=" + _req.dMTReq.SenderNO + "&OutletID=" + _req.dMTReq.APIOutletID + "&json_Sender_Detail=" + JsonConvert.SerializeObject(param);
                response = AppWebRequest.O.CallUsingHttpWebRequest_GET(request);
                MRuyCreateSenderRes _apiRes = JsonConvert.DeserializeObject<MRuyCreateSenderRes>(response);
                if (_apiRes.status == "SUCCESS")
                {
                    _req.senderRequest.UserID = _req.dMTReq.UserID;
                    _req.senderRequest.ReffID = _apiRes.OTP_Reference;
                    _req.senderRequest.RequestNo = _apiRes.LiveId;
                    new ProcUpdateSender(_dal).Call(_req.senderRequest);

                    res.Statuscode = ErrorCodes.One;
                    res.Msg = nameof(DMTErrorCodes.Partial_Registration_Please_verify_sender_with_OTP).Replace("_", " ");
                    res.ErrorCode = DMTErrorCodes.Partial_Registration_Please_verify_sender_with_OTP;
                    res.CommonInt = ErrorCodes.One;
                    res.ReffID = _apiRes.OTP_Reference;
                }
                else
                {
                    res.Msg = _apiRes.msg;
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
                    LoginTypeID = _req.dMTReq.LT,
                    UserId = _req.dMTReq.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            DMTReqRes dMTReq = new DMTReqRes
            {
                APIID = _req.dMTReq.ApiID,
                Method = "CreateSender",
                RequestModeID = _req.dMTReq.RequestMode,
                Request = request + "|" + JsonConvert.SerializeObject(_req),
                Response = response + "|" + JsonConvert.SerializeObject(res),
                SenderNo = _req.dMTReq.SenderNO,
                UserID = _req.dMTReq.UserID,
                TID = _req.dMTReq.TID
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            return res;
        }
        public ResponseStatus VerifySender(DMTReq _req, string OTP)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", ""),
                ErrorCode = ErrorCodes.Unknown_Error
            };

            string url = GetApiDetail().Replace("{fname}", "Sender_Verify");
            string response = "", request = "";
            try
            {
                SenderRequest senderRequest = (SenderRequest)new ProcGetSender(_dal).Call(_req.SenderNO);
                if (string.IsNullOrWhiteSpace(senderRequest.Name))
                {
                    res.Msg = ErrorCodes.DMTSNE;
                    return res;
                }
                request = url + "&SenderMobileNo=" + _req.SenderNO + "&OutletID=" + _req.APIOutletID + "&OTP=" + OTP + "&OTP_Reference=" + senderRequest.ReffID;
                response = AppWebRequest.O.CallUsingHttpWebRequest_GET(request);
                DMR_Common _apiRes = JsonConvert.DeserializeObject<DMR_Common>(response);
                if (_apiRes.status == "SUCCESS")
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = ErrorCodes.DMTSCS;
                }
                else
                {
                    res.Msg = _apiRes.msg;
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "VerifySender",
                    Error = ex.Message,
                    LoginTypeID = _req.LT,
                    UserId = _req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            DMTReqRes dMTReq = new DMTReqRes
            {
                APIID = _req.ApiID,
                Method = "VerifySender",
                RequestModeID = _req.RequestMode,
                Request = request + "|" + JsonConvert.SerializeObject(_req),
                Response = response + "|" + JsonConvert.SerializeObject(res),
                SenderNo = _req.SenderNO,
                UserID = _req.UserID,
                TID = _req.TID
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            return res;
        }
        public ResponseStatus CreateBeneficiary(AddBeni addBeni, DMTReq _req)
        {
            string url = GetApiDetail().Replace("{fname}", "Add_Beneficiary");
            string response = "", request = "";
            ResponseStatus res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };

            try
            {
                var param = new MryAddGetBeneficiary
                {
                    AccountNumber = addBeni.AccountNo,
                    BankName = addBeni.BankName,
                    AccountHolderName = addBeni.BeneName,
                    IFSC = addBeni.IFSC
                };
                request = url + "&SenderMobileNo=" + _req.SenderNO + "&OutletID=" + _req.APIOutletID + "&Json_Beneficiary_Detail=" + JsonConvert.SerializeObject(param);
                response = AppWebRequest.O.CallUsingHttpWebRequest_GET(request);
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<DMR_Common>(response);
                    if (_apiRes.status == "SUCCESS")
                    {
                        res.Statuscode = ErrorCodes.One;
                        res.Msg = ErrorCodes.BENESCS;
                        return res;
                    }
                    else
                    {
                        res.Msg = _apiRes.msg;
                    }
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "CreateBeneficiary",
                    Error = response,
                    LoginTypeID = _req.LT,
                    UserId = _req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            DMTReqRes dMTReq = new DMTReqRes
            {
                APIID = _req.ApiID,
                Method = "CreateBeneficiary",
                RequestModeID = _req.RequestMode,
                Request = request + "|" + JsonConvert.SerializeObject(_req),
                Response = response + "|" + JsonConvert.SerializeObject(res),
                SenderNo = _req.SenderNO,
                UserID = _req.UserID,
                TID = _req.TID
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            return res;
        }
        public BeniRespones GetBeneficiary(DMTReq _req)
        {
            var res = new BeniRespones
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            string url = GetApiDetail().Replace("{fname}", "Get_Beneficiary");
            string response = "", request = "";
            try
            {

                request = url + "&SenderMobileNo=" + _req.SenderNO + "&OutletID=" + _req.APIOutletID;
                response = AppWebRequest.O.CallUsingHttpWebRequest_GET(request);
                MryGetBeneficiary _ApiRes = JsonConvert.DeserializeObject<MryGetBeneficiary>(response);
                if (_ApiRes != null && (_ApiRes.status ?? string.Empty) == "SUCCESS")
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = ErrorCodes.SUCCESS;
                    List<AddBeni> ListBeni = new List<AddBeni>();
                    if (_ApiRes.Account_Detail != null)
                    {
                        foreach (var item in _ApiRes.Account_Detail)
                        {
                            var addBeni = new AddBeni
                            {
                                AccountNo = item.AccountNumber,
                                BankName = item.BankName,
                                IFSC = item.IFSCCode,
                                BeneName = item.BeneficiaryName,
                                MobileNo = item.BankName,
                                BeneID = item.BeneficiaryCode

                            };
                            ListBeni.Add(addBeni);
                        }
                    }

                    res.addBeni = ListBeni;
                }
                else
                {
                    res.Msg = _ApiRes.msg;
                    //IProcedure _proc = new PrcoGetErrorCode(_dal);
                    //res = (ResponseStatus)_proc.Call(commonReq);
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GetBeneficiary",
                    Error = ex.Message,
                    LoginTypeID = _req.LT,
                    UserId = _req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            DMTReqRes dMTReq = new DMTReqRes
            {
                APIID = _req.ApiID,
                Method = "GetBeneficiary",
                RequestModeID = _req.RequestMode,
                Request = request + "|" + JsonConvert.SerializeObject(_req),
                Response = response + "|" + JsonConvert.SerializeObject(res),
                SenderNo = _req.SenderNO,
                UserID = _req.UserID,
                TID = _req.TID
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            return res;
        }
        public ResponseStatus DeleteBeneficiary(DMTReq _req, string BeniID)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            string url = GetApiDetail().Replace("{fname}", "Delete_Beneficiary");
            string response = "", request = "";
            try
            {
                request = url + "&SenderMobileNo=" + _req.SenderNO + "&OutletID=" + _req.APIOutletID + "&BeneficiaryId=" + BeniID;
                response = AppWebRequest.O.CallUsingHttpWebRequest_GET(request);
                DMR_Common _apiRes = JsonConvert.DeserializeObject<DMR_Common>(response);
                if (_apiRes.status == "SUCCESS")
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = ErrorCodes.DMTDBS;

                }
                else
                {
                    res.Msg = _apiRes.msg;
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "DeleteBeneficiary",
                    Error = ex.Message,
                    LoginTypeID = _req.LT,
                    UserId = _req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            DMTReqRes dMTReq = new DMTReqRes
            {
                APIID = _req.ApiID,
                Method = "DeleteBeneficiary",
                RequestModeID = _req.RequestMode,
                Request = request + "|" + JsonConvert.SerializeObject(_req),
                Response = response + "|" + JsonConvert.SerializeObject(res),
                SenderNo = _req.SenderNO,
                UserID = _req.UserID,
                TID = _req.TID
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            return res;
        }
        public DMRTransactionResponse SendMoney(DMTReq _req, ReqSendMoney sendMoney, DMRTransactionResponse res)
        {
            res.Statuscode = RechargeRespType.PENDING;
            res.Msg = RechargeRespType._PENDING;
            string url = GetApiDetail().Replace("{fname}", "SendMoney");
            string response = "", request = "";
            try
            {
                MrySendMoney param = new MrySendMoney
                {
                    AccountNumber = sendMoney.AccountNo,
                    Amount = sendMoney.Amount.ToString(),
                    BeneficiaryId = sendMoney.BeneID,
                    Channel = sendMoney.Channel ? "IMPS" : "NEFT",
                    RequestId = _req.TID
                };
                request = request = url + "&SenderMobileNo=" + _req.SenderNO + "&OutletID=" + _req.APIOutletID + "&Json_Account_Detail=" + JsonConvert.SerializeObject(param); ;
                response = AppWebRequest.O.CallUsingHttpWebRequest_GET(request);
                MryRespSendMoney _apiRes = JsonConvert.DeserializeObject<MryRespSendMoney>(response);
                if (_apiRes.status == RechargeRespType._SUCCESS)
                {
                    res.Statuscode = RechargeRespType.SUCCESS;
                    res.Msg = _apiRes.msg;
                    res.LiveID = _apiRes.LiveId ?? "";
                    res.VendorID = _apiRes.TransactionId;
                }
                else if (_apiRes.status == RechargeRespType._FAILED)
                {
                    res.Statuscode = RechargeRespType.FAILED;
                    res.Msg = _apiRes.msg;
                    res.LiveID = (_apiRes.LiveId ?? string.Empty).Trim().Equals(string.Empty) ? _apiRes.msg : _apiRes.LiveId ?? string.Empty;
                    if (res.LiveID.ToLower().Contains("nsuffici"))
                    {
                        res.Statuscode = RechargeRespType.PENDING;
                        res.Msg = nameof(ErrorCodes.Request_Accpeted).Replace("_", " ");
                        res.ErrorCode = ErrorCodes.Request_Accpeted;
                        res.LiveID = res.Msg;
                    }

                    res.VendorID = _apiRes.TransactionId;
                }
                else
                {
                    res.Statuscode = RechargeRespType.PENDING;
                    res.Msg = _apiRes.msg;
                }
                res.Request = request;
                res.Response = response;
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "SendMoney",
                    Error = ex.Message,
                    LoginTypeID = _req.LT,
                    UserId = _req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            var dMTReq = new DMTReqRes
            {
                APIID = _req.ApiID,
                Method = "SendMoney",
                RequestModeID = _req.RequestMode,
                Request = request + "|" + JsonConvert.SerializeObject(_req),
                Response = response + "|" + JsonConvert.SerializeObject(res),
                SenderNo = _req.SenderNO,
                UserID = _req.UserID,
                TID = _req.TID
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            return res;
        }
        public DMRTransactionResponse Verification(DMTReq _req, ReqSendMoney sendMoney, DMRTransactionResponse res)
        {
            res.Statuscode = RechargeRespType.PENDING;
            res.Msg = RechargeRespType._PENDING;
            res.VendorID = "";
            res.LiveID = "";
            string url = GetApiDetail().Replace("{fname}", "Verify_Beneficiary");
            string response = "", request = "";
            try
            {
                var param = new MryVerification
                {
                    AccountNumber = sendMoney.AccountNo,
                    IFSC_Or_BankCode = sendMoney.IFSC,
                    RequestId = _req.TID
                };
                request = request = url + "&SenderMobileNo=" + _req.SenderNO + "&OutletID=" + _req.APIOutletID + "&Json_Account_Detail=" + JsonConvert.SerializeObject(param); ;
                response = AppWebRequest.O.CallUsingHttpWebRequest_GET(request);
                MryVerificationResp _apiRes = JsonConvert.DeserializeObject<MryVerificationResp>(response);
                if (_apiRes.status == "SUCCESS")
                {
                    res.Statuscode = RechargeRespType.SUCCESS;
                    res.Msg = _apiRes.msg;
                    res.BeneName = _apiRes.AccountHolderName ?? "";

                }
                else if (_apiRes.status == "FAILED")
                {
                    res.Statuscode = RechargeRespType.FAILED;
                    res.Msg = _apiRes.msg;
                    res.LiveID = _apiRes.msg;
                    if (res.LiveID.ToLower().Contains("nsuffici"))
                    {
                        res.Statuscode = RechargeRespType.PENDING;
                        res.Msg = nameof(ErrorCodes.Request_Accpeted).Replace("_", " ");
                        res.ErrorCode = ErrorCodes.Request_Accpeted;
                        res.LiveID = res.Msg;
                    }
                }
                else
                {
                    res.Statuscode = RechargeRespType.PENDING;
                    res.Msg = _apiRes.msg;
                }
                res.Request = request;
                res.Response = response;
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Verification",
                    Error = ex.Message,
                    LoginTypeID = _req.LT,
                    UserId = _req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            DMTReqRes dMTReq = new DMTReqRes
            {
                APIID = _req.ApiID,
                Method = "Verification",
                RequestModeID = _req.RequestMode,
                Request = request + "|" + JsonConvert.SerializeObject(_req),
                Response = response + "|" + JsonConvert.SerializeObject(res),
                SenderNo = _req.SenderNO,
                UserID = _req.UserID,
                TID = _req.TID
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            return res;
        }
    }

    public partial class MrupayML : IMoneyTransferAPIML
    {
        public MSenderLoginResponse GetSender(MTAPIRequest request)
        {
            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            string _URL = GetApiDetail().Replace("{fname}", "Check_Sender") + "&SenderMobileNo=" + request.SenderMobile + "&OutletID=" + request.APIOutletID + "&SessionId=";
            string response = string.Empty;
            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_GET(_URL);
                DMR_Common _apiRes = JsonConvert.DeserializeObject<DMR_Common>(response);
                if (_apiRes.status == "SUCCESS")
                {
                    res = GetSenderLimit(request);
                    res.ErrorCode = ErrorCodes.Transaction_Successful;
                    res.KYCStatus = SenderKYCStatus.ACTIVE;
                }
                else if (_apiRes.status == "FAILED")
                {
                    if (_apiRes.msg == "Sender Not Exist")
                    {
                        res.Statuscode = ErrorCodes.One;
                        res.Msg = nameof(DMTErrorCodes.Sender_Not_Found).Replace("_", " ");
                        res.IsSenderNotExists = true;
                        res.ErrorCode = DMTErrorCodes.Sender_Not_Found;
                    }
                    else
                    {
                        res.Msg = _apiRes.msg;
                    }
                }
                else
                {
                    res.Msg = _apiRes.msg;
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
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
            var dMTReq = new DMTReqRes
            {
                APIID = request.APIID,
                Method = "GetSender",
                RequestModeID = request.RequestMode,
                Request = _URL,
                Response = response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            return res;
        }
        private MSenderLoginResponse GetSenderLimit(MTAPIRequest request)
        {
            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            string _URL = GetApiDetail().Replace("{fname}", "Get_Sender_Limit") + "&SenderMobileNo=" + request.SenderMobile + "&OutletID=" + request.APIOutletID + "&SessionId=";
            string response = string.Empty;
            try
            {
                response = AppWebRequest.O.CallUsingHttpWebRequest_GET(_URL);
                var _apiRes = JsonConvert.DeserializeObject<SenderLimit>(response);
                if (_apiRes.status == "SUCCESS")
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = nameof(DMTErrorCodes.Detail_Found_Successfully).Replace("_", " ");
                    res.ErrorCode = DMTErrorCodes.Detail_Found_Successfully;
                    res.RemainingLimit = string.IsNullOrEmpty(_apiRes.Remaining_Limit) ? 0 : Convert.ToDecimal(_apiRes.Remaining_Limit);
                }
                else
                {
                    res.Msg = _apiRes.msg;
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GetSenderLimit",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            var dMTReq = new DMTReqRes
            {
                APIID = request.APIID,
                Method = "GetSenderLimit",
                RequestModeID = request.RequestMode,
                Request = _URL,
                Response = response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            return res;
        }
        public MSenderCreateResp CreateSender(MTAPIRequest request)
        {
            var res = new MSenderCreateResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Unknown_Error
            };
            string _URL = GetApiDetail().Replace("{fname}", "Create_Sender");
            string response = "";
            var param = new MRuyCreateSender
            {
                Address = request.Address,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Area = request.Area,
                Mobileno = request.SenderMobile,
                pincode = request.Pincode.ToString()
            };
            try
            {
                _URL = _URL + "&SenderMobileNo=" + request.SenderMobile + "&OutletID=" + request.APIOutletID + "&json_Sender_Detail=" + JsonConvert.SerializeObject(param);
                response = AppWebRequest.O.CallUsingHttpWebRequest_GET(_URL);
                var _apiRes = JsonConvert.DeserializeObject<MRuyCreateSenderRes>(response);
                if (_apiRes.status == "SUCCESS")
                {

                    new ProcUpdateSender(_dal).Call(new SenderRequest
                    {
                        Name = request.FirstName + " " + request.LastName,
                        MobileNo = request.SenderMobile,
                        Pincode = request.Pincode.ToString(),
                        Address = request.Address,
                        City = request.City,
                        StateID = request.StateID,
                        AadharNo = request.AadharNo,
                        Dob = request.DOB,
                        UserID = request.UserID,
                        ReffID = _apiRes.OTP_Reference,
                        RequestNo = _apiRes.LiveId
                    });

                    res.Statuscode = ErrorCodes.One;
                    res.Msg = nameof(DMTErrorCodes.Partial_Registration_Please_verify_sender_with_OTP).Replace("_", " ");
                    res.ErrorCode = DMTErrorCodes.Partial_Registration_Please_verify_sender_with_OTP;
                    res.ReferenceID = _apiRes.OTP_Reference;
                    res.IsOTPGenerated = true;
                }
                else
                {
                    res.Msg = _apiRes.msg;
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
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = request.APIID,
                Method = "CreateSender",
                RequestModeID = request.RequestMode,
                Request = _URL + "|" + JsonConvert.SerializeObject(request),
                Response = response + "|" + JsonConvert.SerializeObject(param),
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            });
            return res;
        }
        public MSenderLoginResponse CreateBeneficiary(MTAPIRequest request)
        {
            string _URL = GetApiDetail().Replace("{fname}", "Add_Beneficiary");
            string response = "";
            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var param = new MryAddGetBeneficiary
            {
                AccountNumber = request.mBeneDetail.AccountNo,
                BankName = request.mBeneDetail.BankName,
                AccountHolderName = request.mBeneDetail.BeneName,
                IFSC = request.mBeneDetail.IFSC
            };
            try
            {

                _URL = _URL + "&SenderMobileNo=" + request.SenderMobile + "&OutletID=" + request.APIOutletID + "&Json_Beneficiary_Detail=" + JsonConvert.SerializeObject(param);
                response = AppWebRequest.O.CallUsingHttpWebRequest_GET(_URL);
                if (!string.IsNullOrEmpty(response))
                {
                    var _apiRes = JsonConvert.DeserializeObject<DMR_Common>(response);
                    if (_apiRes.status == "SUCCESS")
                    {
                        res.Statuscode = ErrorCodes.One;
                        res.Msg = ErrorCodes.BENESCS;
                        res.ErrorCode = ErrorCodes.Transaction_Successful;
                        return res;
                    }
                    else
                    {
                        res.Msg = _apiRes.msg;
                    }
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "CreateBeneficiary",
                    Error = response,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = request.APIID,
                Method = "CreateBeneficiary",
                RequestModeID = request.RequestMode,
                Request = _URL,
                Response = response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            });
            return res;
        }
        public MSenderCreateResp GenerateOTP(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }
        public MBeneficiaryResp GetBeneficiary(MTAPIRequest request)
        {
            var res = new MBeneficiaryResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(DMTErrorCodes.Beneficiary_not_found).Replace("_", " "),
                ErrorCode = DMTErrorCodes.Beneficiary_not_found
            };
            string _URL = GetApiDetail().Replace("{fname}", "Get_Beneficiary");
            string response = "";
            try
            {

                _URL = _URL + "&SenderMobileNo=" + request.SenderMobile + "&OutletID=" + request.APIOutletID;
                response = AppWebRequest.O.CallUsingHttpWebRequest_GET(_URL);
                var _ApiRes = JsonConvert.DeserializeObject<MryGetBeneficiary>(response);
                if (_ApiRes != null && (_ApiRes.status ?? string.Empty) == "SUCCESS")
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                    res.ErrorCode = ErrorCodes.Transaction_Successful;
                    var Beneficiaries = new List<MBeneDetail>();
                    if (_ApiRes.Account_Detail != null)
                    {
                        foreach (var item in _ApiRes.Account_Detail)
                        {
                            Beneficiaries.Add(new MBeneDetail
                            {
                                AccountNo = item.AccountNumber,
                                BankName = item.BankName,
                                IFSC = item.IFSCCode,
                                BeneName = item.BeneficiaryName,
                                MobileNo = item.BankName,
                                BeneID = item.BeneficiaryCode

                            });
                        }
                    }

                    res.Beneficiaries = Beneficiaries;
                }
                else
                {
                    res.Msg = _ApiRes.msg;
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GetBeneficiary",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = request.APIID,
                Method = "GetBeneficiary",
                RequestModeID = request.RequestMode,
                Request = _URL,
                Response = response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            });
            return res;
        }
        public MSenderCreateResp SenderKYC(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }
        public MSenderCreateResp SenderResendOTP(MTAPIRequest request)
        {
            var senderCreateResp = new MSenderCreateResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            string _URL = GetApiDetail().Replace("{fname}", "Sender_Create_Resend_OTP");
            string response = string.Empty;
            try
            {

                var senderRequest = (SenderRequest)new ProcGetSender(_dal).Call(request.SenderMobile);
                if (string.IsNullOrWhiteSpace(senderRequest.Name))
                {
                    senderCreateResp.Msg = nameof(DMTErrorCodes.Sender_Not_Found).Replace("_", " ");
                    senderCreateResp.ErrorCode = DMTErrorCodes.Sender_Not_Found;
                    return senderCreateResp;
                }
                _URL = _URL + "&SenderMobileNo=" + request.SenderMobile + "&OutletID=" + request.APIOutletID + "&OTP_Reference=" + senderRequest.ReffID;
                response = AppWebRequest.O.CallUsingHttpWebRequest_GET(_URL);
                var _apiRes = JsonConvert.DeserializeObject<DMR_Common>(response);
                if (_apiRes.status == "SUCCESS")
                {
                    senderCreateResp.Msg = nameof(DMTErrorCodes.Sender_Already_Exist).Replace("_", " ");
                    senderCreateResp.ErrorCode = DMTErrorCodes.Sender_Already_Exist;
                }
                else if (_apiRes.status == "FAILED")
                {
                    if (_apiRes.msg == "Sender Not Exist")
                    {
                        senderCreateResp.Statuscode = ErrorCodes.One;
                        senderCreateResp.Msg = nameof(DMTErrorCodes.Sender_Not_Found).Replace("_", " ");
                        senderCreateResp.ErrorCode = DMTErrorCodes.Sender_Not_Found;
                        senderCreateResp.IsOTPGenerated = true;
                    }
                    else
                    {
                        senderCreateResp.Msg = _apiRes.msg;
                    }
                }
                else
                {
                    senderCreateResp.Msg = _apiRes.msg;
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "SenderResendOTP",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = request.APIID,
                Method = "SenderResendOTP",
                RequestModeID = request.RequestMode,
                Request = _URL,
                Response = JsonConvert.SerializeObject(response),
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            });
            return senderCreateResp;
        }
        public MSenderLoginResponse ValidateBeneficiary(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }
        public MSenderLoginResponse VerifySender(MTAPIRequest request)
        {
            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", ""),
                ErrorCode = ErrorCodes.Unknown_Error
            };

            string _URL = GetApiDetail().Replace("{fname}", "Sender_Verify");
            string response = string.Empty;
            try
            {
                var senderRequest = (SenderRequest)new ProcGetSender(_dal).Call(request.SenderMobile);
                if (string.IsNullOrWhiteSpace(senderRequest.Name))
                {
                    res.Msg = ErrorCodes.DMTSNE;
                    return res;
                }
                _URL = _URL + "&SenderMobileNo=" + request.SenderMobile + "&OutletID=" + request.APIOutletID + "&OTP=" + request.OTP + "&OTP_Reference=" + senderRequest.ReffID;
                response = AppWebRequest.O.CallUsingHttpWebRequest_GET(_URL);
                var _apiRes = JsonConvert.DeserializeObject<DMR_Common>(response);
                if (_apiRes.status == "SUCCESS")
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = ErrorCodes.DMTSCS;
                    res.ErrorCode = ErrorCodes.Transaction_Successful;
                }
                else
                {
                    res.Msg = nameof(ErrorCodes.Invalid_OTP).Replace("_", " ");
                    res.ErrorCode = ErrorCodes.Invalid_OTP;
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "VerifySender",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = request.APIID,
                Method = "VerifySender",
                RequestModeID = request.RequestMode,
                Request = _URL,
                Response = response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            });
            return res;
        }

        public MSenderLoginResponse RemoveBeneficiary(MTAPIRequest request)
        {
            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError,
                ErrorCode = ErrorCodes.Unknown_Error
            };
            string _URL = GetApiDetail().Replace("{fname}", "Delete_Beneficiary");
            string response = "";
            try
            {
                _URL = _URL + "&SenderMobileNo=" + request.SenderMobile + "&OutletID=" + request.APIOutletID + "&BeneficiaryId=" + request.mBeneDetail.BeneID;
                response = AppWebRequest.O.CallUsingHttpWebRequest_GET(_URL);
                var _apiRes = JsonConvert.DeserializeObject<DMR_Common>(response);
                if (_apiRes.status == "SUCCESS")
                {
                    res.Statuscode = ErrorCodes.One;
                    res.Msg = ErrorCodes.DMTDBS;
                    res.ErrorCode = ErrorCodes.Transaction_Successful;
                }
                else
                {
                    res.Msg = _apiRes.msg;
                }
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "RemoveBeneficiary",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = request.APIID,
                Method = "RemoveBeneficiary",
                RequestModeID = request.RequestMode,
                Request = _URL,
                Response = response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            });
            return res;
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
            string _URL = GetApiDetail().Replace("{fname}", "Verify_Beneficiary");
            var param = new MryVerification
            {
                AccountNumber = request.mBeneDetail.AccountNo,
                IFSC_Or_BankCode = request.mBeneDetail.IFSC,
                RequestId = request.TID.ToString()
            };

            string response = "";
            try
            {

                _URL = _URL + "&SenderMobileNo=" + request.SenderMobile + "&OutletID=" + request.APIOutletID + "&Json_Account_Detail=" + JsonConvert.SerializeObject(param); ;
                response = AppWebRequest.O.CallUsingHttpWebRequest_GET(_URL);
                MryVerificationResp _apiRes = JsonConvert.DeserializeObject<MryVerificationResp>(response);
                if (_apiRes.status == "SUCCESS")
                {
                    res.Statuscode = RechargeRespType.SUCCESS;
                    res.Msg = _apiRes.msg;
                    res.BeneName = _apiRes.AccountHolderName ?? "";

                }
                else if (_apiRes.status == "FAILED")
                {
                    res.Statuscode = RechargeRespType.FAILED;
                    res.Msg = _apiRes.msg;
                    res.LiveID = _apiRes.msg;
                    if (res.LiveID.ToLower().Contains("nsuffici"))
                    {
                        res.Statuscode = RechargeRespType.PENDING;
                        res.Msg = nameof(ErrorCodes.Request_Accpeted).Replace("_", " ");
                        res.ErrorCode = ErrorCodes.Request_Accpeted;
                        res.LiveID = res.Msg;
                    }
                }
                else
                {
                    res.Statuscode = RechargeRespType.PENDING;
                    res.Msg = _apiRes.msg;
                }
                res.Request = _URL;
                res.Response = response;
            }
            catch (Exception ex)
            {
                response = ex.Message + "|" + response;
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "VerifyAccount",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            res.Request = _URL;
            res.Response = response;
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = request.APIID,
                Method = "VerifyAccount",
                RequestModeID = request.RequestMode,
                Request = res.Request,
                Response = res.Response,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TID.ToString()
            });
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
            string _URL = GetApiDetail().Replace("{fname}", "SendMoney");
            var param = new MrySendMoney
            {
                AccountNumber = request.mBeneDetail.AccountNo,
                Amount = request.Amount.ToString(),
                BeneficiaryId = request.mBeneDetail.BeneID,
                Channel = request.TransMode,
                RequestId = request.TID.ToString()
            };
            string response = string.Empty;
            try
            {
                _URL = _URL + "&SenderMobileNo=" + request.SenderMobile + "&OutletID=" + request.APIOutletID + "&Json_Account_Detail=" + JsonConvert.SerializeObject(param); ;
                response = AppWebRequest.O.CallUsingHttpWebRequest_GET(_URL);
                var _apiRes = JsonConvert.DeserializeObject<MryRespSendMoney>(response);
                if (_apiRes.status == RechargeRespType._SUCCESS)
                {
                    res.Statuscode = RechargeRespType.SUCCESS;
                    res.Msg = _apiRes.msg;
                    res.LiveID = _apiRes.LiveId ?? "";
                    res.VendorID = _apiRes.TransactionId;
                }
                else if (_apiRes.status == RechargeRespType._FAILED)
                {
                    res.Statuscode = RechargeRespType.FAILED;
                    res.Msg = _apiRes.msg;
                    res.LiveID = (_apiRes.LiveId ?? string.Empty).Trim().Equals(string.Empty) ? _apiRes.msg : _apiRes.LiveId ?? string.Empty;
                    if (res.LiveID.ToLower().Contains("nsuffici"))
                    {
                        res.Statuscode = RechargeRespType.PENDING;
                        res.Msg = nameof(ErrorCodes.Request_Accpeted).Replace("_", " ");
                        res.ErrorCode = ErrorCodes.Request_Accpeted;
                        res.LiveID = res.Msg;
                    }

                    res.VendorID = _apiRes.TransactionId;
                }
                else
                {
                    res.Statuscode = RechargeRespType.PENDING;
                    res.Msg = _apiRes.msg;
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
            res.Request = _URL;
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

        public MSenderLoginResponse RemoveBeneficiaryValidate(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }

        public MSenderCreateResp SenderEKYC(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
