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
    public class ProcGetUserSubscription : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetUserSubscription(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@TopRows",req.CommonInt),
                 new SqlParameter("@Status",req.CommonStr),
                 new SqlParameter("@MobileNo",req.CommonStr2),
                 new SqlParameter("@CustomerID",req.CommonInt2),
                 new SqlParameter("@Day",req.CommonStr3),
            };

            var getIntouches = new List<GetIntouch>();
            var CustomerCareDetail = new List<CustomerCareDetails>();
            var resp = new GetinTouctListModel
            {
                GetIntouchList = getIntouches,
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

                    foreach (DataRow row in dt.Rows )
                    {
                        var GetIntouch = new GetIntouch
                        {
                            ID = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                            
                            Name = row["_Name"] is DBNull ? "" : row["_Name"].ToString(),
                            EmailID = row["_userEmail"] is DBNull ? "" : row["_userEmail"].ToString(),
                            MobileNo = row["_MobileNO"] is DBNull ? "" : row["_MobileNO"].ToString(),
                            Message = row["_Message"] is DBNull ? "" : row["_Message"].ToString(),
                            RequestStatus = row["_RequestStatus"] is DBNull ? "" : row["_RequestStatus"].ToString(),
                            CustomercareID = row["CustomerCareID"] is DBNull ? 0 : Convert.ToInt32(row["CustomerCareID"]),
                            CustomerCareName = row["customercareName"] is DBNull ? "" : row["customercareName"].ToString(),
                            RequestPage = row["_RequestPage"] is DBNull ? "" : row["_RequestPage"].ToString(),
                            IsMobileMultiple = Convert.ToBoolean(row["IsMobileMultiple"]),
                            Remarks = row["_Remarks"] is DBNull ? "" : row["_Remarks"].ToString(),
                            EntryData = row["_Entrydate"] is DBNull ? "" : row["_Entrydate"].ToString(),
                            ModifyDate = row["_ModifyDate"] is DBNull ? "" : row["_ModifyDate"].ToString(),
                            NextFollowupdate = row["_NextFollowUpdate"] is DBNull ? "" : row["_NextFollowUpdate"].ToString(),
                        };
                        getIntouches.Add(GetIntouch);
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
                        };
                        CustomerCareDetail.Add(GetIntouch);
                    }

                }

            }
            catch (Exception ex)
            {
            }
            return resp;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "Proc_GetUserSubscription";
    }

}
