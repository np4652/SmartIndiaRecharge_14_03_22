using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
namespace RoundpayFinTech.AppCode.DL
{
    
    public class ProcGetWhatsappContactsSearch : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetWhatsappContactsSearch(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                 new SqlParameter("@SearchValue",req.CommonStr),
                  new SqlParameter("@UnSeenMessage",req.CommonInt),
                    new SqlParameter("@Apicode",req.CommonStr2),
                     new SqlParameter("@IsOneDayChat",req.CommonBool),
                     new SqlParameter("@SenderNoId",req.CommonInt2),
                     new SqlParameter("@Task",req.CommonInt3),
                      new SqlParameter("@NotReplied",req.CommonStr3)

            };
            var getWhatsappContacts = new List<GetWhatsappContact>();
            var CustomerCareDetail = new List<CustomerCareDetails>();
            var resp = new GetWhatsappContactListModel
            {
                GetWhatsappContactList = getWhatsappContacts,
                CustomerCareDetail = CustomerCareDetail
            };
            try
            {
                DataSet ds = _dal.GetByProcedureAdapterDS(GetName(), param);
                DataTable dt = new DataTable();
                DataTable dtCus = new DataTable();
                dt = ds.Tables[0];
                dtCus = ds.Tables[1];
                if (dt.Rows.Count > 0 && !dt.Columns.Contains("Msg"))
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        var GetWhatsappContact = new GetWhatsappContact
                        {
                            ID = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                            MobileNo = row["_MobileNO"] is DBNull ? "" : row["_MobileNO"].ToString(),
                            SenderName = row["_SenderName"] is DBNull ? "" : row["_SenderName"].ToString(),
                            Role = row["_Role"] is DBNull ? "" : row["_Role"].ToString(),
                            NewMsgs = row["_NewMsgs"] is DBNull ? 0 : Convert.ToInt32(row["_NewMsgs"]),
                            APIID = row["_APIID"] is DBNull ? 0 : Convert.ToInt32(row["_APIID"]),
                            ApICode = row["_ApICode"] is DBNull ? "" : row["_ApICode"].ToString(),
                            RemChatTime = row["_RemChatTime"] is DBNull ? 0 : Convert.ToInt32(row["_RemChatTime"]),
                            SenderMobileNo = row["_SenderMobileNo"] is DBNull ? "" : row["_SenderMobileNo"].ToString(),
                            PrefixName = row["_PrefixName"] is DBNull ? "" : row["_PrefixName"].ToString(),
                            SenderNoID = row["_SenderNoID"] is DBNull ? 0 : Convert.ToInt32(row["_SenderNoID"]),
                            Task = row["_Task"] is DBNull ? 0 : Convert.ToInt32(row["_Task"])
                        };
                        getWhatsappContacts.Add(GetWhatsappContact);
                    }
                }

                if (dtCus.Rows.Count > 0 && !dt.Columns.Contains("Msg"))
                {

                    foreach (DataRow row in dtCus.Rows)
                    {
                        var GetIntouch = new CustomerCareDetails
                        {
                            CustomercareID = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                            CustomerCareName = row["_Name"] is DBNull ? "" : row["_Name"].ToString(),
                            //CustomerCareMobile = row["_MobileNo"] is DBNull ? "" : row["_MobileNo"].ToString()
                        };
                        CustomerCareDetail.Add(GetIntouch);
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
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                });
            }
            return resp;
        }

        public object Call() => throw new NotImplementedException();


        public string GetName() => "Proc_GetWhatsappContactSearch";

    }
}
