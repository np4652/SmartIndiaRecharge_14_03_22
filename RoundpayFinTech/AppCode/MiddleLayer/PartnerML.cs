using Fintech.AppCode;
using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.DL.Partner;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using Validators;

namespace RoundpayFinTech.AppCode.MiddleLayer
{

    public sealed class PartnerML : IPartnerML
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

        public PartnerML(IHttpContextAccessor accessor, IHostingEnvironment env, bool IsInSession = true)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            _rinfo = new RequestInfo(_accessor, _env);
            if (IsInSession)
            {
                _session = _accessor != null ? _accessor.HttpContext.Session : null;
                bool IsProd = _env.IsProduction();
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory());
                builder.AddJsonFile((IsProd ? "appsettings.json" : "appsettings.Development.json"));
                builder.AddEnvironmentVariables();
                Configuration = builder.Build();
                _WInfo = new LoginML(_accessor, _env).GetWebsiteInfo();
                _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
            }
            _resourceML = new ResourceML(_accessor, _env);

        }

        public ResponseStatus CallSavePartner(PartnerCreate req)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            req.UserID = _lr.UserID;
            IProcedure _p = new ProcSavePartner(_dal);
            _resp = (ResponseStatus)_p.Call(req);
            return _resp;
        }
        public PartnerListResp GetPartnerList(int UserID, string s = "")
        {
            var _resp = new PartnerListResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var _req = new PartnerCreate { ID = 0, UserID = UserID, Status = s != "partners" ? 1 : 0 };
            IProcedure _p = new ProcGetPartnerList(_dal);
            _resp.list = (List<PartnerCreate>)_p.Call(_req);

            return _resp;
        }
        public PartnerDetailsResp GetPartnerByID(int ID)
        {
            var _resp = new PartnerDetailsResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var _req = new PartnerCreate { ID = ID, UserID = ID };
            IProcedure _p = new ProcGetPartnerList(_dal);
            _resp.data = (PartnerCreate)_p.Call(_req);
            if (_resp.data != null)
            {
                _resp.Statuscode = 1;
                _resp.Msg = "Record found";
            }
            return _resp;
        }
        public PartnerDetailsResp GetPartnerByID(PartnerCreate req) {
            var _resp = new PartnerDetailsResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            IProcedure _p = new ProcGetPartnerList(_dal);
            _resp.data = (PartnerCreate)_p.Call(req);
            if (_resp.data != null)
            {
                _resp.Statuscode = 1;
                _resp.Msg = "Record found";
            }
            return _resp;
        }

        public IResponseStatus ChangeStatus(int ID)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            if ((_lr.LoginTypeID == LoginType.ApplicationUser))
            {
                var _req = new ProcToggleStatusRequest
                {
                    LoginID = _lr.UserID,

                    UserID = ID,

                };
                IProcedure _proc = new ProcToggleStatusPartner(_dal);
                _res = (ResponseStatus)_proc.Call(_req);
            }
            return _res;
        }

        public IResponseStatus UpdateStatus(int Status, int ID)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            if ((_lr.LoginTypeID == LoginType.ApplicationUser))
            {
                var _req = new ProcToggleStatusRequest
                {
                    LoginID = _lr.UserID,
                    StatusColumn = Status,
                    UserID = ID,

                };
                IProcedure _proc = new ProcUpdateStatusPartner(_dal);
                _res = (ResponseStatus)_proc.Call(_req);
            }
            return _res;
        }

        public async Task<IResponseStatus> ValidateAEPSURL(string UrlSession)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (string.IsNullOrEmpty(UrlSession))
            {
                return res;
            }
            if (!string.IsNullOrEmpty(UrlSession)) {
                var urlJsonString = HashEncryption.O.Decrypt(UrlSession);
                if (Validate.O.ValidateJSON(urlJsonString))
                {
                    var aEPSURLSessionResp = JsonConvert.DeserializeObject<AEPSURLSessionResp>(urlJsonString);
                    ITransactionML transactionML = new TransactionML(_dal, _rinfo);
                    res = await transactionML.ValidateAEPSURL(aEPSURLSessionResp).ConfigureAwait(false);
                    if (res.Statuscode == ErrorCodes.One)
                    {
                        _session.SetObjectAsJson(SessionKeys.AEPSSession, aEPSURLSessionResp);
                    }
                }
            }
            
            return res;
        }
        public AEPSURLSessionResp IsInValidPartnerSession()
        {
            try
            {
                return _session.GetObjectFromJson<AEPSURLSessionResp>(SessionKeys.AEPSSession);
            }
            catch (Exception ex)
            {
                
            }
            return null;
        }

        public bool CheckPsaId(string PSAId)
        {
            bool _res = false;
            if ((_lr.LoginTypeID == LoginType.ApplicationUser))
            {
                var _req = new CommonReq
                {
                    LoginID = _lr.UserID,
                    LoginTypeID = _lr.LoginTypeID,
                    CommonStr = PSAId
                };
                IProcedure _proc = new ProcGetPsaPrefixAvailability(_dal);
                _res = (bool)_proc.Call(_req);
            }
            return _res;
        }

        public bool UpdatePsaId(string PSAId, string FatherName)
        {
            bool _res = false;
            if ((_lr.LoginTypeID == LoginType.ApplicationUser))
            {
                var _req = new CommonReq
                {
                    LoginID = _lr.UserID,
                    LoginTypeID = _lr.LoginTypeID,
                    CommonStr = PSAId,
                    CommonStr2 = FatherName
                };
                IProcedure _proc = new ProcUpdatePsaPrefix(_dal);
                _res = (bool)_proc.Call(_req);
            }
            return _res;
        }

        public PartnerAEPSResponseModel PartnerAEPS(int partnerID)
        {
            PartnerAEPSResponseModel res = new PartnerAEPSResponseModel()
            {
                StatusCode = ErrorCodes.Minus1,
                Msg = ErrorCodes.FAILED
            };
            CommonReq req = new CommonReq()
            {
                CommonInt = partnerID
            };
            IProcedure _proc = new ProcPartnerGetLoginPageInfo(_dal);
            res = (PartnerAEPSResponseModel)_proc.Call(req);
            res.LogoPath = "/Image/Partner/LOGO/" + partnerID.ToString() + ".png";
            res.BannerPath = "/Image/Partner/Banner/" + partnerID.ToString() + ".png";
            res.BgPath = "/Image/Partner/BGI/" + partnerID.ToString() + ".png";
            return res;
        }

        public List<BankMaster> bindAEPSBanks(string bankName)
        {
            CommonReq req = new CommonReq
            {
                LoginTypeID = 0,
                LoginID = 0,
                CommonStr = bankName
            };
            IProcedure _proc = new ProcbindAEPSBanks(_dal);
            return (List<BankMaster>)_proc.Call(req);
        }
    }
}
