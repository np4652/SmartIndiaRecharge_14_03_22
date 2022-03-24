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
    public class ProcGetAFSlabDetailDisplay : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetAFSlabDetailDisplay(IDAL dal) => _dal = dal;
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
                DataTable dtVendors = new DataTable();
                if (ds.Tables.Count > 0)
                {
                    dt = ds.Tables[0];
                }
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        slabCommissions.Add(new SlabCommission
                        {
                            SlabID = row["SlabID"] is DBNull ? 0 : Convert.ToInt32(row["SlabID"]),
                            Comm = row["Commission"] is DBNull ? 0 : Convert.ToDecimal(row["Commission"]),
                            AmtType = row["AmtType"] is DBNull ? 0 : Convert.ToInt32(row["AmtType"]),
                            ModifyDate = row["ModifyDate"] is DBNull ? "" : row["ModifyDate"].ToString(),
                            //RoleID = row["_RoleID"] is DBNull ? 0 : Convert.ToInt32(row["_RoleID"]),
                            VID = row["_vendorId"] is DBNull ? 0 : Convert.ToInt32(row["_vendorId"]),
                            VName = row["_VendorName"] is DBNull ? "" : Convert.ToString(row["_VendorName"]),
                        });
                    }

                    resp.SlabDetails = slabCommissions;
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