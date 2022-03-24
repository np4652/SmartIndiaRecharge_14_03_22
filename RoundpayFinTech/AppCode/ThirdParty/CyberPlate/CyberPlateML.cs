using System;
using System.Text;
using CyberPlatOpenSSL;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Model.BBPS;
using Fintech.AppCode.DB;
using System.Web;
using Validators;
using RoundpayFinTech.AppCode.Model.ProcModel;

namespace RoundpayFinTech.AppCode.ThirdParty.CyberPlate
{
    public partial class CyberPlateML
    {
        private readonly IDAL _dal;
        private const string Replacing = "pay.cgi";
        private const string ReplaceByVerification = "pay_check.cgi";
        private const string ReplaceByStatusCheck = "pay_status.cgi";
        private const string SDCode = "";
        private const string APCode = "";
        private const string OPCode = "";
        private const string CyberPlatPWD = "";
        private const string CERTNO = "";
        public CyberPlateML(IDAL dal) => _dal = dal;
        public string GetValidationURL(StringBuilder sbPaymentURL)
        {
            if (sbPaymentURL != null && sbPaymentURL.ToString().Contains(Replacing))
            {
                sbPaymentURL.Replace(Replacing, ReplaceByVerification);
            }
            return sbPaymentURL.ToString();
        }

        public string GetStatusURL(StringBuilder sbPaymentURL)
        {
            if (sbPaymentURL != null && sbPaymentURL.ToString().Contains(Replacing))
            {
                sbPaymentURL.Replace(Replacing, ReplaceByStatusCheck);
            }
            return sbPaymentURL.ToString();
        }
        public string _strRequest(string SessionNo, string txtMobileNo, string account, string authenticator3, string amount)
        {
            StringBuilder _reqStr = new StringBuilder();
            #region Create Request
            _reqStr.Append("CERT=" + CERTNO + Environment.NewLine);
            _reqStr.Append("SD=" + SDCode + Environment.NewLine);
            _reqStr.Append("AP=" + APCode + Environment.NewLine);
            _reqStr.Append("OP=" + OPCode + Environment.NewLine);
            _reqStr.Append("SESSION=" + SessionNo + Environment.NewLine);
            _reqStr.Append("NUMBER=" + txtMobileNo + Environment.NewLine);
            _reqStr.Append("ACCOUNT=" + account + Environment.NewLine);
            _reqStr.Append("Authenticator3=" + authenticator3 + Environment.NewLine);
            _reqStr.Append("AMOUNT=" + amount + Environment.NewLine);
            _reqStr.Append("TERM_ID=" + APCode + Environment.NewLine);//Mostly value of TERM_ID=AP Code, but value may be vary.
            _reqStr.Append("COMMENT=test" + txtMobileNo);
            #endregion
            return _reqStr.ToString();
        }
        public string CyberPlatVerification(BBPSLog bBPSLog)
        {
            var objssl = new OpenSSL();
            try
            {
                MatchOptionalParam(bBPSLog);
                string validationURL = GetValidationURL(new StringBuilder(bBPSLog.RequestURL));
                string strRequest = _strRequest(bBPSLog.SessionNo, bBPSLog.BillNumber, bBPSLog.AccountNumber ?? "", bBPSLog.Authenticator3 ?? "", bBPSLog.Amount.ToString());
                objssl.message = objssl.Sign_With_PFX(strRequest, DOCType.CyberPlatFilePath, CyberPlatPWD);
                objssl.htmlText = objssl.CallCryptoAPI(objssl.message, validationURL);
                bBPSLog.RequestURL = validationURL;
                bBPSLog.Request = strRequest;
                bBPSLog.Response = objssl.htmlText;
                new ProcLogFetchBill(_dal).Call(bBPSLog);
            }
            catch (Exception er)
            {
            }
            return objssl.htmlText;
        }
        public CyberPlateBBPSResponse FetchBill(BBPSLog bBPSLog)
        {
            var cyberPlatResponse = new CyberPlateBBPSResponse
            {
                IsEditable = true,
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.URLNOTFOUND
            };
            try
            {
                cyberPlatResponse = GetResponseFromString(CyberPlatVerification(bBPSLog));
                if ((cyberPlatResponse.PRICE ?? string.Empty).Trim() == cyberPlatResponse.ERROR.Trim())
                {
                    cyberPlatResponse.IsEditable = true;
                    cyberPlatResponse.IsEnablePayment = true;
                }
                else
                {

                    cyberPlatResponse.PRICE = (cyberPlatResponse.PRICE ?? "").Trim() == string.Empty ? "0" : (Validate.O.IsNumeric((cyberPlatResponse.PRICE ?? "").Replace(".", "")) ? cyberPlatResponse.PRICE : "0");
                    if (Convert.ToInt32(cyberPlatResponse.ERROR) == 23 || Convert.ToInt32(cyberPlatResponse.ERROR) == 270)
                    {
                        cyberPlatResponse.IsEditable = false;
                        cyberPlatResponse.IsEnablePayment = false;
                        cyberPlatResponse.Statuscode = ErrorCodes.Minus1;
                        cyberPlatResponse.Msg = cyberPlatResponse.ERRMSG;
                        cyberPlatResponse.IsShowMsgOnly = true;
                    }
                    else if (Convert.ToDecimal(cyberPlatResponse.PRICE) > 0)
                    {
                        cyberPlatResponse.IsEditable = false;
                        cyberPlatResponse.IsEnablePayment = true;
                        cyberPlatResponse.Statuscode = ErrorCodes.One;
                        cyberPlatResponse.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                    }
                    else if (Convert.ToInt32(cyberPlatResponse.ERROR) == 7 && Convert.ToInt32(cyberPlatResponse.RESULT) == 1)
                    {
                        cyberPlatResponse.IsEditable = false;
                        cyberPlatResponse.IsEnablePayment = false;
                        cyberPlatResponse.Statuscode = ErrorCodes.Minus1;
                        cyberPlatResponse.IsShowMsgOnly = true;
                        cyberPlatResponse.Msg = nameof(ErrorCodes.No_Payment_Due).Replace("_", " ");
                        cyberPlatResponse.ErrorCode = ErrorCodes.No_Payment_Due.ToString();
                    }
                }
            }
            catch (Exception er) { }
            return cyberPlatResponse;
        }

