using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using RoundpayFinTech.AppCode.Configuration;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.DL.Employee;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.Employee;
using RoundpayFinTech.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Validators;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public class EmpML : IEmpML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly ISession _session;
        private IDAL _dal;
        private readonly IConnectionConfiguration _c;
        private readonly IRequestInfo _rinfo;
        private readonly LoginResponse _lrEmp;
        private readonly LoginResponse _lr;
        public EmpML(IHttpContextAccessor accessor, IHostingEnvironment env, bool IsSession = true)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _session = _accessor.HttpContext.Session;
            _dal = new DAL(_c.GetConnectionString());
            _rinfo = new RequestInfo(_accessor, _env);
            if (IsSession)
            {
                _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
                _lrEmp = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponseEmp);
            }
        }
        public IResponseStatus GetEmpDetailByID(string MobileNo)
        {
            IResponseStatus _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.InvalidParam
            };
            var loginRes = IsAdmin();
            if (loginRes != null)
            {
                if (string.IsNullOrEmpty(MobileNo) || !Validate.O.IsNumeric(MobileNo ?? "") || (MobileNo ?? "").Length > 10)
                    return _res;
                EmpReq req = new EmpReq
                {
                    LoginID = loginRes.UserID,
                    LTID = loginRes.LoginTypeID
                };
                bool InSession = false;
                if (Validate.O.IsMobile(MobileNo ?? ""))
                {
                    req.MobileNo = MobileNo;
                    InSession = loginRes.LoginTypeID == LoginType.ApplicationUser && loginRes.MobileNo == req.MobileNo;
                }
                else
                {
                    req.EmpID = Convert.ToInt32(MobileNo);
                    InSession = loginRes.LoginTypeID == LoginType.ApplicationUser && loginRes.UserID == req.EmpID;
                }
                if (!InSession)
                {
                    IProcedure proc = new ProcGetEmpByID(_dal);
                    _res = (IResponseStatus)proc.Call(req);
                }
                else
                {
                    _res.Statuscode = ErrorCodes.One;
                    _res.Msg = ErrorCodes.SUCCESS;
                    _res.CommonInt = loginRes.UserID;
                    _res.CommonInt2 = loginRes.RoleID;
                    _res.CommonStr = loginRes.MobileNo;
                    _res.CommonStr2 = loginRes.Name;
                }
            }
            return _res;
        }

        public EmpRegModel GetEmpReffDeatil(string MobileNo)
        {
            EmpRegModel _res = new EmpRegModel();
            _res.EmpDetail = GetEmpDetailByID(MobileNo);
            _res.Input = MobileNo;
            _res.IsError = _res.EmpDetail.CommonInt == 0;
            if (!_res.IsError)
                _res.Token = HashEncryption.O.Encrypt(_res.EmpDetail.CommonInt2 + "");
            IProcedure proc = new ProcEmpSelectRole(_dal);
            _res.Roles = (List<RoleMaster>)proc.Call(_res.EmpDetail.CommonInt);
            return _res;
        }

        public List<RoleMaster> GetEmpRole(int EmpID)
        {
            IProcedure proc = new ProcEmpSelectRole(_dal);
            var res = (List<RoleMaster>)proc.Call(EmpID);
            return res;
        }

        public ResponseStatus CallCreateEmp(EmpInfo _req)
        {
            ResponseStatus _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            var loginRes = IsAdmin();
            if (loginRes != null)
            {
                string Token = HashEncryption.O.Decrypt(_req.Token ?? "");
                if (!Validate.O.IsNumeric(Token))
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " Referal";
                    return _resp;
                }
                if (Validate.O.IsNumeric(_req.Name ?? "") || (_req.Name ?? "").Length > 100)
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " Name";
                    return _resp;
                }
                if ((_req.EmpCode ?? "").Length > 100)
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " EmpCode";
                    return _resp;
                }
                if (!Validate.O.IsMobile(_req.MobileNo ?? ""))
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " Mobile Number";
                    return _resp;
                }
                if (!Validate.O.IsEmail(_req.EmailID ?? ""))
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " EmailID";
                    return _resp;
                }
                if (!Validate.O.IsPinCode(_req.Pincode ?? ""))
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " Pincode";
                    return _resp;
                }
                EmpDetailRequest _Empreq = new EmpDetailRequest
                {
                    empInfo = _req,
                    ReferalID = Convert.ToInt32(Token),
                    LoginID = loginRes.UserID,
                    LTID = loginRes.LoginTypeID,
                    Browser = _rinfo.GetBrowserFullInfo(),
                    IP = _rinfo.GetRemoteIP(),
                    Password = HashEncryption.O.CreatePassword(8)
                };
                IProcedure _p = new ProcEmpCreate(_dal);
                _resp = (ResponseStatus)_p.Call(_Empreq);
                if (_resp.Statuscode == ErrorCodes.One)
                {
                    AlertML alert = new AlertML(_accessor, _env);
                    var alertParam = new AlertReplacementModel
                    {
                        LoginID = loginRes.UserID,
                        Password = _Empreq.Password,
                        UserMobileNo = _req.MobileNo,
                        UserEmailID = _req.EmailID,
                        WID = loginRes.WID,
                        FormatID = MessageFormat.Registration,
                        WhatsappNo = _req.MobileNo,
                        TelegramNo = _req.MobileNo
                    };
                    alert.RegistrationSMS(alertParam);
                    alert.SocialAlert(alertParam);
                }
            }
            return _resp;
        }


        public IResponseStatus UnAssignEmployee(int UserID)
        {
            ResponseStatus _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var loginRes = IsAdmin();
            if (loginRes != null)
            {
                var req = new CommonReq
                {
                    LoginID = loginRes.UserID,
                    LoginTypeID = loginRes.LoginTypeID,
                    CommonInt = UserID
                };
                IProcedure _p = new ProcUnAssignEmployee(_dal);
                _resp = (ResponseStatus)_p.Call(req);
            }
            return _resp;
        }

        public EmployeeList GetEmployee(CommonFilter filter)
        {
            var res = new EmployeeList();
            var loginRes = IsAdmin();
            if (loginRes != null || filter.LoginID > 0)
            {
                filter.LoginID = filter.LoginID > 0 ? filter.LoginID : loginRes.UserID;
                filter.LT = filter.LoginID > 0 ? loginRes.LoginTypeID : LoginType.Employee;
                if (filter.Criteria > 0)
                {
                    if ((filter.CriteriaText ?? "") == "")
                    {
                        return res;
                    }
                }
                if (filter.Criteria == Criteria.OutletMobile)
                {
                    filter.MobileNo = filter.CriteriaText;
                }
                if (filter.Criteria == Criteria.EmailID)
                {
                    filter.EmailID = filter.CriteriaText;
                }
                if (filter.Criteria == Criteria.Name)
                {
                    filter.Name = filter.CriteriaText;
                }
                if (filter.Criteria == Criteria.UserID)
                {
                    var Prefix = Validate.O.Prefix(filter.CriteriaText);
                    if (Validate.O.IsNumeric(Prefix))
                        filter.UserID = Validate.O.IsNumeric(filter.CriteriaText) ? Convert.ToInt32(filter.CriteriaText) : filter.UserID;
                    var uid = Validate.O.LoginID(filter.CriteriaText);
                    filter.UserID = Validate.O.IsNumeric(uid) ? Convert.ToInt32(uid) : filter.UserID;
                }
                IProcedure proc = new ProcGetEmployee(_dal);
                res = (EmployeeList)proc.Call(filter);
                res.IsAdmin = loginRes.RoleID == Role.Admin ? true : false;
                res.LoginID = loginRes.UserID;
            }
            return res;
        }

        public EmpReg GetEmployeeByID(int EmpID)
        {
            var res = new EmpReg();
            var loginRes = IsAdmin();
            if (loginRes != null)
            {
                var filter = new CommonFilter
                {
                    LoginID = loginRes.UserID,
                    LT = loginRes.LoginTypeID,
                    UserID = EmpID
                };
                IProcedure proc = new ProcGetEmployeeByID(_dal);
                res = (EmpReg)proc.Call(filter);
                if (res.ReferralID == 0)
                {
                    res.ReferralID = loginRes.UserID;
                    res.ReferralBy = loginRes.Name;
                }
                res.Roles = GetEmpRole(loginRes.UserID);
            }
            return res;
        }

        public ResponseStatus ShowEmpPass(int EmpID)
        {
            IProcedure proc = new ProcShowEmpPass(_dal);
            var res = (ResponseStatus)proc.Call(EmpID);
            return res;
        }

        public ResponseStatus ResendEmpPass(int Id)
        {
            var resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            var loginRes = IsAdmin();
            if (loginRes != null)
            {
                var param = new CommonReq
                {
                    CommonInt = Id,
                    LoginID = loginRes.UserID
                };
                IProcedure proc = new ProcEmpResendPass(_dal);
                var res = (AlertReplacementModel)proc.Call(param);
                IAlertML ml = new AlertML(_accessor, _env);
                if (ml != null)
                {
                    ml.ForgetPasswordSMS(res);
                    ml.ForgetPasswordEmail(res);
                    resp.Statuscode = ErrorCodes.One;
                    resp.Msg = "Passwprd has been sent to your email and mobile successfully";
                }
            }
            return resp;
        }


        public ResponseStatus ChangeEmpStatus(int Id, int Is)
        {
            var Resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            var param = new CommonReq
            {
                CommonInt = Id,
                CommonInt2 = Is
            };

            IProcedure proc = new ProcChangeEmpSts(_dal);
            var res = (ResponseStatus)proc.Call(param);
            if (res != null)
            {
                Resp.Statuscode = ErrorCodes.One;
                Resp.Msg = "Status of employee has been changed Successfully";
            }
            return Resp;
        }


        public ResponseStatus AssignUserToEmp(int EmpID, string mobileNo)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            var loginRes = IsAdmin();
            if (loginRes != null)
            {

                var param = new CommonReq
                {
                    LoginTypeID = loginRes.LoginTypeID,
                    LoginID = loginRes.UserID,
                    CommonInt = EmpID,
                    CommonStr = mobileNo
                };
                IProcedure proc = new ProcAssignUserToEmp(_dal);
                res = (ResponseStatus)proc.Call(param);
            }
            return res;
        }

        public List<EList> SelectEmpByRoleBulk(int Id, bool OnlyChild = false)
        {
            var res = new List<EList>();
            var loginRes = IsAdmin();
            if (loginRes != null)
            {
                var param = new CommonReq
                {
                    LoginTypeID = loginRes.LoginTypeID,
                    LoginID = loginRes.UserID,
                    CommonInt = Id,
                    CommonBool = OnlyChild
                };
                IProcedure proc = new ProcSelectEmpByRole(_dal);
                res = (List<EList>)proc.Call(param);
            }
            return res;
        }

        public IEnumerable<EList> PossibleAMForEmp(int EmpId)
        {
            var res = new List<EList>();
            var loginRes = IsAdmin();
            if (loginRes != null)
            {
                var param = new CommonReq
                {
                    LoginTypeID = loginRes.LoginTypeID,
                    LoginID = loginRes.UserID,
                    CommonInt = EmpId
                };
                IProcedure proc = new ProcAvailableAMForEmp(_dal);
                res = (List<EList>)proc.Call(param);
            }
            return res;
        }

        public ResponseStatus ChangeEmpAssignee(int Id, int ReportingTo)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            var loginRes = IsAdmin();
            if (loginRes != null)
            {
                var param = new CommonReq
                {
                    LoginTypeID = loginRes.LoginTypeID,
                    LoginID = loginRes.UserID,
                    CommonInt = Id,
                    CommonInt2 = ReportingTo
                };
                IProcedure proc = new ProcChangeAssignee(_dal);
                res = (ResponseStatus)proc.Call(param);
            }
            return res;
        }

        public ResponseStatus ChangeEmpOtpStatus(int Id, int Is)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            var param = new CommonReq
            {
                CommonInt = Id,
                CommonInt2 = Is
            };
            IProcedure proc = new ProcChangeEmpOtpSts(_dal);
            res = (ResponseStatus)proc.Call(param);
            return res;
        }

        public EmployeeListU GetEmployeeeUser(CommonFilter filter)
        {

            var res = new EmployeeListU();
            var loginRes = IsAdmin();
            if (loginRes != null)
            {
                filter.LoginID = loginRes.UserID;
                filter.LT = loginRes.LoginTypeID;

                if (filter.Criteria > 0)
                {
                    if ((filter.CriteriaText ?? "") == "")
                    {
                        return res;
                    }
                }
                if (filter.Criteria == Criteria.OutletMobile)
                {
                    filter.MobileNo = filter.CriteriaText;
                }
                if (filter.Criteria == Criteria.EmailID)
                {
                    filter.EmailID = filter.CriteriaText;
                }
                if (filter.Criteria == Criteria.Name)
                {
                    filter.Name = filter.CriteriaText;
                }
                if (filter.Criteria == Criteria.UserID)
                {
                    var Prefix = Validate.O.Prefix(filter.CriteriaText);
                    if (Validate.O.IsNumeric(Prefix))
                        filter.UserID = Validate.O.IsNumeric(filter.CriteriaText) ? Convert.ToInt32(filter.CriteriaText) : filter.UserID;
                    var uid = Validate.O.LoginID(filter.CriteriaText);
                    filter.UserID = Validate.O.IsNumeric(uid) ? Convert.ToInt32(uid) : filter.UserID;
                }
                IProcedure proc = new ProcEmpUserList(_dal);
                res = (EmployeeListU)proc.Call(filter);
            }
            return res;
        }

        public IEnumerable<EmpUserList> TodayOutletsListForEmp(int userId = 0)
        {
            var loginRes = IsAdmin();
            IProcedure proc = new ProcTodayOutletsListForEmp(_dal);
            var res = (IEnumerable<EmpUserList>)proc.Call(userId == 0 ? loginRes.UserID : userId);
            return res;
        }

        public IEnumerable<UserPackageDetail> TodaySellPackages()
        {
            var loginRes = IsAdmin();
            IProcedure proc = new ProcTodaySellPackages(_dal);
            var res = (IEnumerable<UserPackageDetail>)proc.Call(loginRes.UserID);
            return res;
        }

        public EmployeeListU GetEmployeeUserChild(int ReffID, bool IsUp)
        {
            var res = new EmployeeListU();
            var loginRes = IsAdmin();
            if (loginRes != null)
            {
                var param = new CommonReq
                {
                    LoginID = loginRes.UserID,
                    LoginTypeID = loginRes.LoginTypeID,
                    IsListType = IsUp,
                    CommonInt = ReffID
                };
                IProcedure proc = new ProcEmpUserListchild(_dal);
                res = (EmployeeListU)proc.Call(param);
            }
            return res;
        }

        public List<EList> GetAllEmpInBulk()
        {
            var res = new List<EList>();
            var loginRes = IsAdmin();
            if (loginRes != null)
            {
                var param = new CommonReq
                {
                    LoginTypeID = loginRes.LoginTypeID,
                    LoginID = loginRes.UserID

                };
                IProcedure proc = new GetAllEmployee(_dal);
                res = (List<EList>)proc.Call(param);
            }
            return res;
        }

        #region Target
        public List<EmpTarget> GetEmpTarget(int EmpID, int OID)
        {
            var res = new List<EmpTarget>();
            var loginRes = IsAdmin();
            if (loginRes != null)
            {
                var Request = new CommonReq
                {
                    LoginTypeID = loginRes.LoginTypeID,
                    LoginID = loginRes.UserID,
                    CommonInt = EmpID,
                    CommonInt2 = OID,
                    CommonInt3 = ApplicationSetting.TargetType
                };
                IProcedure proc = new procGetEmpTarget(_dal);
                res = (List<EmpTarget>)proc.Call(Request);
            }

            return res;
        }


        public IResponseStatus SaveEmpTarget(EmpTarget req)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Validation Failed"
            };
            var loginRes = IsAdmin();
            if (loginRes != null)
            {
                if (req.Target < req.ChildTarget)
                {
                    res.Msg = "Target can't be lower than downline target";
                    return res;
                }
                req.TargetTypeID = ApplicationSetting.TargetType;
                req.LoginID = loginRes.UserID;
                req.LoginTypeID = loginRes.LoginTypeID;
                IProcedure proc = new ProcSaveEmpTarget(_dal);
                res = (ResponseStatus)proc.Call(req);
            }
            return res;
        }

        #endregion

        public AlertReplacementModel ChangePassword(ChangePassword ChPass)
        {
            AlertReplacementModel _res = new AlertReplacementModel
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };

            var loginRes = IsAdmin();
            if (loginRes != null)
            {
                if ((ChPass.NewPassword ?? "").Length < 8 || (ChPass.NewPassword ?? "").Length > 12)
                {
                    _res.Msg = "New Password length should be 8 to 12 with AlphaNumeric";
                    return _res;
                }
                if ((ChPass.OldPassword ?? "").Length < 8 || (ChPass.OldPassword ?? "").Length > 12)
                {
                    _res.Msg = "Old Password is invalid";
                    return _res;
                }
                var req = new LoginReq
                {
                    CommonStr = ChPass.NewPassword,
                    CommonStr2 = ChPass.OldPassword,
                    LoginID = loginRes.UserID,
                    LoginTypeID = loginRes.LoginTypeID,
                    Browser = _rinfo.GetBrowser(),
                    RequestIP = _rinfo.GetRemoteIP(),
                    RequestMode = RequestMode.PANEL,
                    CommonInt = loginRes.SessID
                };
                IProcedure proc = new ProcChangePassword(_dal);
                _res = (AlertReplacementModel)proc.Call(req);
            }
            return _res;
        }

        #region Reports
        public async Task<List<PSTReportEmp>> GetPSTReportEmp(CommonFilter filter)
        {
            var _resp = new List<PSTReportEmp>();
            var loginRes = IsAdmin();
            if (loginRes != null || filter.LoginID > 0)
            {
                filter.LoginID = filter.LoginID > 0 ? filter.LoginID : loginRes.UserID;
                filter.RoleID = filter.RoleID > 0 ? filter.RoleID : loginRes.RoleID;
                _dal = new DAL(_c.GetConnectionString(1));
                IProcedureAsync _proc = new ProcPSTReportForEmployee(_dal);
                _resp = (List<PSTReportEmp>)await _proc.Call(filter).ConfigureAwait(false);
            }
            return _resp;
        }

        public List<PSTReport> PSTDeatilReport(CommonFilter filter)
        {
            var _resp = new List<PSTReport>();
            var loginRes = IsAdmin();
            if (loginRes != null)
            {
                filter.LoginID = loginRes.UserID;
                IProcedure _proc = new ProcPSTDetailReportForEmp(_dal);
                _resp = (List<PSTReport>)_proc.Call(filter);
            }
            return _resp;
        }

        public async Task<List<TertiaryReport>> GetTertiaryReportEmp(CommonReq filter)
        {
            var _resp = new List<TertiaryReport>();
            var loginRes = IsAdmin();
            if (filter != null && loginRes != null)
            {
                _dal = new DAL(_c.GetConnectionString(1));
                filter.LoginTypeID = filter.LoginTypeID > 0 ? filter.LoginTypeID : loginRes.LoginTypeID;
                filter.LoginID = filter.LoginID > 0 ? filter.LoginID : loginRes.UserID;
                IProcedureAsync _proc = new ProcGetTertiaryReportForEmp(_dal);
                _resp = (List<TertiaryReport>)await _proc.Call(filter).ConfigureAwait(false);
            }
            return _resp;
        }

        public async Task<List<MeetingReportModel>> GetMeetingReport(CommonReq filter)
        {
            var _resp = new List<MeetingReportModel>();
            var loginRes = IsAdmin();
            if (filter != null && loginRes != null)
            {
                //  _dal = new DAL(_c.GetConnectionString(1));
                filter.LoginTypeID = filter.LoginTypeID > 0 ? filter.LoginTypeID : loginRes.LoginTypeID;
                filter.LoginID = filter.LoginID > 0 ? filter.LoginID : loginRes.UserID;
                IProcedureAsync _proc = new ProcGetMeetingReport(_dal);
                _resp = (List<MeetingReportModel>)await _proc.Call(filter).ConfigureAwait(false);
            }
            return _resp;
        }

        public async Task<List<MeetingAddOnReportModel>> GetMeetingDetailReport(CommonReq filter)
        {
            var _resp = new List<MeetingAddOnReportModel>();
            var loginRes = IsAdmin();
            if (filter != null && filter.CommonInt > 0)
            {
                _dal = new DAL(_c.GetConnectionString(1));
                filter.LoginTypeID = filter.LoginTypeID > 0 ? filter.LoginTypeID : loginRes.LoginTypeID;
                filter.LoginID = filter.LoginID > 0 ? filter.LoginID : loginRes.UserID;
                IProcedureAsync _proc = new ProcGetMeetingDetailReport(_dal);
                _resp = (List<MeetingAddOnReportModel>)await _proc.Call(filter).ConfigureAwait(false);
            }
            return _resp;
        }


        public async Task<IEnumerable<EmployeeTargetReport>> GetEmpTargetReport(CommonReq filter)
        {
            var _resp = new List<EmployeeTargetReport>();
            var loginRes = IsAdmin();
            if (filter != null && loginRes != null)
            {
                _dal = new DAL(_c.GetConnectionString(1));
                filter.LoginTypeID = filter.LoginTypeID > 0 ? filter.LoginTypeID : loginRes.LoginTypeID;
                filter.LoginID = filter.LoginID > 0 ? filter.LoginID : loginRes.UserID;
                IProcedureAsync _proc = new ProcGetEmpTargetReport(_dal);
                _resp = (List<EmployeeTargetReport>)await _proc.Call(filter).ConfigureAwait(false);
            }
            return _resp;
        }

        public IEnumerable<ComparisionChart> GetComparisionChart(int UserID = 0)
        {
            var _resp = new List<ComparisionChart>();
            var loginRes = IsAdmin();
            if (loginRes != null || UserID > 0)
            {
                var LoginID = UserID > 0 ? UserID : loginRes.UserID;
                _dal = new DAL(_c.GetConnectionString(1));
                IProcedure _proc = new ProcComparisionChart(_dal);
                _resp = (List<ComparisionChart>)_proc.Call(LoginID);
            }
            return _resp;
        }


        public IEnumerable<LastDayVsTodayData> GetLastdayVsTodayChart(int UserID = 0)
        {
            var _resp = new List<LastDayVsTodayData>();
            var loginRes = IsAdmin();
            if (loginRes != null || UserID > 0)
            {
                var LoginID = UserID > 0 ? UserID : loginRes.UserID;
                _dal = new DAL(_c.GetConnectionString(1));
                IProcedure _proc = new ProcLastDayVsTodayData(_dal);
                _resp = (List<LastDayVsTodayData>)_proc.Call(LoginID);
            }
            return _resp;
        }


        public IEnumerable<TargetSegment> GetTargetSegment(int UserID = 0)
        {
            var _resp = new List<TargetSegment>();
            var loginRes = IsAdmin();
            if (loginRes != null || UserID > 0)
            {
                _dal = new DAL(_c.GetConnectionString(1));
                var LoginID = UserID > 0 ? UserID : loginRes.UserID;
                IProcedure _proc = new ProcTargetSegment(_dal);
                _resp = (List<TargetSegment>)_proc.Call(LoginID);
            }
            return _resp;
        }

        public List<UserCommitment> GetUserCommitment(int UserID = 0)
        {
            var _resp = new List<UserCommitment>();
            var loginRes = IsAdmin();
            if (loginRes != null || UserID > 0)
            {
                var LoginID = UserID > 0 ? UserID : loginRes.UserID;
                IProcedure _proc = new ProcGetUserCommitment(_dal);
                _resp = (List<UserCommitment>)_proc.Call(LoginID);
            }
            return _resp;
        }

        public ResponseStatus SetUserCommitment(UserCommitment req)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            var loginRes = IsAdmin();
            if (loginRes != null || req.EmpID > 0)
            {
                req.EmpID = req.EmpID > 0 ? req.EmpID : loginRes.UserID;
                req.LoginTypeID = req.LoginTypeID > 0 ? req.LoginTypeID : loginRes.LoginTypeID;
                IProcedure proc = new ProcSaveUserCommitment(_dal);
                res = (ResponseStatus)proc.Call(req);
            }
            return res;
        }

        public List<EmpDownlineUser> GetEmpDownlineUser(int UserID = 0)
        {
            var _resp = new List<EmpDownlineUser>();
            var loginRes = IsAdmin();
            if (loginRes != null || UserID > 0)
            {
                var LoginID = UserID > 0 ? UserID : loginRes.UserID;
                IProcedure _proc = new ProcEmpDownline(_dal);
                _resp = (List<EmpDownlineUser>)_proc.Call(LoginID);
            }
            return _resp;
        }

        #endregion

        private LoginResponse IsAdmin()
        {
            var result = new LoginResponse();
            if (_lrEmp != null)
            {
                result = _lrEmp;
            }
            if (_lr != null && _lr.RoleID == Role.Admin)
            {
                result = _lr;

            }
            return result;
        }

        public IEnumerable<TodayLivePST> GetEmpTodayLivePST(int UserID = 0)
        {
            var _resp = new List<TodayLivePST>();
            var loginRes = IsAdmin();
            if (loginRes != null || UserID > 0)
            {
                var LoginID = UserID > 0 ? UserID : loginRes.UserID;
                IProcedure _proc = new ProcGetEmpTodayLivePST(_dal);
                _resp = (List<TodayLivePST>)_proc.Call(LoginID);
                if (ApplicationSetting.IsPackageAllowed == false)
                {
                    for (int i = 0; i < _resp.Count; i++)
                    {
                        if (_resp[i].Type.ToLower() == "package")
                        {
                            _resp.RemoveAt(i);
                        }
                    }
                }
            }
            return _resp;
        }


        public GetinTouctListModel GetUserSubscription(CommonReq req)
        {
            req.LoginID = _lrEmp.UserID;
            req.LoginTypeID = _lrEmp.LoginTypeID;
            var res = new GetinTouctListModel();
            IProcedure proc = new ProcGetUserSubscriptionCusCare(_dal);
            res = (GetinTouctListModel)proc.Call(req);
            res.CustomerCareList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(res.CustomerCareDetail, "CustomercareID", "CustomerCareName");
            return res;
        }

        public async Task<LeadStats> GetLeadStats()
        {
            IProcedureAsync proc = new ProcGetLeadStats(_dal);
            var res = (LeadStats)await proc.Call(new CommonReq { LoginID = _lrEmp.UserID, LoginTypeID = _lrEmp.LoginTypeID });
            return res ?? new LeadStats();
        }

        public IResponseStatus UpdationAgainstLead(LeadDetail req)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            req.LoginID = _lrEmp.UserID;
            req.LoginTypeID = _lrEmp.LoginTypeID;
            IProcedure _proc = new ProcUpdationAgainstLead(_dal);
            res = (ResponseStatus)_proc.Call(req);
            return res;
        }


        public IEnumerable<TodayTransactorsModal> TodayTransactors(int type, int userId = 0)
        {
            var res = new List<TodayTransactorsModal>();
            var loginRes = IsAdmin();
            if (loginRes != null)
            {
                var Request = new CommonReq
                {
                    LoginTypeID = loginRes.LoginTypeID,
                    LoginID = userId == 0 ? loginRes.UserID : userId,
                    CommonInt = type
                };
                IProcedure proc = new procTodayTransactors(_dal);
                res = (List<TodayTransactorsModal>)proc.Call(Request);
            }
            return res;
        }

        public IEnumerable<PSTComparisionTable> GetPSTComparisionTable(int UserID = 0)
        {
            var _resp = new List<PSTComparisionTable>();
            var loginRes = IsAdmin();
            if (loginRes != null || UserID > 0)
            {
                _dal = new DAL(_c.GetConnectionString(1));
                var LoginID = UserID > 0 ? UserID : loginRes.UserID;
                IProcedure _proc = new ProcGetPSTComparisionTable(_dal);
                _resp = (List<PSTComparisionTable>)_proc.Call(LoginID);
            }
            return _resp;
        }

        public IResponseStatus TransferLead(int Id, int TransferTo)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var req = new CommonReq
            {
                LoginID = _lrEmp.UserID,
                LoginTypeID = _lrEmp.LoginTypeID,
                CommonInt = Id,
                CommonInt2 = TransferTo
            };
            IProcedure _proc = new ProcAssignUserSubscription(_dal);
            _res = (ResponseStatus)_proc.Call(req);
            return _res;
        }
        public IEnumerable<PSTDataList> GetLastSevenDayPSTDataForEmp(int UserID = 0)
        {
            _dal = new DAL(_c.GetConnectionString(1));
            IProcedure _proc = new ProcGetLastSevenDayPSTDataForEmp(_dal);
            var res = (IEnumerable<PSTDataList>)_proc.Call(UserID > 0 ? UserID : _lrEmp.UserID);
            return res;
        }
        public IResponseStatus CreateLead(CreateLead req)
        {
            req.LoginID = _lrEmp.UserID;
            req.LoginTypeID = _lrEmp.LoginTypeID;
            req.RequestModeID = 3;
            req.RequestIP = _rinfo.GetRemoteIP();
            IProcedure proc = new ProcInsertContactUs(_dal);
            var res = (ResponseStatus)proc.Call(req);
            return res;
        }
        //--------------------------Ahmed Gulzar--------------------------//
        //--------------------------Sep 26, 2020--------------------------//
        public IResponseStatus CreateMeeting(Meetingdetails req)
        {
            if (req.LoginID == 0 && req.LoginTypeID == 0)
            {
                req.LoginID = _lrEmp.UserID;
                req.LoginTypeID = _lrEmp.LoginTypeID;
            }
            IProcedure proc = new ProcInsertMeetingDetail(_dal);
            var res = (ResponseStatus)proc.Call(req);
            return res;
        }
        public ReasonAndPurpuse ReasonandPurpuse()
        {
            IProcedure proc = new ProcGetResaonAndPurpuse(_dal);
            var res = (ReasonAndPurpuse)proc.Call();
            return res;
        }
        public bool IsClosingDone(int UserId = 0, int LoginType = 0)
        {
            var req = new CommonReq();
            if (UserId == 0 && LoginType == 0)
            {
                req.LoginID = _lrEmp.UserID;
                req.LoginTypeID = _lrEmp.LoginTypeID;
            }
            else
            {
                req.LoginID = UserId;
                req.LoginTypeID = LoginType;
            }

            IProcedure proc = new ProcGetClosingStatus(_dal);
            var res = (bool)proc.Call(req);
            return res;
        }
        public Meetingdetails GetUserdatabyMobileNo(string MobileNo, int UserId = 0, int LoginTypeId = 0)
        {
            var req = new CommonReq();
            if (UserId == 0 && LoginTypeId == 0)
            {
                req.LoginID = _lrEmp.UserID;
                req.LoginTypeID = _lrEmp.LoginTypeID;
            }
            else
            {
                req.LoginID = UserId;
                req.LoginTypeID = LoginTypeId;
            }
            req.CommonStr = MobileNo;
            IProcedure proc = new ProcGetUsersDetailsbyMobileNo(_dal);
            var res = (Meetingdetails)proc.Call(req);
            return res;
        }
        public IEnumerable<PincodeDetail> GetAreabypincode(int Pincode, int UserId = 0, int LoginTypeId = 0)
        {
            var req = new CommonReq();
            if (UserId == 0 && LoginTypeId == 0)
            {
                req.LoginID = _lrEmp.UserID;
                req.LoginTypeID = _lrEmp.LoginTypeID;
            }
            else
            {
                req.LoginID = UserId;
                req.LoginTypeID = LoginTypeId;
            }
            req.CommonInt2 = Pincode;
            IProcedure proc = new ProcGetPincodearea(_dal);
            var res = (List<PincodeDetail>)proc.Call(req);
            return res;
        }
        public List<Meetingdetails> GetMeetingdetails(CommonReq filter = null)
        {
            var loginRes = IsAdmin();
            if (filter != null)
            {
                if (filter.LoginID == 0 && filter.LoginTypeID == 0)
                {
                    //filter.LoginID = _lrEmp.UserID;
                    //filter.LoginTypeID = _lrEmp.LoginTypeID;
                    filter.LoginTypeID = _lrEmp != null && _lrEmp.LoginTypeID > 0 ? _lrEmp.LoginTypeID : loginRes.LoginTypeID;
                    filter.LoginID = _lrEmp != null && _lrEmp.UserID > 0 ? _lrEmp.UserID : loginRes.UserID;
                }
                IProcedure proc = new ProcGetMeetingDetailRpt(_dal);
                var res = (List<Meetingdetails>)proc.Call(filter);
                return res;
            }
            else
            {
                var req = new CommonReq
                {
                    LoginID = _lrEmp != null && _lrEmp.UserID > 0 ? _lrEmp.UserID : loginRes.UserID,
                    LoginTypeID = _lrEmp != null && _lrEmp.LoginTypeID > 0 ? _lrEmp.LoginTypeID : loginRes.LoginTypeID
                };
                IProcedure proc = new ProcGetMeetingDetail(_dal);
                var res = (List<Meetingdetails>)proc.Call(req);
                return res;
            }
        }
        public IResponseStatus DailyClosing(DailyClosingModel req)
        {
            if (req.Expense <= 0 && req.Travel <= 0)
            {
                ResponseStatus _resp = new ResponseStatus
                {
                    Statuscode = ErrorCodes.Minus1,
                    Msg = ErrorCodes.AnError
                };
                return _resp;
            }
            else
            {
                if (req.LoginID == 0 && req.LoginTypeID == 0)
                {
                    req.LoginID = _lrEmp.UserID;
                    req.LoginTypeID = _lrEmp.LoginTypeID;
                }
                IProcedure proc = new ProcInsertDailyClosing(_dal);
                var res = (ResponseStatus)proc.Call(req);
                return res;
            }
        }
        //--------------------------Sep 26, 2020--------------------------//
        //--------------------------Sep 28, 2020--------------------------//
        public async Task<List<MapPointsModel>> GetMapPoints(CommonReq filter)
        {
            var _resp = new List<MapPointsModel>();
            var loginRes = IsAdmin();
            if (filter != null && filter.CommonInt > 0)
            {
                _dal = new DAL(_c.GetConnectionString(1));
                filter.LoginTypeID = filter.LoginTypeID > 0 ? filter.LoginTypeID : loginRes.LoginTypeID;
                filter.LoginID = filter.LoginID > 0 ? filter.LoginID : loginRes.UserID;
                IProcedureAsync _proc = new ProcGetMapPointsById(_dal);
                _resp = (List<MapPointsModel>)await _proc.Call(filter).ConfigureAwait(false);
            }
            return _resp;
        }
        //--------------------------Sep 28, 2020--------------------------//
        public async Task<AttendanceReportModel> GetAttendanceReport(CommonReq filter)
        {
            var _resp = new AttendanceReportModel();
            var loginRes = IsAdmin();
            if (filter != null && loginRes != null)
            {
                _dal = new DAL(_c.GetConnectionString(1));
                filter.LoginTypeID = filter.LoginTypeID > 0 ? filter.LoginTypeID : loginRes.LoginTypeID;
                filter.LoginID = filter.LoginID > 0 ? filter.LoginID : loginRes.UserID;
                IProcedureAsync _proc = new ProcGetAttendanceReport(_dal);
                _resp = (AttendanceReportModel)await _proc.Call(filter).ConfigureAwait(false);
            }
            return _resp;
        }
    }
}
