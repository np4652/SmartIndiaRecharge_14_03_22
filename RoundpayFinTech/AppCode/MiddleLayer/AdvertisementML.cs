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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Validators;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public class AdvertisementML : IAdvertisementML
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



        public AdvertisementML(IHttpContextAccessor accessor, IHostingEnvironment env, bool IsSession = true)
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
        public IResponseStatus UpdateAdvertisement(FileUploadAdvertisementRequest advertisementrequest)
        {
            
            
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if(advertisementrequest.ContentText.Length>300 || advertisementrequest.ReturnUrl.Length==0)
            {
                _resp.Msg = "Invalid Data.";
                return _resp;
            }
            string filename = string.IsNullOrEmpty(advertisementrequest.ContentImage) ? DateTime.Now.Ticks.ToString() : advertisementrequest.ContentImage;
            var rml = new ResourceML(_accessor, _env);
            var res = rml.UploadAdvertisementImage(advertisementrequest.File, 4, advertisementrequest.UserID, filename);
            if (res.Statuscode == ErrorCodes.One)
            {
                try
                {
                    //validate.IsWebURL();
                    var address8 = new Uri(advertisementrequest.ReturnUrl, UriKind.Absolute);
                    if (!(address8.Scheme == Uri.UriSchemeHttps || address8.Scheme == Uri.UriSchemeHttp))
                    {
                        _resp.Msg = "Invalid Return Url.";
                        return _resp;
                    }
                }
                catch (Exception ex)
                {
                    _resp.Msg = "Invalid Return Url.";
                    return _resp;

                }
               

                advertisementrequest.ContentImage = filename;
                IProcedure _proc = new procUpdateAdvertisementRequest(_dal);
                _resp = (ResponseStatus)_proc.Call(advertisementrequest);
            }
            else
            {
                return res;
            }

            return _resp;
        }

        public IEnumerable<AdvertisementRequest> GetAdvertisement(AdvertisementRequest advertisementrequest)
        {
            var resp = new List<AdvertisementRequest>();
            IProcedure _proc = new proc_GetAdvertisementRequest(_dal);
            resp = (List<AdvertisementRequest>)_proc.Call(advertisementrequest);
            return resp;
        }

        public IEnumerable<AdvertisementPackage> GetAdvertisementPackage()
        {
            var resp = new List<AdvertisementPackage>();
            IProcedure _proc = new proc_GetAdvertisementPackage(_dal);
            resp = (List<AdvertisementPackage>)_proc.Call();
            return resp;
        }
        public ResponseStatus ApprovedAdvertisement(AdvertisementRequest UserData)
        {
            IProcedure _proc = new proc_ApproveAdvertisementRequest(_dal);
            var resp = (ResponseStatus)_proc.Call(UserData);

            return resp;
        }
        public IEnumerable<AdvertisementRequest> _GetAdvertisement()
        {
            var resp = new List<AdvertisementRequest>();
            IProcedure _proc = new proc_GetAdvertisementRequestforB2C(_dal);
            resp = (List<AdvertisementRequest>)_proc.Call();
            return resp;
        }

    }







}

