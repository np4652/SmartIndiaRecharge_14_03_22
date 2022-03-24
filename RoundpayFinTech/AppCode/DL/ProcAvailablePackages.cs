using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.Model;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Configuration;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcAvailablePackages : IProcedure
    {
        private readonly IDAL _dal;
        public ProcAvailablePackages(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@UserID",req.CommonInt),
            };
            var Packages = new List<PackageMaster>();
            var Services = new List<ServiceMaster>();
            var resp = new AvailablePackage
            {
                Packages = Packages,
                Services = Services,
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                DataSet ds = _dal.GetByProcedureAdapterDS(GetName(), param);
                DataTable dtPackage = new DataTable();
                DataTable dtService = new DataTable();
                if (ds.Tables.Count > 0)
                {
                    dtPackage = ds.Tables[0];
                    if (dtPackage.Rows.Count > 0 && !dtPackage.Columns.Contains("Msg"))
                    {
                        foreach (DataRow row in dtPackage.Rows)
                        {
                            var package = new PackageMaster
                            {
                                PackageId = row["_PackageID"] is DBNull ? 0 : Convert.ToInt32(row["_PackageID"]),
                                PackageName = row["_Package"] is DBNull ? string.Empty : row["_Package"].ToString(),
                                PackageCost = row["_PackageCost"] is DBNull ? 0 : Convert.ToDecimal(row["_PackageCost"]),
                                Commission = row["_Commission"] is DBNull ? 0 : Convert.ToDecimal(row["_Commission"]),
                                IsDefault = row["_IsDefault"] is DBNull ? false : Convert.ToBoolean(row["_IsDefault"]),
                            };
                            Packages.Add(package);
                        }
                        resp.Packages = Packages;
                    }

                    dtService = ds.Tables.Count > 0 ? ds.Tables[1] : dtService;

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

                    resp.Statuscode = ErrorCodes.One;
                    resp.Msg = "Success";
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
        public string GetName() => "Proc_AvailablePackages";
    }
}