using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.StaticModel;
using Fintech.AppCode.WebRequest;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.MoneyTransfer;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Validators;

namespace RoundpayFinTech.AppCode.ThirdParty.Mahagram
{
    public partial class MahagramAPIML
    {
        private readonly int _APIID;
        private readonly MahagramSetting _mahagramSetting;
        public MahagramAPIML(IHttpContextAccessor accessor, IHostingEnvironment env, IDAL dal, int APIID)
        {
            _accessor = accessor;
            _env = env;
            var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile((_env.IsProduction() ? "appsettings.json" : "appsettings.Development.json"));
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            _dal = dal;
            _mahagramSetting = MGAppSetting();
            _APIID = APIID;
        }
        private MahagramSetting MGAppSetting()
        {
            var mahagramSetting = new MahagramSetting();
            try
            {
                mahagramSetting.Salt = Configuration["DMR:MHAGM:Salt"];
                mahagramSetting.CP = Configuration["DMR:MHAGM:CP"];
                mahagramSetting.SECRET = Configuration["DMR:MHAGM:SECRET"];
                mahagramSetting.BaseURL = Configuration["DMR:MHAGM:BaseURL"];
                mahagramSetting.StatusCheckDMT = Configuration["DMR:MHAGM:StatusCheckDMT"];
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "MGAppSetting",
                    Error = "AppSetting not found:" + ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return mahagramSetting;
        }

        public async Task<ResponseStatus> GetMGSender(DMTReq _req)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            if (string.IsNullOrEmpty(_mahagramSetting.BaseURL))
                return res;
            var mgRequest = new MGDMTRequest
            {
                bc_id = _req.APIOutletID,
                custno = _req.SenderNO
            };
            string URL = _mahagramSetting.BaseURL + "getairtelbenedetails";
            string resp = string.Empty;
            try
            {
                resp = await AppWebRequest.O.PostJsonDataUsingHWRAsync(URL, mgRequest).ConfigureAwait(false);
                var mgResp = JsonConvert.DeserializeObject<MGDMTResponse>(resp);
                res.Msg = nameof(DMTErrorCodes.Sender_Not_Found).Replace("_", " ");
                res.CommonInt = ErrorCodes.One;
                res.ErrorCode = DMTErrorCodes.Sender_Not_Found;
                if (mgResp != null)
                {
                    if (!string.IsNullOrEmpty(mgResp.statuscode))
                    {
                        if (mgResp.statuscode.Equals("002") || mgResp.statuscode.Equals("111"))
                        {
                            res.Msg = nameof(DMTErrorCodes.Sender_Not_Found).Replace("_", " ");
                            res.CommonInt = ErrorCodes.One;
                            res.ErrorCode = DMTErrorCodes.Sender_Not_Found;
                        }
                        else if (mgResp.statuscode.Equals("001") || mgResp.statuscode.Equals("003"))
                        {
                            res.Msg = nameof(DMTErrorCodes.Detail_Found_Successfully).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Detail_Found_Successfully;
                            res.CommonStr = string.IsNullOrEmpty(mgResp.used_limit) ? "0" : mgResp.used_limit;
                            res.CommonStr2 = mgResp.custfirstname;
                            res.CommonInt = ErrorCodes.Minus1;
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
                    FuncName = "GetMGSender",
                    Error = ex.Message,
                    LoginTypeID = _req.LT,
                    UserId = _req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            var dMTReq = new DMTReqRes
            {
                APIID = _APIID,
                Method = "GetMGSender",
                RequestModeID = _req.RequestMode,
                Request = URL + JsonConvert.SerializeObject(mgRequest),
                Response = resp,
                SenderNo = _req.SenderNO,
                UserID = _req.UserID,
                TID = _req.TID
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            return res;
        }

        public async Task<BeniRespones> GetMGAirtelBeneDetails(DMTReq _req)
        {
            var res = new BeniRespones
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(DMTErrorCodes.Beneficiary_not_found).Replace("_", " "),
                ErrorCode = DMTErrorCodes.Beneficiary_not_found
            };
            if (string.IsNullOrEmpty(_mahagramSetting.BaseURL))
                return res;
            var mgRequest = new MGDMTRequest
            {
                bc_id = _req.APIOutletID,
                custno = _req.SenderNO
            };
            string URL = _mahagramSetting.BaseURL + "getairtelbenedetails";
            string resp = string.Empty;
            try
            {
                resp = await AppWebRequest.O.PostJsonDataUsingHWRAsync(URL, mgRequest).ConfigureAwait(false);
                var mgResp = JsonConvert.DeserializeObject<MGDMTResponse>(resp);
                if (mgResp != null)
                {
                    if (!string.IsNullOrEmpty(mgResp.statuscode))
                    {
                        if (mgResp.statuscode.Equals("001"))
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Transaction_Successful;

                            var ListBeni = new List<AddBeni>();
                            foreach (var item in mgResp.Data)
                            {
                                if (item.bankid != "")
                                {
                                    var addBeni = new AddBeni
                                    {
                                        AccountNo = item.beneaccno,
                                        BankName = item.bankname,
                                        IFSC = item.ifsc,
                                        BeneName = item.benename,
                                        MobileNo = item.benemobile,
                                        BeneID = item.id,
                                        IsVerified = !((item.status ?? string.Empty).ToUpper().Equals("NV")),
                                        BankID = Validate.O.IsNumeric(item.bankid) ? Convert.ToInt32(item.bankid) : 0
                                    };
                                    ListBeni.Add(addBeni);
                                }

                            }
                            res.addBeni = ListBeni;
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
                    FuncName = "GetAirtelBeneDetails",
                    Error = ex.Message,
                    LoginTypeID = _req.LT,
                    UserId = _req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }

            var dMTReq = new DMTReqRes
            {
                APIID = _APIID,
                Method = "GetAirtelBeneDetails",
                RequestModeID = _req.RequestMode,
                Request = URL + JsonConvert.SerializeObject(mgRequest),
                Response = resp,
                SenderNo = _req.SenderNO,
                UserID = _req.UserID,
                TID = _req.TID
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            return res;
        }

        public async Task<ResponseStatus> MGGenerateAirtelOTP(DMTReq _req)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            if (string.IsNullOrEmpty(_mahagramSetting.BaseURL))
                return res;
            var mgRequest = new MGDMTRequest
            {
                bc_id = _req.APIOutletID,
                custno = _req.SenderNO
            };
            string URL = _mahagramSetting.BaseURL + "airtelOTP";
            string resp = string.Empty;
            try
            {
                resp = await AppWebRequest.O.PostJsonDataUsingHWRAsync(URL, mgRequest).ConfigureAwait(false);
                var mgResp = JsonConvert.DeserializeObject<List<MGDMTResponse>>(resp);
                if (mgResp != null)
                {
                    if (mgResp.Count > 0)
                    {
                        if (!string.IsNullOrEmpty(mgResp[0].statuscode))
                        {
                            if (mgResp[0].statuscode.Equals("001"))
                            {
                                res.Statuscode = ErrorCodes.One;
                                res.Msg = nameof(DMTErrorCodes.OTP_for_verification_has_been_sent_successfully).Replace("_", " ");
                                res.ErrorCode = DMTErrorCodes.OTP_for_verification_has_been_sent_successfully;
                                res.CommonInt = ErrorCodes.One;
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
                    FuncName = "MGGenerateAirtelOTP",
                    Error = ex.Message,
                    LoginTypeID = _req.LT,
                    UserId = _req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            var dMTReq = new DMTReqRes
            {
                APIID = _APIID,
                Method = "MGGenerateAirtelOTP",
                RequestModeID = _req.RequestMode,
                Request = URL + JsonConvert.SerializeObject(mgRequest),
                Response = resp,
                SenderNo = _req.SenderNO,
                UserID = _req.UserID,
                TID = _req.TID
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            return res;
        }

        public async Task<ResponseStatus> MGReGenerateAirtelOTP(DMTReq _req)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            if (string.IsNullOrEmpty(_mahagramSetting.BaseURL))
                return res;
            var mgRequest = new MGDMTRequest
            {
                bc_id = _req.APIOutletID,
                custno = _req.SenderNO
            };
            string URL = _mahagramSetting.BaseURL + "airtelResendOTP" +
                "";
            string resp = string.Empty;
            try
            {
                resp = await AppWebRequest.O.PostJsonDataUsingHWRAsync(URL, mgRequest).ConfigureAwait(false);
                var mgResp = JsonConvert.DeserializeObject<List<MGDMTResponse>>(resp);
                if (mgResp != null)
                {
                    if (mgResp.Count > 0)
                    {
                        if (string.IsNullOrEmpty(mgResp[0].statuscode))
                        {
                            if (mgResp[0].statuscode.Equals("001"))
                            {
                                res.Statuscode = ErrorCodes.One;
                                res.Msg = nameof(DMTErrorCodes.OTP_for_verification_has_been_sent_successfully).Replace("_", " ");
                                res.ErrorCode = DMTErrorCodes.OTP_for_verification_has_been_sent_successfully;
                                res.CommonInt = ErrorCodes.One;
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
                    FuncName = "MGReGenerateAirtelOTP",
                    Error = ex.Message,
                    LoginTypeID = _req.LT,
                    UserId = _req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            var dMTReq = new DMTReqRes
            {
                APIID = _APIID,
                Method = "MGReGenerateAirtelOTP",
                RequestModeID = _req.RequestMode,
                Request = URL + JsonConvert.SerializeObject(mgRequest),
                Response = resp,
                SenderNo = _req.SenderNO,
                UserID = _req.UserID,
                TID = _req.TID
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            return res;
        }

        public async Task<ResponseStatus> MGAPICustRegistration(CreateSen createSen)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            if (string.IsNullOrEmpty(_mahagramSetting.BaseURL))
                return res;
            if (string.IsNullOrEmpty(createSen.OTP))
            {
                res.Statuscode = ErrorCodes.Minus1;
                res.Msg = nameof(ErrorCodes.Invalid_OTP).Replace("_", " ");
                res.ErrorCode = ErrorCodes.Invalid_OTP;
                return res;
            }
            var mgRequest = new MGDMTRequest
            {
                saltkey = _mahagramSetting.Salt,
                secretkey = _mahagramSetting.SECRET,
                bc_id = createSen.dMTReq.APIOutletID,
                cust_f_name = createSen.senderRequest.Name,
                cust_l_name = createSen.senderRequest.LastName,
                custno = createSen.senderRequest.MobileNo,
                pincode = createSen.senderRequest.Pincode,
                otp = createSen.OTP,
                Dob = createSen.senderRequest.Dob,
                Address = "Address - " + (createSen.senderRequest.Pincode ?? string.Empty),
                StateCode= createSen.senderRequest.MahagramStateCode
            };

            string URL = _mahagramSetting.BaseURL + "apiCustRegistration";
            string resp = string.Empty;
            try
            {
                resp = await AppWebRequest.O.PostJsonDataUsingHWRAsync(URL, mgRequest).ConfigureAwait(false);
                var mgResp = JsonConvert.DeserializeObject<List<MGDMTResponse>>(resp);
                if (mgResp != null)
                {
                    if (!string.IsNullOrEmpty(mgResp[0].statuscode))
                    {
                        if (mgResp[0].statuscode.Equals("000"))
                        {
                            res.Msg = nameof(ErrorCodes.Invalid_OTP).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Invalid_OTP;
                        }
                        else if (mgResp[0].statuscode.Equals("001"))
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = nameof(DMTErrorCodes.Sender_Created_Successfully).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Sender_Created_Successfully;
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
                    FuncName = "MGAPICustRegistration",
                    Error = ex.Message,
                    LoginTypeID = createSen.dMTReq.LT,
                    UserId = createSen.dMTReq.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            var dMTReq = new DMTReqRes
            {
                APIID = _APIID,
                Method = "MGAPICustRegistration",
                RequestModeID = createSen.dMTReq.RequestMode,
                Request = URL + JsonConvert.SerializeObject(mgRequest),
                Response = resp,
                SenderNo = createSen.dMTReq.SenderNO,
                UserID = createSen.dMTReq.UserID,
                TID = createSen.dMTReq.TID
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            return res;
        }

        public async Task<ResponseStatus> MGAirtelBeneAdd(AddBeni addBeni, DMTReq _req)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            if (string.IsNullOrEmpty(_mahagramSetting.BaseURL))
                return res;
            IBankML bankML = new BankML(_accessor, _env, false);
            var BankDetail = bankML.BankMasters(addBeni.BankID);
            if (BankDetail.Mahagram_BankID < 1)
            {
                res.Statuscode = RechargeRespType.FAILED;
                res.Msg = nameof(DMTErrorCodes.Acquiring_Bank_CBS_or_node_offline).Replace("_", " ");
                res.ErrorCode = DMTErrorCodes.Acquiring_Bank_CBS_or_node_offline;
                return res;
            }
            var mgRequest = new MGDMTRequest
            {
                bankname = BankDetail.Mahagram_BankID.ToString(),
                beneaccno = addBeni.AccountNo,
                benemobile = addBeni.MobileNo,
                benename = addBeni.BeneName,
                custno = _req.SenderNO,
                ifsc = addBeni.IFSC,
                bc_id = _req.APIOutletID
            };
            string URL = _mahagramSetting.BaseURL + "airtelbeneadd";
            string resp = string.Empty;
            try
            {
                resp = await AppWebRequest.O.PostJsonDataUsingHWRAsync(URL, mgRequest);
                var mgResp = JsonConvert.DeserializeObject<MGDMTResponse>(resp);
                if (mgResp != null)
                {
                    if (!string.IsNullOrEmpty(mgResp.statuscode))
                    {
                        if (mgResp.statuscode.Equals("001"))
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = nameof(DMTErrorCodes.Beneficiary_Added_Successfully).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Beneficiary_Added_Successfully;
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
                    FuncName = "MGAirtelBeneAdd",
                    Error = ex.Message,
                    LoginTypeID = _req.LT,
                    UserId = _req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            var dMTReq = new DMTReqRes
            {
                APIID = _APIID,
                Method = "MGAirtelBeneAdd",
                RequestModeID = _req.RequestMode,
                Request = URL + JsonConvert.SerializeObject(mgRequest),
                Response = resp,
                SenderNo = _req.SenderNO,
                UserID = _req.UserID,
                TID = _req.TID
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            return res;
        }

        public async Task<ResponseStatus> MGVerifyBeneOTP(DMTReq _req, string BeneMobile, string AccountNo, string OTP)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            if (string.IsNullOrEmpty(_mahagramSetting.BaseURL))
                return res;
            if (string.IsNullOrEmpty(OTP))
            {
                res.Msg = nameof(ErrorCodes.Invalid_OTP).Replace("_", " ");
                res.ErrorCode = ErrorCodes.Invalid_OTP;
                return res;
            }
            var mgRequest = new MGDMTRequest
            {
                beneaccno = AccountNo,
                benemobile = BeneMobile,
                custno = _req.SenderNO,
                otp = OTP
            };
            string URL = _mahagramSetting.BaseURL + "verifybeneotp";
            string resp = string.Empty;
            try
            {
                resp = await AppWebRequest.O.PostJsonDataUsingHWRAsync(URL, new { mgRequest.beneaccno, mgRequest.benemobile, mgRequest.custno, mgRequest.otp });
                var mgResp = JsonConvert.DeserializeObject<List<MGDMTResponse>>(resp);
                if (mgResp != null)
                {
                    if (mgResp.Count > 0)
                    {
                        if (!string.IsNullOrEmpty(mgResp[0].statuscode))
                        {
                            if (mgResp[0].statuscode.Equals("001"))
                            {
                                res.Statuscode = ErrorCodes.One;
                                res.Msg = nameof(DMTErrorCodes.OTP_verified_successfully).Replace("_", " ");
                                res.ErrorCode = DMTErrorCodes.OTP_verified_successfully;
                                res.CommonInt = ErrorCodes.One;
                            }
                            else if (mgResp[0].statuscode.Equals("000") || mgResp[0].statuscode.Equals("003"))
                            {
                                res.Msg = nameof(ErrorCodes.Invalid_OTP).Replace("_", " ");
                                res.ErrorCode = ErrorCodes.Invalid_OTP;
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
                    FuncName = "MGVerifyBeneOTP",
                    Error = ex.Message,
                    LoginTypeID = _req.LT,
                    UserId = _req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            var dMTReq = new DMTReqRes
            {
                APIID = _APIID,
                Method = "MGVerifyBeneOTP",
                RequestModeID = _req.RequestMode,
                Request = URL + JsonConvert.SerializeObject(mgRequest),
                Response = resp,
                SenderNo = _req.SenderNO,
                UserID = _req.UserID,
                TID = _req.TID
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            return res;
        }

        public async Task<DMRTransactionResponse> MGApiVerifybene(DMTReq _req, ReqSendMoney sendMoney, DMRTransactionResponse res)
        {
            res.Statuscode = RechargeRespType.PENDING;
            res.Msg = RechargeRespType._PENDING;
            res.VendorID = "";
            res.LiveID = "";
            if (string.IsNullOrEmpty(_mahagramSetting.BaseURL))
                return res;
            IBankML bankML = new BankML(_accessor, _env, false);
            var BankDetail = bankML.BankMasters(sendMoney.BankID);
            if (BankDetail.Mahagram_BankID < 1)
            {
                res.Statuscode = RechargeRespType.FAILED;
                res.Msg = nameof(DMTErrorCodes.Acquiring_Bank_CBS_or_node_offline).Replace("_", " ");
                res.ErrorCode = DMTErrorCodes.Acquiring_Bank_CBS_or_node_offline;
                return res;
            }
            var mgRequest = new MGDMTRequest
            {
                bankname = BankDetail.Mahagram_BankID.ToString(),
                beneaccno = sendMoney.AccountNo,
                benemobile = _req.SenderNO,
                benename = sendMoney.BeneName,
                custno = _req.SenderNO,
                ifsc = sendMoney.IFSC,
                clientrefno = _req.TID,
                bc_id = _req.APIOutletID,
                secretkey = _mahagramSetting.SECRET,
                saltkey = _mahagramSetting.Salt
            };
            //string URL = _mahagramSetting.BaseURL + "ApiVerifybene";//staging
            string URL = _mahagramSetting.BaseURL + "VerifybeneApi";
            string resp = string.Empty;
            try
            {
                resp = await AppWebRequest.O.PostJsonDataUsingHWRAsync(URL, mgRequest);
                var mgResp = JsonConvert.DeserializeObject<MGDMTTransactionResp>(resp);
                if (mgResp != null)
                {
                    if (!string.IsNullOrEmpty(mgResp.statuscode))
                    {
                        if (mgResp.statuscode.Equals("001"))
                        {
                            res.Statuscode = RechargeRespType.SUCCESS;
                            res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Transaction_Successful;
                            if (mgResp.Data != null && mgResp.Data.Count > 0)
                            {
                                res.VendorID = mgResp.Data[0].externalrefno;
                                res.BeneName = mgResp.Data[0].benename;
                                res.LiveID = mgResp.Data[0].rrn;
                            }
                            if (mgResp.Data[0].errorcode.Equals("1") || mgResp.Data[0].errorcode.Equals("9028") || mgResp.Data[0].errorcode.Equals("000") || mgResp.Data[0].errorcode.Equals("M1") || mgResp.Data[0].errorcode.Equals("014") || mgResp.Data[0].errorcode.Equals("010")
                                || mgResp.Data[0].errorcode.Equals("1077") || mgResp.Data[0].errorcode.Equals("52") || mgResp.Data[0].errorcode.Equals("052") || mgResp.Data[0].errorcode.Equals("003002") || mgResp.Data[0].errorcode.Equals("111") || mgResp.Data[0].errorcode.Equals("EXT_RSP9086")
)
                            {

                                res.Statuscode = RechargeRespType.FAILED;
                                res.Msg = mgResp.Data[0].messagetext;
                                res.ErrorCode = ErrorCodes.Unknown_Error;
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
                    FuncName = "MGAirtelBeneAdd",
                    Error = ex.Message,
                    LoginTypeID = _req.LT,
                    UserId = _req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            var dMTReq = new DMTReqRes
            {
                APIID = _APIID,
                Method = "MGAirtelBeneAdd",
                RequestModeID = _req.RequestMode,
                Request = URL + JsonConvert.SerializeObject(mgRequest),
                Response = resp,
                SenderNo = _req.SenderNO,
                UserID = _req.UserID,
                TID = _req.TID
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            res.Request = dMTReq.Request;
            res.Response = dMTReq.Response;
            return res;
        }

        public async Task<DMRTransactionResponse> MGPayMode(DMTReq _req, ReqSendMoney sendMoney, DMRTransactionResponse res)
        {
            res.Statuscode = RechargeRespType.PENDING;
            res.Msg = nameof(ErrorCodes.Request_Accpeted).Replace("_", " ");
            res.ErrorCode = ErrorCodes.Request_Accpeted;
            res.LiveID = res.Msg;
            if (string.IsNullOrEmpty(_mahagramSetting.BaseURL))
                return res;
            //IBankML bankML = new BankML(_accessor, _env, false);
            //var BankDetail = bankML.BankMasters(sendMoney.BankID);
            //if (BankDetail.Mahagram_BankID < 1)
            //{
            //    res.Statuscode = RechargeRespType.FAILED;
            //    res.Msg = nameof(DMTErrorCodes.Acquiring_Bank_CBS_or_node_offline).Replace("_", " ");
            //    res.ErrorCode = DMTErrorCodes.Acquiring_Bank_CBS_or_node_offline;
            //    return res;
            //}
            var mgRequest = new MGDMTRequest
            {
                amount = sendMoney.Amount.ToString(),
                bankname = sendMoney.BankID.ToString(),
                beneaccno = sendMoney.AccountNo,
                benemobile = sendMoney.BeneMobile,
                benename = sendMoney.BeneName,
                custno = _req.SenderNO,
                ifsc = sendMoney.IFSC,
                clientrefno = _req.TID,
                bc_id = _req.APIOutletID,
                secretkey = _mahagramSetting.SECRET,
                saltkey = _mahagramSetting.Salt
            };
            //string URL = _mahagramSetting.BaseURL + "Apiairtelpaymode";//Staging
            string URL = _mahagramSetting.BaseURL + "Apipaymode";
            string resp = string.Empty;
            try
            {
                resp = await AppWebRequest.O.PostJsonDataUsingHWRAsync(URL, mgRequest);
                var mgResp = JsonConvert.DeserializeObject<MGDMTTransactionResp>(resp);
                if (mgResp != null)
                {
                    if (!string.IsNullOrEmpty(mgResp.statuscode))
                    {
                        if (mgResp.statuscode.Equals("001"))
                        {
                            res.Statuscode = RechargeRespType.SUCCESS;
                            res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Transaction_Successful;
                            if (mgResp.Data != null && mgResp.Data.Count > 0)
                            {
                                res.VendorID = mgResp.Data[0].externalrefno;
                                res.BeneName = mgResp.Data[0].benename;
                                res.LiveID = mgResp.Data[0].rrn;
                            }

                            if (mgResp.Data[0].errorcode.Equals("1") || mgResp.Data[0].errorcode.Equals("3") || mgResp.Data[0].errorcode.Equals("4") || mgResp.Data[0].errorcode.Equals("8")
                     || mgResp.Data[0].errorcode.Equals("10") || mgResp.Data[0].errorcode.Equals("12") || mgResp.Data[0].errorcode.Equals("13") || mgResp.Data[0].errorcode.Equals("14") || mgResp.Data[0].errorcode.Equals("20")
                     || mgResp.Data[0].errorcode.Equals("22") || mgResp.Data[0].errorcode.Equals("51") || mgResp.Data[0].errorcode.Equals("52") || mgResp.Data[0].errorcode.Equals("54") || mgResp.Data[0].errorcode.Equals("86")
                     || mgResp.Data[0].errorcode.Equals("92") || mgResp.Data[0].errorcode.Equals("94") || mgResp.Data[0].errorcode.Equals("96") || mgResp.Data[0].errorcode.Equals("99") || mgResp.Data[0].errorcode.Equals("100")
                     || mgResp.Data[0].errorcode.Equals("302") || mgResp.Data[0].errorcode.Equals("608") || mgResp.Data[0].errorcode.Equals("801") || mgResp.Data[0].errorcode.Equals("911") || mgResp.Data[0].errorcode.Equals("912")
                     || mgResp.Data[0].errorcode.Equals("1075") || mgResp.Data[0].errorcode.Equals("1076") || mgResp.Data[0].errorcode.Equals("1077") || mgResp.Data[0].errorcode.Equals("1206") || mgResp.Data[0].errorcode.Equals("1515")
                     || mgResp.Data[0].errorcode.Equals("1616") || mgResp.Data[0].errorcode.Equals("3403") || mgResp.Data[0].errorcode.Equals("9001") || mgResp.Data[0].errorcode.Equals("9006") || mgResp.Data[0].errorcode.Equals("93097")
                     || mgResp.Data[0].errorcode.Equals("333998") || mgResp.Data[0].errorcode.Equals("999001") || mgResp.Data[0].errorcode.Equals("AS_111") || mgResp.Data[0].errorcode.Equals("BL101") || mgResp.Data[0].errorcode.Equals("M0")
                     || mgResp.Data[0].errorcode.Equals("M1") || mgResp.Data[0].errorcode.Equals("M2") || mgResp.Data[0].errorcode.Equals("M3") || mgResp.Data[0].errorcode.Equals("M4") || mgResp.Data[0].errorcode.Equals("M5")
                     || mgResp.Data[0].errorcode.Equals("M7") || mgResp.Data[0].errorcode.Equals("PM0405") || mgResp.Data[0].errorcode.Equals("PM0640") || mgResp.Data[0].errorcode.Equals("PM0684") || mgResp.Data[0].errorcode.Equals("052") || mgResp.Data[0].errorcode.Equals("010") || mgResp.Data[0].errorcode.Equals("10") || mgResp.Data[0].errorcode.Equals("014") || mgResp.Data[0].errorcode.Equals("003002")|| mgResp.Data[0].errorcode.Equals("EXT_RSP9086")
)
                            {
                                res.Statuscode = RechargeRespType.FAILED;
                                res.Msg = mgResp.message;
                                res.ErrorCode = ErrorCodes.Unknown_Error;
                            }
                        }
                        else if (mgResp.statuscode.Equals("000") && mgResp.message.Contains("nsufficen"))
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = nameof(ErrorCodes.ServiceDown);
                            res.ErrorCode = ErrorCodes.ServiceDown;
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
                    FuncName = "MGPayMode",
                    Error = ex.Message,
                    LoginTypeID = _req.LT,
                    UserId = _req.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            var dMTReq = new DMTReqRes
            {
                APIID = _APIID,
                Method = "MGPayMode",
                RequestModeID = _req.RequestMode,
                Request = URL + JsonConvert.SerializeObject(mgRequest),
                Response = resp,
                SenderNo = _req.SenderNO,
                UserID = _req.UserID,
                TID = _req.TID
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            res.Request = dMTReq.Request;
            res.Response = dMTReq.Response;
            return res;
        }

        internal class MahagramSetting
        {
            public string Salt { get; set; }
            public string CP { get; set; }
            public string SECRET { get; set; }
            public string BaseURL { get; set; }
            public string StatusCheckDMT { get; set; }
        }
    }

    public partial class MahagramAPIML : IMoneyTransferAPIML
    {
        public MahagramAPIML(IHttpContextAccessor accessor, IHostingEnvironment env, IDAL dal)
        {
            _accessor = accessor;
            _env = env;
            var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile((_env.IsProduction() ? "appsettings.json" : "appsettings.Development.json"));
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            _dal = dal;
            _mahagramSetting = MGAppSetting();
        }
        public MSenderLoginResponse GetSender(MTAPIRequest request)
        {
            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.One,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            if (string.IsNullOrEmpty(_mahagramSetting.BaseURL))
                return res;
            var mgRequest = new MGDMTRequest
            {
                bc_id = request.APIOutletID,
                custno = request.SenderMobile
            };
            string URL = _mahagramSetting.BaseURL + "getairtelbenedetails";
            string resp = string.Empty;
            try
            {
                resp = AppWebRequest.O.PostJsonDataUsingHWRAsync(URL, mgRequest).Result;
                var mgResp = JsonConvert.DeserializeObject<MGDMTResponse>(resp);
                res.Msg = nameof(DMTErrorCodes.Sender_Not_Found).Replace("_", " ");
                res.IsSenderNotExists = true;
                res.ErrorCode = DMTErrorCodes.Sender_Not_Found;
                if (mgResp != null)
                {
                    if (!string.IsNullOrEmpty(mgResp.statuscode))
                    {
                        if (mgResp.statuscode.Equals("002") || mgResp.statuscode.Equals("111"))
                        {
                            res.Msg = nameof(DMTErrorCodes.Sender_Not_Found).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Sender_Not_Found;
                        }
                        else if (mgResp.statuscode.Equals("001") || mgResp.statuscode.Equals("003"))
                        {
                            res.Msg = nameof(DMTErrorCodes.Detail_Found_Successfully).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Detail_Found_Successfully;
                            var totalLimit = string.IsNullOrEmpty(mgResp.total_limit) ? 0 : Convert.ToDecimal(mgResp.total_limit.Trim());
                            var usedLimit = string.IsNullOrEmpty(mgResp.used_limit) ? 0 : Convert.ToDecimal(mgResp.used_limit.Trim());
                            res.RemainingLimit = totalLimit - usedLimit;
                            res.AvailbleLimit = usedLimit;
                            res.SenderName = (mgResp.custfirstname??string.Empty)+" "+(mgResp.custlastname??string.Empty);
                            res.IsSenderNotExists = false;
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
                    FuncName = "GetMGSender",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            var dMTReq = new DMTReqRes
            {
                APIID = request.APIID,
                Method = "GetMGSender",
                RequestModeID = request.RequestMode,
                Request = URL + JsonConvert.SerializeObject(mgRequest),
                Response = resp,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            return res;
        }
        public MSenderLoginResponse CreateBeneficiary(MTAPIRequest request)
        {
            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            if (string.IsNullOrEmpty(_mahagramSetting.BaseURL))
                return res;
            IBankML bankML = new BankML(_accessor, _env, false);
            var BankDetail = bankML.BankMasters(request.BankID);
            if (BankDetail.Mahagram_BankID < 1)
            {
                res.Statuscode = RechargeRespType.FAILED;
                res.Msg = nameof(DMTErrorCodes.Acquiring_Bank_CBS_or_node_offline).Replace("_", " ");
                res.ErrorCode = DMTErrorCodes.Acquiring_Bank_CBS_or_node_offline;
                return res;
            }
            var mgRequest = new MGDMTRequest
            {
                bankname = BankDetail.Mahagram_BankID.ToString(),
                beneaccno = request.mBeneDetail.AccountNo,
                benemobile = request.mBeneDetail.MobileNo,
                benename = request.mBeneDetail.BeneName,
                custno = request.SenderMobile,
                ifsc = request.mBeneDetail.IFSC,
                bc_id = request.APIOutletID
            };
            string URL = _mahagramSetting.BaseURL + "airtelbeneadd";
            string resp = string.Empty;
            try
            {
                resp = AppWebRequest.O.PostJsonDataUsingHWRAsync(URL, mgRequest).Result;
                var mgResp = JsonConvert.DeserializeObject<MGDMTResponse>(resp);
                if (mgResp != null)
                {
                    if (!string.IsNullOrEmpty(mgResp.statuscode))
                    {
                        if (mgResp.statuscode.Equals("001"))
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = nameof(DMTErrorCodes.Beneficiary_Added_Successfully).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Beneficiary_Added_Successfully;
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
                    FuncName = "MGAirtelBeneAdd",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = _APIID,
                Method = "MGAirtelBeneAdd",
                RequestModeID = request.RequestMode,
                Request = URL + JsonConvert.SerializeObject(mgRequest),
                Response = resp,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            });
            return res;
        }
        public MSenderCreateResp CreateSender(MTAPIRequest request)
        {
            throw new NotImplementedException();
        }
        public MSenderCreateResp GenerateOTP(MTAPIRequest request)
        {
            var res = new MSenderCreateResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            if (string.IsNullOrEmpty(_mahagramSetting.BaseURL))
                return res;
            var mgRequest = new MGDMTRequest
            {
                bc_id = request.APIOutletID,
                custno = request.SenderMobile
            };
            string URL = _mahagramSetting.BaseURL + "airtelOTP";
            string resp = string.Empty;
            try
            {
                resp = AppWebRequest.O.PostJsonDataUsingHWRAsync(URL, mgRequest).Result;
                var mgResp = JsonConvert.DeserializeObject<List<MGDMTResponse>>(resp);
                if (mgResp != null)
                {
                    if (mgResp.Count > 0)
                    {
                        if (!string.IsNullOrEmpty(mgResp[0].statuscode))
                        {
                            if (mgResp[0].statuscode.Equals("001"))
                            {
                                res.Statuscode = ErrorCodes.One;
                                res.Msg = nameof(DMTErrorCodes.OTP_for_verification_has_been_sent_successfully).Replace("_", " ");
                                res.ErrorCode = DMTErrorCodes.OTP_for_verification_has_been_sent_successfully;
                                res.IsOTPGenerated = true;
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
                    FuncName = "GenerateOTP",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            var dMTReq = new DMTReqRes
            {
                APIID = request.APIID,
                Method = "GenerateOTP",
                RequestModeID = request.RequestMode,
                Request = URL + JsonConvert.SerializeObject(mgRequest),
                Response = resp,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
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
            if (string.IsNullOrEmpty(_mahagramSetting.BaseURL))
                return res;
            var mgRequest = new MGDMTRequest
            {
                bc_id = request.APIOutletID,
                custno = request.SenderMobile
            };
            string URL = _mahagramSetting.BaseURL + "getairtelbenedetails";
            string resp = string.Empty;
            try
            {
                resp =  AppWebRequest.O.PostJsonDataUsingHWRAsync(URL, mgRequest).Result;
                var mgResp = JsonConvert.DeserializeObject<MGDMTResponse>(resp);
                if (mgResp != null)
                {
                    if (!string.IsNullOrEmpty(mgResp.statuscode))
                    {
                        if (mgResp.statuscode.Equals("001"))
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Transaction_Successful;

                            var Beneficiaries = new List<MBeneDetail>();
                            foreach (var item in mgResp.Data)
                            {
                                if (item.bankid != "")
                                {
                                    Beneficiaries.Add(new MBeneDetail
                                    {
                                        AccountNo = item.beneaccno,
                                        BankName = item.bankname,
                                        IFSC = item.ifsc,
                                        BeneName = item.benename,
                                        MobileNo = item.benemobile,
                                        BeneID = item.id,
                                        IsVerified = !((item.status ?? string.Empty).ToUpper().Equals("NV")),
                                        BankID = Validate.O.IsNumeric(item.bankid) ? Convert.ToInt32(item.bankid) : 0
                                    });
                                }

                            }
                            res.Beneficiaries = Beneficiaries;
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
                Request = URL + JsonConvert.SerializeObject(mgRequest),
                Response = resp,
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
            var res = new MSenderCreateResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            if (string.IsNullOrEmpty(_mahagramSetting.BaseURL))
                return res;
            var mgRequest = new MGDMTRequest
            {
                bc_id = request.APIOutletID,
                custno = request.SenderMobile
            };
            string URL = _mahagramSetting.BaseURL + "airtelResendOTP" +
                "";
            string resp = string.Empty;
            try
            {
                resp =  AppWebRequest.O.PostJsonDataUsingHWRAsync(URL, mgRequest).Result;
                var mgResp = JsonConvert.DeserializeObject<List<MGDMTResponse>>(resp);
                if (mgResp != null)
                {
                    if (mgResp.Count > 0)
                    {
                        if (string.IsNullOrEmpty(mgResp[0].statuscode))
                        {
                            if (mgResp[0].statuscode.Equals("001"))
                            {
                                res.Statuscode = ErrorCodes.One;
                                res.Msg = nameof(DMTErrorCodes.OTP_for_verification_has_been_sent_successfully).Replace("_", " ");
                                res.ErrorCode = DMTErrorCodes.OTP_for_verification_has_been_sent_successfully;
                                res.IsOTPGenerated = true;
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
                Request = URL + JsonConvert.SerializeObject(mgRequest),
                Response = resp,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            });
            return res;
        }
        public MSenderLoginResponse ValidateBeneficiary(MTAPIRequest request)
        {
            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            if (string.IsNullOrEmpty(_mahagramSetting.BaseURL))
                return res;
            if (string.IsNullOrEmpty(request.OTP))
            {
                res.Msg = nameof(ErrorCodes.Invalid_OTP).Replace("_", " ");
                res.ErrorCode = ErrorCodes.Invalid_OTP;
                return res;
            }
            var mgRequest = new MGDMTRequest
            {
                beneaccno = request.mBeneDetail.AccountNo,
                benemobile = request.mBeneDetail.MobileNo,
                custno = request.SenderMobile,
                otp = request.OTP
            };
            string URL = _mahagramSetting.BaseURL + "verifybeneotp";
            string resp = string.Empty;
            try
            {
                resp = AppWebRequest.O.PostJsonDataUsingHWRAsync(URL, new { mgRequest.beneaccno, mgRequest.benemobile, mgRequest.custno, mgRequest.otp }).Result;
                var mgResp = JsonConvert.DeserializeObject<List<MGDMTResponse>>(resp);
                if (mgResp != null)
                {
                    if (mgResp.Count > 0)
                    {
                        if (!string.IsNullOrEmpty(mgResp[0].statuscode))
                        {
                            if (mgResp[0].statuscode.Equals("001"))
                            {
                                res.Statuscode = ErrorCodes.One;
                                res.Msg = nameof(DMTErrorCodes.OTP_verified_successfully).Replace("_", " ");
                                res.ErrorCode = DMTErrorCodes.OTP_verified_successfully;
                            }
                            else if (mgResp[0].statuscode.Equals("000") || mgResp[0].statuscode.Equals("003"))
                            {
                                res.Msg = nameof(ErrorCodes.Invalid_OTP).Replace("_", " ");
                                res.ErrorCode = ErrorCodes.Invalid_OTP;
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
                    FuncName = "MGVerifyBeneOTP",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            new ProcUpdateLogDMRReqResp(_dal).Call(new DMTReqRes
            {
                APIID = _APIID,
                Method = "MGVerifyBeneOTP",
                RequestModeID = request.RequestMode,
                Request = URL + JsonConvert.SerializeObject(mgRequest),
                Response = resp,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            });
            return res;
        }
        public MSenderLoginResponse VerifySender(MTAPIRequest request)
        {
            var res = new MSenderLoginResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(ErrorCodes.Unknown_Error).Replace("_", " "),
                ErrorCode = ErrorCodes.Unknown_Error
            };
            if (string.IsNullOrEmpty(_mahagramSetting.BaseURL))
                return res;
            if (string.IsNullOrEmpty(request.OTP))
            {
                res.Statuscode = ErrorCodes.Minus1;
                res.Msg = nameof(ErrorCodes.Invalid_OTP).Replace("_", " ");
                res.ErrorCode = ErrorCodes.Invalid_OTP;
                return res;
            }
            var mgRequest = new MGDMTRequest
            {
                saltkey = _mahagramSetting.Salt,
                secretkey = _mahagramSetting.SECRET,
                bc_id = request.APIOutletID,
                cust_f_name = request.FirstName,
                cust_l_name = request.LastName,
                custno = request.SenderMobile,
                pincode = request.Pincode.ToString(),
                otp = request.OTP,
                Dob = !Validate.O.IsDateIn_dd_MMM_yyyy_Format(request.DOB) ? DateTime.Now.ToString("dd-MM-yyyy") : Convert.ToDateTime(request.DOB.Replace(" ", "/")).ToString("dd-MM-yyyy"),
                Address = "Address - " + request.Pincode,
                StateCode = request.StateName
            };

            string URL = _mahagramSetting.BaseURL + "apiCustRegistration";
            string resp = string.Empty;
            try
            {
                resp = AppWebRequest.O.PostJsonDataUsingHWRAsync(URL, mgRequest).Result;
                var mgResp = JsonConvert.DeserializeObject<List<MGDMTResponse>>(resp);
                if (mgResp != null)
                {
                    if (!string.IsNullOrEmpty(mgResp[0].statuscode))
                    {
                        if (mgResp[0].statuscode.Equals("000"))
                        {
                            res.Msg = nameof(ErrorCodes.Invalid_OTP).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Invalid_OTP;
                        }
                        else if (mgResp[0].statuscode.Equals("001"))
                        {
                            res.Statuscode = ErrorCodes.One;
                            res.Msg = nameof(DMTErrorCodes.Sender_Created_Successfully).Replace("_", " ");
                            res.ErrorCode = DMTErrorCodes.Sender_Created_Successfully;
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
                    FuncName = "VerifySender",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            var dMTReq = new DMTReqRes
            {
                APIID = _APIID,
                Method = "VerifySender",
                RequestModeID = request.RequestMode,
                Request = URL + JsonConvert.SerializeObject(mgRequest),
                Response = resp,
                SenderNo = request.SenderMobile,
                UserID = request.UserID,
                TID = request.TransactionID
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            return res;
        }
        public MSenderLoginResponse RemoveBeneficiary(MTAPIRequest request)
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
            if (string.IsNullOrEmpty(_mahagramSetting.BaseURL))
                return res;
            IBankML bankML = new BankML(_accessor, _env, false);
            var BankDetail = bankML.BankMasters(request.mBeneDetail.BankID);
            if (BankDetail.Mahagram_BankID < 1)
            {
                res.Statuscode = RechargeRespType.FAILED;
                res.Msg = nameof(DMTErrorCodes.Acquiring_Bank_CBS_or_node_offline).Replace("_", " ");
                res.ErrorCode = DMTErrorCodes.Acquiring_Bank_CBS_or_node_offline;
                return res;
            }
            var mgRequest = new MGDMTRequest
            {
                bankname = BankDetail.Mahagram_BankID.ToString(),
                beneaccno = request.mBeneDetail.AccountNo,
                benemobile = request.SenderMobile,
                benename = request.mBeneDetail.BeneName,
                custno = request.SenderMobile,
                ifsc = request.mBeneDetail.IFSC,
                clientrefno = request.TID.ToString(),
                bc_id = request.APIOutletID,
                secretkey = _mahagramSetting.SECRET,
                saltkey = _mahagramSetting.Salt
            };
            //string URL = _mahagramSetting.BaseURL + "ApiVerifybene";//staging
            string _URL = _mahagramSetting.BaseURL + "VerifybeneApi";
            string response = string.Empty;
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWRAsync(_URL, mgRequest).Result;
                var mgResp = JsonConvert.DeserializeObject<MGDMTTransactionResp>(response);
                if (mgResp != null)
                {
                    if (!string.IsNullOrEmpty(mgResp.statuscode))
                    {
                        if (mgResp.statuscode.Equals("001"))
                        {
                            res.Statuscode = RechargeRespType.SUCCESS;
                            res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Transaction_Successful;
                            if (mgResp.Data != null && mgResp.Data.Count > 0)
                            {
                                res.VendorID = mgResp.Data[0].externalrefno;
                                res.BeneName = mgResp.Data[0].benename;
                                res.LiveID = mgResp.Data[0].rrn;
                            }
                            if (mgResp.Data[0].errorcode.Equals("1") || mgResp.Data[0].errorcode.Equals("9028") || mgResp.Data[0].errorcode.Equals("000") || mgResp.Data[0].errorcode.Equals("M1") || mgResp.Data[0].errorcode.Equals("014") || mgResp.Data[0].errorcode.Equals("010") || mgResp.Data[0].errorcode.Equals("EXT_RSP9086"))
                            {

                                res.Statuscode = RechargeRespType.FAILED;
                                res.Msg = mgResp.Data[0].messagetext;
                                res.ErrorCode = ErrorCodes.Unknown_Error;
                            }
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
                    FuncName = "VerifyAccount",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
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
            if (string.IsNullOrEmpty(_mahagramSetting.BaseURL))
                return res;
            
            var mgRequest = new MGDMTRequest
            {
                amount = request.Amount.ToString(),
                bankname = request.mBeneDetail.BankID.ToString(),
                beneaccno = request.mBeneDetail.AccountNo,
                benemobile = request.mBeneDetail.MobileNo,
                benename = request.mBeneDetail.BeneName,
                custno = request.SenderMobile,
                ifsc = request.mBeneDetail.IFSC,
                clientrefno = request.TID.ToString(),
                bc_id = request.APIOutletID,
                secretkey = _mahagramSetting.SECRET,
                saltkey = _mahagramSetting.Salt
            };
            //string URL = _mahagramSetting.BaseURL + "Apiairtelpaymode";//Staging
            string URL = _mahagramSetting.BaseURL + "Apipaymode";
            string response = string.Empty;
            try
            {
                response =  AppWebRequest.O.PostJsonDataUsingHWRAsync(URL, mgRequest).Result;
                var mgResp = JsonConvert.DeserializeObject<MGDMTTransactionResp>(response);
                if (mgResp != null)
                {
                    if (!string.IsNullOrEmpty(mgResp.statuscode))
                    {
                        if (mgResp.statuscode.Equals("001"))
                        {
                            res.Statuscode = RechargeRespType.SUCCESS;
                            res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                            res.ErrorCode = ErrorCodes.Transaction_Successful;
                            if (mgResp.Data != null && mgResp.Data.Count > 0)
                            {
                                res.VendorID = mgResp.Data[0].externalrefno;
                                res.VendorID2 = mgResp.Data[0].fesessionid;
                                res.BeneName = mgResp.Data[0].benename;
                                res.LiveID = mgResp.Data[0].rrn;
                            }

                            if (mgResp.Data[0].errorcode.Equals("1") || mgResp.Data[0].errorcode.Equals("3") || mgResp.Data[0].errorcode.Equals("4") || mgResp.Data[0].errorcode.Equals("8")|| mgResp.Data[0].errorcode.Equals("08")
                     || mgResp.Data[0].errorcode.Equals("10") || mgResp.Data[0].errorcode.Equals("12") || mgResp.Data[0].errorcode.Equals("13") || mgResp.Data[0].errorcode.Equals("14") || mgResp.Data[0].errorcode.Equals("20")
                     || mgResp.Data[0].errorcode.Equals("22") || mgResp.Data[0].errorcode.Equals("51") || mgResp.Data[0].errorcode.Equals("52") || mgResp.Data[0].errorcode.Equals("54") || mgResp.Data[0].errorcode.Equals("86")
                     || mgResp.Data[0].errorcode.Equals("92") || mgResp.Data[0].errorcode.Equals("94") || mgResp.Data[0].errorcode.Equals("96") || mgResp.Data[0].errorcode.Equals("99") || mgResp.Data[0].errorcode.Equals("100")
                     || mgResp.Data[0].errorcode.Equals("302") || mgResp.Data[0].errorcode.Equals("608") || mgResp.Data[0].errorcode.Equals("801") || mgResp.Data[0].errorcode.Equals("911") || mgResp.Data[0].errorcode.Equals("912")
                     || mgResp.Data[0].errorcode.Equals("1075") || mgResp.Data[0].errorcode.Equals("1076") || mgResp.Data[0].errorcode.Equals("1077") || mgResp.Data[0].errorcode.Equals("1206") || mgResp.Data[0].errorcode.Equals("1515")
                     || mgResp.Data[0].errorcode.Equals("1616") || mgResp.Data[0].errorcode.Equals("3403") || mgResp.Data[0].errorcode.Equals("9001") || mgResp.Data[0].errorcode.Equals("9006") || mgResp.Data[0].errorcode.Equals("93097")
                     || mgResp.Data[0].errorcode.Equals("333998") || mgResp.Data[0].errorcode.Equals("999001") || mgResp.Data[0].errorcode.Equals("AS_111") || mgResp.Data[0].errorcode.Equals("BL101") || mgResp.Data[0].errorcode.Equals("M0")
                     || mgResp.Data[0].errorcode.Equals("M1") || mgResp.Data[0].errorcode.Equals("M2") || mgResp.Data[0].errorcode.Equals("M3") || mgResp.Data[0].errorcode.Equals("M4") || mgResp.Data[0].errorcode.Equals("M5")
                     || mgResp.Data[0].errorcode.Equals("M7") || mgResp.Data[0].errorcode.Equals("PM0405") || mgResp.Data[0].errorcode.Equals("PM0640") || mgResp.Data[0].errorcode.Equals("PM0684") || mgResp.Data[0].errorcode.Equals("EXT_RSP9086") || mgResp.Data[0].errorcode.Equals("9028") || mgResp.Data[0].errorcode.Equals("052") || mgResp.Data[0].errorcode.Equals("010") || mgResp.Data[0].errorcode.Equals("014") || mgResp.Data[0].errorcode.Equals("003002"))
                            {
                                res.Statuscode = RechargeRespType.FAILED;
                                res.Msg = mgResp.message;
                                res.ErrorCode = ErrorCodes.Unknown_Error;
                            }
                        }
                        else if (mgResp.statuscode.Equals("000") && (mgResp.message.Contains("nsufficen") || mgResp.message.Contains("Service is down")))
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = nameof(ErrorCodes.ServiceDown);
                            res.ErrorCode = ErrorCodes.ServiceDown;
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
            res.Request = URL+"?"+JsonConvert.SerializeObject(mgRequest);
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


        public DMRTransactionResponse GetTransactionStatus(int TID, string TransactionID, int RequestMode, int UserID, int APIID,
string VendorID, string VendorID2)
        {
            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = RechargeRespType._PENDING,
                VendorID = "",
                LiveID = "",
                ErrorCode = ErrorCodes.Request_Accpeted
            };

            if (string.IsNullOrEmpty(_mahagramSetting.BaseURL))
                return res;
            var mgRequest = new MGDMTRequest
            {
                secretkey = _mahagramSetting.SECRET,
                saltkey = _mahagramSetting.Salt,
                Mhid = VendorID,
                FsessionId = VendorID2
            };
            string URL = _mahagramSetting.StatusCheckDMT;
            string response = string.Empty;
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWRAsync(URL, mgRequest).Result;
                var mgResp = JsonConvert.DeserializeObject<MGDMTTransactionResp>(response);
                if (mgResp != null)
                {
                    if (!string.IsNullOrEmpty(mgResp.statuscode))
                    {
                        if (mgResp.statuscode.Equals("000") && mgResp.message.Equals("Success") && mgResp.Data != null)
                        {
                            if (mgResp.Data.Count > 0)
                            {
                                res.Statuscode = RechargeRespType.SUCCESS;
                                res.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                res.ErrorCode = ErrorCodes.Transaction_Successful;
                                res.VendorID = mgResp.Data[0].externalrefno;
                                res.VendorID2 = mgResp.Data[0].fesessionid;
                                res.BeneName = mgResp.Data[0].benename;
                                res.LiveID = mgResp.Data[0].rrn;
                            }
                        }
                        else if (mgResp.message.Contains("nsufficen"))
                        {
                            res.Statuscode = RechargeRespType.FAILED;
                            res.Msg = nameof(ErrorCodes.ServiceDown);
                            res.ErrorCode = ErrorCodes.ServiceDown;
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
                    UserId = 1
                });
            }

            var dMTReq = new DMTReqRes
            {
                APIID = APIID,
                Method = "GetTransactionStatus",
                RequestModeID = RequestMode,
                Request = string.Format("{0}?{1}", URL, JsonConvert.SerializeObject(mgRequest)),
                Response = response,
                SenderNo = string.Empty,
                UserID = UserID,
                TID = TID.ToString()
            };
            new ProcUpdateLogDMRReqResp(_dal).Call(dMTReq);
            res.Request = dMTReq.Request;
            res.Response = response;
            return res;
        }
    }
}
