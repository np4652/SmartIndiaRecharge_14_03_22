using Fintech.AppCode;
using Fintech.AppCode.Configuration;
using Fintech.AppCode.DB;
using Fintech.AppCode.HelperClass;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using Fintech.AppCode.WebRequest;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;
using RoundpayFinTech.AppCode.DL;
using RoundpayFinTech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.MiddleLayer.Dmt_Api;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.App;
using RoundpayFinTech.AppCode.Model.BBPS;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Report;
using RoundpayFinTech.AppCode.Model.ROffer;
using RoundpayFinTech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.StaticModel.MoneyTransfer;
using RoundpayFinTech.AppCode.ThirdParty.AxisBank;
using RoundpayFinTech.AppCode.ThirdParty.BillAvenue;
using RoundpayFinTech.AppCode.ThirdParty.PayU;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Validators;

namespace RoundpayFinTech.AppCode.MiddleLayer
{
    public class OperatorML : IOperatorML, IOperatorAppML, ITargetML
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IHostingEnvironment _env;
        private readonly ISession _session;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        private readonly IRequestInfo _rinfo;
        private readonly WebsiteInfo _WInfo;
        private readonly LoginResponse _lr;
        private readonly IUserML userML;
        public OperatorML(IHttpContextAccessor accessor, IHostingEnvironment env, bool InSession = true)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());

            if (InSession)
            {
                _session = _accessor.HttpContext.Session;
                _rinfo = new RequestInfo(_accessor, _env);
                _WInfo = new LoginML(_accessor, _env).GetWebsiteInfo();
                _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
                userML = new UserML(_lr);
            }
        }
        public IEnumerable<OpTypeMaster> GetOptypeVendor(int id)
        {
            ProcGetOperator proc = new ProcGetOperator(_dal);
            var res = proc.OpTypesVendorMaster(id).ToList();
            var defItem = new OpTypeMaster
            {
                ID = 0,
                OpType = ":: Select Operator ::"
            };
            res.Insert(0, defItem);
            return res;
        }
        public OperatorDetail GetOperator(int ID)
        {
            var res = new OperatorDetail();
            if (_lr.RoleID > 0 && ID > 0)
            {
                IProcedure proc = new ProcGetOperator(_dal);
                res = (OperatorDetail)proc.Call(new CommonReq
                {
                    CommonInt = ID,
                    CommonInt2 = 0,
                    LoginID = _lr.UserID
                });
            }
            return res;
        }
        public RPBillerModel GetRPBillerByID(string BillerID)
        {
            IProcedure proc = new ProcGetRPBiller(_dal);
            return (RPBillerModel)proc.Call(new CommonReq
            {
                CommonStr = BillerID
            });
        }
        public RPBillerModel GetRPBillerByType(int OpTypeID)
        {
            IProcedure proc = new ProcGetRPBiller(_dal);
            return (RPBillerModel)proc.Call(new CommonReq
            {
                CommonStr = string.Empty,
                CommonInt = OpTypeID
            });
        }
        public IEnumerable<OperatorDetail> GetOperators(int Type = 0)
        {
            var res = new List<OperatorDetail>();
            IProcedure proc = new ProcGetOperator(_dal);
            res = (List<OperatorDetail>)proc.Call(new CommonReq
            {
                CommonInt = 0,
                CommonInt2 = Type,
                LoginID = 1
            });
            return res;
        }
        public IEnumerable<OperatorDetail> GetOperatorsSession(int uid, int RoleID)
        {
            var req = new CommonReq
            {
                CommonInt = 0,
                CommonInt2 = 0,
                LoginID = uid
            };
            IProcedure proc = new ProcGetOperator(_dal);
            var res = (List<OperatorDetail>)proc.Call(req);
            if (RoleID == Role.Customer)
            {
                res.Where(x => x.AllowedChannel == 2 || x.AllowedChannel == 3);
            }
            else
            {
                res.Where(x => x.AllowedChannel == 1 || x.AllowedChannel == 3);
            }
            return res;
        }
        public IResponseStatus UpdateAllBillers(int OpTypeID)
        {
            IProcedure procChek = new ProcCheckBillerInfoAllowed(_dal);
            var resList = (List<ResponseStatus>)procChek.Call(new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID,
                CommonInt = -1,
                CommonInt2 = OpTypeID
            });
            if (resList.Count == 0)
            {
                return new ResponseStatus
                {
                    Statuscode = ErrorCodes.Minus1,
                    Msg = "No operator is eligible for selected filter"
                };
            }
            if (resList[0].CommonStr3.Equals(APICode.BILLAVENUE))
            {
                BillAvenueML billAvenueML = new BillAvenueML(_accessor, _env, _dal);
                var bAResp = billAvenueML.GetBillerInfo(resList, resList[0].CommonStr2);
                if (bAResp.responseCode == "000")
                {
                    foreach (var item in bAResp.billerList)
                    {
                        var procUpdateReq = new BillerInfoUpdateModel
                        {
                            OID = resList.Where(x => x.CommonStr == item.billerId).Select(x => x.CommonInt).ToList()[0],
                            LoginID = _lr.UserID,
                            BillerName = item.billerName,
                            BillerAdhoc = item.billerAdhoc,
                            BillerCoverage = item.billerCoverage,
                            BillerAmountOptions = item.billerAmountOptions,
                            BillerPaymentModes = item.billerPaymentModes,
                            IsBillValidation = (item.billerSupportBillValidation ?? string.Empty).Equals("MANDATORY"),
                            IsAmountInValidation = (item.rechargeAmountInValidationRequest ?? string.Empty).Equals("MANDATORY")
                        };
                        procUpdateReq.ExactNess = procUpdateReq.BillerAdhoc == true ? ExactnessType.All : ExactnessType.Exact;
                        if ((item.billerPaymentExactness ?? string.Empty).ToUpper().In("EXACT_UP", "EXACT &AMP; ABOVE"))
                        {
                            procUpdateReq.ExactNess = ExactnessType.ExactAndAbove;
                        }
                        else if ((item.billerPaymentExactness ?? string.Empty).ToUpper().In("EXACT_DOWN", "EXACT AND BELOW"))
                        {
                            procUpdateReq.ExactNess = ExactnessType.ExactAndBelow;
                        }

                        if (!string.IsNullOrEmpty(procUpdateReq.BillerAmountOptions))
                        {
                            procUpdateReq.BillerAmountOptions = procUpdateReq.BillerAmountOptions.Replace("|", "").Replace(",,,", "|");
                            procUpdateReq.BillerAmountOptions = procUpdateReq.BillerAmountOptions.Replace(",,", ",").Replace(",", "|");
                        }
                        procUpdateReq.tp_OperatorParams = new DataTable();
                        procUpdateReq.tp_OperatorParams.Columns.Add("PName", typeof(string));
                        procUpdateReq.tp_OperatorParams.Columns.Add("PDataType", typeof(string));
                        procUpdateReq.tp_OperatorParams.Columns.Add("PMinLength", typeof(int));
                        procUpdateReq.tp_OperatorParams.Columns.Add("PMaxLength", typeof(int));
                        procUpdateReq.tp_OperatorParams.Columns.Add("RegEx", typeof(string));
                        procUpdateReq.tp_OperatorParams.Columns.Add("IsOptional", typeof(bool)); procUpdateReq.tp_OperatorParams.Columns.Add("Ind", typeof(int));
                        procUpdateReq.tp_OperatorParams.Columns.Add("IsAccountNo", typeof(bool));
                        procUpdateReq.tp_OperatorParams.Columns.Add("IsCustomerNo", typeof(bool));
                        procUpdateReq.tp_OperatorParams.Columns.Add("Remark", typeof(string));
                        procUpdateReq.tp_OperatorParams.Columns.Add("IsDropDown", typeof(bool));

                        if (item.billerInputParams != null)
                        {
                            if (item.billerInputParams.Count > 0)
                            {
                                for (int i = 0; i < item.billerInputParams.Count; i++)
                                {
                                    procUpdateReq.tp_OperatorParams.Rows.Add(new object[] { item.billerInputParams[i].paramName, item.billerInputParams[i].dataType, item.billerInputParams[i].minLength, item.billerInputParams[i].maxLength, item.billerInputParams[i].regEx, item.billerInputParams[i].isOptional, i + 1, (i == 0) ? item.billerDescription ?? string.Empty : string.Empty, false });
                                }
                            }
                        }
                        procUpdateReq.tp_OperatorPaymentChanel = new DataTable();
                        procUpdateReq.tp_OperatorPaymentChanel.Columns.Add("PaymentChanel", typeof(string));
                        procUpdateReq.tp_OperatorPaymentChanel.Columns.Add("MinAmount", typeof(int));
                        procUpdateReq.tp_OperatorPaymentChanel.Columns.Add("MaxAmount", typeof(Int64));
                        if (item.billerPaymentChannels != null)
                        {
                            if (item.billerPaymentChannels.Count > 0)
                            {
                                for (int i = 0; i < item.billerPaymentChannels.Count; i++)
                                {
                                    procUpdateReq.tp_OperatorPaymentChanel.Rows.Add(new object[] { item.billerPaymentChannels[i].paymentChannelName, item.billerPaymentChannels[i].minAmount, item.billerPaymentChannels[i].maxAmount });
                                }
                            }
                        }
                        procUpdateReq.tp_OperatorDictionary = new DataTable();
                        procUpdateReq.tp_OperatorDictionary.Columns.Add("OID", typeof(int));
                        procUpdateReq.tp_OperatorDictionary.Columns.Add("ind", typeof(int));
                        procUpdateReq.tp_OperatorDictionary.Columns.Add("ParamID", typeof(int));
                        procUpdateReq.tp_OperatorDictionary.Columns.Add("DropDownValue", typeof(string));
                        if (item.billerOperatorDictionary != null)
                        {
                            if (item.billerOperatorDictionary.Count > 0)
                            {
                                for (int i = 0; i < item.billerOperatorDictionary.Count; i++)
                                {
                                    procUpdateReq.tp_OperatorDictionary.Rows.Add(new object[] { item.billerOperatorDictionary[i].OID, item.billerOperatorDictionary[i].Ind, item.billerOperatorDictionary[i].ParamID, item.billerOperatorDictionary[i].DropDownValue });
                                }
                            }
                        }
                        IProcedure procUpdateBUM = new ProcUpdateBillerInfo(_dal);
                        procUpdateBUM.Call(procUpdateReq);
                    }
                }
            }
            else if (resList[0].CommonStr3.Equals(APICode.AXISBANK))
            {
                var axsbnkML = new AxisBankBBPSML(_accessor, _env, _dal);
                foreach (var item in resList)
                {
                    var billerFields = axsbnkML.AxisBankGetBillerFields(item.CommonStr, item.CommonStr4);
                    if (billerFields.statusCode.Equals("success"))
                    {
                        var billerDetails = axsbnkML.AxisBankGetBillerDetails(item.CommonStr, item.CommonStr4);
                        if (billerDetails.statusCode.Equals("success"))
                        {
                            var procUpdateReq = new BillerInfoUpdateModel
                            {
                                OID = item.CommonInt,
                                LoginID = _lr.UserID,
                                BillerName = billerDetails.data.blrName,
                                BillerAdhoc = billerDetails.data.blrAcceptsAdhoc != "F",
                                BillerCoverage = billerDetails.data.blrCoverage,
                                IsBillValidation = billerDetails.data.supportValidationApi == null ? false : false,
                                IsAmountInValidation = false,
                                BillFetchRequirement = billerDetails.data.fetchRequirement.Equals("MANDATORY") ? 1 : 2

                            };
                            if (billerDetails.data.blrResponseParams.amountOptions != null)
                            {
                                if (billerDetails.data.blrResponseParams.amountOptions[0].amountBreakupSet != null)
                                {
                                    procUpdateReq.BillerAmountOptions = string.Join("|", billerDetails.data.blrResponseParams.amountOptions[0].amountBreakupSet);
                                }
                            }
                            if (billerDetails.data.blrPaymentModes != null)
                            {
                                procUpdateReq.BillerPaymentModes = string.Join(",", billerDetails.data.blrPaymentModes.Select(x => x.paymentMode));
                            }
                            // procUpdateReq.ExactNess = item.CommonInt2;
                            procUpdateReq.ExactNess = procUpdateReq.BillerAdhoc == true ? ExactnessType.All : ExactnessType.Exact;
                            if ((billerDetails.data.blrPmtAmtExactness ?? string.Empty).ToUpper().In("EXACT_UP", "EXACT &AMP; ABOVE"))
                            {
                                procUpdateReq.ExactNess = ExactnessType.ExactAndAbove;
                            }
                            else if ((billerDetails.data.blrPmtAmtExactness ?? string.Empty).ToUpper().In("EXACT_DOWN", "EXACT AND BELOW"))
                            {
                                procUpdateReq.ExactNess = ExactnessType.ExactAndBelow;
                            }
                            procUpdateReq.tp_OperatorParams = new DataTable();
                            procUpdateReq.tp_OperatorParams.Columns.Add("PName", typeof(string));
                            procUpdateReq.tp_OperatorParams.Columns.Add("PDataType", typeof(string));
                            procUpdateReq.tp_OperatorParams.Columns.Add("PMinLength", typeof(int));
                            procUpdateReq.tp_OperatorParams.Columns.Add("PMaxLength", typeof(int));
                            procUpdateReq.tp_OperatorParams.Columns.Add("RegEx", typeof(string));
                            procUpdateReq.tp_OperatorParams.Columns.Add("IsOptional", typeof(bool));
                            procUpdateReq.tp_OperatorParams.Columns.Add("Ind", typeof(int));
                            procUpdateReq.tp_OperatorParams.Columns.Add("IsAccountNo", typeof(bool));
                            procUpdateReq.tp_OperatorParams.Columns.Add("IsCustomerNo", typeof(bool));
                            procUpdateReq.tp_OperatorParams.Columns.Add("Remark", typeof(string));
                            procUpdateReq.tp_OperatorParams.Columns.Add("IsDropDown", typeof(bool));
                            if (billerFields.data != null)
                            {
                                if (billerFields.data.Count > 0)
                                {
                                    for (int i = 0; i < billerFields.data.Count; i++)
                                    {
                                        procUpdateReq.tp_OperatorParams.Rows.Add(new object[] { billerFields.data[i].name, billerFields.data[i].dataType, billerFields.data[i].minLength, billerFields.data[i].maxLength, billerFields.data[i].regex ?? string.Empty, billerFields.data[i].isMandatory, i + 1, (i == 0), false, (i == 0) ? billerDetails.data.blrDescription ?? string.Empty : string.Empty, false });
                                    }
                                }
                            }
                            procUpdateReq.tp_OperatorPaymentChanel = new DataTable();
                            procUpdateReq.tp_OperatorPaymentChanel.Columns.Add("PaymentChanel", typeof(string));
                            procUpdateReq.tp_OperatorPaymentChanel.Columns.Add("MinAmount", typeof(int));
                            procUpdateReq.tp_OperatorPaymentChanel.Columns.Add("MaxAmount", typeof(int));
                            if (billerDetails.data.blrPaymentChannels != null)
                            {
                                if (billerDetails.data.blrPaymentChannels.Count > 0)
                                {
                                    for (int i = 0; i < billerDetails.data.blrPaymentChannels.Count; i++)
                                    {
                                        procUpdateReq.tp_OperatorPaymentChanel.Rows.Add(new object[] { billerDetails.data.blrPaymentChannels[i].paymentChannel, billerDetails.data.blrPaymentChannels[i].minLimit ?? 0, billerDetails.data.blrPaymentChannels[i].maxLimit ?? 0 });
                                    }
                                }
                            }
                            procUpdateReq.tp_OperatorDictionary = new DataTable();
                            procUpdateReq.tp_OperatorDictionary.Columns.Add("OID", typeof(int));
                            procUpdateReq.tp_OperatorDictionary.Columns.Add("ind", typeof(int));
                            procUpdateReq.tp_OperatorDictionary.Columns.Add("ParamID", typeof(int));
                            procUpdateReq.tp_OperatorDictionary.Columns.Add("DropDownValue", typeof(string));
                            if (billerDetails.data.blrOperatorDictonary != null)
                            {
                                if (billerFields.data.Count > 0)
                                {
                                    for (int i = 0; i < billerFields.data.Count; i++)
                                    {
                                        procUpdateReq.tp_OperatorDictionary.Rows.Add(new object[] { billerDetails.data.blrOperatorDictonary[i].OID ?? 0, billerDetails.data.blrOperatorDictonary[i].Ind, billerDetails.data.blrOperatorDictonary[i].ParamID, billerDetails.data.blrOperatorDictonary[i].DropDownValue });
                                    }
                                }
                            }
                            IProcedure procUpdateBUM = new ProcUpdateBillerInfo(_dal);
                            var _res = (ResponseStatus)procUpdateBUM.Call(procUpdateReq);
                        }
                    }
                }
            }
            else if (resList[0].CommonStr3.Equals(APICode.PAYUBBPS))
            {
                var payuBBPSML = new PayuBBPSML(_accessor, _env, _dal);
                var bAResp = payuBBPSML.GetBillerByCategory(resList[0].CommonStr4);
                if (bAResp.code == 200)
                {
                    if (bAResp.payload.billers != null)
                    {
                        if (bAResp.payload.billers.Count > 0)
                        {
                            var existingBillerID = resList.Select(x => x.CommonStr).ToList();
                            var nonExistentBillerID = bAResp.payload.billers.Select(x => new AxisBankBillerListModel
                            {
                                billerId = x.billerId,
                                name = x.billerName,
                                type = x.flowType,
                                exactness = x.paymentAmountExactness
                            }).Where(w => !w.billerId.In(existingBillerID)).ToList();
                            if (nonExistentBillerID.Count > 0)
                            {
                                IProcedure proc = new ProcSaveBillerAxisBank(_dal);
                                var NotUseFulRes = (ResponseStatus)proc.Call(new SaveBillerAxisBankRequest
                                {
                                    OpTypeID = OpTypeID,
                                    billerList = nonExistentBillerID
                                });
                            }
                            var readyToSave = bAResp.payload.billers.Where(w => w.billerId.In(existingBillerID)).ToList();
                            if (readyToSave.Count > 0)
                            {
                                foreach (var item in readyToSave)
                                {
                                    var procUpdateReq = new BillerInfoUpdateModel
                                    {
                                        OID = resList.Where(x => x.CommonStr == item.billerId).Select(x => x.CommonInt).ToList()[0],
                                        LoginID = _lr.UserID,
                                        BillerName = item.billerName,
                                        BillerAdhoc = item.isAdhoc,
                                        BillerCoverage = item.regionCode,
                                        IsBillValidation = (item.supportBillValidation ?? string.Empty).Equals("SUPPORTED"),
                                        //IsAmountInValidation = (item.rechargeAmountInValidationRequest ?? string.Empty).Equals("MANDATORY")
                                    };

                                    if (item.blrResponseParams != null)
                                    {
                                        if (item.blrResponseParams.amountOptions != null)
                                        {
                                            if (item.blrResponseParams.amountOptions.Count > 0)
                                            {
                                                if (item.blrResponseParams.amountOptions[0].amountBreakupSet != null)
                                                {
                                                    procUpdateReq.BillerAmountOptions = string.Join('|', item.blrResponseParams.amountOptions[0].amountBreakupSet);
                                                }
                                            }
                                        }
                                    }
                                    if (item.paymentModesAllowed != null)
                                    {
                                        procUpdateReq.BillerPaymentModes = string.Join(',', item.paymentModesAllowed.Select(x => x.paymentMode));
                                    }
                                    procUpdateReq.ExactNess = procUpdateReq.BillerAdhoc == true ? ExactnessType.All : ExactnessType.Exact;
                                    if ((item.paymentAmountExactness ?? string.Empty).ToUpper().Equals("EXACT_AND_ABOVE"))
                                    {
                                        procUpdateReq.ExactNess = ExactnessType.ExactAndAbove;
                                    }
                                    else if ((item.paymentAmountExactness ?? string.Empty).ToUpper().Equals("EXACT_AND_BELOW"))
                                    {
                                        procUpdateReq.ExactNess = ExactnessType.ExactAndBelow;
                                    }
                                    procUpdateReq.tp_OperatorParams = new DataTable();
                                    procUpdateReq.tp_OperatorParams.Columns.Add("PName", typeof(string));
                                    procUpdateReq.tp_OperatorParams.Columns.Add("PDataType", typeof(string));
                                    procUpdateReq.tp_OperatorParams.Columns.Add("PMinLength", typeof(int));
                                    procUpdateReq.tp_OperatorParams.Columns.Add("PMaxLength", typeof(int));
                                    procUpdateReq.tp_OperatorParams.Columns.Add("RegEx", typeof(string));
                                    procUpdateReq.tp_OperatorParams.Columns.Add("IsOptional", typeof(bool));
                                    procUpdateReq.tp_OperatorParams.Columns.Add("Ind", typeof(int));
                                    procUpdateReq.tp_OperatorParams.Columns.Add("IsAccountNo", typeof(bool));
                                    procUpdateReq.tp_OperatorParams.Columns.Add("IsCustomerNo", typeof(bool));
                                    procUpdateReq.tp_OperatorParams.Columns.Add("Remark", typeof(string));
                                    procUpdateReq.tp_OperatorParams.Columns.Add("IsDropDown", typeof(bool));

                                    if (item.customerParams != null)
                                    {
                                        if (item.customerParams.Count > 0)
                                        {
                                            for (int i = 0; i < item.customerParams.Count; i++)
                                            {
                                                procUpdateReq.tp_OperatorParams.Rows.Add(new object[] { item.customerParams[i].paramName, item.customerParams[i].dataType, item.customerParams[i].minLength, item.customerParams[i].maxLength, item.customerParams[i].regex ?? string.Empty, item.customerParams[i].optional, i + 1, (i == 0), false, string.Empty, false });
                                            }
                                        }
                                    }
                                    procUpdateReq.tp_OperatorPaymentChanel = new DataTable();
                                    procUpdateReq.tp_OperatorPaymentChanel.Columns.Add("PaymentChanel", typeof(string));
                                    procUpdateReq.tp_OperatorPaymentChanel.Columns.Add("MinAmount", typeof(int));
                                    procUpdateReq.tp_OperatorPaymentChanel.Columns.Add("MaxAmount", typeof(Int64));
                                    if (item.paymentChannelsAllowed != null)
                                    {
                                        if (item.paymentChannelsAllowed.Count > 0)
                                        {
                                            for (int i = 0; i < item.paymentChannelsAllowed.Count; i++)
                                            {
                                                procUpdateReq.tp_OperatorPaymentChanel.Rows.Add(new object[] { item.paymentChannelsAllowed[i].paymentMode, item.paymentChannelsAllowed[i].minLimit * 100, item.paymentChannelsAllowed[i].maxLimit * 100 });
                                            }
                                        }
                                    }
                                    procUpdateReq.tp_OperatorDictionary = new DataTable();
                                    procUpdateReq.tp_OperatorDictionary.Columns.Add("OID", typeof(int));
                                    procUpdateReq.tp_OperatorDictionary.Columns.Add("ind", typeof(int));
                                    procUpdateReq.tp_OperatorDictionary.Columns.Add("ParamID", typeof(int)); procUpdateReq.tp_OperatorDictionary.Columns.Add("DropDownValue", typeof(string));


                                    if (item.OperatorDictionary != null)
                                    {
                                        if (item.OperatorDictionary.Count > 0)
                                        {
                                            for (int i = 0; i < item.OperatorDictionary.Count; i++)
                                            {
                                                procUpdateReq.tp_OperatorDictionary.Rows.Add(new object[] { item.OperatorDictionary[i].OID, item.OperatorDictionary[i].Ind, item.OperatorDictionary[i].ParamID, item.OperatorDictionary[i].DropDownValue });
                                            }
                                        }
                                    }



                                    IProcedure procUpdateBUM = new ProcUpdateBillerInfo(_dal);
                                    procUpdateBUM.Call(procUpdateReq);
                                }
                            }
                        }
                    }
                }
            }
            else if (resList[0].CommonStr3.Equals(APICode.RPFINTECH))
            {
                FintechAPIML fintechBBPSML = new FintechAPIML(_accessor, _env, resList[0].CommonStr3, 0, _dal);
                var fintechResp = fintechBBPSML.GetRPBillerByType(OpTypeID);
                if (fintechResp.Statuscode == ErrorCodes.One)
                {
                    if (fintechResp.data != null)
                    {
                        if (fintechResp.data.billerList.Count > 0)
                        {
                            var existingRPBillerID = resList.Select(x => x.ReffID).ToList();
                            var nonExistentBillerID = fintechResp.data.billerList.Where(w => !w.RPBillerID.In(existingRPBillerID)).ToList();
                            if (nonExistentBillerID.Count > 0)
                            {
                                foreach (var item in nonExistentBillerID)
                                {
                                    var procReq = new RPBillerInfoUpdate
                                    {
                                        AccountName = item.AccountName,
                                        AccountNoKey = item.AccountNoKey,
                                        AccountRemark = item.AccountRemark,
                                        BillerAdhoc = item.BillerAdhoc,
                                        BillerAmountOptions = item.BillerAmountOptions,
                                        BillerCoverage = item.BillerCoverage,
                                        BillerName = item.BillerName,
                                        BillerPaymentModes = item.BillerPaymentModes,
                                        EarlyPaymentAmountKey = item.EarlyPaymentAmountKey,
                                        EarlyPaymentDateKey = item.EarlyPaymentDateKey,
                                        ExactNess = item.ExactNess,
                                        IsAccountNumeric = item.IsAccountNumeric,
                                        IsAmountInValidation = item.IsAmountValidation,
                                        IsAmountOptions = item.IsAmountOptions,
                                        IsBBPS = item.IsBBPS,
                                        IsBilling = item.IsBilling,
                                        IsBillValidation = item.IsBillValidation,
                                        LatePaymentAmountKey = item.LatePaymentAmountKey,
                                        LoginID = 1,
                                        MaxAmount = item.MaxAmount,
                                        MaxLength = item.MaxLength,
                                        MinAmount = item.MinAmount,
                                        MinLength = item.MinLength,
                                        Name = item.Name,
                                        OPID = item.OPID,
                                        OpTypeID = item.OpTypeID,
                                        RegExAccount = item.RegExAccount,
                                        RPBillerID = item.RPBillerID,
                                        tp_OperatorParams = new DataTable(),
                                        tp_OperatorPaymentChanel = new DataTable()
                                    };
                                    procReq.tp_OperatorParams.Columns.Add("PName", typeof(string));
                                    procReq.tp_OperatorParams.Columns.Add("PDataType", typeof(string));
                                    procReq.tp_OperatorParams.Columns.Add("PMinLength", typeof(int));
                                    procReq.tp_OperatorParams.Columns.Add("PMaxLength", typeof(int));
                                    procReq.tp_OperatorParams.Columns.Add("RegEx", typeof(string));
                                    procReq.tp_OperatorParams.Columns.Add("IsOptional", typeof(bool));
                                    procReq.tp_OperatorParams.Columns.Add("Ind", typeof(int));
                                    procReq.tp_OperatorParams.Columns.Add("IsAccountNo", typeof(bool));
                                    procReq.tp_OperatorParams.Columns.Add("IsCustomerNo", typeof(bool));
                                    procReq.tp_OperatorParams.Columns.Add("Remark", typeof(string));
                                    procReq.tp_OperatorParams.Columns.Add("IsDropDown", typeof(bool));

                                    var opParams = fintechResp.data.billerParamList.Where(w => w.OID == item.OID).ToList();
                                    if (opParams != null)
                                    {
                                        if (opParams.Count > 0)
                                        {
                                            for (int i = 0; i < opParams.Count; i++)
                                            {
                                                procReq.tp_OperatorParams.Rows.Add(new object[] { opParams[i].ParamName, opParams[i].DataType, opParams[i].MinLength, opParams[i].MaxLength, opParams[i].RegEx ?? string.Empty, opParams[i].IsOptional, i + 1,opParams[i].IsAccountNo, opParams[i].IsCustomerNo ,
                                                    opParams[i].Remark , opParams[i].IsDropDown });
                                            }
                                        }
                                    }
                                    procReq.tp_OperatorPaymentChanel.Columns.Add("PaymentChanel", typeof(string));
                                    procReq.tp_OperatorPaymentChanel.Columns.Add("MinAmount", typeof(int));
                                    procReq.tp_OperatorPaymentChanel.Columns.Add("MaxAmount", typeof(Int64));

                                    var paymentChannelsAllowed = fintechResp.data.billerPaymentChanel.Where(w => w.OID == item.OID).ToList();
                                    if (paymentChannelsAllowed != null)
                                    {
                                        if (paymentChannelsAllowed.Count > 0)
                                        {
                                            for (int i = 0; i < paymentChannelsAllowed.Count; i++)
                                            {
                                                procReq.tp_OperatorPaymentChanel.Rows.Add(new object[] { paymentChannelsAllowed[i].PaymentChanel, paymentChannelsAllowed[i].MinAmount * 100, paymentChannelsAllowed[i].MaxAmount * 100 });
                                            }
                                        }
                                    }
                                    procReq.tp_OperatorDictionary = new DataTable();
                                    procReq.tp_OperatorDictionary.Columns.Add("OID", typeof(int));
                                    procReq.tp_OperatorDictionary.Columns.Add("ind", typeof(int));
                                    procReq.tp_OperatorDictionary.Columns.Add("ParamID", typeof(int));
                                    procReq.tp_OperatorDictionary.Columns.Add("DropDownValue", typeof(string));

                                    var OperatorDictionary = fintechResp.data.billerOperatorDictionary.Where(w => w.OID == item.OID).ToList();
                                    if (OperatorDictionary != null)
                                    {
                                        if (OperatorDictionary.Count > 0)
                                        {
                                            for (int i = 0; i < OperatorDictionary.Count; i++)
                                            {
                                                procReq.tp_OperatorDictionary.Rows.Add(new object[] { OperatorDictionary[i].OID, OperatorDictionary[i].Ind, OperatorDictionary[i].ParamID, OperatorDictionary[i].DropDownValue });
                                            }
                                        }
                                    }
                                    IProcedure procNonExistent = new ProcUpdateFintechBillerInfoNotExistent(_dal);
                                    procNonExistent.Call(procReq);
                                }
                            }
                            var readyToSave = fintechResp.data.billerList.Where(w => w.RPBillerID.In(existingRPBillerID)).ToList();

                            if (readyToSave.Count > 0)
                            {
                                foreach (var item in readyToSave)
                                {
                                    var tempOID = 0;
                                    var resListOID = resList.Where(x => x.ReffID == item.OPID).Select(x => x.CommonInt).ToList();
                                    if (resListOID.Count > 0)
                                    {
                                        tempOID = resListOID[0];
                                    }
                                    if (tempOID > 0)
                                    {
                                        var RPBillerID = resList.Where(x => x.ReffID == item.OPID).Select(x => x.ReffID).ToList()[0];
                                        var RPOID = fintechResp.data.billerList.Where(x => x.OPID == RPBillerID).Select(s => s.OID).ToList()[0];
                                        var procUpdateReq = new BillerInfoUpdateModel
                                        {
                                            OID = tempOID,
                                            LoginID = _lr.UserID,
                                            BillerName = item.BillerName,
                                            BillerAdhoc = item.BillerAdhoc,
                                            BillerCoverage = item.BillerCoverage,
                                            IsBillValidation = item.IsBillValidation,
                                            IsAmountInValidation = item.IsAmountValidation,
                                            BillerAmountOptions = item.BillerAmountOptions,
                                            BillerPaymentModes = item.BillerPaymentModes,
                                            ExactNess = item.ExactNess
                                        };

                                        procUpdateReq.tp_OperatorParams = new DataTable();
                                        procUpdateReq.tp_OperatorParams.Columns.Add("PName", typeof(string));
                                        procUpdateReq.tp_OperatorParams.Columns.Add("PDataType", typeof(string));
                                        procUpdateReq.tp_OperatorParams.Columns.Add("PMinLength", typeof(int));
                                        procUpdateReq.tp_OperatorParams.Columns.Add("PMaxLength", typeof(int));
                                        procUpdateReq.tp_OperatorParams.Columns.Add("RegEx", typeof(string));
                                        procUpdateReq.tp_OperatorParams.Columns.Add("IsOptional", typeof(bool));
                                        procUpdateReq.tp_OperatorParams.Columns.Add("Ind", typeof(int));
                                        procUpdateReq.tp_OperatorParams.Columns.Add("IsAccountNo", typeof(bool));
                                        procUpdateReq.tp_OperatorParams.Columns.Add("IsCustomerNo", typeof(bool));
                                        procUpdateReq.tp_OperatorParams.Columns.Add("Remark", typeof(string));
                                        procUpdateReq.tp_OperatorParams.Columns.Add("IsDropDown", typeof(bool));

                                        var opParams = fintechResp.data.billerParamList.Where(w => w.OID == RPOID).ToList();
                                        if (opParams != null)
                                        {
                                            if (opParams.Count > 0)
                                            {
                                                for (int i = 0; i < opParams.Count; i++)
                                                {
                                                    procUpdateReq.tp_OperatorParams.Rows.Add(new object[] { opParams[i].ParamName, opParams[i].DataType, opParams[i].MinLength, opParams[i].MaxLength, opParams[i].RegEx ?? string.Empty, opParams[i].IsOptional, i + 1, (i == 0), false, string.Empty, false });
                                                }
                                            }
                                        }
                                        procUpdateReq.tp_OperatorPaymentChanel = new DataTable();
                                        procUpdateReq.tp_OperatorPaymentChanel.Columns.Add("PaymentChanel", typeof(string));
                                        procUpdateReq.tp_OperatorPaymentChanel.Columns.Add("MinAmount", typeof(int));
                                        procUpdateReq.tp_OperatorPaymentChanel.Columns.Add("MaxAmount", typeof(Int64));

                                        var paymentChannelsAllowed = fintechResp.data.billerPaymentChanel.Where(w => w.OID == RPOID).ToList();
                                        if (paymentChannelsAllowed != null)
                                        {
                                            if (paymentChannelsAllowed.Count > 0)
                                            {
                                                for (int i = 0; i < paymentChannelsAllowed.Count; i++)
                                                {
                                                    procUpdateReq.tp_OperatorPaymentChanel.Rows.Add(new object[] { paymentChannelsAllowed[i].PaymentChanel, paymentChannelsAllowed[i].MinAmount * 100, paymentChannelsAllowed[i].MaxAmount * 100 });
                                                }
                                            }
                                        }


                                        procUpdateReq.tp_OperatorDictionary = new DataTable();
                                        procUpdateReq.tp_OperatorDictionary.Columns.Add("OID", typeof(int));
                                        procUpdateReq.tp_OperatorDictionary.Columns.Add("ind", typeof(int));
                                        procUpdateReq.tp_OperatorDictionary.Columns.Add("ParamID", typeof(int)); procUpdateReq.tp_OperatorDictionary.Columns.Add("DropDownValue", typeof(string));

                                        var OperatorDictionary = fintechResp.data.billerOperatorDictionary.Where(w => w.OID == item.OID).ToList();
                                        if (OperatorDictionary != null)
                                        {
                                            if (OperatorDictionary.Count > 0)
                                            {
                                                for (int i = 0; i < OperatorDictionary.Count; i++)
                                                {
                                                    procUpdateReq.tp_OperatorDictionary.Rows.Add(new object[] { OperatorDictionary[i].OID, OperatorDictionary[i].Ind, OperatorDictionary[i].ParamID, OperatorDictionary[i].DropDownValue });
                                                }
                                            }
                                        }

                                        IProcedure procUpdateBUM = new ProcUpdateBillerInfo(_dal);
                                        procUpdateBUM.Call(procUpdateReq);
                                    }

                                }
                            }
                        }
                    }
                }
            }
            return new ResponseStatus
            {
                Statuscode = ErrorCodes.One,
                Msg = "Bulk updation has been processed and completed"
            };
        }
        public void UpdateBillerLog(CommonReq req)
        {
            IProcedure proc = new ProcLogBillerReqResp(_dal);
            proc.Call(req);
        }
        public IResponseStatus UpdateBillerInfo(int OID)
        {
            IProcedure procChek = new ProcCheckBillerInfoAllowed(_dal);
            var res = (ResponseStatus)procChek.Call(new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID,
                CommonInt = OID
            });
            if (res.Statuscode == ErrorCodes.One)
            {
                if (res.CommonStr3.Equals(APICode.BILLAVENUE))
                {
                    BillAvenueML billAvenueML = new BillAvenueML(_accessor, _env, _dal);
                    var bAResp = billAvenueML.GetBillerInfo(res.CommonStr, res.CommonStr2, OID, 0);
                    if (bAResp.responseCode == "000")
                    {
                        var procUpdateReq = new BillerInfoUpdateModel
                        {
                            OID = OID,
                            LoginID = _lr.UserID,
                            BillerName = bAResp.biller.billerName,
                            BillerAdhoc = bAResp.biller.billerAdhoc,
                            BillerCoverage = bAResp.biller.billerCoverage,
                            BillerAmountOptions = bAResp.biller.billerAmountOptions,
                            BillerPaymentModes = bAResp.biller.billerPaymentModes,
                            IsBillValidation = (bAResp.biller.billerSupportBillValidation ?? string.Empty).Equals("MANDATORY"),
                            IsAmountInValidation = (bAResp.biller.rechargeAmountInValidationRequest ?? string.Empty).Equals("MANDATORY"),
                            BillFetchRequirement = bAResp.biller.billerFetchRequiremet.Equals("NOT_SUPPORTED") ? 2 : 1
                        };
                        procUpdateReq.ExactNess = ExactnessType.Exact;
                        if ((bAResp.biller.billerPaymentExactness ?? string.Empty).ToUpper().In("EXACT_UP", "EXACT &AMP; ABOVE"))
                        {
                            procUpdateReq.ExactNess = ExactnessType.ExactAndAbove;
                        }
                        else if ((bAResp.biller.billerPaymentExactness ?? string.Empty).ToUpper().In("EXACT_DOWN", "EXACT AND BELOW"))
                        {
                            procUpdateReq.ExactNess = ExactnessType.ExactAndBelow;
                        }
                        if (!string.IsNullOrEmpty(procUpdateReq.BillerAmountOptions))
                        {
                            procUpdateReq.BillerAmountOptions = procUpdateReq.BillerAmountOptions.Replace("|", "").Replace(",,,", "|");
                            procUpdateReq.BillerAmountOptions = procUpdateReq.BillerAmountOptions.Replace(",,", ",").Replace(",", "|");
                        }
                        procUpdateReq.tp_OperatorParams = new DataTable();
                        procUpdateReq.tp_OperatorParams.Columns.Add("PName", typeof(string));
                        procUpdateReq.tp_OperatorParams.Columns.Add("PDataType", typeof(string));
                        procUpdateReq.tp_OperatorParams.Columns.Add("PMinLength", typeof(int));
                        procUpdateReq.tp_OperatorParams.Columns.Add("PMaxLength", typeof(int));
                        procUpdateReq.tp_OperatorParams.Columns.Add("RegEx", typeof(string));
                        procUpdateReq.tp_OperatorParams.Columns.Add("IsOptional", typeof(bool));
                        procUpdateReq.tp_OperatorParams.Columns.Add("Ind", typeof(int));
                        procUpdateReq.tp_OperatorParams.Columns.Add("IsAccountNo", typeof(bool));
                        procUpdateReq.tp_OperatorParams.Columns.Add("IsCustomerNo", typeof(bool));
                        procUpdateReq.tp_OperatorParams.Columns.Add("Remark", typeof(string));
                        procUpdateReq.tp_OperatorParams.Columns.Add("IsDropDown", typeof(bool));

                        if (bAResp.biller.billerInputParams != null)
                        {
                            if (bAResp.biller.billerInputParams.Count > 0)
                            {
                                for (int i = 0; i < bAResp.biller.billerInputParams.Count; i++)
                                {
                                    procUpdateReq.tp_OperatorParams.Rows.Add(new object[] { bAResp.biller.billerInputParams[i].paramName, bAResp.biller.billerInputParams[i].dataType, bAResp.biller.billerInputParams[i].minLength, bAResp.biller.billerInputParams[i].maxLength, bAResp.biller.billerInputParams[i].regEx ?? string.Empty, bAResp.biller.billerInputParams[i].isOptional, i + 1, (i == 0), false, (i == 0) ? bAResp.biller.billerDescription ?? string.Empty : string.Empty, false });
                                }
                            }
                        }
                        procUpdateReq.tp_OperatorPaymentChanel = new DataTable();
                        procUpdateReq.tp_OperatorPaymentChanel.Columns.Add("PaymentChanel", typeof(string));
                        procUpdateReq.tp_OperatorPaymentChanel.Columns.Add("MinAmount", typeof(int));
                        procUpdateReq.tp_OperatorPaymentChanel.Columns.Add("MaxAmount", typeof(int));
                        if (bAResp.biller.billerPaymentChannels != null)
                        {
                            if (bAResp.biller.billerPaymentChannels.Count > 0)
                            {
                                for (int i = 0; i < bAResp.biller.billerPaymentChannels.Count; i++)
                                {
                                    procUpdateReq.tp_OperatorPaymentChanel.Rows.Add(new object[] { bAResp.biller.billerPaymentChannels[i].paymentChannelName, bAResp.biller.billerPaymentChannels[i].minAmount, bAResp.biller.billerPaymentChannels[i].maxAmount });
                                }
                            }
                        }


                        procUpdateReq.tp_OperatorDictionary = new DataTable();
                        procUpdateReq.tp_OperatorDictionary.Columns.Add("OID", typeof(int));
                        procUpdateReq.tp_OperatorDictionary.Columns.Add("ind", typeof(int));
                        procUpdateReq.tp_OperatorDictionary.Columns.Add("ParamID", typeof(int));
                        procUpdateReq.tp_OperatorDictionary.Columns.Add("DropDownValue", typeof(string));


                        if (bAResp.biller.BillerOperatorDictionary != null)
                        {
                            if (bAResp.biller.BillerOperatorDictionary.Count > 0)
                            {
                                for (int i = 0; i < bAResp.biller.BillerOperatorDictionary.Count; i++)
                                {
                                    procUpdateReq.tp_OperatorDictionary.Rows.Add(new object[] { bAResp.biller.BillerOperatorDictionary[i].OID, bAResp.biller.BillerOperatorDictionary[i].Ind, bAResp.biller.BillerOperatorDictionary[i].ParamID, bAResp.biller.BillerOperatorDictionary[i].DropDownValue });
                                }
                            }
                        }

                        IProcedure procUpdateBUM = new ProcUpdateBillerInfo(_dal);
                        res = (ResponseStatus)procUpdateBUM.Call(procUpdateReq);
                    }
                    else
                    {
                        res.Statuscode = ErrorCodes.Minus1;
                        res.Msg = bAResp.errorInfo != null ? bAResp.errorInfo.error.errorMessage : nameof(ErrorCodes.Unknown_Error).Replace("_", " ");
                    }
                }
                else if (res.CommonStr3.Equals(APICode.AXISBANK) && !string.IsNullOrEmpty(res.CommonStr4))
                {
                    var axsbnkML = new AxisBankBBPSML(_accessor, _env, _dal);
                    var billerFields = axsbnkML.AxisBankGetBillerFields(res.CommonStr, res.CommonStr4);
                    if (billerFields.statusCode.Equals("success"))
                    {
                        var billerDetails = axsbnkML.AxisBankGetBillerDetails(res.CommonStr, res.CommonStr4);
                        if (billerDetails.statusCode.Equals("success"))
                        {
                            var procUpdateReq = new BillerInfoUpdateModel
                            {
                                OID = OID,
                                LoginID = _lr.UserID,
                                BillerName = billerDetails.data.blrName,
                                BillerAdhoc = billerDetails.data.blrAcceptsAdhoc != "F",
                                BillerCoverage = billerDetails.data.blrCoverage,
                                IsBillValidation = billerDetails.data.supportValidationApi == null ? false : false,
                                IsAmountInValidation = false,
                                BillFetchRequirement = billerDetails.data.fetchRequirement.Equals("MANDATORY") ? 1 : 2
                            };
                            if (billerDetails.data.blrResponseParams.amountOptions != null)
                            {
                                if (billerDetails.data.blrResponseParams.amountOptions[0].amountBreakupSet != null)
                                {
                                    procUpdateReq.BillerAmountOptions = string.Join("|", billerDetails.data.blrResponseParams.amountOptions[0].amountBreakupSet);
                                }
                            }
                            if (billerDetails.data.blrPaymentModes != null)
                            {
                                procUpdateReq.BillerPaymentModes = string.Join(",", billerDetails.data.blrPaymentModes.Select(x => x.paymentMode));
                            }
                            // procUpdateReq.ExactNess = res.CommonInt2;
                            procUpdateReq.ExactNess = ExactnessType.Exact;



                            if ((billerDetails.data.blrPmtAmtExactness ?? string.Empty).ToUpper().In("EXACT_UP", "EXACT &AMP; ABOVE"))
                            {
                                procUpdateReq.ExactNess = ExactnessType.ExactAndAbove;
                            }
                            else if ((billerDetails.data.blrPmtAmtExactness ?? string.Empty).ToUpper().In("EXACT_DOWN", "EXACT AND BELOW"))
                            {
                                procUpdateReq.ExactNess = ExactnessType.ExactAndBelow;
                            }

                            procUpdateReq.tp_OperatorParams = new DataTable();
                            procUpdateReq.tp_OperatorParams.Columns.Add("PName", typeof(string));
                            procUpdateReq.tp_OperatorParams.Columns.Add("PDataType", typeof(string));
                            procUpdateReq.tp_OperatorParams.Columns.Add("PMinLength", typeof(int));
                            procUpdateReq.tp_OperatorParams.Columns.Add("PMaxLength", typeof(int));
                            procUpdateReq.tp_OperatorParams.Columns.Add("RegEx", typeof(string));
                            procUpdateReq.tp_OperatorParams.Columns.Add("IsOptional", typeof(bool));
                            procUpdateReq.tp_OperatorParams.Columns.Add("Ind", typeof(int));
                            procUpdateReq.tp_OperatorParams.Columns.Add("IsAccountNo", typeof(bool));
                            procUpdateReq.tp_OperatorParams.Columns.Add("IsCustomerNo", typeof(bool));
                            procUpdateReq.tp_OperatorParams.Columns.Add("Remark", typeof(string));
                            procUpdateReq.tp_OperatorParams.Columns.Add("IsDropDown", typeof(bool));

                            if (billerFields.data != null)
                            {
                                if (billerFields.data.Count > 0)
                                {
                                    for (int i = 0; i < billerFields.data.Count; i++)
                                    {
                                        procUpdateReq.tp_OperatorParams.Rows.Add(new object[] { billerFields.data[i].name, billerFields.data[i].dataType, billerFields.data[i].minLength, billerFields.data[i].maxLength, billerFields.data[i].regex ?? string.Empty, billerFields.data[i].isMandatory, i + 1, (i == 0), false, (i == 0) ? billerDetails.data.blrDescription ?? string.Empty : string.Empty, false });
                                    }
                                }
                            }
                            procUpdateReq.tp_OperatorPaymentChanel = new DataTable();
                            procUpdateReq.tp_OperatorPaymentChanel.Columns.Add("PaymentChanel", typeof(string));
                            procUpdateReq.tp_OperatorPaymentChanel.Columns.Add("MinAmount", typeof(int));
                            procUpdateReq.tp_OperatorPaymentChanel.Columns.Add("MaxAmount", typeof(int));
                            if (billerDetails.data.blrPaymentChannels != null)
                            {
                                if (billerDetails.data.blrPaymentChannels.Count > 0)
                                {
                                    for (int i = 0; i < billerDetails.data.blrPaymentChannels.Count; i++)
                                    {
                                        procUpdateReq.tp_OperatorPaymentChanel.Rows.Add(new object[] { billerDetails.data.blrPaymentChannels[i].paymentChannel, billerDetails.data.blrPaymentChannels[i].minLimit ?? 0, billerDetails.data.blrPaymentChannels[i].maxLimit ?? 0 });
                                    }
                                }
                            }

                            procUpdateReq.tp_OperatorDictionary = new DataTable();
                            procUpdateReq.tp_OperatorDictionary.Columns.Add("OID", typeof(int));
                            procUpdateReq.tp_OperatorDictionary.Columns.Add("ind", typeof(int));
                            procUpdateReq.tp_OperatorDictionary.Columns.Add("ParamID", typeof(int));
                            procUpdateReq.tp_OperatorDictionary.Columns.Add("DropDownValue", typeof(string));


                            if (billerDetails.data.blrOperatorDictonary != null)
                            {
                                if (billerFields.data.Count > 0)
                                {
                                    for (int i = 0; i < billerFields.data.Count; i++)
                                    {
                                        procUpdateReq.tp_OperatorDictionary.Rows.Add(new object[] { billerDetails.data.blrOperatorDictonary[i].OID ?? 0, billerDetails.data.blrOperatorDictonary[i].Ind, billerDetails.data.blrOperatorDictonary[i].ParamID, billerDetails.data.blrOperatorDictonary[i].DropDownValue });
                                    }
                                }
                            }

                            IProcedure procUpdateBUM = new ProcUpdateBillerInfo(_dal);
                            res = (ResponseStatus)procUpdateBUM.Call(procUpdateReq);
                        }
                    }
                    else
                    {
                        res.Statuscode = ErrorCodes.Minus1;
                        res.Msg = !string.IsNullOrEmpty(billerFields.statusMessage) ? billerFields.statusMessage : nameof(ErrorCodes.Unknown_Error).Replace("_", " ");
                    }
                }
                else if (res.CommonStr3.Equals(APICode.PAYUBBPS))
                {
                    res.Statuscode = ErrorCodes.Minus1;
                    res.Msg = "Only Bulk Update is possible";
                }
                else if (res.CommonStr3.Equals(APICode.RPFINTECH))
                {
                    FintechAPIML fintechBBPSML = new FintechAPIML(_accessor, _env, res.CommonStr3, 0, _dal);
                    var fintechResp = fintechBBPSML.GetRPBillerByBillerID(res.ReffID ?? string.Empty);
                    if (fintechResp.Statuscode == ErrorCodes.Two)
                    {
                        if (fintechResp.data != null)
                        {
                            if (fintechResp.data.billerList.Count > 0)
                            {
                                var readyToSave = fintechResp.data.billerList;

                                if (readyToSave.Count > 0)
                                {
                                    foreach (var item in readyToSave)
                                    {
                                        var procUpdateReq = new BillerInfoUpdateModel
                                        {
                                            OID = readyToSave[0].OID,
                                            LoginID = _lr.UserID,
                                            BillerName = item.BillerName,
                                            BillerAdhoc = item.BillerAdhoc,
                                            BillerCoverage = item.BillerCoverage,
                                            IsBillValidation = item.IsBillValidation,
                                            IsAmountInValidation = item.IsAmountValidation,
                                            BillerAmountOptions = item.BillerAmountOptions,
                                            BillerPaymentModes = item.BillerPaymentModes,
                                            ExactNess = item.ExactNess
                                        };

                                        procUpdateReq.tp_OperatorParams = new DataTable();
                                        procUpdateReq.tp_OperatorParams.Columns.Add("PName", typeof(string));
                                        procUpdateReq.tp_OperatorParams.Columns.Add("PDataType", typeof(string));
                                        procUpdateReq.tp_OperatorParams.Columns.Add("PMinLength", typeof(int));
                                        procUpdateReq.tp_OperatorParams.Columns.Add("PMaxLength", typeof(int));
                                        procUpdateReq.tp_OperatorParams.Columns.Add("RegEx", typeof(string));
                                        procUpdateReq.tp_OperatorParams.Columns.Add("IsOptional", typeof(bool));
                                        procUpdateReq.tp_OperatorParams.Columns.Add("Ind", typeof(int));
                                        procUpdateReq.tp_OperatorParams.Columns.Add("IsAccountNo", typeof(bool));
                                        procUpdateReq.tp_OperatorParams.Columns.Add("IsCustomerNo", typeof(bool));
                                        procUpdateReq.tp_OperatorParams.Columns.Add("Remark", typeof(string));
                                        procUpdateReq.tp_OperatorParams.Columns.Add("IsDropDown", typeof(bool));

                                        var opParams = fintechResp.data.billerParamList.Where(w => w.OID == readyToSave[0].OID).ToList();
                                        if (opParams != null)
                                        {
                                            if (opParams.Count > 0)
                                            {
                                                for (int i = 0; i < opParams.Count; i++)
                                                {
                                                    procUpdateReq.tp_OperatorParams.Rows.Add(new object[] { opParams[i].ParamName, opParams[i].DataType, opParams[i].MinLength, opParams[i].MaxLength, opParams[i].RegEx ?? string.Empty, opParams[i].IsOptional, i + 1,opParams[i].IsAccountNo, opParams[i].IsCustomerNo ,
                                                    opParams[i].Remark , opParams[i].IsDropDown });
                                                }
                                            }
                                        }
                                        procUpdateReq.tp_OperatorPaymentChanel = new DataTable();
                                        procUpdateReq.tp_OperatorPaymentChanel.Columns.Add("PaymentChanel", typeof(string));
                                        procUpdateReq.tp_OperatorPaymentChanel.Columns.Add("MinAmount", typeof(int));
                                        procUpdateReq.tp_OperatorPaymentChanel.Columns.Add("MaxAmount", typeof(Int64));

                                        var paymentChannelsAllowed = fintechResp.data.billerPaymentChanel.Where(w => w.OID == readyToSave[0].OID).ToList();
                                        if (paymentChannelsAllowed != null)
                                        {
                                            if (paymentChannelsAllowed.Count > 0)
                                            {
                                                for (int i = 0; i < paymentChannelsAllowed.Count; i++)
                                                {
                                                    procUpdateReq.tp_OperatorPaymentChanel.Rows.Add(new object[] { paymentChannelsAllowed[i].PaymentChanel, paymentChannelsAllowed[i].MinAmount * 100, paymentChannelsAllowed[i].MaxAmount * 100 });
                                                }
                                            }
                                        }

                                        procUpdateReq.tp_OperatorDictionary = new DataTable();
                                        procUpdateReq.tp_OperatorDictionary.Columns.Add("OID", typeof(int));
                                        procUpdateReq.tp_OperatorDictionary.Columns.Add("ind", typeof(int));
                                        procUpdateReq.tp_OperatorDictionary.Columns.Add("ParamID", typeof(int));
                                        procUpdateReq.tp_OperatorDictionary.Columns.Add("DropDownValue", typeof(string));

                                        var OperatorDictionary = fintechResp.data.billerOperatorDictionary.Where(w => w.OID == item.OID).ToList();
                                        if (OperatorDictionary != null)
                                        {
                                            if (OperatorDictionary.Count > 0)
                                            {
                                                for (int i = 0; i < OperatorDictionary.Count; i++)
                                                {
                                                    procUpdateReq.tp_OperatorDictionary.Rows.Add(new object[] { OperatorDictionary[i].OID, OperatorDictionary[i].Ind, OperatorDictionary[i].ParamID, OperatorDictionary[i].DropDownValue });
                                                }
                                            }
                                        }



                                        IProcedure procUpdateBUM = new ProcUpdateBillerInfo(_dal);
                                        procUpdateBUM.Call(procUpdateReq);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return res;
        }
        public ResponseStatus UpdateAxisBankBillerList(int OpType, string APIOpType)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AnError
            };
            var AXBML = new AxisBankBBPSML(_accessor, _env, _dal);
            var apiRes = AXBML.GetBillerList(APIOpType);
            res.Msg = apiRes.statusMessage;
            if ((apiRes.statusCode ?? string.Empty).Equals("success"))
            {
                res.Statuscode = ErrorCodes.One;
                if (apiRes.data.Count > 0)
                {
                    IProcedure proc = new ProcSaveBillerAxisBank(_dal);
                    res = (ResponseStatus)proc.Call(new SaveBillerAxisBankRequest
                    {
                        OpTypeID = OpType,
                        billerList = apiRes.data
                    });
                }
            }
            return res;
        }
        public IEnumerable<OpTypeMaster> GetAPIOpType(string APICode)
        {
            ProcGetOperator proc = new ProcGetOperator(_dal);
            return proc.APIOpTypeMasters(APICode);
        }
        public IEnumerable<IndustryTypeModel> GetIndustryWiseOpTypeList()
        {
            var res = new List<IndustryTypeModel>();
            ProcGetOperator proc = new ProcGetOperator(_dal);
            var Procres = proc.GetIndustryWiseOpTypes();
            if (Procres.Count > 0)
            {
                var distinctIndustry = Procres.Select(x => new IndustryTypeModel { ID = x.ID, IndustryType = x.IndustryType, Remark = x.Remark }).Distinct().ToList();
                foreach (var item in distinctIndustry)
                {
                    item.OpTypes = Procres.Where(w => w.ID == item.ID).Select(x => new OpTypeMaster { ID = x.OpTypeID, OpType = x.OpType }).ToList();
                    res.Add(item);
                }
            }
            return res;
        }
        public IEnumerable<OperatorDetail> GetPaymentModesOp(int LoginID)
        {
            IProcedure proc = new ProcGetPaymentModes(_dal);
            return (List<OperatorDetail>)proc.Call(new CommonReq
            {
                LoginID = LoginID
            });
        }
        public IEnumerable<OperatorDetail> GetOperators(string OpTypes)
        {
            var res = new List<OperatorDetail>();
            if (_lr.RoleID > 0)
            {
                var proc = new ProcGetOperator(_dal);
                res = proc.GetOperatorsByOpTypes(OpTypes);
                if (res.Count > 0)
                {
                    if (_lr.RoleID == Role.Customer)
                    {
                        res = res.Where(x => x.AllowedChannel == 2 || x.AllowedChannel == 3).ToList();
                    }
                    else
                    {
                        res = res.Where(x => x.AllowedChannel == 1 || x.AllowedChannel == 3).ToList();
                    }
                }
            }
            return res;
        }
        public IEnumerable<OperatorDetail> GetOperatorsActive(int Type = 0)
        {
            var res = new List<OperatorDetail>();
            if (_lr.RoleID > 0)
            {
                IProcedure proc = new ProcGetOperator(_dal);
                res = (List<OperatorDetail>)proc.Call(new CommonReq
                {
                    CommonInt = 0,
                    CommonInt2 = Type,
                    LoginID = _lr.UserID
                });
                if (res != null)
                {
                    res = res.Where(x => x.IsActive).ToList();
                }
            }
            return res;
        }
        public IEnumerable<OperatorDetail> GetActiveOperators(int LoginID, int OpType)
        {
            IProcedure proc = new ProcGetActiveOperators(_dal);
            return (List<OperatorDetail>)proc.Call(new CommonReq
            {
                LoginID = LoginID,
                CommonInt = OpType
            });
        }
        public IEnumerable<OperatorDetail> GetOperatorsByGroup(int Type)
        {
            var res = new List<OperatorDetail>();
            if (_lr.RoleID > 0)
            {
                IProcedure proc = new ProcGetOperator(_dal);
                res = (List<OperatorDetail>)proc.Call(new CommonReq
                {
                    CommonInt = 0,
                    CommonInt2 = Type,
                    LoginID = _lr.UserID
                });
                if (res.Count > 0)
                {
                    res = res.Where(x => x.IsGroupLeader).ToList();
                }
            }
            return res;
        }
        public IResponseStatus SaveOperator(OperatorDetail operatorDetail)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin || userML.IsCustomerCareAuthorised(ActionCodes.AddEditOPERATOR))
            {
                if (!operatorDetail.IsOPID && (string.IsNullOrEmpty(operatorDetail.Name) || Validators.Validate.O.IsNumeric(operatorDetail.Name)))
                {
                    res.Msg = "Operator name is non numeric mandatory field and can not be start with number.";
                    return res;
                }
                if (string.IsNullOrEmpty(operatorDetail.OPID) || operatorDetail.OPID.Trim().Length > 10)
                {
                    res.Msg = "OPID is mandatory not moere than 10 charecters";
                    return res;
                }
                var operatorRequest = new OperatorRequest
                {
                    Detail = operatorDetail,
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonStr = _rinfo.GetRemoteIP(),
                    CommonStr2 = _rinfo.GetBrowser()
                };
                IProcedure proc = new ProcOperatorCU(_dal);
                return (IResponseStatus)proc.Call(operatorRequest);
            }
            return res;
        }
        public IResponseStatus UpdateBillerID(OperatorDetail operatorDetail)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin || userML.IsCustomerCareAuthorised(ActionCodes.AddEditOPERATOR))
            {
                if (string.IsNullOrEmpty(operatorDetail.BillerID))
                {
                    res.Msg = "Invalid BillerID";
                    return res;
                }
                IProcedure proc = new ProcUpdateBillerID(_dal);
                return (IResponseStatus)proc.Call(new OperatorRequest
                {
                    Detail = operatorDetail,
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonStr = _rinfo.GetRemoteIP(),
                    CommonStr2 = _rinfo.GetBrowser()
                });
            }
            return res;
        }
        public IEnumerable<OpTypeMaster> GetOptypes(int ServiceID = 0)
        {
            ProcGetOperator proc = new ProcGetOperator(_dal);
            return proc.OpTypeMasters(ServiceID);
        }
        public IEnumerable<OpTypeMaster> GetOptypeInRange()
        {
            ProcGetOperator proc = new ProcGetOperator(_dal);
            return proc.OpTypesInRange();
        }
        public IEnumerable<OpTypeMaster> GetOptypeInSlab()
        {
            ProcGetOperator proc = new ProcGetOperator(_dal);
            return proc.OpTypesInSlab();
        }
        public IEnumerable<APIOpCode> GetAPIOpCode(int OpTypeID)
        {
            ProcGetAPIOpCodePivot _proc = new ProcGetAPIOpCodePivot(_dal);
            var aPIOpCodes = (List<APIOpCode>)_proc.Call(new CommonReq
            {
                CommonInt = OpTypeID,
                LoginID = _lr.LoginTypeID == LoginType.CustomerCare ? 1 : _lr.UserID
            });
            return aPIOpCodes;
        }
        public IResponseStatus UpdateAPIOpCode(APIOpCode aPIOpCode)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            if ((!userML.IsEndUser() && _lr.LoginTypeID == LoginType.ApplicationUser) || userML.IsCustomerCareAuthorised(ActionCodes.AddEditOPERATOR))
            {
                if (string.IsNullOrEmpty(aPIOpCode.OpCode))
                {
                    _resp.Msg = "OpCode is mandatory field!";
                    return _resp;
                }
                IProcedure _proc = new ProcUpdateAPIOpCode(_dal);
                _resp = (ResponseStatus)_proc.Call(new APIOpCodeReq
                {
                    aPIOpCode = aPIOpCode,
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonStr = _rinfo.GetRemoteIP(),
                    CommonStr2 = _rinfo.GetBrowser(),
                    CommonStr3 = aPIOpCode.BillOpCode
                });
            }
            return _resp;
        }
        public IEnumerable<APIOpCode> GetAPIOpCodeCircle(int OID, int APIID)
        {
            IProcedure _proc = new ProcGetAPIOpCodeCircle(_dal);
            var aPIOpCodes = (List<APIOpCode>)_proc.Call(new CommonReq
            {
                CommonInt = APIID,
                CommonInt2 = OID
            });
            return aPIOpCodes;
        }
        public IResponseStatus UpdateAPIOpCodeCircle(APIOpCode aPIOpCode)
        {
            var _resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };

            if ((_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser) || userML.IsCustomerCareAuthorised(ActionCodes.AddEditOPERATOR))
            {
                if (string.IsNullOrEmpty(aPIOpCode.OpCode))
                {
                    _resp.Msg = "OpCode is mandatory field!";
                    return _resp;
                }
                var req = new APIOpCodeReq
                {
                    aPIOpCode = aPIOpCode,
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonStr = _rinfo.GetRemoteIP(),
                    CommonStr2 = _rinfo.GetBrowser()
                };
                IProcedure _proc = new ProcUpdateAPIOpCodeCircle(_dal);
                _resp = (ResponseStatus)_proc.Call(req);
            }
            return _resp;
        }
        public async Task<IEnumerable<OperatorDetail>> GetOPListBYServices(string ServiceTypeIDs)
        {
            IProcedureAsync _proc = new ProcGetOpListBySerivces(_dal);
            return (List<OperatorDetail>)await _proc.Call(ServiceTypeIDs).ConfigureAwait(false);
        }

        public OperatorParamModels OperatorOptional(int OID)
        {
            IProcedure _proc = new ProcGetOperatorOptional(_dal);
            return (OperatorParamModels)_proc.Call(new CommonReq
            {
                LoginID = _lr.UserID,
                CommonInt = OID,
                LoginTypeID = _lr.LoginTypeID
            });
        }

        public IEnumerable<OperatorOptional> OperatorOption(int OID)
        {
            var commonReq = new CommonReq
            {
                LoginID = _lr.UserID,
                CommonInt = OID,
                LoginTypeID = _lr.LoginTypeID
            };
            IProcedure _proc = new procGetOperatorOption(_dal);
            return (List<OperatorOptional>)_proc.Call(commonReq);
        }
        public ResponseStatus DeleteOperatorOption(int OptionType, int OID)
        {
            var commonReq = new CommonReq
            {
                LoginID = _lr.UserID,
                CommonInt = OptionType,
                CommonInt2 = OID,
                LoginTypeID = _lr.LoginTypeID
            };
            IProcedure _proc = new ProcDelOperatorOption(_dal);
            return (ResponseStatus)_proc.Call(commonReq);
        }

        public OperatorParamModels OperatorOptionalApp(CommonReq commonReq)
        {
            IProcedure _proc = new ProcGetOperatorOptional(_dal);
            return (OperatorParamModels)_proc.Call(commonReq);
        }

        public IResponseStatus UpdateOption(OperatorOptionalReq req)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin || userML.IsCustomerCareAuthorised(ActionCodes.AddEditOPERATOR))
            {
                if (string.IsNullOrWhiteSpace(req.Remark) || req.Remark.Length > 200)
                {
                    res.Msg = ErrorCodes.InvalidParam + " Remark";
                    return res;
                }
                if (string.IsNullOrWhiteSpace(req.DisplayName) || req.DisplayName.Length > 500)
                {
                    res.Msg = ErrorCodes.InvalidParam + " DisplayName";
                    return res;
                }
                req.LoginID = _lr.UserID;
                req.LT = _lr.LoginTypeID;
                IProcedure _proc = new ProcUpdateOption(_dal);
                res = (ResponseStatus)_proc.Call(req);
            }
            return res;
        }

        public async Task<IEnumerable<NumberSeries>> NumberList()
        {
            IProcedureAsync _proc = new ProcGetNumberSeries(_dal);
            return (List<NumberSeries>)await _proc.Call();
        }

        public async Task<IEnumerable<CirlceMaster>> CircleList()
        {
            IProcedureAsync _proc = new ProcGetCircle(_dal);
            return (List<CirlceMaster>)await _proc.Call();
        }

        public IEnumerable<OperatorDetail> GetOperatorsApp(int RoleID)
        {
            var req = new CommonReq
            {
                CommonInt = 0,
                CommonInt2 = 0
            };
            IProcedure proc = new ProcGetOperator(_dal);
            var res = (List<OperatorDetail>)proc.Call(req);
            if (RoleID == Role.Customer)
            {
                res.Where(x => x.AllowedChannel == 2 || x.AllowedChannel == 3);
            }
            else
            {
                res.Where(x => x.AllowedChannel == 1 || x.AllowedChannel == 3);
            }
            return res;
        }

        public async Task<IEnumerable<string>> GetDowns()
        {
            if (_lr.LoginTypeID != LoginType.ApplicationUser)
            {
                return new List<string>();
            }
            IProcedureAsync proc = new ProcGetDownOperators(_dal);
            return (List<string>)await proc.Call();
        }

        public IResponseStatus UpdateBlockDenomination(APISwitched switched)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.APISwitch))
            {
                var req = new APISwitchedReq
                {
                    aPISwitched = switched,
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonStr = _rinfo.GetRemoteIP(),
                    CommonStr2 = _rinfo.GetBrowser()
                };
                IProcedure _proc = new ProcUpdateBlockDenomination(_dal);
                _res = (ResponseStatus)_proc.Call(req);
            }
            return _res;
        }

        public ApiOperatorOptionalMappingModel AOPMapping(int A, int O)
        {
            var _res = new ApiOperatorOptionalMappingModel
            {
                StatusCode = ErrorCodes.Minus1,
                Msg = ErrorCodes.FAILED
            };
            if (!userML.IsEndUser() && _lr.LoginTypeID == LoginType.ApplicationUser || _lr.RoleID == Role.Retailor_Seller && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.APISwitch))
            {
                var req = new GetApiOptionalParam
                {
                    _APIID = A,
                    _OID = O,
                    LoginID = _lr.LoginTypeID == LoginType.CustomerCare ? 1 : _lr.UserID
                };
                IProcedure _proc = new ProcGetApiOperatorOptionalMapping(_dal);
                _res = (ApiOperatorOptionalMappingModel)_proc.Call(req);
            }
            return _res;
        }
        public ApiOperatorOptionalMappingModel AOPMappingAPP(int A, int O)
        {
            var _res = new ApiOperatorOptionalMappingModel
            {
                StatusCode = ErrorCodes.Minus1,
                Msg = ErrorCodes.FAILED
            };

            var req = new GetApiOptionalParam
            {
                _APIID = A,
                _OID = O,
                LoginID = _lr != null ? (_lr.LoginTypeID == LoginType.CustomerCare ? 1 : _lr.UserID) : 1
            };
            IProcedure _proc = new ProcGetApiOperatorOptionalMapping(_dal);
            _res = (ApiOperatorOptionalMappingModel)_proc.Call(req);

            return _res;
        }

        public IResponseStatus SaveAOPMapping(ApiOperatorOptionalMappingModel model)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.FAILED
            };
            if (!userML.IsEndUser() && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.APISwitch))
            {
                model.LoginID = _lr.LoginTypeID == LoginType.CustomerCare ? 1 : _lr.UserID;
                IProcedure _proc = new ProcSaveApiOptionalParam(_dal);
                _res = (ResponseStatus)_proc.Call(model);
            }
            return _res;
        }
        public IEnumerable<RangeModel> GetRange()
        {
            IProcedure _proc = new ProcGetRange(_dal);
            return (List<RangeModel>)_proc.Call(new CommonReq());
        }
        public RangeModel GetRange(int ID)
        {
            if (ID > 0)
            {
                IProcedure _proc = new ProcGetRange(_dal);
                return (RangeModel)_proc.Call(new CommonReq { CommonInt = ID });
            }
            return new RangeModel();
        }

        public IResponseStatus SaveRange(RangeModel rangeDetail)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin || userML.IsCustomerCareAuthorised(ActionCodes.AddEditOPERATOR))
            {

                var rangeModelReq = new RangeModelReq
                {
                    Detail = rangeDetail,
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonStr = _rinfo.GetRemoteIP(),
                    CommonStr2 = _rinfo.GetBrowser()
                };
                IProcedure proc = new ProcRangeCU(_dal);
                return (IResponseStatus)proc.Call(rangeModelReq);
            }
            return res;
        }
        #region Denomination
        public IEnumerable<DenominationModal> GetDenomination()
        {
            IProcedure _proc = new ProcGetDenomination(_dal);
            return (List<DenominationModal>)_proc.Call(new CommonReq());
        }

        public DenominationModal GetDenomination(int ID)
        {
            if (ID > 0)
            {
                IProcedure _proc = new ProcGetDenomination(_dal);
                return (DenominationModal)_proc.Call(new CommonReq { CommonInt = ID });
            }
            return new DenominationModal();
        }

        public IResponseStatus SaveDenom(DenominationModal denomDetail)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin || userML.IsCustomerCareAuthorised(ActionCodes.AddEditOPERATOR))
            {
                var rangeModelReq = new DenominationModalReq
                {
                    Detail = denomDetail,
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonStr = _rinfo.GetRemoteIP(),
                    CommonStr2 = _rinfo.GetBrowser()
                };
                IProcedure proc = new ProcSaveDenomination(_dal);
                return (IResponseStatus)proc.Call(rangeModelReq);
            }
            return res;
        }

        public List<DenomDetailByRole> GetDenomDetailByRole(DenomDetailByRole DetailReq)
        {
            var res = new List<DenomDetailByRole>();
            if (!_lr.RoleID.In(Role.APIUser, Role.Retailor_Seller))
            {
                var Request = new DenomDetailReq
                {
                    Detail = DetailReq,
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonStr = _rinfo.GetRemoteIP(),
                    CommonStr2 = _rinfo.GetBrowser()
                };
                IProcedure proc = new ProcGetDenomDetailByRole(_dal);
                res = (List<DenomDetailByRole>)proc.Call(Request);
            }
            return res;
        }

        public IResponseStatus SaveDenomDetailByRole(DenomDetailByRole Detail)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin || userML.IsCustomerCareAuthorised(ActionCodes.AddEditOPERATOR))
            {

                var rangeModelReq = new DenomDetailReq
                {
                    Detail = Detail,
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonStr = _rinfo.GetRemoteIP(),
                    CommonStr2 = _rinfo.GetBrowser()
                };
                IProcedure proc = new ProcSaveDenomDetailByRole(_dal);
                return (IResponseStatus)proc.Call(rangeModelReq);
            }
            return res;
        }

        public DenominationRangeList GetDenominationRange()
        {
            IProcedure _proc = new ProcGetDenominationRange(_dal);
            return (DenominationRangeList)_proc.Call(new CommonReq());
        }

        public DenominationRange GetDenominationRange(int ID)
        {
            if (ID > 0)
            {
                IProcedure _proc = new ProcGetDenominationRange(_dal);
                return (DenominationRange)_proc.Call(new CommonReq { CommonInt = ID });
            }
            return new DenominationRange();
        }

        public IResponseStatus SaveDenomRange(DenominationRange denomRange)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin || userML.IsCustomerCareAuthorised(ActionCodes.AddEditOPERATOR))
            {
                var rangeModelReq = new DenominationRangeReq
                {
                    Detail = denomRange,
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonStr = _rinfo.GetRemoteIP(),
                    CommonStr2 = _rinfo.GetBrowser()
                };
                IProcedure proc = new ProcSaveDenominationRange(_dal);
                return (IResponseStatus)proc.Call(rangeModelReq);
            }
            return res;
        }
        #endregion

        public IResponseStatus SaveTollFree(OperatorDetail operatorDetail)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin || userML.IsCustomerCareAuthorised(ActionCodes.AddEditOPERATOR))
            {
                if (string.IsNullOrEmpty(operatorDetail.TollFree) || operatorDetail.TollFree.Trim().Length > 150)
                {
                    res.Msg = "TollFree is empty or not more than 150 charecters";
                    return res;
                }
                var operatorRequest = new OperatorRequest
                {
                    Detail = operatorDetail,
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonStr = _rinfo.GetRemoteIP(),
                    CommonStr2 = _rinfo.GetBrowser()
                };
                IProcedure proc = new ProcOperatorCU(_dal);
                return (IResponseStatus)proc.Call(operatorRequest);
            }
            return res;
        }

        public IEnumerable<OperatorDetail> GetMobileTollFree()
        {
            var res = new List<OperatorDetail>();
            if (_lr.RoleID > 0)
            {

                IProcedure proc = new ProcGetMobileTollFree(_dal);
                res = (List<OperatorDetail>)proc.Call();
            }
            return res;
        }
        public IEnumerable<OperatorDetail> GetDTHTollFree()
        {
            var res = new List<OperatorDetail>();
            if (_lr.RoleID > 0)
            {
                CommonReq req = new CommonReq
                {
                    CommonInt = 0,
                    CommonInt2 = 3
                };
                IProcedure proc = new ProcGetOperator(_dal);
                res = (List<OperatorDetail>)proc.Call(req);
            }
            return res;
        }
        #region Target
        public List<TargetModel> GetTarget(TargetModel DetailReq)
        {
            var res = new List<TargetModel>();
            if (!_lr.RoleID.In(Role.APIUser, Role.Retailor_Seller))
            {
                var Request = new TargetModelReq
                {
                    Detail = DetailReq,
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonStr = _rinfo.GetRemoteIP(),
                    CommonStr2 = _rinfo.GetBrowser()
                };
                IProcedure proc = new ProcGetTarget(_dal);
                res = (List<TargetModel>)proc.Call(Request);
            }
            return res;
        }
        public IResponseStatus SaveTarget(TargetModel rangeDetail)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin || userML.IsCustomerCareAuthorised(ActionCodes.AddEditOPERATOR))
            {

                var rangeModelReq = new TargetModelReq
                {
                    Detail = rangeDetail,
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonStr = _rinfo.GetRemoteIP(),
                    CommonStr2 = _rinfo.GetBrowser()
                };
                IProcedure proc = new ProcSaveTarget(_dal);
                return (IResponseStatus)proc.Call(rangeModelReq);
            }
            return res;
        }
        public IResponseStatus UploadGift(IFormFile file, string filename)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.IsWebsite && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                if (!file.ContentType.Any())
                {
                    _res.Msg = "File not found!";
                    return _res;
                }
                if (file.Length < 1)
                {
                    _res.Msg = "Empty file not allowed!";
                    return _res;
                }
                if (file.Length / 1024 > 1024)
                {
                    _res.Msg = "File size exceeded! Not more than 1 MB is allowed";
                    return _res;
                }
                string ext = Path.GetExtension(filename);
                byte[] filecontent = null;
                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    filecontent = ms.ToArray();
                }

                if (!Validate.O.IsFileAllowed(filecontent, ext))
                {
                    _res.Msg = "Invalid File Format!";
                    return _res;
                }
                try
                {
                    if (!Directory.Exists(DOCType.GiftImgPath))
                    {
                        Directory.CreateDirectory(DOCType.GiftImgPath);
                    }
                    string[] Extensions = { ".png", ".jpeg", ".jpg" };
                    foreach (string s in Extensions)
                    {
                        string ExistFile = DOCType.GiftImgPath + filename.Split('.')[0] + s;
                        if (File.Exists(ExistFile))
                        {
                            File.Delete(ExistFile);
                        }
                    }
                    StringBuilder sb = new StringBuilder();
                    sb.Append(DOCType.GiftImgPath);
                    sb.Append(filename);
                    using (FileStream fs = File.Create(sb.ToString()))
                    {
                        file.CopyTo(fs);
                        fs.Flush();
                    }
                    _res.Statuscode = ErrorCodes.One;
                    _res.Msg = "Image uploaded successfully";
                }
                catch (Exception ex)
                {
                    _res.Msg = "Error in Image uploading. Try after sometime...";
                    ErrorLog errorLog = new ErrorLog
                    {
                        ClassName = GetType().Name,
                        FuncName = "UploadLogo",
                        Error = ex.Message,
                        LoginTypeID = _lr.LoginTypeID,
                        UserId = _lr.UserID
                    };
                    var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                }
            }
            return _res;
        }

        public TargetModel GetTargetByRole(TargetModel DetailReq)
        {
            var res = new TargetModel();
            if (!_lr.RoleID.In(Role.APIUser, Role.Retailor_Seller))
            {
                var Request = new TargetModelReq
                {
                    Detail = DetailReq,
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonStr = _rinfo.GetRemoteIP(),
                    CommonStr2 = _rinfo.GetBrowser()
                };
                IProcedure proc = new ProcGetTargetByRole(_dal);
                res = (TargetModel)proc.Call(Request);
            }
            return res;
        }
        #endregion
        public APIDenomination GetApiDenom(APIDenominationReq req)
        {
            req.LoginTypeID = _lr.LoginTypeID;
            req.LoginID = _lr.UserID;
            IProcedure _proc = new ProcGetApiDenom(_dal);
            return (APIDenomination)_proc.Call(req);
        }
        public APIDenomination GetApiDenomUser(APIDenominationReq req)
        {
            req.LoginTypeID = _lr.LoginTypeID;
            req.LoginID = _lr.UserID;
            IProcedure _proc = new ProcGetApiDenomUser(_dal);
            return (APIDenomination)_proc.Call(req);
        }
        public IEnumerable<ServiceMaster> GetServices()
        {
            ProcGetServices _proc = new ProcGetServices(_dal);
            List<ServiceMaster> _Services = (List<ServiceMaster>)_proc.Call();
            return _Services;
        }
        public int GetServiceByOpTypeID(int ID)
        {
            ProcGetServices _proc = new ProcGetServices(_dal);
            return (int)_proc.GetServiceID(ID);
        }

        #region Package
        public IEnumerable<DTHPackage> GetDTHPackage(int ID, int OID)
        {
            var req = new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID,
                CommonInt = ID,
                CommonInt2 = OID,
            };
            var _proc = new ProcGetDTHPackage(_dal);
            var _res = (List<DTHPackage>)_proc.Call(req);
            return _res;
        }

        public IEnumerable<DTHChannelCategory> GetDTHChannelCategory(int ID)
        {
            CommonReq req = new CommonReq
            {
                CommonInt = ID
            };
            var _proc = new ProcGetDTHChannelCategory(_dal);
            var _res = (List<DTHChannelCategory>)_proc.Call(req);
            return _res;
        }

        public IResponseStatus SaveDTHPackage(DTHPackage req)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin || userML.IsCustomerCareAuthorised(ActionCodes.AddEditOPERATOR))
            {
                if (req.OID == 0 && req.IsActive == false)
                {
                    res.Msg = "Please Select Operator.";
                }
                else if ((req.PackageName == "" || req.PackageName == null) && req.IsActive == false)
                {
                    res.Msg = "Please Enter Package Name.";
                }
                else if (req.PackageMRP == 0 && req.IsActive == false)
                {
                    res.Msg = "Please Enter Package MRP.";
                }
                else if (req.BookingAmount == 0 && req.IsActive == false)
                {
                    res.Msg = "Please Enter Booking Amount.";
                }
                else if (req.Validity == 0 && req.IsActive == false)
                {
                    res.Msg = "Please Enter Validity in Days.";
                }
                else
                {
                    var rangeModelReq = new DTHPackageReq
                    {
                        LoginTypeID = _lr.LoginTypeID,
                        LoginID = _lr.UserID,
                        ID = req.ID,
                        OID = req.OID,
                        PackageName = req.PackageName,
                        PackageMRP = req.PackageMRP,
                        BookingAmount = req.BookingAmount,
                        Validity = req.Validity,
                        Description = req.Description,
                        Remark = req.Remark,
                        IsActive = req.IsActive,
                        FRC = req.FRC,
                    };
                    IProcedure proc = new ProcSaveDTHPackage(_dal);
                    return (IResponseStatus)proc.Call(rangeModelReq);
                }
            }
            return res;
        }


        public IResponseStatus SaveDTHChannelCategory(DTHChannelCategory req)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin || userML.IsCustomerCareAuthorised(ActionCodes.AddEditOPERATOR))
            {
                if (req.Category == "" && req.Del == false)
                {
                    res.Msg = "Please Select Operator.";
                }
                else
                {
                    var rangeModelReq = new CommonReq
                    {
                        LoginTypeID = _lr.LoginTypeID,
                        LoginID = _lr.UserID,
                        CommonInt = req.ID,
                        CommonStr = req.Category,
                        CommonBool = req.Del,
                    };
                    IProcedure proc = new ProcSaveDTHChannelCategory(_dal);
                    return (IResponseStatus)proc.Call(rangeModelReq);
                }
            }
            return res;
        }

        public IResponseStatus SaveBulkDTHPackage(List<DTHPackage> req)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin || userML.IsCustomerCareAuthorised(ActionCodes.AddEditOPERATOR))
            {
                var dt = ConverterHelper.O.ToDataTable(req);
                dt.Columns.Remove("OpTypeID");
                dt.Columns.Remove("Del");
                dt.Columns.Remove("Operator");
                dt.Columns.Remove("ID");
                var rangeModelReq = new DTHPackageReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    PackageTable = dt
                };
                IProcedure proc = new ProcSaveBulkDTHPackage(_dal);
                return (IResponseStatus)proc.Call(rangeModelReq);
            }
            return res;
        }

        public IEnumerable<DTHChannel> GetDTHChannel(int ID)
        {
            CommonReq req = new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID,
                CommonInt = ID
            };
            var _proc = new ProcGetChannel(_dal);
            var _res = (List<DTHChannel>)_proc.Call(req);
            return _res;
        }

        public IEnumerable<ChannelCategory> GetChannelCategory()
        {
            var _proc = new ProcGetChannelCategory(_dal);
            var _res = (List<ChannelCategory>)_proc.Call();
            return _res;
        }

        public IResponseStatus SaveDTHChannel(DTHChannel req)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin || userML.IsCustomerCareAuthorised(ActionCodes.AddEditOPERATOR))
            {
                if (req.CategoryID == 0 && !req.Del)
                {
                    res.Msg = "Please Select Channel Category.";
                }
                else if ((req.ChannelName == "" || req.ChannelName == null) && !req.Del)
                {
                    res.Msg = "Please Select Channel Name.";
                }
                else
                {
                    var rangeModelReq = new DTHChannelReq
                    {
                        LoginTypeID = _lr.LoginTypeID,
                        LoginID = _lr.UserID,
                        ID = req.ID,
                        ChannelName = req.ChannelName,
                        CategoryID = req.CategoryID,
                        Del = req.Del
                    };
                    IProcedure proc = new ProcSaveDTHChannel(_dal);
                    return (IResponseStatus)proc.Call(rangeModelReq);
                }
            }
            return res;
        }

        public IEnumerable<DTHChannelMap> MapChannelToPack(int PackageID)
        {
            CommonReq req = new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID,
                CommonInt = PackageID
            };
            var _proc = new ProcGetMappedChannel(_dal);
            var _Channels = (List<DTHChannel>)_proc.Call(req);
            var _proc2 = new ProcGetChannelCategory(_dal);
            var _Categories = (List<ChannelCategory>)_proc2.Call();
            var _FinalList = new List<DTHChannelMap>();
            foreach (var i in _Categories)
            {
                var _res = new DTHChannelMap
                {
                    PackageID = PackageID,
                    CategoryID = i.ID,
                    CategoryName = i.CategoryName,
                    Channels = _Channels.Where(x => x.CategoryID == i.ID).ToList(),
                };
                _FinalList.Add(_res);
            }
            return _FinalList;
        }

        public IResponseStatus SaveChannelMapping(DTHChannelMap req)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin || userML.IsCustomerCareAuthorised(ActionCodes.AddEditOPERATOR))
            {
                var Req = new DTHChannelMap
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    ChannelID = req.ChannelID,
                    PackageID = req.PackageID,
                    IsActive = req.IsActive
                };
                IProcedure proc = new ProcSaveChannelMapping(_dal);
                return (IResponseStatus)proc.Call(Req);
            }
            return res;
        }

        public string getMaxOpCode(int OpType)
        {
            string res = string.Empty;
            var proc = new ProcGetOperator(_dal);
            res = (string)proc.getMaxOpCode(OpType);
            return res;
        }

        public IResponseStatus changeValidationType(int OID, int CircleValidationType)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin || userML.IsCustomerCareAuthorised(ActionCodes.AddEditOPERATOR))
            {
                var proc = new ProcOperatorCU(_dal);
                return (IResponseStatus)proc.changeValidationType(OID, CircleValidationType);
            }
            return res;
        }

        public IResponseStatus UploadOperatorPDF(IFormFile file, int OID, LoginResponse _lr)
        {
            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.RoleID == Role.Admin && _lr.LoginTypeID == LoginType.ApplicationUser)
            {
                if (!file.ContentType.Any())
                {
                    _res.Msg = "File not found!";
                    return _res;
                }
                if (file.Length < 1)
                {
                    _res.Msg = "Empty file not allowed!";
                    return _res;
                }
                if ((file.Length / 1024) > 1024)
                {
                    _res.Msg = "File size exceeded! Not more than 1 MB is allowed";
                    return _res;
                }
                var filename = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                string ext = Path.GetExtension(filename);
                string[] allowedExt = { ".PDF", ".pdf", ".png", ".PNG", ".jpeg", ".JPEG", ".jpg", ".JPG" };
                if (!allowedExt.Contains(ext))
                {
                    _res.Msg = "Only PDF file is allowed!";
                    return _res;
                }
                byte[] filecontent = null;
                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    filecontent = ms.ToArray();
                }
                if (!Validate.O.IsFileAllowed(filecontent, ext))
                {
                    _res.Msg = "Invalid File Format!";
                    return _res;
                }
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(DOCType.OperatorPdfFile);
                    sb.Append(OID);
                    sb.Append(ext);
                    using (FileStream fs = File.Create(sb.ToString()))
                    {
                        file.CopyTo(fs);
                        fs.Flush();
                    }
                    _res.Statuscode = ErrorCodes.One;
                    _res.Msg = "Operator Plan uploaded successfully";
                }
                catch (Exception ex)
                {
                    _res.Msg = "Error in Operator Plan uploading. Try after sometime...";
                    ErrorLog errorLog = new ErrorLog
                    {
                        ClassName = GetType().Name,
                        FuncName = "UploadOperatorPlan",
                        Error = ex.Message,
                        LoginTypeID = _lr.LoginTypeID,
                        UserId = _lr.UserID
                    };
                    var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                }
            }
            return _res;
        }
        #endregion
        public DTHPackageResponse GetDTHPackageForApp(DTHPackageRequest AppReq)
        {
            var resp = new DTHPackageResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            var req = new CommonReq
            {
                LoginTypeID = LoginType.ApplicationUser,
                LoginID = AppReq.UserID,
                CommonInt2 = AppReq.OID,
            };
            var _proc = new ProcGetDTHPackage(_dal);
            var res = (List<DTHPackage>)_proc.Call(req);
            if (res != null)
            {
                resp.Statuscode = ErrorCodes.One;
                resp.Msg = "Succsses";
                resp.DTHPackage = res;
            }
            return resp;
        }
        public IEnumerable<ChannelUnderCategory> DTHChannelByPackage(int ID)
        {
            CommonReq req = new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID,
                CommonInt = ID
            };
            var _proc = new ProcGetChannelByPackage(_dal);
            var Channels = (List<DTHChannel>)_proc.Call(req);

            var proc = new ProcGetDTHChannelCategory(_dal);
            var DthCat = (List<DTHChannelCategory>)proc.Call(new CommonReq());
            var FinalList = new List<ChannelUnderCategory>();
            if (DthCat != null)
            {
                foreach (var item in DthCat)
                {
                    ChannelUnderCategory data = new ChannelUnderCategory
                    {
                        ID = item.ID,
                        Category = item.Category,
                        channels = Channels.Where(x => x.CategoryID == item.ID)
                    };
                    FinalList.Add(data);
                }
            }
            return FinalList;
        }

        public DTHChannelResponse DTHChannelByPackageForApp(DTHChannelRequest AppReq)
        {
            var resp = new DTHChannelResponse
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            CommonReq req = new CommonReq
            {
                LoginTypeID = AppReq.LoginTypeID,
                LoginID = AppReq.UserID,
                CommonInt = AppReq.PID
            };
            var _proc = new ProcGetChannelByPackage(_dal);
            var res = (List<DTHChannel>)_proc.Call(req);
            if (res != null)
            {
                resp.Statuscode = ErrorCodes.One;
                resp.Msg = "success";
                resp.DTHChannels = res;
            }
            return resp;
        }

        public IEnumerable<OperatorDetail> GetOperatorsByService(string SCode)
        {
            IProcedure proc = new ProcGetOperatorByService(_dal);
            var res = (List<OperatorDetail>)proc.Call(new CommonReq
            {
                CommonStr = SCode
            });
            return res;
        }
        public List<TargetModel> ShowGiftImages()
        {
            var res = new List<TargetModel>();

            var req = new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID
            };
            IProcedure proc = new ProcShowGiftImages(_dal);
            res = (List<TargetModel>)proc.Call(req);

            return res;
        }
        public IEnumerable<OperatorOptionalStuff> OperatorOptionalStuff(int OPID = 0)
        {
            IProcedure proc = new ProcOperatorOptionalStuff(_dal);
            var res = (List<OperatorOptionalStuff>)proc.Call(OPID);
            return res;
        }
        public IEnumerable<DTHPackage> GetDTHDiscription(int PID, int OID)
        {
            var req = new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID,
                CommonInt = PID,
                CommonInt2 = OID,
            };
            var _proc = new ProcGetDTHPackage(_dal);
            var _res = (List<DTHPackage>)_proc.Call(req);
            return _res;
        }

        public async Task<IEnumerable<APICircleCode>> APICircleCode()
        {
            IProcedureAsync _proc = new ProcGetAPICircleCode(_dal);
            var res = (List<APICircleCode>)await _proc.Call();
            return res;
        }
        public IResponseStatus SaveAPICircleCode(APICircleCode req)
        {
            req.LoginID = _lr.UserID;
            req.LT = _lr.LoginTypeID;
            IProcedure _proc = new ProcSaveAPICircleCode(_dal);
            var res = (ResponseStatus)_proc.Call(req);
            return res;
        }
        public async Task<IEnumerable<CirlceMaster>> CircleListWithAll()
        {
            IProcedureAsync _proc = new ProcGetCircleWithAll(_dal);
            return (List<CirlceMaster>)await _proc.Call();
        }
        public async Task<IEnumerable<CircleWithDomination>> GetCircleWithDominations(CircleWithDomination req)
        {
            req.LT = _lr.LoginTypeID;
            req.LoginID = _lr.UserID;
            IProcedureAsync proc = new ProcGetCircleWithDomination(_dal);
            var res = (List<CircleWithDomination>)await proc.Call(req).ConfigureAwait(false);
            return res;
        }
        public async Task<IEnumerable<CircleWithDomination>> GetCircleWithDominationsAPI(CircleWithDomination req)
        {
            req.LT = _lr.LoginTypeID;
            req.LoginID = _lr.UserID;
            IProcedureAsync proc = new ProcGetCircleWithDominationAPI(_dal);
            var res = (List<CircleWithDomination>)await proc.Call(req).ConfigureAwait(false);
            return res;
        }
        public async Task<IEnumerable<CircleWithDomination>> GetRemainDominationsSpecialSlab(CircleWithDomination req)
        {
            req.LT = _lr.LoginTypeID;
            req.LoginID = _lr.UserID;
            IProcedureAsync proc = new ProcGetRemainDominationSpecial(_dal);
            var res = (List<CircleWithDomination>)await proc.Call(req).ConfigureAwait(false);
            return res;
        }
        public async Task<IEnumerable<CircleWithDomination>> GetRemainDominationsSpecialSlabAPI(CircleWithDomination req)
        {
            req.LT = _lr.LoginTypeID;
            req.LoginID = _lr.UserID;
            IProcedureAsync proc = new ProcGetRemainDominationSpecialAPI(_dal);
            var res = (List<CircleWithDomination>)await proc.Call(req).ConfigureAwait(false);
            return res;
        }
        public IResponseStatus SaveSpecialSlabDetail(CircleWithDomination Detail)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin || userML.IsCustomerCareAuthorised(ActionCodes.AddEditOPERATOR))
            {
                Detail.LT = _lr.LoginTypeID;
                Detail.LoginID = _lr.UserID;
                if (!string.IsNullOrEmpty(Detail.DenomIDs) && Detail.DenomIDs != "[]")
                {
                    var doc = JsonConvert.DeserializeXmlNode(Detail.DenomIDs);
                    Detail.DenomIDs = doc.OuterXml;
                }

                if (!string.IsNullOrEmpty(Detail.DenomRangeIDs) && Detail.DenomRangeIDs != "[]")
                {
                    var doc = JsonConvert.DeserializeXmlNode(Detail.DenomRangeIDs);
                    Detail.DenomRangeIDs = doc.OuterXml;
                }

                IProcedure proc = new ProcSaveSpecialSlbalDetail(_dal);
                return (IResponseStatus)proc.Call(Detail);
            }

            return res;
        }

        public IResponseStatus SaveSpecialSlabDetailAPI(CircleWithDomination Detail)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin || userML.IsCustomerCareAuthorised(ActionCodes.AddEditOPERATOR))
            {
                Detail.LT = _lr.LoginTypeID;
                Detail.LoginID = _lr.UserID;
                if (!string.IsNullOrEmpty(Detail.DenomIDs) && Detail.DenomIDs != "[]")
                {
                    var doc = JsonConvert.DeserializeXmlNode(Detail.DenomIDs);
                    Detail.DenomIDs = doc.OuterXml;
                }

                if (!string.IsNullOrEmpty(Detail.DenomRangeIDs) && Detail.DenomRangeIDs != "[]")
                {
                    var doc = JsonConvert.DeserializeXmlNode(Detail.DenomRangeIDs);
                    Detail.DenomRangeIDs = doc.OuterXml;
                }

                IProcedure proc = new ProcSaveSpecialAPIIDlDetail(_dal);
                return (IResponseStatus)proc.Call(Detail);
            }

            return res;
        }

        public IResponseStatus UpdateSpecialSlabDomID(CircleWithDomination Detail)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin || userML.IsCustomerCareAuthorised(ActionCodes.AddEditOPERATOR))
            {
                Detail.LT = _lr.LoginTypeID;
                Detail.LoginID = _lr.UserID;

                IProcedure proc = new ProcUpdateSpecialSlbalDominaton(_dal);
                return (IResponseStatus)proc.Call(Detail);
            }

            return res;
        }

        public IResponseStatus UpdateSpecialAPIIDDomID(CircleWithDomination Detail)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin || userML.IsCustomerCareAuthorised(ActionCodes.AddEditOPERATOR))
            {
                Detail.LT = _lr.LoginTypeID;
                Detail.LoginID = _lr.UserID;

                IProcedure proc = new ProcUpdateSpecialAPIIDDominaton(_dal);
                return (IResponseStatus)proc.Call(Detail);
            }

            return res;
        }

        public IResponseStatus UpdateGroupSpecialSlabDomID(CircleWithDomination Detail)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin || userML.IsCustomerCareAuthorised(ActionCodes.AddEditOPERATOR))
            {
                Detail.LT = _lr.LoginTypeID;
                Detail.LoginID = _lr.UserID;
                if (!string.IsNullOrEmpty(Detail.DenomIDs) && Detail.DenomIDs != "[]")
                {
                    var doc = JsonConvert.DeserializeXmlNode(Detail.DenomIDs);
                    Detail.DenomIDs = doc.OuterXml;
                }

                if (!string.IsNullOrEmpty(Detail.DenomRangeIDs) && Detail.DenomRangeIDs != "[]")
                {
                    var doc = JsonConvert.DeserializeXmlNode(Detail.DenomRangeIDs);
                    Detail.DenomRangeIDs = doc.OuterXml;
                }

                IProcedure proc = new ProcUpdateSpecialSlbalGroupDominaton(_dal);
                return (IResponseStatus)proc.Call(Detail);
            }

            return res;
        }

        public IResponseStatus UpdateGroupSpecialAPIIDDomID(CircleWithDomination Detail)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin || userML.IsCustomerCareAuthorised(ActionCodes.AddEditOPERATOR))
            {
                Detail.LT = _lr.LoginTypeID;
                Detail.LoginID = _lr.UserID;
                if (!string.IsNullOrEmpty(Detail.DenomIDs) && Detail.DenomIDs != "[]")
                {
                    var doc = JsonConvert.DeserializeXmlNode(Detail.DenomIDs);
                    Detail.DenomIDs = doc.OuterXml;
                }

                if (!string.IsNullOrEmpty(Detail.DenomRangeIDs) && Detail.DenomRangeIDs != "[]")
                {
                    var doc = JsonConvert.DeserializeXmlNode(Detail.DenomRangeIDs);
                    Detail.DenomRangeIDs = doc.OuterXml;
                }

                IProcedure proc = new ProcUpdateSpecialAPIIDGroupDominaton(_dal);
                return (IResponseStatus)proc.Call(Detail);
            }

            return res;
        }
        public ResponseStatus UpdateAmtVal(int OID, bool STS)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser && _lr.RoleID == Role.Admin || userML.IsCustomerCareAuthorised(ActionCodes.AddEditOPERATOR))
            {
                var _req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    CommonInt = OID,
                    CommonBool = STS
                };
                IProcedure proc = new ProcUpdateOPIsAmountVal(_dal);
                return (ResponseStatus)proc.Call(_req);
            }
            return res;
        }


        public bool IsKYCRequiredForService(string SPKey)
        {
            IProcedure proc = new ProcGetIsKYCRequired(_dal);
            return (bool)proc.Call(SPKey);
        }

        public IEnumerable<AccOpenSetting> AccountOpeningSetting(int OID)
        {
            var commonReq = new CommonReq
            {
                LoginID = _lr.UserID,
                CommonInt = OID,
                LoginTypeID = _lr.LoginTypeID
            };
            IProcedure _proc = new procGetAccountOpeningRedirectionData(_dal);
            var retlist = (List<AccOpenSetting>)_proc.Call(commonReq);

            return retlist;
        }
        public List<AccountOpData> GetAccountOpeningRedirectionDataByOpType(CommonReq req)
        {
            IProcedure proc = new ProcGetAccountOpeningRedirectionDataByOpType(_dal);
            var res = (List<AccountOpData>)proc.Call(req);
            foreach (var itm in res)
            {
                itm.Content = FormatacItalicBold(itm.Content);

            };
            return res;
        }

        public IResponseStatus UpdateAccountOpeningSetting(int OID, string Content, string URI)
        {
            try
            {
                var _resp = new ResponseStatus
                {
                    Statuscode = ErrorCodes.Minus1,
                    Msg = ErrorCodes.AuthError
                };

                if (string.IsNullOrWhiteSpace(URI) || URI.Length > 300)
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " Redirect URL";
                    return _resp;
                }
                if (string.IsNullOrWhiteSpace(Content) || Content.Length > 500)
                {
                    _resp.Msg = ErrorCodes.InvalidParam + " Content";
                    return _resp;
                }

                var _req = new AccOpenSetting
                {
                    LoginID = _lr.LoginTypeID,
                    LT = _lr.LoginTypeID,
                    OID = OID,
                    Content = Content,
                    RedirectURL = URI


                };


                if ((_lr.LoginTypeID == LoginType.ApplicationUser && !_lr.RoleID.In(Role.APIUser, Role.Customer, Role.Retailor_Seller)))
                {
                    Validate validate = Validate.O;

                    //if(!Uri.IsWellFormedUriString(URI, UriKind.Absolute))
                    // { 
                    //     _resp.Msg = "Redirect URL  is Invalid!";
                    //     return _resp;

                    // }

                    IProcedure _proc = new procUpdateAccountOpeningSetting(_dal);
                    _resp = (ResponseStatus)_proc.Call(_req);
                }


                return _resp;
            }

            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "UpdateAccountOpeningSetting",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = 1
                });

                throw;
            }


        }
        public string FormatacItalicBold(string UText)
        {
            string iss = string.Empty;
            if (UText != "")
            {
                int ac = 1;
                int us = 1;
                int flagspaceac = 0;
                int flagspaceus = 0;

                int acrep = 0;
                int usrep = 0;
                for (int i = 0; i < UText.Length; i++)
                {
                    if (UText[i] == ' ')
                    {
                        if (acrep == 2)
                        {
                            iss = iss + (UText[i] + "");
                            iss = iss.Replace("<b> ", "*");
                            ac--;
                            acrep = 0;
                        }
                        if (usrep == 2)
                        {
                            iss = iss + (UText[i] + "");
                            iss = iss.Replace("<i> ", "_");
                            us--;
                            usrep = 0;
                        }
                        flagspaceac = 1;
                        flagspaceus = 1;
                    }
                    if (flagspaceus == 1 && UText[i] != '_')
                    {
                        flagspaceus = 0;
                    }
                    else if (flagspaceus == 1 && UText[i] == '_')
                    {
                        flagspaceus = 0;
                    }
                    if (flagspaceac == 1 && UText[i] != '*')
                    {
                        flagspaceac = 0;
                    }
                    else if (flagspaceac == 1 && UText[i] == '*')
                    {
                        flagspaceac = 0;
                    }
                    if (UText[i] == '*')
                    {
                        if (flagspaceac == 0)
                        {
                            if (ac % 2 == 0)
                            {
                                iss = iss + "</b>";

                                acrep = 1;
                                ac++;
                            }
                            else
                            {
                                iss = iss + "<b>";

                                acrep = 2;
                                ac++;
                            }
                        }
                        else
                        {
                            iss = iss + (UText[i] + "");
                        }

                    }
                    else if (UText[i] == '_')
                    {
                        if (flagspaceus == 0)
                        {
                            if (us % 2 == 0)
                            {
                                iss = iss + "</i>";
                                usrep = 1;
                                us++;
                            }
                            else
                            {
                                iss = iss + "<i>";
                                usrep = 2;
                                us++;
                            }
                        }
                        else
                        {
                            iss = iss + (UText[i] + "");
                        }
                    }
                    else
                    {
                        iss = iss + (UText[i] + "");
                        acrep = 0;
                        usrep = 0;
                    }
                }
            }
            return iss;
        }

        #region RNPPLAN
        public ResponseStatus UpdateRechPlans(int OID)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.AddEditOPERATOR))
            {
                string resp = string.Empty;
                int APIID;
                IProcedure proc = new ProcGetPlansDetialsFBI(_dal);
                var data = (OpRechargePlanResp)proc.Call(new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    UserID = _lr.UserID,
                    CommonInt = OID,
                    CommonInt2 = 0
                });
                res.Statuscode = data.Statuscode;
                res.Msg = data.Msg;

                if (data.Statuscode == ErrorCodes.One)
                {
                    APIID = data.APIID;
                    DataTable dtAllPlans = new DataTable();
                    dtAllPlans.Columns.Add("_OID", typeof(int));
                    dtAllPlans.Columns.Add("_CircleID", typeof(int));
                    dtAllPlans.Columns.Add("_RechPlanTypeID", typeof(int));
                    dtAllPlans.Columns.Add("_RAmount", typeof(string));
                    dtAllPlans.Columns.Add("_Validity", typeof(string));
                    dtAllPlans.Columns.Add("_Details", typeof(string));
                    dtAllPlans.Columns.Add("_EntryDate", typeof(DateTime));
                    dtAllPlans.Columns.Add("_ModifyDate", typeof(DateTime));
                    dtAllPlans.Columns.Add("_APIID", typeof(int));

                    //looping for plan to insert into temp dt
                    foreach (var ccd in data.CircleCodes)
                    {
                        if (!string.IsNullOrEmpty(ccd.CircleCode))
                        {
                            StringBuilder sb = new StringBuilder(data.URL);
                            sb.Replace("{CIRCLE}", ccd.CircleCode);
                            if (data.APICode.Equals(PlanType.Roundpay))
                            {
                                resp = AppWebRequest.O.CallUsingHttpWebRequest_GET(sb.ToString());
                                IProcedureAsync _err = new ProcLogPlansAPIReqResp(_dal);
                                _err.Call(new CommonReq
                                {
                                    CommonStr = "BulkInsertion | " + data.APICode + " | " + ccd.CircleCode,
                                    CommonStr2 = sb.ToString(),
                                    CommonStr3 = resp
                                });
                                if (!resp.Contains("No Data Found"))
                                {
                                    var plans = JsonConvert.DeserializeObject<RoundpaySimplePlanResp>(resp);
                                    if (plans != null)
                                    {
                                        try
                                        {
                                            if (plans.Records != null)
                                            {
                                                if (plans.Records.TwoG != null)
                                                {
                                                    var catId = GetCategoryId(nameof(plans.Records.TwoG));
                                                    foreach (var comboItem in plans.Records.TwoG)
                                                    {
                                                        dtAllPlans.Rows.Add(OID, ccd.CircleID, catId, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);
                                                    }
                                                }
                                                if (plans.Records.ThreeG != null)
                                                {
                                                    var catId = GetCategoryId(nameof(plans.Records.ThreeG));
                                                    foreach (var comboItem in plans.Records.ThreeG)
                                                    {
                                                        dtAllPlans.Rows.Add(OID, ccd.CircleID, catId, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);
                                                    }
                                                }
                                                if (plans.Records.FourG != null)
                                                {
                                                    var catId = GetCategoryId(nameof(plans.Records.FourG));
                                                    foreach (var comboItem in plans.Records.FourG)
                                                    {
                                                        dtAllPlans.Rows.Add(OID, ccd.CircleID, catId, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);
                                                    }
                                                }
                                                if (plans.Records.AllRounder != null)
                                                {
                                                    var catId = GetCategoryId(nameof(plans.Records.AllRounder));
                                                    foreach (var comboItem in plans.Records.AllRounder)
                                                    {
                                                        dtAllPlans.Rows.Add(OID, ccd.CircleID, catId, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);
                                                    }
                                                }
                                                if (plans.Records.DATA != null)
                                                {
                                                    var catId = GetCategoryId(nameof(plans.Records.DATA));
                                                    foreach (var comboItem in plans.Records.DATA)
                                                    {
                                                        dtAllPlans.Rows.Add(OID, ccd.CircleID, catId, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);

                                                    }
                                                }
                                                if (plans.Records.frc != null)
                                                {
                                                    var catId = GetCategoryId(nameof(plans.Records.frc));
                                                    foreach (var comboItem in plans.Records.frc)
                                                    {
                                                        dtAllPlans.Rows.Add(OID, ccd.CircleID, catId, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);

                                                    }
                                                }
                                                if (plans.Records.FRCNonPrime != null)
                                                {
                                                    var catId = GetCategoryId(nameof(plans.Records.FRCNonPrime));
                                                    foreach (var comboItem in plans.Records.FRCNonPrime)
                                                    {
                                                        dtAllPlans.Rows.Add(OID, ccd.CircleID, catId, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);

                                                    }
                                                }
                                                if (plans.Records.HotStar != null)
                                                {
                                                    var catId = GetCategoryId(nameof(plans.Records.HotStar));
                                                    foreach (var comboItem in plans.Records.HotStar)
                                                    {
                                                        dtAllPlans.Rows.Add(OID, ccd.CircleID, catId, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now);

                                                    }
                                                }
                                                if (plans.Records.international != null)
                                                {
                                                    var catId = GetCategoryId(nameof(plans.Records.international));
                                                    foreach (var comboItem in plans.Records.international)
                                                    {
                                                        dtAllPlans.Rows.Add(OID, ccd.CircleID, catId, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);

                                                    }
                                                }
                                                if (plans.Records.isd != null)
                                                {
                                                    var catId = GetCategoryId(nameof(plans.Records.isd));
                                                    foreach (var comboItem in plans.Records.isd)
                                                    {
                                                        dtAllPlans.Rows.Add(OID, ccd.CircleID, catId, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);

                                                    }
                                                }
                                                if (plans.Records.JioPhone != null)
                                                {
                                                    var catId = GetCategoryId(nameof(plans.Records.JioPhone));
                                                    foreach (var comboItem in plans.Records.JioPhone)
                                                    {
                                                        dtAllPlans.Rows.Add(OID, ccd.CircleID, catId, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);

                                                    }
                                                }
                                                if (plans.Records.Local != null)
                                                {
                                                    var catId = GetCategoryId(nameof(plans.Records.Local));
                                                    foreach (var comboItem in plans.Records.Local)
                                                    {
                                                        dtAllPlans.Rows.Add(OID, ccd.CircleID, catId, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);

                                                    }
                                                }
                                                if (plans.Records.NewAllinOne != null)
                                                {
                                                    var catId = GetCategoryId(nameof(plans.Records.NewAllinOne));
                                                    foreach (var comboItem in plans.Records.NewAllinOne)
                                                    {
                                                        dtAllPlans.Rows.Add(OID, ccd.CircleID, catId, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);

                                                    }
                                                }
                                                if (plans.Records.Other != null)
                                                {
                                                    var catId = GetCategoryId(nameof(plans.Records.Other));
                                                    foreach (var comboItem in plans.Records.Other)
                                                    {
                                                        dtAllPlans.Rows.Add(OID, ccd.CircleID, catId, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);

                                                    }
                                                }
                                                if (plans.Records.roaming != null)
                                                {
                                                    var catId = GetCategoryId(nameof(plans.Records.roaming));
                                                    foreach (var comboItem in plans.Records.roaming)
                                                    {
                                                        dtAllPlans.Rows.Add(OID, ccd.CircleID, catId, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);

                                                    }
                                                }
                                                if (plans.Records.smartrecharge != null)
                                                {
                                                    var catId = GetCategoryId(nameof(plans.Records.smartrecharge));
                                                    foreach (var comboItem in plans.Records.smartrecharge)
                                                    {
                                                        dtAllPlans.Rows.Add(OID, ccd.CircleID, catId, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);

                                                    }
                                                }
                                                if (plans.Records.sms != null)
                                                {
                                                    var catId = GetCategoryId(nameof(plans.Records.sms));
                                                    foreach (var comboItem in plans.Records.sms)
                                                    {
                                                        dtAllPlans.Rows.Add(OID, ccd.CircleID, catId, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);
                                                    }
                                                }
                                                if (plans.Records.stv != null)
                                                {
                                                    var catId = GetCategoryId(nameof(plans.Records.stv));
                                                    foreach (var comboItem in plans.Records.stv)
                                                    {
                                                        dtAllPlans.Rows.Add(OID, ccd.CircleID, catId, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);

                                                    }
                                                }
                                                if (plans.Records.talktime != null)
                                                {
                                                    var catId = GetCategoryId(nameof(plans.Records.talktime));
                                                    foreach (var comboItem in plans.Records.talktime)
                                                    {
                                                        dtAllPlans.Rows.Add(OID, ccd.CircleID, catId, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);

                                                    }
                                                }
                                                if (plans.Records.unlimited != null)
                                                {
                                                    var catId = GetCategoryId(nameof(plans.Records.unlimited));
                                                    foreach (var comboItem in plans.Records.unlimited)
                                                    {
                                                        dtAllPlans.Rows.Add(OID, ccd.CircleID, catId, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);

                                                    }
                                                }
                                                if (plans.Records.Validity != null)
                                                {
                                                    var catId = GetCategoryId(nameof(plans.Records.Validity));
                                                    foreach (var comboItem in plans.Records.Validity)
                                                    {
                                                        dtAllPlans.Rows.Add(OID, ccd.CircleID, catId, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);

                                                    }
                                                }
                                                if (plans.Records.VAS != null)
                                                {
                                                    var catId = GetCategoryId(nameof(plans.Records.VAS));
                                                    foreach (var comboItem in plans.Records.VAS)
                                                    {
                                                        dtAllPlans.Rows.Add(OID, ccd.CircleID, catId, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);
                                                    }
                                                }
                                                if (plans.Records.workfromhome != null)
                                                {
                                                    var catId = GetCategoryId(nameof(plans.Records.workfromhome));
                                                    foreach (var comboItem in plans.Records.workfromhome)
                                                    {
                                                        dtAllPlans.Rows.Add(OID, ccd.CircleID, catId, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);

                                                    }
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                                            {
                                                ClassName = GetType().Name,
                                                FuncName = "UpdateRechPlans [" + data.APICode + "]",
                                                Error = ex.Message,
                                                LoginTypeID = 1,
                                                UserId = 1
                                            });
                                        }
                                    }
                                }
                            }
                            else if (data.APICode.Equals(PlanType.PLANSINFO))
                            {
                                resp = AppWebRequest.O.CallUsingHttpWebRequest_GET(sb.ToString());
                                IProcedureAsync _err = new ProcLogPlansAPIReqResp(_dal);
                                _err.Call(new CommonReq
                                {
                                    CommonStr = "BulkInsertion | " + data.APICode + " | " + ccd.CircleCode,
                                    CommonStr2 = sb.ToString(),
                                    CommonStr3 = resp
                                });
                                var plans = JsonConvert.DeserializeObject<PlansInfoRechPlanResp>(resp);

                                try
                                {
                                    foreach (var pifo in plans.data)
                                    {
                                        var catId = GetCategoryId(pifo.category);
                                        dtAllPlans.Rows.Add(OID, ccd.CircleID, catId, pifo.amount, pifo.validity, pifo.benefit, DateTime.Now, DateTime.Now, APIID);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                                    {
                                        ClassName = GetType().Name,
                                        FuncName = "UpdateRechPlans [" + data.APICode + "]",
                                        Error = ex.Message,
                                        LoginTypeID = 1,
                                        UserId = 1
                                    });
                                }
                            }
                            else if (data.APICode.Equals(PlanType.MPLAN))
                            {
                                resp = AppWebRequest.O.CallUsingHttpWebRequest_GET(sb.ToString());
                                IProcedureAsync _err = new ProcLogPlansAPIReqResp(_dal);
                                _err.Call(new CommonReq
                                {
                                    CommonStr = "BulkInsertion | " + data.APICode + " | " + ccd.CircleCode,
                                    CommonStr2 = sb.ToString(),
                                    CommonStr3 = resp
                                });

                                var plans = JsonConvert.DeserializeObject<SubMplanSimplePlanResp>(resp);
                                try
                                {
                                    if (plans.Records != null)
                                    {
                                        if (plans.Records.COMBO != null)
                                        {
                                            var catId = GetCategoryId(nameof(plans.Records.COMBO));
                                            foreach (var comboItem in plans.Records.COMBO)
                                            {
                                                dtAllPlans.Rows.Add(OID, ccd.CircleID, catId, comboItem.RS, comboItem.Validity, comboItem.Desc, DateTime.Now, DateTime.Now, APIID);

                                            }
                                        }
                                        if (plans.Records.FullTT != null)
                                        {
                                            var catId = GetCategoryId(nameof(plans.Records.FullTT));
                                            foreach (var comboItem in plans.Records.FullTT)
                                            {
                                                dtAllPlans.Rows.Add(OID, ccd.CircleID, catId, comboItem.RS, comboItem.Validity, comboItem.Desc, DateTime.Now, DateTime.Now, APIID);

                                            }
                                        }
                                        if (plans.Records.TOPUP != null)
                                        {
                                            var catId = GetCategoryId(nameof(plans.Records.TOPUP));
                                            foreach (var comboItem in plans.Records.TOPUP)
                                            {
                                                dtAllPlans.Rows.Add(OID, ccd.CircleID, catId, comboItem.RS, comboItem.Validity, comboItem.Desc, DateTime.Now, DateTime.Now, APIID);

                                            }
                                        }
                                        if (plans.Records.ThreeGFourG != null)
                                        {
                                            var catId = GetCategoryId(nameof(plans.Records.ThreeGFourG));
                                            foreach (var comboItem in plans.Records.ThreeGFourG)
                                            {
                                                dtAllPlans.Rows.Add(OID, ccd.CircleID, catId, comboItem.RS, comboItem.Validity, comboItem.Desc, DateTime.Now, DateTime.Now, APIID);

                                            }
                                        }
                                        if (plans.Records.TwoG != null)
                                        {
                                            var catId = GetCategoryId(nameof(plans.Records.TwoG));
                                            foreach (var comboItem in plans.Records.TwoG)
                                            {
                                                dtAllPlans.Rows.Add(OID, ccd.CircleID, catId, comboItem.RS, comboItem.Validity, comboItem.Desc, DateTime.Now, DateTime.Now, APIID);

                                            }
                                        }
                                        if (plans.Records.RateCutter != null)
                                        {
                                            var catId = GetCategoryId(nameof(plans.Records.RateCutter));
                                            foreach (var comboItem in plans.Records.RateCutter)
                                            {
                                                dtAllPlans.Rows.Add(OID, ccd.CircleID, catId, comboItem.RS, comboItem.Validity, comboItem.Desc, DateTime.Now, DateTime.Now, APIID);

                                            }
                                        }
                                        if (plans.Records.Roaming != null)
                                        {
                                            var catId = GetCategoryId(nameof(plans.Records.Roaming));
                                            foreach (var comboItem in plans.Records.Roaming)
                                            {
                                                dtAllPlans.Rows.Add(OID, ccd.CircleID, catId, comboItem.RS, comboItem.Validity, comboItem.Desc, DateTime.Now, DateTime.Now, APIID);

                                            }
                                        }
                                        if (plans.Records.SMS != null)
                                        {
                                            var catId = GetCategoryId(nameof(plans.Records.SMS));
                                            foreach (var comboItem in plans.Records.SMS)
                                            {
                                                dtAllPlans.Rows.Add(OID, ccd.CircleID, catId, comboItem.RS, comboItem.Validity, comboItem.Desc, DateTime.Now, DateTime.Now, APIID);

                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                                    {
                                        ClassName = GetType().Name,
                                        FuncName = "UpdateRechPlans [" + data.APICode + "]",
                                        Error = ex.Message,
                                        LoginTypeID = 1,
                                        UserId = 1
                                    });
                                }
                            }
                            else if (data.APICode.Equals(PlanType.PLANAPI))
                            {
                                resp = AppWebRequest.O.CallUsingHttpWebRequest_GET(sb.ToString());
                                IProcedureAsync _err = new ProcLogPlansAPIReqResp(_dal);
                                _err.Call(new CommonReq
                                {
                                    CommonStr = "BulkInsertion | " + data.APICode + " | " + ccd.CircleCode,
                                    CommonStr2 = sb.ToString(),
                                    CommonStr3 = resp
                                });
                                var plans = JsonConvert.DeserializeObject<PlanApiViewPlan>(resp);
                                try
                                {
                                    if (plans.RDATA != null)
                                    {
                                        if (plans.RDATA.COMBO != null)
                                        {
                                            var catId = GetCategoryId(nameof(plans.RDATA.COMBO));
                                            foreach (var comboItem in plans.RDATA.COMBO)
                                            {
                                                dtAllPlans.Rows.Add(OID, ccd.CircleID, catId, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);

                                            }
                                        }
                                        if (plans.RDATA.DATA != null)
                                        {
                                            var catId = GetCategoryId(nameof(plans.RDATA.DATA));
                                            foreach (var comboItem in plans.RDATA.DATA)
                                            {
                                                dtAllPlans.Rows.Add(OID, ccd.CircleID, catId, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);

                                            }
                                        }
                                        if (plans.RDATA.FRC != null)
                                        {
                                            var catId = GetCategoryId(nameof(plans.RDATA.FRC));
                                            foreach (var comboItem in plans.RDATA.FRC)
                                            {
                                                dtAllPlans.Rows.Add(OID, ccd.CircleID, catId, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);

                                            }
                                        }
                                        if (plans.RDATA.FULLTT != null)
                                        {
                                            var catId = GetCategoryId(nameof(plans.RDATA.FULLTT));
                                            foreach (var comboItem in plans.RDATA.FULLTT)
                                            {
                                                dtAllPlans.Rows.Add(OID, ccd.CircleID, catId, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);

                                            }
                                        }
                                        if (plans.RDATA.TOPUP != null)
                                        {
                                            var catId = GetCategoryId(nameof(plans.RDATA.TOPUP));
                                            foreach (var comboItem in plans.RDATA.TOPUP)
                                            {
                                                dtAllPlans.Rows.Add(OID, ccd.CircleID, catId, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);
                                            }
                                        }
                                        if (plans.RDATA.ThreeG4G != null)
                                        {
                                            var catId = GetCategoryId(nameof(plans.RDATA.ThreeG4G));
                                            foreach (var comboItem in plans.RDATA.ThreeG4G)
                                            {
                                                dtAllPlans.Rows.Add(OID, ccd.CircleID, catId, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);
                                            }
                                        }
                                        if (plans.RDATA.RATECUTTER != null)
                                        {
                                            var catId = GetCategoryId(nameof(plans.RDATA.RATECUTTER));
                                            foreach (var comboItem in plans.RDATA.RATECUTTER)
                                            {
                                                dtAllPlans.Rows.Add(OID, ccd.CircleID, catId, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);
                                            }
                                        }
                                        if (plans.RDATA.TwoG != null)
                                        {
                                            var catId = GetCategoryId(nameof(plans.RDATA.TwoG));
                                            foreach (var comboItem in plans.RDATA.TwoG)
                                            {
                                                dtAllPlans.Rows.Add(OID, ccd.CircleID, catId, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);
                                            }
                                        }
                                        if (plans.RDATA.SMS != null)
                                        {
                                            var catId = GetCategoryId(nameof(plans.RDATA.SMS));
                                            foreach (var comboItem in plans.RDATA.SMS)
                                            {
                                                dtAllPlans.Rows.Add(OID, ccd.CircleID, catId, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);
                                            }
                                        }
                                        if (plans.RDATA.Romaing != null)
                                        {
                                            var catId = GetCategoryId(nameof(plans.RDATA.Romaing));
                                            foreach (var comboItem in plans.RDATA.Romaing)
                                            {
                                                dtAllPlans.Rows.Add(OID, ccd.CircleID, catId, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);
                                            }
                                        }
                                        if (plans.RDATA.STV != null)
                                        {
                                            var catId = GetCategoryId(nameof(plans.RDATA.STV));
                                            foreach (var comboItem in plans.RDATA.STV)
                                            {
                                                dtAllPlans.Rows.Add(OID, ccd.CircleID, catId, comboItem.rs, comboItem.validity, comboItem.desc, DateTime.Now, DateTime.Now, APIID);
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                                    {
                                        ClassName = GetType().Name,
                                        FuncName = "UpdateRechPlans [" + data.APICode + "]",
                                        Error = ex.Message,
                                        LoginTypeID = 1,
                                        UserId = 1
                                    });
                                }
                            }
                        }
                    }
                    if (dtAllPlans.Rows.Count > 0)
                    {
                        IProcedure _proc = new ProcBulkPlansInsertion(_dal);
                        res = (ResponseStatus)_proc.Call(new BulkInsertionObj
                        {
                            LT = _lr.LoginTypeID,
                            LoginID = _lr.UserID,
                            OID = OID,
                            tp_Rechargeplans = dtAllPlans
                        });
                    }
                }
            }
            return res;
        }
        private int GetCategoryId(string category)
        {
            int catId = 0;
            IProcedure proc = new ProcGetPlanTypeID(_dal);
            var resp = (ResponseStatus)proc.Call(new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                LoginID = _lr.UserID,
                CommonStr = category
            });
            if (resp.Statuscode == ErrorCodes.One)
                catId = resp.CommonInt;
            return catId;
        }
        public string GetCategory(string category)
        {
            if (category.In("Data Pack", "data", "ThreeGFourG", "TwoG", "ThreeG4G", "FourG"))
            {
                category = "Data";
                return category;
            }
            else if (category == "international roaming")
            {
                category = "International";
                return category;
            }
            else if (category == "validity extension")
            {
                category = "Validity";
                return category;
            }
            else if (category == "workfromhome")
            {
                category = "Work From Home";
                return category;
            }
            else if (category == "FRCNonPrime")
            {
                category = "FRC/non-Prime";
                return category;
            }
            else if (category == "AllRounder")
            {
                category = "All Rounder";
                return category;
            }
            else if (category == "TOPUP")
            {
                category = "Top up";
                return category;
            }
            else if (category == "NewAllinOne")
            {
                category = "NEW ALL-IN-ONE";
                return category;
            }
            else if (category.In("Smart", "smartrecharge"))
            {
                category = "Smart Recharge";
                return category;
            }
            else
            {
                return category;
            }
        }
        public ResponseStatus UpdateDTHPlans(int OID)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if (_lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.AddEditOPERATOR))
            {
                string resp = string.Empty;
                string resp2 = string.Empty;

                IProcedure proc = new ProcGetDTHPlansDetialsFBI(_dal);
                var data = (OpRechargePlanResp)proc.Call(new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    UserID = _lr.UserID,
                    CommonInt = OID,
                    CommonInt2 = 0
                });
                if (data.Statuscode == ErrorCodes.One)
                {
                    DataTable dtAllPlans = new DataTable();
                    dtAllPlans.Columns.Add("_OID", typeof(int));
                    dtAllPlans.Columns.Add("_CircleID", typeof(int));
                    dtAllPlans.Columns.Add("_APIID", typeof(int));
                    dtAllPlans.Columns.Add("_PackageName", typeof(string));
                    dtAllPlans.Columns.Add("_PackagePrice", typeof(string));
                    dtAllPlans.Columns.Add("_PackagePrice_3", typeof(string));
                    dtAllPlans.Columns.Add("_PackagePrice_6", typeof(string));
                    dtAllPlans.Columns.Add("_PackagePrice_12", typeof(string));
                    dtAllPlans.Columns.Add("_PackageDescription", typeof(string));
                    dtAllPlans.Columns.Add("_PackageType", typeof(string));
                    dtAllPlans.Columns.Add("_PackageLanguage", typeof(string));
                    dtAllPlans.Columns.Add("_PackageId", typeof(int));

                    foreach (var ccd in data.CircleCodes)
                    {
                        if (!string.IsNullOrEmpty(ccd.CircleCode))
                        {
                            StringBuilder sb = new StringBuilder(data.URL);
                            sb.Replace("{CIRCLE}", ccd.CircleCode);
                            if (data.APICode.Equals(PlanType.PLANSINFO))
                            {
                                resp = AppWebRequest.O.CallUsingHttpWebRequest_GET(sb.ToString());

                                IProcedureAsync _err = new ProcLogPlansAPIReqResp(_dal);
                                _err.Call(new CommonReq
                                {
                                    CommonStr = "BulkInsertion | " + data.APICode + " | " + ccd.CircleCode,
                                    CommonStr2 = sb.ToString(),
                                    CommonStr3 = resp
                                });
                                var plans = JsonConvert.DeserializeObject<PlansInfoDTHP>(resp);
                                if (plans != null)
                                {
                                    foreach (var item in plans.plans)
                                    {
                                        dtAllPlans.Rows.Add(OID, ccd.CircleID, data.APIID, item.package_name, item.package_price, item.package_price_3, item.package_price_6, item.package_price_12, item.package_description, item.package_type, item.package_language, item.package_id);
                                    };
                                }
                                var _pID = dtAllPlans.DefaultView.ToTable(true, "_PackageId");
                                if (_pID != null)
                                {
                                    foreach (DataRow dr in _pID.Rows)
                                    {
                                        DataTable dtChannelList = new DataTable();
                                        dtChannelList.Columns.Add("_ChName", typeof(string));
                                        dtChannelList.Columns.Add("_Genre", typeof(string));
                                        dtChannelList.Columns.Add("_LogoURL", typeof(string));
                                        dtChannelList.Columns.Add("_PackageId", typeof(int));
                                        StringBuilder sb2 = new StringBuilder(data.URL2);
                                        sb2.Replace("{PID}", dr["_PackageId"].ToString());
                                        resp2 = AppWebRequest.O.CallUsingHttpWebRequest_GET(sb2.ToString());
                                        _err.Call(new CommonReq
                                        {
                                            CommonStr = "BulkInsertion Channel | " + dr["_PackageId"].ToString(),
                                            CommonStr2 = sb2.ToString(),
                                            CommonStr3 = resp2
                                        });
                                        var chList = JsonConvert.DeserializeObject<PIChannelList>(resp2);

                                        foreach (var item in chList.channels)
                                        {
                                            dtChannelList.Rows.Add(item.name, item.genre, item.logo, Convert.ToInt32(dr["_PackageId"]));
                                        }
                                        if (dtChannelList.Rows.Count > 0)
                                        {
                                            IProcedure __ = new ProcBulkDTHChannelList(_dal);
                                            res = (ResponseStatus)__.Call(new BulkInsertionObj
                                            {
                                                LT = _lr.LoginTypeID,
                                                LoginID = _lr.UserID,
                                                PackageID = Convert.ToInt32(dr["_PackageId"]),
                                                tp_Rechargeplans = dtChannelList
                                            });
                                        }
                                    }
                                }
                            }
                            if (data.APICode.Equals(PlanType.MPLAN))
                            {
                                resp = AppWebRequest.O.CallUsingHttpWebRequest_GET(sb.ToString());
                                IProcedureAsync _err = new ProcLogPlansAPIReqResp(_dal);
                                _err.Call(new CommonReq
                                {
                                    CommonStr = "BulkInsertion | " + data.APICode + " | " + ccd.CircleCode,
                                    CommonStr2 = sb.ToString(),
                                    CommonStr3 = resp
                                });

                                var plans = JsonConvert.DeserializeObject<SubMplanDTHSimplePlanResp>(resp);

                                if (plans.Records != null)
                                {
                                    if (plans.Records.Plan != null)
                                    {
                                        foreach (var item in plans.Records.Plan)
                                        {
                                            string OneMonth = string.Empty, ThreeMonth = string.Empty, SixMonth = string.Empty, OneYear = string.Empty;
                                            if ((item.RS.OneMonth ?? "").Length > 1)
                                            {
                                                OneMonth = item.RS.OneMonth;
                                            }
                                            if ((item.RS.ThreeMonth ?? "").Length > 1)
                                            {
                                                ThreeMonth = item.RS.ThreeMonth;
                                            }
                                            if ((item.RS.SixMonth ?? "").Length > 1)
                                            {
                                                SixMonth = item.RS.SixMonth;
                                            }
                                            if ((item.RS.OneYear ?? "").Length > 1)
                                            {
                                                OneYear = item.RS.OneYear;
                                            }
                                            dtAllPlans.Rows.Add(OID, ccd.CircleID, data.APIID, item.PlanName, OneMonth, ThreeMonth, SixMonth, OneYear, item.Desc, "Plan", "Lang", 0);
                                        }

                                    }
                                    if (plans.Records.AddOnPack != null)
                                    {
                                        foreach (var item in plans.Records.AddOnPack)
                                        {
                                            string OneMonth = string.Empty, ThreeMonth = string.Empty, SixMonth = string.Empty, OneYear = string.Empty;
                                            if ((item.RS.OneMonth ?? "").Length > 1)
                                            {
                                                OneMonth = item.RS.OneMonth;
                                            }
                                            if ((item.RS.ThreeMonth ?? "").Length > 1)
                                            {
                                                ThreeMonth = item.RS.ThreeMonth;
                                            }
                                            if ((item.RS.SixMonth ?? "").Length > 1)
                                            {
                                                SixMonth = item.RS.SixMonth;
                                            }
                                            if ((item.RS.OneYear ?? "").Length > 1)
                                            {
                                                OneYear = item.RS.OneYear;
                                            }
                                            dtAllPlans.Rows.Add(OID, ccd.CircleID, data.APIID, item.PlanName, OneMonth, ThreeMonth, SixMonth, OneYear, item.Desc, "Add On Pack", "Lang", 0);
                                        }
                                    }
                                }
                            }
                            if (data.APICode.Equals(PlanType.PLANAPI))
                            {
                                resp = AppWebRequest.O.CallUsingHttpWebRequest_GET(sb.ToString());
                                IProcedureAsync _err = new ProcLogPlansAPIReqResp(_dal);
                                _err.Call(new CommonReq
                                {
                                    CommonStr = "BulkInsertion | " + data.APICode + " | " + ccd.CircleCode,
                                    CommonStr2 = sb.ToString(),
                                    CommonStr3 = resp
                                });

                                var plans = JsonConvert.DeserializeObject<DTHPlan>(resp);

                                if (plans.RDATA != null)
                                {
                                    if (plans.RDATA.Plan != null)
                                    {
                                        foreach (var item in plans.RDATA.Plan)
                                        {
                                            string OneMonth = string.Empty, ThreeMonth = string.Empty, SixMonth = string.Empty, OneYear = string.Empty;
                                            if ((item.RS.OneMonth ?? "").Length > 1)
                                            {
                                                OneMonth = item.RS.OneMonth;
                                            }
                                            if ((item.RS.ThreeMonth ?? "").Length > 1)
                                            {
                                                ThreeMonth = item.RS.ThreeMonth;
                                            }
                                            if ((item.RS.SixMonth ?? "").Length > 1)
                                            {
                                                SixMonth = item.RS.SixMonth;
                                            }
                                            if ((item.RS.OneYear ?? "").Length > 1)
                                            {
                                                OneYear = item.RS.OneYear;
                                            }
                                            dtAllPlans.Rows.Add(OID, ccd.CircleID, data.APIID, item.PlanName, OneMonth, ThreeMonth, SixMonth, OneYear, item.Desc, "Plan", "Lang", 0);
                                        }
                                    }

                                    if (plans.RDATA.AddOnPack != null)
                                    {
                                        foreach (var item in plans.RDATA.AddOnPack)
                                        {
                                            string OneMonth = string.Empty, ThreeMonth = string.Empty, SixMonth = string.Empty, OneYear = string.Empty;
                                            if ((item.RS.OneMonth ?? "").Length > 1)
                                            {
                                                OneMonth = item.RS.OneMonth;
                                            }
                                            if ((item.RS.ThreeMonth ?? "").Length > 1)
                                            {
                                                ThreeMonth = item.RS.ThreeMonth;
                                            }
                                            if ((item.RS.SixMonth ?? "").Length > 1)
                                            {
                                                SixMonth = item.RS.SixMonth;
                                            }
                                            if ((item.RS.OneYear ?? "").Length > 1)
                                            {
                                                OneYear = item.RS.OneYear;
                                            }
                                            dtAllPlans.Rows.Add(OID, ccd.CircleID, data.APIID, item.PlanName, OneMonth, ThreeMonth, SixMonth, OneYear, item.Desc, "Add On Pack", "Lang", 0);
                                        }
                                    }

                                }
                            }

                        }
                    }
                    if (dtAllPlans.Rows.Count > 0)
                    {
                        IProcedure _proc = new ProcBulkDTHPlansInsertion(_dal);
                        res = (ResponseStatus)_proc.Call(new BulkInsertionObj
                        {
                            LT = _lr.LoginTypeID,
                            LoginID = _lr.UserID,
                            OID = OID,
                            tp_Rechargeplans = dtAllPlans
                        });
                    }
                }
                else
                {
                    res.Statuscode = data.Statuscode;
                    res.Msg = data.Msg;
                }
            }
            return res;
        }

        public ResponseStatus MapPlansOperator(int toOid, int toMapOid)
        {
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            IProcedure proc = new ProcMapPlansOperator(_dal);
            res = (ResponseStatus)proc.Call(new CommonReq
            {
                LoginTypeID = _lr.LoginTypeID,
                UserID = _lr.UserID,
                CommonInt = toOid,
                CommonInt2 = toMapOid,
                CommonStr = _rinfo.GetLocalIP(),
                CommonStr2 = _rinfo.GetBrowser()
            });
            return res;
        }
        #endregion

        public IEnumerable<EXACTNESSMaster> GetExactness()
        {
            IProcedure proc = new ProcGetExactness(_dal);
            return (List<EXACTNESSMaster>)proc.Call();
        }

    }
}
