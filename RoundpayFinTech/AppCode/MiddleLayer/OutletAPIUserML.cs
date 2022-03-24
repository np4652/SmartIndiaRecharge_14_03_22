using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.DL.Shopping;
using RoundpayFinTech.AppCode.HelperClass;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Shopping;
using RoundpayFinTech.AppCode.StaticModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Validators;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public partial class APIUserML : IOutletAPIUserMiddleLayer
    {
        public ServiceResponse SaveOutletOfAPIUser(APIRequestOutlet aPIRequest)
        {
            var response = ValidateOutletParameters(aPIRequest);
            if (response.Errorcode == ErrorCodes.Transaction_Successful)
            {

                #region RequestValidationFromDB
                /**
                 * Request validation from DB started
                 * **/
                var validateReq = new ValidataRechargeApirequest
                {
                    LoginID = aPIRequest.UserID,
                    IPAddress = _info.GetRemoteIP(),
                    Token = aPIRequest.Token,
                    SPKey = "SPKEY"
                };
                IProcedureAsync _procValidate = new ProcValidataRechargeApirequest(_dal);
                var validateRes = (ValidataRechargeApiResp)_procValidate.Call(validateReq).Result;
                if (validateRes.Statuscode == ErrorCodes.Minus1)
                {
                    response.Msg = validateRes.Msg;
                    response.Errorcode = Convert.ToInt32(validateRes.ErrorCode);
                    return response;
                }
                #endregion
                List<int> requiredDoxID = null;
                IProcedure docProc = new ProcGetDocTypeDetails(_dal);
                var UserDox = (List<DocTypeMaster>)docProc.Call(new CommonReq
                {
                    LoginTypeID = LoginType.ApplicationUser,
                    LoginID = 1
                });
                if (UserDox.Any())
                {
                    requiredDoxID = UserDox.Where(x => x.IsOptional == false).Select(x => x.ID).ToList();
                }
                response = ValidateOutletKYCParameters(aPIRequest, requiredDoxID);
                if (response.Errorcode == ErrorCodes.Transaction_Successful)
                {
                    IProcedure procOutletReg = new ProcRegisterOutlet(_dal);
                    aPIRequest.data.OutletID = 0;
                    var outletRegRes = (ResponseStatus)procOutletReg.Call(new OutletProcModal
                    {
                        UserID = aPIRequest.UserID,
                        IsOutsider = true,
                        OutletName = aPIRequest.data.FirstName + (string.IsNullOrEmpty(aPIRequest.data.MiddleName) ? " " : " " + aPIRequest.data.MiddleName + " ") + aPIRequest.data.LastName,
                        RoleID = Role.APIUser,
                        KYCStatus = KYCStatusType.INCOMPLETE,
                        data = aPIRequest.data
                    });
                    response.Statuscode = outletRegRes.Statuscode;
                    response.Msg = outletRegRes.Msg;
                    if (response.Statuscode == ErrorCodes.Minus1)
                    {
                        response.Errorcode = ErrorCodes.Unknown_Error;
                        return response;
                    }
                    response.data = new
                    {
                        OutletID = outletRegRes.CommonInt
                    };
                    response.Errorcode = ErrorCodes.Transaction_Successful;
                    if (outletRegRes.CommonInt > 0)
                    {
                        UploadKYCFromAPI(aPIRequest, outletRegRes.CommonInt);
                    }
                }
            }
            return response;
        }
        public ServiceResponse UpdateOutletOfAPIUser(APIRequestOutlet aPIRequest)
        {
            var response = ValidateOutletParameters(aPIRequest);

            if (response.Errorcode == ErrorCodes.Transaction_Successful)
            {
                if (aPIRequest.data.OutletID < 10000)
                {
                    response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " OutletID.";
                    response.Errorcode = ErrorCodes.Invalid_Parameter;
                    return response;
                }

                #region RequestValidationFromDB
                /**
                 * Request validation from DB started
                 * **/
                var validateReq = new ValidataRechargeApirequest
                {
                    LoginID = aPIRequest.UserID,
                    IPAddress = _info.GetRemoteIP(),
                    Token = aPIRequest.Token,
                    SPKey = "SPKEY"
                };
                IProcedureAsync _procValidate = new ProcValidataRechargeApirequest(_dal);
                var validateRes = (ValidataRechargeApiResp)_procValidate.Call(validateReq).Result;
                if (validateRes.Statuscode == ErrorCodes.Minus1)
                {
                    response.Msg = validateRes.Msg;
                    response.Errorcode = Convert.ToInt32(validateRes.ErrorCode);
                    return response;
                }
                #endregion
                List<int> requiredDoxID = null;
                IProcedure docProc = new ProcGetUserDocuments(_dal);
                var UserDox = (List<DocTypeMaster>)docProc.Call(new DocTypeMaster
                {
                    LoginTypeID = LoginType.ApplicationUser,
                    LoginId = 1,
                    UserId = aPIRequest.UserID,
                    OutletID = aPIRequest.OutletID
                });
                if (UserDox.Any())
                {
                    requiredDoxID = UserDox.Where(x => x.IsOptional == false && (x.VerifyStatus == DocVerifyType.NOTUPLOADED || x.VerifyStatus == DocVerifyType.REJECTED)).Select(x => x.DocTypeID).ToList();
                }
                response = ValidateOutletKYCParameters(aPIRequest, requiredDoxID);
                if (response.Errorcode == ErrorCodes.Transaction_Successful)
                {
                    IProcedure procOutletReg = new ProcRegisterOutlet(_dal);
                    var outletRegRes = (ResponseStatus)procOutletReg.Call(new OutletProcModal
                    {
                        UserID = aPIRequest.UserID,
                        IsOutsider = true,
                        OutletName = aPIRequest.data.FirstName + (string.IsNullOrEmpty(aPIRequest.data.MiddleName) ? " " : " " + aPIRequest.data.MiddleName + " ") + aPIRequest.data.LastName,
                        RoleID = Role.APIUser,
                        KYCStatus = KYCStatusType.INCOMPLETE,
                        data = aPIRequest.data
                    });
                    response.Statuscode = outletRegRes.Statuscode;
                    response.Msg = outletRegRes.Msg;
                    if (response.Statuscode == ErrorCodes.Minus1)
                    {
                        response.Errorcode = ErrorCodes.Unknown_Error;
                        return response;
                    }
                    response.data = new
                    {
                        OutletID = outletRegRes.CommonInt
                    };
                    response.Errorcode = ErrorCodes.Transaction_Successful;
                    if (outletRegRes.CommonInt > 0)
                    {
                        UploadKYCFromAPI(aPIRequest, outletRegRes.CommonInt);
                    }
                }
            }
            return response;
        }
        public ServiceResponse CheckOutletStatus(APIRequestOutlet aPIRequest)
        {
            var response = new ServiceResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Parmater Validated",
                Errorcode = ErrorCodes.Transaction_Successful
            };
            #region CodeValidation
            if (aPIRequest == null)
            {
                response.Msg = "Invalid Request";
                response.Errorcode = ErrorCodes.Invalid_Parameter;
                return response;
            }
            if (aPIRequest.UserID < 2)
            {
                response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " UserID";
                response.Errorcode = ErrorCodes.Invalid_Parameter;
                return response;
            }
            if ((aPIRequest.Token ?? string.Empty).Length != 32)
            {
                response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " Token";
                response.Errorcode = ErrorCodes.Invalid_Parameter;
                return response;
            }
            if (aPIRequest.data == null)
            {
                response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " data";
                response.Errorcode = ErrorCodes.Invalid_Parameter;
                return response;
            }
            if (!Validate.O.IsMobile(aPIRequest.data.MobileNo ?? string.Empty))
            {
                response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " MobileNo";
                response.Errorcode = ErrorCodes.Invalid_Parameter;
                return response;
            }
            #endregion

            #region RequestValidationFromDB
            /**
             * Request validation from DB started
             * **/
            var validateReq = new ValidataRechargeApirequest
            {
                LoginID = aPIRequest.UserID,
                IPAddress = _info.GetRemoteIP(),
                Token = aPIRequest.Token,
                SPKey = "SPKEY"
            };
            IProcedureAsync _procValidate = new ProcValidataRechargeApirequest(_dal);
            var validateRes = (ValidataRechargeApiResp)_procValidate.Call(validateReq).Result;
            if (validateRes.Statuscode == ErrorCodes.Minus1)
            {
                response.Msg = validateRes.Msg;
                response.Errorcode = Convert.ToInt32(validateRes.ErrorCode);
                return response;
            }
            #endregion
            IProcedure proc = new ProcGetOutletsOfAPIUser(_dal);
            var procRes = (OutletsOfUsersList)proc.Call(new CommonReq
            {
                LoginID = aPIRequest.UserID,
                CommonStr = aPIRequest.data.MobileNo
            });
            if (procRes._ID > 10000)
            {
                response.Statuscode = ErrorCodes.One;
                response.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
                response.data = new
                {
                    OutletID = procRes._ID,
                    KYCStatus = procRes._KYCStatus,
                    VerifyStatus = procRes._VerifyStatus
                };
            }
            else
            {
                response.Msg = ErrorCodes.NODATA;
            }
            return response;
        }
        public ServiceResponse CheckOutletServiceStatus(APIRequestOutlet aPIRequest)
        {
            var response = new ServiceResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Parmater Validated",
                Errorcode = ErrorCodes.Transaction_Successful
            };
            #region CodeValidation
            if (aPIRequest == null)
            {
                response.Msg = "Invalid Request";
                response.Errorcode = ErrorCodes.Invalid_Parameter;
                return response;
            }
            if (string.IsNullOrEmpty(aPIRequest.SPkey))
            {
                response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " SPkey";
                response.Errorcode = ErrorCodes.Invalid_Parameter;
                return response;
            }
            if (aPIRequest.UserID < 2)
            {
                response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " UserID";
                response.Errorcode = ErrorCodes.Invalid_Parameter;
                return response;
            }
            if ((aPIRequest.Token ?? string.Empty).Length != 32)
            {
                response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " Token";
                response.Errorcode = ErrorCodes.Invalid_Parameter;
                return response;
            }
            if (aPIRequest.data == null)
            {
                response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " data";
                response.Errorcode = ErrorCodes.Invalid_Parameter;
                return response;
            }
            if (aPIRequest.data.OutletID < 10000)
            {
                response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " OutletID";
                response.Errorcode = ErrorCodes.Invalid_Parameter;
                return response;
            }

            #endregion

            #region RequestValidationFromDB
            /**
             * Request validation from DB started
             * **/
            var validateReq = new ValidataRechargeApirequest
            {
                LoginID = aPIRequest.UserID,
                IPAddress = _info.GetRemoteIP(),
                Token = aPIRequest.Token,
                SPKey = aPIRequest.SPkey
            };
            IProcedureAsync _procValidate = new ProcValidataRechargeApirequest(_dal);
            var validateRes = (ValidataRechargeApiResp)_procValidate.Call(validateReq).Result;
            if (validateRes.Statuscode == ErrorCodes.Minus1)
            {
                response.Msg = validateRes.Msg;
                response.Errorcode = Convert.ToInt32(validateRes.ErrorCode);
                return response;
            }
            if (validateRes.OID < 1)
            {
                response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " SPkey.";
                response.Errorcode = ErrorCodes.Invalid_Parameter;
                return response;
            }
            #endregion
            BBPSML bbpsML = new BBPSML(_accessor, _env, false);
            var onboadresp = bbpsML.CheckOutletStatusForServices(new CheckOutletStatusReqModel
            {
                LoginTypeID = LoginType.ApplicationUser,
                LoginID = aPIRequest.UserID,
                OID = validateRes.OID,
                OutletID = aPIRequest.data.OutletID,
                OTP=aPIRequest.data.OTP,
                RMode= RequestMode.API,
                PartnerID= aPIRequest.PartnerID,
                Token= aPIRequest.Token,
                OTPRefID=aPIRequest.data.OTPRefID,
                PidData=aPIRequest.data.PidData,
                BioAuthType=aPIRequest.data.BioAuthType,
                IsVerifyBiometric= aPIRequest.data.BioAuthType>0 && string.IsNullOrEmpty(aPIRequest.data.OTP)
            });
            response.Statuscode = onboadresp.Statuscode;
            response.Msg = onboadresp.Msg;
            response.Errorcode = onboadresp.ErrorCode;
            response.data = onboadresp.ResponseStatusForAPI;
            if (onboadresp.ResponseStatusForAPI != null)
            {
                response.Statuscode = onboadresp.ResponseStatusForAPI.Statuscode;
                response.Msg = onboadresp.ResponseStatusForAPI.Msg;
            }
            return response;
        }
        public ServiceResponse GetResources(APIRequestOutlet aPIRequest)
        {
            var response = new ServiceResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Parmater Validated",
                Errorcode = ErrorCodes.Transaction_Successful
            };
            if (aPIRequest == null)
            {
                response.Msg = "Invalid Request";
                response.Errorcode = ErrorCodes.Invalid_Parameter;
                return response;
            }
            if (aPIRequest.UserID < 2)
            {
                response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " UserID";
                response.Errorcode = ErrorCodes.Invalid_Parameter;
                return response;
            }
            if ((aPIRequest.Token ?? string.Empty).Length != 32)
            {
                response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " Token";
                response.Errorcode = ErrorCodes.Invalid_Parameter;
                return response;
            }
            #region RequestValidationFromDB
            /**
             * Request validation from DB started
             * **/
            var validateReq = new ValidataRechargeApirequest
            {
                LoginID = aPIRequest.UserID,
                IPAddress = _info.GetRemoteIP(),
                Token = aPIRequest.Token,
                SPKey = "SPKEY"
            };
            IProcedureAsync _procValidate = new ProcValidataRechargeApirequest(_dal);
            var validateRes = (ValidataRechargeApiResp)_procValidate.Call(validateReq).Result;
            if (validateRes.Statuscode == ErrorCodes.Minus1)
            {
                response.Msg = validateRes.Msg;
                response.Errorcode = Convert.ToInt32(validateRes.ErrorCode);
                return response;
            }
            #endregion

            var allResorces = new AllResourceModalForAPIUsers();
            IProcedure BankProc = new ProcGetBankMaster(_dal);
            allResorces.Banks = (List<BankMaster>)BankProc.Call(new CommonReq
            {
                LoginTypeID = LoginType.ApplicationUser,
                LoginID = 1
            });
            IProcedure StateProc = new ProcGetState(_dal);
            allResorces.States = (List<StateMaster>)StateProc.Call();
            IProcedure CityProc = new ProcGetCity(_dal);
            allResorces.Cities = (List<City>)CityProc.Call(0);

            IProcedure docProc = new ProcGetDocTypeDetails(_dal);
            var docRes = (List<DocTypeMaster>)docProc.Call(new CommonReq
            {
                LoginTypeID = LoginType.ApplicationUser,
                LoginID = 1
            });
            if (docRes.Count > 0)
            {
                allResorces.KYCDocs = docRes.Select(x => new { x.ID, x.DocName, IsRequired = !x.IsOptional });
            }
            allResorces.OtherData = new { OutletStatic.LocationType, OutletStatic.ShopType, OutletStatic.QualificationType, OutletStatic.PopulationType };
            response.Statuscode = ErrorCodes.One;
            response.Msg = nameof(ErrorCodes.Transaction_Successful).Replace("_", " ");
            response.data = allResorces;

            return response;
        }
        private ServiceResponse ValidateOutletParameters(APIRequestOutlet aPIRequest)
        {
            var response = new ServiceResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Parmater Validated",
                Errorcode = ErrorCodes.Transaction_Successful
            };
            if (aPIRequest == null)
            {
                response.Msg = "Invalid Request";
                response.Errorcode = ErrorCodes.Invalid_Parameter;
                return response;
            }
            if (aPIRequest.UserID < 2)
            {
                response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " UserID";
                response.Errorcode = ErrorCodes.Invalid_Parameter;
                return response;
            }
            if ((aPIRequest.Token ?? string.Empty).Length != 32)
            {
                response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " Token";
                response.Errorcode = ErrorCodes.Invalid_Parameter;
                return response;
            }
            if (aPIRequest.data == null)
            {
                response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " data";
                response.Errorcode = ErrorCodes.Invalid_Parameter;
                return response;
            }
            if ((aPIRequest.data.FirstName ?? string.Empty).Length > 30 || (aPIRequest.data.FirstName ?? string.Empty).Length < 2 || Validate.O.IsNumeric((aPIRequest.data.FirstName ?? string.Empty)))
            {
                response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " FirstName. Length must be between 2 and 30";
                response.Errorcode = ErrorCodes.Invalid_Parameter;
                return response;
            }
            if (!string.IsNullOrEmpty(aPIRequest.data.MiddleName))
            {
                if ((aPIRequest.data.MiddleName ?? string.Empty).Length > 30 || (aPIRequest.data.MiddleName ?? string.Empty).Length < 2 || Validate.O.IsNumeric((aPIRequest.data.MiddleName ?? string.Empty)))
                {
                    response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " MiddleName. Length must be between 2 and 30";
                    response.Errorcode = ErrorCodes.Invalid_Parameter;
                    return response;
                }
            }
            if ((aPIRequest.data.LastName ?? string.Empty).Length > 30 || (aPIRequest.data.LastName ?? string.Empty).Length < 2 || Validate.O.IsNumeric((aPIRequest.data.LastName ?? string.Empty)))
            {
                response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " LastName. Length must be between 2 and 30";
                response.Errorcode = ErrorCodes.Invalid_Parameter;
                return response;
            }
            if (!Validate.O.IsMobile(aPIRequest.data.MobileNo ?? string.Empty))
            {
                response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " MobileNo. Length must be 10";
                response.Errorcode = ErrorCodes.Invalid_Parameter;
                return response;
            }
            if (!Validate.O.IsMobile(aPIRequest.data.AlternateMobile ?? string.Empty))
            {
                response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " AlternateMobile. Length must be 10";
                response.Errorcode = ErrorCodes.Invalid_Parameter;
                return response;
            }
            if ((aPIRequest.data.Company ?? string.Empty).Length > 100 || (aPIRequest.data.Company ?? string.Empty).Length < 5 || Validate.O.IsNumeric(aPIRequest.data.Company ?? string.Empty))
            {
                response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " Company. Length must be between 5 and 100";
                response.Errorcode = ErrorCodes.Invalid_Parameter;
                return response;
            }
            aPIRequest.data.EmailID = (aPIRequest.data.EmailID ?? string.Empty).ToLower();
            if (!Validate.O.IsEmail(aPIRequest.data.EmailID) || string.IsNullOrEmpty(aPIRequest.data.EmailID))
            {
                response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " EmailID.";
                response.Errorcode = ErrorCodes.Invalid_Parameter;
                return response;
            }
            if ((aPIRequest.data.Pincode ?? string.Empty).Length != 6)
            {
                response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " Pincode. Length must be 6";
                response.Errorcode = ErrorCodes.Invalid_Parameter;
                return response;
            }
            if ((aPIRequest.data.Address ?? string.Empty).Length > 150 || (aPIRequest.data.Address ?? string.Empty).Length < 5 || Validate.O.IsNumeric(aPIRequest.data.Address ?? string.Empty))
            {
                response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " Address. Length must be between 5 and 150";
                response.Errorcode = ErrorCodes.Invalid_Parameter;
                return response;
            }
            if (!Validate.O.IsPANUpper(aPIRequest.data.PAN ?? string.Empty))
            {
                response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " PAN.";
                response.Errorcode = ErrorCodes.Invalid_Parameter;
                return response;
            }
            if (!Validate.O.IsAADHAR(aPIRequest.data.AADHAR ?? string.Empty))
            {
                response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " AADHAR.";
                response.Errorcode = ErrorCodes.Invalid_Parameter;
                return response;
            }
            if (aPIRequest.data.StateID < 1 || aPIRequest.data.StateID > 40)
            {
                response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " StateID.";
                response.Errorcode = ErrorCodes.Invalid_Parameter;
                return response;
            }
            if (aPIRequest.data.CityID < 1)
            {
                response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " CityID.";
                response.Errorcode = ErrorCodes.Invalid_Parameter;
                return response;
            }
            if (!Validate.O.IsDateIn_dd_MMM_yyyy_Format(aPIRequest.data.DOB ?? string.Empty))
            {
                response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " DOB. Format must be like dd MMM yyyy e.g. 01 JAN 1900";
                response.Errorcode = ErrorCodes.Invalid_Parameter;
                return response;
            }
            double dDiff = (DateTime.Now - Convert.ToDateTime(aPIRequest.data.DOB)).TotalDays / 365;
            if (dDiff < 18 || dDiff > 100)
            {
                response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " DOB. Age must be between 18 and 100";
                response.Errorcode = ErrorCodes.Invalid_Parameter;
                return response;
            }
            if (!OutletStatic.ShopType.Any((aPIRequest.data.ShopType ?? string.Empty).ToLower().Contains))
            {
                response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " ShopType. Length must be between 5 and 50";
                response.Errorcode = ErrorCodes.Invalid_Parameter;
                return response;
            }
            if (!OutletStatic.QualificationType.Any((aPIRequest.data.Qualification ?? string.Empty).Contains))
            {
                response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " Qualification. Length must be between 3 and 20";
                response.Errorcode = ErrorCodes.Invalid_Parameter;
                return response;
            }
            if (!OutletStatic.PopulationType.Any((aPIRequest.data.Poupulation ?? string.Empty).Contains))
            {
                response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " Poupulation. Length must be between 5 and 20";
                response.Errorcode = ErrorCodes.Invalid_Parameter;
                return response;
            }
            if (!OutletStatic.LocationType.Any((aPIRequest.data.LocationType ?? string.Empty).ToLower().Contains))
            {
                response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " LocationType. Length must be between 5 and 12";
                response.Errorcode = ErrorCodes.Invalid_Parameter;
                return response;
            }
            if ((aPIRequest.data.Landmark ?? string.Empty).Length > 100 || (aPIRequest.data.Landmark ?? string.Empty).Length < 5 || Validate.O.IsNumeric(aPIRequest.data.Landmark ?? string.Empty))
            {
                response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " Landmark. Length must be between 5 and 100";
                response.Errorcode = ErrorCodes.Invalid_Parameter;
                return response;
            }
            if (Validate.O.IsLatitudeInValid(aPIRequest.data.Lattitude))
            {
                response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " Lattitude. Lattitude between 90 to -90 and must be followed by 4 decimal palces.";
                response.Errorcode = ErrorCodes.Invalid_Parameter;
                return response;
            }
            if (Validate.O.IsLongitudeInValid(aPIRequest.data.Longitude))
            {
                response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " Longitude. Longitude between 180 to -180 and must be followed by 4 decimal palces.";
                response.Errorcode = ErrorCodes.Invalid_Parameter;
                return response;
            }
            if ((aPIRequest.data.BankName ?? string.Empty).Length > 100 || (aPIRequest.data.BankName ?? string.Empty).Length < 5 || Validate.O.IsNumeric(aPIRequest.data.BankName ?? string.Empty))
            {
                response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " BankName. Length must be between 5 and 100";
                response.Errorcode = ErrorCodes.Invalid_Parameter;
                return response;
            }
            if ((aPIRequest.data.AccountNumber ?? string.Empty).Length > 25 || (aPIRequest.data.AccountNumber ?? string.Empty).Length < 5)
            {
                response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " AccountNumber.";
                response.Errorcode = ErrorCodes.Invalid_Parameter;
                return response;
            }
            if ((aPIRequest.data.AccountHolder ?? string.Empty).Length > 150 || (aPIRequest.data.AccountHolder ?? string.Empty).Length < 3 || Validate.O.IsNumeric(aPIRequest.data.AccountHolder ?? string.Empty))
            {
                response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " AccountHolder.";
                response.Errorcode = ErrorCodes.Invalid_Parameter;
                return response;
            }
            if ((aPIRequest.data.BranchName ?? string.Empty).Length > 50 || (aPIRequest.data.BranchName ?? string.Empty).Length < 3 || Validate.O.IsNumeric(aPIRequest.data.BranchName ?? string.Empty))
            {
                response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " BranchName.";
                response.Errorcode = ErrorCodes.Invalid_Parameter;
                return response;
            }
            if ((aPIRequest.data.IFSC ?? string.Empty).Length > 20 || (aPIRequest.data.IFSC ?? string.Empty).Length < 5 || Validate.O.IsNumeric(aPIRequest.data.IFSC ?? string.Empty))
            {
                response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " IFSC.";
                response.Errorcode = ErrorCodes.Invalid_Parameter;
                return response;
            }
            return response;
        }
        private ServiceResponse ValidateOutletKYCParameters(APIRequestOutlet aPIRequest, IEnumerable<int> requiredDoxID)
        {
            var response = new ServiceResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = "Parmater Validated",
                Errorcode = ErrorCodes.Transaction_Successful
            };
            if (aPIRequest.kycDoc == null)
            {
                response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " kycDoc.";
                response.Errorcode = ErrorCodes.Invalid_Parameter;
                return response;
            }
            if (requiredDoxID != null)
            {
                foreach (var item in requiredDoxID)
                {
                    if (item == DOCType.PAN && string.IsNullOrEmpty(aPIRequest.kycDoc.PAN))
                    {
                        response.Msg = "Required kycDoc PAN Card was not supplied.";
                        response.Errorcode = ErrorCodes.Invalid_Parameter;
                        return response;
                    }
                    if (item == DOCType.AADHAR && string.IsNullOrEmpty(aPIRequest.kycDoc.AADHAR))
                    {
                        response.Msg = "Required kycDoc Aadhaar Card was not supplied.";
                        response.Errorcode = ErrorCodes.Invalid_Parameter;
                        return response;
                    }
                    if (item == DOCType.PHOTO && string.IsNullOrEmpty(aPIRequest.kycDoc.PHOTO))
                    {
                        response.Msg = "Required kycDoc Photo (Passport Size) was not supplied.";
                        response.Errorcode = ErrorCodes.Invalid_Parameter;
                        return response;
                    }
                    if (item == DOCType.ServiceAggreement && string.IsNullOrEmpty(aPIRequest.kycDoc.ServiceAggreement))
                    {
                        response.Msg = "Required kycDoc Service Agreement was not supplied.";
                        response.Errorcode = ErrorCodes.Invalid_Parameter;
                        return response;
                    }
                    if (item == DOCType.GSTRegistration && string.IsNullOrEmpty(aPIRequest.kycDoc.GSTRegistration))
                    {
                        response.Msg = "Required kycDoc GST Registration was not supplied.";
                        response.Errorcode = ErrorCodes.Invalid_Parameter;
                        return response;
                    }
                    if (item == DOCType.CancelledCheque && string.IsNullOrEmpty(aPIRequest.kycDoc.CancelledCheque))
                    {
                        response.Msg = "Required kycDoc Cancelled Cheque was not supplied.";
                        response.Errorcode = ErrorCodes.Invalid_Parameter;
                        return response;
                    }
                    if (item == DOCType.BusinessAddressProof && string.IsNullOrEmpty(aPIRequest.kycDoc.BusinessAddressProof))
                    {
                        response.Msg = "Required kycDoc Business Address Proof was not supplied.";
                        response.Errorcode = ErrorCodes.Invalid_Parameter;
                        return response;
                    }
                    if (item == DOCType.PASSBOOK && string.IsNullOrEmpty(aPIRequest.kycDoc.PASSBOOK))
                    {
                        response.Msg = "Required kycDoc BANK PASSBOOK was not supplied.";
                        response.Errorcode = ErrorCodes.Invalid_Parameter;
                        return response;
                    }
                    if (item == DOCType.VoterID && string.IsNullOrEmpty(aPIRequest.kycDoc.VoterID))
                    {
                        response.Msg = "Required kycDoc Voter ID was not supplied.";
                        response.Errorcode = ErrorCodes.Invalid_Parameter;
                        return response;
                    }
                    if (item == DOCType.DrivingLicense && string.IsNullOrEmpty(aPIRequest.kycDoc.DrivingLicense))
                    {
                        response.Msg = "Required kycDoc Driving License was not supplied.";
                        response.Errorcode = ErrorCodes.Invalid_Parameter;
                        return response;
                    }
                    if (item == DOCType.ShopImage && string.IsNullOrEmpty(aPIRequest.kycDoc.ShopImage))
                    {
                        response.Msg = "Required kycDoc Shop Image was not supplied.";
                        response.Errorcode = ErrorCodes.Invalid_Parameter;
                        return response;
                    }
                }
            }
            if (!string.IsNullOrEmpty(aPIRequest.kycDoc.PAN))
            {
                byte[] b = Validate.O.TryToConnvertBase64String(aPIRequest.kycDoc.PAN);
                if (b == null)
                {
                    response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " kycDoc PAN Card. Invalid base64 string";
                    response.Errorcode = ErrorCodes.Invalid_Parameter;
                    return response;
                }
                var ext = Validate.O.GetIfAllowedExtensionIsExists(b);
                if (string.IsNullOrEmpty(ext))
                {
                    response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " kycDoc PAN. File type is not allowed.";
                    response.Errorcode = ErrorCodes.Invalid_Parameter;
                    return response;
                }
                if (Validate.O.CalculateSizeOfBase64File(aPIRequest.kycDoc.PAN) / 1024 > 200)
                {
                    ImageResizer imageResizer = new ImageResizer(200 * 1024, b, Validate.O.GetIfAllowedExtensionIsExists(b).ToLower());
                    aPIRequest.kycDoc.PAN = Convert.ToBase64String(imageResizer.ScaleImage());
                }
            }
            if (!string.IsNullOrEmpty(aPIRequest.kycDoc.AADHAR))
            {
                byte[] b = Validate.O.TryToConnvertBase64String(aPIRequest.kycDoc.AADHAR);
                if (b == null)
                {
                    response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " kycDoc Aadhaar Card. Invalid base64 string";
                    response.Errorcode = ErrorCodes.Invalid_Parameter;
                    return response;
                }

                var ext = Validate.O.GetIfAllowedExtensionIsExists(b);
                if (string.IsNullOrEmpty(ext))
                {
                    response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " kycDoc AADHAR. File type is not allowed.";
                    response.Errorcode = ErrorCodes.Invalid_Parameter;
                    return response;
                }
                if (Validate.O.CalculateSizeOfBase64File(aPIRequest.kycDoc.AADHAR) / 200 > 1024)
                {
                    ImageResizer imageResizer = new ImageResizer(200 * 1024, b, Validate.O.GetIfAllowedExtensionIsExists(b).ToLower());
                    aPIRequest.kycDoc.AADHAR = Convert.ToBase64String(imageResizer.ScaleImage());
                }
            }
            if (!string.IsNullOrEmpty(aPIRequest.kycDoc.PHOTO))
            {
                byte[] b = Validate.O.TryToConnvertBase64String(aPIRequest.kycDoc.PHOTO);
                if (b == null)
                {
                    response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " kycDoc Photo (Passport Size). Invalid base64 string";
                    response.Errorcode = ErrorCodes.Invalid_Parameter;
                    return response;
                }
                var ext = Validate.O.GetIfAllowedExtensionIsExists(b);
                if (string.IsNullOrEmpty(ext))
                {
                    response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " kycDoc PHOTO. File type is not allowed.";
                    response.Errorcode = ErrorCodes.Invalid_Parameter;
                    return response;
                }
                if (Validate.O.CalculateSizeOfBase64File(aPIRequest.kycDoc.PHOTO) / 200 > 1024)
                {
                    ImageResizer imageResizer = new ImageResizer(200 * 1024, b, Validate.O.GetIfAllowedExtensionIsExists(b).ToLower());
                    aPIRequest.kycDoc.PHOTO = Convert.ToBase64String(imageResizer.ScaleImage());
                }
            }
            if (!string.IsNullOrEmpty(aPIRequest.kycDoc.ServiceAggreement))
            {
                byte[] b = Validate.O.TryToConnvertBase64String(aPIRequest.kycDoc.ServiceAggreement);
                if (b == null)
                {
                    response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " kycDoc Service Agreement. Invalid base64 string";
                    response.Errorcode = ErrorCodes.Invalid_Parameter;
                    return response;
                }

                var ext = Validate.O.GetIfAllowedExtensionIsExists(b);
                if (string.IsNullOrEmpty(ext))
                {
                    response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " kycDoc ServiceAggreement. File type is not allowed.";
                    response.Errorcode = ErrorCodes.Invalid_Parameter;
                    return response;
                }
                if (Validate.O.CalculateSizeOfBase64File(aPIRequest.kycDoc.ServiceAggreement) / 200 > 1024)
                {
                    ImageResizer imageResizer = new ImageResizer(200 * 1024, b, Validate.O.GetIfAllowedExtensionIsExists(b).ToLower());
                    aPIRequest.kycDoc.ServiceAggreement = Convert.ToBase64String(imageResizer.ScaleImage());
                }
            }
            if (!string.IsNullOrEmpty(aPIRequest.kycDoc.GSTRegistration))
            {
                byte[] b = Validate.O.TryToConnvertBase64String(aPIRequest.kycDoc.GSTRegistration);
                if (b == null)
                {
                    response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " kycDoc GST Registration. Invalid base64 string";
                    response.Errorcode = ErrorCodes.Invalid_Parameter;
                    return response;
                }

                var ext = Validate.O.GetIfAllowedExtensionIsExists(b);
                if (string.IsNullOrEmpty(ext))
                {
                    response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " kycDoc GSTRegistration. File type is not allowed.";
                    response.Errorcode = ErrorCodes.Invalid_Parameter;
                    return response;
                }
                if (Validate.O.CalculateSizeOfBase64File(aPIRequest.kycDoc.GSTRegistration) / 200 > 1024)
                {
                    ImageResizer imageResizer = new ImageResizer(200 * 1024, b, Validate.O.GetIfAllowedExtensionIsExists(b).ToLower());
                    aPIRequest.kycDoc.GSTRegistration = Convert.ToBase64String(imageResizer.ScaleImage());
                }
            }
            if (!string.IsNullOrEmpty(aPIRequest.kycDoc.CancelledCheque))
            {
                byte[] b = Validate.O.TryToConnvertBase64String(aPIRequest.kycDoc.CancelledCheque);
                if (b == null)
                {
                    response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " kycDoc Cancelled Cheque. Invalid base64 string";
                    response.Errorcode = ErrorCodes.Invalid_Parameter;
                    return response;
                }

                var ext = Validate.O.GetIfAllowedExtensionIsExists(b);
                if (string.IsNullOrEmpty(ext))
                {
                    response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " kycDoc Cancelled Cheque. File type is not allowed.";
                    response.Errorcode = ErrorCodes.Invalid_Parameter;
                    return response;
                }
                if (Validate.O.CalculateSizeOfBase64File(aPIRequest.kycDoc.CancelledCheque) / 200 > 1024)
                {
                    ImageResizer imageResizer = new ImageResizer(200 * 1024, b, Validate.O.GetIfAllowedExtensionIsExists(b).ToLower());
                    aPIRequest.kycDoc.CancelledCheque = Convert.ToBase64String(imageResizer.ScaleImage());
                }
            }
            if (!string.IsNullOrEmpty(aPIRequest.kycDoc.BusinessAddressProof))
            {
                byte[] b = Validate.O.TryToConnvertBase64String(aPIRequest.kycDoc.BusinessAddressProof);
                if (b == null)
                {
                    response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " kycDoc BusinessAddressProof. Invalid base64 string";
                    response.Errorcode = ErrorCodes.Invalid_Parameter;
                    return response;
                }

                var ext = Validate.O.GetIfAllowedExtensionIsExists(b);
                if (string.IsNullOrEmpty(ext))
                {
                    response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " kycDoc BusinessAddressProof. File type is not allowed.";
                    response.Errorcode = ErrorCodes.Invalid_Parameter;
                    return response;
                }
                if (Validate.O.CalculateSizeOfBase64File(aPIRequest.kycDoc.BusinessAddressProof) / 200 > 1024)
                {
                    ImageResizer imageResizer = new ImageResizer(200 * 1024, b, Validate.O.GetIfAllowedExtensionIsExists(b).ToLower());
                    aPIRequest.kycDoc.BusinessAddressProof = Convert.ToBase64String(imageResizer.ScaleImage());
                }
            }
            if (!string.IsNullOrEmpty(aPIRequest.kycDoc.PASSBOOK))
            {
                byte[] b = Validate.O.TryToConnvertBase64String(aPIRequest.kycDoc.PASSBOOK);
                if (b == null)
                {
                    response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " kycDoc PASSBOOK. Invalid base64 string";
                    response.Errorcode = ErrorCodes.Invalid_Parameter;
                    return response;
                }

                var ext = Validate.O.GetIfAllowedExtensionIsExists(b);
                if (string.IsNullOrEmpty(ext))
                {
                    response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " kycDoc PASSBOOK. File type is not allowed.";
                    response.Errorcode = ErrorCodes.Invalid_Parameter;
                    return response;
                }
                if (Validate.O.CalculateSizeOfBase64File(aPIRequest.kycDoc.PASSBOOK) / 200 > 1024)
                {
                    ImageResizer imageResizer = new ImageResizer(200 * 1024, b, Validate.O.GetIfAllowedExtensionIsExists(b).ToLower());
                    aPIRequest.kycDoc.PASSBOOK = Convert.ToBase64String(imageResizer.ScaleImage());
                }
            }
            if (!string.IsNullOrEmpty(aPIRequest.kycDoc.VoterID))
            {
                byte[] b = Validate.O.TryToConnvertBase64String(aPIRequest.kycDoc.VoterID);
                if (b == null)
                {
                    response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " kycDoc VoterID. Invalid base64 string";
                    response.Errorcode = ErrorCodes.Invalid_Parameter;
                    return response;
                }

                var ext = Validate.O.GetIfAllowedExtensionIsExists(b);
                if (string.IsNullOrEmpty(ext))
                {
                    response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " kycDoc VoterID. File type is not allowed.";
                    response.Errorcode = ErrorCodes.Invalid_Parameter;
                    return response;
                }
                if (Validate.O.CalculateSizeOfBase64File(aPIRequest.kycDoc.VoterID) / 200 > 1024)
                {
                    ImageResizer imageResizer = new ImageResizer(200 * 1024, b, Validate.O.GetIfAllowedExtensionIsExists(b).ToLower());
                    aPIRequest.kycDoc.VoterID = Convert.ToBase64String(imageResizer.ScaleImage());
                }
            }
            if (!string.IsNullOrEmpty(aPIRequest.kycDoc.DrivingLicense))
            {
                byte[] b = Validate.O.TryToConnvertBase64String(aPIRequest.kycDoc.DrivingLicense);
                if (b == null)
                {
                    response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " kycDoc DrivingLicense. Invalid base64 string";
                    response.Errorcode = ErrorCodes.Invalid_Parameter;
                    return response;
                }

                var ext = Validate.O.GetIfAllowedExtensionIsExists(b);
                if (string.IsNullOrEmpty(ext))
                {
                    response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " kycDoc DrivingLicense. File type is not allowed.";
                    response.Errorcode = ErrorCodes.Invalid_Parameter;
                    return response;
                }
                if (Validate.O.CalculateSizeOfBase64File(aPIRequest.kycDoc.DrivingLicense) / 200 > 1024)
                {
                    ImageResizer imageResizer = new ImageResizer(200 * 1024, b, Validate.O.GetIfAllowedExtensionIsExists(b).ToLower());
                    aPIRequest.kycDoc.DrivingLicense = Convert.ToBase64String(imageResizer.ScaleImage());
                }
            }
            if (!string.IsNullOrEmpty(aPIRequest.kycDoc.ShopImage))
            {
                byte[] b = Validate.O.TryToConnvertBase64String(aPIRequest.kycDoc.ShopImage);
                if (b == null)
                {
                    response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " kycDoc ShopImage. Invalid base64 string";
                    response.Errorcode = ErrorCodes.Invalid_Parameter;
                    return response;
                }

                var ext = Validate.O.GetIfAllowedExtensionIsExists(b);
                if (string.IsNullOrEmpty(ext))
                {
                    response.Msg = nameof(ErrorCodes.Invalid_Parameter).Replace("_", " ") + " kycDoc ShopImage. File type is not allowed.";
                    response.Errorcode = ErrorCodes.Invalid_Parameter;
                    return response;
                }
                if (Validate.O.CalculateSizeOfBase64File(aPIRequest.kycDoc.ShopImage) / 200 > 1024)
                {
                    ImageResizer imageResizer = new ImageResizer(200 * 1024, b, Validate.O.GetIfAllowedExtensionIsExists(b).ToLower());
                    aPIRequest.kycDoc.ShopImage = Convert.ToBase64String(imageResizer.ScaleImage());
                }
            }
            return response;
        }
        private void UploadKYCFromAPI(APIRequestOutlet aPIRequest, int ChildOutletID)
        {
            if (aPIRequest.kycDoc != null && ChildOutletID > 0)
            {
                var docTypeMaster = new DocTypeMaster
                {
                    LoginTypeID = LoginType.ApplicationUser,
                    LoginId = aPIRequest.UserID,
                    UserId = aPIRequest.UserID,
                    OutletID = ChildOutletID
                };
                IUserAPPML userML = new UserML(_accessor, _env, false);
                var UserDox = userML.GetDocumentsForApp(docTypeMaster);
                if (UserDox != null)
                {
                    foreach (var item in UserDox)
                    {
                        if (item.ID == 0 || item.VerifyStatus != DocVerifyType.VERIFIED && item.VerifyStatus != DocVerifyType.NOTVERIFIED)
                        {
                            try
                            {
                                string base64String = null;
                                if (item.DocTypeID == DOCType.PAN)
                                {
                                    base64String = aPIRequest.kycDoc.PAN;
                                }
                                if (item.DocTypeID == DOCType.AADHAR)
                                {
                                    base64String = aPIRequest.kycDoc.AADHAR;
                                }
                                if (item.DocTypeID == DOCType.PHOTO)
                                {
                                    base64String = aPIRequest.kycDoc.PHOTO;
                                }
                                if (item.DocTypeID == DOCType.ServiceAggreement)
                                {
                                    base64String = aPIRequest.kycDoc.ServiceAggreement;
                                }
                                if (item.DocTypeID == DOCType.GSTRegistration)
                                {
                                    base64String = aPIRequest.kycDoc.GSTRegistration;
                                }
                                if (item.DocTypeID == DOCType.CancelledCheque)
                                {
                                    base64String = aPIRequest.kycDoc.CancelledCheque;
                                }
                                if (item.DocTypeID == DOCType.BusinessAddressProof)
                                {
                                    base64String = aPIRequest.kycDoc.BusinessAddressProof;
                                }
                                if (item.DocTypeID == DOCType.PASSBOOK)
                                {
                                    base64String = aPIRequest.kycDoc.PASSBOOK;
                                }
                                if (item.DocTypeID == DOCType.VoterID)
                                {
                                    base64String = aPIRequest.kycDoc.VoterID;
                                }
                                if (item.DocTypeID == DOCType.DrivingLicense)
                                {
                                    base64String = aPIRequest.kycDoc.DrivingLicense;
                                }
                                if (item.DocTypeID == DOCType.ShopImage)
                                {
                                    base64String = aPIRequest.kycDoc.ShopImage;
                                }

                                if (!string.IsNullOrEmpty(base64String))
                                {
                                    byte[] b = Validate.O.TryToConnvertBase64String(base64String);
                                    string fileExtension = Validate.O.GetIfAllowedExtensionIsExists(b);
                                    var sbURL = new StringBuilder();
                                    sbURL.Append(item.DocTypeID);
                                    sbURL.Append("_");
                                    sbURL.Append(aPIRequest.UserID);
                                    sbURL.Append("_");
                                    sbURL.Append(ChildOutletID);
                                    sbURL.Append(fileExtension.ToLower());
                                    File.WriteAllBytes(DOCType.DocFilePath + sbURL.ToString(), b);
                                    var dr = new DocumentReq
                                    {
                                        LT = LoginType.ApplicationUser,
                                        LoginID = aPIRequest.UserID,
                                        URL = sbURL.ToString(),
                                        DocType = item.DocTypeID,
                                        ChildUserID = aPIRequest.UserID,
                                        IP = _info.GetRemoteIP(),
                                        Browser = _info.GetBrowserFullInfo(),
                                        APIUserOutletID = ChildOutletID,
                                        RequestModeID = RequestMode.API
                                    };
                                    IProcedure _proc = new ProcUploadDocument(_dal);
                                    var docRes = (ResponseStatus)_proc.Call(dr);
                                    if (docRes.Statuscode == ErrorCodes.Minus1)
                                        throw new Exception("DocError " + docRes.Msg);
                                }
                            }
                            catch (Exception ex)
                            {
                                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                                {
                                    ClassName = GetType().Name,
                                    FuncName = "UploadKYCFromAPI",
                                    Error = item.DocName + ":" + ex.Message,
                                    LoginTypeID = LoginType.ApplicationUser,
                                    UserId = ChildOutletID
                                });
                            }
                        }
                    }
                    var req = new KYCStatusReq
                    {
                        LT = LoginType.ApplicationUser,
                        LoginID = aPIRequest.UserID,
                        OutletID = ChildOutletID,
                        KYCStatus = KYCStatusType.APPLIED,
                        RequestModeID = RequestMode.API
                    };
                    var res = userML.ChangeKYCStatusForApp(req);
                    if (res.Statuscode == ErrorCodes.Minus1)
                    {
                        var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                        {
                            ClassName = GetType().Name,
                            FuncName = "UpdateKYCStatusFromAPI",
                            Error = "InternalLog:" + res.Msg,
                            LoginTypeID = LoginType.ApplicationUser,
                            UserId = ChildOutletID
                        });
                    }
                }
            }
        }
    }
}
