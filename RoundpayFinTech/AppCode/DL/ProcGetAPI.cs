using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetAPI : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetAPI(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@APIID",req.CommonInt),
                new SqlParameter("@LoginID",req.LoginID)
            };
            var resp = new List<APIDetail>();
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    if (req.IsListType)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            var aPIDetail = new APIDetail
                            {
                                ID = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                                APIType = row["_APIType"] is DBNull ? 0 : Convert.ToInt16(row["_APIType"]),
                                _APIType = APITypes.GetAPIType(row["_APIType"] is DBNull ? 0 : Convert.ToInt16(row["_APIType"])),
                                Name = row["_Name"] is DBNull ? string.Empty : row["_Name"].ToString(),
                                URL = row["_URL"] is DBNull ? string.Empty : row["_URL"].ToString(),
                                APICode = row["_APICode"] is DBNull ? string.Empty : row["_APICode"].ToString(),
                                StatusCheckURL = row["_StatusCheckURL"] is DBNull ? string.Empty : row["_StatusCheckURL"].ToString(),
                                BalanceURL = row["_BalanceURL"] is DBNull ? string.Empty : row["_BalanceURL"].ToString(),
                                DisputeURL = row["_DisputeURL"] is DBNull ? string.Empty : row["_DisputeURL"].ToString(),
                                FetchBillURL = row["_FetchBillURL"] is DBNull ? string.Empty : row["_FetchBillURL"].ToString(),
                                RequestMethod = row["_RequestMethod"] is DBNull ? string.Empty : row["_RequestMethod"].ToString(),
                                StatusName = row["_StatusName"] is DBNull ? string.Empty : row["_StatusName"].ToString(),
                                FailCode = row["_FailCode"] is DBNull ? string.Empty : row["_FailCode"].ToString(),
                                SuccessCode = row["_SuccessCode"] is DBNull ? string.Empty : row["_SuccessCode"].ToString(),
                                ActiveSts = row["_ActiveSts"] is DBNull ? false : Convert.ToBoolean(row["_ActiveSts"]),
                                IsOutletRequired = row["_IsOutletRequired"] is DBNull ? false : Convert.ToBoolean(row["_IsOutletRequired"]),
                                FixedOutletID = row["_FixedOutletID"] is DBNull ? string.Empty : row["_FixedOutletID"].ToString(),
                                GroupID = row["_GroupID"] is DBNull ? 0 : Convert.ToInt16(row["_GroupID"]),
                                VendorID = row["_VendorID"] is DBNull ? string.Empty : row["_VendorID"].ToString(),
                                ResponseTypeID = row["_ResponseTypeID"] is DBNull ? 0 : Convert.ToInt16(row["_ResponseTypeID"]),
                                Remark = row["_Remark"] is DBNull ? string.Empty : row["_Remark"].ToString(),
                                EntryBy = row["_EntryBy"] is DBNull ? 0 : Convert.ToInt32(row["_EntryBy"]),
                                EntryDate = row["_EntryDate"] is DBNull ? string.Empty : row["_EntryDate"].ToString(),
                                ModifyBy = row["_ModifyBy"] is DBNull ? 0 : Convert.ToInt32(row["_ModifyBy"]),
                                ModifyDate = row["_ModifyDate"] is DBNull ? string.Empty : row["_ModifyDate"].ToString(),
                                IsOpDownAllow = row["_IsOpDownAllow"] is DBNull ? false : Convert.ToBoolean(row["_IsOpDownAllow"]),
                                LiveID = row["_LiveID"] is DBNull ? string.Empty : row["_LiveID"].ToString(),
                                SurchargeType = row["_SurchargeType"] is DBNull ? 1 : Convert.ToInt16(row["_SurchargeType"]),
                                MsgKey = row["_MsgKey"] is DBNull ? string.Empty : row["_MsgKey"].ToString(),
                                BillNoKey = row["_BillNoKey"] is DBNull ? string.Empty : row["_BillNoKey"].ToString(),
                                BillDateKey = row["_BillDateKey"] is DBNull ? string.Empty : row["_BillDateKey"].ToString(),
                                BillAmountKey = row["_BillAmountKey"] is DBNull ? string.Empty : row["_BillAmountKey"].ToString(),
                                CustomerNameKey = row["_CustomerNameKey"] is DBNull ? string.Empty : row["_CustomerNameKey"].ToString(),
                                DueDateKey = row["_DueDateKey"] is DBNull ? string.Empty : row["_DueDateKey"].ToString(),
                                GroupName = row["_GroupName"] is DBNull ? string.Empty : row["_GroupName"].ToString(),
                                GroupCode = row["_GroupCode"] is DBNull ? string.Empty : row["_GroupCode"].ToString(),
                                BillStatusKey = row["_BillStatusKey"] is DBNull ? string.Empty : row["_BillStatusKey"].ToString(),
                                BillStatusValue = row["_BillStatusValue"] is DBNull ? string.Empty : row["_BillStatusValue"].ToString(),
                                IsOutletManual = row["_IsOutletManual"] is DBNull ? false : Convert.ToBoolean(row["_IsOutletManual"]),
                                ContentType = row["_ContentType"] is DBNull ? 0 : Convert.ToInt16(row["_ContentType"]),
                                BalanceKey = row["_BalanceKey"] is DBNull ? string.Empty : row["_BalanceKey"].ToString(),
                                BillReqMethod = row["_BillReqMethod"] is DBNull ? string.Empty : row["_BillReqMethod"].ToString(),
                                BillResTypeID = row["_BillResTypeID"] is DBNull ? 0 : Convert.ToInt16(row["_BillResTypeID"]),
                                DFormatID = row["_DFormatID"] is DBNull ? 0 : Convert.ToInt16(row["_DFormatID"]),
                                InSwitch = row["_InSwitch"] is DBNull ? false : Convert.ToBoolean(row["_InSwitch"]),
                                ErrorCodeKey = row["_ErrorCodeKey"] is DBNull ? string.Empty : row["_ErrorCodeKey"].ToString(),
                                RefferenceKey = row["_RefKey"] is DBNull ? string.Empty : row["_RefKey"].ToString(),
                                MaxLimitPerTransaction = row["_MaxLimitPerTransaction"] is DBNull ? 0 : Convert.ToInt32(row["_MaxLimitPerTransaction"]),
                                IsDMT = row["_IsDMT"] is DBNull ? false : Convert.ToBoolean(row["_IsDMT"]),
                                IsRealTime = row["_IsRealTime"] is DBNull ? false : Convert.ToBoolean(row["_IsRealTime"]),
                                IsBBPS = row["_IsBBPS"] is DBNull ? false : Convert.ToBoolean(row["_IsBBPS"]),
                                IsAEPS = row["_IsAEPS"] is DBNull ? false : Convert.ToBoolean(row["_IsAEPS"]),
                                Mobileno = row["_MobileNo"] is DBNull ? string.Empty : row["_MobileNo"].ToString(),
                                WhatsAppNo = row["_WhatsappNo"] is DBNull ? string.Empty : row["_WhatsappNo"].ToString(),
                                VenderMail = row["_VenderEmailID"] is DBNull ? string.Empty : row["_VenderEmailID"].ToString(),
                                HandoutID = row["_HangoutID"] is DBNull ? string.Empty : row["_HangoutID"].ToString(),
                                ValidateURL = row["_ValidateURL"] is DBNull ? string.Empty : row["_ValidateURL"].ToString(),
                                AdditionalInfoListKey = row["_AdditionalInfoListKey"] is DBNull ? string.Empty : row["_AdditionalInfoListKey"].ToString(),
                                AdditionalInfoKey = row["_AdditionalInfoKey"] is DBNull ? string.Empty : row["_AdditionalInfoKey"].ToString(),
                                AdditionalInfoValue = row["_AdditionalInfoValue"] is DBNull ? string.Empty : row["_AdditionalInfoValue"].ToString(),
                                ValidationStatusKey = row["_ValidationStatusKey"] is DBNull ? string.Empty : row["_ValidationStatusKey"].ToString(),
                                ValidationStatusValue = row["_ValidationStatusValue"] is DBNull ? string.Empty : row["_ValidationStatusValue"].ToString(),
                                APIOutletIDMob = row["_APIOutletIDMob"] is DBNull ? string.Empty : row["_APIOutletIDMob"].ToString(),
                                GeoCodeAGT = row["_GeoCodeAGT"] is DBNull ? string.Empty : row["_GeoCodeAGT"].ToString(),
                                GeoCodeMOB = row["_GeoCodeMOB"] is DBNull ? string.Empty : row["_GeoCodeMOB"].ToString(),
                                GeoCodeINT = row["_GeoCodeINT"] is DBNull ? string.Empty : row["_GeoCodeINT"].ToString(),
                                HookTIDKey = row["_HookTIDKey"] is DBNull ? string.Empty : row["_HookTIDKey"].ToString(),
                                HookStatusKey = row["_HookStatusKey"] is DBNull ? string.Empty : row["_HookStatusKey"].ToString(),
                                HookResTypeID = row["_HookResTypeID"] is DBNull ? 0 : Convert.ToInt16(row["_HookResTypeID"]),
                                HookVendorKey = row["_HookVendorKey"] is DBNull ? string.Empty : row["_HookVendorKey"].ToString(),
                                HookLiveIDKey = row["_HookLiveIDKey"] is DBNull ? string.Empty : row["_HookLiveIDKey"].ToString(),
                                HookBalanceKey = row["_HookBalanceKey"] is DBNull ? string.Empty : row["_HookBalanceKey"].ToString(),
                                HookMsgKey = row["_HookMsgKey"] is DBNull ? string.Empty : row["_HookMsgKey"].ToString(),
                                HookSuccessCode = row["_HookSuccessCode"] is DBNull ? string.Empty : row["_HookSuccessCode"].ToString(),
                                HookFailCode = row["_HookFailCode"] is DBNull ? string.Empty : row["_HookFailCode"].ToString(),
                                FirstDelimiter = row["_FirstDelimiter"] is DBNull ? string.Empty : row["_FirstDelimiter"].ToString(),
                                SecondDelimiter = row["_SecondDelimiter"] is DBNull ? string.Empty : row["_SecondDelimiter"].ToString(),
                                HookFirstDelimiter = row["_HookFirstDelimiter"] is DBNull ? string.Empty : row["_HookFirstDelimiter"].ToString(),
                                HookSecondDelimiter = row["_HookSecondDelimiter"] is DBNull ? string.Empty : row["_HookSecondDelimiter"].ToString(),
                                BillFetchAPICode = row["_BillFetchAPICode"] is DBNull ? string.Empty : row["_BillFetchAPICode"].ToString(),
                            };
                            resp.Add(aPIDetail);
                        }
                    }
                    else
                    {
                        if (dt.Rows.Count > 0)
                        {
                            return new APIDetail
                            {
                                ID = dt.Rows[0]["_ID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_ID"]),
                                APIType = dt.Rows[0]["_APIType"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_APIType"]),
                                _APIType = APITypes.GetAPIType(dt.Rows[0]["_APIType"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_APIType"])),
                                Name = dt.Rows[0]["_Name"] is DBNull ? string.Empty : dt.Rows[0]["_Name"].ToString(),
                                URL = dt.Rows[0]["_URL"] is DBNull ? string.Empty : dt.Rows[0]["_URL"].ToString(),
                                APICode = dt.Rows[0]["_APICode"] is DBNull ? string.Empty : dt.Rows[0]["_APICode"].ToString(),
                                StatusCheckURL = dt.Rows[0]["_StatusCheckURL"] is DBNull ? string.Empty : dt.Rows[0]["_StatusCheckURL"].ToString(),
                                BalanceURL = dt.Rows[0]["_BalanceURL"] is DBNull ? string.Empty : dt.Rows[0]["_BalanceURL"].ToString(),
                                DisputeURL = dt.Rows[0]["_DisputeURL"] is DBNull ? string.Empty : dt.Rows[0]["_DisputeURL"].ToString(),
                                FetchBillURL = dt.Rows[0]["_FetchBillURL"] is DBNull ? string.Empty : dt.Rows[0]["_FetchBillURL"].ToString(),
                                RequestMethod = dt.Rows[0]["_RequestMethod"] is DBNull ? string.Empty : dt.Rows[0]["_RequestMethod"].ToString(),
                                StatusName = dt.Rows[0]["_StatusName"] is DBNull ? string.Empty : dt.Rows[0]["_StatusName"].ToString(),
                                FailCode = dt.Rows[0]["_FailCode"] is DBNull ? string.Empty : dt.Rows[0]["_FailCode"].ToString(),
                                SuccessCode = dt.Rows[0]["_SuccessCode"] is DBNull ? string.Empty : dt.Rows[0]["_SuccessCode"].ToString(),
                                ActiveSts = dt.Rows[0]["_ActiveSts"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_ActiveSts"]),
                                IsOutletRequired = dt.Rows[0]["_IsOutletRequired"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsOutletRequired"]),
                                FixedOutletID = dt.Rows[0]["_FixedOutletID"] is DBNull ? string.Empty : dt.Rows[0]["_FixedOutletID"].ToString(),
                                GroupID = dt.Rows[0]["_GroupID"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_GroupID"]),
                                VendorID = dt.Rows[0]["_VendorID"] is DBNull ? string.Empty : dt.Rows[0]["_VendorID"].ToString(),
                                ResponseTypeID = dt.Rows[0]["_ResponseTypeID"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_ResponseTypeID"]),
                                Remark = dt.Rows[0]["_Remark"] is DBNull ? string.Empty : dt.Rows[0]["_Remark"].ToString(),
                                EntryBy = dt.Rows[0]["_EntryBy"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_EntryBy"]),
                                EntryDate = dt.Rows[0]["_EntryDate"] is DBNull ? string.Empty : dt.Rows[0]["_EntryDate"].ToString(),
                                ModifyBy = dt.Rows[0]["_ModifyBy"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_ModifyBy"]),
                                ModifyDate = dt.Rows[0]["_ModifyDate"] is DBNull ? string.Empty : dt.Rows[0]["_ModifyDate"].ToString(),
                                IsOpDownAllow = dt.Rows[0]["_IsOpDownAllow"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsOpDownAllow"]),
                                LiveID = dt.Rows[0]["_LiveID"] is DBNull ? string.Empty : dt.Rows[0]["_LiveID"].ToString(),
                                SurchargeType = dt.Rows[0]["_SurchargeType"] is DBNull ? 1 : Convert.ToInt16(dt.Rows[0]["_SurchargeType"]),
                                MsgKey = dt.Rows[0]["_MsgKey"] is DBNull ? string.Empty : dt.Rows[0]["_MsgKey"].ToString(),
                                BillNoKey = dt.Rows[0]["_BillNoKey"] is DBNull ? string.Empty : dt.Rows[0]["_BillNoKey"].ToString(),
                                BillDateKey = dt.Rows[0]["_BillDateKey"] is DBNull ? string.Empty : dt.Rows[0]["_BillDateKey"].ToString(),
                                BillAmountKey = dt.Rows[0]["_BillAmountKey"] is DBNull ? string.Empty : dt.Rows[0]["_BillAmountKey"].ToString(),
                                CustomerNameKey = dt.Rows[0]["_CustomerNameKey"] is DBNull ? string.Empty : dt.Rows[0]["_CustomerNameKey"].ToString(),
                                DueDateKey = dt.Rows[0]["_DueDateKey"] is DBNull ? string.Empty : dt.Rows[0]["_DueDateKey"].ToString(),
                                GroupName = dt.Rows[0]["_GroupName"] is DBNull ? string.Empty : dt.Rows[0]["_GroupName"].ToString(),
                                GroupCode = dt.Rows[0]["_GroupCode"] is DBNull ? string.Empty : dt.Rows[0]["_GroupCode"].ToString(),
                                BillStatusKey = dt.Rows[0]["_BillStatusKey"] is DBNull ? string.Empty : dt.Rows[0]["_BillStatusKey"].ToString(),
                                BillStatusValue = dt.Rows[0]["_BillStatusValue"] is DBNull ? string.Empty : dt.Rows[0]["_BillStatusValue"].ToString(),
                                IsOutletManual = dt.Rows[0]["_IsOutletManual"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsOutletManual"]),
                                ContentType = dt.Rows[0]["_ContentType"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_ContentType"]),
                                BalanceKey = dt.Rows[0]["_BalanceKey"] is DBNull ? string.Empty : dt.Rows[0]["_BalanceKey"].ToString(),
                                BillReqMethod = dt.Rows[0]["_BillReqMethod"] is DBNull ? string.Empty : dt.Rows[0]["_BillReqMethod"].ToString(),
                                BillResTypeID = dt.Rows[0]["_BillResTypeID"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_BillResTypeID"]),
                                DFormatID = dt.Rows[0]["_DFormatID"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_DFormatID"]),
                                InSwitch = dt.Rows[0]["_InSwitch"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_InSwitch"]),
                                ErrorCodeKey = dt.Rows[0]["_ErrorCodeKey"] is DBNull ? string.Empty : dt.Rows[0]["_ErrorCodeKey"].ToString(),
                                RefferenceKey = dt.Rows[0]["_RefKey"] is DBNull ? string.Empty : dt.Rows[0]["_RefKey"].ToString(),
                                MaxLimitPerTransaction = dt.Rows[0]["_MaxLimitPerTransaction"] is DBNull ? 0 :Convert.ToInt32(dt.Rows[0]["_MaxLimitPerTransaction"]),
                                IsDMT = dt.Rows[0]["_IsDMT"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsDMT"]),
                                IsRealTime = dt.Rows[0]["_IsRealTime"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsRealTime"]),
                                IsBBPS = dt.Rows[0]["_IsBBPS"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsBBPS"]),
                                IsAEPS = dt.Rows[0]["_IsAEPS"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsAEPS"]),
                                Mobileno = dt.Rows[0]["_MobileNo"] is DBNull ? string.Empty : dt.Rows[0]["_MobileNo"].ToString(),
                                WhatsAppNo = dt.Rows[0]["_WhatsappNo"] is DBNull ? string.Empty : dt.Rows[0]["_WhatsappNo"].ToString(),
                                VenderMail = dt.Rows[0]["_VenderEmailID"] is DBNull ? string.Empty : dt.Rows[0]["_VenderEmailID"].ToString(),
                                HandoutID = dt.Rows[0]["_HangoutID"] is DBNull ? string.Empty : dt.Rows[0]["_HangoutID"].ToString(),
                                ValidateURL = dt.Rows[0]["_ValidateURL"] is DBNull ? string.Empty : dt.Rows[0]["_ValidateURL"].ToString(),
                                AdditionalInfoListKey = dt.Rows[0]["_AdditionalInfoListKey"] is DBNull ? string.Empty : dt.Rows[0]["_AdditionalInfoListKey"].ToString(),
                                AdditionalInfoKey = dt.Rows[0]["_AdditionalInfoKey"] is DBNull ? string.Empty : dt.Rows[0]["_AdditionalInfoKey"].ToString(),
                                AdditionalInfoValue = dt.Rows[0]["_AdditionalInfoValue"] is DBNull ? string.Empty : dt.Rows[0]["_AdditionalInfoValue"].ToString(),
                                ValidationStatusKey = dt.Rows[0]["_ValidationStatusKey"] is DBNull ? string.Empty : dt.Rows[0]["_ValidationStatusKey"].ToString(),
                                ValidationStatusValue = dt.Rows[0]["_ValidationStatusValue"] is DBNull ? string.Empty : dt.Rows[0]["_ValidationStatusValue"].ToString(),
                                APIOutletIDMob = dt.Rows[0]["_APIOutletIDMob"] is DBNull ? string.Empty : dt.Rows[0]["_APIOutletIDMob"].ToString(),
                                GeoCodeAGT = dt.Rows[0]["_GeoCodeAGT"] is DBNull ? string.Empty : dt.Rows[0]["_GeoCodeAGT"].ToString(),
                                GeoCodeMOB = dt.Rows[0]["_GeoCodeMOB"] is DBNull ? string.Empty : dt.Rows[0]["_GeoCodeMOB"].ToString(),
                                GeoCodeINT = dt.Rows[0]["_GeoCodeINT"] is DBNull ? string.Empty : dt.Rows[0]["_GeoCodeINT"].ToString(),
                                HookTIDKey = dt.Rows[0]["_HookTIDKey"] is DBNull ? string.Empty : dt.Rows[0]["_HookTIDKey"].ToString(),
                                HookStatusKey = dt.Rows[0]["_HookStatusKey"] is DBNull ? string.Empty : dt.Rows[0]["_HookStatusKey"].ToString(),
                                HookResTypeID = dt.Rows[0]["_HookResTypeID"] is DBNull ? 0 :Convert.ToInt16(dt.Rows[0]["_HookResTypeID"]),
                                HookVendorKey = dt.Rows[0]["_HookVendorKey"] is DBNull ? string.Empty : dt.Rows[0]["_HookVendorKey"].ToString(),
                                HookLiveIDKey = dt.Rows[0]["_HookLiveIDKey"] is DBNull ? string.Empty : dt.Rows[0]["_HookLiveIDKey"].ToString(),
                                HookBalanceKey = dt.Rows[0]["_HookBalanceKey"] is DBNull ? string.Empty : dt.Rows[0]["_HookBalanceKey"].ToString(),
                                HookSuccessCode = dt.Rows[0]["_HookSuccessCode"] is DBNull ? string.Empty : dt.Rows[0]["_HookSuccessCode"].ToString(),
                               HookFailCode = dt.Rows[0]["_HookFailCode"] is DBNull ? string.Empty : dt.Rows[0]["_HookFailCode"].ToString(),
                                HookMsgKey = dt.Rows[0]["_HookMsgKey"] is DBNull ? string.Empty : dt.Rows[0]["_HookMsgKey"].ToString(),
                                FirstDelimiter = dt.Rows[0]["_FirstDelimiter"] is DBNull ? string.Empty : dt.Rows[0]["_FirstDelimiter"].ToString(),
                                SecondDelimiter = dt.Rows[0]["_SecondDelimiter"] is DBNull ? string.Empty : dt.Rows[0]["_SecondDelimiter"].ToString(),
                                HookFirstDelimiter = dt.Rows[0]["_HookFirstDelimiter"] is DBNull ? string.Empty : dt.Rows[0]["_HookFirstDelimiter"].ToString(),
                                HookSecondDelimiter = dt.Rows[0]["_HookSecondDelimiter"] is DBNull ? string.Empty : dt.Rows[0]["_HookSecondDelimiter"].ToString(),
                                BillFetchAPICode = dt.Rows[0]["_BillFetchAPICode"] is DBNull ? string.Empty : dt.Rows[0]["_BillFetchAPICode"].ToString(),

                            };
                        }
                    }
                }
            }
            catch (Exception ex) { }
            if (!req.IsListType) return new APIDetail();
            return resp;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetAPI";
    }
}
