using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcUserList : IProcedure
    {
        private readonly IDAL _dal;
        public ProcUserList(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var _req = (UserRequest)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", _req.LoginID),
                new SqlParameter("@LT", _req.LTID),
                new SqlParameter("@Mobile", _req.MobileNo??string.Empty),
                new SqlParameter("@Name", _req.Name??string.Empty),
                new SqlParameter("@Email", _req.EmailID??string.Empty),
                new SqlParameter("@_RoleID", _req.RoleID),
                new SqlParameter("@SortTypeID", _req.SortByID),
                new SqlParameter("@IsDesc", _req.IsDesc),
                new SqlParameter("@UserID", _req.UserID),
                new SqlParameter("@TopRows", _req.TopRows),
                new SqlParameter("@btnNumber", _req.btnID),
                new SqlParameter("@slabName", _req.SlabName??string.Empty)
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
                                Role = Convert.ToString(row["_Role"]),
                                OutletName = Convert.ToString(row["_OutletName"]),
                                MobileNo = Convert.ToString(row["_MobileNo"]),
                                Status = row["_Status"] is DBNull ? false : Convert.ToBoolean(row["_Status"]),
                                IsOTP = row["_IsOTP"] is DBNull ? false : Convert.ToBoolean(row["_IsOTP"]),
                                Slab = Convert.ToString(row["_Slab"]),
                                EmpID = row["_EmpID"] is DBNull ? 0 : Convert.ToInt32(row["_EmpID"]),
                                WebsiteName = Convert.ToString(row["_WebsiteName"]),
                                JoinBy = Convert.ToString(row["JoinBy"]),
                                JoinDate = Convert.ToString(row["JoinDate"]),
                                KYCStatus = Convert.ToString(row["Kyc_Status"]),
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
                                IsVirtual = row["_IsVirtual"] is DBNull ? false : Convert.ToBoolean(row["_IsVirtual"]),
                                IsFlatCommission = row["_IsFlatCommission"] is DBNull ? false : Convert.ToBoolean(row["_IsFlatCommission"]),
                                IsAutoBilling = row["_IsAutoBilling"] is DBNull ? false : Convert.ToBoolean(row["_IsAutoBilling"]),
                                InvoiceByAdmin = row["_InvoiceByAdmin"] is DBNull ? false : Convert.ToBoolean(row["_InvoiceByAdmin"]),
                                IsMarkedGreen = row["_IsMarkedGreen"] is DBNull ? false : Convert.ToBoolean(row["_IsMarkedGreen"]),
                                IsCalculateCommissionFromCircle = row["_IsCalculateCommissionFromCircle"] is DBNull ? false : Convert.ToBoolean(row["_IsCalculateCommissionFromCircle"]),
                                UserArea = row["_Area"] is DBNull ? string.Empty : Convert.ToString(row["_Area"]),
                                IsPaymentGateway = row["_IsPaymentGateway"] is DBNull ? false : Convert.ToBoolean(row["_IsPaymentGateway"]),
                                IsDownLinePG = row["_IsDownLinePG"] is DBNull ? false : Convert.ToBoolean(row["_IsDownLinePG"]),
                                Candebit = row["_Candebit"] is DBNull ? false : Convert.ToBoolean(row["_Candebit"]),
                                CandebitDownline = row["_CandebitDownline"] is DBNull ? false : Convert.ToBoolean(row["_CandebitDownline"]),
                                IsGoogle2FAEnable = row["_Is_Google_2FA_Enable"] is DBNull ? false : Convert.ToBoolean(row["_Is_Google_2FA_Enable"]),
                                AccountSecretKey = row["_AccountSecretKey"] is DBNull ? string.Empty : Convert.ToString(row["_AccountSecretKey"]),
                                introducerName = row["_introducerName"] is DBNull ? string.Empty : Convert.ToString(row["_introducerName"]),
                                introducerMobile = row["_introducerMobile"] is DBNull ? string.Empty : Convert.ToString(row["_introducerMobile"])
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
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_UserList";
    }
}
