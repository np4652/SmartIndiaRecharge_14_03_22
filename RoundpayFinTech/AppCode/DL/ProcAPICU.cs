using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcAPICU : IProcedure
    {
        private readonly IDAL _dal;
        public ProcAPICU(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (APIDetailReq)obj;
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            SqlParameter[] param = {
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@LT",req.LT),
                new SqlParameter("@ID",req.ID),
                new SqlParameter("@APIType",req.APIType),
                new SqlParameter("@Name",req.Name),
                new SqlParameter("@Url ",req.URL??string.Empty),
                new SqlParameter("@StatusCheckURL",req.StatusCheckURL??string.Empty),
                new SqlParameter("@BalanceURL",req.BalanceURL??string.Empty),
                new SqlParameter("@DisputeURL",req.DisputeURL??string.Empty),
                new SqlParameter("@FetchBillURL",req.FetchBillURL??string.Empty),
                new SqlParameter("@RequestMethod",req.RequestMethod??string.Empty),
                new SqlParameter("@StatusName",req.StatusName??string.Empty),
                new SqlParameter("@SuccessCode",req.SuccessCode??string.Empty),
                new SqlParameter("@FailCode",req.FailCode??string.Empty),
                new SqlParameter("@LiveID",req.LiveID??string.Empty),
                new SqlParameter("@VendorID",req.VendorID??string.Empty),
                new SqlParameter("@ResponseTypeID",req.ResponseTypeID),
                new SqlParameter("@Remark",req.Remark??string.Empty),
                new SqlParameter("@IsOutletRequired",req.IsOutletRequired),
                new SqlParameter("@FixedOutletID",req.FixedOutletID??string.Empty),
                new SqlParameter("@IsOpDownAllow",req.IsOpDownAllow),
                new SqlParameter("@IP",req.IP??string.Empty),
                new SqlParameter("@Browser",req.Browser??string.Empty),
                new SqlParameter("@SurchargeType",req.SurchargeType),
                new SqlParameter("@MsgKey",req.MsgKey??string.Empty),
                new SqlParameter("@BillNoKey",req.BillNoKey??string.Empty),
                new SqlParameter("@BillDateKey",req.BillDateKey??string.Empty),
                new SqlParameter("@DueDateKey",req.DueDateKey??string.Empty),
                new SqlParameter("@BillAmountKey",req.BillAmountKey??string.Empty),
                new SqlParameter("@CustomerNameKey",req.CustomerNameKey??string.Empty),
                new SqlParameter("@ErrorCodeKey",req.ErrorCodeKey??string.Empty),
                new SqlParameter("@GroupID",req.GroupID),
                new SqlParameter("@GroupName",req.GroupName??string.Empty),
                new SqlParameter("@GroupCode",req.GroupCode??string.Empty),
                new SqlParameter("@BillStatusKey",req.BillStatusKey??string.Empty),
                new SqlParameter("@BillStatusValue",req.BillStatusValue??string.Empty),
                new SqlParameter("@IsOutletManual",req.IsOutletManual),
                new SqlParameter("@ContentType",req.ContentType),
                new SqlParameter("@BalanceKey",req.BalanceKey??string.Empty),
                new SqlParameter("@BillReqMethod",req.BillReqMethod??"GET"),
                new SqlParameter("@BillResTypeID",req.BillResTypeID),
                new SqlParameter("@InSwitch",req.InSwitch),
                new SqlParameter("@RefKey",req.RefferenceKey??string.Empty),
                new SqlParameter("@DFormatID",req.DFormatID),
                new SqlParameter("@MaxLimitPerTransaction",req.MaxLimitPerTransaction),
                new SqlParameter("@WID",req.WID),
                new SqlParameter("@VenderMail",req.VenderMail??string.Empty),
                new SqlParameter("@HandoutID",req.HandoutID??string.Empty),
                new SqlParameter("@Mobileno",req.Mobileno??string.Empty),
                new SqlParameter("@WhatsAppNo",req.WhatsAppNo??string.Empty),
                new SqlParameter("@PartnerUserID",req.PartnerUserID),
                new SqlParameter("@ValidateURL",req.ValidateURL??string.Empty),
                new SqlParameter("@AdditionalInfoListKey",req.AdditionalInfoListKey??string.Empty),
                new SqlParameter("@AdditionalInfoKey",req.AdditionalInfoKey??string.Empty),
                new SqlParameter("@AdditionalInfoValue",req.AdditionalInfoValue??string.Empty),
                new SqlParameter("@ValidationStatusKey",req.ValidationStatusKey??string.Empty),
                new SqlParameter("@ValidationStatusValue",req.ValidationStatusValue??string.Empty),
                new SqlParameter("@APIOutletIDMob",req.APIOutletIDMob??string.Empty),
                new SqlParameter("@GeoCodeAGT",req.GeoCodeAGT??string.Empty),
                new SqlParameter("@GeoCodeMOB",req.GeoCodeMOB??string.Empty),
                new SqlParameter("@GeoCodeINT",req.GeoCodeINT??string.Empty),
                new SqlParameter("@HookResTypeID",req.HookResTypeID),
                new SqlParameter("@HookStatusKey",req.HookStatusKey??string.Empty),
                new SqlParameter("@HookTIDKey",req.HookTIDKey??string.Empty),
                new SqlParameter("@HookVendorKey",req.HookVendorKey??string.Empty),
                new SqlParameter("@HookLiveIDKey",req.HookLiveIDKey??string.Empty),
                new SqlParameter("@HookMsgKey",req.HookMsgKey??string.Empty),
                new SqlParameter("@HookSuccessCode",req.HookSuccessCode??string.Empty),
                new SqlParameter("@HookFailCode",req.HookFailCode??string.Empty),
                new SqlParameter("@FirstDelimiter",req.FirstDelimiter??string.Empty),
                new SqlParameter("@SecondDelimiter",req.SecondDelimiter??string.Empty),
                new SqlParameter("@HookFirstDelimiter",req.HookFirstDelimiter??string.Empty),
                new SqlParameter("@HookSecondDelimiter",req.HookSecondDelimiter??string.Empty),
                new SqlParameter("@BillFetchAPICode",req.BillFetchAPICode??string.Empty),
            };
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? string.Empty : dt.Rows[0]["Msg"].ToString();
                }
            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LT,
                    UserId = req.LoginID
                });
            }
            return res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_API_CU";
    }
}
