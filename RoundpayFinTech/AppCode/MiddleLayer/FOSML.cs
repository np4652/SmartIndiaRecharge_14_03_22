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
using RoundpayFinTech.AppCode.Model.DepartmentModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Validators;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public class FOSML:IFOSML
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
        private readonly IResourceML _resourceML;

        public FOSML(IHttpContextAccessor accessor, IHostingEnvironment env, bool IsInSession = true)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            _rinfo = new RequestInfo(_accessor, _env);
            if (IsInSession)
            {
                _session = _accessor.HttpContext.Session;
                bool IsProd = _env.IsProduction();
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory());
                builder.AddJsonFile((IsProd ? "appsettings.json" : "appsettings.Development.json"));
                builder.AddEnvironmentVariables();
                Configuration = builder.Build();
                _WInfo = new LoginML(_accessor, _env).GetWebsiteInfo();
                _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
                _resourceML = new ResourceML(_accessor, _env);
            }

        }
        public FOSML(LoginResponse lr)
        {
            _lr = lr;
        }
        public IResponseStatus AssignRetailerToFOS(UserRequest req)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            req.LoginID = _lr.UserID;
            req.LTID = _lr.LoginTypeID;

            DataTable dt = new DataTable();            dt.Columns.Add("_UserId");            foreach (var item in req.UserIds)            {                DataRow dr = dt.NewRow();                dr["_UserId"] = item;                dt.Rows.Add(dr);            }            req.dt = dt;

            IProcedure proc = new ProcAssignRetailerToFOS(_dal);
            res = (ResponseStatus)proc.Call(req);
            return res;
        }

        public UserRoleSlab GetRole()
        {
            IProcedure proc = new ProcUserSelectRoleSlab(_dal);
            return (UserRoleSlab)proc.Call(1);
        }
        public bool IsCustomerCareAuthorised(string OperationCode)
        {
            if (LoginType.CustomerCare == _lr.LoginTypeID)
            {
                var OperationsAssigned = _lr.operationsAssigned ?? new List<OperationAssigned>();
                if (OperationsAssigned.Any())
                {
                    if (OperationsAssigned.Any(x => x.OperationCode == OperationCode && x.IsActive))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public bool IsEndUser()
        {
            return _lr.RoleID.In(Role.APIUser, Role.Retailor_Seller, Role.Customer) && _lr.LoginTypeID == LoginType.ApplicationUser;
        }
        public UserList GetList(CommonFilter _filter)
        {
            var _resp = new UserList();
            if ((_lr.LoginTypeID == LoginType.ApplicationUser && !IsEndUser()) || IsCustomerCareAuthorised(ActionCodes.ShowUser))
            {
                var validate = Validate.O;
                var _req = new UserRequest
                {
                    LoginID = 1,
                    SortByID = _filter.SortByID,
                    LTID = 1,
                    Browser = _rinfo.GetBrowserFullInfo(),
                    IP = _rinfo.GetRemoteIP(),
                    RoleID = _filter.RoleID,
                    IsDesc = _filter.IsDesc
                };
                if (_filter.Criteria > 0)
                {
                    if ((_filter.CriteriaText ?? "") == "")
                    {
                        return _resp;
                    }
                }
                if (_filter.Criteria == Criteria.OutletMobile)
                {
                    if (!validate.IsMobile(_filter.CriteriaText))
                    {
                        return _resp;
                    }
                    _req.MobileNo = _filter.CriteriaText;
                }
                if (_filter.Criteria == Criteria.EmailID)
                {
                    if (!validate.IsEmail(_filter.CriteriaText))
                    {
                        return _resp;
                    }
                    _req.EmailID = _filter.CriteriaText;
                }
                if (_filter.Criteria == Criteria.Name)
                {
                    if (validate.IsNumeric(_filter.CriteriaText) || _filter.CriteriaText.Length > 101)
                    {
                        return _resp;
                    }
                    _req.Name = _filter.CriteriaText;
                }
                if (_filter.Criteria == Criteria.UserID)
                {
                    var Prefix = Validate.O.Prefix(_filter.CriteriaText);
                    if (Validate.O.IsNumeric(Prefix))
                        _filter.UserID = Validate.O.IsNumeric(_filter.CriteriaText) ? Convert.ToInt32(_filter.CriteriaText) : _filter.UserID;
                    var uid = Validate.O.LoginID(_filter.CriteriaText);
                    _req.UserID = Validate.O.IsNumeric(uid) ? Convert.ToInt32(uid) : _filter.UserID;
                }
                IProcedure proc = new ProcUserList(_dal);
                _resp.userReports = (List<UserReport>)proc.Call(_req);
                if (_lr.LoginTypeID == LoginType.ApplicationUser && !IsEndUser() || IsCustomerCareAuthorised(ActionCodes.ShowUser))
                {
                    _resp.CanChangeOTPStatus = (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser) || IsCustomerCareAuthorised(ActionCodes.ChangeOTPStatus);
                    _resp.CanChangeUserStatus = _lr.LoginTypeID == LoginType.ApplicationUser || IsCustomerCareAuthorised(ActionCodes.ChangeUserStatus);
                    _resp.CanAssignPackage = _lr.LoginTypeID == LoginType.ApplicationUser || IsCustomerCareAuthorised(ActionCodes.PackageTransfer);
                    _resp.CanFundTransfer = _lr.LoginTypeID == LoginType.ApplicationUser || IsCustomerCareAuthorised(ActionCodes.FundTransfer);
                    _resp.CanEdit = _lr.LoginTypeID == LoginType.ApplicationUser || IsCustomerCareAuthorised(ActionCodes.EditUser);
                    _resp.CanChangeSlab = (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser) || IsCustomerCareAuthorised(ActionCodes.AddEditSLAB);
                    _resp.LoginID = (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || _lr.LoginTypeID == LoginType.CustomerCare) ? 1 : _lr.UserID;
                }
            }
            return _resp;
        }

        public List<FOSUserExcelModel> GetFOSUserExcel(CommonFilter f)
        {
            IUserML userML = new UserML(_accessor, _env);
            var res = userML.GetList(f);
            if (f.MapStatus == MapStatus.Unassigned)
                res.userReports = res.userReports.Where(w => w.RoleID == Role.Retailor_Seller & w.FOSId == MapStatus.Unassign).ToList();
            else if (f.MapStatus == MapStatus.Assigned)
            {
                if (f.FOSID > MapStatus.Unassign)
                    res.userReports = res.userReports.Where(w => w.RoleID == Role.Retailor_Seller & w.FOSId == f.FOSID).ToList();
                else
                    res.userReports = res.userReports.Where(w => w.RoleID == Role.Retailor_Seller & w.FOSId != MapStatus.Unassign).ToList();
            }
            else
            {
                res.userReports = res.userReports.Where(w => w.RoleID == Role.Retailor_Seller).ToList();
            }
            List<FOSUserExcelModel> _lst = new List<FOSUserExcelModel>();
            foreach (var i in res.userReports)
            {
                FOSUserExcelModel obj = new FOSUserExcelModel()
                {
                    OutletName = i.OutletName,
                    OutletMobile = i.MobileNo,
                    PrepaidBalance = i.Balance,
                    UtilityBalance = i.UBalance,
                    Slab = i.Slab,
                    JoinDate = i.JoinDate,
                    JoinBy = i.JoinBy,
                    KYCStatus = i.KYCStatus,
                    FOSName = i.FOSName==string.Empty?"Unassigned": i.FOSName,
                    FOSMobile = i.FOSMobile
                };
                _lst.Add(obj);
            }
            return _lst;
        }
        public UserList GetListFOS(CommonFilter _filter)
        {
            var _resp = new UserList();
            if ((_lr.LoginTypeID == LoginType.ApplicationUser && !IsEndUser()) || IsCustomerCareAuthorised(ActionCodes.ShowUser))
            {
                var validate = Validate.O;
                var _req = new UserRequest
                {
                    LoginID = _lr.UserID,
                    SortByID = _filter.SortByID,
                    LTID = 1,
                    Browser = _rinfo.GetBrowserFullInfo(),
                    IP = _rinfo.GetRemoteIP(),
                    RoleID = _filter.RoleID,
                    IsDesc = _filter.IsDesc
                };
                if (_filter.Criteria > 0)
                {
                    if ((_filter.CriteriaText ?? "") == "")
                    {
                        return _resp;
                    }
                }
                if (_filter.Criteria == Criteria.OutletMobile)
                {
                    if (!validate.IsMobile(_filter.CriteriaText))
                    {
                        return _resp;
                    }
                    _req.MobileNo = _filter.CriteriaText;
                }
                if (_filter.Criteria == Criteria.EmailID)
                {
                    if (!validate.IsEmail(_filter.CriteriaText))
                    {
                        return _resp;
                    }
                    _req.EmailID = _filter.CriteriaText;
                }
                if (_filter.Criteria == Criteria.Name)
                {
                    if (validate.IsNumeric(_filter.CriteriaText) || _filter.CriteriaText.Length > 101)
                    {
                        return _resp;
                    }
                    _req.Name = _filter.CriteriaText;
                }
                if (_filter.Criteria == Criteria.UserID)
                {
                    var Prefix = Validate.O.Prefix(_filter.CriteriaText);
                    if (Validate.O.IsNumeric(Prefix))
                        _filter.UserID = Validate.O.IsNumeric(_filter.CriteriaText) ? Convert.ToInt32(_filter.CriteriaText) : _filter.UserID;
                    var uid = Validate.O.LoginID(_filter.CriteriaText);
                    _req.UserID = Validate.O.IsNumeric(uid) ? Convert.ToInt32(uid) : _filter.UserID;
                }
                IProcedure proc = new ProcUserListFOS(_dal);
                _resp = (UserList)proc.Call(_req);
                if (_lr.LoginTypeID == LoginType.ApplicationUser && !IsEndUser() || IsCustomerCareAuthorised(ActionCodes.ShowUser))
                {
                    _resp.CanChangeOTPStatus = (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser) || IsCustomerCareAuthorised(ActionCodes.ChangeOTPStatus);
                    _resp.CanChangeUserStatus = _lr.LoginTypeID == LoginType.ApplicationUser || IsCustomerCareAuthorised(ActionCodes.ChangeUserStatus);
                    _resp.CanAssignPackage = _lr.LoginTypeID == LoginType.ApplicationUser || IsCustomerCareAuthorised(ActionCodes.PackageTransfer);
                    _resp.CanFundTransfer = _lr.LoginTypeID == LoginType.ApplicationUser || IsCustomerCareAuthorised(ActionCodes.FundTransfer);
                    _resp.CanEdit = _lr.LoginTypeID == LoginType.ApplicationUser || IsCustomerCareAuthorised(ActionCodes.EditUser);
                    _resp.CanChangeSlab = (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser) || IsCustomerCareAuthorised(ActionCodes.AddEditSLAB);
                    _resp.LoginID = (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || _lr.LoginTypeID == LoginType.CustomerCare) ? 1 : _lr.UserID;
                }
            }
            return _resp;
        }
    }
}
