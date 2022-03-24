using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.DepartmentModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcOperationAssigned : IProcedure
    {
        private readonly IDAL _dal;
        public ProcOperationAssigned(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            SqlParameter[] param = {
                new SqlParameter("@RoleID",(int)obj)
            };
            var res = new List<OperationAssigned>();
            try
            {
                var dt = _dal.GetByProcedure(GetName(),param);
                foreach (DataRow row in dt.Rows)
                {
                    var operationAssigned = new OperationAssigned
                    {
                        MenuID = Convert.ToInt32(row["MenuID"] is DBNull ? 0 : row["MenuID"]),
                        Menu = row["_Menu"] is DBNull ? "" : row["_Menu"].ToString(),
                        OperationID = Convert.ToInt32(row["OperationID"] is DBNull ? 0 : row["OperationID"]),
                        Operation = row["_OperationName"] is DBNull ? "" : row["_OperationName"].ToString(),
                        IsActive = row["IsActive"] is DBNull ? false : Convert.ToBoolean(row["IsActive"])
                    };
                    res.Add(operationAssigned);
                }
            }
            catch (Exception ex)
            {
            }
            return res;

        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "proc_OperationAssigned";
        }
    }

    public class ProcOperationAssignedToCC : IProcedure
    {
        private readonly IDAL _dal;
        public ProcOperationAssignedToCC(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            SqlParameter[] param = {
                new SqlParameter("@RoleID",(int)obj)
            };
            var res = new List<OperationAssigned>();
            try
            {
                var dt = _dal.Get(GetName(), param);
                foreach (DataRow row in dt.Rows)
                {
                    var operationAssigned = new OperationAssigned
                    {
                        Operation = row["_OperationName"] is DBNull ? "" : row["_OperationName"].ToString(),
                        OperationCode = row["_OperationCode"] is DBNull ? "" : row["_OperationCode"].ToString(),
                        IsActive = row["_IsActive"] is DBNull ? false : Convert.ToBoolean(row["_IsActive"])
                    };
                    res.Add(operationAssigned);
                }
            }
            catch (Exception ex)
            {
            }
            return res;

        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "select mo._OperationName,mo._OperationCode, toa._IsActive from tbl_OperationAssigned toa inner join MASTER_OPERATION mo on mo._ID=toa._OperationID where toa._AssignedRoleID=@RoleID order by toa._ID";
        }
    }
}
