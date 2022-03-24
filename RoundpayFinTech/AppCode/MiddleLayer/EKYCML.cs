using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.MoneyTransfer;
using RoundpayFinTech.AppCode.StaticModel.MoneyTransfer;
using RoundpayFinTech.AppCode.ThirdParty.Zoop;
using System.Linq;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public class EKYCML : IEKYCML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        private readonly IRequestInfo _rinfo;
        public EKYCML(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            _rinfo = new RequestInfo(_accessor, _env);
        }
        public EKYCGetDetail GetEKYCDetailOfUser(CommonReq commonReq)
        {
            IProcedure proc = new ProcGetEKYCDetail(_dal);
            var res = (EKYCGetDetail)proc.Call(commonReq);

            return res;
        }
        public IDAL GetDAL() => _dal;
        public EKYCGSTINResponseModel ValidateGST(EKYCRequestModel req)
        {
            var funRep = new EKYCGSTINResponseModel
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "(NAPI)EKYC Service down"
            };
            IProcedure _proc = new ProcCheckEKYCOperatorSwithing(_dal);
            var validateRes = (EKYCSwitchAPIResp)_proc.Call(new CommonReq { LoginID = req.UserID, CommonStr = SPKeys.GSTVerification });
            var apiResp = new EKYCByGSTINModel
            {

            };
            funRep.Statuscode = validateRes.Statuscode;
            funRep.Msg = validateRes.Msg;

            if (validateRes.Statuscode == ErrorCodes.One)
            {
                if (req.IsSkip == false)
                {
                    if (string.IsNullOrEmpty(validateRes.APICode))
                    {
                        return funRep;
                    }
                    if (validateRes.APICode == APICode.ZOOP)
                    {
                        var zoopML = new ZoopML(_accessor, _env, _dal, validateRes.APIID);
                        apiResp = zoopML.ValidateGSTINAdvance(req.VerificationAccount);
                        funRep.Statuscode = apiResp.Statuscode;
                    }
                }
                var procReq = new EKYCByGSTINProcReq
                {
                    APIID = req.IsSkip ? 0 : validateRes.APIID,
                    Address = apiResp.Address,
                    AgreegateturnOver = apiResp.AgreegateturnOver,
                    AuthorisedSignatory = apiResp.AuthorisedSignatory,
                    CentralJurisdiction = apiResp.CentralJurisdiction,
                    ChildUserID = req.ChildUserID,
                    UserID = req.UserID,
                    EmailID = apiResp.EmailID,
                    GSTIN = req.VerificationAccount,
                    IsExternal = req.IsExternal,
                    IsSkip = req.IsSkip,
                    LegalName = apiResp.LegalName,
                    MobileNo = apiResp.MobileNo,
                    RegisterDate = apiResp.RegisterDate,
                    StateJurisdiction = apiResp.StateJurisdiction,
                    TradeName = apiResp.TradeName,
                    Statuscode = apiResp.Statuscode,
                    Msg = apiResp.Msg,
                    APIStatus = apiResp.Statuscode == ErrorCodes.One && req.IsSkip == false,
                    CompanyTypeID = req.CompanyTypeID
                };
                if (!string.IsNullOrEmpty(apiResp.GSTIN))
                {
                    var PANNo = apiResp.GSTIN.Substring(2, 10);
                    procReq.PANNumber = PANNo;
                }
                _proc = new ProcEKYCByGSTIN(_dal);
                var procRes = (ResponseStatus)_proc.Call(procReq);
                funRep.Statuscode = procRes.Statuscode;
                funRep.Msg = procRes.Msg;
                funRep.EKYCID = procRes.CommonInt;
                funRep.IsVerified = apiResp.Statuscode == ErrorCodes.One;
                funRep.PANNo = procReq.PANNumber;
                if (procReq.APIStatus)
                {
                    if (!string.IsNullOrEmpty(procReq.AuthorisedSignatory))
                    {
                        funRep.AuthorisedSignatory = procReq.AuthorisedSignatory.Split(',').ToList();
                    }
                }
            }
            return funRep;
        }

        public EKYCByAadharModelOTP GenerateAadharOTP(EKYCRequestModel req)
        {
            var funRep = new EKYCByAadharModelOTP
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "(NAPI)EKYC Service down"
            };
            IProcedure _proc = new ProcCheckEKYCOperatorSwithing(_dal);
            var validateRes = (EKYCSwitchAPIResp)_proc.Call(new CommonReq { LoginID = req.UserID, CommonStr = SPKeys.AadhaarVerification });
            var apiResp = new EKYCByAadharModelOTP
            {

            };
            funRep.Statuscode = validateRes.Statuscode;
            funRep.Msg = validateRes.Msg;

            if (validateRes.Statuscode == ErrorCodes.One)
            {
                if (string.IsNullOrEmpty(validateRes.APICode))
                {
                    return funRep;
                }
                if (validateRes.APICode == APICode.ZOOP)
                {
                    //var zoopML = new ZoopML(_accessor, _env, _dal, validateRes.APIID);
                    //apiResp = zoopML.GenerateAadharOTP(req.VerificationAccount, req.DirectorName);
                    //funRep.Statuscode = apiResp.Statuscode;
                    //funRep.Msg = apiResp.Msg;
                    //Aadhar only
                    _proc = new ProcInitiateEKYCCallRequest(_dal);
                    var InitiateID = (int)_proc.Call(new CommonReq
                    {
                        LoginID = req.UserID,
                        CommonInt = validateRes.APIID
                    });
                    var zoopML = new ZoopML(_accessor, _env, _dal, validateRes.APIID);
                    var initResp = zoopML.InitiateCall(InitiateID);

                    if (initResp.Statuscode == ErrorCodes.One)
                    {
                        _proc = new ProcUpdateInitiateEKYC(_dal);
                        _proc.Call(new CommonReq
                        {
                            CommonInt = InitiateID,
                            CommonStr = initResp.VendorID,
                            CommonStr1 = initResp.SecurityKey
                        });
                        funRep.Statuscode = ErrorCodes.One;
                        funRep.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                        funRep.IsCallSDK = true;
                        funRep.InitiateID = initResp.VendorID;
                    }
                }
                funRep.ReferenceID = apiResp.ReferenceID;
                funRep.IsAadharValid = apiResp.IsAadharValid;
                funRep.IsNumberLinked = apiResp.IsNumberLinked;
                funRep.IsOTPSent = funRep.IsCallSDK == false;
            }
            return funRep;
        }

        public EKYCByAadharModel ValidateAadharOTP(EKYCRequestModel req)
        {
            var funRep = new EKYCByAadharModel
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "(NAPI)EKYC Service down"
            };
            IProcedure _proc = new ProcCheckEKYCOperatorSwithing(_dal);
            var validateRes = (EKYCSwitchAPIResp)_proc.Call(new CommonReq { LoginID = req.UserID, CommonStr = SPKeys.AadhaarVerification });
            var apiResp = new EKYCByAadharModel
            {

            };
            funRep.Statuscode = validateRes.Statuscode;
            funRep.Msg = validateRes.Msg;

            if (validateRes.Statuscode == ErrorCodes.One)
            {

                if (string.IsNullOrEmpty(validateRes.APICode))
                {
                    return funRep;
                }
                ProcAadharOTPReference procAadharOTPReference = new ProcAadharOTPReference(_dal);
                var otpRefResp = procAadharOTPReference.GetAadharRefrence(req.ReferenceID);
                if (string.IsNullOrEmpty(otpRefResp.CommonStr))
                {
                    funRep.Statuscode = ErrorCodes.Minus1;
                    funRep.Msg = "Invalid OTP Refrence";
                    return funRep;
                }
                if (validateRes.APICode == APICode.ZOOP)
                {
                    var zoopML = new ZoopML(_accessor, _env, _dal, validateRes.APIID);
                    apiResp = zoopML.ValidateAadharOTP(otpRefResp.CommonStr, req.OTP);
                    funRep.Statuscode = apiResp.Statuscode;
                    funRep.Msg = apiResp.Msg;
                }
                if (apiResp.Statuscode == ErrorCodes.One)
                {
                    var procReq = new EKYCByAadharProcReq
                    {
                        APIID = validateRes.APIID,
                        ChildUserID = req.ChildUserID,
                        UserID = req.UserID,
                        IsExternal = req.IsExternal,
                        APIStatus = apiResp.Statuscode == ErrorCodes.One,
                        AadhaarNo = otpRefResp.CommonStr2,
                        Country = apiResp.Country,
                        District = apiResp.District,
                        DOB = apiResp.DOB,
                        FullName = apiResp.FullName,
                        Gender = apiResp.Gender,
                        HasImage = apiResp.HasImage,
                        House = apiResp.House,
                        IsMobileVerified = apiResp.IsMobileVerified,
                        Landmark = apiResp.Landmark,
                        Location = apiResp.Location,
                        ParentName = apiResp.ParentName,
                        Pincode = apiResp.Pincode,
                        PostOffice = apiResp.PostOffice,
                        Profile = apiResp.Profile,
                        ShareCode = apiResp.ShareCode,
                        Street = apiResp.Street,
                        State = apiResp.State,
                        SubDistrict = apiResp.SubDistrict,
                        VTC = apiResp.VTC,
                        DirectorName = otpRefResp.CommonStr3
                    };
                    funRep = apiResp;
                    _proc = new ProcEKYCByAadhar(_dal);
                    var procRes = (ResponseStatus)_proc.Call(procReq);
                    funRep.Statuscode = procRes.Statuscode;
                    funRep.Msg = procRes.Msg;
                };
            }
            return funRep;
        }

        public EKYCByPANModel GetPanDetail(EKYCRequestModel req)
        {
            var funRep = new EKYCByPANModel
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "(NAPI)EKYC Service down"
            };
            IProcedure _proc = new ProcCheckEKYCOperatorSwithing(_dal);
            var validateRes = (EKYCSwitchAPIResp)_proc.Call(new CommonReq { LoginID = req.UserID, CommonStr = SPKeys.PANVerification });
            var apiResp = new EKYCByPANModel
            {

            };
            funRep.Statuscode = validateRes.Statuscode;
            funRep.Msg = validateRes.Msg;

            if (validateRes.Statuscode == ErrorCodes.One)
            {
                if (string.IsNullOrEmpty(validateRes.APICode))
                {
                    return funRep;
                }
                if (validateRes.APICode == APICode.ZOOP)
                {
                    var zoopML = new ZoopML(_accessor, _env, _dal, validateRes.APIID);
                    apiResp = zoopML.ValidatePANNumber(req.VerificationAccount);
                    funRep.Statuscode = apiResp.Statuscode;
                    funRep.Msg = apiResp.Msg;
                }
                if (apiResp.Statuscode == ErrorCodes.One)
                {
                    var procReq = new EKYCByPANModelProcReq
                    {
                        APIID = validateRes.APIID,
                        ChildUserID = req.ChildUserID,
                        UserID = req.UserID,
                        IsExternal = req.IsExternal,
                        APIStatus = apiResp.Statuscode == ErrorCodes.One,
                        FullName = apiResp.FullName,
                        FirstName = apiResp.FirstName,
                        LastName = apiResp.LastName,
                        PANNumber = req.VerificationAccount,
                        IsAadharSeeded = apiResp.IsAadharSeeded,
                        IsPANValid = apiResp.IsPANValid,
                        Title = apiResp.Title,
                        DirectorName = req.DirectorName
                    };
                    funRep = apiResp;
                    _proc = new ProcEKYCByPAN(_dal);
                    var procRes = (ResponseStatus)_proc.Call(procReq);
                    funRep.Statuscode = procRes.Statuscode;
                    funRep.Msg = procRes.Msg;
                };
            }
            return funRep;
        }
        public EKYCByBankAccountModel GetBankAccountDetail(EKYCRequestModel req)
        {
            var funRep = new EKYCByBankAccountModel
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Account Validation failed"
            };
            IProcedure _proc = new ProcCheckEKYCOperatorSwithing(_dal);
            var validateRes = (EKYCSwitchAPIResp)_proc.Call(new CommonReq { LoginID = req.UserID, CommonStr = SPKeys.AccountVerification });

            funRep.Statuscode = validateRes.Statuscode;
            funRep.Msg = validateRes.Msg;

            if (validateRes.Statuscode == ErrorCodes.One)
            {
                IMoneyTransferML mtml = new MoneyTransferML(_accessor, _env);
                var localRes = mtml.VerifyAccount(new MBeneVerifyRequest
                {
                    RequestMode = req.RequestMode,
                    SenderMobile = validateRes.MobileNo,
                    UserID = req.UserID,
                    OutletID = 0,
                    OID = validateRes.OID,
                    AccountNo = req.VerificationAccount,
                    IFSC = req.IFSC,
                    BankID = req.BankID,
                    Bank = string.Empty,
                    BeneficiaryName = string.Empty,
                    TransMode = "IMPS",
                    ReferenceID = string.Empty,
                    APIRequestID = string.Empty,
                    IsInternal = true
                });
                if (localRes.Statuscode > 0 && localRes.Statuscode != 3)
                {
                    var procReq = new EKYCByBankAccountModelProcReq
                    {
                        APIID = validateRes.APIID,
                        ChildUserID = req.ChildUserID,
                        UserID = req.UserID,
                        IsExternal = req.IsExternal,
                        APIStatus = localRes.Statuscode > 0,
                        AccountNumber = req.VerificationAccount,
                        AccountHolder = localRes.BeneName,
                        IFSC = req.IFSC,
                        LiveID = localRes.LiveID,
                        BankID = req.BankID,
                        Bank = localRes.Bank
                    };
                    _proc = new ProcEKYCByBankAccount(_dal);
                    var procRes = (ResponseStatus)_proc.Call(procReq);
                    funRep.Statuscode = procRes.Statuscode;
                    funRep.Msg = procRes.Msg;
                    funRep.AccountHolder = localRes.BeneName;
                    funRep.LiveID = localRes.LiveID;
                }
                else
                {
                    funRep.Statuscode = ErrorCodes.Minus1;
                    funRep.Msg = localRes.Msg;
                }

            }
            return funRep;
        }

        public ResponseStatus EditEKYCStep(EKYCRequestModel req)
        {
            IProcedure proc = new ProcRequestEditStepEKYC(_dal);
            return (ResponseStatus)proc.Call(new EKYCStepEditReq
            {
                UserID = req.UserID,
                EditStepID = req.EditStepID,
                ChildUserID = req.ChildUserID,
                IsExternal = req.IsExternal
            });
        }

        public void UpdateEKYCFromCallBack(EKYCByAadharProcReq procReq)
        {
            if (procReq.InitiateID > 0)
            {
                IProcedure _proc = new ProcValidateInitiatedEKYCRequest(_dal);
                var validRes = (ResponseStatus)_proc.Call(new CommonReq
                {
                    CommonInt = procReq.InitiateID,
                    CommonStr = procReq.VendorID
                });
                if (validRes.Statuscode == ErrorCodes.One)
                {
                    _proc = new ProcEKYCByAadhar(_dal);
                    _proc.Call(procReq);
                }
            }

        }
    }
}
