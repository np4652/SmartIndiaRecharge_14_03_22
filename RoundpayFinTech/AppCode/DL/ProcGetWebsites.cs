using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetWebsites : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetWebsites(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@LT",req.LoginTypeID),

            };
            var WebsiteList = new List<WebsiteModel>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {

                    foreach (DataRow row in dt.Rows)
                    {
                        var Web = new WebsiteModel
                        {
                            WID = row["_WID"] is DBNull ? 0 : Convert.ToInt16(row["_WID"]),
                            UserID = row["_UserID"] is DBNull ? 0 : Convert.ToInt16(row["_UserID"]),
                            Name = row["_Name"] is DBNull ? "" : row["_Name"].ToString(),
                            DomainName = row["_WebsiteName"] is DBNull ? "" : row["_WebsiteName"].ToString(),
                            MobileNo = row["_MobileNO"] is DBNull ? "" : row["_MobileNO"].ToString(),
                            Prefix = row["_Prefix"] is DBNull ? "" : row["_Prefix"].ToString(),
                            IsActive = row["_IsActive"] is DBNull ? false : Convert.ToBoolean(row["_IsActive"]),
                            AppName = Convert.ToString(row["_AppName"]),
                            AppPackageID = row["_AppPackageID"] is DBNull ? "" : row["_AppPackageID"].ToString(),
                            IsWLAPIAllowed = row["_IsWLAPIAllowed"] is DBNull ? false : Convert.ToBoolean(row["_IsWLAPIAllowed"]),
                            RefferalContent = row["_ReferralContent"] is DBNull ? string.Empty : row["_ReferralContent"].ToString()
                        };
                        WebsiteList.Add(Web);
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

            return WebsiteList;

        }

        public object Call() => throw new NotImplementedException();

        public string GetName() => "Proc_WebsiteList";
    }


}
