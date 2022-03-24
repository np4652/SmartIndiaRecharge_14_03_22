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
using System.Text;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public class CoupanVoucherML: ICoupanVoucherML
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

        

        public CoupanVoucherML(IHttpContextAccessor accessor, IHostingEnvironment env, bool IsSession = true)
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
        public IEnumerable<CoupanMaster> GetCouponMaster()
        {
            var resp = new List<CoupanMaster>();

            if (_lr.LoginTypeID == LoginType.ApplicationUser && !userML.IsEndUser() || userML.IsCustomerCareAuthorised(ActionCodes.AddEditSLAB))
            {
                var req = new CommonReq
                {
                    LoginID = _lr.UserID,
                    LoginTypeID = _lr.LoginTypeID,
                    CommonInt = -1
                };
                IProcedure proc = new procCouponMasterModule(_dal);
                resp = (List<CoupanMaster>)proc.Call(req);
            }
            return resp;

        }

        public CoupanMaster GetCouponMaster(int Id)
        {
            var resp = new CoupanMaster();

            if (_lr.LoginTypeID == LoginType.ApplicationUser && !userML.IsEndUser() || userML.IsCustomerCareAuthorised(ActionCodes.AddEditSLAB))
            {
                var req = new CommonReq
                {
                    LoginID = _lr.UserID,
                    LoginTypeID = _lr.LoginTypeID,
                    CommonInt = Id
                };
                IProcedure proc = new procCouponMasterModule(_dal);
                resp = (CoupanMaster)proc.Call(req);
               
            }
            return resp;

        }

        public IResponseStatus UpdateCouponMaster(CoupanMaster couponMaster)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            if ((_lr.LoginTypeID == LoginType.ApplicationUser && !_lr.RoleID.In(Role.APIUser, Role.Customer, Role.Retailor_Seller))  && (couponMaster.Min<= couponMaster.Max))
            {
                Validate validate = Validate.O;
                if (string.IsNullOrEmpty(couponMaster.VoucherType) || couponMaster.OID==0)
                {
                    _resp.Msg = "Invalid Data!";
                    return _resp;
                }
                var req = new CoupanReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    coupanMaster = couponMaster
                   
                };
                IProcedure _proc = new ProcUpdateCouponMaster(_dal);
                _resp = (ResponseStatus)_proc.Call(req);
            }
            return _resp;
        }

        public IEnumerable<CoupanVoucher> GetCouponVoucher(int Id)
        {
            var resp = new List<CoupanVoucher>();
            if ((_lr.RoleID == Role.Admin || _lr.IsWebsite) && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                var req = new CoupanVoucher
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                      ID = Id,
                    CommonInt = -1
                };
                IProcedure _proc = new ProcGetCouponVoucherList(_dal);
                resp = (List<CoupanVoucher>)_proc.Call(req);
            }
            return resp;
        }

        public IResponseStatus UpdateCouponVoucher(CoupanVoucher couponvoucher)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            if ((_lr.LoginTypeID == LoginType.ApplicationUser && !_lr.RoleID.In(Role.APIUser, Role.Customer, Role.Retailor_Seller)) || userML.IsCustomerCareAuthorised(ActionCodes.AddEditSLAB))
            {
                Validate validate = Validate.O;
                if (string.IsNullOrEmpty(couponvoucher.APIID)|| string.IsNullOrEmpty(couponvoucher.CouponCode))
                {
                    _resp.Msg = "Invalid Data!";
                    return _resp;
                }


                      couponvoucher.LoginTypeID = _lr.LoginTypeID;
                    couponvoucher.LoginID = _lr.UserID;
                   

               
                IProcedure _proc = new ProcUpdateCouponVoucher(_dal);
                _resp = (ResponseStatus)_proc.Call(couponvoucher);
            }
            return _resp;
        }

        public IEnumerable<DenominationVoucher> GetDenominationVoucher()
        {
            var resp = new List<DenominationVoucher>();
            if ((_lr.RoleID == Role.Admin || _lr.IsWebsite) && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                var req = new CoupanVoucher
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    
                };
                IProcedure _proc = new ProcGetDenominationVoucher(_dal);
                resp = (List<DenominationVoucher>)_proc.Call(req);
            }
            return resp;
        }
        public IResponseStatus UpdateCouponSetting(DenominationVoucher para)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            if ((_lr.LoginTypeID == LoginType.ApplicationUser && !_lr.RoleID.In(Role.APIUser, Role.Customer, Role.Retailor_Seller)) || userML.IsCustomerCareAuthorised(ActionCodes.AddEditSLAB)&& para.DenominationIDs.Length>0)
            {



                para.LoginTypeID = _lr.LoginTypeID;
                para.LoginID = _lr.UserID;
              



                IProcedure _proc = new ProcUpdateCouponSetting(_dal);
                _resp = (ResponseStatus)_proc.Call(para);
            }
            return _resp;
        }

        public IResponseStatus ChangeCouponStatus(int ID)
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
                IProcedure _proc = new ProcUpdateCouponStatus(_dal);
                _res = (ResponseStatus)_proc.Call(commonReq);
            }
            return _res;

        }

        public IEnumerable<DenominationVoucher> GetCouponSetting(int ID)
        {
            var resp = new List<DenominationVoucher>();

            if (_lr.LoginTypeID == LoginType.ApplicationUser && !userML.IsEndUser() || userML.IsCustomerCareAuthorised(ActionCodes.AddEditSLAB))
            {
                var req = new CommonReq
                {
                    LoginID = _lr.UserID,
                    LoginTypeID = _lr.LoginTypeID,
                    CommonInt = ID
                };
                IProcedure proc = new procGetCouponSetting(_dal);
                resp = (List<DenominationVoucher>)proc.Call(req);
            }
            return resp;

        }
        public async Task<IResponseStatus> UploadCouponExcelUPloda(CoupanVoucherEXlReq reqData)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if ((_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser) || userML.IsCustomerCareAuthorised(ActionCodes.PendingResend))
            {
                var _req = new CoupanVoucherEXlReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                     VocherID=reqData.VocherID,
                    APIID = reqData.APIID,
                    Couponvocher = reqData.Couponvocher,
                    CommonStr = _rinfo.GetRemoteIP(),
                    CommonStr2 = _rinfo.GetBrowser(),
                };
                IProcedureAsync _proc = new ProcInsertBulkCoupon(_dal);
                _res = (ResponseStatus)await _proc.Call(_req).ConfigureAwait(false);
            }
            return _res;
        }
        public IEnumerable<VoucherImage> GetCouponvocherImage(int OID)
        {
            var ret = new List<VoucherImage>();
            try
            {
                var root = DOCType.CouponVoucherPath.Replace("{0}", OID.ToString());
                var root1 = DOCType.CouponVoucherSufix.Replace("{0}", OID.ToString());
                var crf = _rinfo.GetCurrentReqInfo();
                return Directory.EnumerateFiles(root).Select(x => new VoucherImage
                {
                    SiteFileName = Path.GetFileName(x),
                    SiteResourceUrl = new StringBuilder().AppendFormat("{0}://{1}/{2}/{3}", crf.Scheme, crf.HostValue, string.Format (root1), Path.GetFileName(x)).ToString(),
                    
                }).OrderBy(x => x.FileName);
            }
            catch (System.Exception ex)
            {
               
                
            }
            return ret;
        }



        public IResponseStatus DelCouponVoucher(CoupanVoucher couponvoucher)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            if ((_lr.LoginTypeID == LoginType.ApplicationUser && !_lr.RoleID.In(Role.APIUser, Role.Customer, Role.Retailor_Seller)) || userML.IsCustomerCareAuthorised(ActionCodes.AddEditSLAB))
            {
                couponvoucher.LoginTypeID = _lr.LoginTypeID;
                couponvoucher.LoginID = _lr.UserID;
                IProcedure _proc = new ProcDelCouponVoucher(_dal);
                _resp = (ResponseStatus)_proc.Call(couponvoucher);
            }
            return _resp;
        }

    }
}
