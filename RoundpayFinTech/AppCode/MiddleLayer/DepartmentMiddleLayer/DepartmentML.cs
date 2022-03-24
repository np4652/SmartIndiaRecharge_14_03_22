using Fintech.AppCode;
using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using NUglify.Helpers;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.DL.DepartmentDL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.DepartmentModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.MiddleLayer.DepartmentMiddleLayer
{
    public class DepartmentML : IDepartmentML, IMenuOpsML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly ISession _session;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        private readonly LoginResponse _lr;
        public DepartmentML(IHttpContextAccessor accessor, IHostingEnvironment env, bool InSession = true)
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
        public Department GetDepartment(int ID)
        {
            var res = new Department();
            if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser && ID>0)
            {
                var req = new CommonReq
                {
                    LoginID = _lr.UserID,
                    CommonBool = true,
                    CommonInt = ID
                };
                IProcedure proc = new ProcGetDepartmentAndRoles(_dal);
                res = (Department)proc.Call(req);
            }
            return res;
        }
        public IEnumerable<Department> GetDepartment()
        {
            var res = new List<Department>();
            if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                var req = new CommonReq
                {
                    LoginID = _lr.UserID,
                    CommonBool = true,
                    CommonInt = 0
                };
                IProcedure proc = new ProcGetDepartmentAndRoles(_dal);
                res = (List<Department>)proc.Call(req);
            }

            return res;
        }
        public DepartmentRole GetDepartmentRole(int ID)
        {
            var res = new DepartmentRole();
            if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser && ID > 0)
            {
                var req = new CommonReq
                {
                    LoginID = _lr.UserID,
                    CommonBool1 = false,
                    CommonInt = ID
                };
                IProcedure proc = new ProcGetDepartmentAndRoles(_dal);
                res = (DepartmentRole)proc.Call(req);
            }
            return res;
        }
        public IEnumerable<DepartmentRole> GetDepartmentRoles(int DeptID = 0)
        {
            var res = new List<DepartmentRole>();
            if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                var req = new CommonReq
                {
                    LoginID = _lr.UserID,
                    CommonBool1 = DeptID>0,
                    CommonInt = DeptID
                };
                IProcedure proc = new ProcGetDepartmentAndRoles(_dal);
                res = (List<DepartmentRole>)proc.Call(req);
            }
            return res;
        }
        public IResponseStatus SaveDepartment(Department department)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                if ((department.Name ?? "").Trim() == "")
                {
                    res.Msg = ErrorCodes.InvalidParam + " Role";
                    return res;
                }
                IProcedure proc = new ProcSaveDepartment(_dal);
                res = (ResponseStatus)proc.Call(department);
            }
            return res;
        }
        public IResponseStatus SaveDepartmentRole(DepartmentRole departmentRole)
        {
            var res = new ResponseStatus
            {
                Statuscode=ErrorCodes.Minus1,
                Msg=ErrorCodes.AuthError
            };
            if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                if ((departmentRole.Name ?? "").Trim() == "")
                {
                    res.Msg = ErrorCodes.InvalidParam + " Role";
                    return res;
                }
                if ((departmentRole.Prefix?? "").Trim().Length != 2)
                {
                    res.Msg = ErrorCodes.InvalidParam + " Prefix length should be 2";
                    return res;
                }
                IProcedure proc = new ProcSaveDepartmentRole(_dal);
                res = (ResponseStatus)proc.Call(departmentRole);
            }
            return res;
        }
        public async Task<IEnumerable<MasterMenu>> GetMenuOperations(string str)
        {
            var res = new List<MasterMenu>();
            if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                if (string.IsNullOrEmpty(str))
                    return res;
                IProcedureAsync _proc = new ProcMenuOperations(_dal);
                res = (List<MasterMenu>)await _proc.Call(str);
            }
            return res;
        }
        public async Task<IResponseStatus> UpdateMenuOperations(MenuOperation mo)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.RoleID == Role.Admin && _lr.LoginTypeID==LoginType.ApplicationUser)
            {
                IProcedureAsync _proc = new ProcUpdateMenuOperations(_dal);
                _res = (ResponseStatus)await _proc.Call(mo);
            }
            return _res;

        }
        public async Task<IEnumerable<TemplateFormatKeyMappingDisplay>> GetMapMsgTamplateToKey(string str)
        {
            var res = new TemplateFormatKeyMapping();
            var _Finalres = new List<TemplateFormatKeyMappingDisplay>();
            if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                IProcedureAsync _proc = new ProcGetMapMsgTamplateToKey(_dal);
                res = (TemplateFormatKeyMapping)await _proc.Call(str);
                foreach (var i in res.Messgaes)
                {
                    var read = new TemplateFormatKeyMappingDisplay
                    {
                        Messgae = i.FormatType,
                        FormatID = i.ID,
                        Keyswords = res.Keyswords.DistinctBy(x => x.ID).ToList()
                    };
                    _Finalres.Add(read);
                }
            }
            return _Finalres;
        }
        public async Task<IResponseStatus> MapTemplateAndKey(int FormatID, int KeyID, bool IsActive)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            var param = new CommonReq()
            {
                CommonInt = FormatID,
                CommonInt2 = KeyID,
                CommonBool = IsActive
            };
            if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                IProcedureAsync _proc = new ProcMapTemplateAndKey(_dal);
                _res = (ResponseStatus)await _proc.Call(param);
            }
            return _res;

        }
    }
}
