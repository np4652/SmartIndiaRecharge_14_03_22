using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetNotifications : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetNotifications(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var UserID = (int)obj;
            SqlParameter[] param = { 
                new SqlParameter("@UserID",UserID)
            };
            var res = new List<Notification>();
            try
            {
                var dt = _dal.GetByProcedure(GetName(),param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow item in dt.Rows)
                    {
                        res.Add(new Notification
                        {
                            ID = item["_ID"] is DBNull ? 0 : Convert.ToInt32(item["_ID"]),
                            Title = item["_Title"] is DBNull ? "" : item["_Title"].ToString(),
                            Url = item["_Url"] is DBNull ? "" : item["_Url"].ToString(),
                            ImageUrl = item["_ImageUrl"] is DBNull ? "" : item["_ImageUrl"].ToString(),
                            Message = item["_Message"] is DBNull ? "" : item["_Message"].ToString(),
                            Response = item["_Response"] is DBNull ? "" : item["_Response"].ToString(),
                            EntryDate = item["_EntryDate"] is DBNull ? "" : item["_EntryDate"].ToString(),
                            WID = item["_WID"] is DBNull ? 0 : Convert.ToInt32(item["_WID"])
                        });
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_GetNotifications";
    }
}
