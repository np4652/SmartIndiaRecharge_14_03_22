using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;

namespace RoundpayFinTech.AppCode.DL
{
    public class ProcGetPackageDetails : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetPackageDetails(IDAL dal) => _dal = dal;
        public object Call(object obj) => throw new NotImplementedException();
        public object Call()
        {
            var res = new List<PackageDetail>();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName());
                if (dt.Rows.Count > 0) {
                    foreach (DataRow row in dt.Rows)
                    {
                        PackageDetail packageDetail = new PackageDetail {
                            ID=row["_PackageID"] is DBNull?0:Convert.ToInt32(row["_PackageID"]),
                            ServiceID = row["ServiceID"] is DBNull ? 0 : Convert.ToInt32(row["ServiceID"]),
                            ServiceName = row["ServiceName"] is DBNull ? "" : row["ServiceName"].ToString(),
                            ModifyDate = row["_ModifyDate"] is DBNull ? "" : row["_ModifyDate"].ToString(),
                            IsActive= row["_IsActive"] is DBNull ? false: Convert.ToBoolean(row["_IsActive"]),
                            IsServiceActive = row["_IsServiceActive"] is DBNull ? false: Convert.ToBoolean(row["_IsServiceActive"])
                        };
                        res.Add(packageDetail);
                    }
                }
            }
            catch (Exception)
            {
            }
            return res;
        }

        public string GetName() => "proc_GetPackageDetails";
    }
}
