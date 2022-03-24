using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.ThirdParty.Razorpay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public class SmartCollectML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        public SmartCollectML(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
        }
        public UserSmartDetailModel GetUserSmartDetails(int LoginID, int UserID)
        {
            IProcedure proc = new ProcGetUserSmartCollectDetail(_dal);
            return (UserSmartDetailModel)proc.Call(new CommonReq
            {
                LoginID = LoginID,
                UserID = UserID
            });
        }
        public ResponseStatus UpdateSmartCollectDetailOfUser(int LoginID, int UserID)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            IProcedure proc = new ProcGetUserSmartCollectDetail(_dal);
            var smartDetailList = (UserSmartDetailModel)proc.Call(new CommonReq
            {
                LoginID = LoginID,
                UserID = UserID
            });
            if (smartDetailList != null)
            {
                if (smartDetailList.USDList.Count > 0)
                {
                    var userSmartDetail_Razorpay = new RoundpayFinTech.AppCode.Model.UserSmartDetail();
                    if (smartDetailList.USDList.Where(w => w.SmartCollectTypeID == SmartCollectType.RazorPaySmartCollect).Count() > 0)
                    {
                        userSmartDetail_Razorpay = smartDetailList.USDList.Where(w => w.SmartCollectTypeID == SmartCollectType.RazorPaySmartCollect).ToList()[0];
                        if (userSmartDetail_Razorpay.SmartCollectTypeID > 0)
                        {
                            RazorpaySmartCollectML RZRPayObj = new RazorpaySmartCollectML(_accessor, _env, _dal);
                            var createCustomerResp = RZRPayObj.CreateCustomer(new SmartCollectCreateCustomerRequest
                            {
                                Name = smartDetailList.Name,
                                EmailID = smartDetailList.EmailID,
                                Contact = smartDetailList.MobileNo,
                                GSTIN = smartDetailList.GSTIN,
                                NotesKey1 = "Customer Registration " + smartDetailList.MobileNo,
                                NotesKey2 = "Customer Registration " + smartDetailList.EmailID
                            });
                            if (createCustomerResp.Statuscode == ErrorCodes.One)
                            {
                                var virtualAcResp = RZRPayObj.CreateVirtualAccount(new SmartCollectCreateCustomerRequest
                                {
                                    CustomerID = createCustomerResp.CustomerID,
                                    Contact = smartDetailList.MobileNo,
                                    Name = smartDetailList.Name
                                });
                                IProcedure procUpdate = new ProcUpdateCustomerSmartAccountDetail(_dal);
                                res = (ResponseStatus)procUpdate.Call(new UpdateSmartCollectRequestModel
                                {
                                    CustomerID= createCustomerResp.CustomerID,
                                    LoginID=LoginID,
                                    UserID=UserID,
                                    SmartAccountNo= virtualAcResp.AccountNumber,
                                    SmartCollectTypeID= SmartCollectType.RazorPaySmartCollect,
                                    SmartQRShortURL=virtualAcResp.QRShortURL,
                                    SmartVPA=virtualAcResp.VPAAddress
                                });
                            }
                        }
                    }
                }
            }
            return res;
        }
    }
}
