using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public class DTHSubscriptionML : IDTHSubscriptionML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        private readonly IRequestInfo _info;

        public DTHSubscriptionML(IHttpContextAccessor accessor, IHostingEnvironment env, IRequestInfo info)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            _info = info;
        }
        public async Task<DTHConnectionServiceResponse> DoDTHSubscription(DTHConnectionServiceRequest dTHConnectionServiceRequest)
        {
            dTHConnectionServiceRequest.RequestIP = _info.GetRemoteIP();
            IProcedureAsync proc = new ProcDTHConnectionService(_dal);
            return (DTHConnectionServiceResponse)await proc.Call(dTHConnectionServiceRequest).ConfigureAwait(false);
        }
    }
}
