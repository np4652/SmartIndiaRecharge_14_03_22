using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using Fintech.AppCode.WebRequest;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.ThirdParty.Razorpay
{
    public class RazorpaySmartCollectML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration Configuration;
        private readonly IDAL _dal;
        private readonly RZRPayAppSetting apiSetting;

        public RazorpaySmartCollectML(IHttpContextAccessor accessor, IHostingEnvironment env, IDAL dal)
        {
            _accessor = accessor;
            _env = env;
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile((_env.IsProduction() ? "appsettings.json" : "appsettings.Development.json"));
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            _dal = dal;
            apiSetting = AppSetting();
        }
        private RZRPayAppSetting AppSetting()
        {
            var setting = new RZRPayAppSetting();
            try
            {
                setting = new RZRPayAppSetting
                {
                    key_id = Configuration["SMARTCOLLECT:RZRPAY:key_id"],
                    key_secret = Configuration["SMARTCOLLECT:RZRPAY:key_secret"],
                    CreateCustomerURL = Configuration["SMARTCOLLECT:RZRPAY:CreateCustomerURL"],
                    CreateVirtualAccountURL = Configuration["SMARTCOLLECT:RZRPAY:CreateVirtualAccountURL"]
                };
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "RZRPayAppSetting",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                });
            }
            return setting;
        }
        public SmartCollectCreateCustomerResponse CreateCustomer(SmartCollectCreateCustomerRequest smartCollectCreateCustomerRequest)
        {
            var resp = new SmartCollectCreateCustomerResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Customer can not be created"
            };
            var payload = new
            {
                name = smartCollectCreateCustomerRequest.Name,
                email = smartCollectCreateCustomerRequest.EmailID,
                contact = smartCollectCreateCustomerRequest.Contact,
                fail_existing = "0",
                gstin = smartCollectCreateCustomerRequest.GSTIN,
                notes = new
                {
                    notes_key_1 = smartCollectCreateCustomerRequest.NotesKey1,
                    notes_key_2 = smartCollectCreateCustomerRequest.NotesKey2
                }
            };

            string BasicAuthString = Convert.ToBase64String(Encoding.UTF8.GetBytes(apiSetting.key_id + ":" + apiSetting.key_secret));
            var header = new Dictionary<string, string>
            {
                { "Authorization",string.Format("Basic {0}",BasicAuthString)}
            };
            var Request = apiSetting.CreateCustomerURL + JsonConvert.SerializeObject(header) + "|" + JsonConvert.SerializeObject(payload);
            var response = string.Empty;
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(apiSetting.CreateCustomerURL, payload, header).Result;
                if (!string.IsNullOrEmpty(response))
                {
                    var apiResp = JsonConvert.DeserializeObject<RZRPayCreateCustomerResp>(response);
                    if (apiResp != null)
                    {
                        if (!string.IsNullOrEmpty(apiResp.id))
                        {
                            resp.Statuscode = ErrorCodes.One;
                            resp.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                            resp.CustomerID = apiResp.id;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response = "Exception:" + ex.Message + "|" + response;
            }
            new ProcLogAPICommonReqResp(_dal).Call(new LogAPICommonReqRespModel
            {
                _Request = Request,
                _Response = response,
                _ClassName = GetType().Name,
                _Method = "CreateCustomer"
            });
            return resp;
        }
        public SmartCollectionVACResponse CreateVirtualAccount(SmartCollectCreateCustomerRequest smartCollectCreateCustomerRequest)
        {
            var resp = new SmartCollectionVACResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Virtual account vpa can not be created"
            };

            string[] types = new string[] { "bank_account", "vpa", "qr_code" };
            var payload = new
            {
                receivers = new
                {
                    types,
                    vpa = new
                    {
                        descriptor = "F" + smartCollectCreateCustomerRequest.Contact.Substring(1, smartCollectCreateCustomerRequest.Contact.Length - 1)
                    }
                },
                description = "Fintech User " + smartCollectCreateCustomerRequest.Name,
                customer_id = smartCollectCreateCustomerRequest.CustomerID,
                //close_by= DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                notes = new
                {
                    project_name = "Fintech"
                }
            };
            string BasicAuthString = Convert.ToBase64String(Encoding.UTF8.GetBytes(apiSetting.key_id + ":" + apiSetting.key_secret));
            var header = new Dictionary<string, string>
            {
                { "Authorization",string.Format("Basic {0}",BasicAuthString)}
            };
            var Request = apiSetting.CreateVirtualAccountURL + JsonConvert.SerializeObject(header) + "|" + JsonConvert.SerializeObject(payload);
            var response = string.Empty;
            try
            {
                response = AppWebRequest.O.PostJsonDataUsingHWRTLS(apiSetting.CreateVirtualAccountURL, payload, header).Result;
                if (!string.IsNullOrEmpty(response))
                {
                    var apiResp = JsonConvert.DeserializeObject<RZRPayCreateVAccountResp>(response);
                    if (apiResp != null)
                    {
                        if (apiResp.status == "active")
                        {
                            if (!string.IsNullOrEmpty(apiResp.id))
                            {
                                resp.CustomerID = apiResp.customer_id;
                                resp.QRCodeID = apiResp.receivers[2].id;
                                resp.QRShortURL = apiResp.receivers[2].short_url;
                                resp.VPAId = apiResp.receivers[1].id;
                                resp.VPAAddress = apiResp.receivers[1].address;
                                resp.AccounID = apiResp.receivers[0].id;
                                resp.AccountNumber = apiResp.receivers[0].account_number;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response = "Exception:" + ex.Message + "|" + response;
            }
            new ProcLogAPICommonReqResp(_dal).Call(new LogAPICommonReqRespModel
            {
                _Request = Request,
                _Response = response,
                _ClassName = GetType().Name,
                _Method = "CreateVirtualAccount"
            });
            return resp;
        }
    }
}
