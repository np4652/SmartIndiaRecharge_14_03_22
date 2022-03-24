using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.Employee;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetEmployeeByID : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetEmployeeByID(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonFilter)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID", req.LoginID),
                new SqlParameter("@LT", req.LT),
                new SqlParameter("@EmpID", req.UserID)
            };
            var res = new EmpReg();
            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        res.EmpID = Convert.ToInt32(dr["_ID"]);
                        res.EmpCode = Convert.ToString(dr["_EmpCode"]);
                        res.Name = Convert.ToString(dr["_Name"]);
                        res.MobileNo = Convert.ToString(dr["_MobileNo"]);
                        res.EmailID = Convert.ToString(dr["_EmailID"]);
                        res.EmpRoleID = Convert.ToInt32(dr["_EmpRoleID"]);
                        res.ReferralID = Convert.ToInt32(dr["_ReferalID"]);
                        res.ReferralBy = Convert.ToString(dr["_ReferralBy"]);
                        res.ReportingTo = Convert.ToInt32(dr["_ReportingTo"]);
                        res.ReportingToName = Convert.ToString(dr["_ReportingToName"]);
                        res.PinCode = Convert.ToInt32(dr["_PinCode"]);
                        res.Address = Convert.ToString(dr["_Address"]);
                        res.PAN = Convert.ToString(dr["_PAN"]);
                        res.AADHAR = Convert.ToString(dr["_AADHAR"]);
                        res.AADHAR = Convert.ToString(dr["_AADHAR"]);
                        res.IsActive = dr["_IsActive"] is DBNull ? false : Convert.ToBoolean(dr["_IsActive"]);
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
                    LoginTypeID = req.LT,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "Proc_GetEmployeeByID";
    }
}
