using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.MoneyTransfer;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Validators;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public class DMTAPIUserML : IDMTAPIUserML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        private readonly IRequestInfo _info;
        public DMTAPIUserML(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            _info = new RequestInfo(_accessor, _env);
        }

        public async Task<SenderLoginResponse> GetSender(DMTAPIRequest dMTAPIRequest)
        {
            var res = new SenderLoginResponse
            {
                ErrorCode = ErrorCodes.Invalid_Access,
                Message = nameof(ErrorCodes.Invalid_Access).Replace("_", " "),
                Statuscode = ErrorCodes.Minus1
            };
            #region RequestValidationFromCode  
            /**
             * Request validation from Code started
             * **/
            if (dMTAPIRequest.UserID < 2)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = "Unauthorised access!";
                return res;
            }
            if (dMTAPIRequest.OutletID < 10000)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(dMTAPIRequest.OutletID);
                return res;
            }
            if ((dMTAPIRequest.Token ?? string.Empty).Length != 32)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(dMTAPIRequest.Token);
                return res;
            }
            if (string.IsNullOrEmpty(dMTAPIRequest.SPKey))
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(dMTAPIRequest.SPKey);
                return res;
            }
            #endregion
            #region RequestValidationFromDB
            /**
             * Request validation from DB started
             * **/
            var validateReq = new ValidataRechargeApirequest
            {
                LoginID = dMTAPIRequest.UserID,
                IPAddress = _info.GetRemoteIP(),
                Token = dMTAPIRequest.Token,
                SPKey = dMTAPIRequest.SPKey
            };
            IProcedureAsync _procValidate = new ProcValidataRechargeApirequest(_dal);
            var validateRes = (ValidataRechargeApiResp)await _procValidate.Call(validateReq).ConfigureAwait(false);
            if (validateRes.Statuscode == ErrorCodes.Minus1)
            {
                res.Message = validateRes.Msg;
                res.ErrorCode = Convert.ToInt32(string.IsNullOrEmpty(validateRes.ErrorCode) ? "0" : validateRes.ErrorCode);
                return res;
            }
            #endregion
            IMoneyTransferML mtml = new MoneyTransferML(_accessor, _env);
            var localRes = mtml.GetSender(new MTCommonRequest
            {
                RequestMode = RequestMode.API,
                SenderMobile = dMTAPIRequest.SenderMobile ?? string.Empty,
                UserID = dMTAPIRequest.UserID,
                OutletID = dMTAPIRequest.OutletID,
                OID = validateRes.OID
            });
            res.Statuscode = localRes.Statuscode;
            res.Message = localRes.Msg;
            res.ErrorCode = localRes.ErrorCode;
            res.IsSenderNotExists = localRes.IsSenderNotExists;
            res.IsActive = !localRes.IsNotActive;
            res.SenderName = localRes.SenderName;
            res.SenderMobile = dMTAPIRequest.SenderMobile;
            res.TotalLimit = localRes.AvailbleLimit;
            res.AvailbleLimit = localRes.RemainingLimit;
            res.ReferenceID = localRes.ReferenceID;
            res.IsEKYCAvailable = localRes.IsEKYCAvailable;
            res.KYCStatus = localRes.KYCStatus;
            return res;
        }
        public async Task<SenderCreateResponse> CreateSender(CreateSednerReq dMTAPIRequest)
        {
            var res = new SenderCreateResponse
            {
                ErrorCode = ErrorCodes.Invalid_Access,
                Message = nameof(ErrorCodes.Invalid_Access).Replace("_", " "),
                Statuscode = ErrorCodes.Minus1
            };
            #region RequestValidationFromCode  
            /**
             * Request validation from Code started
             * **/
            if (dMTAPIRequest.UserID < 2)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = "Unauthorised access!";
                return res;
            }
            if (dMTAPIRequest.OutletID < 10000)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(dMTAPIRequest.OutletID);
                return res;
            }
            if ((dMTAPIRequest.Token ?? string.Empty).Length != 32)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(dMTAPIRequest.Token);
                return res;
            }
            if (string.IsNullOrEmpty(dMTAPIRequest.SPKey))
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(dMTAPIRequest.SPKey);
                return res;
            }

            #endregion
            #region RequestValidationFromDB
            /**
             * Request validation from DB started
             * **/
            var validateReq = new ValidataRechargeApirequest
            {
                LoginID = dMTAPIRequest.UserID,
                IPAddress = _info.GetRemoteIP(),
                Token = dMTAPIRequest.Token,
                SPKey = dMTAPIRequest.SPKey
            };
            IProcedureAsync _procValidate = new ProcValidataRechargeApirequest(_dal);
            var validateRes = (ValidataRechargeApiResp)await _procValidate.Call(validateReq).ConfigureAwait(false);
            if (validateRes.Statuscode == ErrorCodes.Minus1)
            {
                res.Message = validateRes.Msg;
                res.ErrorCode = Convert.ToInt32(string.IsNullOrEmpty(validateRes.ErrorCode) ? "0" : validateRes.ErrorCode);
                return res;
            }
            #endregion
            IMoneyTransferML mtml = new MoneyTransferML(_accessor, _env);
            var localRes = mtml.CreateSender(new MTSenderDetail
            {
                RequestMode = RequestMode.API,
                SenderMobile = dMTAPIRequest.SenderMobile,
                UserID = dMTAPIRequest.UserID,
                OutletID = dMTAPIRequest.OutletID,
                OID = validateRes.OID,
                FName = dMTAPIRequest.FirstName,
                LName = dMTAPIRequest.LastName,
                Address = dMTAPIRequest.Address,
                DOB = dMTAPIRequest.DOB,
                Pincode = Convert.ToInt32(dMTAPIRequest.Pincode),
                OTP = dMTAPIRequest.OTP,
                ReferenceID = dMTAPIRequest.ReferenceID
            });
            res.Statuscode = localRes.Statuscode;
            res.Message = localRes.Msg;
            res.ErrorCode = localRes.ErrorCode;
            res.IsOTPRequired = localRes.IsOTPGenerated;
            res.IsOTPResendAvailble = localRes.IsOTPResendAvailble;
            res.ReferenceID = localRes.ReferenceID ?? string.Empty;
            return res;
        }
        public async Task<SenderCreateResponse> SenderKYC(int UserID, string Token, int OutletID, string SenderMobile, string ReferenceID, string SPKey, string NameOnKYC, string AadharNo, string PANNo, IFormFile AadharFront, IFormFile AadharBack, IFormFile SenderPhoto, IFormFile PAN)
        {
            var res = new SenderCreateResponse
            {
                ErrorCode = ErrorCodes.Invalid_Access,
                Message = nameof(ErrorCodes.Invalid_Access).Replace("_", " "),
                Statuscode = ErrorCodes.Minus1
            };
            #region RequestValidationFromCode  
            /**
             * Request validation from Code started
             * **/
            if (UserID < 2)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = "Unauthorised access!";
                return res;
            }
            if (OutletID < 10000)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(OutletID);
                return res;
            }
            if ((Token ?? string.Empty).Length != 32)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(Token);
                return res;
            }
            if (string.IsNullOrEmpty(SPKey))
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(SPKey);
                return res;
            }

            #endregion
            #region RequestValidationFromDB
            /**
             * Request validation from DB started
             * **/
            var validateReq = new ValidataRechargeApirequest
            {
                LoginID = UserID,
                IPAddress = _info.GetRemoteIP(),
                Token = Token,
                SPKey = SPKey
            };
            IProcedureAsync _procValidate = new ProcValidataRechargeApirequest(_dal);
            var validateRes = (ValidataRechargeApiResp)await _procValidate.Call(validateReq).ConfigureAwait(false);
            if (validateRes.Statuscode == ErrorCodes.Minus1)
            {
                res.Message = validateRes.Msg;
                res.ErrorCode = Convert.ToInt32(string.IsNullOrEmpty(validateRes.ErrorCode) ? "0" : validateRes.ErrorCode);
                return res;
            }
            #endregion
            IMoneyTransferML mtml = new MoneyTransferML(_accessor, _env);
            var localRes = mtml.SenderKYC(new MTSenderDetail
            {
                RequestMode = RequestMode.API,
                SenderMobile = SenderMobile,
                UserID = UserID,
                OutletID = OutletID,
                OID = validateRes.OID,
                NameOnKYC = NameOnKYC,
                AadharNo = AadharNo,
                PANNo = PANNo,
                AadharFront = AadharFront,
                AadharBack = AadharBack,
                SenderPhoto = SenderPhoto,
                PAN = PAN,
                ReferenceID = ReferenceID
            });
            res.Statuscode = localRes.Statuscode;
            res.Message = localRes.Msg;
            res.ErrorCode = localRes.ErrorCode;
            return res;
        }
        public async Task<DMTAPIResponse> VerifySender(DMTAPIRequest dMTAPIRequest)
        {
            var res = new DMTAPIResponse
            {
                ErrorCode = ErrorCodes.Invalid_Access,
                Message = nameof(ErrorCodes.Invalid_Access).Replace("_", " "),
                Statuscode = ErrorCodes.Minus1
            };
            #region RequestValidationFromCode  
            /**
             * Request validation from Code started
             * **/
            if (dMTAPIRequest.UserID < 2)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = "Unauthorised access!";
                return res;
            }
            if (dMTAPIRequest.OutletID < 10000)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(dMTAPIRequest.OutletID);
                return res;
            }
            if ((dMTAPIRequest.Token ?? string.Empty).Length != 32)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(dMTAPIRequest.Token);
                return res;
            }
            if (string.IsNullOrEmpty(dMTAPIRequest.SPKey))
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(dMTAPIRequest.SPKey);
                return res;
            }


            #endregion
            #region RequestValidationFromDB
            /**
             * Request validation from DB started
             * **/
            var validateReq = new ValidataRechargeApirequest
            {
                LoginID = dMTAPIRequest.UserID,
                IPAddress = _info.GetRemoteIP(),
                Token = dMTAPIRequest.Token,
                SPKey = dMTAPIRequest.SPKey
            };
            IProcedureAsync _procValidate = new ProcValidataRechargeApirequest(_dal);
            var validateRes = (ValidataRechargeApiResp)await _procValidate.Call(validateReq).ConfigureAwait(false);
            if (validateRes.Statuscode == ErrorCodes.Minus1)
            {
                res.Message = validateRes.Msg;
                res.ErrorCode = Convert.ToInt32(string.IsNullOrEmpty(validateRes.ErrorCode) ? "0" : validateRes.ErrorCode);
                return res;
            }
            #endregion
            IMoneyTransferML mtml = new MoneyTransferML(_accessor, _env);
            var localRes = mtml.VerifySender(new MTOTPRequest
            {
                RequestMode = RequestMode.API,
                SenderMobile = dMTAPIRequest.SenderMobile ?? string.Empty,
                UserID = dMTAPIRequest.UserID,
                OutletID = dMTAPIRequest.OutletID,
                OID = validateRes.OID,
                ReferenceID = dMTAPIRequest.ReferenceID,
                OTP = dMTAPIRequest.OTP
            });
            res.Statuscode = localRes.Statuscode;
            res.Message = localRes.Msg;
            res.ErrorCode = localRes.ErrorCode;
            return res;
        }
        public async Task<SenderCreateResponse> SenderResendOTP(DMTAPIRequest dMTAPIRequest)
        {
            var res = new SenderCreateResponse
            {
                ErrorCode = ErrorCodes.Invalid_Access,
                Message = nameof(ErrorCodes.Invalid_Access).Replace("_", " "),
                Statuscode = ErrorCodes.Minus1
            };
            #region RequestValidationFromCode  
            /**
             * Request validation from Code started
             * **/
            if (dMTAPIRequest.UserID < 2)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = "Unauthorised access!";
                return res;
            }
            if (dMTAPIRequest.OutletID < 10000)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(dMTAPIRequest.OutletID);
                return res;
            }
            if ((dMTAPIRequest.Token ?? string.Empty).Length != 32)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(dMTAPIRequest.Token);
                return res;
            }
            if (string.IsNullOrEmpty(dMTAPIRequest.SPKey))
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(dMTAPIRequest.SPKey);
                return res;
            }


            #endregion
            #region RequestValidationFromDB
            /**
             * Request validation from DB started
             * **/
            var validateReq = new ValidataRechargeApirequest
            {
                LoginID = dMTAPIRequest.UserID,
                IPAddress = _info.GetRemoteIP(),
                Token = dMTAPIRequest.Token,
                SPKey = dMTAPIRequest.SPKey
            };
            IProcedureAsync _procValidate = new ProcValidataRechargeApirequest(_dal);
            var validateRes = (ValidataRechargeApiResp)await _procValidate.Call(validateReq).ConfigureAwait(false);
            if (validateRes.Statuscode == ErrorCodes.Minus1)
            {
                res.Message = validateRes.Msg;
                res.ErrorCode = Convert.ToInt32(string.IsNullOrEmpty(validateRes.ErrorCode) ? "0" : validateRes.ErrorCode);
                return res;
            }
            #endregion
            IMoneyTransferML mtml = new MoneyTransferML(_accessor, _env);
            var localRes = mtml.SenderResendOTP(new MTOTPRequest
            {
                RequestMode = RequestMode.API,
                SenderMobile = dMTAPIRequest.SenderMobile,
                UserID = dMTAPIRequest.UserID,
                OutletID = dMTAPIRequest.OutletID,
                OID = validateRes.OID,
                ReferenceID = dMTAPIRequest.ReferenceID
            });
            res.Statuscode = localRes.Statuscode;
            res.Message = localRes.Msg;
            res.ErrorCode = localRes.ErrorCode;
            return res;
        }
        public async Task<SenderCreateResponse> CreateBeneficiary(CreateBeneReq dMTAPIRequest)
        {
            var res = new SenderCreateResponse
            {
                ErrorCode = ErrorCodes.Invalid_Access,
                Message = nameof(ErrorCodes.Invalid_Access).Replace("_", " "),
                Statuscode = ErrorCodes.Minus1
            };
            #region RequestValidationFromCode  
            /**
             * Request validation from Code started
             * **/
            if (dMTAPIRequest.UserID < 2)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = "Unauthorised access!";
                return res;
            }
            if (dMTAPIRequest.OutletID < 10000)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(dMTAPIRequest.OutletID);
                return res;
            }
            if ((dMTAPIRequest.Token ?? string.Empty).Length != 32)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(dMTAPIRequest.Token);
                return res;
            }
            if (string.IsNullOrEmpty(dMTAPIRequest.SPKey))
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(dMTAPIRequest.SPKey);
                return res;
            }

            #endregion
            #region RequestValidationFromDB
            /**
             * Request validation from DB started
             * **/
            var validateReq = new ValidataRechargeApirequest
            {
                LoginID = dMTAPIRequest.UserID,
                IPAddress = _info.GetRemoteIP(),
                Token = dMTAPIRequest.Token,
                SPKey = dMTAPIRequest.SPKey
            };
            IProcedureAsync _procValidate = new ProcValidataRechargeApirequest(_dal);
            var validateRes = (ValidataRechargeApiResp)await _procValidate.Call(validateReq).ConfigureAwait(false);
            if (validateRes.Statuscode == ErrorCodes.Minus1)
            {
                res.Message = validateRes.Msg;
                res.ErrorCode = Convert.ToInt32(string.IsNullOrEmpty(validateRes.ErrorCode) ? "0" : validateRes.ErrorCode);
                return res;
            }
            #endregion
            IMoneyTransferML mtml = new MoneyTransferML(_accessor, _env);
            var localRes = mtml.CreateBeneficiary(new MTBeneficiaryAddRequest
            {
                RequestMode = RequestMode.API,
                SenderMobile = dMTAPIRequest.SenderMobile,
                UserID = dMTAPIRequest.UserID,
                OutletID = dMTAPIRequest.OutletID,
                OID = validateRes.OID,
                ReferenceID = dMTAPIRequest.ReferenceID,
                BeneDetail = new MBeneDetail
                {
                    AccountNo = dMTAPIRequest.BeneAccountNumber,
                    BankID = dMTAPIRequest.BankID,
                    BankName = dMTAPIRequest.BankName,
                    BeneName = dMTAPIRequest.BeneName,
                    IFSC = dMTAPIRequest.IFSC,
                    MobileNo = dMTAPIRequest.BeneMobile,
                    TransMode = dMTAPIRequest.TransMode
                }
            });
            res.Statuscode = localRes.Statuscode;
            res.Message = localRes.Msg;
            res.ErrorCode = localRes.ErrorCode;
            res.IsOTPRequired = localRes.IsOTPGenerated;
            res.BeneID = localRes.BeneID;
            return res;
        }
        public async Task<APIDMTBeneficiaryResponse> GetBeneficiary(DMTAPIRequest dMTAPIRequest)
        {
            var res = new APIDMTBeneficiaryResponse
            {
                ErrorCode = ErrorCodes.Invalid_Access,
                Message = nameof(ErrorCodes.Invalid_Access).Replace("_", " "),
                Statuscode = ErrorCodes.Minus1
            };
            #region RequestValidationFromCode  
            /**
             * Request validation from Code started
             * **/
            if (dMTAPIRequest.UserID < 2)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = "Unauthorised access!";
                return res;
            }
            if (dMTAPIRequest.OutletID < 10000)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(dMTAPIRequest.OutletID);
                return res;
            }
            if ((dMTAPIRequest.Token ?? string.Empty).Length != 32)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(dMTAPIRequest.Token);
                return res;
            }
            if (string.IsNullOrEmpty(dMTAPIRequest.SPKey))
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(dMTAPIRequest.SPKey);
                return res;
            }
            #endregion
            #region RequestValidationFromDB
            /**
             * Request validation from DB started
             * **/
            var validateReq = new ValidataRechargeApirequest
            {
                LoginID = dMTAPIRequest.UserID,
                IPAddress = _info.GetRemoteIP(),
                Token = dMTAPIRequest.Token,
                SPKey = dMTAPIRequest.SPKey
            };
            IProcedureAsync _procValidate = new ProcValidataRechargeApirequest(_dal);
            var validateRes = (ValidataRechargeApiResp)await _procValidate.Call(validateReq).ConfigureAwait(false);
            if (validateRes.Statuscode == ErrorCodes.Minus1)
            {
                res.Message = validateRes.Msg;
                res.ErrorCode = Convert.ToInt32(string.IsNullOrEmpty(validateRes.ErrorCode) ? "0" : validateRes.ErrorCode);
                return res;
            }
            #endregion
            IMoneyTransferML mtml = new MoneyTransferML(_accessor, _env);
            var localRes = mtml.GetBeneficiary(new MTCommonRequest
            {
                RequestMode = RequestMode.API,
                SenderMobile = dMTAPIRequest.SenderMobile,
                UserID = dMTAPIRequest.UserID,
                OutletID = dMTAPIRequest.OutletID,
                OID = validateRes.OID,
                ReferenceID = dMTAPIRequest.ReferenceID
            });
            res.Statuscode = localRes.Statuscode;
            res.Message = localRes.Msg;
            res.ErrorCode = localRes.ErrorCode;
            res.Beneficiaries = localRes.Beneficiaries;
            return res;
        }
        public async Task<SenderCreateResponse> RemoveBeneficiary(DeleteBeneReq dMTAPIRequest)
        {
            var res = new SenderCreateResponse
            {
                ErrorCode = ErrorCodes.Invalid_Access,
                Message = nameof(ErrorCodes.Invalid_Access).Replace("_", " "),
                Statuscode = ErrorCodes.Minus1
            };
            #region RequestValidationFromCode  
            /**
             * Request validation from Code started
             * **/
            if (dMTAPIRequest.UserID < 2)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = "Unauthorised access!";
                return res;
            }
            if (dMTAPIRequest.OutletID < 10000)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(dMTAPIRequest.OutletID);
                return res;
            }
            if ((dMTAPIRequest.Token ?? string.Empty).Length != 32)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(dMTAPIRequest.Token);
                return res;
            }
            if (string.IsNullOrEmpty(dMTAPIRequest.SPKey))
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(dMTAPIRequest.SPKey);
                return res;
            }
            #endregion
            #region RequestValidationFromDB
            /**
             * Request validation from DB started
             * **/
            var validateReq = new ValidataRechargeApirequest
            {
                LoginID = dMTAPIRequest.UserID,
                IPAddress = _info.GetRemoteIP(),
                Token = dMTAPIRequest.Token,
                SPKey = dMTAPIRequest.SPKey
            };
            IProcedureAsync _procValidate = new ProcValidataRechargeApirequest(_dal);
            var validateRes = (ValidataRechargeApiResp)await _procValidate.Call(validateReq).ConfigureAwait(false);
            if (validateRes.Statuscode == ErrorCodes.Minus1)
            {
                res.Message = validateRes.Msg;
                res.ErrorCode = Convert.ToInt32(string.IsNullOrEmpty(validateRes.ErrorCode) ? "0" : validateRes.ErrorCode);
                return res;
            }
            #endregion
            IMoneyTransferML mtml = new MoneyTransferML(_accessor, _env);
            var localRes = mtml.RemoveBeneficiary(new MBeneVerifyRequest
            {
                RequestMode = RequestMode.API,
                SenderMobile = dMTAPIRequest.SenderMobile,
                UserID = dMTAPIRequest.UserID,
                OutletID = dMTAPIRequest.OutletID,
                OID = validateRes.OID,
                ReferenceID = dMTAPIRequest.ReferenceID,
                OTP = dMTAPIRequest.OTP,
                BeneficiaryID = dMTAPIRequest.BeneID
            });
            res.Statuscode = localRes.Statuscode;
            res.Message = localRes.Msg;
            res.ErrorCode = localRes.ErrorCode;
            res.IsOTPRequired = localRes.IsOTPGenerated;
            return res;
        }
        public async Task<SenderCreateResponse> GenerateOTPBeneficiary(DeleteBeneReq dMTAPIRequest)
        {
            var res = new SenderCreateResponse
            {
                ErrorCode = ErrorCodes.Invalid_Access,
                Message = nameof(ErrorCodes.Invalid_Access).Replace("_", " "),
                Statuscode = ErrorCodes.Minus1
            };
            #region RequestValidationFromCode  
            /**
             * Request validation from Code started
             * **/
            if (dMTAPIRequest.UserID < 2)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = "Unauthorised access!";
                return res;
            }
            if (dMTAPIRequest.OutletID < 10000)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(dMTAPIRequest.OutletID);
                return res;
            }
            if ((dMTAPIRequest.Token ?? string.Empty).Length != 32)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(dMTAPIRequest.Token);
                return res;
            }
            if (string.IsNullOrEmpty(dMTAPIRequest.SPKey))
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(dMTAPIRequest.SPKey);
                return res;
            }
            #endregion
            #region RequestValidationFromDB
            /**
             * Request validation from DB started
             * **/
            var validateReq = new ValidataRechargeApirequest
            {
                LoginID = dMTAPIRequest.UserID,
                IPAddress = _info.GetRemoteIP(),
                Token = dMTAPIRequest.Token,
                SPKey = dMTAPIRequest.SPKey
            };
            IProcedureAsync _procValidate = new ProcValidataRechargeApirequest(_dal);
            var validateRes = (ValidataRechargeApiResp)await _procValidate.Call(validateReq).ConfigureAwait(false);
            if (validateRes.Statuscode == ErrorCodes.Minus1)
            {
                res.Message = validateRes.Msg;
                res.ErrorCode = Convert.ToInt32(string.IsNullOrEmpty(validateRes.ErrorCode) ? "0" : validateRes.ErrorCode);
                return res;
            }
            #endregion
            IMoneyTransferML mtml = new MoneyTransferML(_accessor, _env);
            var localRes = mtml.GenerateOTP(new MTBeneficiaryAddRequest
            {
                RequestMode = RequestMode.API,
                SenderMobile = dMTAPIRequest.SenderMobile,
                UserID = dMTAPIRequest.UserID,
                OutletID = dMTAPIRequest.OutletID,
                OID = validateRes.OID,
                ReferenceID = dMTAPIRequest.ReferenceID,
                BeneDetail = new MBeneDetail
                {
                    BeneID = dMTAPIRequest.BeneID
                }
            });
            res.Statuscode = localRes.Statuscode;
            res.Message = localRes.Msg;
            res.ErrorCode = localRes.ErrorCode;
            res.IsOTPRequired = localRes.IsOTPGenerated;
            res.IsOTPResendAvailble = localRes.IsOTPResendAvailble;
            return res;
        }
        public async Task<DMTAPIResponse> ValidateBeneficiaryOTP(DMTTransactionReq dMTAPIRequest)
        {
            var res = new DMTAPIResponse
            {
                ErrorCode = ErrorCodes.Invalid_Access,
                Message = nameof(ErrorCodes.Invalid_Access).Replace("_", " "),
                Statuscode = ErrorCodes.Minus1
            };
            #region RequestValidationFromCode  
            /**
             * Request validation from Code started
             * **/
            if (dMTAPIRequest.UserID < 2)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = "Unauthorised access!";
                return res;
            }
            if (dMTAPIRequest.OutletID < 10000)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(dMTAPIRequest.OutletID);
                return res;
            }
            if ((dMTAPIRequest.Token ?? string.Empty).Length != 32)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(dMTAPIRequest.Token);
                return res;
            }
            if (string.IsNullOrEmpty(dMTAPIRequest.SPKey))
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(dMTAPIRequest.SPKey);
                return res;
            }


            #endregion
            #region RequestValidationFromDB
            /**
             * Request validation from DB started
             * **/
            var validateReq = new ValidataRechargeApirequest
            {
                LoginID = dMTAPIRequest.UserID,
                IPAddress = _info.GetRemoteIP(),
                Token = dMTAPIRequest.Token,
                SPKey = dMTAPIRequest.SPKey
            };
            IProcedureAsync _procValidate = new ProcValidataRechargeApirequest(_dal);
            var validateRes = (ValidataRechargeApiResp)await _procValidate.Call(validateReq).ConfigureAwait(false);
            if (validateRes.Statuscode == ErrorCodes.Minus1)
            {
                res.Message = validateRes.Msg;
                res.ErrorCode = Convert.ToInt32(string.IsNullOrEmpty(validateRes.ErrorCode) ? "0" : validateRes.ErrorCode);
                return res;
            }
            #endregion
            IMoneyTransferML mtml = new MoneyTransferML(_accessor, _env);
            var localRes = mtml.ValidateBeneficiary(new MBeneVerifyRequest
            {
                RequestMode = RequestMode.API,
                SenderMobile = dMTAPIRequest.SenderMobile,
                UserID = dMTAPIRequest.UserID,
                OutletID = dMTAPIRequest.OutletID,
                OID = validateRes.OID,
                MobileNo = dMTAPIRequest.BeneMobile,
                AccountNo = dMTAPIRequest.BeneAccountNumber,
                OTP = dMTAPIRequest.OTP,
                ReferenceID = dMTAPIRequest.ReferenceID,
                BeneficiaryID = dMTAPIRequest.BeneID
            });
            res.Statuscode = localRes.Statuscode;
            res.Message = localRes.Msg;
            res.ErrorCode = localRes.ErrorCode;
            return res;
        }
        public async Task<SenderCreateResponse> ValidateRemoveBeneficiaryOTP(DMTTransactionReq dMTAPIRequest)
        {
            var res = new SenderCreateResponse
            {
                ErrorCode = ErrorCodes.Invalid_Access,
                Message = nameof(ErrorCodes.Invalid_Access).Replace("_", " "),
                Statuscode = ErrorCodes.Minus1
            };
            #region RequestValidationFromCode  
            /**
             * Request validation from Code started
             * **/
            if (dMTAPIRequest.UserID < 2)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = "Unauthorised access!";
                return res;
            }
            if (dMTAPIRequest.OutletID < 10000)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(dMTAPIRequest.OutletID);
                return res;
            }
            if ((dMTAPIRequest.Token ?? string.Empty).Length != 32)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(dMTAPIRequest.Token);
                return res;
            }
            if (string.IsNullOrEmpty(dMTAPIRequest.SPKey))
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(dMTAPIRequest.SPKey);
                return res;
            }


            #endregion
            #region RequestValidationFromDB
            /**
             * Request validation from DB started
             * **/
            var validateReq = new ValidataRechargeApirequest
            {
                LoginID = dMTAPIRequest.UserID,
                IPAddress = _info.GetRemoteIP(),
                Token = dMTAPIRequest.Token,
                SPKey = dMTAPIRequest.SPKey
            };
            IProcedureAsync _procValidate = new ProcValidataRechargeApirequest(_dal);
            var validateRes = (ValidataRechargeApiResp)await _procValidate.Call(validateReq).ConfigureAwait(false);
            if (validateRes.Statuscode == ErrorCodes.Minus1)
            {
                res.Message = validateRes.Msg;
                res.ErrorCode = Convert.ToInt32(string.IsNullOrEmpty(validateRes.ErrorCode) ? "0" : validateRes.ErrorCode);
                return res;
            }
            #endregion
            IMoneyTransferML mtml = new MoneyTransferML(_accessor, _env);
            var localRes = mtml.ValidateRemoveBeneficiary(new MBeneVerifyRequest
            {
                RequestMode = RequestMode.API,
                SenderMobile = dMTAPIRequest.SenderMobile,
                UserID = dMTAPIRequest.UserID,
                OutletID = dMTAPIRequest.OutletID,
                OID = validateRes.OID,
                MobileNo = dMTAPIRequest.BeneMobile,
                AccountNo = dMTAPIRequest.BeneAccountNumber,
                OTP = dMTAPIRequest.OTP,
                ReferenceID = dMTAPIRequest.ReferenceID,
                BeneficiaryID = dMTAPIRequest.BeneID
            });
            res.Statuscode = localRes.Statuscode;
            res.Message = localRes.Msg;
            res.ErrorCode = localRes.ErrorCode;
            res.IsOTPRequired = localRes.IsOTPGenerated;
            return res;
        }
        public async Task<DMTTransactionResponse> VerifyAccount(DMTTransactionReq dMTAPIRequest)
        {
            var res = new DMTTransactionResponse
            {
                ErrorCode = ErrorCodes.Invalid_Access,
                Message = nameof(ErrorCodes.Invalid_Access).Replace("_", " "),
                Statuscode = ErrorCodes.Minus1
            };
            #region RequestValidationFromCode  
            /**
             * Request validation from Code started
             * **/
            if (dMTAPIRequest.UserID < 2)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = "Unauthorised access!";
                return res;
            }
            if (dMTAPIRequest.OutletID < 10000)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(dMTAPIRequest.OutletID);
                return res;
            }
            if ((dMTAPIRequest.Token ?? string.Empty).Length != 32)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(dMTAPIRequest.Token);
                return res;
            }
            if (string.IsNullOrEmpty(dMTAPIRequest.SPKey))
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(dMTAPIRequest.SPKey);
                return res;
            }
            #endregion
            #region RequestValidationFromDB
            /**
             * Request validation from DB started
             * **/
            var validateReq = new ValidataRechargeApirequest
            {
                LoginID = dMTAPIRequest.UserID,
                IPAddress = _info.GetRemoteIP(),
                Token = dMTAPIRequest.Token,
                SPKey = dMTAPIRequest.SPKey
            };
            IProcedureAsync _procValidate = new ProcValidataRechargeApirequest(_dal);
            var validateRes = (ValidataRechargeApiResp)await _procValidate.Call(validateReq).ConfigureAwait(false);
            if (validateRes.Statuscode == ErrorCodes.Minus1)
            {
                res.Message = validateRes.Msg;
                res.ErrorCode = Convert.ToInt32(string.IsNullOrEmpty(validateRes.ErrorCode) ? "0" : validateRes.ErrorCode);
                return res;
            }
            #endregion
            IMoneyTransferML mtml = new MoneyTransferML(_accessor, _env);
            var localRes = mtml.VerifyAccount(new MBeneVerifyRequest
            {
                RequestMode = RequestMode.API,
                SenderMobile = dMTAPIRequest.SenderMobile,
                UserID = dMTAPIRequest.UserID,
                OutletID = dMTAPIRequest.OutletID,
                OID = validateRes.OID,
                AccountNo = dMTAPIRequest.BeneAccountNumber,
                IFSC = dMTAPIRequest.IFSC,
                BankID = dMTAPIRequest.BankID,
                Bank = dMTAPIRequest.BankName,
                BeneficiaryName = dMTAPIRequest.BeneName,
                TransMode = dMTAPIRequest.TransMode == 1 ? "IMPS" : "NEFT",
                ReferenceID = dMTAPIRequest.ReferenceID,
                APIRequestID = dMTAPIRequest.APIRequestID
            });
            res.Status = localRes.Statuscode;
            res.Statuscode = localRes.Statuscode > 0 ? ErrorCodes.One : localRes.Statuscode;
            res.Message = localRes.Msg;
            res.ErrorCode = localRes.ErrorCode;
            res.BeneName = localRes.BeneName;
            res.LiveID = localRes.LiveID;
            res.RPID = localRes.TransactionID;
            return res;
        }
        public async Task<DMTTransactionResponse> AccountTransfer(DMTTransactionReq dMTAPIRequest)
        {
            var res = new DMTTransactionResponse
            {
                ErrorCode = ErrorCodes.Invalid_Access,
                Message = nameof(ErrorCodes.Invalid_Access).Replace("_", " "),
                Statuscode = ErrorCodes.Minus1
            };
            #region RequestValidationFromCode  
            /**
             * Request validation from Code started
             * **/
            if (dMTAPIRequest.UserID < 2)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = "Unauthorised access!";
                return res;
            }
            if (dMTAPIRequest.OutletID < 10000)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(dMTAPIRequest.OutletID);
                return res;
            }
            if ((dMTAPIRequest.Token ?? string.Empty).Length != 32)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(dMTAPIRequest.Token);
                return res;
            }
            if (string.IsNullOrEmpty(dMTAPIRequest.SPKey))
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(dMTAPIRequest.SPKey);
                return res;
            }
            if (!Validate.O.IsNumeric(dMTAPIRequest.BeneID ?? string.Empty) || (dMTAPIRequest.BeneID ?? string.Empty) == "0")
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(dMTAPIRequest.BeneID);
                return res;
            }
            if (dMTAPIRequest.TransMode < 1)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(dMTAPIRequest.TransMode);
                return res;
            }
            //if (dMTAPIRequest.BankID < 1)
            //{
            //    res.ErrorCode = ErrorCodes.Invalid_Parameter;
            //    res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(dMTAPIRequest.BankID);
            //    return res;
            //}

            #endregion
            #region RequestValidationFromDB
            /**
             * Request validation from DB started
             * **/
            var validateReq = new ValidataRechargeApirequest
            {
                LoginID = dMTAPIRequest.UserID,
                IPAddress = _info.GetRemoteIP(),
                Token = dMTAPIRequest.Token,
                SPKey = dMTAPIRequest.SPKey
            };
            IProcedureAsync _procValidate = new ProcValidataRechargeApirequest(_dal);
            var validateRes = (ValidataRechargeApiResp)await _procValidate.Call(validateReq).ConfigureAwait(false);
            if (validateRes.Statuscode == ErrorCodes.Minus1)
            {
                res.Message = validateRes.Msg;
                res.ErrorCode = Convert.ToInt32(string.IsNullOrEmpty(validateRes.ErrorCode) ? "0" : validateRes.ErrorCode);
                return res;
            }
            #endregion
            IMoneyTransferML mtml = new MoneyTransferML(_accessor, _env);
            var localRes = mtml.AccountTransfer(new MBeneVerifyRequest
            {
                RequestMode = RequestMode.API,
                SenderMobile = dMTAPIRequest.SenderMobile,
                UserID = dMTAPIRequest.UserID,
                OutletID = dMTAPIRequest.OutletID,
                OID = validateRes.OID,
                AccountNo = dMTAPIRequest.BeneAccountNumber,
                IFSC = dMTAPIRequest.IFSC,
                BankID = dMTAPIRequest.BankID,
                Bank = dMTAPIRequest.BankName,
                Amount = dMTAPIRequest.Amount,
                BeneficiaryName = dMTAPIRequest.BeneName,
                TransMode = dMTAPIRequest.TransMode == 1 ? "IMPS" : "NEFT",
                ReferenceID = dMTAPIRequest.ReferenceID,
                BeneficiaryID = dMTAPIRequest.BeneID,
                APIRequestID = dMTAPIRequest.APIRequestID
            });
            res.Status = localRes.Statuscode;
            res.Statuscode = localRes.Statuscode > 0 ? ErrorCodes.One : localRes.Statuscode;
            res.Message = localRes.Msg;
            res.ErrorCode = localRes.ErrorCode;
            res.BeneName = localRes.BeneName;
            res.LiveID = localRes.LiveID;
            res.RPID = localRes.TransactionID;
            return res;
        }

        public async Task<DMTTransactionResponse> DoUPIPayment(DMTTransactionReq dMTAPIRequest)
        {
            var res = new DMTTransactionResponse
            {
                ErrorCode = ErrorCodes.Invalid_Access,
                Message = nameof(ErrorCodes.Invalid_Access).Replace("_", " "),
                Statuscode = ErrorCodes.Minus1
            };
            #region RequestValidationFromCode  
            /**
             * Request validation from Code started
             * **/
            if (dMTAPIRequest.UserID < 2)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = "Unauthorised access!";
                return res;
            }
            if ((dMTAPIRequest.Token ?? string.Empty).Length != 32)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(dMTAPIRequest.Token);
                return res;
            }
            //if (string.IsNullOrEmpty(dMTAPIRequest.SPKey))
            //{
            //    res.ErrorCode = ErrorCodes.Invalid_Parameter;
            //    res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(dMTAPIRequest.SPKey);
            //    return res;
            //}


            #endregion
            #region RequestValidationFromDB
            /**
             * Request validation from DB started
             * **/
            var validateReq = new ValidataRechargeApirequest
            {
                LoginID = dMTAPIRequest.UserID,
                IPAddress = _info.GetRemoteIP(),
                Token = dMTAPIRequest.Token,
                SPKey = "SPKey"
            };
            IProcedureAsync _procValidate = new ProcValidataRechargeApirequest(_dal);
            var validateRes = (ValidataRechargeApiResp)await _procValidate.Call(validateReq).ConfigureAwait(false);
            if (validateRes.Statuscode == ErrorCodes.Minus1)
            {
                res.Message = validateRes.Msg;
                res.ErrorCode = Convert.ToInt32(string.IsNullOrEmpty(validateRes.ErrorCode) ? "0" : validateRes.ErrorCode);
                return res;
            }
            #endregion
            IMoneyTransferML mtml = new MoneyTransferML(_accessor, _env);
            var localRes = mtml.DoUPIPaymentService(new MBeneVerifyRequest
            {
                RequestMode = RequestMode.API,
                UserID = dMTAPIRequest.UserID,
                AccountNo = dMTAPIRequest.BeneAccountNumber,
                Amount = dMTAPIRequest.Amount,
                BeneficiaryName = dMTAPIRequest.BeneName,
                APIRequestID = dMTAPIRequest.APIRequestID
            });
            res.Status = localRes.Statuscode;
            res.Statuscode = localRes.Statuscode > 0 ? ErrorCodes.One : localRes.Statuscode;
            res.Message = localRes.Msg;
            res.ErrorCode = localRes.ErrorCode;
            res.BeneName = localRes.BeneName;
            res.LiveID = localRes.LiveID;
            res.RPID = localRes.TransactionID;
            return res;
        }
    }
}
