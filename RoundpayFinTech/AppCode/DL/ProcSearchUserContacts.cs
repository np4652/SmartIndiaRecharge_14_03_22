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
    public class ProcSearchUserContacts : IProcedure
    {
        private readonly IDAL _dal;
        public ProcSearchUserContacts(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                 new SqlParameter("@SearchValue",req.CommonStr)
            };
            var getWhatsappContacts = new List<GetWhatsappContact>();
            var getWhatsappAPI = new List<WhatsappAPI>();
            var resp = new GetWhatsappContactListModel
            {
                RoleID=req.CommonInt,
                LoginTypeID = req.LoginTypeID,
                UserID = req.LoginID,
                GetWhatsappContactList = getWhatsappContacts,
                WhatsappAPIList = getWhatsappAPI
            };
            try
            {
                DataTable dt = new DataTable();
                DataTable dtapi = new DataTable();
                DataSet ds=_dal.GetByProcedureAdapterDS(GetName(), param);
                dt = ds.Tables[0];
                 dtapi = ds.Tables[1];
                if (dt.Rows.Count > 0 && !dt.Columns.Contains("Msg"))
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        var GetWhatsappContact = new GetWhatsappContact
                        {
                            ID = row["UserID"] is DBNull ? 0 : Convert.ToInt32(row["UserID"]),
                            MobileNo = row["_MobileNo"] is DBNull ? "" : row["_MobileNo"].ToString(),
                            UserContact = row["FullName"] is DBNull ? "" : row["FullName"].ToString(),
                        };
                        getWhatsappContacts.Add(GetWhatsappContact);
                    }
                }
                if (dtapi.Rows.Count > 0 && !dtapi.Columns.Contains("Msg"))
                {
                    foreach (DataRow row in dtapi.Rows)
                    {
                        var GetWhatsappAPI = new WhatsappAPI
                        {
                            APIID = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                            APINAME = row["_Name"] is DBNull ? "" : row["_Name"].ToString(),
                           
                        };
                        getWhatsappAPI.Add(GetWhatsappAPI);
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
        public string GetName() => "proc_Search_UserContacts";
    }
}
