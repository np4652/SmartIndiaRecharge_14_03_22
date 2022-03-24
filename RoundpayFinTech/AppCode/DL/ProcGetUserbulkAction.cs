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
    public class ProcGetUserbulkAction : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetUserbulkAction(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@LT", _req.LoginTypeID),
                new SqlParameter("@ReffID", _req.CommonInt),
                new SqlParameter("@IsWhole", _req.CommonBool),
                new SqlParameter("@RoleID", _req.CommonInt2),
                new SqlParameter("@IsSelf",_req.CommonBool1)
            };
            var userReports = new List<UserReportBulk>();
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        var userReport = new UserReportBulk
                        {
                            ID = Convert.ToInt32(row["_ID"]),
                            Role = row["_Role"].ToString(),
                            OutletName = row["_OutletName"].ToString(),
                            MobileNo = row["_MobileNo"].ToString(),
                            WhatsAppNumber = Convert.ToString(row["_WhatsappNo"]),   
                            EMail = Convert.ToString(row["_EMailID"]),
                            EmailVerifiedStatus = Convert.ToString(row["_EmailVerifiedStatus"]),
                            IsEmailVerified = row["_IsEmailVerified"] is DBNull ? false : Convert.ToBoolean(row["_IsEmailVerified"]),
                            RentalStatus = Convert.ToString(row["_RentalType"]),
                            PackageName= Convert.ToString(row["_PackageName"]),
                            Status = Convert.ToBoolean(row["_Status"]),
                            IsOTP = Convert.ToBoolean(row["_IsOTP"]),
                            Slab = row["_Slab"].ToString(),
                            WebsiteName = row["_WebsiteName"].ToString(),
                            JoinBy = row["JoinBy"].ToString(),
                            JoinDate = row["JoinDate"].ToString(),
                            KYCStatus = row["Kyc_Status"].ToString(),
                            Balance = row["_Balance"] is DBNull ? 0 : Convert.ToDecimal(row["_Balance"]),
                            OSBalance = row["_OSBalance"] is DBNull ? 0 : Convert.ToDecimal(row["_OSBalance"]),
                            UBalance = row["_UBalance"] is DBNull ? 0 : Convert.ToDecimal(row["_UBalance"]),
                            BBalance = row["_BBalance"] is DBNull ? 0 : Convert.ToDecimal(row["_BBalance"]),
                            PacakgeBalance = row["_PackageBalance"] is DBNull ? 0 : Convert.ToDecimal(row["_PackageBalance"]),
                            CommRate = row["_CommRate"] is DBNull ? 0 : Convert.ToDecimal(row["_CommRate"]),
                            Prefix = row["_Prefix"] is DBNull ? "" : row["_Prefix"].ToString(),
                            Capping = row["_BCapping"] is DBNull ? 0 : Convert.ToDecimal(row["_BCapping"]),
                            ReferalID = Convert.ToInt32(row["_ReferalID"]),
                            RoleID = Convert.ToInt32(row["_RoleID"]),
                            IsAutoBilling = row["_IsAutoBilling"] is DBNull ? false : Convert.ToBoolean(row["_IsAutoBilling"]),
                            MaxBillingCountAB = row["_MaxBillingPerDay"] is DBNull ? 0 : Convert.ToInt32(row["_MaxBillingPerDay"]),
                            BalanceForAB = row["_BalanceForAutoBilling"] is DBNull ? 0 : Convert.ToInt32(row["_BalanceForAutoBilling"]),
                            AlertBalance = row["_AlertBalance"] is DBNull ? 0 : Convert.ToInt32(row["_AlertBalance"]),
                            FromFOSAB = row["_IsAutoBillingFromFOS"] is DBNull ? false : Convert.ToBoolean(row["_IsAutoBillingFromFOS"]),
                            MaxCreditLimitAB = row["_MaxCreditLimitAB"] is DBNull ? 0 : Convert.ToInt32(row["_MaxCreditLimitAB"]),
                            MaxTransferLimitAB = row["_MaxTransferLimitAB"] is DBNull ? 0 : Convert.ToInt32(row["_MaxTransferLimitAB"])
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
                    LoginTypeID = _req.LoginTypeID,
                    UserId = _req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return userReports;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetUserbulkAction";
    }
}
