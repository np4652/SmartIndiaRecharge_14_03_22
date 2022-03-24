using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RoundpayFinTech.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class DeviceController : Controller
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        public DeviceController(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
        }

        [HttpPost]
        [Route("Home/device-register")]
        [Route("device-register")]
        public IActionResult DeviceRegister()
        {
            IDeviceML deviceML = new DeviceML(_accessor, _env);
            DeviceMaster device = new DeviceMaster();
            return PartialView("Partial/_DeviceAdd", deviceML.DeviceMasters());
        }

        [HttpPost]
        [Route("Home/device-save")]
        [Route("device-save")]
        public IActionResult DeviceSave(DeviceMaster deviceMaster)
        {
            DeviceSaveResp resp = new DeviceSaveResp();
            IDeviceML deviceML = new DeviceML(_accessor, _env);
            if (deviceML.DeviceSave(deviceMaster))
            {
                resp.Status = true;
                resp.Msg = "Device Updated Successfully";
            }
            else
            {
                resp.Status = true;
                resp.Msg = "!Error";
            }
            return Json(resp);
        }
        #region ReadDeviceMantra        
        [HttpPost]
        [Route("Home/getAepsBalance")]
        [Route("getAepsBalance")]
        public async Task<IActionResult> _GetBalance(string PidData, string aadhar, string bank, int t,string _lat,string _long)
        {
            IDeviceML deviceMl = new DeviceML(_accessor, _env);
            return Json(await deviceMl.CheckBalance(PidData, aadhar, bank, t,0,"", _lat, _long));
        }
        [HttpPost]
        [Route("Home/AepsWithdraw")]
        [Route("AepsWithdraw")]
        public async Task<IActionResult> _WithDraw(string PidData, string aadhar, string bank,int t, int amount, string _lat, string _long)
        {
            IDeviceML deviceMl = new DeviceML(_accessor, _env);
            return Json(await deviceMl.Withdrawl(PidData, aadhar, bank, t, amount,0,"", _lat, _long).ConfigureAwait(false));
        }
        
        [HttpPost]
        [Route("Home/Aadharpay")]
        [Route("Aadharpay")]
        public async Task<IActionResult> Aadharpay(string PidData, string aadhar, string bank, int t, int amount,string _lat,string _long)
        {
            IDeviceML deviceMl = new DeviceML(_accessor, _env);
            return Json(await deviceMl.Aadharpay(PidData, aadhar, bank, t, amount, 0,"", _lat, _long).ConfigureAwait(false));
        }
        #endregion
    }
}
