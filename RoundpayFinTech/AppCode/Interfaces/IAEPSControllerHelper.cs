using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface IAEPSControllerHelper
    {
        Task SaveAEPSLog(string APICode, string Method, string Requset, string Response);
    }
}
