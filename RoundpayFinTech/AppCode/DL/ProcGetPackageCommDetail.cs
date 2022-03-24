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
    public class ProcGetPackageCommDetail : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetPackageCommDetail(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            CommonReq req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@PackageId",req.CommonInt),
                new SqlParameter("@ActionType",req.CommonInt2),
            };
            var packageMaster = new PackageMaster();
            try
            {
                DataSet ds = _dal.GetByProcedureAdapterDS(GetName(), param);
                DataTable dt = new DataTable();
                DataTable dtParent = new DataTable();
                DataTable dtRole = new DataTable();
                if (ds.Tables.Count > 0)
                {
                    dt = ds.Tables[0];
                    dtRole = ds.Tables.Count == 2 ? ds.Tables[1] : dtRole;
                }
                if (dt.Rows.Count > 0 && !dt.Columns.Contains("Msg"))
                {
                    packageMaster.PackageId = dt.Rows[0]["_PackageId"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_PackageId"]);
                    packageMaster.PackageName = dt.Rows[0]["_Package"] is DBNull ? string.Empty : dt.Rows[0]["_Package"].ToString();
                    packageMaster.RoleDetail = new List<PkgLevelCommission>();
                    foreach (DataRow dr in dtRole.Rows)
                    {
                        var pkglvlComm = new PkgLevelCommission
                        {
                            Role= dr["_Role"] is DBNull ? string.Empty : dr["_Role"].ToString(),
                            RoleId= dr["_ID"] is DBNull ? 0 : Convert.ToInt32(dr["_ID"]),
                            Commission = dr["_Commission"] is DBNull ? 0 : Convert.ToDecimal(dr["_Commission"]),
                            CommType = dr["_CommType"] is DBNull ? false : Convert.ToBoolean(dr["_CommType"]),
                            ModifyDate = dr["ModifyDate"] is DBNull ? string.Empty : Convert.ToString(dr["ModifyDate"])
                        };
                        packageMaster.RoleDetail.Add(pkglvlComm);
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
            return packageMaster;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "Proc_GetPackageCommAdmin";
        }
    }
}
