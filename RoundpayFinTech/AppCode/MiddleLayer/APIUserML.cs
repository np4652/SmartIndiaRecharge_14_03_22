using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer.Dmt_Api;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Recharge;
using System;
using System.Linq;
using System.Threading.Tasks;
using Validators;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public sealed partial class APIUserML : IAPIUserMiddleLayer
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        private readonly IRequestInfo _info;
        public APIUserML(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            _info = new RequestInfo(_accessor, _env);
        }
        public async Task<object> TestUpdateTransaction(TransactionStatus transactionStatus)
        {
            IProcedureAsync _updateProc = new ProcUpdateTransactionServiceStatus(_dal);
            return (_CallbackData)await _updateProc.Call(transactionStatus);
        }
        #region DMRRegion
        public async Task<SenderLoginResponse> SenderLogin(DMTAPIRequest dMTAPIRequest)
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
                SPKey = "DMT"
            };
            IProcedureAsync _procValidate = new ProcValidataRechargeApirequest(_dal);
            var validateRes = (ValidataRechargeApiResp)await _procValidate.Call(validateReq);
            if (validateRes.Statuscode == ErrorCodes.Minus1)
            {
                res.Message = validateRes.Msg;
                res.ErrorCode = Convert.ToInt32(validateRes.ErrorCode ?? "0");
                return res;
            }
            #endregion
            IDmtML dmtML = new DmtML(_accessor, _env, _dal, _info);
            var mlReq = new DMTReq
            {
                UserID = dMTAPIRequest.UserID,
                LT = LoginType.ApplicationUser,
                RequestMode = RequestMode.API,
                OutletID = dMTAPIRequest.OutletID,
                IsValidate = false,
                SenderNO = dMTAPIRequest.SenderMobile
            };

            var mlResp = await dmtML.CheckSender(mlReq).ConfigureAwait(false);
            res.Statuscode = mlResp.Statuscode;
            res.Message = mlResp.Msg;
            res.ErrorCode = mlResp.ErrorCode;
            res.IsSenderNotExists = mlResp.CommonInt == ErrorCodes.One;
            res.SenderName = mlResp.CommonStr2 ?? string.Empty;
            res.SenderMobile = dMTAPIRequest.SenderMobile;
            res.AvailbleLimit = Validate.O.IsNumeric((mlResp.CommonStr ?? string.Empty).Replace(".", "")) ? Convert.ToDecimal(mlResp.CommonStr) : 0;
            res.ReferenceID = mlResp.ReffID ?? string.Empty;
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
            if ((dMTAPIRequest.FirstName ?? string.Empty).Trim().Length < 2)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(dMTAPIRequest.FirstName);
                return res;
            }
            if ((dMTAPIRequest.LastName ?? string.Empty).Trim().Length < 2)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(dMTAPIRequest.LastName);
                return res;
            }
            if ((dMTAPIRequest.SenderMobile ?? string.Empty).Trim().Length != 10)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(dMTAPIRequest.SenderMobile);
                return res;
            }
            if (!Validate.O.IsDateIn_dd_MMM_yyyy_Format(dMTAPIRequest.DOB ?? string.Empty))
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(dMTAPIRequest.DOB);
                return res;
            }
            if ((dMTAPIRequest.Pincode ?? string.Empty).Length != 6)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(dMTAPIRequest.Pincode);
                return res;
            }
            if ((dMTAPIRequest.Address ?? string.Empty).Trim().Length < 5)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(dMTAPIRequest.Address);
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
                SPKey = "DMT"
            };
            IProcedureAsync _procValidate = new ProcValidataRechargeApirequest(_dal);
            var validateRes = (ValidataRechargeApiResp)await _procValidate.Call(validateReq);
            if (validateRes.Statuscode == ErrorCodes.Minus1)
            {
                res.Message = validateRes.Msg;
                res.ErrorCode = Convert.ToInt32(validateRes.ErrorCode ?? "0");
                return res;
            }
            #endregion
            IDmtML dmtML = new DmtML(_accessor, _env, _dal, _info);
            var dMTReq = new DMTReq
            {
                UserID = dMTAPIRequest.UserID,
                LT = LoginType.ApplicationUser,
                RequestMode = RequestMode.API,
                OutletID = dMTAPIRequest.OutletID,
                IsValidate = false,
                SenderNO = dMTAPIRequest.SenderMobile
            };
            var senderReq = new SenderRequest
            {
                UserID = dMTAPIRequest.UserID,
                Name = dMTAPIRequest.FirstName,
                LastName = dMTAPIRequest.LastName,
                MobileNo = dMTAPIRequest.SenderMobile,
                Pincode = dMTAPIRequest.Pincode,
                Address = dMTAPIRequest.Address,
                OTP = dMTAPIRequest.OTP ?? string.Empty,
                Dob = dMTAPIRequest.DOB
            };
            var mlReq = new CreateSen
            {
                dMTReq = dMTReq,
                senderRequest = senderReq
            };
            var mlResp = await dmtML.CreateSender(mlReq);
            res.Statuscode = mlResp.Statuscode;
            res.Message = mlResp.Msg;
            res.ErrorCode = mlResp.ErrorCode;
            res.IsOTPRequired = mlResp.Statuscode == ErrorCodes.One && mlResp.CommonInt == ErrorCodes.One;
            res.ReferenceID = mlResp.ReffID ?? string.Empty;
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
            if ((dMTAPIRequest.OTP ?? string.Empty).Length < 3)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(dMTAPIRequest.OTP);
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
                SPKey = "DMT"
            };
            IProcedureAsync _procValidate = new ProcValidataRechargeApirequest(_dal);
            var validateRes = (ValidataRechargeApiResp)await _procValidate.Call(validateReq);
            if (validateRes.Statuscode == ErrorCodes.Minus1)
            {
                res.Message = validateRes.Msg;
                res.ErrorCode = Convert.ToInt32(validateRes.ErrorCode ?? "0");
                return res;
            }
            #endregion
            IDmtML dmtML = new DmtML(_accessor, _env, _dal, _info);
            var mlReq = new DMTReq
            {
                UserID = dMTAPIRequest.UserID,
                LT = LoginType.ApplicationUser,
                RequestMode = RequestMode.API,
                OutletID = dMTAPIRequest.OutletID,
                IsValidate = false,
                SenderNO = dMTAPIRequest.SenderMobile,
                ReffID = dMTAPIRequest.ReferenceID
            };

            var mlResp = await dmtML.VerifySender(mlReq, dMTAPIRequest.OTP);
            res.Statuscode = mlResp.Statuscode;
            res.Message = mlResp.Msg;
            res.ErrorCode = mlResp.ErrorCode;
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
                SPKey = "DMT"
            };
            IProcedureAsync _procValidate = new ProcValidataRechargeApirequest(_dal);
            var validateRes = (ValidataRechargeApiResp)await _procValidate.Call(validateReq);
            if (validateRes.Statuscode == ErrorCodes.Minus1)
            {
                res.Message = validateRes.Msg;
                res.ErrorCode = Convert.ToInt32(validateRes.ErrorCode ?? "0");
                return res;
            }
            #endregion
            IDmtML dmtML = new DmtML(_accessor, _env, _dal, _info);
            var mlReq = new DMTReq
            {
                UserID = dMTAPIRequest.UserID,
                LT = LoginType.ApplicationUser,
                RequestMode = RequestMode.API,
                OutletID = dMTAPIRequest.OutletID,
                IsValidate = false,
                SenderNO = dMTAPIRequest.SenderMobile,
                ReffID = dMTAPIRequest.ReferenceID
            };
            var addBene = new AddBeni
            {
                BeneName = dMTAPIRequest.BeneName ?? string.Empty,
                MobileNo = dMTAPIRequest.BeneMobile ?? string.Empty,
                AccountNo = dMTAPIRequest.BeneAccountNumber ?? string.Empty,
                BankName = dMTAPIRequest.BankName ?? "",
                IFSC = dMTAPIRequest.IFSC ?? "",
                SenderMobileNo = dMTAPIRequest.SenderMobile ?? string.Empty,
                BankID = dMTAPIRequest.BankID
            };
            var mlResp = await dmtML.CreateBeneficiary(addBene, mlReq);
            res.Statuscode = mlResp.Statuscode;
            res.Message = mlResp.Msg;
            res.ErrorCode = mlResp.ErrorCode;
            res.ReferenceID = mlResp.ReffID;
            res.IsOTPRequired = mlResp.Statuscode == ErrorCodes.One && mlResp.CommonInt == ErrorCodes.One && (mlResp.ReffID ?? string.Empty) != string.Empty;
            return res;
        }
        public async Task<SenderCreateResponse> DeleteBeneficiary(DeleteBeneReq dMTAPIRequest)
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
                SPKey = "DMT"
            };
            IProcedureAsync _procValidate = new ProcValidataRechargeApirequest(_dal);
            var validateRes = (ValidataRechargeApiResp)await _procValidate.Call(validateReq);
            if (validateRes.Statuscode == ErrorCodes.Minus1)
            {
                res.Message = validateRes.Msg;
                res.ErrorCode = Convert.ToInt32(validateRes.ErrorCode ?? "0");
                return res;
            }
            #endregion
            IDmtML dmtML = new DmtML(_accessor, _env, _dal, _info);
            var mlReq = new DMTReq
            {
                UserID = dMTAPIRequest.UserID,
                LT = LoginType.ApplicationUser,
                RequestMode = RequestMode.API,
                OutletID = dMTAPIRequest.OutletID,
                IsValidate = false,
                SenderNO = dMTAPIRequest.SenderMobile,
                ReffID = dMTAPIRequest.ReferenceID
            };
            var mlResp = await dmtML.DeleteBeneficiary(mlReq, dMTAPIRequest.BeneID, dMTAPIRequest.OTP);
            res.Statuscode = mlResp.Statuscode;
            res.Message = mlResp.Msg;
            res.ErrorCode = mlResp.ErrorCode;
            res.IsOTPRequired = mlResp.CommonBool;
            return res;
        }
        public async Task<BeneResponse> GetBeneficiary(DMTAPIRequest dMTAPIRequest)
        {
            var res = new BeneResponse
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
                SPKey = "DMT"
            };
            IProcedureAsync _procValidate = new ProcValidataRechargeApirequest(_dal);
            var validateRes = (ValidataRechargeApiResp)await _procValidate.Call(validateReq);
            if (validateRes.Statuscode == ErrorCodes.Minus1)
            {
                res.Message = validateRes.Msg;
                res.ErrorCode = Convert.ToInt32(validateRes.ErrorCode ?? "0");
                return res;
            }
            #endregion
            IDmtML dmtML = new DmtML(_accessor, _env, _dal, _info);
            var mlReq = new DMTReq
            {
                UserID = dMTAPIRequest.UserID,
                LT = LoginType.ApplicationUser,
                RequestMode = RequestMode.API,
                OutletID = dMTAPIRequest.OutletID,
                IsValidate = false,
                SenderNO = dMTAPIRequest.SenderMobile,
                ReffID = dMTAPIRequest.ReferenceID
            };
            var mlResp = await dmtML.GetBeneficiary(mlReq);
            if (mlResp.addBeni != null)
            {
                if (mlResp.addBeni.Count > 0)
                {
                    res.Beneficiaries = mlResp.addBeni.Select(x => new APIBeneficary { BankID = x.BankID, BankName = x.BankName, BeneAccountNumber = x.AccountNo, BeneID = x.BeneID, BeneMobile = x.MobileNo, BeneName = x.BeneName, IFSC = x.IFSC, IsVerified = x.IsVerified });
                }
            }
            res.Statuscode = mlResp.Statuscode;
            res.Message = mlResp.Msg;
            res.ErrorCode = mlResp.ErrorCode;
            return res;
        }
        public async Task<SenderCreateResponse> GenerateBenficiaryOTP(DMTAPIRequest dMTAPIRequest)
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
                SPKey = "DMT"
            };
            IProcedureAsync _procValidate = new ProcValidataRechargeApirequest(_dal);
            var validateRes = (ValidataRechargeApiResp)await _procValidate.Call(validateReq);
            if (validateRes.Statuscode == ErrorCodes.Minus1)
            {
                res.Message = validateRes.Msg;
                res.ErrorCode = Convert.ToInt32(validateRes.ErrorCode ?? "0");
                return res;
            }
            #endregion
            IDmtML dmtML = new DmtML(_accessor, _env, _dal, _info);
            var mlReq = new DMTReq
            {
                UserID = dMTAPIRequest.UserID,
                LT = LoginType.ApplicationUser,
                RequestMode = RequestMode.API,
                OutletID = dMTAPIRequest.OutletID,
                IsValidate = false,
                SenderNO = dMTAPIRequest.SenderMobile
            };
            var mlResp = await dmtML.GenerateOTP(mlReq);
            res.Statuscode = mlResp.Statuscode;
            res.Message = mlResp.Msg;
            res.ErrorCode = mlResp.ErrorCode;
            res.ReferenceID = mlResp.ReffID ?? string.Empty;
            return res;
        }
        public async Task<DMTAPIResponse> ValidateBeneficiary(CreateBeneReq dMTAPIRequest)
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
                SPKey = "DMT"
            };
            IProcedureAsync _procValidate = new ProcValidataRechargeApirequest(_dal);
            var validateRes = (ValidataRechargeApiResp)await _procValidate.Call(validateReq);
            if (validateRes.Statuscode == ErrorCodes.Minus1)
            {
                res.Message = validateRes.Msg;
                res.ErrorCode = Convert.ToInt32(validateRes.ErrorCode ?? "0");
                return res;
            }
            #endregion
            IDmtML dmtML = new DmtML(_accessor, _env, _dal, _info);
            var mlReq = new DMTReq
            {
                UserID = dMTAPIRequest.UserID,
                LT = LoginType.ApplicationUser,
                RequestMode = RequestMode.API,
                OutletID = dMTAPIRequest.OutletID,
                IsValidate = false,
                SenderNO = dMTAPIRequest.SenderMobile,
                ReffID = dMTAPIRequest.ReferenceID
            };
            var mlResp = await dmtML.ValidateBeneficiary(mlReq, dMTAPIRequest.BeneMobile, dMTAPIRequest.BeneAccountNumber, dMTAPIRequest.OTP);

            res.Statuscode = mlResp.Statuscode;
            res.Message = mlResp.Msg;
            res.ErrorCode = mlResp.ErrorCode;
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
            if ((dMTAPIRequest.APIRequestID ?? string.Empty).Trim() == string.Empty)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(dMTAPIRequest.APIRequestID);
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
                SPKey = "DMT"
            };
            IProcedureAsync _procValidate = new ProcValidataRechargeApirequest(_dal);
            var validateRes = (ValidataRechargeApiResp)await _procValidate.Call(validateReq);
            if (validateRes.Statuscode == ErrorCodes.Minus1)
            {
                res.Message = validateRes.Msg;
                res.ErrorCode = Convert.ToInt32(validateRes.ErrorCode ?? "0");
                return res;
            }
            #endregion
            IDmtML dmtML = new DmtML(_accessor, _env, _dal, _info);
            var mlReq = new DMTReq
            {
                UserID = dMTAPIRequest.UserID,
                LT = LoginType.ApplicationUser,
                RequestMode = RequestMode.API,
                OutletID = dMTAPIRequest.OutletID,
                IsValidate = false,
                SenderNO = dMTAPIRequest.SenderMobile
            };
            var reqSendMoney = new ReqSendMoney
            {
                AccountNo = dMTAPIRequest.BeneAccountNumber,
                Amount = 0,
                BeneID = "",
                Channel = false,
                IFSC = dMTAPIRequest.IFSC,
                MobileNo = dMTAPIRequest.BeneMobile,
                Bank = dMTAPIRequest.BankName,
                BankID = dMTAPIRequest.BankID,
                BeneName = dMTAPIRequest.BeneName,
                APIRequestID = dMTAPIRequest.APIRequestID
            };
            var mlResp = await dmtML.Verification(mlReq, reqSendMoney);

            res.Message = mlResp.Msg;
            res.ErrorCode = mlResp.ErrorCode;
            res.Status = mlResp.Statuscode;
            res.Statuscode = mlResp.Statuscode > 0 ? ErrorCodes.One : mlResp.Statuscode;
            res.RPID = mlResp.TransactionID;
            res.LiveID = mlResp.LiveID;
            res.BeneName = mlResp.BeneName;
            return res;
        }
        public async Task<DMTTransactionResponse> SendMoney(DMTTransactionReq dMTAPIRequest)
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
            if ((dMTAPIRequest.APIRequestID ?? string.Empty).Trim() == string.Empty)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(dMTAPIRequest.APIRequestID);
                return res;
            }
            if (dMTAPIRequest.Amount > 5000)
            {
                res.ErrorCode = ErrorCodes.Invalid_Parameter;
                res.Message = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " " + nameof(dMTAPIRequest.Amount);
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
                SPKey = "DMT"
            };
            IProcedureAsync _procValidate = new ProcValidataRechargeApirequest(_dal);
            var validateRes = (ValidataRechargeApiResp)await _procValidate.Call(validateReq);
            if (validateRes.Statuscode == ErrorCodes.Minus1)
            {
                res.Message = validateRes.Msg;
                res.ErrorCode = Convert.ToInt32(validateRes.ErrorCode ?? "0");
                return res;
            }
            #endregion
            IDmtML dmtML = new DmtML(_accessor, _env, _dal, _info);
            var mlReq = new DMTReq
            {
                UserID = dMTAPIRequest.UserID,
                LT = LoginType.ApplicationUser,
                RequestMode = RequestMode.API,
                OutletID = dMTAPIRequest.OutletID,
                IsValidate = false,
                SenderNO = dMTAPIRequest.SenderMobile
            };
            var reqSendMoney = new ReqSendMoney
            {
                AccountNo = dMTAPIRequest.BeneAccountNumber,
                Amount = dMTAPIRequest.Amount,
                BeneID = dMTAPIRequest.BeneID,
                Channel = true,
                IFSC = dMTAPIRequest.IFSC,
                MobileNo = dMTAPIRequest.BeneMobile,
                Bank = dMTAPIRequest.BankName,
                BankID = dMTAPIRequest.BankID,
                BeneName = dMTAPIRequest.BeneName,
                APIRequestID = dMTAPIRequest.APIRequestID
            };
            var mlResp = await dmtML.SendMoney(mlReq, reqSendMoney).ConfigureAwait(false);

            res.Message = mlResp.Msg;
            res.ErrorCode = mlResp.ErrorCode;
            res.Status = mlResp.Statuscode;
            res.Statuscode = mlResp.Statuscode > 0 ? ErrorCodes.One : mlResp.Statuscode;
            res.RPID = mlResp.TransactionID;
            res.LiveID = mlResp.LiveID;
            res.BeneName = mlResp.BeneName;
            return res;
        }
        #endregion

        #region AEPSRegion
        public async Task<GenerateURLResponse> GenerateAEPSURL(PartnerAPIRequest aPIRequest)
        {
            var urlResponse = new GenerateURLResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            #region CodeValidation
            if (aPIRequest.UserID < 1)
            {
                urlResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " UserID";
                return urlResponse;
            }
            if (string.IsNullOrEmpty(aPIRequest.Token))
            {
                urlResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " Token";
                return urlResponse;
            }           
            if (aPIRequest.OutletID < 10000)
            {
                urlResponse.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " OutletID";
                return urlResponse;
            }
            #endregion
            ITransactionML transactionML = new TransactionML(_dal, _info);
            return await transactionML.GenerateAEPSURL(aPIRequest).ConfigureAwait(false);
        }

        #endregion
        #region PANRegion
        public async Task<TransactionResponse> APIPANService(RechargeAPIRequest req)
        {
            var resp = new TransactionResponse
            {
                ERRORCODE = ErrorCodes.Request_Accpeted.ToString(),
                MSG = RechargeRespType._PENDING
            };
            try
            {
                var _req = new _RechargeAPIRequest
                {
                    UserID = req.UserID,
                    Token = req.Token,
                    Format = req.Format,
                    OutletID = req.OutletID,
                    Account = req.Account,
                    Amount = req.Amount,
                    APIRequestID = req.APIRequestID,
                    SPKey = req.SPKey,
                    OID = 0,
                    Optional1 = req.Optional1,
                    Optional2 = req.Optional2,
                    Optional3 = req.Optional3,
                    Optional4 = req.Optional4,
                    RequestMode = RequestMode.API,
                    GEOCode = req.GEOCode ?? "",
                    CustomerNumber = req.CustomerNumber ?? "",
                    RefID = req.RefID ?? "",
                    Pincode = req.Pincode,
                    IPAddress = _info.GetRemoteIP()
                };
                var transactionML = new TransactionML(_accessor, _env, _dal, _info);
                resp = await transactionML.DoPSATransaction(_req).ConfigureAwait(false);
                if (resp.STATUS == RechargeRespType.FAILED)
                {
                    resp.OPID = string.Empty;
                }
                if (resp.ERRORCODE.Equals("245"))//Repeat APIREQUESTID found error!
                {
                    resp.STATUS = RechargeRespType.PENDING;
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return resp;
        }
        #endregion
        public async Task<TransactionResponse> APIRecharge(RechargeAPIRequest req)
        {
            var resp = new TransactionResponse
            {
                ERRORCODE = ErrorCodes.Request_Accpeted.ToString(),
                MSG = RechargeRespType._PENDING
            };
            try
            {
                var _req = new _RechargeAPIRequest
                {
                    UserID = req.UserID,
                    Token = req.Token,
                    Format = req.Format,
                    OutletID = req.OutletID,
                    Account = req.Account,
                    Amount = req.Amount,
                    APIRequestID = req.APIRequestID,
                    SPKey = req.SPKey,
                    OID = 0,
                    Optional1 = req.Optional1,
                    Optional2 = req.Optional2,
                    Optional3 = req.Optional3,
                    Optional4 = req.Optional4,
                    RequestMode = RequestMode.API,
                    GEOCode = req.GEOCode ?? "",
                    CustomerNumber = req.CustomerNumber ?? "",
                    RefID = req.RefID ?? "",
                    Pincode = req.Pincode,
                    IPAddress = _info.GetRemoteIP(),
                    FetchBillID=req.FetchBillID,
                    IsReal = req.IsReal
                };
                var transactionML = new TransactionML(_accessor, _env, _dal, _info);
                resp = await transactionML.DoTransaction(_req).ConfigureAwait(false);
                if (resp.STATUS == RechargeRespType.FAILED)
                {
                    resp.OPID = string.Empty;
                }
                if (resp.ERRORCODE.Equals("245"))//Repeat APIREQUESTID found error!
                {
                    resp.STATUS = RechargeRespType.PENDING;
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = 1
                });
            }
            return resp;
        }
        public async Task<TransactionResponse> FetchBill(RechargeAPIRequest req)
        {
            var resp = new TransactionResponse
            {
                ERRORCODE = ErrorCodes.Unable_to_get_bill_details_Please_try_again_after_few_minute.ToString(),
                MSG = nameof(ErrorCodes.Unable_to_get_bill_details_Please_try_again_after_few_minute).Replace("_", " "),
                STATUS = RechargeRespType.FAILED,
                IsBillFetch = true,
                ACCOUNT = req.Account,
                AMOUNT = req.Amount
            };
            try
            {
                var _req = new _RechargeAPIRequest
                {
                    UserID = req.UserID,
                    Token = req.Token,
                    Format = req.Format,
                    OutletID = req.OutletID,
                    Account = req.Account,
                    Amount = req.Amount,
                    APIRequestID = req.APIRequestID,
                    SPKey = req.SPKey,
                    OID = 0,
                    Optional1 = req.Optional1,
                    Optional2 = req.Optional2,
                    Optional3 = req.Optional3,
                    Optional4 = req.Optional4,
                    RequestMode = RequestMode.API,
                    GEOCode = req.GEOCode ?? "",
                    CustomerNumber = req.CustomerNumber ?? "",
                    RefID = req.RefID ?? "",
                    Pincode = req.Pincode,
                    IPAddress = _info.GetRemoteIP()
                };
                var transactionML = new TransactionML(_accessor, _env, _dal, _info);
                resp = await transactionML.DoFetchBill(_req).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "FetchBill",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            resp.REFID = (resp.REFID ?? string.Empty).Trim() == string.Empty ? "RefferenceID" : resp.REFID;
            return resp;
        }
        public async Task<StatusMsg> GetBalance(APIRequest req)
        {
            var resp = new StatusMsg
            {
                ERRORCODE = ErrorCodes.Invalid_Access.ToString(),
                MSG = nameof(ErrorCodes.Invalid_Access),
                STATUS = RechargeRespType.FAILED
            };
            try
            {
                #region RequestValidationFromDB
                /**
                 * Request validation from DB started
                 * **/
                var validateReq = new ValidataRechargeApirequest
                {
                    LoginID = req.UserID,
                    IPAddress = _info.GetRemoteIP(),
                    Token = req.Token,
                    SPKey = "SPKEY"
                };
                IProcedureAsync _procValidate = new ProcValidataRechargeApirequest(_dal);
                var validateRes = (ValidataRechargeApiResp)await _procValidate.Call(validateReq).ConfigureAwait(false);
                if (validateRes.Statuscode == ErrorCodes.Minus1)
                {
                    resp.MSG = validateRes.Msg;
                    resp.ERRORCODE = validateRes.ErrorCode;

                    return resp;
                }
                #endregion
                var userML = new UserML(_accessor, _env, false);
                var balance = userML.GetUserBalnace(req.UserID, LoginType.ApplicationUser);
                resp.STATUS = RechargeRespType.SUCCESS;
                resp.ERRORCODE = ErrorCodes.Transaction_Successful.ToString();
                resp.MSG = nameof(ErrorCodes.Transaction_Successful);
                resp.BAL = balance.Balance;
            }
            catch { }
            return resp;
        }
        public async Task<TransactionResponse> GetStatusCheck(StatusAPIRequest req)
        {
            var _req = new _StatusAPIRequest
            {
                UserID = req.UserID,
                Token = req.Token,
                Format = req.Format,
                AgentID = req.AgentID,
                RPID = req.RPID,
                RequestMode = RequestMode.API,
                Optional1 = req.Optional1
            };

            var transactionML = new TransactionML(_dal, _info);
            var resp = await transactionML.CheckStatus(_req, _c).ConfigureAwait(false);
            resp.IsRefundStatusShow = true;
            return resp;
        }
        public async Task<DMTCheckStatusResponse> GetDMTStatusCheck(StatusAPIRequest req)
        {
            var _req = new _StatusAPIRequest
            {
                UserID = req.UserID,
                Token = req.Token,
                Format = req.Format,
                AgentID = req.AgentID,
                RPID = req.RPID,
                RequestMode = RequestMode.API,
                Optional1 = req.Optional1
            };

            var transactionML = new TransactionML(_dal, _info);
            var resp = await transactionML.CheckDMTStatus(_req, _c).ConfigureAwait(false);
            resp.IsRefundStatusShow = true;
            return resp;
        }
        public async Task<StatusMsg> MarkDispute(RefundAPIRequest req)
        {
            var resp = new StatusMsg
            {
                ERRORCODE = ErrorCodes.Invalid_Access.ToString(),
                MSG = nameof(ErrorCodes.Invalid_Access),
                STATUS = RechargeRespType.FAILED
            };
            try
            {
                #region RequestValidationFromDB
                /**
                 * Request validation from DB started
                 * **/
                var validateReq = new ValidataRechargeApirequest
                {
                    LoginID = req.UserID,
                    IPAddress = _info.GetRemoteIP(),
                    Token = req.Token,
                    SPKey = "SPKEY"
                };
                IProcedureAsync _procValidate = new ProcValidataRechargeApirequest(_dal);
                var validateRes = (ValidataRechargeApiResp)await _procValidate.Call(validateReq).ConfigureAwait(false);
                if (validateRes.Statuscode == ErrorCodes.Minus1)
                {
                    resp.MSG = validateRes.Msg;
                    resp.ERRORCODE = validateRes.ErrorCode;

                    return resp;
                }
                #endregion
                var filter = new RefundRequestReq
                {
                    refundRequest = new RefundRequest
                    {
                        RPID = req.RPID,
                        UserID = req.UserID,
                        OTP = req.OTP,
                        IsResend = req.IsResend
                    },
                    LoginTypeID = LoginType.ApplicationUser,
                    LoginID = req.UserID
                };
                IAppReportML reportML = new ReportML(_accessor, _env, false);
                var ssr = await reportML.MarkDispute(filter).ConfigureAwait(false);
                if (ssr.Statuscode == ErrorCodes.One)
                {
                    resp.STATUS = RechargeRespType.SUCCESS;
                    resp.ERRORCODE = ErrorCodes.Transaction_Successful.ToString();
                    resp.MSG = nameof(ErrorCodes.Transaction_Successful);
                }
                else
                {
                    resp.STATUS = RechargeRespType.FAILED;
                    resp.ERRORCODE = ErrorCodes.Unknown_Error.ToString();
                    resp.MSG = ssr.Msg;
                }
            }
            catch { }
            return resp;
        }
        public async Task SaveAPILog(APIReqResp aPIReqResp)
        {
            IProcedureAsync _proc = new ProcLogAPIUserReqResp(_dal);
            await _proc.Call(aPIReqResp).ConfigureAwait(false);
        }
        public async Task SaveDMRAPILog(APIReqResp aPIReqResp)
        {
            aPIReqResp.RequestIP = _info.GetRemoteIP();
            var _proc = new ProcLogAPIUserReqResp(_dal);
            await _proc.SaveDMRAPIUserLog(aPIReqResp).ConfigureAwait(false);
        }
        public bool BolckSameRequest(string _Method, string _Req)
        {
            IProcedure proc = new ProcSameRequestBlock(_dal);
            return (bool)proc.Call(new BlockSameRequest
            {
                Method=_Method,
                Request = HashEncryption.O.MD5Hash(_Req),
                IP=_info.GetRemoteIP(),
                Browser=_info.GetBrowser()
            });
        }

        public bool TestAPI()
        {
            var rbl = new RBLML(_accessor, _env, _dal);
            return rbl.ChannelPartnerLogin();
        }
        public IResponseStatus AddMAtm(AddmAtmModel model)
        {
            IProcedure _proc = new ProcAddMAtmRequest(_dal);
            return (ResponseStatus)_proc.Call(model);
        }
        public async Task<BillerAPIResponse> GetRPBillerByType(APIBillerRequest req)
        {
            var resp = new BillerAPIResponse
            {
                ErrorCode = ErrorCodes.Invalid_Access,
                Msg = nameof(ErrorCodes.Invalid_Access),
                Statuscode = ErrorCodes.Minus1
            };
            try
            {
                #region RequestValidationFromDB
                /**
                 * Request validation from DB started
                 * **/
                var validateReq = new ValidataRechargeApirequest
                {
                    LoginID = req.UserID,
                    IPAddress = _info.GetRemoteIP(),
                    Token = req.Token,
                    SPKey = "SPKEY"
                };
                IProcedureAsync _procValidate = new ProcValidataRechargeApirequest(_dal);
                var validateRes = (ValidataRechargeApiResp)await _procValidate.Call(validateReq).ConfigureAwait(false);
                if (validateRes.Statuscode == ErrorCodes.Minus1)
                {
                    resp.Msg = validateRes.Msg;
                    resp.ErrorCode = Convert.ToInt32(validateRes.ErrorCode??"0");
                    return resp;
                }
                #endregion
                IOperatorML opML = new OperatorML(_accessor, _env, false);
                resp.data = opML.GetRPBillerByType(req.OpTypeID);
                resp.Statuscode = RechargeRespType.SUCCESS;
                resp.ErrorCode = ErrorCodes.Transaction_Successful;
                resp.Msg = nameof(ErrorCodes.Transaction_Successful);
            }
            catch(Exception ex) {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GetRPBillerByType",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = 1
                });
            }

            return resp;
        }

        public async Task<BillerAPIResponse> GetRPBillerByID(APIBillerRequest req)
        {
            var resp = new BillerAPIResponse
            {
                ErrorCode = ErrorCodes.Invalid_Access,
                Msg = nameof(ErrorCodes.Invalid_Access),
                Statuscode = ErrorCodes.Minus1
            };
            try
            {
                #region RequestValidationFromDB
                /**
                 * Request validation from DB started
                 * **/
                var validateReq = new ValidataRechargeApirequest
                {
                    LoginID = req.UserID,
                    IPAddress = _info.GetRemoteIP(),
                    Token = req.Token,
                    SPKey = "SPKEY"
                };
                IProcedureAsync _procValidate = new ProcValidataRechargeApirequest(_dal);
                var validateRes = (ValidataRechargeApiResp)await _procValidate.Call(validateReq).ConfigureAwait(false);
                if (validateRes.Statuscode == ErrorCodes.Minus1)
                {
                    resp.Msg = validateRes.Msg;
                    resp.ErrorCode = Convert.ToInt32(validateRes.ErrorCode ?? "0");
                    return resp;
                }
                #endregion
                IOperatorML opML = new OperatorML(_accessor, _env, false);
                resp.data = opML.GetRPBillerByID(req.BillerID);
                resp.Statuscode = RechargeRespType.SUCCESS;
                resp.ErrorCode = ErrorCodes.Transaction_Successful;
                resp.Msg = nameof(ErrorCodes.Transaction_Successful);
            }
            catch(Exception ex) {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "GetRPBillerByID",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = 1
                });
            }

            return resp;
        }
    }
}
