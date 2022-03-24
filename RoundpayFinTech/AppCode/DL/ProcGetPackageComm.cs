using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using RoundpayFinTech.AppCode.Configuration;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetPackageComm : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetPackageComm(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
            };
            var PackageDetail = new List<PackageMaster>();
            var ParentPackageDetail = new List<PackageMaster>();
            var Services = new List<ServiceMaster>();
            var CommissionDetail = new List<PkgLevelCommissionReq>();
            var resp = new PackageCommission
            {
                PackageDetail = PackageDetail,
                Services = Services,
                ParentPackageDetail = ParentPackageDetail,
                CommissionDetail = CommissionDetail
            };
            if (!ApplicationSetting.IsPackageAllowed)
                return resp;
            try
            {
                DataSet ds = _dal.GetByProcedureAdapterDS(GetName(), param);
                DataTable dt = new DataTable();
                DataTable dtParent = new DataTable();
                DataTable dtService = new DataTable();
                DataTable dtCommission = new DataTable();
                if (ds.Tables.Count > 0)
                {
                    dt = ds.Tables[0];
                    dtParent = ds.Tables.Count > 1 ? ds.Tables[1] : dtParent;
                    dtService = ds.Tables.Count > 2 ? ds.Tables[2] : dtService;
                    dtCommission = ds.Tables.Count > 3 ? ds.Tables[3] : dtCommission;
                }
                if (dt.Rows.Count > 0 && !dt.Columns.Contains("Msg"))
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        var package = new PackageMaster
                        {
                            PackageId = row["_PackageID"] is DBNull ? 0 : Convert.ToInt32(row["_PackageID"]),
                            PackageName = row["_Package"] is DBNull ? string.Empty : row["_Package"].ToString(),
                            IsDefault = row["_IsDefault"] is DBNull ? false : Convert.ToBoolean(row["_IsDefault"]),
                            IsCurrent = row["_IsCurrent"] is DBNull ? false : Convert.ToBoolean(row["_IsCurrent"])
                        };
                        PackageDetail.Add(package);
                    }
                    resp.PackageDetail = PackageDetail;
                }
                if (dtParent.Rows.Count > 0 && !dtParent.Columns.Contains("Msg"))
                {
                    foreach (DataRow row in dtParent.Rows)
                    {
                        var packageMaster = new PackageMaster
                        {
                            PackageId = row["_PackageID"] is DBNull ? 0 : Convert.ToInt32(row["_PackageID"]),
                            Commission = row["_Commission"] is DBNull ? 0 : Convert.ToDecimal(row["_Commission"]),
                            PackageCost = row["_PackageCost"] is DBNull ? 0 : Convert.ToDecimal(row["_PackageCost"]),
                        };
                        ParentPackageDetail.Add(packageMaster);
                    }
                    resp.ParentPackageDetail = ParentPackageDetail;
                }
                if (dtService.Rows.Count > 0 && !dtService.Columns.Contains("Msg"))
                {
                    foreach (DataRow row in dtService.Rows)
                    {
                        var serviceMaster = new ServiceMaster
                        {
                            ServiceName = row["_Name"] is DBNull ? string.Empty : Convert.ToString(row["_Name"]),
                            PackageId = row["_PackageID"] is DBNull ? 0 : Convert.ToInt32(row["_PackageID"]),
                        };
                        Services.Add(serviceMaster);
                    }
                    resp.Services = Services;
                }
                if (dtCommission.Rows.Count > 0 && !dtCommission.Columns.Contains("Msg"))
                {
                    foreach (DataRow row in dtCommission.Rows)
                    {
                        var commDetail = new PkgLevelCommissionReq
                        {
                            Commission = row["_Commission"] is DBNull ? 0 : Convert.ToDecimal(row["_Commission"]),
                            ModifyDate = row["_ModifyDate"] is DBNull ? string.Empty : Convert.ToString(row["_ModifyDate"]),
                            PackageId = row["_PackageID"] is DBNull ? 0 : Convert.ToInt32(row["_PackageID"])
                        };
                        CommissionDetail.Add(commDetail);
                    }
                    resp.CommissionDetail = CommissionDetail;
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
        public string GetName() => "Proc_GetPackageComm";
    }
}