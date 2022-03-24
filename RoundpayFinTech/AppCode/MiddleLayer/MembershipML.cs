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
using System;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public class MembershipML: IMembershipML
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
        public MembershipML(IHttpContextAccessor accessor, IHostingEnvironment env, bool IsSession = true)
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

        public IResponseStatus ChangeMemberShipStatus(int ID)
        {
           
                var _res = new ResponseStatus
                {
                    Statuscode = ErrorCodes.Minus1,
                    Msg = ErrorCodes.AnError
                };
                if ((_lr.LoginTypeID == LoginType.ApplicationUser))
                {
                    CommonReq commonReq = new CommonReq
                    {
                        LoginID = _lr.UserID,
                        LoginTypeID = _lr.LoginTypeID,
                        CommonInt = ID
                    };
                    IProcedure _proc = new ProcUpdateMemberShipStatus(_dal);
                    _res = (ResponseStatus)_proc.Call(commonReq);
                }
                return _res;
            
        }
        public IResponseStatus ChangeCouponAllowedStatus(int ID)
        {

            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            if ((_lr.LoginTypeID == LoginType.ApplicationUser))
            {
                CommonReq commonReq = new CommonReq
                {
                    LoginID = _lr.UserID,
                    LoginTypeID = _lr.LoginTypeID,
                    CommonInt = ID
                };
                IProcedure _proc = new ProcUpdateMemberShipCouponAllowed(_dal);
                _res = (ResponseStatus)_proc.Call(commonReq);
            }
            return _res;

        }

        public IEnumerable<MembershipMaster> GetMembershipMaster()
        {
            var resp = new List<MembershipMaster>();

            if (_lr.LoginTypeID == LoginType.ApplicationUser && !userML.IsEndUser() || userML.IsCustomerCareAuthorised(ActionCodes.AddEditSLAB))
            {
                var req = new CommonReq
                {
                    LoginID = _lr.UserID,
                    LoginTypeID = _lr.LoginTypeID,
                    CommonInt = -1
                };
                IProcedure proc = new ProcGetMembershipMaster(_dal);
                resp = (List<MembershipMaster>)proc.Call(req);
            }
            return resp;
        }
        public MembershipMaster GetMembershipMaster(int MembershipID)
        {
            var resp = new MembershipMaster();

            if (_lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.AddEditAPI))
            {
                var req = new CommonReq
                {
                    LoginID = _lr.UserID,
                    LoginTypeID = _lr.LoginTypeID,
                    CommonInt = MembershipID
                };
                IProcedure proc = new ProcGetMembershipMaster(_dal);
                resp = (MembershipMaster)proc.Call(req);
            }
            return resp;
        }
        public IEnumerable<MembershipmasterB2C> GetB2CMemberShipType(int LoginID) {
            IProcedure proc = new ProcGetB2CMemberTypeByMember(_dal);
              return (List<MembershipmasterB2C>)proc.Call(LoginID);
        }

        public IEnumerable<B2CMemberCouponDetail> GetB2CCoupon(int LoginID) {
            IProcedure proc = new ProcGetB2CCoupons(_dal);
            return (List<B2CMemberCouponDetail>)proc.Call(LoginID);
        }
        public ResponseStatus RedeemCoupon(int UserID,string CouponCode) {
            IProcedure proc = new ProcRedeemCoupon(_dal);
            return (ResponseStatus)proc.Call(new CommonReq
            {
                LoginID=UserID,
                CommonStr=CouponCode,
                CommonStr2=_rinfo.GetRemoteIP(),
                CommonStr3=_rinfo.GetBrowser()
            });
        }
        public ResponseStatus GetMemberShip(CommonReq  req) {
            req.CommonStr = _rinfo.GetRemoteIP();
            req.CommonStr2 = _rinfo.GetBrowser();
            IProcedure proc = new ProcGetB2CMemberShip(_dal);
            return (ResponseStatus)proc.Call(req);
        }
        public IResponseStatus UpdateMembershipMaster(MembershipMaster memMaster)
        {
            try
            {
                var _resp = new ResponseStatus
                {
                    Statuscode = ErrorCodes.Minus1,
                    Msg = ErrorCodes.AuthError
                };

                if ((_lr.LoginTypeID == LoginType.ApplicationUser && !_lr.RoleID.In(Role.APIUser, Role.Customer, Role.Retailor_Seller)) || userML.IsCustomerCareAuthorised(ActionCodes.AddEditSLAB))
                {
                    Validate validate = Validate.O;
                    if (validate.IsNumeric(memMaster.MemberShipType ?? "1") || validate.IsStartsWithNumber(memMaster.MemberShipType))
                    {
                        _resp.Msg = "MemberShip name is non numeric mandatory field and can not be start with number.";
                        return _resp;
                    }
                    
                    var req = new MembershipMasteReq
                    {
                        LoginTypeID = _lr.LoginTypeID,
                        LoginID = _lr.UserID,
                        memMaster = memMaster,
                        CommonStr = _rinfo.GetRemoteIP(),
                        CommonStr2 = _rinfo.GetBrowser()
                    };
                    IProcedure _proc = new ProcUpdateMemberMaster(_dal);
                    _resp = (ResponseStatus)_proc.Call(req);
                }
                return _resp;
            }
            
                catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "UpdateMembershipMaster",
                    Error =  ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = 1
                });
            
            throw;
            }


        }
    }
}
