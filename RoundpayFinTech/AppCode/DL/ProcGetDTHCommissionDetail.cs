using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetDTHCommissionDetail : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetDTHCommissionDetail(IDAL dal)
        {
            _dal = dal;
        }
        public object Call(object obj)
        {
            CommonReq req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@SlabID",req.CommonInt),
                new SqlParameter("@OID",req.CommonInt2)
            };
            List<DTHCommission> DTHCommissions = new List<DTHCommission>();
            List<RoleMaster> Roles = new List<RoleMaster>();
            List<OperatorDetail> Operators = new List<OperatorDetail>();
            DTHCommissionModel resp = new DTHCommissionModel
            {
                IsAdminDefined = false,
                DTHCommissions = DTHCommissions,
                ParentSlabDetails = DTHCommissions,
                Roles = Roles,
                Operators = Operators,
                SlabID = req.CommonInt
            };
            try
            {
                DataSet ds = _dal.GetByProcedureAdapterDS(GetName(), param);
                DataTable dt = new DataTable();
                DataTable dtOperator = new DataTable();
                DataTable dtParent = new DataTable();
                DataTable dtRole = new DataTable();
                if (ds.Tables.Count > 0)
                {
                    dt = ds.Tables[0];
                    if (req.IsListType)
                    {
                        dtOperator = ds.Tables.Count >= 2 ? ds.Tables[1] : dtOperator;
                        dtRole = ds.Tables.Count >= 3 ? ds.Tables[2] : dtRole;
                    }
                    else
                    {
                        dtOperator = ds.Tables.Count >= 2 ? ds.Tables[2] : dtOperator;
                        dtParent = ds.Tables.Count > 2 ? ds.Tables[1] : dtParent;
                    }
                }
                if (dt.Rows.Count > 0 && !dt.Columns.Contains("Msg"))
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        var _Commission = new DTHCommission()
                        {
                            OID = Convert.ToInt32(row["_OID"]),
                            Operator = Convert.ToString(row["_Operator"]),
                            PackageID = Convert.ToInt32(row["_PackageID"]),
                            PackageName = Convert.ToString(row["_PackageName"]),
                            PackageMRP=Convert.ToInt32(row["_PackageMRP"]),
                            CommissionID = row["_CommissionID"] is DBNull ? 0 : Convert.ToInt32(row["_CommissionID"]),
                            SlabID = row["_SlabID"] is DBNull ? 0 : Convert.ToInt32(row["_SlabID"]),
                            Comm = row["_Commission"] is DBNull ? 0 : Convert.ToDecimal(row["_Commission"]),
                            CommType = row["_CommType"] is DBNull ? 0 : Convert.ToInt32(row["_CommType"]),
                            AmtType = row["_AmtType"] is DBNull ? 0 : Convert.ToInt32(row["_AmtType"]),
                            ModifyDate = Convert.ToString(row["_ModifyDate"]),
                        };
                        if (dt.Columns.Contains("_RoleID"))
                        {
                            _Commission.RoleID = row["_RoleID"] is DBNull ? 0 : Convert.ToInt32(row["_RoleID"]);
                            if (!resp.IsAdminDefined)
                                resp.IsAdminDefined = !resp.IsAdminDefined;
                        }
                        DTHCommissions.Add(_Commission);
                    }
                    resp.DTHCommissions = DTHCommissions;
                }

                if (dtOperator!=null && dtOperator.Rows.Count > 0 )
                {
                    foreach (DataRow row in dtOperator.Rows)
                    {
                        var _Commission = new OperatorDetail()
                        {
                            OID = Convert.ToInt32(row["_ID"]),
                            Operator = Convert.ToString(row["_Name"]),
                        };
                        Operators.Add(_Commission);
                    }
                    resp.Operators = Operators;
                }

                if (dtParent.Rows.Count > 0 && !dtParent.Columns.Contains("Msg"))
                {
                    DTHCommissions = new List<DTHCommission>();
                    if (dtParent.Columns.Contains("_SlabID"))
                    {
                        foreach (DataRow row in dtParent.Rows)
                        {
                            var _Comm = new DTHCommission
                            {
                                OID = Convert.ToInt32(row["_OID"]),
                                Operator = Convert.ToString(row["_Operator"]),
                                PackageID = Convert.ToInt32(row["_PackageID"]),
                                PackageName = Convert.ToString(row["_PackageName"]),
                                PackageMRP = Convert.ToInt32(row["_PackageMRP"]),
                                CommissionID = row["_CommissionID"] is DBNull ? 0 : Convert.ToInt32(row["_CommissionID"]),
                                SlabID = row["_SlabID"] is DBNull ? 0 : Convert.ToInt32(row["_SlabID"]),
                                Comm = row["_Commission"] is DBNull ? 0 : Convert.ToDecimal(row["_Commission"]),
                                CommType = row["_CommType"] is DBNull ? 0 : Convert.ToInt32(row["_CommType"]),
                                AmtType = row["_AmtType"] is DBNull ? 0 : Convert.ToInt32(row["_AmtType"]),
                                ModifyDate = Convert.ToString(row["_ModifyDate"]),
                            };
                            if (dt.Columns.Contains("_RoleID"))
                            {
                                _Comm.RoleID = row["_RoleID"] is DBNull ? 0 : Convert.ToInt32(row["_RoleID"]);
                            }
                            DTHCommissions.Add(_Comm);
                        }
                    }
                    resp.ParentSlabDetails = DTHCommissions;
                }
                if (resp.IsAdminDefined)
                {
                    if (dtRole.Rows.Count > 0)
                    {
                        foreach (DataRow row in dtRole.Rows)
                        {
                            var roleMaster = new RoleMaster
                            {
                                ID = row["_ID"] is DBNull ? 0 : Convert.ToInt16(row["_ID"]),
                                Role = row["_Role"] is DBNull ? "" : row["_Role"].ToString(),
                                Prefix = row["_Prefix"] is DBNull ? "" : row["_Prefix"].ToString(),
                            };
                            Roles.Add(roleMaster);
                        }
                        resp.Roles = Roles;
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
            return resp;
        }

        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_GetDTHCommissionDetail";
    }
}
