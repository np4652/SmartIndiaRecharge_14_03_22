using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using RoundpayFinTech.AppCode.Configuration;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.App;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Validators;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public class PackageML : IPackageML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly ISession _session;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        private readonly LoginResponse _lr;
        private readonly IUserML userML;
        private readonly IRequestInfo _info;
        public PackageML(IHttpContextAccessor accessor, IHostingEnvironment env,bool InSession=true)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            _info = new RequestInfo(_accessor, _env);
            if (InSession)
            {
                _session = _accessor.HttpContext.Session;
                _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
                userML = new UserML(_lr);
            }
        }
        public PackageModel GetPackageMaster()
        {
            var resp = new PackageModel
            {
                Slabs = new List<SlabMaster>(),
                Packages = new List<PackageDetail>(),
                Services = new List<ServiceMaster>()
            };

            if (((_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin) || userML.IsCustomerCareAuthorised(ActionCodes.ShowPackage)) && ApplicationSetting.IsPackageAllowed)
            {
                var req = new CommonReq
                {
                    LoginID = _lr.UserID,
                    LoginTypeID = _lr.LoginTypeID,
                    CommonInt = -1
                };
                IProcedure proc = new ProcGetPackageMaster(_dal);
                resp.Slabs = (List<SlabMaster>)proc.Call(req);
                IProcedure procPDetail = new ProcGetPackageDetails(_dal);
                resp.Packages = (List<PackageDetail>)procPDetail.Call();
                if (resp.Packages.Count > 0)
                {
                    resp.Services = resp.Packages.GroupBy(g => new { g.ServiceID, g.ServiceName, g.IsServiceActive ,g.selfAssigned}).Select(g => new ServiceMaster { ServiceID = g.Key.ServiceID, ServiceName = g.Key.ServiceName, IsServiceActive = g.Key.IsServiceActive ,selfAssigned=g.Key.selfAssigned}).ToList();
                }
            }
            return resp;
        }
        public SlabMaster GetPackageMaster(int PackageID)
        {
            var resp = new SlabMaster();

            if ((_lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowPackage)) && ApplicationSetting.IsPackageAllowed)
            {
                var req = new CommonReq
                {
                    LoginID = _lr.UserID,
                    LoginTypeID = _lr.LoginTypeID,
                    CommonInt = PackageID
                };
                IProcedure proc = new ProcGetPackageMaster(_dal);
                resp = (SlabMaster)proc.Call(req);
            }
            return resp;
        }
        public PackageModel AvailablePackage(int UserId)
        {
            var resp = new PackageModel
            {
                Slabs = new List<SlabMaster>(),
                Packages = new List<PackageDetail>(),
                Services = new List<ServiceMaster>()
            };

            if ((_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin) || userML.IsCustomerCareAuthorised(ActionCodes.ShowPackage))
            {
                var req = new CommonReq
                {
                    LoginID = _lr.UserID,
                    LoginTypeID = _lr.LoginTypeID,
                    CommonInt = -1
                };
                IProcedure proc = new ProcGetPackageMaster(_dal);
                resp.Slabs = (List<SlabMaster>)proc.Call(req);
                IProcedure procPDetail = new ProcGetPackageDetails(_dal);
                resp.Packages = (List<PackageDetail>)procPDetail.Call();
                if (resp.Packages.Count > 0)
                {
                    resp.Services = resp.Packages.GroupBy(g => new { g.ServiceID, g.ServiceName, g.ID, g.IsActive }).Select(g => new ServiceMaster { ServiceID = g.Key.ServiceID, ServiceName = g.Key.ServiceName, PackageId = g.Key.ID, IsActive = g.Key.IsActive }).ToList();
                }
                var pkgAvail = new PackageAvailableModel
                {
                    LoginId = UserId,
                    LoginTypeId = _lr.LoginTypeID,
                };
                IProcedure procPkgAvail = new ProcGetAvailablePackage(_dal);
                resp.AvailablePackage = (List<PackageAvailableModel>)procPkgAvail.Call(pkgAvail);
                resp.AvailablePackage = resp.AvailablePackage.Where(w => w.UserId == UserId).ToList();
            }
            return resp;
        }

        public PackageModel GetUpgradePackage(int UserId)
        {
            var resp = new PackageModel
            {
                Slabs = new List<SlabMaster>(),
                Packages = new List<PackageDetail>(),
                Services = new List<ServiceMaster>()
            };

            if (_lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowPackage))
            {
                IProcedure proc = new ProcGetUpgradePackageMaster(_dal);
                resp.Slabs = (List<SlabMaster>)proc.Call(UserId);
                IProcedure procPDetail = new ProcGetPackageDetails(_dal);
                resp.Packages = (List<PackageDetail>)procPDetail.Call();
                if (resp.Packages.Count > 0)
                {
                    resp.Services = resp.Packages.GroupBy(g => new { g.ServiceID, g.ServiceName, g.ID, g.IsActive }).Select(g => new ServiceMaster { ServiceID = g.Key.ServiceID, ServiceName = g.Key.ServiceName, PackageId = g.Key.ID, IsActive = g.Key.IsActive }).ToList();
                }
            }
            return resp;
        }
        public IResponseStatus UpdatePackageMaster(SlabMaster packageMaster)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            if ((_lr.LoginTypeID == LoginType.ApplicationUser && !userML.IsEndUser()) || userML.IsCustomerCareAuthorised(ActionCodes.ServiceUpdatePackage))
            {
                Validate validate = Validate.O;
                if (validate.IsNumeric(packageMaster.Slab ?? "1") || validate.IsStartsWithNumber(packageMaster.Slab))
                {
                    _resp.Msg = "Package name is non numeric mandatory field and can not be start with number.";
                    return _resp;
                }
                var req = new SlabMasterReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    slabMaster = packageMaster,
                    CommonStr = _info.GetRemoteIP(),
                    CommonStr2 = _info.GetBrowser(),
                };
                IProcedure _proc = new ProcUpdatePackageMaster(_dal);
                _resp = (ResponseStatus)_proc.Call(req);
            }
            return _resp;
        }

        public IResponseStatus UpdatePackageDetail(int PackageID, int ServiceID, bool IsActive)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            if ((_lr.LoginTypeID == LoginType.ApplicationUser && !userML.IsEndUser()) || userML.IsCustomerCareAuthorised(ActionCodes.ServiceUpdatePackage))
            {
                var req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonInt = PackageID,
                    CommonInt2 = ServiceID,
                    IsListType = IsActive,
                    CommonStr = _info.GetRemoteIP(),
                    CommonStr2 = _info.GetBrowser()
                };
                IProcedure _proc = new ProcUpdateServiceForPackage(_dal);
                _resp = (ResponseStatus)_proc.Call(req);
            }
            return _resp;
        }

        public PackageMaster GetPackageCommission(CommonReq req)
        {
            var packageMaster = new PackageMaster();
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin)
            {
                req.LoginID = _lr.UserID;
                req.LoginTypeID = _lr.LoginTypeID;
                IProcedure _proc = new ProcGetPackageCommDetail(_dal);
                packageMaster = (PackageMaster)_proc.Call(req);
            }
            return packageMaster;
        }
        public PackageCommission GetPackageCommissionChannel()
        {
            var packageComm = new PackageCommission();
            var req = new CommonReq
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID
            };
            IProcedure _proc = new ProcGetPackageComm(_dal);
            packageComm = (PackageCommission)_proc.Call(req);
            return packageComm;
        }
        public PackageCommission GetPackageCommissionChannel(CommonReq req)
        {
            IProcedure _proc = new ProcGetPackageComm(_dal);
            return (PackageCommission)_proc.Call(req);
        }

        public List<_AvailablePackage> GetAvailablePackages(int UserID)
        {
            var req = new CommonReq
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID,
                CommonInt = UserID,
            };
            IProcedure _proc = new ProcAvailablePackages(_dal);
            var result= (AvailablePackage)_proc.Call(req);
            var final = new List<_AvailablePackage>();
            foreach(var p in result.Packages)
            {
                var package = new _AvailablePackage
                {
                    PackageId = p.PackageId,
                    PackageName = p.PackageName,
                    PackageCost = p.PackageCost,
                    IsDefault = p.IsDefault,
                    Commission = p.Commission,
                    IsEndUser = _lr.RoleID.In(Role.Retailor_Seller, Role.APIUser, Role.Customer) ? true : false,
                    IsAdminDefined = _lr.IsAdminDefined,
                    Services = result.Services.Where(x => x.PackageId == p.PackageId).ToList()
                };
                final.Add(package);
            }
            return final;
        }

        public AvailablePackageResponse GetAvailablePackagesForApp(int LoginID,int LoginTypeID)
        {
            var req = new CommonReq
            {
                LoginID = LoginID,
                LoginTypeID = LoginTypeID,
                CommonInt = -1,
            };
            IProcedure _proc = new ProcAvailablePackages(_dal);
            var result = (AvailablePackage)_proc.Call(req);
            var final = new List<_AvailablePackageForApp>();
            foreach (var p in result.Packages)
            {
                var package = new _AvailablePackageForApp
                {
                    PackageId = p.PackageId,
                    PackageName = p.PackageName,
                    PackageCost = p.PackageCost,
                    IsDefault = p.IsDefault,
                    Commission = p.Commission,
                    Services = result.Services.Where(x => x.PackageId == p.PackageId).ToList()
                };
                final.Add(package);
            }
            var AvailablePackageRes = new AvailablePackageResponse
            {
                PDetail = final,
                Statuscode = result.Statuscode,
                Msg = result.Msg
            };
            return AvailablePackageRes;
        }


        public IResponseStatus UpdateLevelPackageCommission(PkgLevelCommissionReq req)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser)
            {
                req.LoginId = _lr.UserID;
                req.LoginTypeId = _lr.LoginTypeID;
                //req.RoleId = _lr.RoleID;
                IProcedure _proc = new ProcUpdateLevelPackageCommission(_dal);
                _resp = (ResponseStatus)_proc.Call(req);
            }
            return _resp;
        }

        public IResponseStatus UpdateAvailablePackage(PackageAvailableModel req)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin)
            {
                req.LoginId = _lr.UserID;
                req.LoginTypeId = _lr.LoginTypeID;
                IProcedure _proc = new ProcUpdateAvailablePackage(_dal);
                _resp = (ResponseStatus)_proc.Call(req);
            }
            return _resp;
        }

        
        public IResponseStatus UpgradePackage(PackageAvailableModel req)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser)
            {
                req.LoginId = _lr.UserID;
                req.LoginTypeId = _lr.LoginTypeID;
                req.Browser = _info.GetRemoteIP();
                req.IP = _info.GetBrowser();
                IProcedure _proc = new ProcUpgradePackage(_dal);
                _resp = (ResponseStatus)_proc.Call(req);
            }
            return _resp;
        }

        public IResponseStatus UpgradePackageForApp(UpgradePackageReq  AppReq)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (AppReq.LoginTypeID == LoginType.ApplicationUser)
            {
                var req = new PackageAvailableModel
                {
                    UserId = AppReq.UID,
                    LoginId = AppReq.UserID,
                    LoginTypeId = AppReq.LoginTypeID,
                    AvailablePackageId = AppReq.AvailablePackageId,
                    IsAvailable = true,
                    Browser = _info.GetBrowser(),
                    IP = _info.GetRemoteIP(),
                };
                IProcedure _proc = new ProcUpgradePackage(_dal);
                _resp = (ResponseStatus)_proc.Call(req);
            }
            return _resp;
        }

        public List<PackageAvailableModel> GetAvailablePackage(PackageAvailableModel req)
        {
            var _resp = new List<PackageAvailableModel>();
            req.LoginId = _lr.UserID;
            req.LoginTypeId = _lr.LoginTypeID;
            IProcedure _proc = new ProcGetAvailablePackage(_dal);
            _resp = (List<PackageAvailableModel>)_proc.Call(req);
            return _resp;
        }

        public async Task<IResponseStatus> UpgradePackage(int PackageID) {
            var req = new PackageUpgradeRequest {
                LT=_lr.LoginTypeID,
                LoginID=_lr.UserID,
                PackageID=PackageID,
                RequestMode=RequestMode.PANEL,
                IP= _info.GetRemoteIP(),
                Browser=_info.GetBrowser()
            };
            IProcedureAsync _proc = new ProcUpgradePackageService(_dal);
            return (ResponseStatus) await _proc.Call(req);
        }
        public async Task<IResponseStatus> UpgradePackage(PackageUpgradeRequest req)
        {
            req.IP = _info.GetRemoteIP();
            req.Browser = _info.GetBrowser();
            IProcedureAsync _proc = new ProcUpgradePackageService(_dal);
            return (ResponseStatus)await _proc.Call(req);
        }



        #region VASPackage
        public PackageModel VASGetPackageMaster()
        {
            var resp = new PackageModel
            {
                Slabs = new List<SlabMaster>(),
                Packages = new List<PackageDetail>(),
                Services = new List<ServiceMaster>()
            };
            if (((_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin) || userML.IsCustomerCareAuthorised(ActionCodes.ShowPackage)) && ApplicationSetting.IsPackageAllowed)
            {
                var req = new CommonReq
                {
                    LoginID = _lr.UserID,
                    LoginTypeID = _lr.LoginTypeID,
                    CommonInt = -1
                };
                IProcedure proc = new ProcGetVASPackageMaster(_dal);
                resp.Slabs = (List<SlabMaster>)proc.Call(req);
                IProcedure procPDetail = new ProcGetVASPackageDetails(_dal);
                resp.Packages = (List<PackageDetail>)procPDetail.Call(new CommonReq { 
                    CommonInt = ServiceType.ValueAddedService
                });
                if (resp.Packages.Count > 0)
                {
                    resp.Services = resp.Packages.GroupBy(g => new {g.ServiceID, g.ServiceName, g.IsServiceActive, g.selfAssigned,g.IsActive,g.Charge }).Select(g => new ServiceMaster { ServiceID = g.Key.ServiceID, ServiceName = g.Key.ServiceName, IsServiceActive = g.Key.IsServiceActive, selfAssigned = g.Key.selfAssigned, IsActive = g.Key.IsActive, Charge = g.Key.Charge }).ToList();
                }
            }
            return resp;
        }
        public SlabMaster VASGetPackageMaster(int ID)
        {
            var resp = new SlabMaster();
            if ((_lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.ShowPackage)) && ApplicationSetting.IsPackageAllowed)
            {
                var req = new CommonReq
                {
                    LoginID = _lr.UserID,
                    LoginTypeID = _lr.LoginTypeID,
                    CommonInt = ID
                };
                IProcedure proc = new ProcGetVASPackageMaster(_dal);
                resp = (SlabMaster)proc.Call(req);
            }
            return resp;
        }
        public IResponseStatus UpdateVASPackageMaster(SlabMaster packageMaster)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            if ((_lr.LoginTypeID == LoginType.ApplicationUser && !userML.IsEndUser()) || userML.IsCustomerCareAuthorised(ActionCodes.ServiceUpdatePackage))
            {
                Validate validate = Validate.O;
                if (validate.IsNumeric(packageMaster.Slab ?? "1") || validate.IsStartsWithNumber(packageMaster.Slab))
                {
                    _resp.Msg = "Package name is non numeric mandatory field and can not be start with number.";
                    return _resp;
                }
                if (!validate.IsNumeric(packageMaster.DailyHitCount.ToString()))
                {
                    _resp.Msg = "Daily Hit Count must be Numbers.";
                    return _resp;
                }
                if (!validate.IsNumeric(packageMaster.ValidityInDays.ToString()))
                {
                    _resp.Msg = "Validity In Days must be Numbers.";
                    return _resp;
                }

                var req = new SlabMasterReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    slabMaster = packageMaster,
                    CommonStr = _info.GetRemoteIP(),
                    CommonStr2 = _info.GetBrowser(),
                    
                };
                IProcedure _proc = new ProcUpdateVASPackageMaster(_dal);
                _resp = (ResponseStatus)_proc.Call(req);
            }
            return _resp;
        }
        public IResponseStatus UpdateVASPackageDetail(int PackageID, int ServiceID, bool IsActive,bool IsChargeable,int Charge)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            if ((_lr.LoginTypeID == LoginType.ApplicationUser && !userML.IsEndUser()) || userML.IsCustomerCareAuthorised(ActionCodes.ServiceUpdatePackage))
            {
                var req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonInt = PackageID,
                    CommonInt2 = ServiceID,
                    CommonBool = IsActive,
                    CommonStr = _info.GetRemoteIP(),
                    CommonStr2 = _info.GetBrowser(),
                    CommonBool1 = IsChargeable,
                    CommonInt3 = Charge
                };
                IProcedure _proc = new ProcUpdateServiceForVASPackage(_dal);
                _resp = (ResponseStatus)_proc.Call(req);
            }
            return _resp;
        }
        public IResponseStatus VASPackageDetailForView(int UID)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            if ((_lr.LoginTypeID == LoginType.ApplicationUser && !userML.IsEndUser()) || userML.IsCustomerCareAuthorised(ActionCodes.ServiceUpdatePackage))
            {
                var req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID
                };
                IProcedure _proc = new ProcUpdateServiceForVASPackage(_dal);
                _resp = (ResponseStatus)_proc.Call(req);
            }
            return _resp;
        }

        public PackageModel VASPackageMasterForPlans()
        {
            var resp = new PackageModel
            {
                Slabs = new List<SlabMaster>(),
                Packages = new List<PackageDetail>(),
                Services = new List<ServiceMaster>()
            };
            if (((_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID.In(Role.APIUser)) || userML.IsCustomerCareAuthorised(ActionCodes.ShowPackage)) && ApplicationSetting.IsPackageAllowed)
            {
                var req = new CommonReq
                {
                    LoginID = _lr.UserID,
                    LoginTypeID = _lr.LoginTypeID,
                    CommonInt = -1
                };
                IProcedure proc = new ProcGetVASPackageMaster(_dal);
                resp.Slabs = (List<SlabMaster>)proc.Call(req);
                IProcedure _proc = new ProcGetVASPackagePlanForUser(_dal);
                resp.vasUserPackage = (List<VasUserPackage>)_proc.Call(req);
                IProcedure procPDetail = new ProcGetVASPackageDetails(_dal);
                resp.Packages = (List<PackageDetail>)procPDetail.Call(new CommonReq
                {
                    CommonInt = ServiceType.ValueAddedService
                });
                if (resp.Packages.Count > 0)
                {
                    resp.Services = resp.Packages.GroupBy(g => new { g.ServiceID, g.ServiceName, g.IsServiceActive, g.selfAssigned, g.IsActive, g.Charge }).Select(g => new ServiceMaster { ServiceID = g.Key.ServiceID, ServiceName = g.Key.ServiceName, IsServiceActive = g.Key.IsServiceActive, selfAssigned = g.Key.selfAssigned, IsActive = g.Key.IsActive, Charge = g.Key.Charge }).ToList();
                }
            }
            return resp;
        }

        public ResponseStatus VASPlanBuyOrRenew(int p,bool a,int u,int lt)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            if (_lr.LoginTypeID == LoginType.ApplicationUser)
            {
                var req = new CommonReq
                {
                    LoginTypeID = lt,
                    LoginID = u,
                    CommonInt = p,
                    CommonBool = a,
                    CommonStr = _info.GetRemoteIP(),
                    CommonStr1 = _info.GetBrowserFullInfo()
                };
                IProcedure _proc = new ProcVASPlanBuyOrRenew(_dal);
                _resp = (ResponseStatus)_proc.Call(req);
            }
            return _resp;
        }

        #endregion

    }
}
