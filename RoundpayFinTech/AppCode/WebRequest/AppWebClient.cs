using System;
using System.Net;

namespace Fintech.AppCode.WebRequest
{
    public class AppWebClient : WebClient
    {
        private int _timeout { get; set; }
        public int Timeout { get; set; }
        public AppWebClient() => _timeout = 60 * 1000;
        public AppWebClient(int timout) => _timeout = timout;
        protected override System.Net.WebRequest GetWebRequest(Uri address)
        {
            var result = base.GetWebRequest(address);
            result.Timeout = _timeout;
            return result;
        }
    }
}
