using Fintech.AppCode;
using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System.Collections.Generic;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public class ErrorCodeML : IErrorCodeML, IErrorCodeMLParent
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly ISession _session;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        private readonly IRequestInfo _rinfo;
        private readonly WebsiteInfo _WInfo;
        private readonly LoginResponse _lr;
        private readonly IUserML userML;
        public ErrorCodeML(IHttpContextAccessor accessor, IHostingEnvironment env, bool InSession = true)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            _rinfo = new RequestInfo(_accessor, _env);
            if (InSession)
            {
                _session = _accessor.HttpContext.Session;
                _WInfo = new LoginML(_accessor, _env).GetWebsiteInfo();
                _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
                userML = new UserML(_lr);
            }
        }
        public ErrorCodeML(IDAL dal)
        {
            _dal = dal;
        }
        public IEnumerable<ErrorCodeDetail> Get()
        {
            IProcedure _proc = new ProcGetErrorCode(_dal);
            return (List<ErrorCodeDetail>)_proc.Call(new CommonReq());
        }

        public ErrorCodeDetail Get(int ID)
        {
            if (ID > 0)
            {
                IProcedure _proc = new ProcGetErrorCode(_dal);
                return (ErrorCodeDetail)_proc.Call(new CommonReq { CommonInt = ID });
            }
            return new ErrorCodeDetail();
        }

        public ErrorCodeDetail Get(string ErrCode)
        {
            if ((ErrCode ?? "") != "")
            {
                IProcedure _proc = new ProcGetErrorCode(_dal);
                return (ErrorCodeDetail)_proc.Call(new CommonReq { CommonStr = ErrCode });
            }
            return new ErrorCodeDetail();
        }

        public List<APIErrorCode> GetAPIErrorCode()
        {
            IProcedure _proc = new ProcGetAPIErrorCode(_dal);
            return (List<APIErrorCode>)_proc.Call();
        }
        public APIErrorCode GetAPIErrorCode(APIErrorCode APIErrCode)
        {
            if (!string.IsNullOrEmpty(APIErrCode.APICode) && !string.IsNullOrEmpty(APIErrCode.GroupCode))
            {
                IProcedure _proc = new ProcGetAPIErrorCode(_dal);
                return (APIErrorCode)_proc.Call(APIErrCode);
            }
            return new APIErrorCode();
        }
        public ErrorCodeDetail GetAPIErrorCodeDescription(string APIGroupCode, string APIErrorCode)
        {
            if (!string.IsNullOrEmpty(APIGroupCode) && !string.IsNullOrEmpty(APIErrorCode))
            {
                IProcedure _proc = new ProcGetErrorCodeDescByAPIGroup(_dal);
                return (ErrorCodeDetail)_proc.Call(new CommonReq
                {
                    CommonStr= APIGroupCode,
                    CommonStr2= APIErrorCode
                });
            }
            return new ErrorCodeDetail();
        }
        public IEnumerable<ErrorTypeMaster> GetTypes()
        {
            var proc = new ProcGetErrorCode(_dal);
            return proc.ErTypeMasters();
        }

        public IResponseStatus Save(ErrorCodeDetail errorCodeDetail)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin || userML.IsCustomerCareAuthorised(ActionCodes.AddEditOPERATOR))
            {
                if (!errorCodeDetail.IsCode && (string.IsNullOrEmpty(errorCodeDetail.Error) || Validators.Validate.O.IsNumeric(errorCodeDetail.Error)))
                {
                    res.Msg = "Error is non numeric mandatory field and can not be start with number.";
                    return res;
                }
                if (string.IsNullOrEmpty(errorCodeDetail.Code) || errorCodeDetail.Code.Trim().Length > 10)
                {
                    res.Msg = "Code is mandatory not moere than 10 charecters";
                    return res;
                }
                var errorCodeDetailReq = new ErrorCodeDetailReq
                {
                    Detail = errorCodeDetail,
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonStr = _rinfo.GetRemoteIP(),
                    CommonStr2 = _rinfo.GetBrowser()
                };
                IProcedure proc = new ProcErrorCodeCU(_dal);
                return (IResponseStatus)proc.Call(errorCodeDetailReq);
            }
            return res;
        }

        public IResponseStatus update(ErrorCodeDetail errorCodeDetail)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin || userML.IsCustomerCareAuthorised(ActionCodes.AddEditOPERATOR))
            {
                var errorCodeDetailReq = new ErrorCodeDetailReq
                {
                    Detail = errorCodeDetail,
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonStr = _rinfo.GetRemoteIP(),
                    CommonStr2 = _rinfo.GetBrowser()
                };
                IProcedure proc = new ProErrorCodeupdate(_dal);
                return (IResponseStatus)proc.Call(errorCodeDetailReq);
            }
            return res;
        }

        public IResponseStatus UpdateAPIErCode(APIErrorCode aPIErrorCode)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            if ((_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser) || userML.IsCustomerCareAuthorised(ActionCodes.AddEditOPERATOR))
            {
                if (string.IsNullOrEmpty(aPIErrorCode.APICode))
                {
                    _resp.Msg = "ErrorCode is mandatory field!";
                    return _resp;
                }
                var req = new APIErrorCodeReq
                {
                    APIErrorCode = aPIErrorCode,
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonStr = _rinfo.GetRemoteIP(),
                    CommonStr2 = _rinfo.GetBrowser()
                };
                IProcedure _proc = new ProcUpdateAPIErrCode(_dal);
                _resp = (ResponseStatus)_proc.Call(req);
            }
            return _resp;
        }
    }
}
