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
    public class SellerCouponModuleML : ISellerCouponModuleML
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



        public SellerCouponModuleML(IHttpContextAccessor accessor, IHostingEnvironment env, bool IsSession = true)
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

        public IEnumerable<CoupanMaster> GetCouponVoucherList(int OPID)
        {
            var resp = new List<CoupanMaster>();

            if (_lr.LoginTypeID == LoginType.ApplicationUser )
            {
                var req = new CommonReq
                {
                    LoginID = _lr.UserID,
                    LoginTypeID = _lr.LoginTypeID,
                    CommonInt = OPID
                };
                IProcedure proc = new procCouponMasterList(_dal);
                resp = (List<CoupanMaster>)proc.Call(req);

            }

            return resp;
        }

        public IResponseStatus SaveCouponVocher(CouponData data)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

           
            if (data == null ||( data.Amount ==0 && data.Quantity ==0 && data.OID ==0 ))
            {
                _resp.Statuscode = ErrorCodes.Minus1;
                _resp.Msg = ErrorCodes.InvalidParam;
                return _resp;

            };

            if (!Validate.O.IsEmail(data.To ?? ""))
            {
                _resp.Msg = ErrorCodes.InvalidParam + " To Email.";
                return _resp;
            }

            data.LoginTypeID = _lr.LoginTypeID;
                data.LoginID = _lr.UserID;
                data.RequestIP = _rinfo.GetRemoteIP();
                data.Browser = _rinfo.GetBrowser();
                data.RequestMode = RequestMode.PANEL;
                IProcedure _proc = new proc_VoucherCouponService(_dal);
                _resp = (ResponseStatus)_proc.Call(data);

            if (_resp.Statuscode == 1) {
                AlertReplacementModel para = new AlertReplacementModel();
                para.CouponCode = _resp.CommonStr;
              
                para.Amount = data.Amount;
                para.CouponQty = data.Quantity;
                para.CouponValdity = 0;
                para.UserID = _lr.UserID;
                para.UserName = _lr.Name;
                para.UserEmailID = data.To;
                para.LoginID = _lr.UserID;
                para.WID = _lr.WID;
                return SendCouponCode(para);
                    
                    }
          
            return _resp;
        }

        public IResponseStatus SendCouponCode(AlertReplacementModel param)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            param.FormatID = 34;

            var a = new AlertML(_accessor, _env).CouponVocherEmail(param, true);


            return a;


        }

            public bool CountImage()
        {
            var root = DOCType.CouponVoucherPath.Replace("{0}/", "");
            var a=Directory.GetFiles(root, "*", SearchOption.AllDirectories).Length;
            return a>=10?true:false;
        }

        public IEnumerable<ImageCount_OID> GetCouponvocherImage(int optype)
        {
            var ret = new List<ImageCount_OID>();
            var OpML = new OperatorML(_accessor, _env);
            
            try
            {
                IEnumerable<OperatorDetail> OpDetail = OpML.GetOperators(optype);
                foreach (var itm in OpDetail)
                {
                    var root = DOCType.CouponVoucherPath.Replace("{0}", itm.OID.ToString());
                    var _img = new ImageCount_OID {
                        OID = itm.OID,
                        Oname = itm.Name,
                        ImageCount = Directory.EnumerateFiles(root).Count(),
                       CouponDetail= GetCouponVoucherList(itm.OID).Take(2)

                   };
                    if(_img.CouponDetail!=null && _img.CouponDetail.Count()>0)
                    ret.Add(_img);

                }


            }
            catch (System.Exception ex)
            {


            }
            return ret.OrderByDescending(x=>x.ImageCount);
        }
    }

} 