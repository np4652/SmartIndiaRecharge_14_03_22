using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcAPIGroupDetail : IProcedure
    {
        private readonly IDAL _dal;
        public ProcAPIGroupDetail(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var ID = (int)obj;
            SqlParameter[] param = {
                new SqlParameter("@ID",ID)
            };
            var Query = "select _ID, _Name, _GroupCode, _RequestMethod, _StatusName, _LiveID, _VendorID,  _FailCode, _SuccessCode, _MsgKey, _BillNoKey, _BillDateKey, _DueDateKey, _BillAmountKey, _CustomerNameKey, _ErrorCodeKey, _ResponseTypeID,_BillStatusKey,_BillStatusValue,_BalanceKey,_AdditionalInfoListKey,_AdditionalInfoKey,_AdditionalInfoValue from MASTER_API_GROUP where _ID=@ID";
            var res = new APIGroupDetail();
            try
            {
                var dt = _dal.Get(Query, param);
                if (dt.Rows.Count > 0)
                {
                    res.GroupID = dt.Rows[0]["_ID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_ID"]);
                    res.GroupName = dt.Rows[0]["_Name"] is DBNull ? string.Empty : dt.Rows[0]["_Name"].ToString();
                    res.GroupCode = dt.Rows[0]["_GroupCode"] is DBNull ? string.Empty : dt.Rows[0]["_GroupCode"].ToString();
                    res.RequestMethod = dt.Rows[0]["_RequestMethod"] is DBNull ? string.Empty : dt.Rows[0]["_RequestMethod"].ToString();
                    res.StatusName = dt.Rows[0]["_StatusName"] is DBNull ? string.Empty : dt.Rows[0]["_StatusName"].ToString();
                    res.LiveID = dt.Rows[0]["_LiveID"] is DBNull ? string.Empty : dt.Rows[0]["_LiveID"].ToString();
                    res.VendorID = dt.Rows[0]["_VendorID"] is DBNull ? string.Empty : dt.Rows[0]["_VendorID"].ToString();
                    res.FailCode = dt.Rows[0]["_FailCode"] is DBNull ? string.Empty : dt.Rows[0]["_FailCode"].ToString();
                    res.SuccessCode = dt.Rows[0]["_SuccessCode"] is DBNull ? string.Empty : dt.Rows[0]["_SuccessCode"].ToString();
                    res.MsgKey = dt.Rows[0]["_GroupCode"] is DBNull ? string.Empty : dt.Rows[0]["_MsgKey"].ToString();
                    res.BillNoKey = dt.Rows[0]["_BillNoKey"] is DBNull ? string.Empty : dt.Rows[0]["_BillNoKey"].ToString();
                    res.BillDateKey = dt.Rows[0]["_BillDateKey"] is DBNull ? string.Empty : dt.Rows[0]["_BillDateKey"].ToString();
                    res.DueDateKey = dt.Rows[0]["_DueDateKey"] is DBNull ? string.Empty : dt.Rows[0]["_DueDateKey"].ToString();
                    res.BillAmountKey = dt.Rows[0]["_BillAmountKey"] is DBNull ? string.Empty : dt.Rows[0]["_BillAmountKey"].ToString();
                    res.CustomerNameKey = dt.Rows[0]["_CustomerNameKey"] is DBNull ? string.Empty : dt.Rows[0]["_CustomerNameKey"].ToString();
                    res.ErrorCodeKey = dt.Rows[0]["_ErrorCodeKey"] is DBNull ? string.Empty : dt.Rows[0]["_ErrorCodeKey"].ToString();
                    res.ResponseTypeID = dt.Rows[0]["_ResponseTypeID"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_ResponseTypeID"]);
                    res.BillStatusKey = dt.Rows[0]["_BillStatusKey"] is DBNull ? string.Empty : dt.Rows[0]["_BillStatusKey"].ToString();
                    res.BillStatusValue = dt.Rows[0]["_BillStatusValue"] is DBNull ? string.Empty : dt.Rows[0]["_BillStatusValue"].ToString();
                    res.BalanceKey = dt.Rows[0]["_BalanceKey"] is DBNull ? string.Empty : dt.Rows[0]["_BalanceKey"].ToString();
                    res.AdditionalInfoListKey = dt.Rows[0]["_AdditionalInfoListKey"] is DBNull ? string.Empty : dt.Rows[0]["_AdditionalInfoListKey"].ToString();
                    res.AdditionalInfoKey = dt.Rows[0]["_AdditionalInfoKey"] is DBNull ? string.Empty : dt.Rows[0]["_AdditionalInfoKey"].ToString();
                    res.AdditionalInfoValue = dt.Rows[0]["_AdditionalInfoValue"] is DBNull ? string.Empty : dt.Rows[0]["_AdditionalInfoValue"].ToString();
                }
            }
            catch (Exception)
            {
            }
            return res;
        }

        public object Call()
        {
            var lst = new List<APIGroupDetail>();
            try
            {
                var dt = _dal.Get(GetName());
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow item in dt.Rows)
                    {
                        lst.Add(new APIGroupDetail
                        {
                            GroupID = item["_ID"] is DBNull ? 0 : Convert.ToInt32(item["_ID"]),
                            GroupName = item["_Name"] is DBNull ? string.Empty : item["_Name"].ToString(),
                            GroupCode = item["_GroupCode"] is DBNull ? string.Empty : item["_GroupCode"].ToString(),
                            RequestMethod = item["_RequestMethod"] is DBNull ? string.Empty : item["_RequestMethod"].ToString(),
                            StatusName = item["_StatusName"] is DBNull ? string.Empty : item["_StatusName"].ToString(),
                            LiveID = item["_LiveID"] is DBNull ? string.Empty : item["_LiveID"].ToString(),
                            VendorID = item["_VendorID"] is DBNull ? string.Empty : item["_VendorID"].ToString(),
                            FailCode = item["_FailCode"] is DBNull ? string.Empty : item["_FailCode"].ToString(),
                            SuccessCode = item["_SuccessCode"] is DBNull ? string.Empty : item["_SuccessCode"].ToString(),
                            MsgKey = item["_GroupCode"] is DBNull ? string.Empty : item["_MsgKey"].ToString(),
                            BillNoKey = item["_BillNoKey"] is DBNull ? string.Empty : item["_BillNoKey"].ToString(),
                            BillDateKey = item["_BillDateKey"] is DBNull ? string.Empty : item["_BillDateKey"].ToString(),
                            DueDateKey = item["_DueDateKey"] is DBNull ? string.Empty : item["_DueDateKey"].ToString(),
                            BillAmountKey = item["_BillAmountKey"] is DBNull ? string.Empty : item["_BillAmountKey"].ToString(),
                            CustomerNameKey = item["_CustomerNameKey"] is DBNull ? string.Empty : item["_CustomerNameKey"].ToString(),
                            ErrorCodeKey = item["_ErrorCodeKey"] is DBNull ? string.Empty : item["_ErrorCodeKey"].ToString(),
                            ResponseTypeID = item["_ResponseTypeID"] is DBNull ? 0 : Convert.ToInt16(item["_ResponseTypeID"]),
                            BillStatusKey = item["_BillStatusKey"] is DBNull ? string.Empty : item["_BillStatusKey"].ToString(),
                            BillStatusValue = item["_BillStatusValue"] is DBNull ? string.Empty : item["_BillStatusValue"].ToString(),
                            BalanceKey = item["_BalanceKey"] is DBNull ? string.Empty : item["_BalanceKey"].ToString(),
                            AdditionalInfoListKey = item["_AdditionalInfoListKey"] is DBNull ? string.Empty : item["_AdditionalInfoListKey"].ToString(),
                            AdditionalInfoKey = item["_AdditionalInfoKey"] is DBNull ? string.Empty : item["_AdditionalInfoKey"].ToString(),
                            AdditionalInfoValue = item["_AdditionalInfoValue"] is DBNull ? string.Empty : item["_AdditionalInfoValue"].ToString()
                        });
                    }
                }
            }
            catch (Exception) { }
            return lst;
        }

        public string GetName() => "select _ID, _Name, _GroupCode, _RequestMethod, _StatusName, _LiveID, _VendorID, _FailCode, _SuccessCode, _MsgKey, _BillNoKey, _BillDateKey, _DueDateKey, _BillAmountKey, _CustomerNameKey, _ErrorCodeKey, _ResponseTypeID,_BillStatusKey,_BillStatusValue,_BalanceKey,_AdditionalInfoListKey,_AdditionalInfoKey,_AdditionalInfoValue from MASTER_API_GROUP order by _Name";
    }

}
