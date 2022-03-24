using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.DL;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.App;
using RoundpayFinTech.AppCode.Model.MoneyTransfer;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Validators;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public class SellerML : ISellerML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly ISession _session;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        private readonly LoginResponse _lr;
        private readonly IRequestInfo _info;
        public SellerML(IHttpContextAccessor accessor, IHostingEnvironment env, bool InSession = true)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            if (InSession)
            {
                _session = _accessor.HttpContext.Session;
                _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
                //userML = new UserML(_lr);
            }
            _dal = new DAL(_c.GetConnectionString());
            bool IsProd = _env.IsProduction();
            _info = new RequestInfo(_accessor, _env);
        }
        public List<PieChartList> DashBoard()
        {
            IProcedure f = new ProcUserDashBoard(_dal);
            return (List<PieChartList>)f.Call(_lr.UserID);
        }
        public List<Package_Cl> GetPackage()
        {
            IProcedure proc = new ProcAssignService(_dal);
            return (List<Package_Cl>)proc.Call(_lr.UserID);
        }
        public IEnumerable<Package_Cl> GetPackage(int UserID)
        {
            IProcedure proc = new ProcAssignService(_dal);
            return (List<Package_Cl>)proc.Call(UserID);
        }
        public async Task<IResponseStatus> Recharge(RechargeRequest req)
        {
            IResponseStatus resp = new ResponseStatus
            {
                Statuscode = 1,
                Msg = "PENDING"
            };
            try
            {
                var _req = new _RechargeAPIRequest
                {
                    UserID = _lr.UserID,
                    Account = req.Mob,
                    Amount = req.Amt,
                    APIRequestID = "",
                    OID = req.OID,
                    Optional1 = req.O1,
                    Optional2 = req.O2,
                    Optional3 = req.O3,
                    Optional4 = req.O4,
                    OutletID = _lr.OutletID,
                    RefID = req.ReferenceID ?? string.Empty,
                    RequestMode = RequestMode.PANEL,
                    IPAddress = _info.GetRemoteIP(),
                    CustomerNumber = req.CustNo,
                    GEOCode = req.lat + "," + req.lng,
                    Pincode = _lr.Pincode,
                    SecurityKey = req.SecKey ?? string.Empty,
                    FetchBillID = req.FetchBillID,
                    CCFAmount = req.CCFAmount,
                    PaymentMode = req.PaymentMode
                };
                var transactionML = new TransactionML(_accessor, _env, _dal, _info);
                var TResp = await transactionML.DoTransaction(_req).ConfigureAwait(false);
                resp.Statuscode = TResp.STATUS;
                resp.Msg = TResp.MSG;
                resp.CommonStr = TResp.RPID;
                resp.CommonStr2 = TResp.OPID;
                resp.CommonStr3 = DateTime.Now.ToString("dd MMM yyyy hh:mm:ss tt");
                resp.CommonStr4 = TResp.VendorID;
            }
            catch { }
            return resp;
        }
        public async Task<AppTransactionRes> AppRecharge(AppRechargeRequest req)
        {
            var resp = new AppTransactionRes
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = RechargeRespType._PENDING
            };
            try
            {
                var _req = new _RechargeAPIRequest
                {
                    UserID = req.UserID,
                    Account = req.Mob,
                    Amount = req.Amt,
                    APIRequestID = "",
                    OID = req.OID,
                    Optional1 = req.O1,
                    Optional2 = req.O2,
                    Optional3 = req.O3,
                    Optional4 = req.O4,
                    OutletID = req.OutletID,
                    GEOCode = req.GEOCode ?? string.Empty,
                    IMEI = req.IMEI ?? string.Empty,
                    CustomerNumber = req.CustNo ?? string.Empty,
                    RefID = req.ReferenceID ?? string.Empty,
                    Pincode = req.Pincode,
                    IPAddress = _info.GetRemoteIP(),
                    SecurityKey = req.SecKey ?? "",
                    RequestMode = req.RequestMode,
                    IsReal = req.IsReal,
                    PromoCodeID = req.PromoCodeID,
                    FetchBillID = req.FetchBillID
                };
                var transactionML = new TransactionML(_accessor, _env, _dal, _info);
                var TResp = await transactionML.DoTransaction(_req).ConfigureAwait(false);
                resp.Statuscode = TResp.STATUS;
                resp.Msg = TResp.MSG;
                resp.TransactionID = TResp.RPID;
                resp.LiveID = TResp.STATUS == RechargeRespType.FAILED ? TResp.MSG : TResp.OPID;
                if (TResp.ERRORCODE.Equals("150"))//Repeat request found error!
                {
                    resp.Statuscode = RechargeRespType.PENDING;
                }
            }
            catch { }
            return resp;
        }
        #region DMR
        public async Task<IResponseStatus> CheckSender(string MobileNo)
        {
            var req = new DMTReq
            {
                RequestMode = RequestMode.PANEL,
                SenderNO = MobileNo ?? "",
                LT = _lr.LoginTypeID,
                UserID = _lr.UserID,
                OutletID = _lr.OutletID,
                IsValidate = false
            };
            var dmt = new DmtML(_accessor, _env);
            return await dmt.CheckSender(req).ConfigureAwait(false);
        }
        public async Task<IResponseStatus> DoKYCSender(SenderRequest senderKYC)
        {
            var req = new DMTReq
            {
                RequestMode = RequestMode.PANEL,
                SenderNO = senderKYC.MobileNo,
                LT = _lr.LoginTypeID,
                UserID = _lr.UserID,
                OutletID = _lr.OutletID,
                IsValidate = false
            };
            var sen = new CreateSen
            {
                dMTReq = req,
                senderRequest = senderKYC
            };
            var dmt = new DmtML(_accessor, _env);
            return await dmt.DoSenderKYC(sen).ConfigureAwait(false);
        }
        public async Task<IResponseStatus> CreateSender(SenderRequest _sreq)
        {
            var req = new DMTReq
            {
                RequestMode = RequestMode.PANEL,
                SenderNO = _sreq.MobileNo,
                LT = _lr.LoginTypeID,
                UserID = _lr.UserID,
                OutletID = _lr.OutletID,
                IsValidate = false
            };
            var sen = new CreateSen
            {
                dMTReq = req,
                senderRequest = _sreq
            };
            var dmt = new DmtML(_accessor, _env);
            return await dmt.CreateSender(sen).ConfigureAwait(false);
        }
        public async Task<IResponseStatus> VerifySender(string MobileNo, string OTP, string RefID)
        {
            var req = new DMTReq
            {
                RequestMode = RequestMode.PANEL,
                SenderNO = MobileNo,
                LT = _lr.LoginTypeID,
                UserID = _lr.UserID,
                OutletID = _lr.OutletID,
                IsValidate = false,
                ReffID = RefID
            };
            var dmt = new DmtML(_accessor, _env);
            return await dmt.VerifySender(req, OTP);
        }
        public async Task<IResponseStatus> SenderResendOTP(string MobileNo, string SID)
        {
            var req = new DMTReq
            {
                RequestMode = RequestMode.PANEL,
                SenderNO = MobileNo,
                LT = _lr.LoginTypeID,
                UserID = _lr.UserID,
                OutletID = _lr.OutletID,
                IsValidate = false,
                ReffID = SID
            };
            var dmt = new DmtML(_accessor, _env);
            return await dmt.SenderResendOTP(req);
        }
        public async Task<IResponseStatus> AddBeni(AddBeni addBeni)
        {
            var req = new DMTReq
            {
                RequestMode = RequestMode.PANEL,
                SenderNO = addBeni.SenderMobileNo,
                LT = _lr.LoginTypeID,
                UserID = _lr.UserID,
                OutletID = _lr.OutletID,
                IsValidate = false,
                ReffID = addBeni.SID
            };
            var dmt = new DmtML(_accessor, _env);
            return await dmt.CreateBeneficiary(addBeni, req);
        }
        public async Task<BeniRespones> GetBeneficiary(string MobileNo, string SID)
        {
            var req = new DMTReq
            {
                RequestMode = RequestMode.PANEL,
                SenderNO = MobileNo,
                LT = _lr.LoginTypeID,
                UserID = _lr.UserID,
                OutletID = _lr.OutletID,
                IsValidate = false,
                ReffID = SID
            };
            var dmt = new DmtML(_accessor, _env);
            return await dmt.GetBeneficiary(req);
        }
        public async Task<DMRTransactionResponse> SendMoney(ReqSendMoney reqSendMoney)
        {
            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = RechargeRespType._PENDING
            };
            var req = new DMTReq
            {
                RequestMode = RequestMode.PANEL,
                SenderNO = reqSendMoney.MobileNo,
                LT = _lr.LoginTypeID,
                UserID = _lr.UserID,
                OutletID = _lr.OutletID,
                IsValidate = false
            };

            var dmt = new DmtML(_accessor, _env);
            var resp = await dmt.SendMoney(req, reqSendMoney).ConfigureAwait(false);
            if (resp.ErrorCode == 150)
            {
                resp.Statuscode = 1;
                resp.Msg = "Request already in progress";
            }
            resp.Status = RechargeRespType.GetRechargeStatusText(resp.Statuscode);
            return resp;
        }
        public async Task<IResponseStatus> DeleteBene(string MobileNo, string BeneID, string SID, string OTP)
        {
            var req = new DMTReq
            {
                RequestMode = RequestMode.PANEL,
                SenderNO = MobileNo,
                LT = _lr.LoginTypeID,
                UserID = _lr.UserID,
                OutletID = _lr.OutletID,
                IsValidate = false,
                ReffID = SID
            };
            var dmt = new DmtML(_accessor, _env);
            return await dmt.DeleteBeneficiary(req, BeneID, OTP);
        }
        public async Task<IResponseStatus> GenerateOTP(string MobileNo)
        {
            var req = new DMTReq
            {
                RequestMode = RequestMode.PANEL,
                SenderNO = MobileNo,
                LT = _lr.LoginTypeID,
                UserID = _lr.UserID,
                OutletID = _lr.OutletID
            };
            var dmt = new DmtML(_accessor, _env);
            return await dmt.GenerateOTP(req);
        }
        public async Task<IResponseStatus> ValidateBeneficiaryOTP(string MobileNo, string BeneMobile, string AccountNo, string OTP)
        {
            var req = new DMTReq
            {
                RequestMode = RequestMode.PANEL,
                SenderNO = MobileNo,
                LT = _lr.LoginTypeID,
                UserID = _lr.UserID,
                OutletID = _lr.OutletID
            };
            var dmt = new DmtML(_accessor, _env);
            return await dmt.ValidateBeneficiary(req, BeneMobile, AccountNo, OTP);
        }
        public async Task<DMRTransactionResponse> DMTVerification(string MobileNo, string AccountNo, string IFSC, string Bank, int BankID, string BeneName)
        {
            var res = new DMRTransactionResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = RechargeRespType._PENDING
            };
            var req = new DMTReq
            {
                RequestMode = RequestMode.PANEL,
                SenderNO = MobileNo,
                LT = _lr.LoginTypeID,
                UserID = _lr.UserID,
                OutletID = _lr.OutletID,
                IsValidate = false
            };
            var reqSendMoney = new ReqSendMoney
            {
                AccountNo = AccountNo,
                Amount = 0,
                BeneID = "",
                Channel = false,
                IFSC = IFSC,
                MobileNo = MobileNo,
                Bank = Bank,
                BankID = BankID,
                BeneName = BeneName
            };
            var dmt = new DmtML(_accessor, _env);
            var resp = await dmt.Verification(req, reqSendMoney);
            resp.Status = RechargeRespType.GetRechargeStatusText(resp.Statuscode);
            return resp;
        }
        public ChargeAmount GetCharge(decimal amt)
        {
            var _res = new ChargeAmount
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            if (amt < 1)
            {
                _res.Statuscode = ErrorCodes.Minus1;
                _res.Msg = "Fill Amount";
                return _res;
            }
            var _req = new CommonReq
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID,
                CommonDecimal = amt
            };
            var Helper = ConverterHelper.O.SplitAmounts(Convert.ToInt32(amt), 0, 0);
            IProcedure procDMR = new ProcGetDMRCharge(_dal);
            var CalculateAmount = 0M;
            foreach (var item in Helper)
            {
                _req.CommonDecimal = item.Amount;
                var procRes = (ChargeAmount)procDMR.Call(_req);
                _res = procRes;
                if (_res.Statuscode == ErrorCodes.Minus1)
                {
                    break;
                }
                CalculateAmount += procRes.Charged;
            }
            if (_res.Statuscode == ErrorCodes.Minus1)
            {
                _res.Charged = 0;
            }
            else
            {
                _res.Charged = CalculateAmount;
            }
            return _res;
        }
        public ChargeAmount GetChargeForApp(CommonReq _req)
        {
            var _res = new ChargeAmount
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            if (_req.CommonDecimal < 1)
            {
                _res.Statuscode = ErrorCodes.Minus1;
                _res.Msg = "Fill Amount";
                return _res;
            }
            var Helper = ConverterHelper.O.SplitAmounts(Convert.ToInt32(_req.CommonDecimal), 0, 0);
            IProcedure procDMR = new ProcGetDMRCharge(_dal);
            var CalculateAmount = 0M;
            foreach (var item in Helper)
            {
                _req.CommonDecimal = item.Amount;
                var procRes = (ChargeAmount)procDMR.Call(_req);
                _res = procRes;
                if (_res.Statuscode == ErrorCodes.Minus1)
                {
                    break;
                }
                CalculateAmount += procRes.Charged;
            }
            if (_res.Statuscode == ErrorCodes.Minus1)
            {
                _res.Charged = 0;
            }
            else
            {
                _res.Charged = CalculateAmount;
            }
            return _res;
        }
        public TransactionDetail DMRReceipt(string GroupID, decimal convenientFee)
        {
            string TransDate = string.IsNullOrEmpty(GroupID) ? DateTime.Now.ToString("dd MMM yyyy") : ConnectionStringHelper.O.ConvertTransactionIDTo_dd_MMM_yyyy(GroupID);
            IProcedure procDMR = new ProcDMRTransactionReceipt(ChangeConString(TransDate));
            return (TransactionDetail)procDMR.Call(new CommonReq
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID,
                CommonStr = GroupID,
                CommonDecimal = convenientFee
            });
        }
        public TransactionDetail TransactionReceipt(int TID, string TransactionID, decimal convenientFee)
        {
            string TransDate = string.IsNullOrEmpty(TransactionID) ? DateTime.Now.ToString("dd MMM yyyy") : ConnectionStringHelper.O.ConvertTransactionIDTo_dd_MMM_yyyy(TransactionID);
            IProcedure proc = new ProcTransactionReceipt(ChangeConString(TransDate));
            return (TransactionDetail)proc.Call(new CommonReq
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID,
                CommonInt = TID,
                CommonDecimal = convenientFee
            });
        }
        public TransactionDetail TransactionReceiptApp(int TID, string TransactionID, decimal convenientFee, int UserID)
        {
            string TransDate = string.IsNullOrEmpty(TransactionID) ? DateTime.Now.ToString("dd MMM yyyy") : ConnectionStringHelper.O.ConvertTransactionIDTo_dd_MMM_yyyy(TransactionID);
            IProcedure proc = new ProcTransactionReceipt(ChangeConString(TransDate));
            return (TransactionDetail)proc.Call(new CommonReq
            {
                LoginID = UserID,
                LoginTypeID = LoginType.ApplicationUser,
                CommonInt = TID,
                CommonDecimal = convenientFee
            });
        }
        public TransactionDetail DMRReceiptApp(CommonReq req)
        {
            string TransDate = string.IsNullOrEmpty(req.CommonStr) ? DateTime.Now.ToString("dd MMM yyyy") : ConnectionStringHelper.O.ConvertTransactionIDTo_dd_MMM_yyyy(req.CommonStr);

            IProcedure procDMR = new ProcDMRTransactionReceipt(ChangeConString(TransDate));
            return (TransactionDetail)procDMR.Call(req);
        }
        private IDAL ChangeConString(string _date)
        {
            if (Validate.O.IsDateIn_dd_MMM_yyyy_Format(_date))
            {
                TypeMonthYear typeMonthYear = ConnectionStringHelper.O.GetTypeMonthYear(Convert.ToDateTime(_date.Replace(" ", "/")));
                if (typeMonthYear.ConType != ConnectionStringType.DBCon)
                {
                    return new DAL(_c.GetConnectionString(typeMonthYear.ConType, (typeMonthYear.MM ?? "") + "_" + (typeMonthYear.YYYY ?? "")));
                }
            }
            return _dal;
        }
        #endregion
        #region DMR-Pipe
        public MSenderLoginResponse GetSender(string MobileNo, int OID)
        {
            IMoneyTransferML mtml = new MoneyTransferML(_accessor, _env);
            return mtml.GetSender(new MTCommonRequest
            {
                RequestMode = RequestMode.PANEL,
                SenderMobile = MobileNo ?? "",
                UserID = _lr.UserID,
                OutletID = _lr.OutletID,
                OID = OID
            });
        }
        public MSenderCreateResp SenderKYC(SenderRequest request)
        {
            IMoneyTransferML mtml = new MoneyTransferML(_accessor, _env);
            return mtml.SenderKYC(new MTSenderDetail
            {
                RequestMode = RequestMode.PANEL,
                SenderMobile = request.MobileNo ?? "",
                UserID = _lr.UserID,
                OutletID = _lr.OutletID,
                OID = request.OID,
                NameOnKYC = request.NameOnKYC,
                AadharNo = request.AadharNo,
                PANNo = request.PANNo,
                AadharFront = request.AadharFront,
                AadharBack = request.AadharBack,
                SenderPhoto = request.SenderPhoto,
                PAN = request.PAN
            });
        }
        public MSenderCreateResp SenderEKYC(SenderRequest request)
        {
            IMoneyTransferML mtml = new MoneyTransferML(_accessor, _env);
            return mtml.SenderEKYC(new MTSenderDetail
            {
                RequestMode = RequestMode.PANEL,
                SenderMobile = request.MobileNo ?? "",
                UserID = _lr.UserID,
                OutletID = _lr.OutletID,
                OID = request.OID,
                NameOnKYC = request.NameOnKYC,
                AadharNo = request.AadharNo,
                PidData = request.PidData,
                ReferenceID = request.ReffID
            });
        }
        public MSenderCreateResp CreateSenderP(SenderRequest request)
        {
            IMoneyTransferML mtml = new MoneyTransferML(_accessor, _env);
            return mtml.CreateSender(new MTSenderDetail
            {
                RequestMode = RequestMode.PANEL,
                SenderMobile = request.MobileNo ?? "",
                UserID = _lr.UserID,
                OutletID = _lr.OutletID,
                OID = request.OID,
                FName = request.Name,
                LName = request.LastName,
                Address = request.Address,
                DOB = request.Dob,
                Pincode = request.PinCode,
                OTP = request.OTP
            });
        }
        public MSenderLoginResponse VerifySender(string MobileNo, string OTP, string RefID, int OID)
        {
            IMoneyTransferML mtml = new MoneyTransferML(_accessor, _env);
            return mtml.VerifySender(new MTOTPRequest
            {
                RequestMode = RequestMode.PANEL,
                SenderMobile = MobileNo ?? "",
                UserID = _lr.UserID,
                OutletID = _lr.OutletID,
                OID = OID,
                ReferenceID = RefID,
                OTP = OTP
            });
        }
        public MSenderCreateResp SenderResendOTP(string MobileNo, string SID, int OID)
        {
            IMoneyTransferML mtml = new MoneyTransferML(_accessor, _env);
            return mtml.SenderResendOTP(new MTOTPRequest
            {
                RequestMode = RequestMode.PANEL,
                SenderMobile = MobileNo ?? "",
                UserID = _lr.UserID,
                OutletID = _lr.OutletID,
                OID = OID,
                ReferenceID = SID
            });
        }
        public MSenderLoginResponse AddBeneFiciary(AddBeni request)
        {
            IMoneyTransferML mtml = new MoneyTransferML(_accessor, _env);
            return mtml.CreateBeneficiary(new MTBeneficiaryAddRequest
            {
                RequestMode = RequestMode.PANEL,
                SenderMobile = request.SenderMobileNo ?? string.Empty,
                UserID = _lr.UserID,
                OutletID = _lr.OutletID,
                OID = request.OID,
                ReferenceID = request.SID,
                BeneDetail = new MBeneDetail
                {
                    AccountNo = request.AccountNo,
                    BankID = request.BankID,
                    BankName = request.BankName,
                    BeneName = request.BeneName,
                    IFSC = request.IFSC,
                    MobileNo = request.MobileNo,
                    TransMode = request.ttype
                },
            });
        }
        public MBeneficiaryResp GetBeneficiary(string MobileNo, string ReferenceID, int OID)
        {
            IMoneyTransferML mtml = new MoneyTransferML(_accessor, _env);
            return mtml.GetBeneficiary(new MTCommonRequest
            {
                RequestMode = RequestMode.PANEL,
                SenderMobile = MobileNo ?? "",
                UserID = _lr.UserID,
                OutletID = _lr.OutletID,
                OID = OID,
                ReferenceID = ReferenceID
            });
        }
        public MDMTResponse AccountTransfer(ReqSendMoney reqSendMoney)
        {
            var resp = new MDMTResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = RechargeRespType._PENDING,
                ErrorCode = ErrorCodes.Request_Accpeted
            };

            IMoneyTransferML mtml = new MoneyTransferML(_accessor, _env);
            resp = mtml.AccountTransfer(new MBeneVerifyRequest
            {
                RequestMode = RequestMode.PANEL,
                SenderMobile = reqSendMoney.MobileNo,
                UserID = _lr.UserID,
                OutletID = _lr.OutletID,
                OID = reqSendMoney.o,
                AccountNo = reqSendMoney.AccountNo,
                IFSC = reqSendMoney.IFSC,
                BankID = reqSendMoney.BankID,
                Bank = reqSendMoney.Bank,
                Amount = reqSendMoney.Amount,
                BeneficiaryName = reqSendMoney.BeneName,
                TransMode = reqSendMoney.Channel ? "IMPS" : "NEFT",
                SecureKey = reqSendMoney.SecKey,
                ReferenceID = reqSendMoney.RefferenceID,
                BeneficiaryID = reqSendMoney.BeneID
            });

            if (resp.ErrorCode == 150)
            {
                resp.Statuscode = 1;
                resp.Msg = "Request already in progress";
            }
            resp.Status = RechargeRespType.GetRechargeStatusText(resp.Statuscode);
            return resp;
        }
        public MSenderLoginResponse DeleteBene(int OID, string MobileNo, string BeneID, string ReferenceID, string OTP)
        {
            IMoneyTransferML mtml = new MoneyTransferML(_accessor, _env);
            return mtml.RemoveBeneficiary(new MBeneVerifyRequest
            {
                RequestMode = RequestMode.PANEL,
                SenderMobile = MobileNo ?? "",
                UserID = _lr.UserID,
                OutletID = _lr.OutletID,
                OID = OID,
                ReferenceID = ReferenceID,
                OTP = OTP,
                BeneficiaryID = BeneID
            });
        }
        public MSenderCreateResp GenerateOTP(int OID, string MobileNo, string ReferenceID, string BeneID)
        {
            IMoneyTransferML mtml = new MoneyTransferML(_accessor, _env);
            return mtml.GenerateOTP(new MTBeneficiaryAddRequest
            {
                RequestMode = RequestMode.PANEL,
                SenderMobile = MobileNo ?? "",
                UserID = _lr.UserID,
                OutletID = _lr.OutletID,
                OID = OID,
                ReferenceID = ReferenceID,
                BeneDetail = new MBeneDetail
                {
                    BeneID = BeneID
                }
            });
        }
        public MSenderLoginResponse ValidateBeneficiaryOTP(int OID, string MobileNo, string BeneMobile, string AccountNo, string OTP, string ReferenceID, string BeneID)
        {

            IMoneyTransferML mtml = new MoneyTransferML(_accessor, _env);
            return mtml.ValidateBeneficiary(new MBeneVerifyRequest
            {
                RequestMode = RequestMode.PANEL,
                SenderMobile = MobileNo ?? "",
                UserID = _lr.UserID,
                OutletID = _lr.OutletID,
                OID = OID,
                MobileNo = BeneMobile,
                AccountNo = AccountNo,
                OTP = OTP,
                ReferenceID = ReferenceID,
                BeneficiaryID = BeneID
            });
        }
        public MDMTResponse VerifyAccount(int OID, string MobileNo, string AccountNo, string IFSC, string Bank, int BankID, string BeneName, string ReferenceID)
        {
            var resp = new MDMTResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = RechargeRespType._PENDING,
                ErrorCode = ErrorCodes.Request_Accpeted
            };

            IMoneyTransferML mtml = new MoneyTransferML(_accessor, _env);
            resp = mtml.VerifyAccount(new MBeneVerifyRequest
            {
                RequestMode = RequestMode.PANEL,
                SenderMobile = MobileNo,
                UserID = _lr.UserID,
                OutletID = _lr.OutletID,
                OID = OID,
                AccountNo = AccountNo,
                IFSC = IFSC,
                BankID = BankID,
                Bank = Bank,
                BeneficiaryName = BeneName,
                TransMode = "NEFT",
                ReferenceID = ReferenceID
            });
            resp.Status = RechargeRespType.GetRechargeStatusText(resp.Statuscode);
            return resp;
        }
        public ChargeAmount GetCharges(int OID, int amt)
        {
            IMoneyTransferML mtml = new MoneyTransferML(_accessor, _env);
            return mtml.GetCharge(new MTGetChargeRequest
            {
                RequestMode = RequestMode.PANEL,
                UserID = _lr.UserID,
                OutletID = _lr.OutletID,
                OID = OID,
                Amount = amt
            });
        }

        #endregion
        #region UPIPayement
        public MDMTResponse DoUPIPayment(ReqSendMoney reqSendMoney ,int UserID=0)
        {
            var resp = new MDMTResponse
            {
                Statuscode = RechargeRespType.PENDING,
                Msg = RechargeRespType._PENDING,
                ErrorCode = ErrorCodes.Request_Accpeted
            };

            IMoneyTransferML transactionML = new MoneyTransferML(_accessor, _env);
            resp = transactionML.DoUPIPaymentService(new MBeneVerifyRequest
            {
                RequestMode = RequestMode.PANEL,
                UserID = _lr.UserID,
                AccountNo = reqSendMoney.AccountNo,
                Amount = reqSendMoney.Amount,
                BeneficiaryName = reqSendMoney.BeneName
            });
            if (resp.ErrorCode == 150)
            {
                resp.Statuscode = 1;
                resp.Msg = "Request already in progress";
            }
            resp.Status = RechargeRespType.GetRechargeStatusText(resp.Statuscode);
            return resp;
        }
        #endregion
        #region OnboardSection
        public ValidateOuletModel CheckOnboardUser(int OID, string OTP, int OTPRefID, string PidData, bool IsBio,int BioAuthtype)
        {
            var resp = new ValidateOuletModel
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError,
                ErrorCode = ErrorCodes.Invalid_Access,
                IsConfirmation = true
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Retailor_Seller)
            {
                var BBPSML = new BBPSML(_accessor, _env);
                resp = BBPSML.CheckOutletStatusForServices(new CheckOutletStatusReqModel
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    OID = OID,
                    OutletID = _lr.OutletID,
                    OTP = OTP,
                    RMode = RequestMode.PANEL,
                    OTPRefID = OTPRefID,
                    PidData = PidData,
                    IsVerifyBiometric = IsBio,
                    BioAuthType=BioAuthtype
                });
            }
            return resp;
        }
        #endregion

        #region PSA
        public async Task<IResponseStatus> PSATransaction(RechargeRequest req)
        {
            IResponseStatus resp = new ResponseStatus
            {
                Statuscode = 1,
                Msg = "PENDING"
            };
            try
            {
                var _req = new _RechargeAPIRequest
                {
                    UserID = _lr.UserID,
                    Account = req.Mob,
                    Amount = req.Amt,
                    APIRequestID = "",
                    OID = req.OID,
                    OutletID = _lr.OutletID,
                    RefID = "",
                    RequestMode = RequestMode.PANEL,
                    IPAddress = _info.GetRemoteIP(),
                    Pincode = _lr.Pincode,
                    SecurityKey = req.SecKey ?? string.Empty
                };
                var transactionML = new TransactionML(_accessor, _env, _dal, _info);
                var TResp = await transactionML.DoPSATransaction(_req);
                resp.Statuscode = TResp.STATUS;
                resp.Msg = TResp.MSG;
                resp.CommonStr = RechargeRespType.GetRechargeStatusText(TResp.STATUS);
            }
            catch { }
            return resp;
        }

        public async Task<IResponseStatus> PSATransaction(_RechargeAPIRequest _req)
        {
            var resp = new ResponseStatus
            {
                Statuscode = 1,
                Msg = "PENDING"
            };
            try
            {
                _req.IPAddress = _info.GetRemoteIP();
                var transactionML = new TransactionML(_accessor, _env, _dal, _info);
                var TResp = await transactionML.DoPSATransaction(_req).ConfigureAwait(false);
                resp.Statuscode = TResp.STATUS;
                resp.Msg = TResp.MSG;
                resp.CommonStr = RechargeRespType.GetRechargeStatusText(TResp.STATUS);
                resp.CommonStr2 = TResp.OPID;
            }
            catch { }
            return resp;
        }
        #endregion

        #region CallMe
        public ResponseStatus CallMeUserRequest(string uMob)
        {
            CommonReq req = new CommonReq
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID,
                CommonStr = uMob

            };
            IProcedure proc = new ProcCallMeUserRequest(_dal);
            ResponseStatus res = (ResponseStatus)proc.Call(req);
            return res;
        }

        public AppResponse AppCallMeUserRequest(CommonReq _req)
        {
            IProcedure proc = new ProcCallMeUserRequest(_dal);
            ResponseStatus res = (ResponseStatus)proc.Call(_req);
            AppResponse _res = new AppResponse
            {
                Statuscode = res.Statuscode,
                Msg = res.Msg
            };
            return _res;
        }
        #endregion
        #region DTHSubscription
        public async Task<IResponseStatus> DoDTHSubscription(DTHConnectionServiceModel mdl)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Retailor_Seller)
            {
                IDTHSubscriptionML dTHSubscriptionML = new DTHSubscriptionML(_accessor, _env, _info);
                var mlRes = await dTHSubscriptionML.DoDTHSubscription(new DTHConnectionServiceRequest
                {
                    PID = mdl.PID,
                    UserID = _lr.UserID,
                    CustomerNumber = mdl.CustomerNumber,
                    Customer = mdl.Customer + ' ' + mdl.Customersurname,
                    Address = mdl.Address,
                    Pincode = mdl.Pincode,
                    SecurityKey = mdl.SecurityKey,
                    Gender = mdl.Gender,
                    AreaID = mdl.AreaID,
                    RequestModeID = RequestMode.PANEL
                }).ConfigureAwait(false);
                res.Statuscode = mlRes.Statuscode;
                res.Msg = mlRes.Msg;
                res.CommonStr = mlRes.TransactionID;
            }
            return res;
        }
        public async Task<IResponseStatus> DoDTHSubscription(DTHConnectionServiceRequest serviceReq)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            serviceReq.RequestModeID = RequestMode.APPS;
            serviceReq.RequestIP = _info.GetRemoteIP();
            IDTHSubscriptionML dTHSubscriptionML = new DTHSubscriptionML(_accessor, _env, _info);
            var mlRes = await dTHSubscriptionML.DoDTHSubscription(serviceReq).ConfigureAwait(false);
            res.Statuscode = mlRes.Statuscode;
            res.Msg = mlRes.Msg;
            res.CommonStr = mlRes.TransactionID;
            return res;
        }
        #endregion

        public TransactionDetail AEPSReceipt(int TID, string TransactionID, decimal convenientFee)
        {
            string TransDate = string.IsNullOrEmpty(TransactionID) ? DateTime.Now.ToString("dd MMM yyyy") : ConnectionStringHelper.O.ConvertTransactionIDTo_dd_MMM_yyyy(TransactionID);
            IProcedure proc = new ProcAEPSReceipt(ChangeConString(TransDate));
            return (TransactionDetail)proc.Call(new CommonReq
            {
                LoginID = _lr.UserID,
                LoginTypeID = _lr.LoginTypeID,
                CommonInt = TID,
                CommonDecimal = convenientFee
            });
        }
    }
}
