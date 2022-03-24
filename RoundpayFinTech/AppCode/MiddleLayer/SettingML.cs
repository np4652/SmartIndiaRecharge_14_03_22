using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.Models;
using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.Model.ProcModel
{
    public class SettingML : ISettingML, ISettingsWithTemplateML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly ISession _session;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        private readonly LoginResponse _lr;
        public SettingML(IHttpContextAccessor accessor, IHostingEnvironment env, bool InSession = true)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            if (InSession)
            {
                _session = _accessor.HttpContext.Session;
                _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
            }
        }
        public IResponseStatus SaveSystemSetting(SystemSetting setting)
        {
            IResponseStatus _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                setting.LoginID = _lr.UserID;
                setting.LT = _lr.LoginTypeID;
                IProcedure _proc = new ProcSystemSetting(_dal);
                _res = (IResponseStatus)_proc.Call(setting);
            }
            return _res;
        }
        public SystemSetting GetSettings()
        {
            var setting = new SystemSetting();
            if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                var req = new CommonReq()
                {
                    LoginID = _lr.UserID,
                    LoginTypeID = _lr.LoginTypeID
                };
                IProcedure _proc = new ProcGetSystemSetting(_dal);
                setting = (SystemSetting)_proc.Call(req);
                IUserML userML = new UserML(_accessor, _env);
                setting.TransactionModes = userML.GetTransactionMode();
            }
            return setting;
        }

        public ApplicationSettingModel GetASSettings()
        {
            IProcedure _proc = new ProcGetASetting(_dal);
            return (ApplicationSettingModel)_proc.Call();
        }

        public SystemSetting GetSettingsForApp()
        {
            var setting = new SystemSetting();
            var req = new CommonReq()
            {
                LoginID = 1,
                LoginTypeID = 1
            };
            IProcedure _proc = new ProcGetSystemSetting(_dal);
            setting = (SystemSetting)_proc.Call(req);
            return setting;
        }
        public bool UpdateSignupSlab(int SlabID)
        {
            if ((_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser) || _lr.LoginTypeID == LoginType.CustomerCare)
            {
                IProcedure _proc = new ProcUpdateSignupSlabID(_dal);
                return (bool)_proc.Call(SlabID);
            }
            return false;
        }
        public IResponseStatus UpdateAddMoneyCharge(int OID, decimal Charge, bool Is)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                var req = new CommonReq
                {
                    LoginID = _lr.UserID,
                    LoginTypeID = _lr.LoginTypeID,
                    CommonInt = OID,
                    CommonDecimal = Charge,
                    CommonBool = Is
                };
                IProcedure proc = new ProcUpdateAddMoneyCharge(_dal);
                _res = (ResponseStatus)proc.Call(req);
            }
            return _res;
        }

        #region Amit
        public SMSSetting getSMSSettingsWithFormat(CommonReq req)
        {
            var _res = new SMSSetting();
            if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                var _req = new CommonReq()
                {
                    LoginID = _lr.UserID,
                    CommonInt = req.CommonInt
                };
                IProcedure _proc = new ProcGetSMSSettingByFormat(_dal);
                _res = (SMSSetting)_proc.Call(req);
            }
            return _res;
        }

        public EmailSettingswithFormat getEmailSettingsWithFormat(CommonReq req)
        {
            var _res = new EmailSettingswithFormat();
            if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                var _req = new CommonReq()
                {
                    LoginID = _lr.UserID,
                    CommonInt = req.CommonInt
                };
                IProcedure _proc = new ProcGetEmailSettingByFormat(_dal);
                _res = (EmailSettingswithFormat)_proc.Call(req);
            }
            return _res;
        }
        #endregion
        #region ReferralSignup
        public ResponseStatus UpdateReferralSetting(bool r, bool u)
        {
            ResponseStatus _res = new ResponseStatus()
            {
                Msg = ErrorCodes.AnError,
                Statuscode = ErrorCodes.Minus1
            };
            CommonReq _req = new CommonReq()
            {
                CommonBool = r,
                CommonBool2 = u
            };
            IProcedure _proc = new ProcUpdateReferralSetting(_dal);
            if ((bool)_proc.Call(_req))
            {
                _res.Msg = ErrorCodes.SUCCESS;
                _res.Statuscode = ErrorCodes.One;
            }
            return _res;
        }
        public ReferralSetting GetReferralSetting()
        {
            ReferralSetting _res = new ReferralSetting()
            {
                Msg = ErrorCodes.AnError,
                Statuscode = ErrorCodes.Minus1
            };

            IProcedure _proc = new ProcGetReferralSetting(_dal);
            _res = (ReferralSetting)_proc.Call(_res);
            return _res;
        }
        public IEnumerable<RoleMaster> GetRoleForReferral(int _userID)
        {
            var _roleMaster = new List<RoleMaster>();
            IProcedure _proc = new ProcGetRoleForReferral(_dal);
            _roleMaster = (List<RoleMaster>)_proc.Call(_userID);
            return _roleMaster;
        }
        #endregion
    }
}
