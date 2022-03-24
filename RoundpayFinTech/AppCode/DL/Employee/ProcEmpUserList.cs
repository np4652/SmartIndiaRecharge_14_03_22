using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.Employee;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL.Employee
{
    public class ProcEmpUserList : IProcedure
    {
        private readonly IDAL _dal;
        public ProcEmpUserList(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonFilter)obj;
            SqlParameter[] param =
             {
              new SqlParameter("@LoginID", req.LoginID),
                new SqlParameter("@LT", req.LT),
                new SqlParameter("@Mobile", req.MobileNo??""),
                new SqlParameter("@Name", req.Name??""),
                new SqlParameter("@Email", req.EmailID??""),
                new SqlParameter("@RoleID", req.RoleID),
                new SqlParameter("@SortTypeID", req.SortByID),
                new SqlParameter("@IsDesc", req.IsDesc),
                new SqlParameter("@EmpID", req.LoginID),
                new SqlParameter("@TopRows", req.TopRows),
                new SqlParameter("@btnNumber", req.btnID)
            };
            var res = new EmployeeListU();
            var EmpUserslists = new List<EmpUserList>();
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
                            EmpUserslists.Add(new EmpUserList
                            {
                                ID = Convert.ToInt32(row["_ID"]),
                                Role = row["_Role"].ToString(),
                                Name = row["_Name"].ToString(),
                                OutletName = row["_OutletName"].ToString(),
                                MobileNo = row["_MobileNo"].ToString(),
                                Status = Convert.ToBoolean(row["_Status"]),
                                IsOTP = Convert.ToBoolean(row["_IsOTP"]),
                                Slab = row["_Slab"].ToString(),
                                WebsiteName = row["_WebsiteName"].ToString(),
                                JoinDate = row["JoinDate"].ToString(),
                                KYCStatus = row["Kyc_Status"].ToString(),
                                Prefix = row["_Prefix"] is DBNull ? "" : row["_Prefix"].ToString(),
                                RoleID = row["_RoleID"] is DBNull ? 0 : Convert.ToInt16(row["_RoleID"]),
                                IntroID = row["_ReferalID"] is DBNull ? 0 : Convert.ToInt32(row["_ReferalID"]),
                                Balance = row["_Balance"] is DBNull ? 0 : Convert.ToDecimal(row["_Balance"]),
                                BBalance = row["_BBalance"] is DBNull ? 0 : Convert.ToDecimal(row["_BBalance"]),
                                CBalance= row["_CBalance"] is DBNull ? 0 : Convert.ToDecimal(row["_CBalance"]),
                                PacakgeBalance= row["_PackageBalance"] is DBNull ? 0 : Convert.ToDecimal(row["_PackageBalance"]),
                                UBalance= row["_UBalance"] is DBNull ? 0 : Convert.ToDecimal(row["_UBalance"]),
                            });
                        }
                    }
                    res.EmpReports = EmpUserslists;
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LT,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_EmpUserList";
    }
}
