using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.DepartmentModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcCustomerCares : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcCustomerCares(IDAL dal)
        {
            _dal = dal;
        }
        public async Task<object> Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@CCID",req.CommonInt),
                new SqlParameter("@MobileNo",req.CommonStr??""),
                new SqlParameter("@RoleID",req.CommonInt2),
                new SqlParameter("@DepartmentID",req.CommonInt3)
            };
            var res = new List<Customercare>();
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        var cc = new Customercare
                        {
                            ID = row["_ID"] is DBNull ? 0 : Convert.ToInt32(row["_ID"]),
                            DeptID = row["_DepartmentID"] is DBNull ? 0 : Convert.ToInt32(row["_DepartmentID"]),
                            RoleID = row["_DepartmentRoleID"] is DBNull ? 0 : Convert.ToInt32(row["_DepartmentRoleID"]),
                            Role = row["RoleName"] is DBNull ? "" : row["RoleName"].ToString(),
                            Department = row["DepartmentName"] is DBNull ? "" : row["DepartmentName"].ToString(),
                            Name = row["_Name"] is DBNull ? "" : row["_Name"].ToString(),
                            MobileNo = row["_MobileNo"] is DBNull ? "" : row["_MobileNo"].ToString(),
                            EmailID = row["_EmailID"] is DBNull ? "" : row["_EmailID"].ToString(),
                            Address = row["_Address"] is DBNull ? "" : row["_Address"].ToString(),
                            Pincode = row["_PinCode"] is DBNull ? "" : row["_PinCode"].ToString(),
                            IsActive = row["_IsActive"] is DBNull ? false : Convert.ToBoolean(row["_IsActive"]),
                            IsOTP = row["_IsOTP"] is DBNull ? false : Convert.ToBoolean(row["_IsOTP"]),
                            Prefix = row["_Prefix"] is DBNull ? "" : row["_Prefix"].ToString(),
                            EntryDate = row["_EntryDate"] is DBNull ? "" : row["_EntryDate"].ToString(),
                            ModifyDate = row["_ModifyDate"] is DBNull ? "" : row["_ModifyDate"].ToString()
                        };
                        res.Add(cc);
                    }
                }
            }
            catch (Exception ex)
            {


            }
            return res;
        }

        public Task<object> Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "proc_CustomerCares";
        }
    }

    public class ProcCustomerCareByID : IProcedureAsync
    {
        private readonly IDAL _dal;
        public ProcCustomerCareByID(IDAL dal)
        {
            _dal = dal;
        }
        public async Task<object> Call(object obj)
        {
            SqlParameter[] param = {
                new SqlParameter("@ID",(int)obj)
            };
            var res = new Customercare();
            try
            {
                var dt = await _dal.GetByProcedureAsync(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.ID = dt.Rows[0]["_ID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_ID"]);
                    res.DeptID = dt.Rows[0]["_DepartmentID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_DepartmentID"]);
                    res.RoleID = dt.Rows[0]["_DepartmentRoleID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_DepartmentRoleID"]);
                    res.Name = dt.Rows[0]["_Name"] is DBNull ? "" : dt.Rows[0]["_Name"].ToString();
                    res.MobileNo = dt.Rows[0]["_MobileNo"] is DBNull ? "" : dt.Rows[0]["_MobileNo"].ToString();
                    res.EmailID = dt.Rows[0]["_EmailID"] is DBNull ? "" : dt.Rows[0]["_EmailID"].ToString();
                    res.Address = dt.Rows[0]["_Address"] is DBNull ? "" : dt.Rows[0]["_Address"].ToString();
                    res.Pincode = dt.Rows[0]["_PinCode"] is DBNull ? "" : dt.Rows[0]["_PinCode"].ToString();
                    res.CityID = dt.Rows[0]["_CityID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_CityID"]);
                    res.StateID = dt.Rows[0]["_StateID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_StateID"]);
                }
            }
            catch (Exception ex)
            {


            }
            return res;
        }

        public Task<object> Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "proc_CustomerCareByID";
        }
    }
}
