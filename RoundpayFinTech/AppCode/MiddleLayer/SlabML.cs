using Fintech.AppCode;
using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System.Collections.Generic;
using System.IO;
using Validators;
using System.Linq;
using System.Threading.Tasks;
using RoundpayFinTech.AppCode.Model.App;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public class SlabML : ISlabML
    {
        private readonly IConfiguration Configuration;
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly ISession _session;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        private readonly IRequestInfo _rinfo;
        private readonly WebsiteInfo _WInfo;
        private readonly LoginResponse _lr;
        private readonly IUserML userML;

        public SlabML(IHttpContextAccessor accessor, IHostingEnvironment env, bool IsSession = true)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            _rinfo = new RequestInfo(_accessor, _env);
            if (IsSession)
            {
                _session = _accessor.HttpContext.Session;
                _WInfo = new LoginML(_accessor, _env).GetWebsiteInfo();
                _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
                userML = new UserML(_lr);
            }
            bool IsProd = _env.IsProduction();
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile((IsProd ? "appsettings.json" : "appsettings.Development.json"));
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IResponseStatus UpdateAPICommission(SlabCommission apiCommission)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            if ((!userML.IsEndUser() && _lr.LoginTypeID == LoginType.ApplicationUser) || (_lr.LoginTypeID == LoginType.CustomerCare && userML.IsCustomerCareAuthorised(ActionCodes.AddEditAPI)))
            {
                if (apiCommission.OID < 1 || apiCommission.SlabID < 1)
                {
                    _resp.Msg = "Incomplete request!";
                    return _resp;
                }
                if (!apiCommission.CommType.In(0, 1))
                {
                    _resp.Msg = "Invalid Commission Type!";
                    return _resp;
                }
                if (!apiCommission.AmtType.In(0, 1))
                {
                    _resp.Msg = "Invalid Amount Type!";
                    return _resp;
                }
                var slabRequest = new SlabRequest
                {
                    Commission = apiCommission,
                    LoginID = _lr.UserID,
                    LoginTypeID = _lr.LoginTypeID,
                    CommonStr = _rinfo.GetRemoteIP(),
                    CommonStr2 = _rinfo.GetBrowser()
                };
                IProcedure _proc = new ProcUpdateAPIComm(_dal);
                _resp = (ResponseStatus)_proc.Call(slabRequest);
            }
            return _resp;
        }

        public IEnumerable<SlabMaster> GetSlabMaster()
        {
            var resp = new List<SlabMaster>();

            if (_lr.LoginTypeID == LoginType.ApplicationUser && !userML.IsEndUser() || userML.IsCustomerCareAuthorised(ActionCodes.AddEditSLAB))
            {
                var req = new CommonReq
                {
                    LoginID = _lr.UserID,
                    LoginTypeID = _lr.LoginTypeID,
                    CommonInt = -1
                };
                IProcedure proc = new ProcGetSlabMaster(_dal);
                resp = (List<SlabMaster>)proc.Call(req);
            }
            return resp;
        }

        public SlabMaster GetSlabMaster(int SlabID)
        {
            var resp = new SlabMaster();

            if (_lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.AddEditAPI))
            {
                var req = new CommonReq
                {
                    LoginID = _lr.UserID,
                    LoginTypeID = _lr.LoginTypeID,
                    CommonInt = SlabID
                };
                IProcedure proc = new ProcGetSlabMaster(_dal);
                resp = (SlabMaster)proc.Call(req);
            }
            return resp;
        }

        public IResponseStatus UpdateSlabMaster(SlabMaster slabMaster)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            if ((_lr.LoginTypeID == LoginType.ApplicationUser && !_lr.RoleID.In(Role.APIUser, Role.Customer, Role.Retailor_Seller)) || userML.IsCustomerCareAuthorised(ActionCodes.AddEditSLAB))
            {
                Validate validate = Validate.O;
                if (validate.IsNumeric(slabMaster.Slab ?? "1") || validate.IsStartsWithNumber(slabMaster.Slab))
                {
                    _resp.Msg = "Slab name is non numeric mandatory field and can not be start with number.";
                    return _resp;
                }
                var req = new SlabMasterReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    slabMaster = slabMaster,
                    CommonStr = _rinfo.GetRemoteIP(),
                    CommonStr2 = _rinfo.GetBrowser()
                };
                IProcedure _proc = new ProcUpdateSlabMaster(_dal);
                _resp = (ResponseStatus)_proc.Call(req);
            }
            return _resp;
        }

        public SlabDetailModel GetSlabDetail(int SlabID, int OpTypeID)
        {
            var res = new SlabDetailModel { };
            if (_lr.LoginTypeID == LoginType.ApplicationUser || (userML.IsCustomerCareAuthorised(ActionCodes.AddEditSLAB)))
            {
                var req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonInt = SlabID,
                    CommonInt2 = OpTypeID,
                    IsListType = _lr.IsAdminDefined || (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser)
                };
                IProcedure proc = new ProcGetSlabDetail(_dal);
                res = (SlabDetailModel)proc.Call(req);
                if (res.IsAdminDefined)
                {
                    List<OperatorDetail> operatorDetails = res.SlabDetails
                        .GroupBy(x => new { x.OID, x.Operator, x.OperatorType, x.IsBBPS })
                        .Select(g => new OperatorDetail { OID = g.Key.OID, Operator = g.Key.Operator, OperatorType = g.Key.OperatorType, IsBBPS = g.Key.IsBBPS })
                        .ToList();
                    res.Operators = operatorDetails;
                }
            }
            return res;
        }
        public SlabDetailModel GetSlabDetailGI(int SlabID, int OpTypeID)
        {
            var res = new SlabDetailModel { };
            if (_lr.LoginTypeID == LoginType.ApplicationUser || (userML.IsCustomerCareAuthorised(ActionCodes.AddEditSLAB)))
            {
                var req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonInt = SlabID,
                    CommonInt2 = OpTypeID,
                    IsListType = _lr.IsAdminDefined || (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser)
                };
                IProcedure proc = new Proc_GetSlabCommGI(_dal);
                res = (SlabDetailModel)proc.Call(req);
                if (res.IsAdminDefined)
                {
                    List<OperatorDetail> operatorDetails = res.SlabDetails
                        .GroupBy(x => new { x.OID, x.Operator, x.OperatorType, x.IsBBPS })
                        .Select(g => new OperatorDetail { OID = g.Key.OID, Operator = g.Key.Operator, OperatorType = g.Key.OperatorType, IsBBPS = g.Key.IsBBPS })
                        .ToList();
                    res.Operators = operatorDetails;
                }
            }
            return res;
        }

        public IEnumerable<SlabDetailDisplayLvl> GetSlabDetailForDisplay()
        {
            var res = new List<SlabDetailDisplayLvl> { };
            if (_lr.LoginTypeID == LoginType.ApplicationUser)
            {
                var req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID
                };
                IProcedure proc = new ProcGetSlabDetailDisplayLvL(_dal);
                var slabDetailModel = (SlabDetailModel)proc.Call(req);
                if (slabDetailModel.SlabDetails.Any())
                {
                    var operatorDetails = slabDetailModel.SlabDetails
                        .GroupBy(x => new { x.OID, x.Operator, x.OperatorType, x.IsBBPS, x.SPKey, x.Min, x.Max, x.BusinessModel, x.CommSettingType, x.ServiceID, x.SCode, x.AllowChannel, x.IsBilling,x.IsSpecialOp })
                        .Select(g => new OperatorDetail { OID = g.Key.OID, Operator = g.Key.Operator, OperatorType = g.Key.OperatorType, IsBBPS = g.Key.IsBBPS, SPKey = g.Key.SPKey, Min = g.Key.Min, Max = g.Key.Max, BusinessModel = g.Key.BusinessModel, CommSettingType = g.Key.CommSettingType, ServiceID = g.Key.ServiceID, SCode = g.Key.SCode, AllowChannel = g.Key.AllowChannel, IsBilling = g.Key.IsBilling,IsSpecialOp=g.Key.IsSpecialOp })
                        .ToList();

                    foreach (var op in operatorDetails)
                    {
                        var slabDetailDisplayLvl = new SlabDetailDisplayLvl
                        {
                            OID = op.OID,
                            Operator = op.Operator,
                            OpType = op.IsBBPS ? op.OperatorType + ",BBPS" : op.OperatorType,
                            SPKey = op.SPKey,
                            IsBBPS = op.IsBBPS,
                            IsBilling = op.IsBilling,
                            Min = op.Min,
                            Max = op.Max,
                            BusinessModel = op.BusinessModel,
                            RoleCommission = new List<SlabRoleCommission>(),
                            CommSettingType = op.CommSettingType,
                            ServiceID = op.ServiceID,
                            SCode = op.SCode,
                            AllowChannel = op.AllowChannel,
                            IsSpecialOp=op.IsSpecialOp
                        };

                        foreach (var roleItem in slabDetailModel.Roles)
                        {
                            var slabDetails = slabDetailModel.SlabDetails.Where(x => x.RoleID == roleItem.ID && x.OID == op.OID).ToList();
                            var slabDetail = new SlabCommission();
                            if (slabDetails.Count > 0)
                            {
                                slabDetail = slabDetails[0];
                            }
                            slabDetailDisplayLvl.RoleCommission.Add(new SlabRoleCommission
                            {
                                RoleID = roleItem.ID,
                                Role = roleItem.Role,
                                Prefix = roleItem.Prefix,
                                Comm = slabDetail.Comm,
                                CommType = slabDetail.CommType,
                                AmtType = slabDetail.AmtType,
                                RComm = slabDetail.RComm,
                                RCommType = slabDetail.RCommType,
                                RAmtType = slabDetail.RAmtType,
                                ModifyDate = slabDetail.ModifyDate
                            });
                        }
                        res.Add(slabDetailDisplayLvl);
                    }
                }
            }
            return res;
        }
        public IEnumerable<DTHSlabDetailDisplay> GetDTHSlabDetailForDisplay(int OID)
        {
            var res = new List<DTHSlabDetailDisplay> { };
            if (_lr.LoginTypeID == LoginType.ApplicationUser)
            {
                var req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonInt = OID
                };
                IProcedure proc = new ProcGetDTHSlabDetailDisplay(_dal);
                var dthCommissionModel = (DTHCommissionModel)proc.Call(req);
                if (dthCommissionModel.DTHCommissions.Any())
                {
                    var PackageDetail = dthCommissionModel.DTHCommissions
                        .GroupBy(x => new { x.PackageID, x.PackageName, x.Operator, x.OpType, x.SPKey, x.PackageMRP, x.BookingAmount, x.BusinessModel })
                        .Select(g => new DTHPackage { OID = g.Key.PackageID, PackageName = g.Key.PackageName, Operator = g.Key.Operator, OpType = g.Key.OpType, SPKey = g.Key.SPKey, PackageMRP = g.Key.PackageMRP, BookingAmount = g.Key.BookingAmount, BusinessModel = g.Key.BusinessModel })
                        .ToList();

                    foreach (var pc in PackageDetail)
                    {
                        var slabDetailDisplay = new DTHSlabDetailDisplay
                        {
                            PackageID = pc.ID,
                            PackageName = pc.PackageName,
                            Operator = pc.Operator,
                            OpType = pc.OpType,
                            SPKey = pc.SPKey,
                            PackageMRP = pc.PackageMRP,
                            BookingAmount = pc.BookingAmount,
                            BusinessModel = pc.BusinessModel,
                            RoleCommission = new List<SlabRoleCommission>()
                        };

                        foreach (var roleItem in dthCommissionModel.Roles)
                        {
                            var slabDetails = dthCommissionModel.DTHCommissions.Where(x => x.RoleID == roleItem.ID && x.PackageID == pc.ID).ToList();
                            var slabDetail = new DTHCommission();
                            if (slabDetails.Count > 0)
                            {
                                slabDetail = slabDetails[0];
                            }
                            slabDetailDisplay.RoleCommission.Add(new SlabRoleCommission
                            {
                                RoleID = roleItem.ID,
                                Role = roleItem.Role,
                                Prefix = roleItem.Prefix,
                                Comm = slabDetail.Comm,
                                CommType = slabDetail.CommType,
                                AmtType = slabDetail.AmtType,
                                ModifyDate = slabDetail.ModifyDate
                            });
                        }
                        res.Add(slabDetailDisplay);
                    }
                }
            }
            return res;
        }

        public IEnumerable<RangeSlabDetailDisplayLvl> GetSlabDetailForDisplayRange()
        {
            var res = new List<RangeSlabDetailDisplayLvl> { };
            if (_lr.LoginTypeID == LoginType.ApplicationUser)
            {
                var req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID
                };
                IProcedure proc = new ProcGetSlabDetailDisplayLvLRange(_dal);
                var slabDetailModel = (RangeDetailModel)proc.Call(req);
                if (slabDetailModel.SlabDetails.Any())
                {
                    var operatorDetails = slabDetailModel.SlabDetails
                        .GroupBy(x => new { x.OID, x.Operator, x.OperatorType, x.IsBBPS, x.SPKey, x.Min, x.Max, x.BusinessModel, x.MaxRange, x.MinRange, x.RangeId })
                        .Select(g => new OperatorDetail { OID = g.Key.OID, Operator = g.Key.Operator, OperatorType = g.Key.OperatorType, IsBBPS = g.Key.IsBBPS, SPKey = g.Key.SPKey, Min = g.Key.Min, Max = g.Key.Max, BusinessModel = g.Key.BusinessModel, MaxRange = g.Key.MaxRange, MinRange = g.Key.MinRange, RangeId = g.Key.RangeId })
                        .ToList();

                    foreach (var op in operatorDetails)
                    {
                        var slabDetailDisplayLvl = new RangeSlabDetailDisplayLvl
                        {
                            OID = op.OID,
                            Operator = op.Operator,
                            OpType = op.IsBBPS ? op.OperatorType + ",BBPS" : op.OperatorType,
                            SPKey = op.SPKey,
                            IsBBPS = op.IsBBPS,
                            Min = op.Min,
                            Max = op.Max,
                            BusinessModel = op.BusinessModel,
                            RoleCommission = new List<SlabRoleCommission>(),
                            MaxRange = op.MaxRange,
                            MinRange = op.MinRange,
                            RangeId = op.RangeId
                        };
                        foreach (var roleItem in slabDetailModel.Roles)
                        {
                            var slabDetails = slabDetailModel.SlabDetails.Where(x => x.RoleID == roleItem.ID && x.OID == op.OID && x.RangeId == op.RangeId).ToList();
                            var slabDetail = new RangeCommission();
                            if (slabDetails.Count > 0)
                            {
                                slabDetail = slabDetails[0];
                            }
                            slabDetailDisplayLvl.RoleCommission.Add(new SlabRoleCommission
                            {
                                RoleID = roleItem.ID,
                                Role = roleItem.Role,
                                Prefix = roleItem.Prefix,
                                Comm = slabDetail.Comm,
                                CommType = slabDetail.CommType,
                                AmtType = slabDetail.AmtType,
                                ModifyDate = slabDetail.ModifyDate,
                                MaxComm = slabDetail.MaxComm
                            });
                        }
                        //slabDetailDisplayLvl.DMRModelID = DMRModelID;
                        res.Add(slabDetailDisplayLvl);
                    }
                }
            }
            return res;
        }


        public IEnumerable<SlabDetailDisplayLvl> GetSlabDetailForDisplayForApp(CommonReq commonReq)
        {
            var res = new List<SlabDetailDisplayLvl> { };
            if (commonReq.LoginTypeID == LoginType.ApplicationUser)
            {
                IProcedure proc = new ProcGetSlabDetailDisplayLvL(_dal);
                var slabDetailModel = (SlabDetailModel)proc.Call(commonReq);
                if (slabDetailModel.SlabDetails.Any())
                {
                    var operatorDetails = slabDetailModel.SlabDetails
                        .GroupBy(x => new { x.OID, x.Operator, x.OperatorType, x.IsBBPS, x.SPKey, x.Min, x.Max, x.BusinessModel, x.CommSettingType, x.OpType })
                        .Select(g => new OperatorDetail { OID = g.Key.OID, Operator = g.Key.Operator, OperatorType = g.Key.OperatorType, IsBBPS = g.Key.IsBBPS, SPKey = g.Key.SPKey, Min = g.Key.Min, Max = g.Key.Max, BusinessModel = g.Key.BusinessModel, CommSettingType = g.Key.CommSettingType, OpType = g.Key.OpType })
                        .ToList();

                    foreach (var op in operatorDetails)
                    {
                        var slabDetailDisplayLvl = new SlabDetailDisplayLvl
                        {
                            OID = op.OID,
                            Operator = op.Operator,
                            OpType = op.IsBBPS ? op.OperatorType + ",BBPS" : op.OperatorType,
                            OpTypeID = op.OpType,
                            SPKey = op.SPKey,
                            IsBBPS = op.IsBBPS,
                            Min = op.Min,
                            Max = op.Max,
                            BusinessModel = op.BusinessModel,
                            RoleCommission = new List<SlabRoleCommission>(),
                            CommSettingType = op.CommSettingType
                        };

                        foreach (var roleItem in slabDetailModel.Roles)
                        {
                            var slabDetails = slabDetailModel.SlabDetails.Where(x => x.RoleID == roleItem.ID && x.OID == op.OID).ToList();
                            var slabDetail = new SlabCommission();
                            if (slabDetails.Count > 0)
                            {
                                slabDetail = slabDetails[0];
                            }
                            slabDetailDisplayLvl.RoleCommission.Add(new SlabRoleCommission
                            {
                                RoleID = roleItem.ID,
                                Role = roleItem.Role,
                                Prefix = roleItem.Prefix,
                                Comm = slabDetail.Comm,
                                RComm = slabDetail.RComm,
                                CommType = slabDetail.CommType,
                                AmtType = slabDetail.AmtType,
                                ModifyDate = slabDetail.ModifyDate
                            });
                        }
                        res.Add(slabDetailDisplayLvl);
                    }
                }
            }
            return res;
        }

        public IResponseStatus UpdateSlabDetail(SlabCommission slabCommission)
        {
            IResponseStatus _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if ((_lr.LoginTypeID == LoginType.ApplicationUser && !userML.IsEndUser()) || userML.IsCustomerCareAuthorised(ActionCodes.AddEditSLAB))
            {
                var req = new SlabRequest
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonStr = _rinfo.GetRemoteIP(),
                    CommonStr2 = _rinfo.GetBrowser(),
                    Commission = slabCommission
                };
                IProcedure proc = new ProcUpdateSlabDetail(_dal);
                var res = (AlertReplacementModel)proc.Call(req);
                _resp.Statuscode = res.Statuscode;
                _resp.Msg = res.Msg;
                if (_lr.RoleID == Role.Admin && LoginType.ApplicationUser == _lr.LoginTypeID)
                {
                    IAlertML ml = new AlertML(_accessor, _env);
                    Parallel.Invoke(() => ml.MarginRevisedSMS(res),
                        () => ml.MarginRevisedEmail(res),
                        () => ml.WebNotification(res),
                        () => ml.MarginRevisedNotification(res, true));
                }
            }
            return _resp;
        }
        public IResponseStatus UpdateSlabDetailGI(SlabCommission slabCommission)
        {
            IResponseStatus _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if ((_lr.LoginTypeID == LoginType.ApplicationUser && !userML.IsEndUser()) || userML.IsCustomerCareAuthorised(ActionCodes.AddEditSLAB))
            {
                var req = new SlabRequest
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonStr = _rinfo.GetRemoteIP(),
                    CommonStr2 = _rinfo.GetBrowser(),
                    Commission = slabCommission
                };
                IProcedure proc = new Proc_UpdateSlabDetialGI(_dal);
                var res = (AlertReplacementModel)proc.Call(req);
                _resp.Statuscode = res.Statuscode;
                _resp.Msg = res.Msg;
                if (_lr.RoleID == Role.Admin && LoginType.ApplicationUser == _lr.LoginTypeID)
                {
                    IAlertML ml = new AlertML(_accessor, _env);
                    Parallel.Invoke(() => ml.MarginRevisedSMS(res),
                        () => ml.MarginRevisedEmail(res),
                        () => ml.WebNotification(res),
                        () => ml.MarginRevisedNotification(res, true));
                }
            }
            return _resp;
        }

        public IResponseStatus UpdateBulkSlabDetail(SlabCommissionReq req)
        {
            IResponseStatus _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if ((_lr.LoginTypeID == LoginType.ApplicationUser && !userML.IsEndUser()) || userML.IsCustomerCareAuthorised(ActionCodes.AddEditSLAB))
            {
                req.LoginTypeID = _lr.LoginTypeID;
                req.LoginID = _lr.UserID;
                req.CommonStr = _rinfo.GetRemoteIP();
                req.CommonStr2 = _rinfo.GetBrowser();

                IProcedure proc = new ProcUpdateBulkSlabDetail(_dal);
                var res = (AlertReplacementModel)proc.Call(req);
                _resp.Statuscode = res.Statuscode;
                _resp.Msg = res.Msg;
                if (_lr.RoleID == Role.Admin && LoginType.ApplicationUser == _lr.LoginTypeID)
                {
                    IAlertML ml = new AlertML(_accessor, _env);
                    Parallel.Invoke(() => ml.MarginRevisedSMS(res),
                        () => ml.MarginRevisedEmail(res),
                        () => ml.WebNotification(res),
                        () => ml.MarginRevisedNotification(res, true));
                }
            }
            return _resp;
        }
        public IResponseStatus CopySlabDetail(int SlabID, string SlabName)
        {
            IResponseStatus _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if ((_lr.LoginTypeID == LoginType.ApplicationUser && !userML.IsEndUser()) || userML.IsCustomerCareAuthorised(ActionCodes.AddEditSLAB))
            {
                var req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonStr = SlabName.Trim(),
                    CommonInt = SlabID
                };
                IProcedure proc = new ProcCopySlab(_dal);
                _resp = (IResponseStatus)proc.Call(req);
            }
            return _resp;
        }

        public IResponseStatus UpdateRangeSlabDetail(RangeCommission slabCommission)
        {
            IResponseStatus _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if ((_lr.LoginTypeID == LoginType.ApplicationUser && !userML.IsEndUser()) || userML.IsCustomerCareAuthorised(ActionCodes.AddEditSLAB))
            {
                var req = new RangeCommissionReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonStr = _rinfo.GetRemoteIP(),
                    CommonStr2 = _rinfo.GetBrowser(),
                    Commission = slabCommission
                };
                IProcedure proc = new procUpdateSlabDetailRange(_dal);
                _resp = (IResponseStatus)proc.Call(req);
            }
            return _resp;
        }
        public IResponseStatus UpdateRangeAPIComm(RangeCommission slabCommission)
        {
            IResponseStatus _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if ((_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin) || userML.IsCustomerCareAuthorised(ActionCodes.AddEditSLAB))
            {
                var req = new RangeCommissionReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonStr = _rinfo.GetRemoteIP(),
                    CommonStr2 = _rinfo.GetBrowser(),
                    Commission = slabCommission
                };
                IProcedure proc = new ProcUpdateAPICommRange(_dal);
                _resp = (IResponseStatus)proc.Call(req);
            }
            return _resp;
        }
        public SlabDetailModel GetSlabCommission(int SlabID)
        {
            var _resp = new SlabDetailModel();
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID != Role.Admin)
            {
                var req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonInt = SlabID
                };
                IProcedure _proc = new ProcGetSlabDetail(_dal);
                _resp = (SlabDetailModel)_proc.Call(req);
            }
            return _resp;
        }

        public IEnumerable<RangeCommission> GetRangeSlabDetail(int SlabID)
        {
            var res = new List<RangeCommission>();
            if (_lr.LoginTypeID == LoginType.ApplicationUser || (userML.IsCustomerCareAuthorised(ActionCodes.AddEditSLAB)))
            {
                var req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonInt = SlabID,
                };
                IProcedure proc = new ProcGetSlabDetailRange(_dal);
                res = (List<RangeCommission>)proc.Call(req);
            }
            return res;
        }
        public SlabDetailModel GetSlabCommissionApp(CommonReq commonReq)
        {
            var _resp = new SlabDetailModel();
            if (commonReq.LoginTypeID == LoginType.ApplicationUser)
            {
                IProcedure _proc = new ProcGetSlabDetail(_dal);
                _resp = (SlabDetailModel)_proc.Call(commonReq);
            }
            return _resp;
        }
        public async Task<SlabCommission> GetLapuRealComm(CommonReq commonReq)
        {
            var _resp = new SlabCommission();
            IProcedureAsync _proc = new ProcGetCommissionForOperator(_dal);
            _resp = (SlabCommission)await _proc.Call(commonReq);
            return _resp;
        }
        public async Task<CommissionDisplay> GetDisplayCommission(CommonReq commonReq)
        {
            var _resp = new CommissionDisplay();

            IProcedureAsync _proc = new ProcCalaculateCommission(_dal);
            _resp = (CommissionDisplay)await _proc.Call(commonReq);
            return _resp;
        }
        public RangeDetailModel GetSlabDetailRange(int SlabID, int OType = 0)
        {
            var res = new RangeDetailModel { };
            if (_lr.LoginTypeID == LoginType.ApplicationUser || (userML.IsCustomerCareAuthorised(ActionCodes.AddEditSLAB)))
            {
                var req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonInt = SlabID,
                    CommonInt2 = CommStttingType.Rangewise,
                    CommonInt3 = OType,
                    IsListType = _lr.IsAdminDefined || (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser)
                };
                IProcedure proc = new ProcGetSlabDetailRange(_dal);
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
        public IResponseStatus RealAPIStatusUpdate(bool Status)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var Req = new CommonReq
            {
                LoginID = _lr.UserID,
                CommonBool = Status,
            };
            IProcedure _proc = new ProcUpdateUserRealCommissionFlag(_dal);
            _resp = (ResponseStatus)_proc.Call(Req);
            return _resp;
        }
        public IResponseStatus RealAPIStatusUpdate(CommonReq Req)
        {
            IProcedure _proc = new ProcUpdateUserRealCommissionFlag(_dal);
            return (ResponseStatus)_proc.Call(Req);
        }
        public DTHCommissionModel GetDTHCommissionDetail(int SlabID, int OpTypeID)
        {
            var res = new DTHCommissionModel { };
            if (_lr.LoginTypeID == LoginType.ApplicationUser || (userML.IsCustomerCareAuthorised(ActionCodes.AddEditSLAB)))
            {
                var req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonInt = SlabID,
                    CommonInt2 = OpTypeID,
                    IsListType = _lr.IsAdminDefined || (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser)
                };
                IProcedure proc = new ProcGetDTHCommissionDetail(_dal);
                res = (DTHCommissionModel)proc.Call(req);
                if (res.IsAdminDefined)
                {
                    List<DTHPackage> DTHPackage = res.DTHCommissions.GroupBy(x => new { x.PackageID, x.PackageName })
                        .Select(g => new DTHPackage { ID = g.Key.PackageID, PackageName = g.Key.PackageName }).ToList();
                    res.DTHPackage = DTHPackage;
                }
            }
            return res;
        }
        public IResponseStatus UpdateDTHCommission(DTHCommission DTHCommission)
        {
            IResponseStatus _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if ((_lr.LoginTypeID == LoginType.ApplicationUser && !userML.IsEndUser()) || userML.IsCustomerCareAuthorised(ActionCodes.AddEditSLAB))
            {
                var req = new DTHCommissionRequest
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonStr = _rinfo.GetRemoteIP(),
                    CommonStr2 = _rinfo.GetBrowser(),
                    Commission = DTHCommission
                };
                IProcedure proc = new ProcUpdateDTHCConnectionDetail(_dal);
                _resp = (IResponseStatus)proc.Call(req);
            }
            return _resp;
        }
        public IEnumerable<IncentiveDetail> GetIncentive(int OID)
        {
            var res = new List<IncentiveDetail>();
            if (_lr.LoginTypeID == LoginType.ApplicationUser)
            {
                var req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonInt = OID
                };
                IProcedure proc = new ProcGetDenominationIncentive(_dal);
                res = (List<IncentiveDetail>)proc.Call(req);
            }
            return res;
        }
        public IEnumerable<IncentiveDetail> GetIncentive(CommonReq req)
        {
            var res = new List<IncentiveDetail>();
            if (req.LoginTypeID == LoginType.ApplicationUser)
            {
                IProcedure proc = new ProcGetDenominationIncentive(_dal);
                res = (List<IncentiveDetail>)proc.Call(req);
            }
            return res;
        }
        public IEnumerable<SlabRangeDetail> GetSlabRangeDetail(int OID)
        {
            var res = new List<SlabRangeDetail>();
            if (_lr.LoginTypeID == LoginType.ApplicationUser)
            {
                var req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonInt = OID
                };
                IProcedure proc = new ProcGetSlabRangeDetail(_dal);
                res = (List<SlabRangeDetail>)proc.Call(req);
            }
            return res;
        }
        public List<SlabSpecialCircleWise> GetSpecialSlabDetail(int OID)
        {
            var res = new List<SlabSpecialCircleWise>();
            if (_lr.LoginTypeID == LoginType.ApplicationUser)
            {
                var req = new CommonReq
                {
                    LoginID = _lr.UserID,
                    CommonInt = OID
                };
                IProcedure proc = new ProcGetSpecialCommissionByUser(_dal);
                res = (List<SlabSpecialCircleWise>)proc.Call(req);
            }
            return res;
        }
        public List<SlabSpecialCircleWise> GetSpecialSlabDetailApp(int OID,int UserID)
        {
            IProcedure proc = new ProcGetSpecialCommissionByUser(_dal);
            return (List<SlabSpecialCircleWise>)proc.Call(new CommonReq
            {
                LoginID = UserID,
                CommonInt = OID
            });
        }
        public SlabRangDetailRes GetSlabRangeDetailForApp(SlabRangDetailReq AppReq)
        {
            var resp = new SlabRangDetailRes
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            if (AppReq.LoginTypeID == LoginType.ApplicationUser)
            {
                var req = new CommonReq
                {
                    LoginTypeID = AppReq.LoginTypeID,
                    LoginID = AppReq.UserID,
                    CommonInt = AppReq.OID
                };
                IProcedure proc = new ProcGetSlabRangeDetail(_dal);
                var res = (List<SlabRangeDetail>)proc.Call(req);
                if (res != null)
                {
                    resp.Statuscode = ErrorCodes.One;
                    resp.Msg = "Succsses";
                    resp.SlabRangeDetail = res;
                }
            }
            return resp;
        }
        public IResponseStatus UpdateFlatCommission(int UserID, int RoleID, decimal CommRate)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin)
            {
                IProcedure proc = new ProcSaveFlatCommission(_dal);
                res = (ResponseStatus)proc.Call(new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonInt = UserID,
                    CommonInt2 = RoleID,
                    CommonDecimal = CommRate
                });
            }
            return res;
        }
        public IEnumerable<FlatCommissionDetail> FlatCommissionDetails(int UserID)
        {
            IEnumerable<FlatCommissionDetail> flatCommissionDetails = null;
            if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                IProcedure proc = new ProcGetFlatCommission(_dal);
                flatCommissionDetails = (List<FlatCommissionDetail>)proc.Call(new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonInt = UserID
                });
            }

            return flatCommissionDetails;
        }

        public IEnumerable<AFSlabDetailDisplayLvl> GetAFSlabDetailForDisplay(int OID)
        {
            var res = new List<AFSlabDetailDisplayLvl> { };
            if (_lr.LoginTypeID == LoginType.ApplicationUser)
            {
                var req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonInt = OID
                };
                IProcedure proc = new ProcGetAFSlabDetailDisplayLvL(_dal);
                var slabDetailModel = (SlabDetailModel)proc.Call(req);
                if (slabDetailModel.SlabDetails.Any())
                {
                    var operatorDetails = slabDetailModel.SlabDetails
                        .GroupBy(x => new { x.VID, x.VName })
                        .Select(g => new AffiliateVendors { Id = g.Key.VID, VendorName = g.Key.VName })
                        .ToList();

                    foreach (var op in operatorDetails)
                    {
                        var slabDetailDisplayLvl = new AFSlabDetailDisplayLvl
                        {
                            VID = op.Id,
                            VName = op.VendorName,
                            RoleCommission = new List<SlabRoleCommission>()
                        };

                        foreach (var roleItem in slabDetailModel.Roles)
                        {
                            var slabDetails = slabDetailModel.SlabDetails.Where(x => x.RoleID == roleItem.ID && x.VID == op.Id).ToList();
                            var slabDetail = new SlabCommission();
                            if (slabDetails.Count > 0)
                            {
                                slabDetail = slabDetails[0];
                            }
                            slabDetailDisplayLvl.RoleCommission.Add(new SlabRoleCommission
                            {
                                RoleID = roleItem.ID,
                                Role = roleItem.Role,
                                Prefix = roleItem.Prefix,
                                Comm = slabDetail.Comm,
                                CommType = slabDetail.CommType,
                                AmtType = slabDetail.AmtType,
                                ModifyDate = slabDetail.ModifyDate
                            });
                        }
                        res.Add(slabDetailDisplayLvl);
                    }
                }
            }
            return res;
        }

        public SlabDetailModel GetAFSlabCommission(int OID)
        {
            var _resp = new SlabDetailModel();
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID != Role.Admin)
            {
                var req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonInt = OID
                };
                IProcedure _proc = new ProcGetAFSlabDetailDisplay(_dal);
                _resp = (SlabDetailModel)_proc.Call(req);
            }
            return _resp;
        }
        public IEnumerable<DenomCommissionDetail> GetDenomCommissionDetail(int OID)
        {
            var res = new List<DenomCommissionDetail>();
            if (_lr.LoginTypeID == LoginType.ApplicationUser)
            {
                var req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonInt = OID
                };
                IProcedure proc = new ProcDenomCommissionDetail(_dal);
                res = (List<DenomCommissionDetail>)proc.Call(req);
            }
            return res;
        }
        public IEnumerable<SlabCommissionSettingRes> SlabCommissionSetting(int OID)
        {
            var req = new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID,
                CommonInt = OID
            };
            IProcedure proc = new ProcSlabCommissionSetting(_dal);
            var res = (List<SlabCommissionSettingRes>)proc.Call(req);
            return res;
        }

        public IEnumerable<SlabCommission> CircleSlabGet(int SlabID, int OID)
        {
            IProcedure _proc = new ProcGetSlabDetailForCircle(_dal);
            return (List<SlabCommission>)_proc.Call(new CommonReq
            {
                CommonInt = SlabID,
                CommonInt2 = OID
            });
        }
        public IResponseStatus UpdateCircleSlab(SlabCommission slabCommission)
        {
            IProcedure _proc = new ProcUpdateSlabDetailCircle(_dal);
            return (ResponseStatus)_proc.Call(new SlabRequest
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID,
                Commission = slabCommission
            });
        }

        public IEnumerable<SlabCommission> CircleSlabAPIGet(int APIID, int OID)
        {
            IProcedure _proc = new ProcGetSlabDetailForCircleAPI(_dal);
            return (List<SlabCommission>)_proc.Call(new CommonReq
            {
                CommonInt = APIID,
                CommonInt2 = OID
            });
        }
        public IResponseStatus UpdateCircleSlabAPI(SlabCommission slabCommission)
        {
            IProcedure _proc = new ProcUpdateSlabDetailCircleAPI(_dal);
            return (ResponseStatus)_proc.Call(new SlabRequest
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID,
                Commission = slabCommission
            });
        }
        public IResponseStatus UpdateDMRModelForSlabDetail(int OID, int SlabID, int DMRModelID)
        {
            IProcedure proc = new ProcUpdateDMRModelForSlabDetail(_dal);
            return (ResponseStatus)proc.Call(new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID,
                CommonInt = SlabID,
                CommonInt2 = OID,
                CommonInt3 = DMRModelID
            });
        }
        public IEnumerable<SlabRangeDetail> GetRealtimeComm()
        {
            var res = new List<SlabRangeDetail>();
            if (_lr.LoginTypeID == LoginType.ApplicationUser)
            {
                var req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID
                };
                IProcedure proc = new ProcGetRealTimeCommision(_dal);
                res = (List<SlabRangeDetail>)proc.Call(req);
            }
            return res;
        }
        public IEnumerable<SlabRangeDetail> GetRealtimeCommApp(int UserID)
        {
            IProcedure proc = new ProcGetRealTimeCommision(_dal);
            return (List<SlabRangeDetail>)proc.Call(new CommonReq
            {
                LoginTypeID = LoginType.ApplicationUser,
                LoginID = UserID
            });
        }

        public async Task<IResponseStatus> DeleteSlab(int slabID, int loginId)
        {
            IProcedureAsync proc = new ProcDeleteSlab(_dal);
            return (ResponseStatus)await proc.Call(new CommonReq { LoginID = loginId, CommonInt = slabID }).ConfigureAwait(true);
        }
    }
}
