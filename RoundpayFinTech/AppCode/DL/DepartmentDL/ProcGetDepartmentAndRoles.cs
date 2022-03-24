using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.DepartmentModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL.DepartmentDL
{
    public class ProcGetDepartmentAndRoles : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetDepartmentAndRoles(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@IsDepartment",req.CommonBool),
                new SqlParameter("@RoleByDeptID",req.CommonBool1),
                new SqlParameter("@ID",req.CommonInt)
            };
            var deptList = new List<Department>();
            var department = new Department();
            var deptRoles = new List<DepartmentRole>();
            var deptRole = new DepartmentRole();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    if (req.CommonBool)
                    {
                        if (req.CommonInt == 0)
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                var dept = new Department
                                {
                                    ID = row["_ID"] is DBNull ? 0 : Convert.ToInt16(row["_ID"]),
                                    Name = row["_Name"] is DBNull ? "" : row["_Name"].ToString(),
                                    Remark = row["_Remark"] is DBNull ? "" : row["_Remark"].ToString(),
                                    IsActive = row["_IsActive"] is DBNull ? false : Convert.ToBoolean(row["_IsActive"])
                                };
                                deptList.Add(dept);
                            }
                        }
                        else
                        {
                            department.ID = dt.Rows[0]["_ID"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_ID"]);
                            department.Name = dt.Rows[0]["_Name"] is DBNull ? "" : dt.Rows[0]["_Name"].ToString();
                            department.Remark = dt.Rows[0]["_Remark"] is DBNull ? "" : dt.Rows[0]["_Remark"].ToString();
                            department.IsActive = dt.Rows[0]["_IsActive"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsActive"]);
                        }
                    }
                    else
                    {
                        if (!req.CommonBool1 && req.CommonInt > 0)
                        {
                            deptRole.ID = dt.Rows[0]["_ID"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_ID"]);
                            deptRole.DepartmentID = dt.Rows[0]["_DepartmentID"] is DBNull ? 0 : Convert.ToInt16(dt.Rows[0]["_DepartmentID"]);
                            deptRole.Name = dt.Rows[0]["_Name"] is DBNull ? "" : dt.Rows[0]["_Name"].ToString();
                            deptRole.Remark = dt.Rows[0]["_Remark"] is DBNull ? "" : dt.Rows[0]["_Remark"].ToString();
                            deptRole.IsActive = dt.Rows[0]["_IsActive"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsActive"]);
                            deptRole.Prefix = dt.Rows[0]["_Prefix"] is DBNull ? "" : dt.Rows[0]["_Prefix"].ToString();
                            deptRole.DepartmentName = dt.Rows[0]["_DepartmentName"] is DBNull ? "" : dt.Rows[0]["_DepartmentName"].ToString();
                            deptRole.ModifyDate = dt.Rows[0]["ModifyDate"] is DBNull ? "" : dt.Rows[0]["ModifyDate"].ToString();
                        }
                        else
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                var _deptRole = new DepartmentRole
                                {
                                    ID = row["_ID"] is DBNull ? 0 : Convert.ToInt16(row["_ID"]),
                                    DepartmentID = row["_DepartmentID"] is DBNull ? 0 : Convert.ToInt16(row["_DepartmentID"]),
                                    Name = row["_Name"] is DBNull ? "" : row["_Name"].ToString(),
                                    Remark = row["_Remark"] is DBNull ? "" : row["_Remark"].ToString(),
                                    IsActive = row["_IsActive"] is DBNull ? false : Convert.ToBoolean(row["_IsActive"]),
                                    Prefix = row["_Prefix"] is DBNull ? "" : row["_Prefix"].ToString(),
                                    DepartmentName = row["_DepartmentName"] is DBNull ? "" : row["_DepartmentName"].ToString(),
                                    ModifyDate = row["ModifyDate"] is DBNull ? "" : row["ModifyDate"].ToString()
                                };
                                deptRoles.Add(_deptRole);
                            }
                        }
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
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            if (req.CommonBool)
            {
                if (req.CommonInt == 0)
                    return deptList;
                else
                    return department;
            }
            else
            {
                if (!req.CommonBool1 && req.CommonInt > 0)
                    return deptRole;
                else
                    return deptRoles;
            }
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "proc_GetDepartmentAndRoles";
        }
    }
}
