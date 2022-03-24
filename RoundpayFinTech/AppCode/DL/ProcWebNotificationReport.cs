using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcWebNotificationReport : IProcedure
    {
        private readonly IDAL _dal;
        public ProcWebNotificationReport(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonFilter)obj;
            var res = new List<WebNotification>();
            SqlParameter[] param = {
                new SqlParameter("@TopRows",req.TopRows),
                new SqlParameter("@SearchText",req.MobileNo??"")
            };

            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        res.Add(new WebNotification
                        {
                            ID = Convert.ToInt32(dr["_ID"]),
                            UserID = Convert.ToInt32(dr["_UserID"]),
                            UserName = Convert.ToString(dr["_Name"]),
                            UserMobileNo = Convert.ToString(dr["_MobileNo"]),
                            Title = Convert.ToString(dr["_Title"]),
                            Notification = Convert.ToString(dr["_Notification"]),
                            IsSeen = dr["_IsSeen"] is DBNull ? false : Convert.ToBoolean(dr["_IsSeen"]),
                            IsActive = dr["_IsActive"] is DBNull ? false : Convert.ToBoolean(dr["_IsActive"]),
                            EntryDate = Convert.ToString(dr["_EntryDate"])
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_WebNotificationReport";
    }
}
