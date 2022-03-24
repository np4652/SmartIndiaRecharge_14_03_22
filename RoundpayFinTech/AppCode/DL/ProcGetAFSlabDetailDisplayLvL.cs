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
    public class ProcGetAFSlabDetailDisplayLvL : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetAFSlabDetailDisplayLvL(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@OID",req.CommonInt)
            };
            List<SlabCommission> slabCommissions = new List<SlabCommission>();
            List<RoleMaster> Roles = new List<RoleMaster>();
            List<AffiliateVendors> Vendors = new List<AffiliateVendors>();
            SlabDetailModel resp = new SlabDetailModel
            {
                IsAdminDefined = false,
                SlabDetails = slabCommissions,
                ParentSlabDetails = slabCommissions,
                Roles = Roles,
                SlabID = req.CommonInt
            };
            try
            {
                DataSet ds = _dal.GetByProcedureAdapterDS(GetName(), param);
                DataTable dt = new DataTable();
                DataTable dtRole = new DataTable();
                DataTable dtVendors = new DataTable();
                if (ds.Tables.Count > 0)
                {
                    
                    dt = ds.Tables[0];
                    dtVendors = ds.Tables.Count > 1 ? ds.Tables[1] : dtVendors;
                    dtRole = ds.Tables.Count > 2 ? ds.Tables[2] : dtRole;
                    
                }
                if (dt.Rows.Count > 0 && !dt.Columns.Contains("Msg"))
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        slabCommissions.Add(new SlabCommission
                        {
                            SlabID = row["SlabID"] is DBNull ? 0 : Convert.ToInt32(row["SlabID"]),
                            Comm = row["Commission"] is DBNull ? 0 : Convert.ToDecimal(row["Commission"]),                           
                            AmtType = row["AmtType"] is DBNull ? 0 : Convert.ToInt32(row["AmtType"]),
                            ModifyDate = row["ModifyDate"] is DBNull ? "" : row["ModifyDate"].ToString(),
                            RoleID = row["_RoleID"] is DBNull ? 0 : Convert.ToInt32(row["_RoleID"]),
                            VID = row["_vendorId"] is DBNull ? 0 : Convert.ToInt32(row["_vendorId"]),
                            VName = row["_VendorName"] is DBNull ? "" : Convert.ToString(row["_VendorName"]),
                        });
                    }
                    resp.SlabDetails = slabCommissions;
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

                    if (dtVendors.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dtVendors.Rows)
                        {
                            var vendor = new AffiliateVendors
                            {
                                Id = Convert.ToInt32(dr["_id"]),
                                VendorName = Convert.ToString(dr["_VendorName"])
                            };
                            Vendors.Add(vendor);
                        }
                        resp.AfVendors = Vendors;
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

        public object Call() => throw new NotImplementedException();


        public string GetName() => "proc_GetAFSlabDetailDisplayLvL";
    }
}
