using Fintech.AppCode.DB;
using Fintech.AppCode.Interfaces;
using Fintech.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.Shopping;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace RoundpayFinTech.AppCode.DL.Shopping
{
    public class ProcGetTrendingProducts : IProcedure
    {
        private readonly IDAL _dal;

        public ProcGetTrendingProducts(IDAL dal) => _dal = dal;

        public object Call(object obj) => throw new NotImplementedException();
        public object Call()
        {
            var res = new List<ProductDetail>();
            try
            {
                DataTable dt = _dal.Get(GetName());
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        var data = new ProductDetail
                        {
                            ProductName = Convert.ToString(dr["_ProductName"]),
                            CategoryID = dr["_CategoryID"] is DBNull ? 0 : Convert.ToInt32(dr["_CategoryID"]),
                            SubCategoryID1 = dr["_SubCategoryID1"] is DBNull ? 0 : Convert.ToInt32(dr["_SubCategoryID1"]),
                            SubCategoryID2 = dr["_SubCategoryID2"] is DBNull ? 0 : Convert.ToInt32(dr["_SubCategoryID2"]),
                            ProductDetailID = dr["_ID"] is DBNull ? 0 : Convert.ToInt32(dr["_ID"]),
                            ProductID = dr["_ProductID"] is DBNull ? 0 : Convert.ToInt32(dr["_ProductID"]),
                            ProductCode = Convert.ToString(dr["_ProductCode"]),
                            VendorID = dr["_VendorID"] is DBNull ? 0 : Convert.ToInt32(dr["_VendorID"]),
                            BrandID = dr["_BrandID"] is DBNull ? 0 : Convert.ToInt32(dr["_BrandID"]),
                            //Quantity = dr["_Quantity"] is DBNull ? 0 : Convert.ToInt32(dr["_Quantity"]),
                            Batch = Convert.ToString(dr["_Batch"]),
                            MRP = dr["_MRP"] is DBNull ? 0 : Convert.ToDecimal(dr["_MRP"]),
                            Discount = dr["_Discount"] is DBNull ? 0 : Convert.ToDecimal(dr["_Discount"]),
                            DiscountType = dr["_DiscountType"] is DBNull ? false : Convert.ToBoolean(dr["_DiscountType"]),
                            Commission = dr["_Commission"] is DBNull ? 0 : Convert.ToDecimal(dr["_Commission"]),
                            CommissionType = dr["_CommissionType"] is DBNull ? false : Convert.ToBoolean(dr["_Commission"]),
                            DetailDescription = Convert.ToString(dr["_Description"]),
                            SellingPrice = dr["_SellingPrice"] is DBNull ? 0 : Convert.ToDecimal(dr["_SellingPrice"])
                        };
                        string path = DOCType.ProductImagePath.Replace("{0}", data.ProductID.ToString());
                        if (Directory.Exists(path))
                        {
                            DirectoryInfo d = new DirectoryInfo(path);
                            FileInfo[] Files = d.GetFiles(data.ProductDetailID.ToString() + "_*");
                            foreach (FileInfo file in Files)
                            {
                                data.ImgUrl = file.Name;
                                break;
                            }
                        }
                        res.Add(data);
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
                    LoginTypeID = 1,
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }
        
        public string GetName() => @"SELECT	m._ProductName,m._CategoryID,m._SubCategoryID1,m._SubCategoryID2,d.* 
                                     FROM tbl_ProductDetail d
                                          Inner Join Master_Product M on d._ProductID=M._ID
                                     where ISNULL(_IsTrending,0)=1";
    }
}
