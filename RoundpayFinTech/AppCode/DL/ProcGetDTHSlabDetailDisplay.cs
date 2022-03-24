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
    public class ProcGetDTHSlabDetailDisplay : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetDTHSlabDetailDisplay(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@OID",req.CommonInt)
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
                SlabID = req.CommonInt
            };
            try
            {
                DataSet ds = _dal.GetByProcedureAdapterDS(GetName(), param);
                DataTable dt = new DataTable();
                DataTable dtRole = new DataTable();
                if (ds.Tables.Count > 0)
                {
                    dt = ds.Tables[0];
                    dtRole = ds.Tables.Count == 2 ? ds.Tables[1] : dtRole;
                }
                if (dt.Rows.Count > 0 && !dt.Columns.Contains("Msg"))
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        DTHCommissions.Add(new DTHCommission
                        {
                            SlabID = row["_SlabID"] is DBNull ? 0 : Convert.ToInt32(row["_SlabID"]),
                            OpType = row["_OpType"] is DBNull ? "" : row["_OpType"].ToString(),
                            Operator = row["_Operator"] is DBNull ? "" : row["_Operator"].ToString(),
                            PackageID = row["_PackageID"] is DBNull ? 0 : Convert.ToInt32(row["_PackageID"]),
                            PackageName = row["_PackageName"] is DBNull ? "" : row["_PackageName"].ToString(),
                            SPKey = row["_SPKey"] is DBNull ? "" : row["_SPKey"].ToString(),
                            Comm = row["_Comm"] is DBNull ? 0 : Convert.ToDecimal(row["_Comm"]),
                            CommType = row["_CommType"] is DBNull ? 0 : Convert.ToInt32(row["_CommType"]),
                            AmtType = row["_AmtType"] is DBNull ? 0 : Convert.ToInt32(row["_AmtType"]),
                            ModifyDate = row["_ModifyDate"] is DBNull ? "" : row["_ModifyDate"].ToString(),
                            PackageMRP = row["_PackageMRP"] is DBNull ? 0 : Convert.ToInt32(row["_PackageMRP"]),
                            BookingAmount = row["_BookingAmount"] is DBNull ? 0 : Convert.ToInt32(row["_BookingAmount"]),
                            BusinessModel = row["_BusinessModel"] is DBNull ? "" : row["_BusinessModel"].ToString(),
                            RoleID = row["_RoleID"] is DBNull ? 0 : Convert.ToInt32(row["_RoleID"])
                        });
                    }
                    resp.DTHCommissions = DTHCommissions;
                    if (dtRole.Rows.Count > 0)
                    {
                        foreach (DataRow row in dtRole.Rows)
                        {
                            var roleMaster = new RoleMaster
                            {
                                ID = row["_ID"] is DBNull ? 0 : Convert.ToInt16(row["_ID"]),
                                Role = row["_Role"] is DBNull ? "" : row["_Role"].ToString(),
                                Prefix = row["_Prefix"] is DBNull ? "" : row["_Prefix"].ToString()
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
                    ClassName = GetName(),
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return resp;
        }

        public object Call()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "proc_GetDTHSlabDetailDisplay";
        }
    }
}