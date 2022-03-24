using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.BBPS;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetRPBiller : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetRPBiller(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@BillerID",req.CommonStr??string.Empty),
                new SqlParameter("@OpTypeID",req.CommonInt)
            };
            var res = new RPBillerModel
            {
                billerList = new List<RPBiller>(),
                billerParamList = new List<RPBillerParam>(),
                billerPaymentChanel = new List<RPBillerPaymentChanel>(),
                billerOperatorDictionary = new List<RPBillerOperatorDictionary>()
            };
            try
            {
                DataSet ds = _dal.GetByProcedureAdapterDS(GetName(), param);

                if (ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    if (dt.Rows.Count > 0)
                    {//Biller
                        foreach (DataRow item in dt.Rows)
                        {
                            res.billerList.Add(new RPBiller
                            {
                                OID = item["_ID"] is DBNull ? 0 : Convert.ToInt32(item["_ID"]),
                                Name = item["_Name"] is DBNull ? string.Empty : item["_Name"].ToString(),
                                OPID = item["_OPID"] is DBNull ? string.Empty : item["_OPID"].ToString(),
                                OpTypeID = item["_OpTypeID"] is DBNull ? 0 : Convert.ToInt32(item["_OpTypeID"]),
                                OpType = item["_OpType"] is DBNull ? string.Empty : Convert.ToString(item["_OpType"]),
                                MinLength = item["_Length"] is DBNull ? 0 : Convert.ToInt32(item["_Length"]),
                                MaxLength = item["_LengthMax"] is DBNull ? 0 : Convert.ToInt32(item["_LengthMax"]),
                                MinAmount = item["_Min"] is DBNull ? 0 : Convert.ToInt32(item["_Min"]),
                                MaxAmount = item["_Max"] is DBNull ? 0 : Convert.ToInt32(item["_Max"]),
                                IsBBPS = item["_IsBBPS"] is DBNull ? false : Convert.ToBoolean(item["_IsBBPS"]),
                                IsBilling = item["_IsBilling"] is DBNull ? false : Convert.ToBoolean(item["_IsBilling"]),
                                AccountName = item["_AccountName"] is DBNull ? string.Empty : Convert.ToString(item["_AccountName"]),
                                AccountRemark = item["_AccountRemak"] is DBNull ? string.Empty : Convert.ToString(item["_AccountRemak"]),
                                IsAccountNumeric = item["_IsAccountNumeric"] is DBNull ? false : Convert.ToBoolean(item["_IsAccountNumeric"]),
                                RPBillerID = item["_RPBillerID"] is DBNull ? string.Empty : Convert.ToString(item["_RPBillerID"]),
                                BillerAmountOptions = item["_BillerAmountOptions"] is DBNull ? string.Empty : Convert.ToString(item["_BillerAmountOptions"]),
                                BillerAdhoc = item["_BillerAdhoc"] is DBNull ? false : Convert.ToBoolean(item["_BillerAdhoc"]),
                                ExactNess = item["_ExactNess"] is DBNull ? 0 : Convert.ToInt16(item["_ExactNess"]),
                                BillerCoverage = item["_BillerCoverage"] is DBNull ? string.Empty : Convert.ToString(item["_BillerCoverage"]),
                                BillerName = item["_BillerName"] is DBNull ? string.Empty : Convert.ToString(item["_BillerName"]),
                                AccountNoKey = item["_AccountNoKey"] is DBNull ? string.Empty : Convert.ToString(item["_AccountNoKey"]),
                                IsBillValidation = item["_IsBillValidation"] is DBNull ? false : Convert.ToBoolean(item["_IsBillValidation"]),
                                BillerPaymentModes = item["BillerPaymentModes"] is DBNull ? string.Empty : Convert.ToString(item["BillerPaymentModes"]),
                                RegExAccount = item["_RegExAccount"] is DBNull ? string.Empty : Convert.ToString(item["_RegExAccount"]),
                                IsAmountOptions = item["_IsAmountOptions"] is DBNull ? false : Convert.ToBoolean(item["_IsAmountOptions"]),
                                EarlyPaymentAmountKey = item["_EarlyPaymentAmountKey"] is DBNull ? string.Empty : Convert.ToString(item["_EarlyPaymentAmountKey"]),
                                LatePaymentAmountKey = item["_LatePaymentAmountKey"] is DBNull ? string.Empty : Convert.ToString(item["_LatePaymentAmountKey"]),
                                EarlyPaymentDateKey = item["_EarlyPaymentDateKey"] is DBNull ? string.Empty : Convert.ToString(item["_EarlyPaymentDateKey"]),
                                BillMonthKey = item["_BillMonthKey"] is DBNull ? string.Empty : Convert.ToString(item["_BillMonthKey"]),
                                IsAmountValidation = item["_IsAmountValidation"] is DBNull ? false : Convert.ToBoolean(item["_IsAmountValidation"])
                            });
                        }
                    }
                    DataTable dt2 = ds.Tables.Count > 1 ? ds.Tables[1] : null;
                    DataTable dt3 = ds.Tables.Count > 2 ? ds.Tables[2] : null;
                    DataTable dt4 = ds.Tables.Count > 2 ? ds.Tables[3] : null;
                    if (dt2 != null)
                    {
                        if (dt2.Rows.Count > 0)
                        {
                            foreach (DataRow item2 in dt2.Rows)
                            {
                                res.billerParamList.Add(new RPBillerParam
                                {
                                    ID = item2["_ID"] is DBNull ? 0 : Convert.ToInt32(item2["_ID"]),
                                    OID = item2["_OID"] is DBNull ? 0 : Convert.ToInt32(item2["_OID"]),
                                    ParamName = item2["_ParamName"] is DBNull ? string.Empty : Convert.ToString(item2["_ParamName"]),
                                    DataType = item2["_DataType"] is DBNull ? string.Empty : Convert.ToString(item2["_DataType"]),                                   
                                    MinLength = item2["_MinLength"] is DBNull ? 0 : Convert.ToInt32(item2["_MinLength"]),
                                    MaxLength = item2["_MaxLength"] is DBNull ? 0 : Convert.ToInt32(item2["_MaxLength"]),
                                    RegEx = item2["_RegEx"] is DBNull ? string.Empty : Convert.ToString(item2["_RegEx"]),
                                    IsAccountNo = item2["_IsAccountNo"] is DBNull ? false : Convert.ToBoolean(item2["_IsAccountNo"]),
                                    IsOptional = item2["_IsOptional"] is DBNull ? false : Convert.ToBoolean(item2["_IsOptional"]),
                                    IsActive = item2["_IsActive"] is DBNull ? false : Convert.ToBoolean(item2["_IsActive"]),
                                    IsCustomerNo = item2["_IsCustomerNo"] is DBNull ? false : Convert.ToBoolean(item2["_IsCustomerNo"]),
                                    IsDropDown = item2["_IsDropDown"] is DBNull ? false : Convert.ToBoolean(item2["_IsDropDown"]),
                                    Remark = item2["_Remark"] is DBNull ? string.Empty : Convert.ToString(item2["_Remark"]),
                                });
                            }
                        }
                    }
                    if (dt3 != null)
                    {
                        if (dt3.Rows.Count > 0)
                        {
                            foreach (DataRow item3 in dt3.Rows)
                            {
                                res.billerPaymentChanel.Add(new RPBillerPaymentChanel
                                {
                                    ID = item3["_ID"] is DBNull ? 0 : Convert.ToInt32(item3["_ID"]),
                                    OID = item3["_OID"] is DBNull ? 0 : Convert.ToInt32(item3["_OID"]),
                                    PaymentChanel = item3["_PaymentChanel"] is DBNull ? string.Empty : Convert.ToString(item3["_PaymentChanel"]),
                                    MinAmount = item3["_MinAmount"] is DBNull ? 0 : Convert.ToInt32(item3["_MinAmount"]),
                                    MaxAmount = item3["_MaxAmount"] is DBNull ? 0 : Convert.ToInt32(item3["_MaxAmount"])
                                });
                            }                           
                        }
                    }
                    if (dt4 != null)
                    {
                        if (dt4.Rows.Count > 0)
                        {
                            foreach (DataRow item4 in dt4.Rows)
                            {
                                res.billerOperatorDictionary.Add(new RPBillerOperatorDictionary
                                {
                                    ParamID = item4["ParamID"] is DBNull ? 0 : Convert.ToInt32(item4["ParamID"]),
                                    Ind = item4["_Ind"] is DBNull ? 0 : Convert.ToInt32(item4["_Ind"]),
                                    OID = item4["_OID"] is DBNull ? 0 : Convert.ToInt32(item4["_OID"]),
                                    DropDownValue = item4["_Value"] is DBNull ? string.Empty : Convert.ToString(item4["_Value"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = req.LoginID
                });
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_GetRPBiller";
    }
}
