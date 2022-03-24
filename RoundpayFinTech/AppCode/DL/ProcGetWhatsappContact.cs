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
    public class ProcGetWhatsappContact : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetWhatsappContact(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
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
       

        public string GetName() => "Proc_GetWhatsappContact";

    }
}
