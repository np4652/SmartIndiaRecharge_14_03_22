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
    public class ProcGetVASPackageDetails : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetVASPackageDetails(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            var res = new List<PackageDetail>();
            SqlParameter[] param = {
                new SqlParameter("@ServiceTypeID",req.CommonInt)
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(),param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        PackageDetail packageDetail = new PackageDetail
                        {
                            ID = row["_VASPackageID"] is DBNull ? 0 : Convert.ToInt32(row["_VASPackageID"]),
                            ServiceID = row["ServiceID"] is DBNull ? 0 : Convert.ToInt32(row["ServiceID"]),
                            ServiceName = row["ServiceName"] is DBNull ? "" : row["ServiceName"].ToString(),
                            ModifyDate = row["_ModifyDate"] is DBNull ? "" : row["_ModifyDate"].ToString(),
                            IsActive = row["IsChargable"] is DBNull ? false : Convert.ToBoolean(row["IsChargable"]),
                            IsServiceActive = row["_IsServiceActive"] is DBNull ? false : Convert.ToBoolean(row["_IsServiceActive"]),
                            Charge = row["Charge"] is DBNull ? 0 : Convert.ToDecimal(row["Charge"]),

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

        public object Call() => throw new NotImplementedException();

        public string GetName() => "proc_GetVASPackageDetails";
    }
}
