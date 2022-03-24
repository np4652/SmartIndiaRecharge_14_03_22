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
    public class ProcGetEmployee : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetEmployee(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonFilter)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", req.LoginID),
                new SqlParameter("@LT", req.LT),
                new SqlParameter("@Mobile", req.MobileNo??""),
                new SqlParameter("@Name", req.Name??""),
                new SqlParameter("@Email", req.EmailID??""),
                new SqlParameter("@EmpRoleID", req.EmployeeRole),
                new SqlParameter("@SortTypeID", req.SortByID),
                new SqlParameter("@IsDesc", req.IsDesc),
                new SqlParameter("@EmpID", req.UserID),
                new SqlParameter("@TopRows", req.TopRows),
                new SqlParameter("@btnNumber", req.btnID)
            };
            var res = new EmployeeList();
            var employeeList = new List<EList>();
            try
            {
                var ds = _dal.GetByProcedureAdapterDS(GetName(), param);
                if (ds.Tables.Count > 0)
                {
                    var dt = ds.Tables[0];
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            employeeList.Add(new EList
                            {
                                EmpID = Convert.ToInt32(dr["_ID"]),
                                EmpCode = Convert.ToString(dr["_EmpCode"]),
                                Name = Convert.ToString(dr["_Name"]),
                                MobileNo = Convert.ToString(dr["_MobileNo"]),
                                EmailID = Convert.ToString(dr["_EmailID"]),
                                EmpRoleID = Convert.ToInt32(dr["_EmpRoleID"]),
                                EmpRole = Convert.ToString(dr["_EmpRole"]),
                                Prefix = Convert.ToString(dr["_Prefix"]),
                                Address = Convert.ToString(dr["_Address"]),
                                PAN = Convert.ToString(dr["_PAN"]),
                                AADHAR = Convert.ToString(dr["_AADHAR"]),
                                ReferralID = Convert.ToInt32(dr["_ReferalID"]),
                                ReferralBy = Convert.ToString(dr["_ReferralBy"]),
                                ReportingTo = dr["_ReportingTo"] is DBNull ? 0 : Convert.ToInt32(dr["_ReportingTo"]),
                                ReportingToName = Convert.ToString(dr["_ReportingToName"]),
                                IsActive = dr["_IsActive"] is DBNull ? false : Convert.ToBoolean(dr["_IsActive"]),
                                IsOtp = dr["_IsOTP"] is DBNull ? false : Convert.ToBoolean(dr["_IsOTP"])
                            });
                        }
                    }
                    res.Employees = employeeList;
                }
                if (ds.Tables.Count > 1)
                {
                    var pageSetting = new PegeSetting
                    {
                        Count = (int?)ds.Tables[1].Rows[0][0],
                        TopRows = req.TopRows,
                        PageNumber = req.btnID
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
                    LoginTypeID = req.LT,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "Proc_GetEmployee";
    }
}

