using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.Employee;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL.Employee
{
    public class ProcTodayOutletsListForEmp : IProcedure
    {
        private readonly IDAL _dal;
        public ProcTodayOutletsListForEmp(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var LoginID = (int)obj;
            SqlParameter[] param =
             {
              new SqlParameter("@LoginID", LoginID)
            };            
            var Userslists = new List<EmpUserList>();
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        Userslists.Add(new EmpUserList
                        {
                            ID = Convert.ToInt32(row["_ID"]),
                            Role = row["_Role"].ToString(),
                            Name = row["_Name"].ToString(),
                            OutletName = row["_OutletName"].ToString(),
                            MobileNo = row["_MobileNo"].ToString(),
                            //Status = Convert.ToBoolean(row["_Status"]),
                            //IsOTP = Convert.ToBoolean(row["_IsOTP"]),
                            //Slab = row["_Slab"].ToString(),
                            //WebsiteName = row["_WebsiteName"].ToString(),
                            //JoinDate = row["JoinDate"].ToString(),
                            //KYCStatus = row["Kyc_Status"].ToString(),
                            //Prefix = row["_Prefix"] is DBNull ? "" : row["_Prefix"].ToString(),
                            //RoleID = row["_RoleID"] is DBNull ? 0 : Convert.ToInt16(row["_RoleID"]),
                            //IntroID = row["_ReferalID"] is DBNull ? 0 : Convert.ToInt32(row["_ReferalID"]),
                            //Balance = row["_Balance"] is DBNull ? 0 : Convert.ToDecimal(row["_Balance"]),
                            //BBalance = row["_BBalance"] is DBNull ? 0 : Convert.ToDecimal(row["_BBalance"]),
                            //CBalance = row["_CBalance"] is DBNull ? 0 : Convert.ToDecimal(row["_CBalance"]),
                            //PacakgeBalance = row["_PackageBalance"] is DBNull ? 0 : Convert.ToDecimal(row["_PackageBalance"]),
                            //UBalance = row["_UBalance"] is DBNull ? 0 : Convert.ToDecimal(row["_UBalance"]),
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
                    LoginTypeID = 0,
                    UserId = LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return Userslists;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "Proc_TodayOutletsListForEmp";
    }
}
