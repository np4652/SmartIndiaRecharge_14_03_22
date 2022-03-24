using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetPendingRefundForNotification : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcGetPendingRefundForNotification(IDAL dal) => _dal = dal;

        public async Task<object> Call(object obj)
        {
            var request = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@APIID",request.CommonInt)
            };
            var response = new List<PendingRechargeNotification>();
            try
            {
                DataTable dt = await _dal.GetAsync(GetName(), param);
                if (dt.Rows.Count > 0 && Convert.ToInt32(dt.Rows[0][0]) == ErrorCodes.One)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        var transaction = new PendingRechargeNotification
                        {
                            AccountNo = Convert.ToString(row["_Account"]),
                            Duration = Convert.ToString(row["_Duration"]),
                            APIID = row["_APIID"] is DBNull ? 0 : Convert.ToInt32(row["_APIID"]),
                            APICode = row["_APICode"] is DBNull ? string.Empty : row["_APICode"].ToString(),
                            WhatsappNo = row["_WhatsappNo"] is DBNull ? string.Empty : row["_WhatsappNo"].ToString(),
                            HangoutId = row["_HangoutId"] is DBNull ? string.Empty : row["_HangoutId"].ToString(),
                            Name = row["_Name"] is DBNull ? string.Empty : row["_Name"].ToString(),
                            CCID = row["_CCID"] is DBNull ? 0 : Convert.ToInt32(row["_CCID"]),
                            CCName = row["_CCName"] is DBNull ? string.Empty : row["_CCName"].ToString(),
                            
                            //TelegramNo = row["_TelegramNo"] is DBNull ? "" : row["_TelegramNo"].ToString(),
                        };
                        response.Add(transaction);
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
                    LoginTypeID = request.LoginTypeID,
                    UserId = request.LoginID
                });
            }
            return response;
        }

        public Task<object> Call() => throw new NotImplementedException();

        public string GetName() => @"Declare @CCID int,@CCName varchar(80);
                                     Select  TOP 1 @CCID=_ID,@CCName=_Name from tbl_CustomerCare(nolock) where _IsAdmin = 1
                                     select distinct 1,api._WhatsappNo, api._HangoutId,api._Name,rl._TID,rl._TransactionID,rl._Account,
                                                     rl._RequestedAmount, datediff(minute,rl._EntryDate , getDate()) _Duration,
                                                     rl._APIID ,api._APICode,Cast(sms._EntryDate as Date) _EntryDate,@CCID _CCID,@CCName _CCName
                                     from   tbl_RefundLog rl (nolock) 
                                            Inner join tbl_API api (nolock) on api._ID=rl._APIID
                                            Left Join tbl_SendSMS sms(nolock) on sms._MobileNo = api._WhatsappNo or sms._MobileNo = api._HangoutID
                                     where   rl._refundStatus=2 and (sms._EntryDate is null or datediff(minute,sms._EntryDate , getDate()) >= 30)";
    }
}