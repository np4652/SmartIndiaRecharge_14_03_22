using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcUserListFOS : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUserListFOS(IDAL dal)
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
                new SqlParameter("@UserID", _req.UserID)
            };
            var res = new UserList();
            var userReports = new List<UserReport>();
            try
            {
                var ds = _dal.GetByProcedureAdapterDS(GetName(), param);
                if (ds.Tables.Count > 0)
                {
                    var dt = ds.Tables[0];
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
                                IsVirtual = row["_IsVirtual"] is DBNull ? false : Convert.ToBoolean(row["_IsVirtual"])
                            };
                            userReports.Add(userReport);
                        }
                    }
                    res.userReports = userReports;
                }
                if (ds.Tables.Count > 1)
                {
                    var pageSetting = new PegeSetting
                    {
                        Count = (int?)ds.Tables[1].Rows[0][0],
                        TopRows = _req.TopRows,
                        PageNumber = _req.btnID
                    };
                    res.PegeSetting = pageSetting;
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
            return res;
        }
        public object Call()
        {
            throw new NotImplementedException();
        }
        public string GetName()
        {
            return "proc_UserList_FOS";
        }
    }
}
