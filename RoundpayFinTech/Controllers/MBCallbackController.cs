using Fintech.AppCode.Configuration;
using Fintech.AppCode.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using RoundpayFinTech.AppCode.MiddleLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoundpayFinTech.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class MBCallbackController : Controller
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;

        public MBCallbackController(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
        }
        public IActionResult FingpayUpdate() 
        {
            StringBuilder resp = new StringBuilder("");
            var request = HttpContext.Request;
            var callbackAPIReq = new CallbackData
            {
                Method = request.Method,
                APIID = 0,
                Content = resp.ToString(),
                Scheme = request.Scheme,
                Path = request.Path
            };
            try
            {
                if (request.Method == "POST")
                {
                    if (request.HasFormContentType)
                    {
                        if (request.Form.Keys.Count > 0)
                        {
                            foreach (var item in request.Form.Keys)
                            {
                                request.Form.TryGetValue(item, out StringValues strVal);
                                if (resp.Length == 0)
                                {
                                    resp.AppendFormat("{0}={1}", item, strVal);
                                }
                                else
                                {
                                    resp.AppendFormat("&{0}={1}", item, strVal);
                                }
                            }
                        }
                    }
                    else
                    {
                        resp = new StringBuilder(request.GetRawBodyStringAsync().Result);

                    }
                }
                else
                {
                    resp = new StringBuilder(request.QueryString.ToString());
                }
                if (resp.Length == 0)
                {
                    resp = new StringBuilder(request.QueryString.ToString());
                }
            }
            catch (Exception ex)
            {
                resp = new StringBuilder(ex.Message);
            }
            callbackAPIReq.Content = resp.ToString();
            var ml = new CallbackML(_accessor, _env);
            var res = ml.LogCallBackRequestBool(callbackAPIReq).Result;
            return Json(res);
        }

        public IActionResult MahagramUpdate()
        {
            StringBuilder resp = new StringBuilder("");
            var request = HttpContext.Request;
            var callbackAPIReq = new CallbackData
            {
                Method = request.Method,
                APIID = 0,
                Content = resp.ToString(),
                Scheme = request.Scheme,
                Path = request.Path
            };
            try
            {
                if (request.Method == "POST")
                {
                    if (request.HasFormContentType)
                    {
                        if (request.Form.Keys.Count > 0)
                        {
                            foreach (var item in request.Form.Keys)
                            {
                                request.Form.TryGetValue(item, out StringValues strVal);
                                if (resp.Length == 0)
                                {
                                    resp.AppendFormat("{0}={1}", item, strVal);
                                }
                                else
                                {
                                    resp.AppendFormat("&{0}={1}", item, strVal);
                                }
                            }
                        }
                    }
                    else
                    {
                        resp = new StringBuilder(request.GetRawBodyStringAsync().Result);

                    }
                }
                else
                {
                    resp = new StringBuilder(request.QueryString.ToString());
                }
                if (resp.Length == 0)
                {
                    resp = new StringBuilder(request.QueryString.ToString());
                }
            }
            catch (Exception ex)
            {
                resp = new StringBuilder(ex.Message);
            }
            callbackAPIReq.Content = resp.ToString();
            var ml = new CallbackML(_accessor, _env);
            var res = ml.LogCallBackRequestBool(callbackAPIReq).Result;
            return Json(res);
        }
    }
}
