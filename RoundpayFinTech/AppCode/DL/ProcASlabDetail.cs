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
    public class ProcASlabDetail : IProcedure
    {
        private readonly IDAL _dal;
        public ProcASlabDetail(IDAL dal) => _dal = dal;

        public object Call(object obj)
        {
            var req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@SlabID",req.CommonInt),
            };

            var DetailList = new List<ASlabDetail>();
            var PDetailList = new List<ASlabDetail>();
            var AfCategories = new List<AffiliateCategory>();
            var vendors = new List<AffiliateVendors>();
            try
            {
                var dt = new DataTable();
                var ds = _dal.GetByProcedureAdapterDS(GetName(), param);

                dt = ds.Tables[0];
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        var data = new ASlabDetail
                        {
                            VendorID = Convert.ToInt32(dr["_VendorID"]),
                            VendorName = Convert.ToString(dr["_VendorName"]),
                            OID = dr["_CategoryID"] is DBNull ? 0 : Convert.ToInt32(dr["_CategoryID"]),
                            AmtType = dr["_AmtType"] is DBNull ? 0 : Convert.ToInt32(dr["_AmtType"]),
                            CommType = dr["_CommType"] is DBNull ? 0 : Convert.ToInt32(dr["_CommType"]),
                            CommAmount = dr["_CommAmount"] is DBNull ? 0 : Convert.ToDecimal(dr["_CommAmount"]),
                            ModifyDate = Convert.ToString(dr["_ModifyDate"])
                        };
                        DetailList.Add(data);
                    }
                }

                dt = ds.Tables[1];
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        var data = new ASlabDetail
                        {
                            VendorID = Convert.ToInt32(dr["_VendorID"]),
                            VendorName = Convert.ToString(dr["_VendorName"]),
                            OID = dr["_CategoryID"] is DBNull ? 0 : Convert.ToInt32(dr["_CategoryID"]),
                            AmtType = dr["_AmtType"] is DBNull ? 0 : Convert.ToInt32(dr["_AmtType"]),
                            CommType = dr["_CommType"] is DBNull ? 0 : Convert.ToInt32(dr["_CommType"]),
                            CommAmount = dr["_CommAmount"] is DBNull ? 0 : Convert.ToDecimal(dr["_CommAmount"]),
                            ModifyDate = Convert.ToString(dr["_ModifyDate"])
                        };
                        PDetailList.Add(data);
                    }
                }

                dt = ds.Tables[2];
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        var data = new AffiliateVendors
                        {
                            Id = Convert.ToInt32(dr["_Id"]),
                            VendorName = Convert.ToString(dr["_VendorName"]),
                        };
                        vendors.Add(data);
                    }
                }

                //dt = ds.Tables[2];
                //if (dt.Rows.Count > 0)
                //{
                //    foreach (DataRow dr in dt.Rows)
                //    {
                //        var data = new AffiliateCategory
                //        {
                //            CategoryId = Convert.ToInt32(dr["_Id"]),
                //            CategoryName = Convert.ToString(dr["_CategoryName"]),
                //        };
                //        AfCategories.Add(data);
                //    }
                //}
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
            var res = new ASlabDetailModel
            {
                SlabID = req.CommonInt,
                RoleID = req.CommonInt2,
                CommissitionDetail = DetailList,
                ParentDetail = PDetailList,
                Vendors = vendors
                //AfCategories = AfCategories
            };
            return res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_ASlabDetail";
    }
}