        public RechargeAPIHit CyberPlatStatusCheck(RechargeAPIHit rechargeAPIHit)
        {
            var objssl = new OpenSSL();
            try
            {
                string statusURL = GetStatusURL(new StringBuilder(rechargeAPIHit.aPIDetail.URL));
                string strRequest = _strRequest(rechargeAPIHit.CPTRNXRequest.SESSION, rechargeAPIHit.CPTRNXRequest.NUMBER, rechargeAPIHit.CPTRNXRequest.ACCOUNT, rechargeAPIHit.CPTRNXRequest.Authenticator3, rechargeAPIHit.CPTRNXRequest.AMOUNT);
                objssl.message = objssl.Sign_With_PFX(strRequest, DOCType.CyberPlatFilePath, CyberPlatPWD);
                objssl.htmlText = objssl.CallCryptoAPI(objssl.message, statusURL);
                rechargeAPIHit.aPIDetail.URL = statusURL + "?" + objssl.message;
                rechargeAPIHit.Response = objssl.htmlText;
                var bBPSLog = new BBPSLog
                {
                    RequestURL = statusURL,
                    Request = strRequest,
                    Response = objssl.message + " | " + objssl.htmlText
                };
                new ProcLogFetchBill(_dal).Call(bBPSLog);
            }
            catch (Exception er)
            {
            }
            return rechargeAPIHit;
        }

