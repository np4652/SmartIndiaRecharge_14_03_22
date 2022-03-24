using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcFOSList : IProcedure
    {
        private readonly IDAL _dal;
        public ProcFOSList(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            var _req = (UserRequest)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@LT", _req.LTID),
                new SqlParameter("@Mobile", _req.MobileNo??""),
                new SqlParameter("@Name", _req.Name??""),
                new SqlParameter("@Email", _req.EmailID??""),
                new SqlParameter("@_RoleID", _req.RoleID),
                new SqlParameter("@SortTypeID", _req.SortByID),
                new SqlParameter("@IsDesc", _req.IsDesc),
                new SqlParameter("@UserID", _req.UserID),
                new SqlParameter("@IsFOSListAdmin", _req.IsFOSListAdmin)
            };
            var userReports = new List<UserReport>();
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        var userReport = new UserReport
                        {
                            ID = Convert.ToInt32(row["_ID"]),
                            Role = row["_Role"].ToString(),
                            OutletName = row["_OutletName"].ToString(),
                            MobileNo = row["_MobileNo"].ToString(),
                            Status = Convert.ToBoolean(row["_Status"]),
                            IsOTP = Convert.ToBoolean(row["_IsOTP"]),
                            Slab = row["_Slab"].ToString(),
                            WebsiteName = row["_WebsiteName"].ToString(),
                            JoinBy = row["JoinBy"].ToString(),
                            JoinDate = row["JoinDate"].ToString(),
                            KYCStatus = row["Kyc_Status"].ToString(),
                            Balance = row["_Balance"] is DBNull ? 0 : Convert.ToDecimal(row["_Balance"]),
                            CommRate = row["_CommRate"] is DBNull ? 0 : Convert.ToDecimal(row["_CommRate"]),
                            Prefix = row["_Prefix"] is DBNull ? "" : row["_Prefix"].ToString(),
                            RoleID = row["_RoleID"] is DBNull ? 0 : Convert.ToInt16(row["_RoleID"]),
                            UBalance = row["_UBalance"] is DBNull ? 0 : Convert.ToDecimal(row["_UBalance"]),
                            BBalance = row["_BBalance"] is DBNull ? 0 : Convert.ToDecimal(row["_BBalance"]),
                            CBalance = row["_CBalance"] is DBNull ? 0 : Convert.ToDecimal(row["_CBalance"]),
                            IDBalnace = row["_IDBalnace"] is DBNull ? 0 : Convert.ToDecimal(row["_IDBalnace"]),
                            OSBalance = row["_OSBalance"] is DBNull ? 0 : Convert.ToDecimal(row["_OSBalance"]),
                            PacakgeBalance = row["_PackageBalance"] is DBNull ? 0 : Convert.ToDecimal(row["_PackageBalance"]),
                            IntroID = row["_ReferalID"] is DBNull ? 0 : Convert.ToInt32(row["_ReferalID"]),
                            BCapping = row["_Capping"] is DBNull ? 0 : Convert.ToDecimal(row["_Capping"]),
                            UCapping = row["_UCapping"] is DBNull ? 0 : Convert.ToDecimal(row["_UCapping"]),
                            BBCapping = row["_BCapping"] is DBNull ? 0 : Convert.ToDecimal(row["_BCapping"]),
                            CCapping = row["_CCapping"] is DBNull ? 0 : Convert.ToDecimal(row["_CCapping"]),
                            IDCapping = row["_ICapping"] is DBNull ? 0 : Convert.ToDecimal(row["_ICapping"]),
                            PackageCapping = row["_PackageCapping"] is DBNull ? 0 : Convert.ToDecimal(row["_PackageCapping"]),
                            FOSId = row["_FOSID"] is DBNull ? 0 : Convert.ToInt32(row["_FOSID"]),
                            FOSName = row["FOSName"] is DBNull ? string.Empty : Convert.ToString(row["FOSName"]),
                            FOSMobile = row["FOSMobile"] is DBNull ? string.Empty : Convert.ToString(row["FOSMobile"]),
                            Name=row["_Name"] is DBNull ? string.Empty : Convert.ToString(row["_Name"]),
                            JoinByMobile= row["JoinByMobile"] is DBNull ? string.Empty : Convert.ToString(row["JoinByMobile"])
                        };
                        userReports.Add(userReport);
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
                    LoginTypeID = _req.LTID,
                    UserId = _req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return userReports;
        }
        public object Call()
        {
            throw new NotImplementedException();
        }
        public string GetName()
        {
            return "proc_FOSList";
        }
    }
}
