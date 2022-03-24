using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fintech.AppCode.WebRequest
{
    public interface IAppWebRequest
    {
        string CallUsingHttpWebRequest_GET(string URL);
        string CallUsingHttpWebRequest_POST(string URL, string PostData,string ContentType);
        string CallUsingWebClient_GET(string URL, int Timeout);
        int GETStatusCode(string URL);

        Task<string> CallUsingHttpWebRequest_GETAsync(string URL, IDictionary<string, string> headers = null);
        Task<string> CallUsingHttpWebRequest_POSTAsync(string URL, string PostData,string ContentType);
        Task<string> CallUsingWebClient_GETAsync(string URL, int Timeout);
        Task<int> GETStatusCodeAsync(string URL);
    }
}
