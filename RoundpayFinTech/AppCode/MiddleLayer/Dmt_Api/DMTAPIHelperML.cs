using Fintech.AppCode.DB;
using Fintech.AppCode.StaticModel;
using Fintech.AppCode.WebRequest;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.MoneyTransfer;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.ThirdParty.OpenBank;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.MiddleLayer.Dmt_Api
{
    public static class DMTAPIHelperML
    {
        public static DMRTransactionResponse AccountVerification(string VerificationURL, string Authorization, string AccountNo, string IFSC, string TransactionID, IDAL _dal)
        {
            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = nameof(ErrorCodes.Request_Accpeted).Replace("_", " "),
                ErrorCode = ErrorCodes.Request_Accpeted
            };
            try
            {
                var param = new
                {
                    number = AccountNo,
                    ifsc = IFSC,
                    reference_number = TransactionID
                };
                res.Request = VerificationURL + "&RequestJson=" + JsonConvert.SerializeObject(param); 
                res.Response = AppWebRequest.O.PostJsonDataUsingHWRTLS(VerificationURL, param, new Dictionary<string, string>
                {
                        { "Authorization",Authorization},
                        { ContentType.Self,ContentType.application_json}
                }).Result;
                var _apiRes = JsonConvert.DeserializeObject<OBVerifiyResponse>(res.Response);
                if (_apiRes != null)
                {
                    if (_apiRes.success)
                    {
                        if (_apiRes.data != null)
                        {
                            res.VendorID = _apiRes.data.reference_number;
                            if (_apiRes.data.status.Equals("COMPLETED"))
                            {
                                res.Statuscode = RechargeRespType.SUCCESS;
                                res.Msg = _apiRes.message;
                                res.BeneName = _apiRes.data.verify_account_holder ?? string.Empty;
                                res.LiveID = _apiRes.data.bank_ref_num.ToString();
                                res.ErrorCode = ErrorCodes.Transaction_Successful;
                            }
                            else if (_apiRes.data.status.Equals("FAILED"))
                            {
                                res.Statuscode = RechargeRespType.FAILED;
                                res.Msg = _apiRes.data.verify_reason ?? string.Empty;
                                if (res.Msg.Contains("suff"))
                                {
                                    res.ErrorCode = DMTErrorCodes.Declined_by_ServiceProvider;
                                    res.Msg = nameof(DMTErrorCodes.Declined_by_ServiceProvider).Replace("_", " ");
                                }
                                else
                                {
                                    res.Msg = string.Format("{0}", res.Msg);
                                    res.ErrorCode = 158;
                                }
                                res.LiveID = res.Msg;
                            }
                            else
                            {
                                res.Statuscode = RechargeRespType.PENDING;
                                res.Msg = nameof(ErrorCodes.Request_Accpeted).Replace("_", " ");
                                res.ErrorCode = ErrorCodes.Request_Accpeted;
                                res.LiveID = res.Msg;
                            }
                        }
                        else
                        {
                            res.Statuscode = RechargeRespType.PENDING;
                            res.Msg = _apiRes.message;
                        }
                    }
                    else
                    {
                        res.Statuscode = RechargeRespType.FAILED;
                        res.Msg = _apiRes.message;
                        res.LiveID = _apiRes.message;
                        if (res.LiveID.ToLower().Contains("nsuffici"))
                        {
                            res.ErrorCode = DMTErrorCodes.Declined_by_ServiceProvider;
                            res.Msg = nameof(DMTErrorCodes.Declined_by_ServiceProvider).Replace("_", " ");
                            res.LiveID = res.Msg;
                        }
                        else
                        {
                            res.Msg = string.Format("{0}", res.Msg);
                            res.ErrorCode = 158;
                            res.LiveID = res.Msg;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                res.Response = ex.Message + "|" + res.Response;
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = "DMTAPIHelperML",
                    FuncName = "AccountVerification",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = 0
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }
        public static MBeneficiaryResp GetBeneficiary(MTAPIRequest request, IDAL _dal, string ClsName)
        {
            var res = new MBeneficiaryResp
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = nameof(DMTErrorCodes.Beneficiary_not_found).Replace("_", " "),
                ErrorCode = DMTErrorCodes.Beneficiary_not_found
            };
            try
            {
                var resDB = (new ProcGetBenificiary(_dal).Call(new DMTReq { SenderNO = request.SenderMobile })) as BenificiaryModel;
                res.Statuscode = resDB.Statuscode;
                res.Msg = resDB.Msg;
                if (resDB != null && resDB.Statuscode == ErrorCodes.One)
                {
                    var Beneficiaries = new List<MBeneDetail>();
                    if (resDB.benificiaries != null && resDB.benificiaries.Count > 0)
                    {
                        foreach (var r in resDB.benificiaries)
                        {
                            Beneficiaries.Add(new MBeneDetail
                            {
                                AccountNo = r._AccountNumber,
                                BankName = r._BankName,
                                IFSC = r._IFSC,
                                BeneName = r._Name,
                                MobileNo = r._MobileNo,
                                BeneID = r._ID.ToString(),
                                BankID = r._BankID
                            });
                        }
                    }
                    res.Beneficiaries = Beneficiaries;
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = ClsName,
                    FuncName = "GetBeneficiary",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = request.UserID
                });
            }
            return res;
        }
    }
}
