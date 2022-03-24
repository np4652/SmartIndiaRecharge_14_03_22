using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Microsoft.AspNetCore.Http;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.Interfaces
{
    public interface IIPAddressML
    {
        IResponseStatus UpdateIpStatus(int Id, bool sts);
        IEnumerable<IPAddressModel> GetIPAddress(string MobileNo, int ID);
        IResponseStatus SaveIPAddress(IPAddressModel iPAddressModel);
        IResponseStatus RemoveIp(int Id);
        IResponseStatus SendIPOTP();
    }
}