        public CyberPlateBBPSResponse GetResponseFromString(string res)
        {
            var resmodel = new CyberPlateBBPSResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.URLNOTFOUND
            };
            try
            {
                if (res != null)
                {
                    res = res.Replace("\r", "").Replace("\n", " ");
                    /*getting error message*/
                    var ind = res.IndexOf("ERRMSG=");
                    if (ind > -1)
                    {
                        int start = res.IndexOf("ERRMSG=") + "ERRMSG=".Length;
                        int end = res.LastIndexOf("END BEGIN");
                        resmodel.ERRMSG = res.Substring(start, end - start);
                    }
                    else
                    {
                        resmodel.ERRMSG = ErrorCodes.URLNOTFOUND;
                    }

                    /*getting error message*/
                    if (res.Contains(" "))
                    {
                        string[] result = res.Split(' ');
                        for (int i = 0; i < result.Length; i++)
                        {
                            string[] finalres = result[i].Split('=');
                            if (finalres.Length == 2)
                            {
                                switch (finalres[0])
                                {
                                    case "DATE":
                                        resmodel.DATE = finalres[1];
                                        break;
                                    case "SESSION":
                                        resmodel.SESSION = finalres[1];
                                        break;
                                    case "ERROR":
                                        resmodel.ERROR = finalres[1];
                                        break;
                                    case "RESULT":
                                        resmodel.RESULT = finalres[1];
                                        break;
                                    case "TRANSID":
                                        resmodel.TRANSID = finalres[1];
                                        break;
                                    case "AUTHCODE":
                                        resmodel.AUTHCODE = finalres[1];
                                        break;
                                    case "TRNXSTATUS":
                                        resmodel.TRNXSTATUS = finalres[1];
                                        break;
                                    case "PRICE":
                                        resmodel.PRICE = finalres[1].Trim();
                                        resmodel.Amount = finalres[1].Trim();
                                        break;
                                    case "ERRORMSG":
                                        resmodel.Msg = finalres[1];
                                        break;
                                    case "ADDINFO":
                                        StringBuilder sb = new StringBuilder(HttpUtility.UrlDecode(finalres[1]));
                                        sb.Replace("<", string.Empty);
                                        string[] arr = sb.ToString().Split('>');
                                        resmodel.BillNumber = arr[0].Trim();
                                        resmodel.BillDate = MakeDate(arr[1]);
                                        resmodel.DueDate = MakeDate(arr[2]);
                                        resmodel.Amount = arr[3].Trim();
                                        resmodel.CustomerName = arr[4].Trim();
                                        resmodel.PRICE = arr[3].Trim();
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception er)
            {
            }
            return resmodel;
        }

        #region Match Optionl Params Of API
        public void MatchOptionalParam(BBPSLog bBPSLog)
        {
            bBPSLog.AccountNumber = GetValueofKey("ACCOUNT", bBPSLog.APIOptionalList, bBPSLog.Optional1, bBPSLog.Optional2, bBPSLog.Optional3, bBPSLog.Optional4);
            bBPSLog.Authenticator3 = GetValueofKey("Authenticator3", bBPSLog.APIOptionalList, bBPSLog.Optional1, bBPSLog.Optional2, bBPSLog.Optional3, bBPSLog.Optional4);
        }

        private string GetValueofKey(string key, ApiOperatorOptionalMappingModel model, string o1, string o2, string o3, string o4)
        {
            if (key == model._Key1)
            {
                return model._Value1.Replace(Replacement.OPTIONAL1, o1);
            }
            if (key == model._Key2)
            {
                return model._Value2.Replace(Replacement.OPTIONAL2, o2);
            }
            if (key == model._Key3)
            {
                return model._Value3.Replace(Replacement.OPTIONAL3, o3);
            }
            if (key == model._Key4)
            {
                return model._Value4.Replace(Replacement.OPTIONAL4, o4);
            }
            return string.Empty;
        }
        #endregion
        private string MakeDate(string dtstr)
        {
            if (Validate.O.IsNumeric(dtstr.Trim()))
            {
                StringBuilder sb = new StringBuilder(dtstr.Trim().Substring(6, 2));
                sb.Append("/");
                sb.Append(dtstr.Trim().Substring(4, 2));
                sb.Append("/");
                sb.Append(dtstr.Trim().Substring(0, 4));
                return sb.ToString();
            }
            return dtstr;
        }
        #region CyberPlat Transaction
        public RechargeAPIHit CyberPlatPayment(RechargeAPIHit rechargeAPIHit)
        {
            OpenSSL openSSL = new OpenSSL();
            string strRequest = _strRequest(rechargeAPIHit.CPTRNXRequest.SESSION, rechargeAPIHit.CPTRNXRequest.NUMBER, rechargeAPIHit.CPTRNXRequest.ACCOUNT, rechargeAPIHit.CPTRNXRequest.Authenticator3, rechargeAPIHit.CPTRNXRequest.AMOUNT);
            openSSL.message = openSSL.Sign_With_PFX(strRequest, DOCType.CyberPlatFilePath, CyberPlatPWD);
            openSSL.htmlText = openSSL.CallCryptoAPI(openSSL.message, rechargeAPIHit.aPIDetail.URL);
            rechargeAPIHit.aPIDetail.URL = rechargeAPIHit.aPIDetail.URL + "?" + openSSL.message;
            rechargeAPIHit.Response = openSSL.htmlText;
            return rechargeAPIHit;
        }

        public RechargeAPIHit CyberPlatVerification(RechargeAPIHit rechargeAPIHit)
        {
            var objssl = new OpenSSL();
            try
            {
                string validationURL = GetValidationURL(new StringBuilder(rechargeAPIHit.aPIDetail.URL));
                string strRequest = _strRequest(rechargeAPIHit.CPTRNXRequest.SESSION, rechargeAPIHit.CPTRNXRequest.NUMBER, rechargeAPIHit.CPTRNXRequest.ACCOUNT, rechargeAPIHit.CPTRNXRequest.Authenticator3, rechargeAPIHit.CPTRNXRequest.AMOUNT);
                objssl.message = objssl.Sign_With_PFX(strRequest, DOCType.CyberPlatFilePath, CyberPlatPWD);
                objssl.htmlText = objssl.CallCryptoAPI(objssl.message, validationURL);
                rechargeAPIHit.aPIDetail.URL = validationURL + "?" + objssl.message;
                rechargeAPIHit.Response = objssl.htmlText;
                var bBPSLog = new BBPSLog();
                bBPSLog.RequestURL = validationURL;
                bBPSLog.Request = strRequest;
                bBPSLog.Response = objssl.message + " | " + objssl.htmlText;
                new ProcLogFetchBill(_dal).Call(bBPSLog);
            }
            catch (Exception er)
            {
            }
            return rechargeAPIHit;
        }
        #endregion
    }
}
