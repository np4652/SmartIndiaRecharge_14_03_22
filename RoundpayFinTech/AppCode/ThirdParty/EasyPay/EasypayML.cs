using Fintech.AppCode.DB;
using Fintech.AppCode.StaticModel;
using Fintech.AppCode.WebRequest;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Model.BBPS;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.ThirdParty.CyberPlate;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Validators;

namespace RoundpayFinTech.AppCode.ThirdParty.EasyPay
{
    public class EasypayML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration Configuration;
        private EasypaySetting appSetting;
        private readonly IDAL _dal;
        public EasypayML(IHttpContextAccessor accessor, IHostingEnvironment env, IDAL dal)
        {
            _accessor = accessor;
            _env = env;
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile((_env.IsProduction() ? "appsettings.json" : "appsettings.Development.json"));
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            appSetting = AppSetting();
            _dal = dal;
        }
        private EasypaySetting AppSetting()
        {
            var setting = new EasypaySetting();
            try
            {
                setting = new EasypaySetting
                {
                    BBPSBaseURL = Configuration["SERVICESETTING:EZYPAY:BBPS:BaseURL"],
                    BBPSID = Configuration["SERVICESETTING:EZYPAY:BBPS:BBPSID"],
                    AuthCode = Configuration["SERVICESETTING:EZYPAY:BBPS:AuthCode"],
                    AgentMobile = Configuration["SERVICESETTING:EZYPAY:BBPS:AgentMobile"],
                    CustomerMobile = Configuration["SERVICESETTING:EZYPAY:BBPS:CustomerMobile"],
                    Pincode = Configuration["SERVICESETTING:EZYPAY:BBPS:Pincode"],
                    Geocode = Configuration["SERVICESETTING:EZYPAY:BBPS:Geocode"]
                };
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "EasypaySetting:EZYPAY",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return setting;
        }
        public BBPSResponse FetchBill(BBPSLog bBPSLog)
        {
            var BillResponse = new BBPSResponse
            {
                IsEditable = false,
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.URLNOTFOUND,
                IsEnablePayment=false
            };
            var URLWithParam = new StringBuilder();
            URLWithParam.Append("msg=B06003~AuthCode~Requestid~Productid~BBPSid~AgentID~CustomerName~Panno~AadharNo~PostalCode~Location~Validator1~Validator2~Validator3~AgentMobile~Value1~Value2~Value3~Value4");
            URLWithParam.Replace("AuthCode", appSetting.AuthCode ?? string.Empty);
            URLWithParam.Replace("Requestid", "ABS"+bBPSLog.SessionNo ?? string.Empty);
            URLWithParam.Replace("Productid", bBPSLog.aPIDetail.APIOpCode ?? string.Empty);
            URLWithParam.Replace("BBPSid", appSetting.BBPSID ?? string.Empty);
            URLWithParam.Replace("AgentID", bBPSLog.UserID.ToString());
            URLWithParam.Replace("CustomerName", bBPSLog.CustomerName ?? string.Empty);
            URLWithParam.Replace("Panno", bBPSLog.PAN ?? string.Empty);
            URLWithParam.Replace("AadharNo", bBPSLog.AadharNo ?? string.Empty);
            URLWithParam.Replace("PostalCode", appSetting.Pincode ?? string.Empty);
            URLWithParam.Replace("Location", appSetting.Geocode ?? string.Empty);
            URLWithParam.Replace("Validator1", bBPSLog.AccountNumber ?? string.Empty);
            URLWithParam.Replace("Validator2", string.IsNullOrEmpty(bBPSLog.Optional3) ? "NA" : bBPSLog.Optional3);
            URLWithParam.Replace("Validator3", "NA");
            URLWithParam.Replace("AgentMobile", appSetting.AgentMobile ?? string.Empty);
            URLWithParam.Replace("Value1", bBPSLog.CircleCode ?? string.Empty);
            URLWithParam.Replace("Value2", "NA");
            URLWithParam.Replace("Value3", "NA");
            URLWithParam.Replace("Value4", "NA");
            bBPSLog.RequestURL = appSetting.BBPSBaseURL;
            bBPSLog.Request = appSetting.BBPSBaseURL + "?" + URLWithParam.ToString();
            var response = string.Empty;
            try
            {
                var APIResp = AppWebRequest.O.CallUsingHttpWebRequest_POST(appSetting.BBPSBaseURL, URLWithParam.ToString());
               // var APIResp = "B06004~ABSA1995470~133~Y~0~300514048~3005140483~W20210510130203166300514048~NA~NA~NA$20210412$20210511~0.00$320.00$634.00~N~MDROBIUL ALAM~1041362901~NA~NA";
               // var APIResp = "B06004~ABSA1993939~133~Y~0~512041288~9307602654~W20210510105345872512041288~NA~NA~NA$NA$20210412~0.00$0.00$2.00~N~ALPANA DHARA~195311190~NA~NA";
               
              // var APIResp = "B06004~ABSA1978197~133~Y~0~502365690~9307602654~W20210508150629759502365690~NA~NA~20210405$20210504$20210603~1004.00$2009.00$2994.00~N~TURAB SEIKH, S O  ABDUL SEIKH~1884714798~NA~NA";
                
                response = APIResp;
                if (!string.IsNullOrEmpty(APIResp))
                {
                    if (APIResp.Contains("~"))
                    {
                        //B06004~ST123456789~133~Y~0~512037116~9851937038~NA~404013652467~20201201~20201210$20210111$00000000~1383.00$2767.00$0.00$4083.00~N~SK.RAHAMAN~NA~NA
                        //MessageCode~ Request id~ Product id~Valid~ErrorMessage~Validator1~ Validator2~ Validator3~BillNumber ~BillDate~Billduedate~BillAmount~Partial Payment ~Value1~Value2~Value3


                        // B06004~ABSA1978197~133~Y~0~502365690~9307602654~W20210508150629759502365690~NA~NA~20210405$20210504$20210603~1004.00$2009.00$2994.00~N~TURAB SEIKH, S O  ABDUL SEIKH~1884714798~NA~NA 

                        //MessageCode~ Request id~ Product id~Valid~ErrorMessage~Validator1~ Validator2~ Validator3~BillNumber ~BillDate~Billduedate~BillAmount~Partial Payment ~Value1~Value2~Value3
                        var splitTiled = APIResp.Split('~');
                        if (splitTiled.Length > 2)
                        {
                            var ezypayResp = new EasypayResponse
                            {
                                MessageCode = splitTiled[0],
                                RequestID = splitTiled[1],
                                ProductID = splitTiled[2],
                                Valid = splitTiled[3],
                                ErrorMessage = splitTiled[4],
                                Validator1 = splitTiled[5],
                                Validator2 = splitTiled[6],
                                Validator3 = splitTiled[7],
                                BillNumber = splitTiled[8],
                                BillDate = splitTiled[9],
                                BillDueDate = splitTiled[10],
                                BillAmount = splitTiled[11],
                                BillPartial = splitTiled[12],
                                Value1 = splitTiled[13],
                                Value2 = splitTiled[14],
                                Value3 = splitTiled[15],
                                Value4 = splitTiled[16],



                            };
                            if (ezypayResp.Valid == "Y")
                            {
                                BillResponse.BillNumber = ezypayResp.BillNumber;
                                BillResponse.BillDate = ezypayResp.BillDate;
                                BillResponse.CustomerName = ezypayResp.Value1;
                                if (ezypayResp.BillPartial.Equals("Y"))
                                {
                                    BillResponse.IsEditable = true;
                                }
                                if (ezypayResp.BillAmount.Contains("$"))
                                {
                                    //0.00$320.00$634.00
                                    //if (ezypayResp.BillAmount.Contains("0.00$"))
                                    //{
                                    //    var d = ezypayResp.BillAmount.Split('$');
                                    //    StringBuilder sb = new StringBuilder();
                                        
                                    //    foreach (var s in d)
                                    //    {
                                    //        if (!s.Equals("0.00"))
                                    //            sb.Append(s.ToString() + "$");
                                    //    }

                                    //    ezypayResp.BillAmount = sb.ToString().Remove(sb.ToString().Length - 1, 1); ;// ezypayResp.BillAmount.Replace("0.00$", "");
                                    //}
                                    BillResponse.Statuscode = ErrorCodes.One;
                                    BillResponse.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                    var splitAmount = ezypayResp.BillAmount.Split('$');
                                    BillResponse.Amount = splitAmount[0];

                                   
                                    if (ezypayResp.BillDueDate.Contains("$"))
                                    {
                                        //if(ezypayResp.BillDueDate.Contains("NA$"))
                                        //{
                                        //    ezypayResp.BillDueDate = ezypayResp.BillDueDate.Replace("NA$", "");
                                        //}
                                        
                                        if(ezypayResp.BillDueDate.Contains("NA$"))
                                        {
                                            ezypayResp.BillDueDate = ezypayResp.BillDueDate.Replace("NA$", "00000000$");
                                        }
                                      
                                        
                                        var splitDate = ezypayResp.BillDueDate.Split('$');

                                        
                                        if (splitDate.Length > 0)
                                        {
                                            var month = string.Empty;
                                            if (BillResponse.BillDate != "NA")
                                            {
                                                BillResponse.BillDate = ConvertStringToDate(BillResponse.BillDate, 0, out month);
                                            }
                                            
                                            
                                             //BillResponse.DueDate = ConvertStringToDate(splitDate[0], 0, out month);
                                            
                                           
                                            BillResponse.BillDates = new List<BillDateAmount>();
                                            var preRemark = string.Empty;
                                            for (int i = 0; i < splitDate.Length; i++)
                                            {
                                                var objBDA = new BillDateAmount
                                                {
                                                    DueDate = splitDate[i].Equals("00000000") ? ConvertStringToDate(splitDate[i], 1, out month) : ConvertStringToDate(splitDate[i], 0, out month),
                                                    DateValue = (i + 1).ToString(),
                                                    Month = month,
                                                    Amount = splitAmount[i]
                                                };
                                                if (i > 1)
                                                {
                                                    if (Convert.ToDecimal(splitAmount[i]) == 0)
                                                    {
                                                        objBDA.DueDate = ConvertStringToDate(splitDate[0], 0, out month);
                                                        objBDA.Month = month;
                                                        objBDA.Amount = splitAmount[3];
                                                        objBDA.DueDate = "Full Bill Payment";
                                                        objBDA.Month = "Quarterly";
                                                        objBDA.IsFull = true;
                                                    }
                                                    else
                                                    {
                                                        objBDA.Amount = splitAmount[2];
                                                    }
                                                }
                                                preRemark = preRemark + objBDA.Month + " Bill + ";
                                                objBDA.Remark = preRemark.Substring(0, preRemark.Length - 2);
                                                if (objBDA.Month.Equals("Quarterly"))
                                                {
                                                    objBDA.Remark = "Full Bill Payment ";
                                                }
                                                BillResponse.BillDates.Add(objBDA);
                                            }
                                        }
                                    }
                                }
                                else if (!string.IsNullOrEmpty(ezypayResp.BillAmount))
                                {
                                    BillResponse.DueDate = ezypayResp.BillDueDate;
                                    BillResponse.Statuscode = ErrorCodes.One;
                                    BillResponse.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                                    var month = string.Empty;
                                    BillResponse.BillDates = new List<BillDateAmount> {
                                        new BillDateAmount
                                        {
                                            Amount=ezypayResp.BillAmount,
                                            DateValue="1",
                                            DueDate=ConvertStringToDate(ezypayResp.BillDueDate, 0, out month),
                                            Month=month,
                                            Remark=month+" Bill"
                                        }
                                    };
                                    BillResponse.Amount = ezypayResp.BillAmount;                                    
                                }
                            }
                            else
                            {
                                BillResponse.Msg = ezypayResp.ErrorMessage;
                                BillResponse.IsShowMsgOnly = true;
                                BillResponse.ErrorMsg = ezypayResp.ErrorMessage;
                            }
                        }
                        else
                        {
                            BillResponse.Msg = splitTiled[1];
                            BillResponse.IsShowMsgOnly = true;
                            BillResponse.ErrorMsg = splitTiled[1];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response = response + "||" + ex.Message;
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            bBPSLog.Response = response;
            if (Convert.ToDecimal(string.IsNullOrEmpty(BillResponse.Amount) ? "0" : BillResponse.Amount) > 0) {
                bBPSLog.helper.Status = RechargeRespType.SUCCESS;
            }
            return BillResponse;
        }
        public RechargeAPIHit MakePaymentURL(int TID, TransactionServiceReq TRequest, RechargeAPIHit rechargeAPI)
        {
            string CircleCode = string.Empty, APIOpCode = rechargeAPI.aPIDetail.APIOpCode;
            if (rechargeAPI.aPIDetail.APIOpCode.Contains('|'))
            {
                APIOpCode = rechargeAPI.aPIDetail.APIOpCode.Split('|')[0];
                CircleCode = rechargeAPI.aPIDetail.APIOpCode.Split('|')[1];
            }
            else
            {
                CircleCode = rechargeAPI.aPIDetail.RechType;
            }

            var URLWithParam = new StringBuilder(appSetting.BBPSBaseURL);
            URLWithParam.Append("?msg=B06005~AuthCode~Requestid~Productid~BBPSid~AgentID~CustomerName~Panno~AadharNo~PostalCode~Location~Validator1~Validator2~Validator3~CustomerMobile~BillAmount~BillDueDate~Value1~Value2~Value3~Value4");
            URLWithParam.Replace("AuthCode", appSetting.AuthCode ?? string.Empty);
            URLWithParam.Replace("Requestid", "ABS"+TID.ToString());
            URLWithParam.Replace("Productid", APIOpCode ?? string.Empty);
            URLWithParam.Replace("BBPSid", appSetting.BBPSID ?? string.Empty);
            URLWithParam.Replace("AgentID", TRequest.UserID.ToString());
            URLWithParam.Replace("CustomerName", TRequest.Optional4 ?? string.Empty);
            URLWithParam.Replace("Panno", TRequest.PAN ?? string.Empty);
            URLWithParam.Replace("AadharNo", TRequest.Aadhar ?? string.Empty);
            URLWithParam.Replace("PostalCode", appSetting.Pincode ?? string.Empty);
            URLWithParam.Replace("Location", appSetting.Geocode ?? string.Empty);
            URLWithParam.Replace("Validator1", TRequest.AccountNo ?? string.Empty);
          ///  URLWithParam.Replace("Validator2", TRequest.CustomerNumber ?? string.Empty);
            URLWithParam.Replace("Validator2", string.IsNullOrEmpty(TRequest.Optional3) ? "NA" :  TRequest.Optional3);
            URLWithParam.Replace("Validator3", "NA");
            URLWithParam.Replace("CustomerMobile", appSetting.AgentMobile ?? string.Empty);
            URLWithParam.Replace("BillAmount", TRequest.AmountR.ToString());
            URLWithParam.Replace("BillDueDate", Validate.O.IsDateIn_dd_MMM_yyyy_Format(TRequest.Optional1) ? Convert.ToDateTime(TRequest.Optional1).ToString("yyyyMMdd", CultureInfo.InvariantCulture) : string.Empty);
            URLWithParam.Replace("CustomerMobile", TRequest.CustomerNumber ?? string.Empty);
            URLWithParam.Replace("Value1", CircleCode ?? string.Empty);
            URLWithParam.Replace("Value2", string.IsNullOrEmpty(CircleCode) ? "NA" : TRequest.Optional2 ?? string.Empty);
            URLWithParam.Replace("Value3", "NA");
            URLWithParam.Replace("Value4", "NA");
            rechargeAPI.aPIDetail.URL = URLWithParam.ToString();
            return rechargeAPI;
        }
        public string DoPayment(RechargeAPIHit rechargeAPIHit)
        {
            try
            {
                var _URL = rechargeAPIHit.aPIDetail.URL.Split('?')[0];
                var _data = rechargeAPIHit.aPIDetail.URL.Split('?')[1];
                return AppWebRequest.O.CallUsingHttpWebRequest_POST(_URL, _data);
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);

                return ex.Message;
            }
        }
        private string ConvertStringToDate(string dt, int AddMonth, out string month)
        {
            month = "";
            //20201210
            if (string.IsNullOrEmpty(dt))
                return string.Empty;
            var Y = dt.Substring(0, 4);
            var M = dt.Substring(4, 2);
            var D = dt.Substring(6, 2);
            M = Validators.Validate.O.Months[Convert.ToInt32(M) - 1 + AddMonth];
            month = M;
            return string.Format("{0} {1} {2}", D, M, Y);
        }
    }
}
