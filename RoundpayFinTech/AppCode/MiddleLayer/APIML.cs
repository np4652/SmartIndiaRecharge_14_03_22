using Fintech.AppCode;
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
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Validators;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public sealed class APIML : IAPIML, ISMSAPIML, IDisposable, IEmailAPIML, IWhatsappSenderNoML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly ISession _session;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        private readonly IRequestInfo _rinfo;
        private readonly LoginResponse _lr;
        private readonly IUserML userML;
        private readonly LoginResponse _lrEmp;
        public APIML(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _session = _accessor != null ? _accessor.HttpContext.Session : null;
            _dal = new DAL(_c.GetConnectionString());
            _rinfo = new RequestInfo(_accessor, _env);
            _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
            userML = new UserML(_lr);
            _lrEmp = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponseEmp);
        }
        
        public APIML(LoginResponse lr) => _lr = lr;
        public void Dispose()
        {

        }
        #region APIGroupRegion
        public APIGroupDetail GetGroup(int GroupID)
        {
            var res = new APIGroupDetail();
            if (_lr.RoleID > 0)
            {
                IProcedure _proc = new ProcAPIGroupDetail(_dal);
                res = (APIGroupDetail)_proc.Call(GroupID);
            }
            return res;
        }
        public IEnumerable<APIGroupDetail> GetGroup()
        {
            var res = new List<APIGroupDetail>();
            if (_lr.RoleID > 0)
            {
                IProcedure _proc = new ProcAPIGroupDetail(_dal);
                res = (List<APIGroupDetail>)_proc.Call();
            }
            return res;
        }
        #endregion
        public IEnumerable<APIDetail> GetAPIDetail()
        {
            var resp = new List<APIDetail>();
            var loginResp = chkSession();
            if (loginResp != null)
            {
                if ((!userML.IsEndUser() && loginResp.LoginTypeID == LoginType.ApplicationUser) || userML.IsCustomerCareAuthorised(ActionCodes.ShowAdminDaybook) || userML.IsCustomerCareAuthorised(ActionCodes.APISTATUSCHECK) || userML.IsCustomerCareAuthorised(ActionCodes.AddEditAPI))
                {
                    var req = new CommonReq
                    {
                        IsListType = true,
                        LoginID = loginResp.LoginTypeID == LoginType.CustomerCare ? 1 : loginResp.UserID
                    };
                    IProcedure _proc = new ProcGetAPI(_dal);
                    resp = (List<APIDetail>)_proc.Call(req);
                }
            }
            return resp;
        }

        public async Task<IEnumerable<APIDetail>> GetAllAPI(int opTypeId)
        {
            IProcedureAsync _proc = new ProcGetAllAPI(_dal);
            var resp = (List<APIDetail>)await _proc.Call(opTypeId).ConfigureAwait(true);
            return resp;
        }

        public async Task<IResponseStatus> UpdateOpTypeWiseAPISwitch(OpTypeWiseAPISwitchingReq req)
        {
            IProcedureAsync _proc = new ProcOpTypeWiseAPISwitch(_dal);
            var resp = (ResponseStatus)await _proc.Call(req).ConfigureAwait(true);
            return resp;
        }

        public IEnumerable<APIDetail> GetAPIDetailForBalance()
        {
            var resp = new List<APIDetail>();
            if ((!userML.IsEndUser() && _lr.LoginTypeID == LoginType.ApplicationUser) || userML.IsCustomerCareAuthorised(ActionCodes.ShowAdminDaybook) || userML.IsCustomerCareAuthorised(ActionCodes.APISTATUSCHECK) || userML.IsCustomerCareAuthorised(ActionCodes.AddEditAPI))
            {
                IProcedure _proc = new ProcGetAPI(_dal);
                var aPIDetails = (List<APIDetail>)_proc.Call(new CommonReq
                {
                    IsListType = true,
                    LoginID = _lr.LoginTypeID == LoginType.CustomerCare ? 1 : _lr.UserID
                });
                resp = aPIDetails.Where(x => x.BalanceKey != string.Empty && x.InSwitch).ToList();
            }
            return resp;
        }
        public APIDetail GetAPIDetailByID(int APIID)
        {
            var resp = new APIDetail();
            if (!userML.IsEndUser() && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                IProcedure _proc = new ProcGetAPI(_dal);
                resp = (APIDetail)_proc.Call(new CommonReq
                {
                    CommonInt = APIID,
                    LoginID = _lr.LoginTypeID == LoginType.CustomerCare ? 1 : _lr.UserID
                });
            }
            return resp;
        }

        public APIDetail GetAPIDetailByAPICode(string APICode)
        {
            var req = new CommonReq
            {
                str = APICode
            };
            IProcedure _proc = new ProcGetPlansAPIByAPICode(_dal);
            return (APIDetail)_proc.Call(req);
        }

        public IResponseStatus SaveAPI(APIDetail req)
        {
            var resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if ((!userML.IsEndUser() && _lr.LoginTypeID == LoginType.ApplicationUser) || (userML.IsCustomerCareAuthorised(ActionCodes.AddEditAPI)))
            {
                if (string.IsNullOrEmpty(req.Name) || Validate.O.IsNumeric(req.Name ?? "") || (req.Name ?? "").Length > 50)
                {
                    resp.Msg = ErrorCodes.InvalidParam + " Name";
                    return resp;
                }
                if (string.IsNullOrEmpty(req.GroupName) || Validate.O.IsNumeric(req.GroupName ?? "") || (req.GroupName ?? "").Length > 50)
                {
                    resp.Msg = ErrorCodes.InvalidParam + " GroupName";
                    return resp;
                }
                if (string.IsNullOrEmpty(req.GroupCode) || Validate.O.IsNumeric(req.GroupCode ?? "") || (req.GroupCode ?? "").Length > 50)
                {
                    resp.Msg = ErrorCodes.InvalidParam + " GroupCode";
                    return resp;
                }
                var _req = new APIDetailReq
                {
                    LoginID = _lr.UserID,
                    LT = _lr.LoginTypeID,
                    WID = _lr.WID,
                    ID = req.ID,
                    APIType = req.APIType,
                    Name = req.Name,
                    URL = req.URL,
                    StatusCheckURL = req.StatusCheckURL,
                    BalanceURL = req.BalanceURL,
                    DisputeURL = req.DisputeURL,
                    FetchBillURL = req.FetchBillURL,
                    RequestMethod = req.RequestMethod,
                    StatusName = req.StatusName,
                    SuccessCode = req.SuccessCode,
                    FailCode = req.FailCode,
                    LiveID = req.LiveID,
                    VendorID = req.VendorID,
                    ResponseTypeID = req.ResponseTypeID,
                    Remark = req.Remark,
                    IsOutletRequired = req.IsOutletRequired,
                    FixedOutletID = req.FixedOutletID,
                    IsOpDownAllow = req.IsOpDownAllow,
                    IP = _rinfo.GetRemoteIP(),
                    Browser = _rinfo.GetBrowserFullInfo(),
                    SurchargeType = req.SurchargeType,
                    MsgKey = req.MsgKey,
                    BillNoKey = req.BillNoKey,
                    BillDateKey = req.BillDateKey,
                    BillAmountKey = req.BillAmountKey,
                    CustomerNameKey = req.CustomerNameKey,
                    DueDateKey = req.DueDateKey,
                    GroupName = req.GroupName,
                    GroupCode = req.GroupCode,
                    GroupID = req.GroupID,
                    BillStatusKey = req.BillStatusKey,
                    BillStatusValue = req.BillStatusValue,
                    IsOutletManual = req.IsOutletManual,
                    ContentType = req.ContentType,
                    BalanceKey = req.BalanceKey,
                    BillReqMethod = req.BillReqMethod,
                    BillResTypeID = req.BillResTypeID,
                    InSwitch = req.InSwitch,
                    ErrorCodeKey = req.ErrorCodeKey,
                    RefferenceKey = req.RefferenceKey,
                    DFormatID = req.DFormatID,
                    MaxLimitPerTransaction = req.MaxLimitPerTransaction,
                    VenderMail = req.VenderMail,
                    HandoutID = req.HandoutID,
                    Mobileno = req.Mobileno,
                    WhatsAppNo = req.WhatsAppNo,
                    PartnerUserID = req.PartnerUserID,
                    ValidateURL=req.ValidateURL,
                    AdditionalInfoListKey=req.AdditionalInfoListKey,
                    AdditionalInfoKey=req.AdditionalInfoKey,
                    AdditionalInfoValue=req.AdditionalInfoValue,
                    ValidationStatusKey=req.ValidationStatusKey,
                    ValidationStatusValue=req.ValidationStatusValue,
                    APIOutletIDMob=req.APIOutletIDMob,
                    GeoCodeAGT = req.GeoCodeAGT,
                    GeoCodeMOB = req.GeoCodeMOB,
                    GeoCodeINT = req.GeoCodeINT,
                    HookBalanceKey=req.HookBalanceKey,
                    HookFailCode=req.HookFailCode,
                    HookLiveIDKey=req.HookLiveIDKey,
                    HookMsgKey=req.HookMsgKey,
                    HookResTypeID=req.HookResTypeID,
                    HookStatusKey=req.HookStatusKey,
                    HookSuccessCode=req.HookSuccessCode,
                    HookTIDKey=req.HookTIDKey,
                    HookVendorKey=req.HookVendorKey,
                    BillFetchAPICode = req.BillFetchAPICode,
                    FirstDelimiter = req.FirstDelimiter,
                    SecondDelimiter = req.SecondDelimiter,
                    HookFirstDelimiter = req.HookFirstDelimiter,
                    HookSecondDelimiter = req.HookSecondDelimiter,
                };
                IProcedure proc = new ProcAPICU(_dal);
                resp = (ResponseStatus)proc.Call(_req);
            }
            return resp;
        }

        public IEnumerable<SlabCommission> GetAPICommission(int APIID)
        {
            var _resp = new List<SlabCommission>();
            if ((!userML.IsEndUser() && _lr.LoginTypeID == LoginType.ApplicationUser) || (userML.IsCustomerCareAuthorised(ActionCodes.AddEditAPI)))
            {
                var _proc = new ProcGetAPICommDetail(_dal);
                _resp = (List<SlabCommission>)_proc.Call(new CommonReq
                {
                    CommonInt = APIID,
                    LoginID = _lr.LoginTypeID == LoginType.CustomerCare ? 1 : _lr.UserID
                });
            }
            return _resp;
        }
        public RangeDetailModel GetAPICommissionRange(int APIID, int OType = 0)
        {
            var res = new RangeDetailModel { };
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin || (userML.IsCustomerCareAuthorised(ActionCodes.AddEditSLAB)))
            {
                var req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonInt = APIID,
                    CommonInt2 = CommStttingType.Rangewise,
                    CommonInt3 = OType
                };
                IProcedure proc = new ProcAPICommissionRange(_dal);
                res = (RangeDetailModel)proc.Call(req);
                if (res.IsAdminDefined)
                {
                    List<OperatorDetail> operatorDetails = res.SlabDetails
                        .GroupBy(x => new { x.OID, x.Operator, x.OperatorType, x.IsBBPS, x.MinRange, x.MaxRange, x.RangeId, x.OpType })
                        .Select(g => new OperatorDetail { OID = g.Key.OID, Operator = g.Key.Operator, OperatorType = g.Key.OperatorType, IsBBPS = g.Key.IsBBPS, MinRange = g.Key.MinRange, MaxRange = g.Key.MaxRange, RangeId = g.Key.RangeId, OpType = g.Key.OpType })
                        .ToList();
                    res.Operators = operatorDetails;
                }
            }
            return res;
        }

        public IResponseStatus UpdateAPISTATUSCHECK(APISTATUSCHECK apistatuscheck)
        {
            var _res = new ResponseStatus()
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if ((_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser) || userML.IsCustomerCareAuthorised(ActionCodes.APISTATUSCHECK))
            {
                apistatuscheck.Checks = apistatuscheck.Checks.Replace("\r", "").Replace("\n", "");
                IProcedure _proc = new ProcUpdateAPIStatusCheck(_dal);
                _res = (ResponseStatus)_proc.Call(apistatuscheck);
            }
            return _res;
        }

        public async Task<APISTATUSCHECK> GetAPISTATUSCHECK(APISTATUSCHECK apistatuscheck)
        {
            if ((_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser) || userML.IsCustomerCareAuthorised(ActionCodes.APISTATUSCHECK))
            {
                apistatuscheck.Msg = Validate.O.ReplaceAllSpecials(apistatuscheck.Msg).Trim();
                string msg = apistatuscheck.Msg;
                IProcedureAsync _proc = new ProcCheckTextResponse(_dal);
                apistatuscheck = (APISTATUSCHECK)await _proc.Call(apistatuscheck);
                apistatuscheck.SplitMsg = msg.Split(' ').ToList();
            }
            else
            {
                apistatuscheck.Statuscode = ErrorCodes.Minus1;
                apistatuscheck.Msg = ErrorCodes.AuthError;
            }
            return apistatuscheck;
        }

        public IEnumerable<APISTATUSCHECK> GetAPISTATUSCHECKs(string CheckText, int Status)
        {
            var res = new List<APISTATUSCHECK>();
            if ((_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser) || userML.IsCustomerCareAuthorised(ActionCodes.APISTATUSCHECK))
            {
                var req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonInt = Status,
                    CommonStr = CheckText
                };
                IProcedure proc = new ProcAPISTATUSCHECKList(_dal);
                res = (List<APISTATUSCHECK>)proc.Call(req);
            }
            return res;
        }

        public IResponseStatus DeleteApiStatusCheck(int Statusid)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Request can not be completed"
            };
            if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                var req = new CommonReq
                {
                    LoginID = _lr.UserID,
                    CommonInt = Statusid
                };
                IProcedure _proc = new ProcRemoveAPISTATUSCHECK(_dal);
                _res = (ResponseStatus)_proc.Call(req);
            }
            return _res;
        }

        #region SMSAPIRegion
        public IEnumerable<SMSAPIDetail> GetSMSAPIDetail()
        {
            var resp = new List<SMSAPIDetail>();
            if ((_lr.RoleID == Role.Admin || _lr.IsWebsite) && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.BulkSMS))
            {
                var req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonInt = 0
                };
                IProcedure _proc = new ProcGetSMSAPI(_dal);
                resp = (List<SMSAPIDetail>)_proc.Call(req);
            }
            return resp;
        }

        public SMSAPIDetail GetSMSAPIDetailByID(int APIID)
        {
            var resp = new SMSAPIDetail();
            if (_lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.BulkSMS))
            {
                if (APIID > 0)
                {
                    var req = new CommonReq
                    {
                        LoginTypeID = _lr.LoginTypeID == LoginType.CustomerCare ? 1 : _lr.LoginTypeID,
                        LoginID = _lr.LoginTypeID == LoginType.CustomerCare ? 1 : _lr.UserID,
                        CommonInt = APIID
                    };
                    IProcedure _proc = new ProcGetSMSAPI(_dal);
                    resp = (SMSAPIDetail)_proc.Call(req);
                }
            }
            return resp;
        }
        public IResponseStatus SaveSMSAPI(APIDetail req)
        {
            var resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if ((_lr.LoginTypeID == LoginType.ApplicationUser && !userML.IsEndUser()) || (userML.IsCustomerCareAuthorised(ActionCodes.AddEditSMSAPI)))
            {
                if (string.IsNullOrEmpty(req.Name) || Validate.O.IsNumeric(req.Name ?? "") || (req.Name ?? "").Length > 50)
                {
                    resp.Msg = ErrorCodes.InvalidParam + " Name";
                    return resp;
                }
                var _req = new APIDetailReq
                {
                    LoginID = _lr.UserID,
                    LT = _lr.LoginTypeID,
                    ID = req.ID,
                    APIType = req.APIType,
                    Name = req.Name,
                    URL = req.URL,
                    RequestMethod = req.RequestMethod,
                    ResponseTypeID = req.ResponseTypeID,
                    IP = _rinfo.GetRemoteIP(),
                    Browser = _rinfo.GetBrowserFullInfo(),
                    TransactionType = req.TransactionType,
                    IsActive = req.IsActive,
                    Default = req.Default,
                    IsWhatsApp = req.IsWhatsApp,
                    IsHangout = req.IsHangout,
                    IsTelegram = req.IsTelegram
                };

                IProcedure proc = new ProcSMSAPICU(_dal);
                resp = (ResponseStatus)proc.Call(_req);
            }
            return resp;
        }

        public IResponseStatus ISSMSAPIActive(int ID, bool IsActive, bool IsDefault)
        {
            var resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if ((_lr.LoginTypeID == LoginType.ApplicationUser && !userML.IsEndUser()) || (userML.IsCustomerCareAuthorised(ActionCodes.AddEditSMSAPI)))
            {
                if (ID == 0)
                {
                    resp.Msg = ErrorCodes.InvalidParam + " ID";
                    return resp;
                }
                var _req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    CommonInt = ID,
                    CommonBool = IsActive,
                    CommonBool1 = IsDefault
                };
                IProcedure proc = new ProcChangeAPIActiveStatus(_dal);
                resp = (ResponseStatus)proc.Call(_req);
            }
            return resp;
        }
        #endregion

        #region EmailAPIRegion
        public IEnumerable<EmailAPIDetail> GetEmailAPIDetail()
        {
            var resp = new List<EmailAPIDetail>();
            if (_lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.BulkSMS))
            {
                var req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonInt = 0
                };
                IProcedure _proc = new ProcGetEmailAPI(_dal);
               // IProcedure _proc = new ProcGetAllEmailProviders(_dal);
                resp = (List<EmailAPIDetail>)_proc.Call(req);
            }
            return resp;
        }

        public List<EmailProvider> GetEmailProviderDetail()
        {
            var resp = new List<EmailProvider>();
            if (_lr.LoginTypeID == LoginType.ApplicationUser)
            {
                IProcedure _proc = new ProcGetAllEmailProviders(_dal);
                resp = (List<EmailProvider>)_proc.Call(new CommonReq
                {
                    LoginID = _lr.UserID,
                    LoginTypeID = _lr.LoginTypeID,
                    CommonInt = 0
                });
            }
            return resp;
        }   

        public EmailAPIDetail GetEmailAPIDetailByID(int APIID)
        {
            var resp = new EmailAPIDetail();
            if (_lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.BulkSMS))
            {
                if (APIID > 0)
                {
                    var req = new CommonReq
                    {
                        LoginTypeID = _lr.LoginTypeID == LoginType.CustomerCare ? 1 : _lr.LoginTypeID,
                        LoginID = _lr.LoginTypeID == LoginType.CustomerCare ? 1 : _lr.UserID,
                        CommonInt = APIID
                    };
                    IProcedure _proc = new ProcGetEmailAPI(_dal);
                    resp = (EmailAPIDetail)_proc.Call(req);
                }
               
            }
            return resp;
        }
        public IResponseStatus SendEmailToId(int APIID ,string ToMail)
        {
            var resp = new EmailAPIDetail();
            var resp1 = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.BulkSMS))
            {
                
                if (APIID > 0)
                {
                    var req = new CommonReq
                    {
                        LoginTypeID = _lr.LoginTypeID == LoginType.CustomerCare ? 1 : _lr.LoginTypeID,
                        LoginID = _lr.LoginTypeID == LoginType.CustomerCare ? 1 : _lr.UserID,
                        CommonInt = APIID
                    };
                  
                  resp = GetEmailAPIDetailByID(APIID);
                }
                if (resp.IsActive)
                {
                    try
                    {

                        var senderEmail = new MailAddress(resp.UserMailID, resp.FromEmail);
                        var receiverEmail = new MailAddress(ToMail, "Receiver");
                        var password = resp.Password;
                        var sub = "Test Mail Subject";
                        var body = "Test Mail Body";
                        var smtp = new SmtpClient
                        {
                            Host = resp.HostName,
                            Port = resp.Port,
                            EnableSsl = true,
                            DeliveryMethod = SmtpDeliveryMethod.Network,
                            UseDefaultCredentials = false,
                            Credentials = new NetworkCredential(senderEmail.Address, password)
                        };
                        using (var mess = new MailMessage(senderEmail, receiverEmail)
                        {
                            Subject = sub,
                            Body = body
                        })
                        {
                            smtp.Send(mess);
                            SendEmail sendEmail = new SendEmail
                            {
                                From = resp.FromEmail,
                                Body = body,
                                Recipients = ToMail,
                                Subject = sub,
                                IsSent = true,
                                WID = resp.WID
                            };
                            EmailDL emailDL = new EmailDL(_dal);
                            emailDL.SaveMail(sendEmail);
                        }
                        resp1.Msg = "Mail Send Succesfully;";
                        return resp1;

                    }
                    catch (Exception ee)
                    {
                        resp1.Msg = "Mail not Send;";
                        return resp1;
                    }
                }

            }
            return resp1;
        }
        public IResponseStatus SaveEmailAPI(EmailAPIDetail req)
        {
            var resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if ((_lr.LoginTypeID == LoginType.ApplicationUser) || (userML.IsCustomerCareAuthorised(ActionCodes.AddEditSMSAPI)))
            {
                if (string.IsNullOrEmpty(req.FromEmail) || Validate.O.IsNumeric(req.FromEmail ?? ""))
                {
                    resp.Msg = ErrorCodes.InvalidParam + " Email";
                    return resp;
                }
                if (string.IsNullOrEmpty(req.Password))
                {
                    resp.Msg = " Fill Password";
                    return resp;
                }
              
                if (string.IsNullOrEmpty(req.UserMailID))
                {
                    resp.Msg = "Fill User Mail ID";
                    return resp;
                }
               
                var _req = new EmailAPIDetailReq
                {
                    LoginID = _lr.UserID,
                    LT = _lr.LoginTypeID,
                    ID = req.ID,
                    FromEmail = req.FromEmail,
                    Password = req.Password,
                    HostName = req.HostName,
                    Port = req.Port,
                    IsActive = req.IsActive,
                    IsEmailVerified = req.IsEmailVerified,
                    IsSSL = req.IsSSL,
                    UserMailID = req.UserMailID,
                    IsDefault = req.IsDefault,
                    IP = _rinfo.GetRemoteIP(),
                    Browser = _rinfo.GetBrowserFullInfo()
                };
                IProcedure proc = new ProcEmailAPICU(_dal);
                resp = (ResponseStatus)proc.Call(_req);
            }
            return resp;
        }

        #endregion

        public async Task<APIBalanceResponse> GetBalanceFromAPI(int APIID)
        {
            var balanceResponse = new APIBalanceResponse();
            var apiDetail = GetAPIDetailByID(APIID);
            if (apiDetail.ID > 0)
            {
                var tHelper = new TransactionHelper(_dal, _accessor, _env);
                balanceResponse = await tHelper.HitGetBalance(apiDetail);
            }
            return balanceResponse;
        }
        private LoginResponse chkSession()
        {
            var result = new LoginResponse();
            if (_lr != null)
            {
                result = _lr;
            }
            if (_lrEmp != null)
            {
                result = _lrEmp;
            }
            return result;
        }
        public IResponseStatus UpdateDMRModelForAPI(int OID, int API, int DMRModelID)
        {
            IProcedure proc = new ProcUpdateDMRModelForAPI(_dal);
            return (ResponseStatus)proc.Call(new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID,
                CommonInt = API,
                CommonInt2 = OID,
                CommonInt3 = DMRModelID
            });
        }

        public IResponseStatus UpdateMapNumber(string r, string mn, bool ia, int id)
        {

            var resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if ((_lr.LoginTypeID == LoginType.ApplicationUser && !userML.IsEndUser()) || (userML.IsCustomerCareAuthorised(ActionCodes.AddEditSMSAPI)))
            {
                var _req = new WhatsappAPIDetail
                {
                    LoginID = _lr.UserID,
                    LT = _lr.LoginTypeID,
                    IsActive = ia,
                    Mobileno = mn,
                    DEPID = r,
                    ID = id
                };
                IProcedure proc = new ProcUpdateDeptMapNumber(_dal);
                resp = (ResponseStatus)proc.Call(_req);
            }
            return resp;
        }
        #region WhatsappAPIRegion

        public IResponseStatus DeleteWtSenderNo(int id)
        {
            var resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if ((_lr.LoginTypeID == LoginType.ApplicationUser && !userML.IsEndUser()) || (userML.IsCustomerCareAuthorised(ActionCodes.AddEditSMSAPI)))
            {
                var _req = new WhatsappAPIDetail
                {
                    LoginID = _lr.UserID,
                    LT = _lr.LoginTypeID,
                    ID = id
                };

                IProcedure proc = new ProcDeleteWhatsappSenderNo(_dal);
                resp = (ResponseStatus)proc.Call(_req);
            }
            return resp;
        }


        public IResponseStatus SaveWtSenderNo(WhatsappAPIDetail req)
        {
            var resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if ((_lr.LoginTypeID == LoginType.ApplicationUser && !userML.IsEndUser()) || (userML.IsCustomerCareAuthorised(ActionCodes.AddEditSMSAPI)))
            {
                if (req.ID == 0)
                {
                    if (string.IsNullOrEmpty(req.Mobileno))
                    {
                        resp.Msg = ErrorCodes.InvalidParam + " Mobileno";
                        return resp;
                    }
                    if (req.Mobileno.Length != 12 || req.Mobileno.Substring(0, 2) != "91")
                    {
                        resp.Msg = ErrorCodes.InvalidParam + " Mobileno";
                        return resp;
                    }
                    req.IsActive = true;
                }

                IProcedure proc = new ProcSaveWhatsappSenderNo(_dal);
                resp = (ResponseStatus)proc.Call(req);
            }
            return resp;
        }


        public IEnumerable<WhatsappAPIDetail> GetWhatsappSenderNoList(int id)
        {
            var resp = new List<WhatsappAPIDetail>();
            if ((_lr.RoleID == Role.Admin || _lr.IsWebsite) && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.BulkSMS))
            {
                var req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonInt = id
                };
                IProcedure _proc = new ProcGetWhatsappSenderNo(_dal);
                resp = (List<WhatsappAPIDetail>)_proc.Call(req);
            }
            return resp;
        }
        #endregion
        public IEnumerable<ApiListModel> GetVoucherApi()
        {
            var resp = new List<ApiListModel>();
            if ((_lr.RoleID == Role.Admin || _lr.IsWebsite) && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.BulkSMS))
            {
                var req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                   
                };
                IProcedure _proc = new ProcGetVoucherApi(_dal);
                resp = (List<ApiListModel>)_proc.Call();
            }
            return resp;
        }

    }
}
