using Fintech.AppCode;
using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using NUglify.Helpers;
using RoundpayFinTech.AppCode.Configuration;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Validators;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public class SwitchingML : ISwitchingML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly ISession _session;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        private readonly IRequestInfo _rinfo;
        private readonly LoginResponse _lr;
        private readonly IUserML userML;
        public SwitchingML(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _session = _accessor.HttpContext.Session;
            _dal = new DAL(_c.GetConnectionString());
            _rinfo = new RequestInfo(_accessor, _env);
            _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
            userML = new UserML(_lr);
        }

        public IEnumerable<PriorityApiSwitch> GetAPISwitching(int OpTypeID)
        {
            List<PriorityApiSwitch> PriorityApiSwitchs = new List<PriorityApiSwitch>();
            if (_lr.LoginTypeID == LoginType.ApplicationUser && !userML.IsEndUser() || userML.IsCustomerCareAuthorised(ActionCodes.APISwitch))
            {
                IProcedure _proc = new ProcGetPriorityApiSwitching(_dal);
                PriorityApiSwitchs = (List<PriorityApiSwitch>)_proc.Call(new CommonReq
                {
                    CommonInt = OpTypeID,
                    LoginID = _lr.LoginTypeID == LoginType.CustomerCare ? 1 : _lr.UserID
                });
            }
            return PriorityApiSwitchs;
        }
        public IResponseStatus SwitchAPI(APISwitched switched)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if ((!userML.IsEndUser() && _lr.LoginTypeID == LoginType.ApplicationUser) || userML.IsCustomerCareAuthorised(ActionCodes.APISwitch))
            {
                IProcedure _proc = new ProcUpdateAPISwitching(_dal);
                _res = (ResponseStatus)_proc.Call(new APISwitchedReq
                {
                    aPISwitched = switched,
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonStr = _rinfo.GetRemoteIP(),
                    CommonStr2 = _rinfo.GetBrowser()
                });
            }
            return _res;
        }
        public IEnumerable<APIOpCode> GetAPISwitchByUser(int ID, int OpTypeID)
        {
            var aPIOpCodes = new List<APIOpCode>();
            if ((!userML.IsEndUser() && _lr.LoginTypeID == LoginType.ApplicationUser) || userML.IsCustomerCareAuthorised(ActionCodes.APISwitch))
            {
                IProcedure _proc = new ProcGetAPISwitchByUser(_dal);
                aPIOpCodes = (List<APIOpCode>)_proc.Call(new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonInt = ID,
                    CommonInt2 = OpTypeID
                });
            }
            return aPIOpCodes;
        }
        public IEnumerable<UserWiseLimitResp> GetUserLimitByUser(int ID)
        {
            List<UserWiseLimitResp> objUserWiseLimitResp = new List<UserWiseLimitResp>();
            if ((_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser) || (userML.IsCustomerCareAuthorised(ActionCodes.APISwitch)))
            {
                CommonReq req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = ID
                };
                ProcGetUserWiseLimit _proc = new ProcGetUserWiseLimit(_dal);
                objUserWiseLimitResp = (List<UserWiseLimitResp>)_proc.Call(req);
            }
            return objUserWiseLimitResp;
        }
        public IResponseStatus UserwiseLimitCU(UserLimitCUReq userLimitCUReq)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            userLimitCUReq.LoginTypeID = _lr.LoginTypeID;
            if ((_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser))
            {
                IProcedure _proc = new ProcUserwiseLimitCU(_dal);
                _res = (ResponseStatus)_proc.Call(userLimitCUReq);
            }
            return _res;
        }

        public IResponseStatus SwitchUserwiseAPI(SwitchAPIUser switchAPIUser)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if ((!userML.IsEndUser() && _lr.LoginTypeID == LoginType.ApplicationUser) || userML.IsCustomerCareAuthorised(ActionCodes.APISwitch))
            {
                IProcedure _proc = new ProcUpdateUserwiseAPISwitch(_dal);
                _res = (ResponseStatus)_proc.Call(new SwitchAPIUserReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    switchAPIUser = switchAPIUser,
                    CommonStr = _rinfo.GetRemoteIP(),
                    CommonStr2 = _rinfo.GetBrowser()
                });
            }
            return _res;
        }

        public IEnumerable<CircleSwitch> GetCircleSwitches(int APIID)
        {
            var circleSwitches = new List<CircleSwitch>();
            if (APIID < 1)
            {
                return circleSwitches;
            }
            if (_lr.LoginTypeID == LoginType.ApplicationUser && !userML.IsEndUser() || userML.IsCustomerCareAuthorised(ActionCodes.APISwitch))
            {
                IProcedure _proc = new ProcGetCircleSwitched(_dal);
                circleSwitches = (List<CircleSwitch>)_proc.Call(new CommonReq
                {
                    CommonInt = APIID,
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID
                });
            }
            return circleSwitches;
        }
        public IEnumerable<CircleSwitch> GetCircleBlocked()
        {
            var circleSwitches = new List<CircleSwitch>();
            if ((_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.APISwitch)) && ApplicationSetting.IsCircleSlabShow)
            {
                IProcedure _proc = new ProcGetCircleBlocked(_dal);
                circleSwitches = (List<CircleSwitch>)_proc.Call(null);
            }
            return circleSwitches;
        }
        public IResponseStatus UpdateCircleSwitch(Circle circle)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Access Denied!"
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser && !userML.IsEndUser() || userML.IsCustomerCareAuthorised(ActionCodes.APISwitch))
            {
                IProcedure _proc = new ProcUpdateCircleSwitch(_dal);
                _res = (ResponseStatus)_proc.Call(new CircleReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    circle = circle,
                    CommonStr = _rinfo.GetRemoteIP(),
                    CommonStr2 = _rinfo.GetBrowser(),
                    CommonInt= circle.MaxCount
                });
            }
            return _res;
        }
        public IResponseStatus UpdateCircleBlock(Circle circle)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Access Denied!"
            };
            if ((_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.APISwitch)) && ApplicationSetting.IsCircleSlabShow)
            {
                var req = new CircleReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    circle = circle,
                    CommonStr = _rinfo.GetRemoteIP(),
                    CommonStr2 = _rinfo.GetBrowser()
                };
                IProcedure _proc = new ProcUpdateCircleBlock(_dal);
                _res = (ResponseStatus)_proc.Call(req);
            }
            return _res;
        }

        public IResponseStatus UpdateDenomination(APIDenominationReq req)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if ((!userML.IsEndUser() && _lr.LoginTypeID == LoginType.ApplicationUser) || userML.IsCustomerCareAuthorised(ActionCodes.APISwitch))
            {
                req.LoginTypeID = _lr.LoginTypeID;
                req.LoginID = _lr.UserID;
                req.CommonStr = _rinfo.GetRemoteIP();
                req.CommonStr2 = _rinfo.GetBrowser();
                IProcedure _proc = new ProcUpdateAPIDenomination(_dal);
                _res = (ResponseStatus)_proc.Call(req);
            }
            return _res;
        }
        public IResponseStatus UpdateDenominationUser(APIDenominationReq req)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if ((!userML.IsEndUser() && _lr.LoginTypeID == LoginType.ApplicationUser) || userML.IsCustomerCareAuthorised(ActionCodes.APISwitch))
            {
                req.LoginTypeID = _lr.LoginTypeID;
                req.LoginID = _lr.UserID;
                req.CommonStr = _rinfo.GetRemoteIP();
                req.CommonStr2 = _rinfo.GetBrowser();
                IProcedure _proc = new ProcUpdateAPIDenominationUser(_dal);
                _res = (ResponseStatus)_proc.Call(req);
            }
            return _res;
        }
        public IEnumerable<DSRDesign> GetDSwitchReport(int OpTypeID)
        {
            var FList = new List<DSRDesign>();
            IProcedure f = new ProcGetDenominationSwitchReport(_dal);
            var _list = (List<DSwitchReport>)f.Call(new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID,
                CommonStr = _rinfo.GetRemoteIP(),
                CommonStr2 = _rinfo.GetBrowser(),
                CommonInt = OpTypeID
            });
            if (_list != null && _list.Count > 0)
            {
                var dsr = _list.Select(x => new DSRDesign { OID = x.OID, Operator = x.Operator, CircleName = x.CircleName }).DistinctBy(x => new { x.OID, x.CircleName }).ToList();
                foreach (var i in dsr)
                {
                    FList.Add(new DSRDesign
                    {
                        OID = i.OID,
                        Operator = i.Operator,
                        DSList = _list.Where(x => x.OID == i.OID && x.CircleName.ToLower().Trim() == i.CircleName.ToLower().Trim()).ToList(),
                        CircleName = i.CircleName
                    });
                }
            }
            return FList;
        }

        public IResponseStatus RemoveDenominationUser(int ID)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if ((!userML.IsEndUser() && _lr.LoginTypeID == LoginType.ApplicationUser) || userML.IsCustomerCareAuthorised(ActionCodes.APISwitch))
            {
                CommonReq req = new CommonReq
                {
                    LoginID = _lr.UserID,
                    LoginTypeID = _lr.LoginTypeID,
                    CommonInt = ID
                };

                IProcedure _proc = new ProcRemoveAPIDenominationUser(_dal);
                _res = (ResponseStatus)_proc.Call(req);
            }
            return _res;
        }

        public IResponseStatus RemoveDenomination(int ID)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if ((!userML.IsEndUser() && _lr.LoginTypeID == LoginType.ApplicationUser) || userML.IsCustomerCareAuthorised(ActionCodes.APISwitch))
            {
                CommonReq req = new CommonReq
                {
                    LoginID = _lr.UserID,
                    LoginTypeID = _lr.LoginTypeID,
                    CommonInt = ID
                };

                IProcedure _proc = new ProcRemoveAPIDenomination(_dal);
                _res = (ResponseStatus)_proc.Call(req);
            }
            return _res;
        }

        public IEnumerable<DSRDesign> GetDSwitchReportUser(int OpTypeID, int UserID, string MobileNo)
        {
            var req = new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID,
                CommonStr = _rinfo.GetRemoteIP(),
                CommonStr2 = _rinfo.GetBrowser(),
                CommonStr3 = MobileNo,
                CommonInt = UserID,
                CommonInt2 = OpTypeID
            };
            var FList = new List<DSRDesign>();
            IProcedure f = new ProcGetDenominationSwitchReportUser(_dal);
            var _list = (List<DSwitchReport>)f.Call(req);
            if (_list != null && _list.Count > 0)
            {
                var dsr = _list.GroupBy(g => new { g.OID, g.Operator, g.UserID, g.OutletName, g.Role, g.MobileNo, g.Prefix, g.CircleName }).Select(x => new DSRDesign { OID = x.Key.OID, Operator = x.Key.Operator, OutletName = x.Key.OutletName, MobileNo = x.Key.MobileNo, UserID = x.Key.UserID, Role = x.Key.Role, Prefix = x.Key.Prefix, CircleName = x.Key.CircleName }).ToList();
                foreach (var i in dsr)
                {
                    FList.Add(new DSRDesign
                    {
                        OID = i.OID,
                        Operator = i.Operator,
                        UserID = i.UserID,
                        OutletName = i.OutletName,
                        MobileNo = i.MobileNo,
                        Prefix = i.Prefix,
                        Role = i.Role,
                        DSList = _list.Where(x => x.OID == i.OID && x.UserID == i.UserID && x.CircleName.ToLower().Trim() == i.CircleName.ToLower().Trim()).ToList(),
                        CircleName = i.CircleName
                    });
                }
            }
            return FList;
        }


        public IResponseStatus UpdateOperatorStatus(UpdateDownStatusReq updateDownStatusReq)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Access Denied!"
            };
            if (updateDownStatusReq == null)
            {
                _resp.Msg = "Invalid request!!";
                return _resp;
            }
            if (updateDownStatusReq.DataKVs == null)
            {
                _resp.Msg = "No record to update!";
                return _resp;
            }
            if (updateDownStatusReq.DataKVs.Count == 0)
            {
                _resp.Msg = "No record to update!";
                return _resp;
            }
            if ((!userML.IsEndUser() && _lr.LoginTypeID == LoginType.ApplicationUser) || userML.IsCustomerCareAuthorised(ActionCodes.APISwitch))
            {
                updateDownStatusReq.LoginID = _lr.UserID;
                updateDownStatusReq.LT = _lr.LoginTypeID;
                updateDownStatusReq.IP = _rinfo.GetRemoteIP();
                updateDownStatusReq.Browser = _rinfo.GetBrowser();

                ProcUpdateDownStatus _proc = new ProcUpdateDownStatus(_dal);
                UpdateDownStatus updateDownStatus = (UpdateDownStatus)_proc.Call(updateDownStatusReq);
                _resp.Statuscode = updateDownStatus.Statuscode;
                _resp.Msg = updateDownStatus.Msg;
                if (updateDownStatus.Statuscode == ErrorCodes.One)
                {
                    var alertParam = new AlertReplacementModel
                    {
                        Company = updateDownStatus.Company,
                        CompanyDomain = updateDownStatus.CompanyDomain,
                        CompanyAddress = updateDownStatus.CompanyAddress,
                        AccountEmail = updateDownStatus.AccountEmail,
                        AccountsContactNo = updateDownStatus.AccountContactNumber,
                        SupportEmail = updateDownStatus.SupportEmail,
                        SupportNumber = updateDownStatus.SupportNumber,
                        WID = updateDownStatus.WID,
                        LoginID = updateDownStatus.LoginID,
                        UserName = updateDownStatus.UserName,
                        OutletName = updateDownStatus.OutletName,
                        UserMobileNo = updateDownStatus.UserMobileNo,
                        BrandName = updateDownStatus.BrandName
                    };
                    IAlertML alert = new AlertML(_accessor, _env);

                    alertParam.Operator = updateDownStatus.UpOperators != "" ? updateDownStatus.UpOperators : updateDownStatus.DownOperators;

                    if (updateDownStatusReq.IsSMS)
                    {
                        alertParam.UserID = -1;
                        //Send Bulk Notification
                        if (updateDownStatus.UpMessage.Trim().Length > 0)
                        {
                            alert.OperatorUpNotification(alertParam, true, true);
                        }
                        if (updateDownStatus.DownMessage.Trim().Length > 0)
                        {
                            alert.OperatorUpNotification(alertParam, false, true);
                        }
                    }
                    if (updateDownStatusReq.IsEmail)
                    {
                        //Send Bulk Email
                        List<string> Emails = updateDownStatus.UserData.Select(x => x.EmailID).ToList();
                        if (Emails.Count > 0)
                        {
                            List<List<string>> EmailsList = new List<List<string>>();
                            while (Emails.Any())
                            {
                                EmailsList.Add(Emails.Take(49).ToList());
                                Emails = Emails.Skip(49).ToList();
                            }

                            if (updateDownStatus.UpMessage.Trim().Length > 0)
                            {
                                foreach (var emailList in EmailsList)
                                {
                                    alertParam.bccList = emailList;
                                    alert.OperatorUpEmail(alertParam, true);
                                }
                            }
                            if (updateDownStatus.DownMessage.Trim().Length > 0)
                            {
                                foreach (var emailList in EmailsList)
                                {
                                    alertParam.bccList = emailList;
                                    alert.OperatorUpEmail(alertParam, false);
                                }
                            }
                        }
                    }
                    alertParam.FormatID = updateDownStatus.UpMessage.Trim().Length > 0 ? MessageFormat.OperatorUPMessage : MessageFormat.OperatorDownMessage;
                    alertParam.NotificationTitle = updateDownStatus.UpMessage.Trim().Length > 0 ? "Operator UP" : "Operator Down";
                    alertParam.UserID = -1;
                    Parallel.Invoke(async () => alert.WebNotification(alertParam));
                }
            }
            return _resp;
        }
        public IEnumerable<Userswitch> Userswitches(string MobileNo, int OpTypeID)
        {
            var res = new List<Userswitch>();
            if (_lr.LoginTypeID == LoginType.ApplicationUser && !userML.IsEndUser() || userML.IsCustomerCareAuthorised(ActionCodes.APISwitch))
            {
                var _req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonStr = MobileNo ?? ""
                };
                if (_req.CommonStr != "")
                {
                    if (!Validate.O.IsMobile(_req.CommonStr))
                    {
                        var Prefix = Validate.O.Prefix(_req.CommonStr);
                        if (Validate.O.IsNumeric(Prefix))
                            _req.CommonInt = Validate.O.IsNumeric(_req.CommonStr) ? Convert.ToInt32(_req.CommonStr) : _req.CommonInt;
                        var uid = Validate.O.LoginID(_req.CommonStr);
                        _req.CommonInt = Validate.O.IsNumeric(uid) ? Convert.ToInt32(uid) : _req.CommonInt;
                        _req.CommonStr = string.Empty;
                    }
                }
                _req.CommonInt2 = OpTypeID;
                IProcedure proc = new ProcUserwiseSwitchReport(_dal);
                return (List<Userswitch>)proc.Call(_req);
            }
            return res;
        }
        public IResponseStatus SetAPI(APISwitched switched)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if ((!userML.IsEndUser() && _lr.LoginTypeID == LoginType.ApplicationUser) || userML.IsCustomerCareAuthorised(ActionCodes.SetBackupAPI))
            {
                var req = new APISwitchedReq
                {
                    aPISwitched = switched,
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonStr = _rinfo.GetRemoteIP(),
                    CommonStr2 = _rinfo.GetBrowser()
                };
                IProcedure _proc = new ProcSetAPISwitching(_dal);
                _res = (ResponseStatus)_proc.Call(req);
            }
            return _res;
        }
        public IResponseStatus BlockUsersForSwitching(int UserID, int SwithID)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if ((_lr.LoginTypeID == LoginType.ApplicationUser && !userML.IsEndUser()) || userML.IsCustomerCareAuthorised(ActionCodes.APISwitch))
            {
                IProcedure proc = new ProcBlockSwithingForUser(_dal);
                res = (ResponseStatus)proc.Call(new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonInt = UserID,
                    CommonInt2 = SwithID,
                    CommonStr = _rinfo.GetRemoteIP(),
                    CommonStr2 = _rinfo.GetBrowserFullInfo()
                });
            }
            return res;
        }
        public List<CircleAPISwitchDetail> GetCircleMultiSwitchedDetail(int CircleID)
        {
            var res = new List<CircleAPISwitchDetail>();
            if (_lr.LoginTypeID == LoginType.ApplicationUser && !userML.IsEndUser() || userML.IsCustomerCareAuthorised(ActionCodes.APISwitch))
            {
                IProcedure _proc = new ProcGetCircleswitchingMulti(_dal);
                res = (List<CircleAPISwitchDetail>)_proc.Call(new CommonReq
                {
                    CommonInt = CircleID,
                    LoginID = _lr.LoginTypeID == LoginType.CustomerCare ? 1 : _lr.UserID
                });
            }
            return res;
        }
    }
}
