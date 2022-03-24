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
using RoundpayFinTech.AppCode.Model.DepartmentModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public class CustomercareML: ICustomercareML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly ISession _session;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        private readonly IRequestInfo _rinfo;
        private readonly WebsiteInfo _WInfo;
        private readonly LoginResponse _lr;
        private readonly IResourceML _resourceML;
        public CustomercareML(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _session = _accessor.HttpContext.Session;
            _dal = new DAL(_c.GetConnectionString());
            _rinfo = new RequestInfo(_accessor, _env);           
            _WInfo = new LoginML(_accessor, _env).GetWebsiteInfo();
            _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
            _resourceML = new ResourceML(_accessor,_env);
        }
        public async Task<IEnumerable<Customercare>> GetCustomerCares(CommonReq req)
        {
            if (!(_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser))
                return null;
            IProcedureAsync proc = new ProcCustomerCares(_dal);
            return (List<Customercare>)await proc.Call(req);
        }
        public async Task<Customercare> GetCustomerCare(int ID)
        {
            if (!(_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser))
                return null;
            IProcedureAsync proc = new ProcCustomerCareByID(_dal);
            return (Customercare)await proc.Call(ID);
        }

        public IResponseStatus CustomerCareCU(Customercare customercare)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                customercare.Password = HashEncryption.O.CreatePassword(8);
                IProcedure proc = new ProcCreateCC(_dal);
                var resp = (AlertReplacementModel)proc.Call(customercare);
                res.Statuscode = resp.Statuscode;
                res.Msg = resp.Msg;
                if (res.Statuscode == ErrorCodes.One && customercare.ID < 1)
                {
                    resp.FormatID = MessageFormat.Registration;
                    IAlertML alertMl = new AlertML(_accessor, _env);
                    Parallel.Invoke(() => alertMl.RegistrationSMS(resp),
                    () => alertMl.RegistrationEmail(resp),
                    () => alertMl.SocialAlert(resp));
                }
            }
            return res;
        }
        public IResponseStatus ChangeOTPStatusCC(int CCID, int type)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID==Role.Admin)
            {
                var _req = new ProcToggleStatusRequest
                {
                    UserID = CCID,
                    StatusColumn = type
                };
                IProcedure _proc = new ProcToggleStatusCC(_dal);
                _res = (ResponseStatus)_proc.Call(_req);
            }
            return _res;
        }
        public IEnumerable<MenuOperationAssigned> CCOperationAssigned(int RoleID)
        {
            var res = new List<MenuOperationAssigned>();
            if (RoleID > 0)
            {
                if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser)
                {
                    IProcedure _proc = new ProcOperationAssigned(_dal);
                    var _res = (List<OperationAssigned>)_proc.Call(RoleID);
                    if (_res.Count > 0)
                    {
                        res = _res.GroupBy(g => new { g.MenuID, g.Menu }).Select(x => new MenuOperationAssigned { MenuID = x.Key.MenuID, Menu = x.Key.Menu }).ToList();
                        foreach (var item in res)
                        {
                            item.OperationAssigneds = _res.Where(x => x.MenuID == item.MenuID)
                                .Select(x => new OperationAssigned { MenuID = x.MenuID, OperationID = x.OperationID, Operation = x.Operation, IsActive = x.IsActive }).ToList();
                        }
                    }
                }
            }
            return res;
        }
        public IResponseStatus UpdateOperationAssigned(int RoleID, int MenuID, int OperationID, bool IsActive)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg= ErrorCodes.AuthError
            };            
            if (_lr.RoleID==Role.Admin && _lr.LoginTypeID==LoginType.ApplicationUser)
            {
                var _req = new CommonReq
                {
                    CommonInt = RoleID,
                    CommonInt2 = MenuID,
                    CommonInt3 = OperationID,
                    CommonBool = IsActive,
                    CommonStr = _rinfo.GetRemoteIP(),
                    CommonStr2 = _rinfo.GetBrowserFullInfo()
                };
                IProcedure _proc = new ProcUpdateOperationAssigned(_dal);
                res = (ResponseStatus)_proc.Call(_req);
            }
            return res;
        }
        public IEnumerable<OperationAssigned> GetOperationAssigneds(int RoleID)
        {
            if (RoleID > 0) {
                IProcedure proc = new ProcOperationAssignedToCC(_dal);
                return (List<OperationAssigned>)proc.Call(RoleID);
            }
            return new List<OperationAssigned>();
        }
    }
}
